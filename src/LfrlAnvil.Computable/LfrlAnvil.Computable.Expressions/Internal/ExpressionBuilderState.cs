﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Computable.Expressions.Errors;

namespace LfrlAnvil.Computable.Expressions.Internal;

// TODO:
// this might have to memorize parent state (e.g. for functions or member accesses, so that we can give a bit more descriptive error msg)
internal sealed class ExpressionBuilderState
{
    private readonly ExpressionTokenStack _tokenStack;
    private readonly ExpressionOperandStack _operandStack;
    private readonly Dictionary<StringSlice, int> _argumentIndexes;
    private readonly List<Expression> _argumentAccessExpressions;
    private readonly ParameterExpression _parameterExpression;
    private readonly ParsedExpressionFactoryInternalConfiguration _configuration;
    private readonly IParsedExpressionNumberParser _numberParser;
    private readonly ExpressionBuilderState _rootState;
    private ExpressionBuilderState _activeState;
    private Expectation _expectation;

    private ExpressionBuilderState(
        ExpressionBuilderState? rootState,
        Dictionary<StringSlice, int> argumentIndexes,
        List<Expression> argumentAccessExpressions,
        ParameterExpression parameterExpression,
        ParsedExpressionFactoryInternalConfiguration configuration,
        IParsedExpressionNumberParser numberParser)
    {
        _tokenStack = new ExpressionTokenStack();
        _operandStack = new ExpressionOperandStack();
        _argumentIndexes = argumentIndexes;
        _argumentAccessExpressions = argumentAccessExpressions;
        _parameterExpression = parameterExpression;
        _configuration = configuration;
        _numberParser = numberParser;
        OperandCount = 0;
        OperatorCount = 0;
        ParenthesesCount = 0;
        _expectation = Expectation.Operand | Expectation.OpenedParenthesis | Expectation.PrefixUnaryConstruct | Expectation.Function;
        LastHandledToken = null;
        _rootState = rootState ?? this;
        _activeState = this;
    }

    internal int OperandCount { get; private set; }
    internal int OperatorCount { get; private set; }
    internal int ParenthesesCount { get; private set; }
    internal IntermediateToken? LastHandledToken { get; private set; }

    [Pure]
    internal static ExpressionBuilderState CreateRoot(
        Type argumentType,
        ParsedExpressionFactoryInternalConfiguration configuration,
        IParsedExpressionNumberParser numberParser)
    {
        var argumentArrayType = argumentType.MakeArrayType();
        var parameterExpression = Expression.Parameter( argumentArrayType, "args" );

        var result = new ExpressionBuilderState(
            rootState: null,
            argumentIndexes: new Dictionary<StringSlice, int>(),
            argumentAccessExpressions: new List<Expression>(),
            parameterExpression: parameterExpression,
            configuration: configuration,
            numberParser: numberParser );

        return result;
    }

    [Pure]
    internal ExpressionBuilderState CreateChild()
    {
        var result = new ExpressionBuilderState(
            rootState: _rootState,
            argumentIndexes: _argumentIndexes,
            argumentAccessExpressions: _argumentAccessExpressions,
            parameterExpression: _parameterExpression,
            configuration: _configuration,
            numberParser: _numberParser );

        _activeState = result;
        _rootState._activeState = result;
        return result;
    }

    internal Chain<ParsedExpressionBuilderError> HandleToken(IntermediateToken token)
    {
        // TODO: better to make a separate class for root state
        Debug.Assert( ReferenceEquals( this, _rootState ), "only root state can handle tokens explicitly" );
        return _activeState.HandleTokenInternal( token );
    }

