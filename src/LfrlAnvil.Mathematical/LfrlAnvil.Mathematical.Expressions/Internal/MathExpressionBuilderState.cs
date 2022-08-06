﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using LfrlAnvil.Mathematical.Expressions.Constructs;
using LfrlAnvil.Mathematical.Expressions.Errors;

namespace LfrlAnvil.Mathematical.Expressions.Internal;

// TODO:
// this might have to memorize parent state (e.g. for functions or member accesses, so that we can give a bit more descriptive error msg)
internal sealed class MathExpressionBuilderState
{
    private readonly MathExpressionTokenStack _tokenStack;
    private readonly MathExpressionOperandStack _operandStack;
    private readonly Dictionary<StringSlice, int> _argumentIndexes;
    private readonly List<Expression> _argumentAccessExpressions;
    private readonly ParameterExpression _parameterExpression;
    private readonly MathExpressionFactoryInternalConfiguration _configuration;
    private readonly IMathExpressionNumberParser _numberParser;
    private readonly MathExpressionBuilderState _rootState;
    private MathExpressionBuilderState _activeState;
    private Expectation _expectation;

    private MathExpressionBuilderState(
        MathExpressionBuilderState? rootState,
        Dictionary<StringSlice, int> argumentIndexes,
        List<Expression> argumentAccessExpressions,
        ParameterExpression parameterExpression,
        MathExpressionFactoryInternalConfiguration configuration,
        IMathExpressionNumberParser numberParser)
    {
        _tokenStack = new MathExpressionTokenStack();
        _operandStack = new MathExpressionOperandStack();
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
    internal static MathExpressionBuilderState CreateRoot(
        Type argumentType,
        MathExpressionFactoryInternalConfiguration configuration,
        IMathExpressionNumberParser numberParser)
    {
        var argumentArrayType = argumentType.MakeArrayType();
        var parameterExpression = Expression.Parameter( argumentArrayType, "args" );

        var result = new MathExpressionBuilderState(
            rootState: null,
            argumentIndexes: new Dictionary<StringSlice, int>(),
            argumentAccessExpressions: new List<Expression>(),
            parameterExpression: parameterExpression,
            configuration: configuration,
            numberParser: numberParser );

        return result;
    }

    [Pure]
    internal MathExpressionBuilderState CreateChild()
    {
        var result = new MathExpressionBuilderState(
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

    internal Chain<MathExpressionBuilderError> HandleToken(IntermediateToken token)
    {
        // TODO: better to make a separate class for root state
        Debug.Assert( ReferenceEquals( this, _rootState ), "only root state can handle tokens explicitly" );
        return _activeState.HandleTokenInternal( token );
    }

    internal UnsafeBuilderResult<MathExpressionBuilderResult> GetResult(Type outputType)
    {
        var errors = HandleAmbiguousConstructAsPostfixUnaryOperator();

        if ( OperandCount == 0 )
            errors = errors.Extend( MathExpressionBuilderError.CreateExpressionMustContainAtLeastOneOperand() );

        if ( OperandCount != OperatorCount + 1 )
            errors = errors.Extend( MathExpressionBuilderError.CreateExpressionContainsInvalidOperandToOperatorRatio() );

        if ( ParenthesesCount > 0 )
        {
            var remainingOpenedParenthesisTokens = _tokenStack
                .Select( x => x.Token )
                .Where( t => t.Type == IntermediateTokenType.OpenedParenthesis );

            errors = errors.Extend(
                MathExpressionBuilderError.CreateExpressionContainsUnclosedParentheses( remainingOpenedParenthesisTokens ) );
        }

        if ( errors.Count > 0 )
            return UnsafeBuilderResult<MathExpressionBuilderResult>.CreateErrors( errors );

        if ( _tokenStack.TryPeek( out var data ) && data.Expectation == Expectation.PrefixUnaryConstruct )
        {
            _tokenStack.Pop();

            errors = ProcessPrefixUnaryConstruct( data.Token );
            if ( errors.Count > 0 )
                return UnsafeBuilderResult<MathExpressionBuilderResult>.CreateErrors( errors );
        }

        while ( _tokenStack.TryPop( out data ) )
        {
            AssumeExpectation( data.Expectation, Expectation.BinaryOperator );

            errors = ProcessBinaryOperator( data.Token );
            if ( errors.Count > 0 )
                return UnsafeBuilderResult<MathExpressionBuilderResult>.CreateErrors( errors );
        }

        var typeCastResult = ConvertResultToOutputType( _operandStack.Pop(), outputType );
        if ( ! typeCastResult.IsOk )
            return typeCastResult.CastErrorsTo<MathExpressionBuilderResult>();

        var result = new MathExpressionBuilderResult( typeCastResult.Result!, _parameterExpression, _argumentIndexes );
        return UnsafeBuilderResult<MathExpressionBuilderResult>.CreateOk( result );
    }

    private UnsafeBuilderResult<Expression> ConvertResultToOutputType(Expression rawBody, Type outputType)
    {
        AssumeEmptyOperandStack();

        if ( rawBody.Type == outputType )
            return UnsafeBuilderResult<Expression>.CreateOk( rawBody );

        if ( ! _configuration.ConvertResultToOutputTypeAutomatically )
        {
            return rawBody.Type.IsAssignableTo( outputType )
                ? UnsafeBuilderResult<Expression>.CreateOk( Expression.Convert( rawBody, outputType ) )
                : UnsafeBuilderResult<Expression>.CreateErrors(
                    MathExpressionBuilderError.CreateExpressionResultTypeIsNotCompatibleWithExpectedOutputType(
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

        validConverter = new MathExpressionTypeConverter( outputType );

        var castErrors = ProcessOutputTypeConverter( validConverter, rawBody );
        return castErrors.Count > 0
            ? UnsafeBuilderResult<Expression>.CreateErrors( castErrors )
            : UnsafeBuilderResult<Expression>.CreateOk( _operandStack.Pop() );
    }

    private Chain<MathExpressionBuilderError> HandleTokenInternal(IntermediateToken token)
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

    private Chain<MathExpressionBuilderError> HandleNumberConstant(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.NumberConstant );

        var errors = HandleAmbiguousConstructAsBinaryOperator();

        if ( ! Expects( Expectation.Operand ) )
            errors = errors.Extend( MathExpressionBuilderError.CreateUnexpectedOperand( token ) );

        if ( ! _numberParser.TryParse( token.Symbol.AsSpan(), out var value ) )
            errors = errors.Extend( MathExpressionBuilderError.CreateNumberConstantParsingFailure( token ) );

        if ( errors.Count > 0 )
            return errors;

        var expression = Expression.Constant( value );
        PushOperand( expression );
        return Chain<MathExpressionBuilderError>.Empty;
    }

    private Chain<MathExpressionBuilderError> HandleStringConstant(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.StringConstant );

        var errors = HandleAmbiguousConstructAsBinaryOperator();

        if ( ! Expects( Expectation.Operand ) )
            errors = errors.Extend( MathExpressionBuilderError.CreateUnexpectedOperand( token ) );

        var index = token.Symbol.StartIndex;
        var endIndex = token.Symbol.EndIndex - 1;

        if ( token.Symbol.Source[endIndex] != _configuration.StringDelimiter )
            errors = errors.Extend( MathExpressionBuilderError.CreateStringConstantParsingFailure( token ) );

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
        return Chain<MathExpressionBuilderError>.Empty;
    }

    private Chain<MathExpressionBuilderError> HandleBooleanConstant(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.BooleanConstant );

        var errors = HandleAmbiguousConstructAsBinaryOperator();

        if ( ! Expects( Expectation.Operand ) )
            errors = errors.Extend( MathExpressionBuilderError.CreateUnexpectedOperand( token ) );

        if ( errors.Count > 0 )
            return errors;

        var value = TokenConstants.IsBooleanTrue( token.Symbol );
        var expression = Expression.Constant( value );
        PushOperand( expression );
        return Chain<MathExpressionBuilderError>.Empty;
    }

    private Chain<MathExpressionBuilderError> HandleArgument(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.Argument );

        var errors = HandleAmbiguousConstructAsBinaryOperator();

        if ( ! Expects( Expectation.Operand ) )
            errors = errors.Extend( MathExpressionBuilderError.CreateUnexpectedOperand( token ) );

        if ( ! TokenValidation.IsValidArgumentName( token.Symbol, _configuration.StringDelimiter ) )
            errors = errors.Extend( MathExpressionBuilderError.CreateInvalidArgumentName( token ) );

        if ( errors.Count > 0 )
            return errors;

        var expression = GetOrAddArgumentAccessExpression( token.Symbol );
        PushOperand( expression );
        return Chain<MathExpressionBuilderError>.Empty;
    }

    private Chain<MathExpressionBuilderError> HandleConstructs(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.Constructs );
        AssumeNotNullTokenConstructs( token.Constructs );

        if ( token.Constructs.Type == MathExpressionConstructTokenType.Function )
        {
            var errors = HandleAmbiguousConstructAsBinaryOperator();
            if ( errors.Count > 0 )
                return errors;

            throw new NotSupportedException( "Function constructs aren't supported yet." );
        }

        if ( token.Constructs.Type == MathExpressionConstructTokenType.Constant )
        {
            var errors = HandleAmbiguousConstructAsBinaryOperator();
            if ( errors.Count > 0 )
                return errors;

            throw new NotSupportedException( "Constant constructs aren't supported yet." );
        }

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
                return Chain<MathExpressionBuilderError>.Empty;
            }

            return hasPostfixUnaryConstructs
                ? PushPostfixUnaryConstruct( token )
                : Chain.Create( MathExpressionBuilderError.CreatePostfixUnaryOrBinaryConstructDoesNotExist( token ) );
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
                return Chain<MathExpressionBuilderError>.Empty;
            }

