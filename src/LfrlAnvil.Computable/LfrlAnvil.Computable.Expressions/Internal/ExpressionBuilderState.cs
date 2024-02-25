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

namespace LfrlAnvil.Computable.Expressions.Internal;

internal class ExpressionBuilderState
{
    private readonly RandomAccessStack<(IntermediateToken Token, Expectation Expectation)> _tokenStack;
    private readonly RandomAccessStack<Expression> _operandStack;
    private readonly InlineDelegateCollectionState? _delegateCollectionState;
    private readonly IParsedExpressionNumberParser _numberParser;
    private readonly ExpressionBuilderRootState _rootState;
    private int _operandCount;
    private int _operatorCount;
    private int _parenthesesCount;
    private Expectation _expectation;

    protected ExpressionBuilderState(
        ParameterExpression parameterExpression,
        ParsedExpressionFactoryInternalConfiguration configuration,
        IParsedExpressionNumberParser numberParser,
        IReadOnlyDictionary<StringSegment, ConstantExpression>? boundArguments)
    {
        Id = 0;
        _tokenStack = new RandomAccessStack<(IntermediateToken, Expectation)>();
        _operandStack = new RandomAccessStack<Expression>();
        LocalTerms = new LocalTermsCollection( parameterExpression, boundArguments );
        _delegateCollectionState = null;
        Configuration = configuration;
        _numberParser = numberParser;
        _operandCount = 0;
        _operatorCount = 0;
        _parenthesesCount = 0;
        LastHandledToken = null;
        _rootState = ReinterpretCast.To<ExpressionBuilderRootState>( this );

        _expectation = Expectation.Operand |
            Expectation.OpenedParenthesis |
            Expectation.PrefixUnaryConstruct |
            Expectation.LocalTermDeclaration;
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
        LocalTerms = prototype.LocalTerms;
        _delegateCollectionState = prototype._delegateCollectionState;
        Configuration = prototype.Configuration;
        _numberParser = prototype._numberParser;
        _operandCount = 0;
        _operatorCount = 0;
        _parenthesesCount = parenthesesCount;
        LastHandledToken = prototype.LastHandledToken;
        _rootState = prototype._rootState;
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
    internal LocalTermsCollection LocalTerms { get; }
    internal int Id { get; }
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
            IntermediateTokenType.LineSeparator => HandleLineSeparator( token ),
            IntermediateTokenType.VariableDeclaration => HandleVariableDeclaration( token ),
            IntermediateTokenType.MacroDeclaration => HandleMacroDeclaration( token ),
            IntermediateTokenType.Assignment => HandleAssignment( token ),
            _ => HandleArgument( token )
        };