    internal UnsafeBuilderResult<ExpressionBuilderResult> GetResult(Type outputType)
    {
        var errors = HandleAmbiguousConstructAsPostfixUnaryOperator();

        if ( OperandCount == 0 )
            errors = errors.Extend( ParsedExpressionBuilderError.CreateExpressionMustContainAtLeastOneOperand() );

        if ( OperandCount != OperatorCount + 1 )
            errors = errors.Extend( ParsedExpressionBuilderError.CreateExpressionContainsInvalidOperandToOperatorRatio() );

        if ( ParenthesesCount > 0 )
        {
            var remainingOpenedParenthesisTokens = _tokenStack
                .Select( x => x.Token )
                .Where( t => t.Type == IntermediateTokenType.OpenedParenthesis );

            errors = errors.Extend(
                ParsedExpressionBuilderError.CreateExpressionContainsUnclosedParentheses( remainingOpenedParenthesisTokens ) );
        }

        if ( errors.Count > 0 )
            return UnsafeBuilderResult<ExpressionBuilderResult>.CreateErrors( errors );

        if ( _tokenStack.TryPeek( out var data ) && data.Expectation == Expectation.PrefixUnaryConstruct )
        {
            _tokenStack.Pop();

            errors = ProcessPrefixUnaryConstruct( data.Token );
            if ( errors.Count > 0 )
                return UnsafeBuilderResult<ExpressionBuilderResult>.CreateErrors( errors );
        }

        while ( _tokenStack.TryPop( out data ) )
        {
            AssumeExpectation( data.Expectation, Expectation.BinaryOperator );

            errors = ProcessBinaryOperator( data.Token );
            if ( errors.Count > 0 )
                return UnsafeBuilderResult<ExpressionBuilderResult>.CreateErrors( errors );
        }

        var typeCastResult = ConvertResultToOutputType( _operandStack.Pop(), outputType );
        if ( ! typeCastResult.IsOk )
            return typeCastResult.CastErrorsTo<ExpressionBuilderResult>();

        var result = new ExpressionBuilderResult( typeCastResult.Result!, _parameterExpression, _argumentIndexes );
        return UnsafeBuilderResult<ExpressionBuilderResult>.CreateOk( result );
    }