            return hasPrefixUnaryConstructs
                ? PushPrefixUnaryConstruct( token )
                : Chain.Create( MathExpressionBuilderError.CreateBinaryOrPrefixUnaryConstructDoesNotExist( token ) );
        }

        if ( Expects( Expectation.BinaryOperator ) )
            return PushBinaryOperator( token );

        if ( Expects( Expectation.PrefixUnaryConstruct ) )
            return PushPrefixUnaryConstruct( token );

        // TODO: add custom constant construct handling

        return Chain.Create( MathExpressionBuilderError.CreateUnexpectedConstruct( token ) );
    }

    private Chain<MathExpressionBuilderError> HandleOpenedParenthesis(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.OpenedParenthesis );

        var errors = HandleAmbiguousConstructAsBinaryOperator();
        if ( errors.Count > 0 )
            return errors;

        if ( ! Expects( Expectation.OpenedParenthesis ) )
            return Chain.Create( MathExpressionBuilderError.CreateUnexpectedOpenedParenthesis( token ) );

        _tokenStack.Push( token, Expectation.OpenedParenthesis );

        _expectation = Expectation.Operand | Expectation.OpenedParenthesis | Expectation.PrefixUnaryConstruct | Expectation.Function;
        ++ParenthesesCount;
        return Chain<MathExpressionBuilderError>.Empty;
    }

    private Chain<MathExpressionBuilderError> HandleClosedParenthesis(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.ClosedParenthesis );

        var errors = HandleAmbiguousConstructAsPostfixUnaryOperator();
        if ( errors.Count > 0 )
            return errors;

        if ( ! Expects( Expectation.ClosedParenthesis ) )
            return Chain.Create( MathExpressionBuilderError.CreateUnexpectedClosedParenthesis( token ) );

        AssumeTokenStackMinCount( 1 );
        var data = _tokenStack.Peek();

        while ( data.Expectation != Expectation.OpenedParenthesis )
        {
            AssumeExpectation( data.Expectation, Expectation.BinaryOperator | Expectation.PrefixUnaryConstruct );
            AssumeNotNullTokenConstructs( data.Token.Constructs );

            _tokenStack.Pop();

            errors = data.Expectation == Expectation.BinaryOperator
                ? ProcessBinaryOperator( data.Token )
                : ProcessPrefixUnaryConstruct( data.Token );

            if ( errors.Count > 0 )
                return errors;

            AssumeTokenStackMinCount( 1 );
            data = _tokenStack.Peek();
        }

        _tokenStack.Pop();

        _expectation = Expectation.PostfixUnaryConstruct | Expectation.BinaryOperator;
        if ( --ParenthesesCount > 0 )
            _expectation |= Expectation.ClosedParenthesis;

        if ( _tokenStack.TryPeek( out data ) && data.Expectation == Expectation.PrefixUnaryConstruct )
            _expectation |= Expectation.PrefixUnaryConstructResolution;

        return Chain<MathExpressionBuilderError>.Empty;
    }

    private Chain<MathExpressionBuilderError> HandleMemberAccess(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.MemberAccess );
        throw new NotSupportedException( "Member access token is not supported yet." );
    }

    private Chain<MathExpressionBuilderError> HandleFunctionParameterSeparator(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.FunctionParameterSeparator );
        throw new NotSupportedException( "Function parameter separator token is not supported yet." );
    }

    private Chain<MathExpressionBuilderError> HandleInlineFunctionSeparator(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.InlineFunctionSeparator );
        throw new NotSupportedException( "Inline function separator token is not supported yet." );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Chain<MathExpressionBuilderError> ProcessPrefixUnaryConstruct(IntermediateToken token)
    {
        AssumeNotNullTokenConstructs( token.Constructs );

        return token.Constructs.Type == MathExpressionConstructTokenType.Operator
            ? ProcessPrefixUnaryOperator( token )
            : ProcessPrefixTypeConverter( token );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Chain<MathExpressionBuilderError> ProcessPrefixUnaryOperator(IntermediateToken token)
    {
        AssumeOperandStackMinCount( 1 );
        AssumeNotNullTokenConstructs( token.Constructs );

        var argumentType = _operandStack[0].Type;
        var @operator = token.Constructs.PrefixUnaryOperators.FindConstruct( argumentType );

        return @operator is null
            ? Chain.Create( MathExpressionBuilderError.CreatePrefixUnaryOperatorDoesNotExist( token, argumentType ) )
            : ProcessUnaryOperator( token, @operator );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Chain<MathExpressionBuilderError> ProcessPrefixTypeConverter(IntermediateToken token)
    {
        AssumeOperandStackMinCount( 1 );
        AssumeNotNullTokenConstructs( token.Constructs );

        var sourceType = _operandStack[0].Type;
        var converter = token.Constructs.PrefixTypeConverters.FindConstruct( sourceType );

        return converter is null
            ? Chain.Create( MathExpressionBuilderError.CreatePrefixTypeConverterDoesNotExist( token, sourceType ) )
            : ProcessTypeConverter( token, converter );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Chain<MathExpressionBuilderError> ProcessPostfixUnaryConstruct(IntermediateToken token)
    {
        AssumeNotNullTokenConstructs( token.Constructs );

        return token.Constructs.Type == MathExpressionConstructTokenType.Operator
            ? ProcessPostfixUnaryOperator( token )
            : ProcessPostfixTypeConverter( token );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Chain<MathExpressionBuilderError> ProcessPostfixUnaryOperator(IntermediateToken token)
    {
        AssumeOperandStackMinCount( 1 );
        AssumeNotNullTokenConstructs( token.Constructs );

        var argumentType = _operandStack[0].Type;
        var @operator = token.Constructs.PostfixUnaryOperators.FindConstruct( argumentType );

        return @operator is null
            ? Chain.Create( MathExpressionBuilderError.CreatePostfixUnaryOperatorDoesNotExist( token, argumentType ) )
            : ProcessUnaryOperator( token, @operator );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Chain<MathExpressionBuilderError> ProcessPostfixTypeConverter(IntermediateToken token)
    {
        AssumeOperandStackMinCount( 1 );
        AssumeNotNullTokenConstructs( token.Constructs );

        var sourceType = _operandStack[0].Type;
        var converter = token.Constructs.PostfixTypeConverters.FindConstruct( sourceType );

        return converter is null
            ? Chain.Create( MathExpressionBuilderError.CreatePostfixTypeConverterDoesNotExist( token, sourceType ) )
            : ProcessTypeConverter( token, converter );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Chain<MathExpressionBuilderError> ProcessUnaryOperator(IntermediateToken token, MathExpressionUnaryOperator @operator)
    {
        return ProcessConstruct( token, @operator, expectedOperandCount: 1 );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Chain<MathExpressionBuilderError> ProcessTypeConverter(IntermediateToken token, MathExpressionTypeConverter converter)
    {
        return ProcessConstruct( token, converter, expectedOperandCount: 1 );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Chain<MathExpressionBuilderError> ProcessBinaryOperator(IntermediateToken token)
    {
        AssumeOperandStackMinCount( 2 );
        AssumeNotNullTokenConstructs( token.Constructs );

        var rightArgumentType = _operandStack[0].Type;
        var leftArgumentType = _operandStack[1].Type;
        var binaryOperator = token.Constructs.BinaryOperators.FindConstruct( leftArgumentType, rightArgumentType );

        return binaryOperator is null
            ? Chain.Create(
                MathExpressionBuilderError.CreateBinaryOperatorDoesNotExist( token, leftArgumentType, rightArgumentType ) )
            : ProcessBinaryOperator( token, binaryOperator );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Chain<MathExpressionBuilderError> ProcessBinaryOperator(IntermediateToken token, MathExpressionBinaryOperator @operator)
    {
        return ProcessConstruct( token, @operator, expectedOperandCount: 2 );
    }

    private Chain<MathExpressionBuilderError> ProcessConstruct(
        IntermediateToken token,
        IMathExpressionConstruct construct,
        int expectedOperandCount)
    {
        AssumeOperandStackMinCount( expectedOperandCount );
        var operandCount = _operandStack.Count;

        try
        {
            construct.Process( _operandStack );
        }
        catch ( Exception exc )
        {
            return Chain.Create( MathExpressionBuilderError.CreateConstructHasThrownException( token, construct, exc ) );
        }

        // TODO: this cannot really happen, since no known construct type allows to override Process method
        // maybe functions will somehow be able to cause this error
        var consumedOperandsCount = operandCount - _operandStack.Count;
        return consumedOperandsCount == expectedOperandCount - 1
            ? Chain<MathExpressionBuilderError>.Empty
            : Chain.Create( MathExpressionBuilderError.CreateConstructConsumedInvalidAmountOfOperands( token, construct ) );
    }

    private Chain<MathExpressionBuilderError> ProcessOutputTypeConverter(MathExpressionTypeConverter converter, Expression rawBody)
    {
        AssumeEmptyOperandStack();
        _operandStack.Push( rawBody );

        try
        {
            converter.Process( _operandStack );
        }
        catch ( Exception exc )
        {
            return Chain.Create( MathExpressionBuilderError.CreateOutputTypeConverterHasThrownException( converter, exc ) );
        }

        return Chain<MathExpressionBuilderError>.Empty;
    }

    private Chain<MathExpressionBuilderError> PushBinaryOperator(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.Constructs );
        AssumeStateExpectation( Expectation.BinaryOperator );
        AssumeNotNullTokenConstructs( token.Constructs );

        var errors = HandleAmbiguousConstructAsPostfixUnaryOperator();
        if ( errors.Count > 0 )
            return errors;

        if ( Expects( Expectation.PrefixUnaryConstructResolution ) )
        {
            AssumeTokenStackMinCount( 1 );
            var data = _tokenStack.Pop();

            AssumeExpectation( data.Expectation, Expectation.PrefixUnaryConstruct );

            errors = ProcessPrefixUnaryConstruct( data.Token );
            if ( errors.Count > 0 )
                return errors;
        }

        var binaryOperators = token.Constructs.BinaryOperators;
        if ( binaryOperators.IsEmpty )
            return Chain.Create( MathExpressionBuilderError.CreateBinaryOperatorCollectionIsEmpty( token ) );

        while ( _tokenStack.TryPeek( out var data ) && data.Expectation == Expectation.BinaryOperator )
        {
            AssumeNotNullTokenConstructs( data.Token.Constructs );

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
        return Chain<MathExpressionBuilderError>.Empty;
    }

    private Chain<MathExpressionBuilderError> PushPrefixUnaryConstruct(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.Constructs );
        AssumeStateExpectation( Expectation.PrefixUnaryConstruct );
        AssumeNotNullTokenConstructs( token.Constructs );

        var errors = HandleAmbiguousConstructAsBinaryOperator();

        if ( token.Constructs.Type == MathExpressionConstructTokenType.Operator )
        {
            if ( token.Constructs.PrefixUnaryOperators.IsEmpty )
                errors = errors.Extend( MathExpressionBuilderError.CreatePrefixUnaryOperatorCollectionIsEmpty( token ) );
        }
        else if ( token.Constructs.PrefixTypeConverters.IsEmpty )
            errors = errors.Extend( MathExpressionBuilderError.CreatePrefixTypeConverterCollectionIsEmpty( token ) );

        if ( errors.Count > 0 )
            return errors;

        _tokenStack.Push( token, Expectation.PrefixUnaryConstruct );
        _expectation &= ~Expectation.PrefixUnaryConstruct;
        _expectation |= Expectation.PrefixUnaryConstructResolution;
        return Chain<MathExpressionBuilderError>.Empty;
    }

    private Chain<MathExpressionBuilderError> PushPostfixUnaryConstruct(IntermediateToken token)
    {
        AssumeTokenType( token, IntermediateTokenType.Constructs );
        AssumeStateExpectation( Expectation.PostfixUnaryConstruct );
        AssumeNotNullTokenConstructs( token.Constructs );

        var isEmpty = token.Constructs.Type == MathExpressionConstructTokenType.Operator
            ? token.Constructs.PostfixUnaryOperators.IsEmpty
            : token.Constructs.PostfixTypeConverters.IsEmpty;

        Debug.Assert( ! isEmpty, "Postfix unary construct collection should not be empty." );

        if ( Expects( Expectation.PrefixUnaryConstructResolution ) )
        {
            AssumeTokenStackMinCount( 1 );
            var prefixData = _tokenStack.Pop();

            AssumeExpectation( prefixData.Expectation, Expectation.PrefixUnaryConstruct );
            AssumeNotNullTokenConstructs( prefixData.Token.Constructs );

            var prefixPrecedence = prefixData.Token.Constructs.Type == MathExpressionConstructTokenType.Operator
                ? prefixData.Token.Constructs.PrefixUnaryOperators.Precedence
                : prefixData.Token.Constructs.PrefixTypeConverters.Precedence;

            var postfixPrecedence = token.Constructs.Type == MathExpressionConstructTokenType.Operator
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
        return Chain<MathExpressionBuilderError>.Empty;
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

    private Chain<MathExpressionBuilderError> HandleAmbiguousConstructAsPostfixUnaryOperator()
    {
        if ( ! Expects( Expectation.AmbiguousPostfixConstructResolution ) )
            return Chain<MathExpressionBuilderError>.Empty;

        AssumeTokenStackMinCount( 1 );
        var data = _tokenStack.Pop();

        if ( Expects( Expectation.AmbiguousPrefixConstructResolution ) )
            return Chain.Create( MathExpressionBuilderError.CreateAmbiguousPostfixUnaryConstructResolutionFailure( data.Token ) );

        AssumeAllExpectations( data.Expectation, Expectation.BinaryOperator | Expectation.PostfixUnaryConstruct );
        _expectation &= ~Expectation.AmbiguousPostfixConstructResolution;
        _expectation |= Expectation.PostfixUnaryConstruct;

        return PushPostfixUnaryConstruct( data.Token );
    }

    private Chain<MathExpressionBuilderError> HandleAmbiguousConstructAsBinaryOperator()
    {
        if ( ! Expects( Expectation.AmbiguousPostfixConstructResolution ) )
            return Chain<MathExpressionBuilderError>.Empty;

        AssumeTokenStackMinCount( 1 );
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
    private void AssumeTokenStackMinCount(int expected)
    {
        Debug.Assert( _tokenStack.Count >= expected, $"Expected at least {expected} tokens on the stack but found {_tokenStack.Count}." );
    }

    [Conditional( "DEBUG" )]
    private void AssumeOperandStackMinCount(int expected)
    {
        Debug.Assert(
            _operandStack.Count >= expected,
            $"Expected at least {expected} operands on the stack but found {_operandStack.Count}." );
    }

    [Conditional( "DEBUG" )]
    private void AssumeEmptyOperandStack()
    {
        Debug.Assert( _operandStack.Count == 0, "Expected operand stack to be empty." );
    }

    [Conditional( "DEBUG" )]
    private void AssumeStateExpectation(Expectation expectation)
    {
        Debug.Assert( Expects( expectation ), $"State doesn't expect {expectation} but {_expectation}." );
    }

    [Conditional( "DEBUG" )]
    private static void AssumeExpectation(Expectation expectation, Expectation expected)
    {
        Debug.Assert( (expectation & expected) != Expectation.None, $"Expected at least one of {expected} but found {expectation}." );
    }

    [Conditional( "DEBUG" )]
    private static void AssumeAllExpectations(Expectation expectation, Expectation expected)
    {
        Debug.Assert( (expectation & expected) == expected, $"Expected all {expected} but found {expectation}." );
    }

    [Conditional( "DEBUG" )]
    private static void AssumeTokenType(IntermediateToken token, IntermediateTokenType expected)
    {
        Debug.Assert( token.Type == expected, $"Expected token type {expected} but found {token.Type}." );
    }

    [Conditional( "DEBUG" )]
    private static void AssumeNotNullTokenConstructs([NotNull] MathExpressionConstructTokenDefinition? constructs)
    {
        Debug.Assert( constructs is not null, "Expected token constructs to not be null." );
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