        LastHandledToken = token;
        return errors;
    }

    internal Chain<ParsedExpressionBuilderError> TryHandleExpressionEndAsInlineDelegate()
    {
        Assume.False( IsRoot );

        var self = ReinterpretCast.To<ExpressionBuilderChildState>( this );
        return self.ParentState.Expects( Expectation.InlineDelegateResolution ) && ! IsHandlingInlineDelegateParameters()
            ? HandleInlineDelegateBodyEnd( token: null )
            : Chain<ParsedExpressionBuilderError>.Empty;
    }

    protected Chain<ParsedExpressionBuilderError> HandleExpressionEnd(IntermediateToken? parentToken)
    {
        var errors = PrepareForExpressionEnd( parentToken );

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
        Assume.ContainsExactly( _operandStack, 1 );
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

    private Chain<ParsedExpressionBuilderError> HandleNumberConstant(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.NumberConstant );

        var errors = HandleAmbiguousConstructAsBinaryOperator();

        if ( ! Expects( Expectation.Operand ) )
        {
            if ( Expects( Expectation.MacroEnd ) )
                return HandleMacroToken( token );

            errors = errors.Extend( ParsedExpressionBuilderError.CreateUnexpectedOperand( token ) );
        }

        if ( ! _numberParser.TryParse( token.Symbol, out var value ) )
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
        {
            if ( Expects( Expectation.MacroEnd ) )
                return HandleMacroToken( token );

            errors = errors.Extend( ParsedExpressionBuilderError.CreateUnexpectedOperand( token ) );
        }

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
        {
            if ( Expects( Expectation.MacroEnd ) )
                return HandleMacroToken( token );

            errors = errors.Extend( ParsedExpressionBuilderError.CreateUnexpectedOperand( token ) );
        }

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

        if ( Expects( Expectation.VariableName ) )
            return HandleVariableName( token );

        if ( Expects( Expectation.MacroName ) )
            return HandleMacroName( token );

        if ( Expects( Expectation.MacroEnd ) )
        {
            return LocalTerms.IsTermStarted( token.Symbol )
                ? Chain.Create( ParsedExpressionBuilderError.CreateUndeclaredLocalTermUsage( token ) )
                : HandleMacroToken( token );
        }

        if ( LocalTerms.TryGetMacro( token.Symbol, out var declaration ) )
            return ProcessMacro( token, declaration );

        if ( Expects( Expectation.MemberName ) )
            return HandleMemberName( token );

        if ( IsHandlingInlineDelegateParameters() )
            return HandleDelegateParameterName( token );

        if ( Expects( Expectation.ParameterName ) )
            return HandleMacroParameterName( token );

        return ProcessLocalTermOrDelegateParameter( token );
    }

    private Chain<ParsedExpressionBuilderError> HandleDelegateParameterName(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.Argument );
        Assume.IsNotNull( _delegateCollectionState );

        var parameterName = token.Symbol;
        var errors = Chain<ParsedExpressionBuilderError>.Empty;

        if ( ! Expects( Expectation.ParameterName ) )
            errors = errors.Extend( ParsedExpressionBuilderError.CreateUnexpectedDelegateParameterName( token ) );

        if ( ! TokenValidation.IsValidLocalTermName( parameterName, Configuration.StringDelimiter ) )
            errors = errors.Extend( ParsedExpressionBuilderError.CreateInvalidDelegateParameterName( token ) );

        if ( errors.Count > 0 )
            return errors;

        Assume.IsNotNull( LastHandledToken );
        Assume.IsNotNull( LastHandledToken.Value.Constructs );
        Assume.IsNotNull( LastHandledToken.Value.Constructs.TypeDeclaration );

        var parameterType = LastHandledToken.Value.Constructs.TypeDeclaration;

        if ( LocalTerms.ContainsArgument( parameterName ) ||
            LocalTerms.ContainsVariable( parameterName ) ||
            LocalTerms.ContainsMacro( parameterName ) ||
            LocalTerms.IsTermStarted( parameterName ) ||
            ! _delegateCollectionState.TryAddParameter( parameterType, parameterName ) )
            return Chain.Create( ParsedExpressionBuilderError.CreateDuplicatedDelegateParameterName( token ) );

        _expectation = Expectation.InlineParameterSeparator | Expectation.InlineParametersResolution;
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> HandleMacroParameterName(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.Argument );

        var parameterName = token.Symbol;
        if ( ! TokenValidation.IsValidLocalTermName( parameterName, Configuration.StringDelimiter ) )
            return Chain.Create( ParsedExpressionBuilderError.CreateInvalidMacroParameterName( token ) );

        if ( LocalTerms.ContainsArgument( parameterName ) ||
            LocalTerms.ContainsVariable( parameterName ) ||
            LocalTerms.ContainsMacro( parameterName ) ||
            LocalTerms.IsTermStarted( parameterName ) )
            return Chain.Create( ParsedExpressionBuilderError.CreateDuplicatedMacroParameterName( token ) );

        _expectation = Expectation.InlineParameterSeparator | Expectation.InlineParametersResolution;
        return LocalTerms.AddMacroParameter( token );
    }

    private Chain<ParsedExpressionBuilderError> HandleMemberName(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.Argument );
        AssumeStateExpectation( Expectation.MemberName );
        Assume.IsNotEmpty( _operandStack );

        var operand = _operandStack[0];
        var handleAsMethod = Configuration.TypeContainsMethod( operand.Type, token.Symbol );
        if ( ! handleAsMethod )
            return HandleFieldOrPropertyAccess( token );

        _expectation = GetExpectationWithPreservedPrefixUnaryConstructResolution( Expectation.MethodResolution );
        _rootState.ActiveState = ExpressionBuilderChildState.CreateFunctionParameters( this );
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> HandleVariableName(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.Argument );
        AssumeStateExpectation( Expectation.VariableName );

        if ( ! TokenValidation.IsValidLocalTermName( token.Symbol, Configuration.StringDelimiter ) )
            return Chain.Create( ParsedExpressionBuilderError.CreateInvalidLocalTermName( token ) );

        if ( LocalTerms.ContainsArgument( token.Symbol ) || LocalTerms.ContainsMacro( token.Symbol ) )
            return Chain.Create( ParsedExpressionBuilderError.CreateDuplicatedLocalTermName( token ) );

        var self = ReinterpretCast.To<ExpressionBuilderChildState>( this );
        self.ParentState.LastHandledToken = token;
        LocalTerms.StartVariable( token.Symbol );

        _expectation = Expectation.Assignment;
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> HandleMacroName(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.Argument );
        AssumeStateExpectation( Expectation.MacroName );

        if ( ! TokenValidation.IsValidLocalTermName( token.Symbol, Configuration.StringDelimiter ) )
            return Chain.Create( ParsedExpressionBuilderError.CreateInvalidLocalTermName( token ) );

        if ( LocalTerms.ContainsArgument( token.Symbol ) ||
            LocalTerms.ContainsVariable( token.Symbol ) ||
            LocalTerms.ContainsMacro( token.Symbol ) )
            return Chain.Create( ParsedExpressionBuilderError.CreateDuplicatedLocalTermName( token ) );

        var self = ReinterpretCast.To<ExpressionBuilderChildState>( this );
        self.ParentState.LastHandledToken = token;
        var errors = LocalTerms.StartMacro( token );

        if ( errors.Count == 0 )
            _expectation = Expectation.Assignment;

        return errors;
    }

    private Chain<ParsedExpressionBuilderError> HandleConstructs(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.Constructs );
        Assume.IsNotNull( token.Constructs );

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
        Assume.IsNotNull( token.Constructs );

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

        return Expects( Expectation.MacroEnd )
            ? HandleMacroToken( token )
            : Chain.Create( ParsedExpressionBuilderError.CreateUnexpectedConstruct( token ) );
    }

    private Chain<ParsedExpressionBuilderError> HandleFunction(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.Constructs );
        Assume.IsNotNull( token.Constructs );

        var errors = HandleAmbiguousConstructAsBinaryOperator();

        if ( ! Expects( Expectation.Operand ) )
        {
            if ( Expects( Expectation.MacroEnd ) )
                return HandleMacroToken( token );

            errors = errors.Extend( ParsedExpressionBuilderError.CreateUnexpectedFunctionCall( token ) );
        }

        if ( errors.Count > 0 )
            return errors;

        _expectation = GetExpectationWithPreservedPrefixUnaryConstructResolution( Expectation.FunctionResolution );
        _rootState.ActiveState = ExpressionBuilderChildState.CreateFunctionParameters( this );
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> HandleConstant(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.Constructs );
        Assume.IsNotNull( token.Constructs );
        Assume.IsNotNull( token.Constructs.Constant );

        var errors = HandleAmbiguousConstructAsBinaryOperator();

        if ( ! Expects( Expectation.Operand ) )
        {
            if ( Expects( Expectation.MacroEnd ) )
                return HandleMacroToken( token );

            errors = errors.Extend( ParsedExpressionBuilderError.CreateUnexpectedOperand( token ) );
        }

        if ( errors.Count > 0 )
            return errors;

        PushOperand( token.Constructs.Constant );
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> HandleTypeDeclaration(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.Constructs );
        Assume.IsNotNull( token.Constructs );
        Assume.IsNotNull( token.Constructs.TypeDeclaration );

        if ( IsHandlingInlineDelegateParameters() )
            return HandleDelegateParameterType( token );

        var errors = HandleAmbiguousConstructAsBinaryOperator();

        if ( ! Expects( Expectation.Operand ) )
        {
            if ( Expects( Expectation.MacroEnd ) )
                return HandleMacroToken( token );

            errors = errors.Extend( ParsedExpressionBuilderError.CreateUnexpectedTypeDeclaration( token ) );
        }

        if ( errors.Count > 0 )
            return errors;

        _expectation = GetExpectationWithPreservedPrefixUnaryConstructResolution(
            Expectation.ArrayResolution | Expectation.ConstructorResolution );

        _rootState.ActiveState = ExpressionBuilderChildState.CreateArrayElementsOrConstructorParameters( this );
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> HandleDelegateParameterType(IntermediateToken token)
    {
        Assume.IsNotNull( token.Constructs );
        Assume.IsNotNull( token.Constructs.TypeDeclaration );
        Assume.IsNotNull( _delegateCollectionState );

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
            if ( Expects( Expectation.MacroEnd ) )
                return HandleMacroToken( token );

            if ( ! IsLastHandledTokenInvocable( out var invocableToken ) )
                return Chain.Create( ParsedExpressionBuilderError.CreateUnexpectedOpenedParenthesis( token ) );

            _tokenStack.Push( (invocableToken, Expectation.InvocationResolution) );
            _expectation = GetExpectationWithPreservedPrefixUnaryConstructResolution( Expectation.InvocationResolution );
            _rootState.ActiveState = ExpressionBuilderChildState.CreateInvocationParameters( this );
            return Chain<ParsedExpressionBuilderError>.Empty;
        }

        if ( ! Expects( Expectation.FunctionParametersStart ) )
        {
            _tokenStack.Push( (token, Expectation.OpenedParenthesis) );
            _expectation = Expectation.Operand | Expectation.OpenedParenthesis | Expectation.PrefixUnaryConstruct;
            ++_parenthesesCount;
            return Chain<ParsedExpressionBuilderError>.Empty;
        }

        Assume.False( IsRoot );

        var self = ReinterpretCast.To<ExpressionBuilderChildState>( this );
        self.ParentState._expectation &= ~Expectation.ArrayResolution;

        _expectation = self.ParentState.Expects( Expectation.MacroParametersResolution )
            ? Expectation.MacroEnd
            : Expectation.Operand | Expectation.OpenedParenthesis | Expectation.PrefixUnaryConstruct;

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
            if ( Expects( Expectation.MacroEnd ) )
                return HandleMacroToken( token );

            return ! IsRoot && ! Expects( Expectation.FunctionParametersStart )
                ? HandleCallParametersOrInlineDelegateBodyEnd( token )
                : Chain.Create( ParsedExpressionBuilderError.CreateUnexpectedClosedParenthesis( token ) );
        }

        Assume.IsNotEmpty( _tokenStack );
        var data = _tokenStack.Peek();

        while ( data.Expectation != Expectation.OpenedParenthesis )
        {
            AssumeExpectation( data.Expectation, Expectation.BinaryOperator | Expectation.PrefixUnaryConstruct );
            Assume.IsNotNull( data.Token.Constructs );

            _tokenStack.Pop();

            errors = data.Expectation == Expectation.BinaryOperator
                ? ProcessBinaryOperator( data.Token )
                : ProcessPrefixUnaryConstruct( data.Token );

            if ( errors.Count > 0 )
                return errors;

            Assume.IsNotEmpty( _tokenStack );
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
            Assume.False( IsRoot );
            var self = ReinterpretCast.To<ExpressionBuilderChildState>( this );
            self.ParentState._expectation &= ~Expectation.ConstructorResolution;
            _expectation = Expectation.Operand | Expectation.OpenedParenthesis | Expectation.PrefixUnaryConstruct;
            _parenthesesCount = 0;
            return Chain<ParsedExpressionBuilderError>.Empty;
        }

        if ( Expects( Expectation.MacroParametersStart ) )
            return HandleMacroParametersStart();

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

        return Expects( Expectation.MacroEnd )
            ? HandleMacroToken( token )
            : Chain.Create( ParsedExpressionBuilderError.CreateUnexpectedOpenedSquareBracket( token ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Chain<ParsedExpressionBuilderError> HandleMacroParametersStart()
    {
        _expectation = Expectation.ParameterName;
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> HandleClosedSquareBracket(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.ClosedSquareBracket );

        if ( ! IsRoot )
        {
            if ( Expects( Expectation.MacroEnd ) )
                return HandleMacroToken( token );

            if ( IsHandlingInlineDelegateParameters() )
                return HandleInlineDelegateParametersEnd( token );

            if ( Expects( Expectation.InlineParametersResolution ) )
                return HandleMacroParametersEnd();

            if ( ! Expects( Expectation.ArrayElementsStart ) )
                return HandleArrayElementsOrIndexerParametersOrInlineDelegateBodyEnd( token );
        }

        return Chain.Create( ParsedExpressionBuilderError.CreateUnexpectedClosedSquareBracket( token ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Chain<ParsedExpressionBuilderError> HandleMacroParametersEnd()
    {
        _expectation = Expectation.MacroName;
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> HandleInlineDelegateParametersEnd(IntermediateToken token)
    {
        Assume.IsNotNull( _delegateCollectionState );
        AssumeTokenType( token, IntermediateTokenType.ClosedSquareBracket );

        if ( ! Expects( Expectation.InlineParametersResolution ) )
            return Chain.Create( ParsedExpressionBuilderError.CreateUnexpectedClosedSquareBracket( token ) );

        _delegateCollectionState.LockParameters();
        _expectation = Expectation.Operand | Expectation.OpenedParenthesis | Expectation.PrefixUnaryConstruct;
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> HandleInlineDelegateBodyEnd(IntermediateToken? token)
    {
        Assume.False( IsRoot );
        Assume.IsNotNull( _delegateCollectionState );

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
        Assume.False( IsRoot );
        AssumeTokenType( token, IntermediateTokenType.ClosedParenthesis );

        var self = ReinterpretCast.To<ExpressionBuilderChildState>( this );
        if ( ! self.ParentState.ExpectsAny(
                Expectation.FunctionResolution |
                Expectation.MethodResolution |
                Expectation.ConstructorResolution |
                Expectation.InvocationResolution |
                Expectation.InlineDelegateResolution ) )
            return Chain.Create( ParsedExpressionBuilderError.CreateUnexpectedClosedParenthesis( token ) );

        if ( self.ParentState.Expects( Expectation.InlineDelegateResolution ) )
            return HandleInlineDelegateBodyEnd( token );

        Assume.IsNotNull( LastHandledToken );

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

        if ( self.ParentState.Expects( Expectation.ConstructorResolution ) )
            return self.ParentState.HandleConstructorResolution( token, self.ElementCount );

        return self.ParentState.HandleInvocationResolution( token, self.ElementCount );
    }

    private Chain<ParsedExpressionBuilderError> HandleFunctionResolution(IntermediateToken token, int parameterCount)
    {
        Assume.IsGreaterThanOrEqualTo( parameterCount, 0 );
        Assume.IsNotNull( LastHandledToken );
        Assume.IsNotNull( LastHandledToken.Value.Constructs );
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
        Assume.IsGreaterThanOrEqualTo( parameterCount, 0 );
        Assume.IsNotNull( LastHandledToken );

        AssumeStateExpectation( Expectation.MethodResolution );
        AddAssumedExpectation( Expectation.Operand );

        var methodNameToken = LastHandledToken.Value;
        LastHandledToken = token;
        _rootState.ActiveState = this;

        var parameters = new Expression[parameterCount + 2];
        _operandStack.PopInto( parameterCount, parameters, startIndex: 2 );
        Assume.IsNotEmpty( _operandStack );

        --_operandCount;
        parameters[0] = _operandStack.Pop();
        parameters[1] = Expression.Constant( methodNameToken.Symbol.ToString() );

        var methodCall = GetInternalVariadicFunction( ParsedExpressionConstructDefaults.MethodCallSymbol );
        return ProcessVariadicFunction( methodNameToken, methodCall, parameters );
    }

    private Chain<ParsedExpressionBuilderError> HandleConstructorResolution(IntermediateToken token, int parameterCount)
    {
        Assume.IsGreaterThanOrEqualTo( parameterCount, 0 );
        Assume.IsNotNull( LastHandledToken );
        Assume.IsNotNull( LastHandledToken.Value.Constructs );
        AssumeConstructsType( LastHandledToken.Value.Constructs, ParsedExpressionConstructType.TypeDeclaration );
        AssumeStateExpectation( Expectation.ConstructorResolution );
        AddAssumedExpectation( Expectation.Operand );

        var typeDeclarationToken = LastHandledToken.Value;
        LastHandledToken = token;
        _rootState.ActiveState = this;

        var parameters = new Expression[parameterCount + 1];
        parameters[0] = Expression.Constant( typeDeclarationToken.Constructs.TypeDeclaration );
        _operandStack.PopInto( parameterCount, parameters, startIndex: 1 );

        var ctorCall = GetInternalVariadicFunction( ParsedExpressionConstructDefaults.CtorCallSymbol );
        return ProcessVariadicFunction( typeDeclarationToken, ctorCall, parameters );
    }

    private Chain<ParsedExpressionBuilderError> HandleInvocationResolution(IntermediateToken token, int parameterCount)
    {
        Assume.IsGreaterThanOrEqualTo( parameterCount, 0 );
        Assume.IsNotEmpty( _tokenStack );
        AssumeStateExpectation( Expectation.InvocationResolution );
        AddAssumedExpectation( Expectation.Operand );

        var data = _tokenStack.Pop();
        AssumeExpectation( data.Expectation, Expectation.InvocationResolution );

        LastHandledToken = token;
        _rootState.ActiveState = this;

        var parameters = new Expression[parameterCount + 1];
        _operandStack.PopInto( parameterCount, parameters, startIndex: 1 );
        Assume.IsNotEmpty( _operandStack );

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
            _ => HandleLineSeparator( token.Value )
        };

        return result;
    }

    private Chain<ParsedExpressionBuilderError> HandleVariableResolution(IntermediateToken token, Expression expression)
    {
        Assume.True( IsRoot );
        AssumeStateExpectation( Expectation.VariableResolution );
        Assume.IsNotNull( LastHandledToken );

        LastHandledToken = token;
        _rootState.ActiveState = this;

        var errors = LocalTerms.FinalizeVariableAssignment( expression, _rootState.GetCompilableDelegates() );
        if ( errors.Count > 0 )
            return errors;

        _rootState.ClearCompilableDelegates();

        _expectation = Expectation.Operand |
            Expectation.OpenedParenthesis |
            Expectation.PrefixUnaryConstruct |
            Expectation.LocalTermDeclaration;

        return errors;
    }

    private Chain<ParsedExpressionBuilderError> HandleMacroResolution(IntermediateToken token)
    {
        Assume.True( IsRoot );
        AssumeStateExpectation( Expectation.MacroResolution );
        Assume.IsNotNull( LastHandledToken );
        Assume.IsEmpty( _operandStack );
        Assume.IsEmpty( _tokenStack );

        LastHandledToken = token;
        _rootState.ActiveState = this;

        var errors = LocalTerms.FinalizeMacroDeclaration();
        if ( errors.Count > 0 )
            return errors;

        _expectation = Expectation.Operand |
            Expectation.OpenedParenthesis |
            Expectation.PrefixUnaryConstruct |
            Expectation.LocalTermDeclaration;

        return errors;
    }

    private Chain<ParsedExpressionBuilderError> HandleMacroParametersResolution(IntermediateToken token, int parameterCount)
    {
        AssumeStateExpectation( Expectation.MacroParametersResolution );
        Assume.IsNotNull( LastHandledToken );
        Assume.IsGreaterThanOrEqualTo( parameterCount, 0 );
        Assume.ContainsAtLeast( _operandStack, parameterCount );
        Assume.IsNotEmpty( _tokenStack );

        var macroName = LastHandledToken.Value;
        LastHandledToken = token;
        _rootState.ActiveState = this;

        LocalTerms.TryGetMacro( macroName.Symbol, out var declaration );
        Assume.IsNotNull( declaration );

        if ( declaration.ParameterCount != parameterCount )
        {
            var error = ParsedExpressionBuilderError.CreateInvalidMacroParameterCount(
                macroName,
                parameterCount,
                declaration.ParameterCount );

            return Chain.Create( error );
        }

        var parameters = new IReadOnlyList<IntermediateToken>[parameterCount];
        for ( var i = parameters.Length - 1; i >= 0; --i )
        {
            var expression = _operandStack.Pop();
            Assume.Equals( expression.NodeType, ExpressionType.Constant );
            var constant = ReinterpretCast.To<ConstantExpression>( expression );
            Assume.IsNotNull( constant.Value );
            parameters[i] = DynamicCast.To<IntermediateToken[]>( constant.Value );
        }

        _expectation = _tokenStack.Pop().Expectation;
        return declaration.Process( _rootState, macroName, parameters );
    }

    private Chain<ParsedExpressionBuilderError> HandleArrayElementsOrIndexerParametersOrInlineDelegateBodyEnd(IntermediateToken token)
    {
        Assume.False( IsRoot );
        AssumeTokenType( token, IntermediateTokenType.ClosedSquareBracket );

        var self = ReinterpretCast.To<ExpressionBuilderChildState>( this );
        if ( ! self.ParentState.ExpectsAny(
                Expectation.ArrayResolution | Expectation.IndexerResolution | Expectation.InlineDelegateResolution ) )
            return Chain.Create( ParsedExpressionBuilderError.CreateUnexpectedClosedSquareBracket( token ) );

        if ( self.ParentState.Expects( Expectation.InlineDelegateResolution ) )
            return HandleInlineDelegateBodyEnd( token );

        Assume.IsNotNull( LastHandledToken );

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
        Assume.IsGreaterThanOrEqualTo( elementCount, 0 );
        Assume.IsNotNull( LastHandledToken );
        Assume.IsNotNull( LastHandledToken.Value.Constructs );
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
        Assume.IsGreaterThanOrEqualTo( parameterCount, 0 );
        Assume.IsNotNull( LastHandledToken );
        AssumeStateExpectation( Expectation.IndexerResolution );
        AddAssumedExpectation( Expectation.Operand );

        var startIndexerToken = LastHandledToken.Value;
        LastHandledToken = token;
        _rootState.ActiveState = this;

        var parameters = new Expression[parameterCount + 1];
        _operandStack.PopInto( parameterCount, parameters, startIndex: 1 );
        Assume.IsNotEmpty( _operandStack );

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
        {
            if ( Expects( Expectation.MacroEnd ) )
                return HandleMacroToken( token );

            return Chain.Create( ParsedExpressionBuilderError.CreateUnexpectedMemberAccess( token ) );
        }

        _expectation = GetExpectationWithPreservedPrefixUnaryConstructResolution( Expectation.MemberName );
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> HandleElementSeparator(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.ElementSeparator );

        if ( IsRoot )
            return Chain.Create( ParsedExpressionBuilderError.CreateUnexpectedElementSeparator( token ) );

        if ( Expects( Expectation.MacroEnd ) )
            return HandleMacroToken( token );

        if ( IsHandlingInlineDelegateParameters() )
            return HandleInlineDelegateParameterSeparator( token );

        if ( Expects( Expectation.InlineParameterSeparator ) )
            return HandleMacroParameterSeparator();

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

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Chain<ParsedExpressionBuilderError> HandleMacroParameterSeparator()
    {
        _expectation = Expectation.ParameterName;
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> HandleInlineDelegateParameterSeparator(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.ElementSeparator );
        Assume.IsNotNull( _delegateCollectionState );

        if ( ! Expects( Expectation.InlineParameterSeparator ) )
            return Chain.Create( ParsedExpressionBuilderError.CreateUnexpectedElementSeparator( token ) );

        _expectation = Expectation.ParameterType;
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> HandleLineSeparator(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.LineSeparator );

        if ( IsRoot )
        {
            if ( _expectation == Expectation.None )
                return Chain.Create( ParsedExpressionBuilderError.CreateUnexpectedLineSeparator( token ) );

            return PrepareForExpressionEnd( token );
        }

        var self = ReinterpretCast.To<ExpressionBuilderChildState>( this );
        if ( self.ParentState.Expects( Expectation.InlineDelegateResolution ) )
            return HandleInlineDelegateBodyEnd( token );

        if ( ! self.ParentState.ExpectsAny( Expectation.VariableResolution | Expectation.MacroResolution ) )
            return Chain.Create( ParsedExpressionBuilderError.CreateUnexpectedLineSeparator( token ) );

        if ( self.ParentState.Expects( Expectation.MacroResolution ) )
            return self.ParentState.HandleMacroResolution( token );

        var errors = HandleExpressionEnd( self.ParentState.LastHandledToken );
        if ( errors.Count > 0 )
            return errors.Extend( ParsedExpressionBuilderError.CreateUnexpectedLineSeparator( token ) );

        var expression = _operandStack.Pop();
        return self.ParentState.HandleVariableResolution( token, expression );
    }

    private Chain<ParsedExpressionBuilderError> HandleVariableDeclaration(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.VariableDeclaration );

        if ( ! Expects( Expectation.LocalTermDeclaration ) )
        {
            if ( Expects( Expectation.MacroEnd ) )
                return HandleMacroToken( token );

            return Chain.Create( ParsedExpressionBuilderError.CreateUnexpectedLocalTermDeclaration( token ) );
        }

        _expectation = Expectation.VariableResolution;
        _rootState.ActiveState = ExpressionBuilderChildState.CreateVariable( this );
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> HandleMacroDeclaration(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.MacroDeclaration );

        if ( ! Expects( Expectation.LocalTermDeclaration ) )
        {
            if ( Expects( Expectation.MacroEnd ) )
                return HandleMacroToken( token );

            return Chain.Create( ParsedExpressionBuilderError.CreateUnexpectedLocalTermDeclaration( token ) );
        }

        _expectation = Expectation.MacroResolution;
        _rootState.ActiveState = ExpressionBuilderChildState.CreateMacro( this );
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> HandleAssignment(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.Assignment );

        if ( Expects( Expectation.Assignment ) )
        {
            Assume.False( IsRoot );

            var self = ReinterpretCast.To<ExpressionBuilderChildState>( this );
            _expectation = self.ParentState.Expects( Expectation.VariableResolution )
                ? Expectation.Operand | Expectation.OpenedParenthesis | Expectation.PrefixUnaryConstruct
                : Expectation.MacroEnd;

            return Chain<ParsedExpressionBuilderError>.Empty;
        }

        if ( token.Constructs is not null )
        {
            token = IntermediateToken.CreateConstructs( token.Symbol, token.Constructs );
            return HandleConstructs( token );
        }

        return Expects( Expectation.MacroEnd )
            ? HandleMacroToken( token )
            : Chain.Create( ParsedExpressionBuilderError.CreateUnexpectedAssignment( token ) );
    }

    private Chain<ParsedExpressionBuilderError> HandleMacroToken(IntermediateToken token)
    {
        Assume.False( IsRoot );
        AssumeStateExpectation( Expectation.MacroEnd );

        var self = ReinterpretCast.To<ExpressionBuilderChildState>( this );
        if ( self.ParentState.Expects( Expectation.MacroResolution ) )
            return LocalTerms.AddMacroToken( token );

        self.ParentState.AssumeStateExpectation( Expectation.MacroParametersResolution );

        var errors = token.Type switch
        {
            IntermediateTokenType.OpenedParenthesis => HandleMacroParametersOpenedParenthesis( token ),
            IntermediateTokenType.ClosedParenthesis => HandleMacroParametersClosedParenthesis( self, token ),
            IntermediateTokenType.ElementSeparator => HandleMacroParametersNextParameter( self, token ),
            _ => PushMacroParametersToken( token )
        };

        return errors;
    }

    private Chain<ParsedExpressionBuilderError> HandleMacroParametersOpenedParenthesis(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.OpenedParenthesis );
        ++_parenthesesCount;
        return PushMacroParametersToken( token );
    }

    private Chain<ParsedExpressionBuilderError> HandleMacroParametersClosedParenthesis(
        ExpressionBuilderChildState self,
        IntermediateToken token)
    {
        AssumeStateExpectation( Expectation.MacroEnd );
        AssumeTokenType( token, IntermediateTokenType.ClosedParenthesis );

        if ( _parenthesesCount > 0 )
        {
            --_parenthesesCount;
            return PushMacroParametersToken( token );
        }

        Assume.IsNotNull( LastHandledToken );

        var containsOneMoreElement = _tokenStack.Count > 0 ||
            LastHandledToken.Value.Type == IntermediateTokenType.ElementSeparator;

        if ( containsOneMoreElement )
        {
            var errors = HandleMacroParametersNextParameter( self, token );
            if ( errors.Count > 0 )
                return errors;
        }

        return self.ParentState.HandleMacroParametersResolution( token, self.ElementCount );
    }

    private Chain<ParsedExpressionBuilderError> HandleMacroParametersNextParameter(
        ExpressionBuilderChildState self,
        IntermediateToken token)
    {
        AssumeStateExpectation( Expectation.MacroEnd );

        if ( _tokenStack.Count == 0 )
            return Chain.Create( ParsedExpressionBuilderError.CreateMacroParameterMustContainAtLeastOneToken( token ) );

        _parenthesesCount = 0;
        self.IncreaseElementCount();

        var tokens = new IntermediateToken[_tokenStack.Count];
        for ( var i = tokens.Length - 1; i >= 0; --i )
            tokens[i] = _tokenStack.Pop().Token;

        self.ParentState._operandStack.Push( Expression.Constant( tokens ) );
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> PushMacroParametersToken(IntermediateToken token)
    {
        AssumeStateExpectation( Expectation.MacroEnd );
        _tokenStack.Push( (token, Expectation.MacroEnd) );
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Chain<ParsedExpressionBuilderError> ProcessPrefixUnaryConstruct(IntermediateToken token)
    {
        Assume.IsNotNull( token.Constructs );
        AssumeConstructsType( token.Constructs, ParsedExpressionConstructType.PrefixUnaryConstruct );

        return token.Constructs.IsAny( ParsedExpressionConstructType.Operator )
            ? ProcessPrefixUnaryOperator( token )
            : ProcessPrefixTypeConverter( token );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Chain<ParsedExpressionBuilderError> ProcessPrefixUnaryOperator(IntermediateToken token)
    {
        Assume.IsNotEmpty( _operandStack );
        Assume.IsNotNull( token.Constructs );
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
        Assume.IsNotEmpty( _operandStack );
        Assume.IsNotNull( token.Constructs );
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
        Assume.IsNotNull( token.Constructs );
        AssumeConstructsType( token.Constructs, ParsedExpressionConstructType.PostfixUnaryConstruct );

        return token.Constructs.IsAny( ParsedExpressionConstructType.Operator )
            ? ProcessPostfixUnaryOperator( token )
            : ProcessPostfixTypeConverter( token );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Chain<ParsedExpressionBuilderError> ProcessPostfixUnaryOperator(IntermediateToken token)
    {
        Assume.IsNotEmpty( _operandStack );
        Assume.IsNotNull( token.Constructs );
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
        Assume.IsNotEmpty( _operandStack );
        Assume.IsNotNull( token.Constructs );
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
        Assume.ContainsAtLeast( _operandStack, 2 );
        Assume.IsNotNull( token.Constructs );
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
        Assume.IsNotEmpty( _operandStack );

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
        Assume.IsNotEmpty( _operandStack );

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
        Assume.ContainsAtLeast( _operandStack, 2 );

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
        Assume.IsEmpty( _operandStack );

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
        Assume.IsNotNull( token.Constructs );
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
        Assume.IsNotNull( token.Constructs );
        Assume.IsNotNull( token.Constructs.VariadicFunction );
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
        Assume.IsNotNull( token.Constructs );

        var errors = HandleAmbiguousConstructAsPostfixUnaryOperator();
        if ( errors.Count > 0 )
            return errors;

        if ( Expects( Expectation.PrefixUnaryConstructResolution ) )
        {
            Assume.IsNotEmpty( _tokenStack );
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
            Assume.IsNotNull( data.Token.Constructs );

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
        Assume.IsNotNull( token.Constructs );

        var errors = HandleAmbiguousConstructAsBinaryOperator();

        if ( token.Constructs.PrefixUnaryOperators.IsEmpty && token.Constructs.PrefixTypeConverters.IsEmpty )
            errors = errors.Extend( ParsedExpressionBuilderError.CreateExpectedPrefixUnaryConstruct( token ) );

        if ( errors.Count > 0 )
            return errors;

        _tokenStack.Push( (token, Expectation.PrefixUnaryConstruct) );
        _expectation &= ~(Expectation.PrefixUnaryConstruct | Expectation.LocalTermDeclaration);
        _expectation |= Expectation.PrefixUnaryConstructResolution;
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> PushPostfixUnaryConstruct(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.Constructs );
        AssumeStateExpectation( Expectation.PostfixUnaryConstruct );
        Assume.IsNotNull( token.Constructs );
        AssumeConstructsType( token.Constructs, ParsedExpressionConstructType.PostfixUnaryConstruct );

        if ( Expects( Expectation.PrefixUnaryConstructResolution ) )
        {
            Assume.IsNotEmpty( _tokenStack );
            var prefixData = _tokenStack.Pop();

            AssumeExpectation( prefixData.Expectation, Expectation.PrefixUnaryConstruct );
            Assume.IsNotNull( prefixData.Token.Constructs );

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

        Assume.IsNotEmpty( _tokenStack );
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

        Assume.IsNotEmpty( _tokenStack );
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

    private Chain<ParsedExpressionBuilderError> PrepareForExpressionEnd(IntermediateToken? token)
    {
        var errors = HandleAmbiguousConstructAsPostfixUnaryOperator();

        if ( _expectation != Expectation.None && ! ExpectsAny( Expectation.Operand | Expectation.BinaryOperator ) )
            errors = errors.Extend( ParsedExpressionBuilderError.CreateUnexpectedEnd( token ) );

        _expectation = Expectation.None;
        return errors;
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
        var constructs = Configuration.Constructs[symbol];
        Assume.IsNotNull( constructs.VariadicFunction );
        return constructs.VariadicFunction;
    }

    private Chain<ParsedExpressionBuilderError> ProcessLocalTermOrDelegateParameter(IntermediateToken token)
    {
        var errors = HandleAmbiguousConstructAsBinaryOperator();

        if ( ! Expects( Expectation.Operand ) )
            errors = errors.Extend( ParsedExpressionBuilderError.CreateUnexpectedOperand( token ) );

        if ( LocalTerms.TryGetVariable( token.Symbol, out var assignment ) )
            return errors.Count > 0 ? errors : ProcessVariable( assignment );

        if ( LocalTerms.IsTermStarted( token.Symbol ) )
            return errors.Extend( ParsedExpressionBuilderError.CreateUndeclaredLocalTermUsage( token ) );

        if ( TryProcessDelegateParameter( token, errors ) )
            return errors;

        return ProcessArgument( token, errors );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool TryProcessDelegateParameter(IntermediateToken token, Chain<ParsedExpressionBuilderError> errors)
    {
        var parameter = _delegateCollectionState?.TryGetParameter( this, token.Symbol );
        if ( parameter is null )
            return false;

        if ( errors.Count == 0 )
            PushOperand( parameter );

        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Chain<ParsedExpressionBuilderError> ProcessVariable(VariableAssignment assignment)
    {
        if ( assignment.Expression.Right is ConstantExpression constant )
        {
            PushOperand( constant );
            return Chain<ParsedExpressionBuilderError>.Empty;
        }

        var variable = assignment.Variable;
        _delegateCollectionState?.AddVariableCapture( this, variable );
        PushOperand( variable );
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Chain<ParsedExpressionBuilderError> ProcessMacro(IntermediateToken token, MacroDeclaration macro)
    {
        if ( macro.ParameterCount == 0 )
            return macro.Process( _rootState, token, Array.Empty<IReadOnlyList<IntermediateToken>>() );

        _rootState.ActiveState = ExpressionBuilderChildState.CreateFunctionParameters( this );
        _tokenStack.Push( (token, _expectation) );
        _expectation = Expectation.MacroParametersResolution;
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Chain<ParsedExpressionBuilderError> ProcessArgument(IntermediateToken token, Chain<ParsedExpressionBuilderError> errors)
    {
        if ( ! TokenValidation.IsValidLocalTermName( token.Symbol, Configuration.StringDelimiter ) )
            errors = errors.Extend( ParsedExpressionBuilderError.CreateInvalidArgumentName( token ) );

        if ( errors.Count > 0 )
            return errors;

        var (argumentAccess, index) = LocalTerms.GetOrAddArgumentAccess( token.Symbol );
        if ( _delegateCollectionState is not null && index is not null )
            _delegateCollectionState.AddArgumentCapture( this, index.Value );

        PushOperand( argumentAccess );
        return errors;
    }

    [Conditional( "DEBUG" )]
    private void AddAssumedExpectation(Expectation expectation)
    {
        _expectation |= expectation;
    }

    [Conditional( "DEBUG" )]
    private void AssumeStateExpectation(Expectation expectation)
    {
        Assume.NotEquals( _expectation & expectation, Expectation.None );
    }

    [Conditional( "DEBUG" )]
    private static void AssumeExpectation(Expectation expectation, Expectation expected)
    {
        Assume.NotEquals( expectation & expected, Expectation.None );
    }

    [Conditional( "DEBUG" )]
    private static void AssumeAllExpectations(Expectation expectation, Expectation expected)
    {
        Assume.Equals( expectation & expected, expected );
    }

    [Conditional( "DEBUG" )]
    private static void AssumeTokenType(IntermediateToken token, IntermediateTokenType expected)
    {
        Assume.Equals( token.Type, expected );
    }

    [Conditional( "DEBUG" )]
    private static void AssumeConstructsType(ConstructTokenDefinition constructs, ParsedExpressionConstructType expected)
    {
        Assume.NotEquals( constructs.Type & expected, ParsedExpressionConstructType.None );
    }

    [Flags]
    protected enum Expectation : ulong
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
        Assignment = 0x100,
        ParameterType = 0x200,
        ParameterName = 0x400,
        VariableName = 0x800,
        MacroName = 0x1000,
        LocalTermDeclaration = 0x2000,
        MacroEnd = 0x4000,
        InlineParameterSeparator = 0x8000,
        VariableResolution = 0x800000000000,
        MacroResolution = 0x1000000000000,
        MacroParametersResolution = 0x2000000000000,
        InlineParametersResolution = 0x4000000000000,
        InlineDelegateResolution = 0x8000000000000,
        InvocationResolution = 0x10000000000000,
        IndexerResolution = 0x20000000000000,
        ArrayResolution = 0x40000000000000,
        FunctionResolution = 0x80000000000000,
        MethodResolution = 0x100000000000000,
        ConstructorResolution = 0x200000000000000,
        PrefixUnaryConstructResolution = 0x400000000000000,
        AmbiguousPostfixConstructResolution = 0x800000000000000,
        AmbiguousPrefixConstructResolution = 0x1000000000000000,
        MacroParametersStart = 0x2000000000000000,
        ArrayElementsStart = 0x4000000000000000,
        FunctionParametersStart = 0x8000000000000000
    }
}