    private UnsafeBuilderResult<Expression> ConvertResultToOutputType(Expression rawBody, Type outputType)
    {
        Assume.IsEmpty( _operandStack, nameof( _operandStack ) );

        if ( rawBody.Type == outputType )
            return UnsafeBuilderResult<Expression>.CreateOk( rawBody );

        if ( ! _configuration.ConvertResultToOutputTypeAutomatically )
        {
            return rawBody.Type.IsAssignableTo( outputType )
                ? UnsafeBuilderResult<Expression>.CreateOk( Expression.Convert( rawBody, outputType ) )
                : UnsafeBuilderResult<Expression>.CreateErrors(
                    ParsedExpressionBuilderError.CreateExpressionResultTypeIsNotCompatibleWithExpectedOutputType(
                        rawBody.Type,
                        outputType ) );
        }

        var validConverter = _configuration.FindFirstValidTypeConverter( rawBody.Type, outputType );
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

    private Chain<ParsedExpressionBuilderError> HandleTokenInternal(IntermediateToken token)
    {
        var errors = token.Type switch
        {
            IntermediateTokenType.NumberConstant => HandleNumberConstant( token ),
            IntermediateTokenType.StringConstant => HandleStringConstant( token ),
            IntermediateTokenType.BooleanConstant => HandleBooleanConstant( token ),
            IntermediateTokenType.OpenedParenthesis => HandleOpenedParenthesis( token ),
            IntermediateTokenType.ClosedParenthesis => HandleClosedParenthesis( token ),
            IntermediateTokenType.Constructs => HandleConstructs( token ),
            IntermediateTokenType.MemberAccess => HandleMemberAccess( token ),
            IntermediateTokenType.FunctionParameterSeparator => HandleFunctionParameterSeparator( token ),
            IntermediateTokenType.InlineFunctionSeparator => HandleInlineFunctionSeparator( token ),
            _ => HandleArgument( token )
        };

        LastHandledToken = token;
        return errors;
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

        if ( token.Symbol.Source[endIndex] != _configuration.StringDelimiter )
            errors = errors.Extend( ParsedExpressionBuilderError.CreateStringConstantParsingFailure( token ) );

        if ( errors.Count > 0 )
            return errors;

        var builder = new StringBuilder();
        ++index;

        while ( index < endIndex )
        {
            var c = token.Symbol.Source[index];
            builder.Append( c );
            index += c == _configuration.StringDelimiter ? 2 : 1;
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

        var errors = HandleAmbiguousConstructAsBinaryOperator();

        if ( ! Expects( Expectation.Operand ) )
            errors = errors.Extend( ParsedExpressionBuilderError.CreateUnexpectedOperand( token ) );

        if ( ! TokenValidation.IsValidArgumentName( token.Symbol, _configuration.StringDelimiter ) )
            errors = errors.Extend( ParsedExpressionBuilderError.CreateInvalidArgumentName( token ) );

        if ( errors.Count > 0 )
            return errors;

        var expression = GetOrAddArgumentAccessExpression( token.Symbol );
        PushOperand( expression );
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> HandleConstructs(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.Constructs );
        Assume.IsNotNull( token.Constructs, nameof( token.Constructs ) );

        if ( token.Constructs.Type == ConstructTokenType.Function )
        {
            var errors = HandleAmbiguousConstructAsBinaryOperator();
            if ( errors.Count > 0 )
                return errors;

            throw new NotImplementedException( "Function constructs aren't supported yet." );
        }

        if ( token.Constructs.Type == ConstructTokenType.Constant )
            return HandleConstant( token );

        if ( Expects( Expectation.AmbiguousPrefixConstructResolution ) )
        {
            var errors = HandleAmbiguousConstructAsBinaryOperator();
            if ( errors.Count > 0 )
                return errors;
        }

        if ( Expects( Expectation.PostfixUnaryConstruct | Expectation.BinaryOperator ) )
        {
            var hasBinaryOperators = ! token.Constructs.BinaryOperators.IsEmpty;
            var hasPostfixUnaryConstructs =
                ! token.Constructs.PostfixUnaryOperators.IsEmpty || ! token.Constructs.PostfixTypeConverters.IsEmpty;

            if ( hasBinaryOperators )
            {
                if ( ! hasPostfixUnaryConstructs )
                    return PushBinaryOperator( token );

                _expectation &= ~Expectation.PostfixUnaryConstruct;
                _expectation |= Expectation.AmbiguousPostfixConstructResolution | Expectation.PrefixUnaryConstruct;
                _tokenStack.Push( token, Expectation.PostfixUnaryConstruct | Expectation.BinaryOperator );
                return Chain<ParsedExpressionBuilderError>.Empty;
            }

            return hasPostfixUnaryConstructs
                ? PushPostfixUnaryConstruct( token )
                : Chain.Create( ParsedExpressionBuilderError.CreatePostfixUnaryOrBinaryConstructDoesNotExist( token ) );
        }

        if ( Expects( Expectation.PrefixUnaryConstruct | Expectation.BinaryOperator ) )
        {
            AssumeStateExpectation( Expectation.AmbiguousPostfixConstructResolution );

            var hasBinaryOperators = ! token.Constructs.BinaryOperators.IsEmpty;
            var hasPrefixUnaryConstructs =
                ! token.Constructs.PrefixUnaryOperators.IsEmpty || ! token.Constructs.PrefixTypeConverters.IsEmpty;

            if ( hasBinaryOperators )
            {
                if ( ! hasPrefixUnaryConstructs )
                    return PushBinaryOperator( token );

                _expectation &= ~Expectation.BinaryOperator;
                _expectation |= Expectation.AmbiguousPrefixConstructResolution;
                _tokenStack.Push( token, Expectation.PrefixUnaryConstruct | Expectation.BinaryOperator );
                return Chain<ParsedExpressionBuilderError>.Empty;
            }

            return hasPrefixUnaryConstructs
                ? PushPrefixUnaryConstruct( token )
                : Chain.Create( ParsedExpressionBuilderError.CreateBinaryOrPrefixUnaryConstructDoesNotExist( token ) );
        }

        if ( Expects( Expectation.BinaryOperator ) )
            return PushBinaryOperator( token );

        if ( Expects( Expectation.PrefixUnaryConstruct ) )
            return PushPrefixUnaryConstruct( token );

        return Chain.Create( ParsedExpressionBuilderError.CreateUnexpectedConstruct( token ) );
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

        PushOperand( token.Constructs.Constant.Expression );
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> HandleOpenedParenthesis(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.OpenedParenthesis );

        var errors = HandleAmbiguousConstructAsBinaryOperator();
        if ( errors.Count > 0 )
            return errors;

        if ( ! Expects( Expectation.OpenedParenthesis ) )
            return Chain.Create( ParsedExpressionBuilderError.CreateUnexpectedOpenedParenthesis( token ) );

        _tokenStack.Push( token, Expectation.OpenedParenthesis );

        _expectation = Expectation.Operand | Expectation.OpenedParenthesis | Expectation.PrefixUnaryConstruct | Expectation.Function;
        ++ParenthesesCount;
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> HandleClosedParenthesis(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.ClosedParenthesis );

        var errors = HandleAmbiguousConstructAsPostfixUnaryOperator();
        if ( errors.Count > 0 )
            return errors;

        if ( ! Expects( Expectation.ClosedParenthesis ) )
            return Chain.Create( ParsedExpressionBuilderError.CreateUnexpectedClosedParenthesis( token ) );

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

        _expectation = Expectation.PostfixUnaryConstruct | Expectation.BinaryOperator;
        if ( --ParenthesesCount > 0 )
            _expectation |= Expectation.ClosedParenthesis;

        if ( _tokenStack.TryPeek( out data ) && data.Expectation == Expectation.PrefixUnaryConstruct )
            _expectation |= Expectation.PrefixUnaryConstructResolution;

        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> HandleMemberAccess(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.MemberAccess );
        throw new NotSupportedException( "Member access token is not supported yet." );
    }

    private Chain<ParsedExpressionBuilderError> HandleFunctionParameterSeparator(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.FunctionParameterSeparator );
        throw new NotSupportedException( "Function parameter separator token is not supported yet." );
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

        return token.Constructs.Type == ConstructTokenType.Operator
            ? ProcessPrefixUnaryOperator( token )
            : ProcessPrefixTypeConverter( token );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Chain<ParsedExpressionBuilderError> ProcessPrefixUnaryOperator(IntermediateToken token)
    {
        Assume.IsNotEmpty( _operandStack, nameof( _operandStack ) );
        Assume.IsNotNull( token.Constructs, nameof( token.Constructs ) );

        var argumentType = _operandStack[0].Type;
        var @operator = token.Constructs.PrefixUnaryOperators.FindConstruct( argumentType );

        return @operator is null
            ? Chain.Create( ParsedExpressionBuilderError.CreatePrefixUnaryOperatorDoesNotExist( token, argumentType ) )
            : ProcessUnaryOperator( token, @operator );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Chain<ParsedExpressionBuilderError> ProcessPrefixTypeConverter(IntermediateToken token)
    {
        Assume.IsNotEmpty( _operandStack, nameof( _operandStack ) );
        Assume.IsNotNull( token.Constructs, nameof( token.Constructs ) );

        var sourceType = _operandStack[0].Type;
        var converter = token.Constructs.PrefixTypeConverters.FindConstruct( sourceType );

        return converter is null
            ? Chain.Create( ParsedExpressionBuilderError.CreatePrefixTypeConverterDoesNotExist( token, sourceType ) )
            : ProcessTypeConverter( token, converter );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Chain<ParsedExpressionBuilderError> ProcessPostfixUnaryConstruct(IntermediateToken token)
    {
        Assume.IsNotNull( token.Constructs, nameof( token.Constructs ) );

        return token.Constructs.Type == ConstructTokenType.Operator
            ? ProcessPostfixUnaryOperator( token )
            : ProcessPostfixTypeConverter( token );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Chain<ParsedExpressionBuilderError> ProcessPostfixUnaryOperator(IntermediateToken token)
    {
        Assume.IsNotEmpty( _operandStack, nameof( _operandStack ) );
        Assume.IsNotNull( token.Constructs, nameof( token.Constructs ) );

        var argumentType = _operandStack[0].Type;
        var @operator = token.Constructs.PostfixUnaryOperators.FindConstruct( argumentType );

        return @operator is null
            ? Chain.Create( ParsedExpressionBuilderError.CreatePostfixUnaryOperatorDoesNotExist( token, argumentType ) )
            : ProcessUnaryOperator( token, @operator );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Chain<ParsedExpressionBuilderError> ProcessPostfixTypeConverter(IntermediateToken token)
    {
        Assume.IsNotEmpty( _operandStack, nameof( _operandStack ) );
        Assume.IsNotNull( token.Constructs, nameof( token.Constructs ) );

        var sourceType = _operandStack[0].Type;
        var converter = token.Constructs.PostfixTypeConverters.FindConstruct( sourceType );

        return converter is null
            ? Chain.Create( ParsedExpressionBuilderError.CreatePostfixTypeConverterDoesNotExist( token, sourceType ) )
            : ProcessTypeConverter( token, converter );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Chain<ParsedExpressionBuilderError> ProcessBinaryOperator(IntermediateToken token)
    {
        Assume.ContainsAtLeast( _operandStack, 2, nameof( _operandStack ) );
        Assume.IsNotNull( token.Constructs, nameof( token.Constructs ) );

        var rightArgumentType = _operandStack[0].Type;
        var leftArgumentType = _operandStack[1].Type;
        var binaryOperator = token.Constructs.BinaryOperators.FindConstruct( leftArgumentType, rightArgumentType );

        return binaryOperator is null
            ? Chain.Create(
                ParsedExpressionBuilderError.CreateBinaryOperatorDoesNotExist( token, leftArgumentType, rightArgumentType ) )
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

        var rightOperand = _operandStack.Pop();
        var leftOperand = _operandStack.Pop();
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
            return Chain.Create( ParsedExpressionBuilderError.CreateBinaryOperatorCollectionIsEmpty( token ) );

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

        _tokenStack.Push( token, Expectation.BinaryOperator );
        _expectation = Expectation.Operand | Expectation.OpenedParenthesis | Expectation.PrefixUnaryConstruct | Expectation.Function;
        ++OperatorCount;
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> PushPrefixUnaryConstruct(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.Constructs );
        AssumeStateExpectation( Expectation.PrefixUnaryConstruct );
        Assume.IsNotNull( token.Constructs, nameof( token.Constructs ) );

        var errors = HandleAmbiguousConstructAsBinaryOperator();

        if ( token.Constructs.Type == ConstructTokenType.Operator )
        {
            if ( token.Constructs.PrefixUnaryOperators.IsEmpty )
                errors = errors.Extend( ParsedExpressionBuilderError.CreatePrefixUnaryOperatorCollectionIsEmpty( token ) );
        }
        else if ( token.Constructs.PrefixTypeConverters.IsEmpty )
            errors = errors.Extend( ParsedExpressionBuilderError.CreatePrefixTypeConverterCollectionIsEmpty( token ) );

        if ( errors.Count > 0 )
            return errors;

        _tokenStack.Push( token, Expectation.PrefixUnaryConstruct );
        _expectation &= ~Expectation.PrefixUnaryConstruct;
        _expectation |= Expectation.PrefixUnaryConstructResolution;
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> PushPostfixUnaryConstruct(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.Constructs );
        AssumeStateExpectation( Expectation.PostfixUnaryConstruct );
        Assume.IsNotNull( token.Constructs, nameof( token.Constructs ) );

        var isEmpty = token.Constructs.Type == ConstructTokenType.Operator
            ? token.Constructs.PostfixUnaryOperators.IsEmpty
            : token.Constructs.PostfixTypeConverters.IsEmpty;

        Assume.False( isEmpty, "Assumed postfix unary constructs collection to not be empty." );

        if ( Expects( Expectation.PrefixUnaryConstructResolution ) )
        {
            Assume.IsNotEmpty( _tokenStack, nameof( _tokenStack ) );
            var prefixData = _tokenStack.Pop();

            AssumeExpectation( prefixData.Expectation, Expectation.PrefixUnaryConstruct );
            Assume.IsNotNull( prefixData.Token.Constructs, nameof( prefixData.Token.Constructs ) );

            var prefixPrecedence = prefixData.Token.Constructs.Type == ConstructTokenType.Operator
                ? prefixData.Token.Constructs.PrefixUnaryOperators.Precedence
                : prefixData.Token.Constructs.PrefixTypeConverters.Precedence;

            var postfixPrecedence = token.Constructs.Type == ConstructTokenType.Operator
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

        _expectation &= ~Expectation.PostfixUnaryConstruct;
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private void PushOperand(Expression operand)
    {
        AssumeStateExpectation( Expectation.Operand );

        _operandStack.Push( operand );

        _expectation = (_expectation & Expectation.PrefixUnaryConstructResolution) |
            Expectation.PostfixUnaryConstruct |
            Expectation.BinaryOperator;

        if ( ParenthesesCount > 0 )
            _expectation |= Expectation.ClosedParenthesis;

        ++OperandCount;
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
        _expectation |= Expectation.PostfixUnaryConstruct;

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
        _expectation |= Expectation.BinaryOperator;

        return PushBinaryOperator( data.Token );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool Expects(Expectation expectation)
    {
        return (_expectation & expectation) == expectation;
    }

    private Expression GetOrAddArgumentAccessExpression(StringSlice name)
    {
        if ( _argumentIndexes.TryGetValue( name, out var index ) )
            return _argumentAccessExpressions[index];

        index = _argumentIndexes.Count;
        var indexExpression = Expression.Constant( index );
        var result = Expression.ArrayIndex( _parameterExpression, indexExpression );

        _argumentIndexes.Add( name, index );
        _argumentAccessExpressions.Add( result );
        return result;
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

    [Flags]
    internal enum Expectation : ushort
    {
        None = 0,
        Operand = 1,
        OpenedParenthesis = 2,
        ClosedParenthesis = 4,
        PrefixUnaryConstruct = 8,
        PostfixUnaryConstruct = 16,
        BinaryOperator = 32,
        Function = 64,
        PrefixUnaryConstructResolution = 128,
        AmbiguousPostfixConstructResolution = 256,
        AmbiguousPrefixConstructResolution = 512
    }
}
