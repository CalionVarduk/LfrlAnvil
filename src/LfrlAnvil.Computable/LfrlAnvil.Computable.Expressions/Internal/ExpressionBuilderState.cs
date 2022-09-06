using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Computable.Expressions.Errors;
using LfrlAnvil.Computable.Expressions.Internal.Delegates;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal class ExpressionBuilderState
{
    private readonly RandomAccessStack<(IntermediateToken Token, Expectation Expectation)> _tokenStack;
    private readonly RandomAccessStack<Expression> _operandStack;
    private readonly List<Expression> _argumentAccessExpressions;
    private readonly InlineDelegateCollectionState? _delegateCollectionState;
    private readonly IParsedExpressionNumberParser _numberParser;
    private readonly ExpressionBuilderRootState _rootState;
    private readonly IReadOnlyDictionary<StringSlice, ConstantExpression>? _boundArguments;
    private int _operandCount;
    private int _operatorCount;
    private int _parenthesesCount;
    private Expectation _expectation;

    protected ExpressionBuilderState(
        ParameterExpression parameterExpression,
        ParsedExpressionFactoryInternalConfiguration configuration,
        IParsedExpressionNumberParser numberParser,
        IReadOnlyDictionary<StringSlice, ConstantExpression>? boundArguments)
    {
        Id = 0;
        _tokenStack = new RandomAccessStack<(IntermediateToken, Expectation)>();
        _operandStack = new RandomAccessStack<Expression>();
        ArgumentIndexes = new Dictionary<StringSlice, int>();
        _argumentAccessExpressions = new List<Expression>();
        _delegateCollectionState = null;
        ParameterExpression = parameterExpression;
        Configuration = configuration;
        _numberParser = numberParser;
        _operandCount = 0;
        _operatorCount = 0;
        _parenthesesCount = 0;
        LastHandledToken = null;
        _rootState = ReinterpretCast.To<ExpressionBuilderRootState>( this );
        _boundArguments = boundArguments;
        _expectation = Expectation.Operand | Expectation.OpenedParenthesis | Expectation.PrefixUnaryConstruct;
    }

    protected ExpressionBuilderState(
        ExpressionBuilderState prototype,
        Expectation initialState,
        int parenthesesCount,
        bool isInlineDelegate)
    {
        Id = prototype._rootState.GetNextStateId();
        _tokenStack = new RandomAccessStack<(IntermediateToken, Expectation)>();
        _operandStack = new RandomAccessStack<Expression>();
        ArgumentIndexes = prototype.ArgumentIndexes;
        _argumentAccessExpressions = prototype._argumentAccessExpressions;
        _delegateCollectionState = prototype._delegateCollectionState;
        ParameterExpression = prototype.ParameterExpression;
        Configuration = prototype.Configuration;
        _numberParser = prototype._numberParser;
        _operandCount = 0;
        _operatorCount = 0;
        _parenthesesCount = parenthesesCount;
        LastHandledToken = prototype.LastHandledToken;
        _rootState = prototype._rootState;
        _boundArguments = prototype._boundArguments;
        _expectation = initialState;

        if ( ! isInlineDelegate )
            return;

        if ( _delegateCollectionState is null )
        {
            _delegateCollectionState = new InlineDelegateCollectionState( this );
            return;
        }

        _delegateCollectionState.Register( this );
    }

    protected ParsedExpressionFactoryInternalConfiguration Configuration { get; }
    protected Dictionary<StringSlice, int> ArgumentIndexes { get; }
    internal int Id { get; }
    internal ParameterExpression ParameterExpression { get; }
    internal IntermediateToken? LastHandledToken { get; private set; }
    internal bool IsRoot => ReferenceEquals( this, _rootState );

    internal Chain<ParsedExpressionBuilderError> HandleTokenInternal(IntermediateToken token)
    {
        var errors = token.Type switch
        {
            IntermediateTokenType.NumberConstant => HandleNumberConstant( token ),
            IntermediateTokenType.StringConstant => HandleStringConstant( token ),
            IntermediateTokenType.BooleanConstant => HandleBooleanConstant( token ),
            IntermediateTokenType.OpenedParenthesis => HandleOpenedParenthesis( token ),
            IntermediateTokenType.ClosedParenthesis => HandleClosedParenthesis( token ),
            IntermediateTokenType.OpenedSquareBracket => HandleOpenedSquareBracket( token ),
            IntermediateTokenType.ClosedSquareBracket => HandleClosedSquareBracket( token ),
            IntermediateTokenType.Constructs => HandleConstructs( token ),
            IntermediateTokenType.MemberAccess => HandleMemberAccess( token ),
            IntermediateTokenType.ElementSeparator => HandleElementSeparator( token ),
            IntermediateTokenType.InlineFunctionSeparator => HandleInlineFunctionSeparator( token ),
            _ => HandleArgument( token )
        };

        LastHandledToken = token;
        return errors;
    }

    internal Chain<ParsedExpressionBuilderError> TryHandleExpressionEndAsInlineDelegate()
    {
        Assume.Equals( IsRoot, false, nameof( IsRoot ) );

        var self = ReinterpretCast.To<ExpressionBuilderChildState>( this );
        return self.ParentState.Expects( Expectation.InlineDelegateResolution ) && ! IsHandlingInlineDelegateParameters()
            ? HandleInlineDelegateBodyEnd( token: null )
            : Chain<ParsedExpressionBuilderError>.Empty;
    }

    protected Chain<ParsedExpressionBuilderError> HandleExpressionEnd(IntermediateToken? parentToken)
    {
        var errors = HandleAmbiguousConstructAsPostfixUnaryOperator();

        if ( _operandCount == 0 )
            errors = errors.Extend( ParsedExpressionBuilderError.CreateExpressionMustContainAtLeastOneOperand( parentToken ) );

        if ( _operandCount != _operatorCount + 1 )
            errors = errors.Extend( ParsedExpressionBuilderError.CreateExpressionContainsInvalidOperandToOperatorRatio( parentToken ) );

        if ( _parenthesesCount > 0 )
        {
            var remainingOpenedParenthesisTokens = _tokenStack
                .Select( x => x.Token )
                .Where( t => t.Type == IntermediateTokenType.OpenedParenthesis );

            errors = errors.Extend(
                ParsedExpressionBuilderError.CreateExpressionContainsUnclosedParentheses( parentToken, remainingOpenedParenthesisTokens ) );
        }

        if ( errors.Count > 0 )
            return errors;

        if ( _tokenStack.TryPeek( out var data ) && data.Expectation == Expectation.PrefixUnaryConstruct )
        {
            _tokenStack.Pop();

            errors = ProcessPrefixUnaryConstruct( data.Token );
            if ( errors.Count > 0 )
                return errors;
        }

        while ( _tokenStack.TryPop( out data ) )
        {
            AssumeExpectation( data.Expectation, Expectation.BinaryOperator );

            errors = ProcessBinaryOperator( data.Token );
            if ( errors.Count > 0 )
                return errors;
        }

        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    protected UnsafeBuilderResult<Expression> ConvertResultToOutputType(Type outputType)
    {
        Assume.ContainsExactly( _operandStack, 1, nameof( _operandStack ) );
        var rawBody = _operandStack.Pop();

        if ( rawBody.Type == outputType )
            return UnsafeBuilderResult<Expression>.CreateOk( rawBody );

        if ( ! Configuration.ConvertResultToOutputTypeAutomatically )
        {
            return rawBody.Type.IsAssignableTo( outputType )
                ? UnsafeBuilderResult<Expression>.CreateOk( Expression.Convert( rawBody, outputType ) )
                : UnsafeBuilderResult<Expression>.CreateErrors(
                    ParsedExpressionBuilderError.CreateExpressionResultTypeIsNotCompatibleWithExpectedOutputType(
                        rawBody.Type,
                        outputType ) );
        }

        var validConverter = Configuration.FindFirstValidTypeConverter( rawBody.Type, outputType );
        if ( validConverter is not null )
        {
            var errors = ProcessOutputTypeConverter( validConverter, rawBody );
            return errors.Count > 0
                ? UnsafeBuilderResult<Expression>.CreateErrors( errors )
                : UnsafeBuilderResult<Expression>.CreateOk( _operandStack.Pop() );
        }

        if ( rawBody.Type.IsAssignableTo( outputType ) )
            return UnsafeBuilderResult<Expression>.CreateOk( Expression.Convert( rawBody, outputType ) );

        validConverter = new ParsedExpressionTypeConverter( outputType );

        var castErrors = ProcessOutputTypeConverter( validConverter, rawBody );
        return castErrors.Count > 0
            ? UnsafeBuilderResult<Expression>.CreateErrors( castErrors )
            : UnsafeBuilderResult<Expression>.CreateOk( _operandStack.Pop() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected Expression GetArgumentAccess(int index)
    {
        return _argumentAccessExpressions[index];
    }

    private Chain<ParsedExpressionBuilderError> HandleNumberConstant(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.NumberConstant );

        var errors = HandleAmbiguousConstructAsBinaryOperator();

        if ( ! Expects( Expectation.Operand ) )
            errors = errors.Extend( ParsedExpressionBuilderError.CreateUnexpectedOperand( token ) );

        if ( ! _numberParser.TryParse( token.Symbol.AsSpan(), out var value ) )
            errors = errors.Extend( ParsedExpressionBuilderError.CreateNumberConstantParsingFailure( token ) );

        if ( errors.Count > 0 )
            return errors;

        var expression = Expression.Constant( value );
        PushOperand( expression );
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> HandleStringConstant(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.StringConstant );

        var errors = HandleAmbiguousConstructAsBinaryOperator();

        if ( ! Expects( Expectation.Operand ) )
            errors = errors.Extend( ParsedExpressionBuilderError.CreateUnexpectedOperand( token ) );

        var index = token.Symbol.StartIndex;
        var endIndex = token.Symbol.EndIndex - 1;

        if ( token.Symbol.Source[endIndex] != Configuration.StringDelimiter )
            errors = errors.Extend( ParsedExpressionBuilderError.CreateStringConstantParsingFailure( token ) );

        if ( errors.Count > 0 )
            return errors;

        var builder = new StringBuilder();
        ++index;

        while ( index < endIndex )
        {
            var c = token.Symbol.Source[index];
            builder.Append( c );
            index += c == Configuration.StringDelimiter ? 2 : 1;
        }

        var expression = Expression.Constant( builder.ToString() );
        PushOperand( expression );
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> HandleBooleanConstant(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.BooleanConstant );

        var errors = HandleAmbiguousConstructAsBinaryOperator();

        if ( ! Expects( Expectation.Operand ) )
            errors = errors.Extend( ParsedExpressionBuilderError.CreateUnexpectedOperand( token ) );

        if ( errors.Count > 0 )
            return errors;

        var value = TokenConstants.IsBooleanTrue( token.Symbol );
        var expression = Expression.Constant( value );
        PushOperand( expression );
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> HandleArgument(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.Argument );

        if ( IsHandlingInlineDelegateParameters() )
            return HandleDelegateParameterName( token );

        if ( Expects( Expectation.MemberName ) )
            return HandleMemberName( token );

        var errors = HandleAmbiguousConstructAsBinaryOperator();

        if ( ! Expects( Expectation.Operand ) )
            errors = errors.Extend( ParsedExpressionBuilderError.CreateUnexpectedOperand( token ) );

        if ( ! TokenValidation.IsValidArgumentName( token.Symbol, Configuration.StringDelimiter ) )
            errors = errors.Extend( ParsedExpressionBuilderError.CreateInvalidArgumentName( token ) );

        if ( errors.Count > 0 )
            return errors;

        var expression = _delegateCollectionState?.TryGetParameter( this, token.Symbol ) ??
            GetOrAddArgumentAccessExpression( token.Symbol );

        PushOperand( expression );
        return errors;
    }

    private Chain<ParsedExpressionBuilderError> HandleDelegateParameterName(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.Argument );
        Assume.IsNotNull( _delegateCollectionState, nameof( _delegateCollectionState ) );

        var errors = Chain<ParsedExpressionBuilderError>.Empty;

        if ( ! Expects( Expectation.ParameterName ) )
            errors = errors.Extend( ParsedExpressionBuilderError.CreateUnexpectedDelegateParameterName( token ) );

        if ( ! TokenValidation.IsValidArgumentName( token.Symbol, Configuration.StringDelimiter ) )
            errors = errors.Extend( ParsedExpressionBuilderError.CreateInvalidDelegateParameterName( token ) );

        if ( errors.Count > 0 )
            return errors;

        Assume.IsNotNull( LastHandledToken, nameof( LastHandledToken ) );
        Assume.IsNotNull( LastHandledToken.Value.Constructs, nameof( LastHandledToken.Value.Constructs ) );
        Assume.IsNotNull(
            LastHandledToken.Value.Constructs.TypeDeclaration,
            nameof( LastHandledToken.Value.Constructs.TypeDeclaration ) );

        var parameterType = LastHandledToken.Value.Constructs.TypeDeclaration;
        var parameterName = token.Symbol;

        if ( ArgumentIndexes.ContainsKey( parameterName ) || ! _delegateCollectionState.TryAddParameter( parameterType, parameterName ) )
            return Chain.Create( ParsedExpressionBuilderError.CreateDuplicatedDelegateParameterName( token ) );

        _expectation = Expectation.InlineDelegateParameterSeparator | Expectation.InlineDelegateParametersResolution;
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> HandleMemberName(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.Argument );
        AssumeStateExpectation( Expectation.MemberName );
        Assume.IsNotEmpty( _operandStack, nameof( _operandStack ) );

        var operand = _operandStack[0];
        var handleAsMethod = Configuration.TypeContainsMethod( operand.Type, token.Symbol );
        if ( ! handleAsMethod )
            return HandleFieldOrPropertyAccess( token );

        _expectation = GetExpectationWithPreservedPrefixUnaryConstructResolution( Expectation.MethodResolution );
        _rootState.ActiveState = ExpressionBuilderChildState.CreateFunctionParameters( this );
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> HandleConstructs(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.Constructs );
        Assume.IsNotNull( token.Constructs, nameof( token.Constructs ) );

        var result = token.Constructs.Type switch
        {
            ParsedExpressionConstructType.Function => HandleFunction( token ),
            ParsedExpressionConstructType.VariadicFunction => HandleFunction( token ),
            ParsedExpressionConstructType.Constant => HandleConstant( token ),
            ParsedExpressionConstructType.TypeDeclaration => HandleTypeDeclaration( token ),
            _ => HandleOperatorOrTypeConverter( token )
        };

        return result;
    }

    private Chain<ParsedExpressionBuilderError> HandleOperatorOrTypeConverter(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.Constructs );
        Assume.IsNotNull( token.Constructs, nameof( token.Constructs ) );

        if ( Expects( Expectation.AmbiguousPrefixConstructResolution ) )
        {
            var errors = HandleAmbiguousConstructAsBinaryOperator();
            if ( errors.Count > 0 )
                return errors;
        }

        if ( Expects( Expectation.PostfixUnaryConstruct | Expectation.BinaryOperator ) )
        {
            var hasBinaryOperators = token.Constructs.IsAny( ParsedExpressionConstructType.BinaryOperator );
            var hasPostfixUnaryConstructs = token.Constructs.IsAny( ParsedExpressionConstructType.PostfixUnaryConstruct );

            if ( hasBinaryOperators )
            {
                if ( ! hasPostfixUnaryConstructs )
                    return PushBinaryOperator( token );

                _expectation &= ~Expectation.PostfixUnaryConstruct;
                _expectation |= Expectation.PrefixUnaryConstruct | Expectation.AmbiguousPostfixConstructResolution;
                _tokenStack.Push( (token, Expectation.PostfixUnaryConstruct | Expectation.BinaryOperator) );
                return Chain<ParsedExpressionBuilderError>.Empty;
            }

            return hasPostfixUnaryConstructs
                ? PushPostfixUnaryConstruct( token )
                : Chain.Create( ParsedExpressionBuilderError.CreateExpectedPostfixUnaryOrBinaryConstruct( token ) );
        }

        if ( Expects( Expectation.PrefixUnaryConstruct | Expectation.BinaryOperator ) )
        {
            AssumeStateExpectation( Expectation.AmbiguousPostfixConstructResolution );

            var hasBinaryOperators = token.Constructs.IsAny( ParsedExpressionConstructType.BinaryOperator );
            var hasPrefixUnaryConstructs = token.Constructs.IsAny( ParsedExpressionConstructType.PrefixUnaryConstruct );

            if ( hasBinaryOperators )
            {
                if ( ! hasPrefixUnaryConstructs )
                    return PushBinaryOperator( token );

                _expectation &= ~Expectation.BinaryOperator;
                _expectation |= Expectation.AmbiguousPrefixConstructResolution;
                _tokenStack.Push( (token, Expectation.PrefixUnaryConstruct | Expectation.BinaryOperator) );
                return Chain<ParsedExpressionBuilderError>.Empty;
            }

            return hasPrefixUnaryConstructs
                ? PushPrefixUnaryConstruct( token )
                : Chain.Create( ParsedExpressionBuilderError.CreateExpectedBinaryOrPrefixUnaryConstruct( token ) );
        }

        if ( Expects( Expectation.BinaryOperator ) )
            return PushBinaryOperator( token );

        if ( Expects( Expectation.PrefixUnaryConstruct ) )
            return PushPrefixUnaryConstruct( token );

        return Chain.Create( ParsedExpressionBuilderError.CreateUnexpectedConstruct( token ) );
    }

    private Chain<ParsedExpressionBuilderError> HandleFunction(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.Constructs );
        Assume.IsNotNull( token.Constructs, nameof( token.Constructs ) );

        var errors = HandleAmbiguousConstructAsBinaryOperator();

        if ( ! Expects( Expectation.Operand ) )
            errors = errors.Extend( ParsedExpressionBuilderError.CreateUnexpectedFunctionCall( token ) );

        if ( errors.Count > 0 )
            return errors;

        _expectation = GetExpectationWithPreservedPrefixUnaryConstructResolution( Expectation.FunctionResolution );
        _rootState.ActiveState = ExpressionBuilderChildState.CreateFunctionParameters( this );
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> HandleConstant(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.Constructs );
        Assume.IsNotNull( token.Constructs, nameof( token.Constructs ) );
        Assume.IsNotNull( token.Constructs.Constant, nameof( token.Constructs.Constant ) );

        var errors = HandleAmbiguousConstructAsBinaryOperator();

        if ( ! Expects( Expectation.Operand ) )
            errors = errors.Extend( ParsedExpressionBuilderError.CreateUnexpectedOperand( token ) );

        if ( errors.Count > 0 )
            return errors;

        PushOperand( token.Constructs.Constant );
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> HandleTypeDeclaration(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.Constructs );
        Assume.IsNotNull( token.Constructs, nameof( token.Constructs ) );
        Assume.IsNotNull( token.Constructs.TypeDeclaration, nameof( token.Constructs.TypeDeclaration ) );

        if ( IsHandlingInlineDelegateParameters() )
            return HandleDelegateParameterType( token );

        var errors = HandleAmbiguousConstructAsBinaryOperator();

        if ( ! Expects( Expectation.Operand ) )
            errors = errors.Extend( ParsedExpressionBuilderError.CreateUnexpectedTypeDeclaration( token ) );

        if ( errors.Count > 0 )
            return errors;

        _expectation = GetExpectationWithPreservedPrefixUnaryConstructResolution( Expectation.ArrayResolution );
        _rootState.ActiveState = ExpressionBuilderChildState.CreateArrayElements( this );
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> HandleDelegateParameterType(IntermediateToken token)
    {
        Assume.IsNotNull( token.Constructs, nameof( token.Constructs ) );
        Assume.IsNotNull( token.Constructs.TypeDeclaration, nameof( token.Constructs.TypeDeclaration ) );
        Assume.IsNotNull( _delegateCollectionState, nameof( _delegateCollectionState ) );

        if ( ! Expects( Expectation.ParameterType ) )
            return Chain.Create( ParsedExpressionBuilderError.CreateUnexpectedTypeDeclaration( token ) );

        _expectation = Expectation.ParameterName;
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> HandleOpenedParenthesis(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.OpenedParenthesis );

        var errors = HandleAmbiguousConstructAsBinaryOperator();
        if ( errors.Count > 0 )
            return errors;

        if ( ! Expects( Expectation.OpenedParenthesis ) )
        {
            if ( ! IsLastHandledTokenInvocable( out var invocableToken ) )
                return Chain.Create( ParsedExpressionBuilderError.CreateUnexpectedOpenedParenthesis( token ) );

            _tokenStack.Push( (invocableToken, Expectation.InvocationResolution) );
            _expectation = GetExpectationWithPreservedPrefixUnaryConstructResolution( Expectation.InvocationResolution );
            _rootState.ActiveState = ExpressionBuilderChildState.CreateInvocationParameters( this );
            return Chain<ParsedExpressionBuilderError>.Empty;
        }

        if ( ! Expects( Expectation.FunctionParametersStart ) )
            _tokenStack.Push( (token, Expectation.OpenedParenthesis) );

        _expectation = Expectation.Operand | Expectation.OpenedParenthesis | Expectation.PrefixUnaryConstruct;
        ++_parenthesesCount;
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> HandleClosedParenthesis(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.ClosedParenthesis );

        var errors = HandleAmbiguousConstructAsPostfixUnaryOperator();
        if ( errors.Count > 0 )
            return errors;

        if ( ! Expects( Expectation.ClosedParenthesis ) )
        {
            return ! IsRoot && ! Expects( Expectation.FunctionParametersStart )
                ? HandleCallParametersOrInlineDelegateBodyEnd( token )
                : Chain.Create( ParsedExpressionBuilderError.CreateUnexpectedClosedParenthesis( token ) );
        }

        Assume.IsNotEmpty( _tokenStack, nameof( _tokenStack ) );
        var data = _tokenStack.Peek();

        while ( data.Expectation != Expectation.OpenedParenthesis )
        {
            AssumeExpectation( data.Expectation, Expectation.BinaryOperator | Expectation.PrefixUnaryConstruct );
            Assume.IsNotNull( data.Token.Constructs, nameof( data.Token.Constructs ) );

            _tokenStack.Pop();

            errors = data.Expectation == Expectation.BinaryOperator
                ? ProcessBinaryOperator( data.Token )
                : ProcessPrefixUnaryConstruct( data.Token );

            if ( errors.Count > 0 )
                return errors;

            Assume.IsNotEmpty( _tokenStack, nameof( _tokenStack ) );
            data = _tokenStack.Peek();
        }

        _tokenStack.Pop();

        _expectation = Expectation.PostfixUnaryConstruct | Expectation.BinaryOperator | Expectation.MemberAccess;
        if ( --_parenthesesCount > 0 )
            _expectation |= Expectation.ClosedParenthesis;

        if ( _tokenStack.TryPeek( out data ) && data.Expectation == Expectation.PrefixUnaryConstruct )
            _expectation |= Expectation.PrefixUnaryConstructResolution;

        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> HandleOpenedSquareBracket(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.OpenedSquareBracket );

        if ( Expects( Expectation.ArrayElementsStart ) )
        {
            _expectation = Expectation.Operand | Expectation.OpenedParenthesis | Expectation.PrefixUnaryConstruct;
            return Chain<ParsedExpressionBuilderError>.Empty;
        }

        if ( Expects( Expectation.PostfixUnaryConstruct ) )
        {
            _expectation = GetExpectationWithPreservedPrefixUnaryConstructResolution( Expectation.IndexerResolution );
            _rootState.ActiveState = ExpressionBuilderChildState.CreateInvocationParameters( this );
            return Chain<ParsedExpressionBuilderError>.Empty;
        }

        if ( Expects( Expectation.Operand ) )
        {
            _expectation = GetExpectationWithPreservedPrefixUnaryConstructResolution( Expectation.InlineDelegateResolution );
            _rootState.ActiveState = ExpressionBuilderChildState.CreateDelegate( this );
            return Chain<ParsedExpressionBuilderError>.Empty;
        }

        return Chain.Create( ParsedExpressionBuilderError.CreateUnexpectedOpenedSquareBracket( token ) );
    }

    private Chain<ParsedExpressionBuilderError> HandleClosedSquareBracket(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.ClosedSquareBracket );

        if ( ! IsRoot )
        {
            if ( IsHandlingInlineDelegateParameters() )
                return HandleInlineDelegateParametersEnd( token );

            if ( ! Expects( Expectation.ArrayElementsStart ) )
                return HandleArrayElementsOrIndexerParametersOrInlineDelegateBodyEnd( token );
        }

        return Chain.Create( ParsedExpressionBuilderError.CreateUnexpectedClosedSquareBracket( token ) );
    }

    private Chain<ParsedExpressionBuilderError> HandleInlineDelegateParametersEnd(IntermediateToken token)
    {
        Assume.IsNotNull( _delegateCollectionState, nameof( _delegateCollectionState ) );
        AssumeTokenType( token, IntermediateTokenType.ClosedSquareBracket );

        if ( ! Expects( Expectation.InlineDelegateParametersResolution ) )
            return Chain.Create( ParsedExpressionBuilderError.CreateUnexpectedClosedSquareBracket( token ) );

        _delegateCollectionState.LockParameters();
        _expectation = Expectation.Operand | Expectation.OpenedParenthesis | Expectation.PrefixUnaryConstruct;
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> HandleInlineDelegateBodyEnd(IntermediateToken? token)
    {
        Assume.Equals( IsRoot, false, nameof( IsRoot ) );
        Assume.IsNotNull( _delegateCollectionState, nameof( _delegateCollectionState ) );

        var self = ReinterpretCast.To<ExpressionBuilderChildState>( this );
        var errors = HandleExpressionEnd( self.ParentState.LastHandledToken );
        if ( errors.Count > 0 )
            return errors.Extend( ParsedExpressionBuilderError.CreateUnexpectedToken( token ) );

        Expression result;
        try
        {
            result = _delegateCollectionState.FinalizeLastState(
                lambdaBody: _operandStack.Pop(),
                compileWhenStatic: ! Configuration.PostponeStaticInlineDelegateCompilation );

            if ( _delegateCollectionState.AreAllStatesFinalized )
                _rootState.AddCompilableDelegate( _delegateCollectionState.CreateCompilableDelegates() );
        }
        catch ( Exception exc )
        {
            return Chain.Create( ParsedExpressionBuilderError.CreateInlineDelegateHasThrownException( token, exc ) );
        }

        return self.ParentState.HandleInlineDelegateResolution( token, result );
    }

    private Chain<ParsedExpressionBuilderError> HandleCallParametersOrInlineDelegateBodyEnd(IntermediateToken token)
    {
        Assume.Equals( IsRoot, false, nameof( IsRoot ) );
        AssumeTokenType( token, IntermediateTokenType.ClosedParenthesis );

        var self = ReinterpretCast.To<ExpressionBuilderChildState>( this );
        if ( ! self.ParentState.ExpectsAny(
                Expectation.FunctionResolution |
                Expectation.MethodResolution |
                Expectation.InvocationResolution |
                Expectation.InlineDelegateResolution ) )
            return Chain.Create( ParsedExpressionBuilderError.CreateUnexpectedClosedParenthesis( token ) );

        if ( self.ParentState.Expects( Expectation.InlineDelegateResolution ) )
            return HandleInlineDelegateBodyEnd( token );

        Assume.IsNotNull( LastHandledToken, nameof( LastHandledToken ) );

        var containsOneMoreElement = _tokenStack.Count > 0 ||
            _operandStack.Count > 0 ||
            LastHandledToken.Value.Type == IntermediateTokenType.ElementSeparator;

        if ( containsOneMoreElement )
        {
            var errors = HandleExpressionEnd( self.ParentState.LastHandledToken );
            if ( errors.Count > 0 )
                return errors.Extend( ParsedExpressionBuilderError.CreateUnexpectedClosedParenthesis( token ) );

            self.IncreaseElementCount();
            self.ParentState._operandStack.Push( _operandStack.Pop() );
        }

        if ( self.ParentState.Expects( Expectation.FunctionResolution ) )
            return self.ParentState.HandleFunctionResolution( token, self.ElementCount );

        if ( self.ParentState.Expects( Expectation.MethodResolution ) )
            return self.ParentState.HandleMethodResolution( token, self.ElementCount );

        return self.ParentState.HandleInvocationResolution( token, self.ElementCount );
    }

    private Chain<ParsedExpressionBuilderError> HandleFunctionResolution(IntermediateToken token, int parameterCount)
    {
        Assume.IsGreaterThanOrEqualTo( parameterCount, 0, nameof( parameterCount ) );
        Assume.IsNotNull( LastHandledToken, nameof( LastHandledToken ) );
        Assume.IsNotNull( LastHandledToken.Value.Constructs, nameof( LastHandledToken.Value.Constructs ) );
        AssumeConstructsType(
            LastHandledToken.Value.Constructs,
            ParsedExpressionConstructType.Function | ParsedExpressionConstructType.VariadicFunction );

        AssumeStateExpectation( Expectation.FunctionResolution );
        AddAssumedExpectation( Expectation.Operand );

        var functionToken = LastHandledToken.Value;
        LastHandledToken = token;
        _rootState.ActiveState = this;

        var parameters = parameterCount == 0 ? Array.Empty<Expression>() : new Expression[parameterCount];
        _operandStack.PopInto( parameterCount, parameters, startIndex: 0 );

        return functionToken.Constructs.Type == ParsedExpressionConstructType.Function
            ? ProcessFunction( functionToken, parameters )
            : ProcessVariadicFunction( functionToken, parameters );
    }

    private Chain<ParsedExpressionBuilderError> HandleMethodResolution(IntermediateToken token, int parameterCount)
    {
        Assume.IsGreaterThanOrEqualTo( parameterCount, 0, nameof( parameterCount ) );
        Assume.IsNotNull( LastHandledToken, nameof( LastHandledToken ) );

        AssumeStateExpectation( Expectation.MethodResolution );
        AddAssumedExpectation( Expectation.Operand );

        var methodNameToken = LastHandledToken.Value;
        LastHandledToken = token;
        _rootState.ActiveState = this;

        var parameters = new Expression[parameterCount + 2];
        _operandStack.PopInto( parameterCount, parameters, startIndex: 2 );
        Assume.IsNotEmpty( _operandStack, nameof( _operandStack ) );

        --_operandCount;
        parameters[0] = _operandStack.Pop();
        parameters[1] = Expression.Constant( methodNameToken.Symbol.ToString() );

        var methodCall = GetInternalVariadicFunction( ParsedExpressionConstructDefaults.MethodCallSymbol );
        return ProcessVariadicFunction( methodNameToken, methodCall, parameters );
    }

    private Chain<ParsedExpressionBuilderError> HandleInvocationResolution(IntermediateToken token, int parameterCount)
    {
        Assume.IsGreaterThanOrEqualTo( parameterCount, 0, nameof( parameterCount ) );
        Assume.IsNotEmpty( _tokenStack, nameof( _tokenStack ) );
        AssumeStateExpectation( Expectation.InvocationResolution );
        AddAssumedExpectation( Expectation.Operand );

        var data = _tokenStack.Pop();
        AssumeExpectation( data.Expectation, Expectation.InvocationResolution );

        LastHandledToken = token;
        _rootState.ActiveState = this;

        var parameters = new Expression[parameterCount + 1];
        _operandStack.PopInto( parameterCount, parameters, startIndex: 1 );
        Assume.IsNotEmpty( _operandStack, nameof( _operandStack ) );

        --_operandCount;
        parameters[0] = _operandStack.Pop();

        var invoke = GetInternalVariadicFunction( ParsedExpressionConstructDefaults.InvokeSymbol );
        return ProcessVariadicFunction( data.Token, invoke, parameters );
    }

    private Chain<ParsedExpressionBuilderError> HandleInlineDelegateResolution(IntermediateToken? token, Expression @delegate)
    {
        AssumeStateExpectation( Expectation.InlineDelegateResolution );
        AddAssumedExpectation( Expectation.Operand );

        LastHandledToken = token;
        _rootState.ActiveState = this;
        PushOperand( @delegate );

        if ( token is null )
            return IsRoot ? Chain<ParsedExpressionBuilderError>.Empty : TryHandleExpressionEndAsInlineDelegate();

        var result = token.Value.Type switch
        {
            IntermediateTokenType.ClosedParenthesis => HandleClosedParenthesis( token.Value ),
            IntermediateTokenType.ClosedSquareBracket => HandleClosedSquareBracket( token.Value ),
            IntermediateTokenType.ElementSeparator => HandleElementSeparator( token.Value ),
            _ => HandleInlineFunctionSeparator( token.Value )
        };

        return result;
    }

    private Chain<ParsedExpressionBuilderError> HandleArrayElementsOrIndexerParametersOrInlineDelegateBodyEnd(IntermediateToken token)
    {
        Assume.Equals( IsRoot, false, nameof( IsRoot ) );
        AssumeTokenType( token, IntermediateTokenType.ClosedSquareBracket );

        var self = ReinterpretCast.To<ExpressionBuilderChildState>( this );
        if ( ! self.ParentState.ExpectsAny(
                Expectation.ArrayResolution | Expectation.IndexerResolution | Expectation.InlineDelegateResolution ) )
            return Chain.Create( ParsedExpressionBuilderError.CreateUnexpectedClosedSquareBracket( token ) );

        if ( self.ParentState.Expects( Expectation.InlineDelegateResolution ) )
            return HandleInlineDelegateBodyEnd( token );

        Assume.IsNotNull( LastHandledToken, nameof( LastHandledToken ) );

        var containsOneMoreElement = _tokenStack.Count > 0 ||
            _operandStack.Count > 0 ||
            LastHandledToken.Value.Type == IntermediateTokenType.ElementSeparator;

        if ( containsOneMoreElement )
        {
            var errors = HandleExpressionEnd( self.ParentState.LastHandledToken );
            if ( errors.Count > 0 )
                return errors.Extend( ParsedExpressionBuilderError.CreateUnexpectedClosedSquareBracket( token ) );

            self.IncreaseElementCount();
            self.ParentState._operandStack.Push( _operandStack.Pop() );
        }

        return self.ParentState.Expects( Expectation.ArrayResolution )
            ? self.ParentState.HandleArrayResolution( token, self.ElementCount )
            : self.ParentState.HandleIndexerResolution( token, self.ElementCount );
    }

    private Chain<ParsedExpressionBuilderError> HandleArrayResolution(IntermediateToken token, int elementCount)
    {
        Assume.IsGreaterThanOrEqualTo( elementCount, 0, nameof( elementCount ) );
        Assume.IsNotNull( LastHandledToken, nameof( LastHandledToken ) );
        Assume.IsNotNull( LastHandledToken.Value.Constructs, nameof( LastHandledToken.Value.Constructs ) );
        AssumeConstructsType( LastHandledToken.Value.Constructs, ParsedExpressionConstructType.TypeDeclaration );
        AssumeStateExpectation( Expectation.ArrayResolution );
        AddAssumedExpectation( Expectation.Operand );

        var typeDeclarationToken = LastHandledToken.Value;
        LastHandledToken = token;
        _rootState.ActiveState = this;

        var parameters = new Expression[elementCount + 1];
        parameters[0] = Expression.Constant( typeDeclarationToken.Constructs.TypeDeclaration );
        _operandStack.PopInto( elementCount, parameters, startIndex: 1 );

        var makeArray = GetInternalVariadicFunction( ParsedExpressionConstructDefaults.MakeArraySymbol );
        return ProcessVariadicFunction( typeDeclarationToken, makeArray, parameters );
    }

    private Chain<ParsedExpressionBuilderError> HandleIndexerResolution(IntermediateToken token, int parameterCount)
    {
        Assume.IsGreaterThanOrEqualTo( parameterCount, 0, nameof( parameterCount ) );
        Assume.IsNotNull( LastHandledToken, nameof( LastHandledToken ) );
        AssumeStateExpectation( Expectation.IndexerResolution );
        AddAssumedExpectation( Expectation.Operand );

        var startIndexerToken = LastHandledToken.Value;
        LastHandledToken = token;
        _rootState.ActiveState = this;

        var parameters = new Expression[parameterCount + 1];
        _operandStack.PopInto( parameterCount, parameters, startIndex: 1 );
        Assume.IsNotEmpty( _operandStack, nameof( _operandStack ) );

        --_operandCount;
        parameters[0] = _operandStack.Pop();

        var indexerCall = GetInternalVariadicFunction( ParsedExpressionConstructDefaults.IndexerCallSymbol );
        return ProcessVariadicFunction( startIndexerToken, indexerCall, parameters );
    }

    private Chain<ParsedExpressionBuilderError> HandleFieldOrPropertyAccess(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.Argument );
        AddAssumedExpectation( Expectation.Operand );

        --_operandCount;
        var operand = _operandStack.Pop();

        var parameters = new[] { operand, Expression.Constant( token.Symbol.ToString() ) };
        var memberAccess = GetInternalVariadicFunction( ParsedExpressionConstructDefaults.MemberAccessSymbol );
        return ProcessVariadicFunction( token, memberAccess, parameters );
    }

    private Chain<ParsedExpressionBuilderError> HandleMemberAccess(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.MemberAccess );

        if ( ! Expects( Expectation.MemberAccess ) )
            return Chain.Create( ParsedExpressionBuilderError.CreateUnexpectedMemberAccess( token ) );

        _expectation = GetExpectationWithPreservedPrefixUnaryConstructResolution( Expectation.MemberName );
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> HandleElementSeparator(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.ElementSeparator );

        if ( IsRoot )
            return Chain.Create( ParsedExpressionBuilderError.CreateUnexpectedElementSeparator( token ) );

        if ( IsHandlingInlineDelegateParameters() )
            return HandleInlineDelegateParameterSeparator( token );

        var self = ReinterpretCast.To<ExpressionBuilderChildState>( this );
        if ( self.ParentState.Expects( Expectation.InlineDelegateResolution ) )
            return HandleInlineDelegateBodyEnd( token );

        var errors = HandleExpressionEnd( self.ParentState.LastHandledToken );
        if ( errors.Count > 0 )
            return errors.Extend( ParsedExpressionBuilderError.CreateUnexpectedElementSeparator( token ) );

        self.IncreaseElementCount();
        self.ParentState._operandStack.Push( _operandStack.Pop() );

        _operandCount = 0;
        _operatorCount = 0;
        _parenthesesCount = 0;
        _expectation = Expectation.Operand | Expectation.OpenedParenthesis | Expectation.PrefixUnaryConstruct;

        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> HandleInlineDelegateParameterSeparator(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.ElementSeparator );
        Assume.IsNotNull( _delegateCollectionState, nameof( _delegateCollectionState ) );

        if ( ! Expects( Expectation.InlineDelegateParameterSeparator ) )
            return Chain.Create( ParsedExpressionBuilderError.CreateUnexpectedElementSeparator( token ) );

        _expectation = Expectation.ParameterType;
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> HandleInlineFunctionSeparator(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.InlineFunctionSeparator );
        throw new NotSupportedException( "Inline function separator token is not supported yet." );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Chain<ParsedExpressionBuilderError> ProcessPrefixUnaryConstruct(IntermediateToken token)
    {
        Assume.IsNotNull( token.Constructs, nameof( token.Constructs ) );
        AssumeConstructsType( token.Constructs, ParsedExpressionConstructType.PrefixUnaryConstruct );

        return token.Constructs.IsAny( ParsedExpressionConstructType.Operator )
            ? ProcessPrefixUnaryOperator( token )
            : ProcessPrefixTypeConverter( token );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Chain<ParsedExpressionBuilderError> ProcessPrefixUnaryOperator(IntermediateToken token)
    {
        Assume.IsNotEmpty( _operandStack, nameof( _operandStack ) );
        Assume.IsNotNull( token.Constructs, nameof( token.Constructs ) );
        AssumeConstructsType( token.Constructs, ParsedExpressionConstructType.PrefixUnaryOperator );

        var argumentType = _operandStack[0].Type;
        var @operator = token.Constructs.PrefixUnaryOperators.FindConstruct( argumentType );

        return @operator is null
            ? Chain.Create( ParsedExpressionBuilderError.CreatePrefixUnaryOperatorCouldNotBeResolved( token, argumentType ) )
            : ProcessUnaryOperator( token, @operator );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Chain<ParsedExpressionBuilderError> ProcessPrefixTypeConverter(IntermediateToken token)
    {
        Assume.IsNotEmpty( _operandStack, nameof( _operandStack ) );
        Assume.IsNotNull( token.Constructs, nameof( token.Constructs ) );
        AssumeConstructsType( token.Constructs, ParsedExpressionConstructType.PrefixTypeConverter );

        var sourceType = _operandStack[0].Type;
        var converter = token.Constructs.PrefixTypeConverters.FindConstruct( sourceType );

        return converter is null
            ? Chain.Create( ParsedExpressionBuilderError.CreatePrefixTypeConverterCouldNotBeResolved( token, sourceType ) )
            : ProcessTypeConverter( token, converter );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Chain<ParsedExpressionBuilderError> ProcessPostfixUnaryConstruct(IntermediateToken token)
    {
        Assume.IsNotNull( token.Constructs, nameof( token.Constructs ) );
        AssumeConstructsType( token.Constructs, ParsedExpressionConstructType.PostfixUnaryConstruct );

        return token.Constructs.IsAny( ParsedExpressionConstructType.Operator )
            ? ProcessPostfixUnaryOperator( token )
            : ProcessPostfixTypeConverter( token );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Chain<ParsedExpressionBuilderError> ProcessPostfixUnaryOperator(IntermediateToken token)
    {
        Assume.IsNotEmpty( _operandStack, nameof( _operandStack ) );
        Assume.IsNotNull( token.Constructs, nameof( token.Constructs ) );
        AssumeConstructsType( token.Constructs, ParsedExpressionConstructType.PostfixUnaryOperator );

        var argumentType = _operandStack[0].Type;
        var @operator = token.Constructs.PostfixUnaryOperators.FindConstruct( argumentType );

        return @operator is null
            ? Chain.Create( ParsedExpressionBuilderError.CreatePostfixUnaryOperatorCouldNotBeResolved( token, argumentType ) )
            : ProcessUnaryOperator( token, @operator );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Chain<ParsedExpressionBuilderError> ProcessPostfixTypeConverter(IntermediateToken token)
    {
        Assume.IsNotEmpty( _operandStack, nameof( _operandStack ) );
        Assume.IsNotNull( token.Constructs, nameof( token.Constructs ) );
        AssumeConstructsType( token.Constructs, ParsedExpressionConstructType.PostfixTypeConverter );

        var sourceType = _operandStack[0].Type;
        var converter = token.Constructs.PostfixTypeConverters.FindConstruct( sourceType );

        return converter is null
            ? Chain.Create( ParsedExpressionBuilderError.CreatePostfixTypeConverterCouldNotBeResolved( token, sourceType ) )
            : ProcessTypeConverter( token, converter );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Chain<ParsedExpressionBuilderError> ProcessBinaryOperator(IntermediateToken token)
    {
        Assume.ContainsAtLeast( _operandStack, 2, nameof( _operandStack ) );
        Assume.IsNotNull( token.Constructs, nameof( token.Constructs ) );
        AssumeConstructsType( token.Constructs, ParsedExpressionConstructType.BinaryOperator );

        var rightArgumentType = _operandStack[0].Type;
        var leftArgumentType = _operandStack[1].Type;
        var binaryOperator = token.Constructs.BinaryOperators.FindConstruct( leftArgumentType, rightArgumentType );

        return binaryOperator is null
            ? Chain.Create(
                ParsedExpressionBuilderError.CreateBinaryOperatorCouldNotBeResolved( token, leftArgumentType, rightArgumentType ) )
            : ProcessBinaryOperator( token, binaryOperator );
    }

    private Chain<ParsedExpressionBuilderError> ProcessUnaryOperator(IntermediateToken token, ParsedExpressionUnaryOperator @operator)
    {
        Assume.IsNotEmpty( _operandStack, nameof( _operandStack ) );

        var operand = _operandStack.Pop();
        Expression result;

        try
        {
            result = @operator.Process( operand );
        }
        catch ( Exception exc )
        {
            return Chain.Create( ParsedExpressionBuilderError.CreateConstructHasThrownException( token, @operator, exc ) );
        }

        _operandStack.Push( result );
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> ProcessTypeConverter(IntermediateToken token, ParsedExpressionTypeConverter converter)
    {
        Assume.IsNotEmpty( _operandStack, nameof( _operandStack ) );

        var operand = _operandStack.Pop();
        Expression result;

        try
        {
            result = converter.Process( operand );
        }
        catch ( Exception exc )
        {
            return Chain.Create( ParsedExpressionBuilderError.CreateConstructHasThrownException( token, converter, exc ) );
        }

        _operandStack.Push( result );
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> ProcessBinaryOperator(IntermediateToken token, ParsedExpressionBinaryOperator @operator)
    {
        Assume.ContainsAtLeast( _operandStack, 2, nameof( _operandStack ) );

        var rightOperand = _operandStack[0];
        var leftOperand = _operandStack[1];
        _operandStack.Pop( count: 2 );
        Expression result;

        try
        {
            result = @operator.Process( leftOperand, rightOperand );
        }
        catch ( Exception exc )
        {
            return Chain.Create( ParsedExpressionBuilderError.CreateConstructHasThrownException( token, @operator, exc ) );
        }

        _operandStack.Push( result );
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> ProcessOutputTypeConverter(ParsedExpressionTypeConverter converter, Expression rawBody)
    {
        Assume.IsEmpty( _operandStack, nameof( _operandStack ) );

        Expression result;

        try
        {
            result = converter.Process( rawBody );
        }
        catch ( Exception exc )
        {
            return Chain.Create( ParsedExpressionBuilderError.CreateOutputTypeConverterHasThrownException( converter, exc ) );
        }

        _operandStack.Push( result );
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> ProcessFunction(IntermediateToken token, IReadOnlyList<Expression> parameters)
    {
        Assume.IsNotNull( token.Constructs, nameof( token.Constructs ) );
        AssumeConstructsType( token.Constructs, ParsedExpressionConstructType.Function );

        var function = token.Constructs.Functions.FindConstruct( parameters );

        if ( function is null )
            return Chain.Create( ParsedExpressionBuilderError.CreateFunctionCouldNotBeResolved( token, parameters ) );

        Expression result;

        try
        {
            result = function.Process( parameters );
        }
        catch ( Exception exc )
        {
            return Chain.Create( ParsedExpressionBuilderError.CreateConstructHasThrownException( token, function, exc ) );
        }

        PushOperand( result );
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Chain<ParsedExpressionBuilderError> ProcessVariadicFunction(IntermediateToken token, IReadOnlyList<Expression> parameters)
    {
        Assume.IsNotNull( token.Constructs, nameof( token.Constructs ) );
        Assume.IsNotNull( token.Constructs.VariadicFunction, nameof( token.Constructs.VariadicFunction ) );
        AssumeConstructsType( token.Constructs, ParsedExpressionConstructType.VariadicFunction );

        var function = token.Constructs.VariadicFunction;
        return ProcessVariadicFunction( token, function, parameters );
    }

    private Chain<ParsedExpressionBuilderError> ProcessVariadicFunction(
        IntermediateToken token,
        ParsedExpressionVariadicFunction function,
        IReadOnlyList<Expression> parameters)
    {
        Expression result;

        try
        {
            result = function.Process( parameters );
        }
        catch ( Exception exc )
        {
            return Chain.Create( ParsedExpressionBuilderError.CreateConstructHasThrownException( token, function, exc ) );
        }

        PushOperand( result );
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> PushBinaryOperator(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.Constructs );
        AssumeStateExpectation( Expectation.BinaryOperator );
        Assume.IsNotNull( token.Constructs, nameof( token.Constructs ) );

        var errors = HandleAmbiguousConstructAsPostfixUnaryOperator();
        if ( errors.Count > 0 )
            return errors;

        if ( Expects( Expectation.PrefixUnaryConstructResolution ) )
        {
            Assume.IsNotEmpty( _tokenStack, nameof( _tokenStack ) );
            var data = _tokenStack.Pop();

            AssumeExpectation( data.Expectation, Expectation.PrefixUnaryConstruct );

            errors = ProcessPrefixUnaryConstruct( data.Token );
            if ( errors.Count > 0 )
                return errors;
        }

        var binaryOperators = token.Constructs.BinaryOperators;
        if ( binaryOperators.IsEmpty )
            return Chain.Create( ParsedExpressionBuilderError.CreateExpectedBinaryOperator( token ) );

        while ( _tokenStack.TryPeek( out var data ) && data.Expectation == Expectation.BinaryOperator )
        {
            Assume.IsNotNull( data.Token.Constructs, nameof( data.Token.Constructs ) );

            var binaryOperatorsOnStack = data.Token.Constructs.BinaryOperators;
            if ( binaryOperators.Precedence < binaryOperatorsOnStack.Precedence )
                break;

            _tokenStack.Pop();

            errors = ProcessBinaryOperator( data.Token );
            if ( errors.Count > 0 )
                return errors;
        }

        _tokenStack.Push( (token, Expectation.BinaryOperator) );
        _expectation = Expectation.Operand | Expectation.OpenedParenthesis | Expectation.PrefixUnaryConstruct;
        ++_operatorCount;
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> PushPrefixUnaryConstruct(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.Constructs );
        AssumeStateExpectation( Expectation.PrefixUnaryConstruct );
        Assume.IsNotNull( token.Constructs, nameof( token.Constructs ) );

        var errors = HandleAmbiguousConstructAsBinaryOperator();

        if ( token.Constructs.PrefixUnaryOperators.IsEmpty && token.Constructs.PrefixTypeConverters.IsEmpty )
            errors = errors.Extend( ParsedExpressionBuilderError.CreateExpectedPrefixUnaryConstruct( token ) );

        if ( errors.Count > 0 )
            return errors;

        _tokenStack.Push( (token, Expectation.PrefixUnaryConstruct) );
        _expectation &= ~Expectation.PrefixUnaryConstruct;
        _expectation |= Expectation.PrefixUnaryConstructResolution;
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> PushPostfixUnaryConstruct(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.Constructs );
        AssumeStateExpectation( Expectation.PostfixUnaryConstruct );
        Assume.IsNotNull( token.Constructs, nameof( token.Constructs ) );
        AssumeConstructsType( token.Constructs, ParsedExpressionConstructType.PostfixUnaryConstruct );

        if ( Expects( Expectation.PrefixUnaryConstructResolution ) )
        {
            Assume.IsNotEmpty( _tokenStack, nameof( _tokenStack ) );
            var prefixData = _tokenStack.Pop();

            AssumeExpectation( prefixData.Expectation, Expectation.PrefixUnaryConstruct );
            Assume.IsNotNull( prefixData.Token.Constructs, nameof( prefixData.Token.Constructs ) );

            var prefixPrecedence = prefixData.Token.Constructs.IsAny( ParsedExpressionConstructType.Operator )
                ? prefixData.Token.Constructs.PrefixUnaryOperators.Precedence
                : prefixData.Token.Constructs.PrefixTypeConverters.Precedence;

            var postfixPrecedence = token.Constructs.IsAny( ParsedExpressionConstructType.Operator )
                ? token.Constructs.PostfixUnaryOperators.Precedence
                : token.Constructs.PostfixTypeConverters.Precedence;

            if ( prefixPrecedence <= postfixPrecedence )
            {
                var errors = ProcessPrefixUnaryConstruct( prefixData.Token );
                if ( errors.Count > 0 )
                    return errors;

                errors = ProcessPostfixUnaryConstruct( token );
                if ( errors.Count > 0 )
                    return errors;
            }
            else
            {
                var errors = ProcessPostfixUnaryConstruct( token );
                if ( errors.Count > 0 )
                    return errors;

                errors = ProcessPrefixUnaryConstruct( prefixData.Token );
                if ( errors.Count > 0 )
                    return errors;
            }

            _expectation &= ~Expectation.PrefixUnaryConstructResolution;
        }
        else
        {
            var errors = ProcessPostfixUnaryConstruct( token );
            if ( errors.Count > 0 )
                return errors;
        }

        _expectation &= ~(Expectation.PostfixUnaryConstruct | Expectation.MemberAccess);
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private void PushOperand(Expression operand)
    {
        AssumeStateExpectation( Expectation.Operand );

        _operandStack.Push( operand );

        _expectation = GetExpectationWithPreservedPrefixUnaryConstructResolution(
            Expectation.PostfixUnaryConstruct |
            Expectation.BinaryOperator |
            Expectation.MemberAccess );

        if ( _parenthesesCount > 0 )
            _expectation |= Expectation.ClosedParenthesis;

        ++_operandCount;
    }

    private Chain<ParsedExpressionBuilderError> HandleAmbiguousConstructAsPostfixUnaryOperator()
    {
        if ( ! Expects( Expectation.AmbiguousPostfixConstructResolution ) )
            return Chain<ParsedExpressionBuilderError>.Empty;

        Assume.IsNotEmpty( _tokenStack, nameof( _tokenStack ) );
        var data = _tokenStack.Pop();

        if ( Expects( Expectation.AmbiguousPrefixConstructResolution ) )
            return Chain.Create( ParsedExpressionBuilderError.CreateAmbiguousPostfixUnaryConstructResolutionFailure( data.Token ) );

        AssumeAllExpectations( data.Expectation, Expectation.BinaryOperator | Expectation.PostfixUnaryConstruct );
        _expectation &= ~Expectation.AmbiguousPostfixConstructResolution;
        AddAssumedExpectation( Expectation.PostfixUnaryConstruct );

        return PushPostfixUnaryConstruct( data.Token );
    }

    private Chain<ParsedExpressionBuilderError> HandleAmbiguousConstructAsBinaryOperator()
    {
        if ( ! Expects( Expectation.AmbiguousPostfixConstructResolution ) )
            return Chain<ParsedExpressionBuilderError>.Empty;

        Assume.IsNotEmpty( _tokenStack, nameof( _tokenStack ) );
        var data = _tokenStack.Pop();

        AssumeExpectation( data.Expectation, Expectation.BinaryOperator );
        AssumeExpectation( data.Expectation, Expectation.PostfixUnaryConstruct | Expectation.PrefixUnaryConstruct );

        var currentResolution = (data.Expectation & Expectation.PostfixUnaryConstruct) != Expectation.None
            ? Expectation.AmbiguousPostfixConstructResolution
            : Expectation.AmbiguousPrefixConstructResolution;

        _expectation &= ~currentResolution;
        AddAssumedExpectation( Expectation.BinaryOperator );

        return PushBinaryOperator( data.Token );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool Expects(Expectation expectation)
    {
        return (_expectation & expectation) == expectation;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool ExpectsAny(Expectation expectation)
    {
        return (_expectation & expectation) != Expectation.None;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Expectation GetExpectationWithPreservedPrefixUnaryConstructResolution(Expectation expectation)
    {
        return (_expectation & Expectation.PrefixUnaryConstructResolution) | expectation;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool IsLastHandledTokenInvocable(out IntermediateToken result)
    {
        result = default;
        if ( LastHandledToken is null || _operandStack.Count == 0 )
            return false;

        result = LastHandledToken.Value;
        if ( result.Type != IntermediateTokenType.Argument &&
            result.Type != IntermediateTokenType.ClosedParenthesis &&
            result.Type != IntermediateTokenType.ClosedSquareBracket &&
            (result.Type != IntermediateTokenType.Constructs || result.Constructs!.Type != ParsedExpressionConstructType.Constant) )
            return false;

        var operand = _operandStack[0];
        return operand.Type.IsAssignableTo( typeof( Delegate ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool IsHandlingInlineDelegateParameters()
    {
        return _delegateCollectionState is not null && ! _delegateCollectionState.AreParametersLocked( this );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ParsedExpressionVariadicFunction GetInternalVariadicFunction(string symbol)
    {
        var constructs = Configuration.Constructs[symbol.AsSlice()];
        Assume.IsNotNull( constructs.VariadicFunction, nameof( constructs.VariadicFunction ) );
        return constructs.VariadicFunction;
    }

    private Expression GetOrAddArgumentAccessExpression(StringSlice name)
    {
        if ( _boundArguments is not null && _boundArguments.TryGetValue( name, out var constant ) )
            return constant;

        if ( ArgumentIndexes.TryGetValue( name, out var index ) )
        {
            _delegateCollectionState?.AddArgumentCapture( this, index );
            return _argumentAccessExpressions[index];
        }

        index = ArgumentIndexes.Count;
        _delegateCollectionState?.AddArgumentCapture( this, index );
        var result = ParameterExpression.CreateArgumentAccess( index );

        ArgumentIndexes.Add( name, index );
        _argumentAccessExpressions.Add( result );
        return result;
    }

    [Conditional( "DEBUG" )]
    private void AddAssumedExpectation(Expectation expectation)
    {
        _expectation |= expectation;
    }

    [Conditional( "DEBUG" )]
    private void AssumeStateExpectation(Expectation expectation)
    {
        Assume.NotEquals( _expectation & expectation, Expectation.None, nameof( _expectation ) );
    }

    [Conditional( "DEBUG" )]
    private static void AssumeExpectation(Expectation expectation, Expectation expected)
    {
        Assume.NotEquals( expectation & expected, Expectation.None, nameof( expectation ) );
    }

    [Conditional( "DEBUG" )]
    private static void AssumeAllExpectations(Expectation expectation, Expectation expected)
    {
        Assume.Equals( expectation & expected, expected, nameof( expectation ) );
    }

    [Conditional( "DEBUG" )]
    private static void AssumeTokenType(IntermediateToken token, IntermediateTokenType expected)
    {
        Assume.Equals( token.Type, expected, nameof( token ) + '.' + nameof( token.Type ) );
    }

    [Conditional( "DEBUG" )]
    private static void AssumeConstructsType(ConstructTokenDefinition constructs, ParsedExpressionConstructType expected)
    {
        Assume.NotEquals(
            constructs.Type & expected,
            ParsedExpressionConstructType.None,
            nameof( constructs ) + '.' + nameof( constructs.Type ) );
    }

    [Flags]
    internal enum Expectation : uint
    {
        None = 0x0,
        Operand = 0x1,
        OpenedParenthesis = 0x2,
        ClosedParenthesis = 0x4,
        PrefixUnaryConstruct = 0x8,
        PostfixUnaryConstruct = 0x10,
        BinaryOperator = 0x20,
        MemberAccess = 0x40,
        MemberName = 0x80,
        ParameterType = 0x100,
        ParameterName = 0x200,
        InlineDelegateParameterSeparator = 0x80000,
        InlineDelegateParametersResolution = 0x100000,
        InlineDelegateResolution = 0x200000,
        InvocationResolution = 0x400000,
        IndexerResolution = 0x800000,
        ArrayResolution = 0x1000000,
        PrefixUnaryConstructResolution = 0x2000000,
        AmbiguousPostfixConstructResolution = 0x4000000,
        AmbiguousPrefixConstructResolution = 0x8000000,
        FunctionResolution = 0x10000000,
        MethodResolution = 0x20000000,
        ArrayElementsStart = 0x40000000,
        FunctionParametersStart = 0x80000000
    }
}
