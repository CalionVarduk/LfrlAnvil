using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Computable.Expressions.Errors;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Tests.MathExpressionFactoryTests;

public partial class MathExpressionFactoryTests
{
    private static bool MatchExpectations(
        MathExpressionCreationException exception,
        string input,
        params MathExpressionBuilderErrorType[] types)
    {
        return exception.Errors.Select( e => e.Type ).SequenceEqual( types ) && exception.Input == input;
    }

    private sealed class MockPrefixUnaryOperator : MathExpressionUnaryOperator
    {
        private readonly string _name;

        internal MockPrefixUnaryOperator(string? name = null)
        {
            _name = name is null ? "PreOp" : $"Pre{name}Op";
        }

        protected override Expression CreateUnaryExpression(Expression operand)
        {
            return Handle( operand, _name );
        }

        internal static Expression Handle(Expression operand, string name)
        {
            var concat = MemberInfoLocator.FindStringConcatMethod();
            var toString = MemberInfoLocator.FindToStringWithFormatProviderMethod( operand.Type );
            var formatProvider = Expression.Constant( CultureInfo.InvariantCulture, typeof( IFormatProvider ) );

            var firstCall = Expression.Call(
                null,
                concat,
                Expression.Constant( $"( {name}|" ),
                Expression.Call( operand, toString, formatProvider ) );

            return Expression.Call( null, concat, firstCall, Expression.Constant( " )" ) );
        }
    }

    private sealed class MockPrefixUnaryOperator<TArg> : MathExpressionUnaryOperator<TArg>
    {
        private readonly string _name;

        internal MockPrefixUnaryOperator(string? name = null)
        {
            _name = name is null ? $"Pre{typeof( TArg ).Name}Op" : $"Pre{typeof( TArg ).Name}{name}Op";
        }

        protected override Expression CreateUnaryExpression(Expression operand)
        {
            return MockPrefixUnaryOperator.Handle( operand, _name );
        }
    }

    private sealed class MockPostfixUnaryOperator : MathExpressionUnaryOperator
    {
        private readonly string _name;

        internal MockPostfixUnaryOperator(string? name = null)
        {
            _name = name is null ? "PostOp" : $"Post{name}Op";
        }

        protected override Expression CreateUnaryExpression(Expression operand)
        {
            return Handle( operand, _name );
        }

        internal static Expression Handle(Expression operand, string name)
        {
            var concat = MemberInfoLocator.FindStringConcatMethod();
            var toString = MemberInfoLocator.FindToStringWithFormatProviderMethod( operand.Type );
            var formatProvider = Expression.Constant( CultureInfo.InvariantCulture, typeof( IFormatProvider ) );

            var firstCall = Expression.Call(
                null,
                concat,
                Expression.Constant( "( " ),
                Expression.Call( operand, toString, formatProvider ) );

            return Expression.Call( null, concat, firstCall, Expression.Constant( $"|{name} )" ) );
        }
    }

    private sealed class MockPostfixUnaryOperator<TArg> : MathExpressionUnaryOperator<TArg>
    {
        private readonly string _name;

        internal MockPostfixUnaryOperator(string? name = null)
        {
            _name = name is null ? $"Post{typeof( TArg ).Name}Op" : $"Post{typeof( TArg ).Name}{name}Op";
        }

        protected override Expression CreateUnaryExpression(Expression operand)
        {
            return MockPostfixUnaryOperator.Handle( operand, _name );
        }
    }

    private sealed class MockPrefixTypeConverter : MathExpressionTypeConverter<string>
    {
        protected override Expression CreateConversionExpression(Expression operand)
        {
            return Handle( operand, "PreCast" );
        }

        internal static Expression Handle(Expression operand, string name)
        {
            var concat = MemberInfoLocator.FindStringConcatMethod();
            var toString = MemberInfoLocator.FindToStringWithFormatProviderMethod( operand.Type );
            var formatProvider = Expression.Constant( CultureInfo.InvariantCulture, typeof( IFormatProvider ) );

            var firstCall = Expression.Call(
                null,
                concat,
                Expression.Constant( $"( {name}|" ),
                Expression.Call( operand, toString, formatProvider ) );

            return Expression.Call( null, concat, firstCall, Expression.Constant( " )" ) );
        }
    }

    private sealed class MockPrefixTypeConverter<TSource> : MathExpressionTypeConverter<string, TSource>
    {
        protected override Expression CreateConversionExpression(Expression operand)
        {
            return MockPrefixTypeConverter.Handle( operand, $"Pre{typeof( TSource ).Name}Cast" );
        }
    }

    private sealed class MockPostfixTypeConverter : MathExpressionTypeConverter<string>
    {
        protected override Expression CreateConversionExpression(Expression operand)
        {
            return Handle( operand, "PostCast" );
        }

        internal static Expression Handle(Expression operand, string name)
        {
            var concat = MemberInfoLocator.FindStringConcatMethod();
            var toString = MemberInfoLocator.FindToStringWithFormatProviderMethod( operand.Type );
            var formatProvider = Expression.Constant( CultureInfo.InvariantCulture, typeof( IFormatProvider ) );

            var firstCall = Expression.Call(
                null,
                concat,
                Expression.Constant( "( " ),
                Expression.Call( operand, toString, formatProvider ) );

            return Expression.Call( null, concat, firstCall, Expression.Constant( $"|{name} )" ) );
        }
    }

    private sealed class MockPostfixTypeConverter<TSource> : MathExpressionTypeConverter<string, TSource>
    {
        protected override Expression CreateConversionExpression(Expression operand)
        {
            return MockPostfixTypeConverter.Handle( operand, $"Post{typeof( TSource ).Name}Cast" );
        }
    }

    private sealed class MockBinaryOperator : MathExpressionBinaryOperator
    {
        private readonly string _name;

        internal MockBinaryOperator(string? name = null)
        {
            _name = name is null ? "BiOp" : $"Bi{name}Op";
        }

        protected override Expression CreateBinaryExpression(Expression left, Expression right)
        {
            return Handle( left, right, _name );
        }

        internal static Expression Handle(Expression left, Expression right, string name)
        {
            var concat = MemberInfoLocator.FindStringConcatMethod();
            var leftToString = MemberInfoLocator.FindToStringWithFormatProviderMethod( left.Type );
            var rightToString = MemberInfoLocator.FindToStringWithFormatProviderMethod( right.Type );
            var formatProvider = Expression.Constant( CultureInfo.InvariantCulture, typeof( IFormatProvider ) );

            var firstCall = Expression.Call(
                null,
                concat,
                Expression.Constant( "( " ),
                Expression.Call( left, leftToString, formatProvider ) );

            var secondCall = Expression.Call( null, concat, firstCall, Expression.Constant( $"|{name}|" ) );
            var thirdCall = Expression.Call( null, concat, secondCall, Expression.Call( right, rightToString, formatProvider ) );
            return Expression.Call( null, concat, thirdCall, Expression.Constant( " )" ) );
        }
    }

    private sealed class MockBinaryOperator<TLeftArg, TRightArg> : MathExpressionBinaryOperator<TLeftArg, TRightArg>
    {
        private readonly string _name;

        internal MockBinaryOperator(string? name = null)
        {
            var typeName = $"{typeof( TLeftArg ).Name}{typeof( TRightArg ).Name}";
            _name = name is null ? $"Bi{typeName}Op" : $"Bi{typeName}{name}Op";
        }

        protected override Expression CreateBinaryExpression(Expression left, Expression right)
        {
            return MockBinaryOperator.Handle( left, right, _name );
        }
    }

    private sealed class ThrowingUnaryOperator : MathExpressionUnaryOperator
    {
        protected override Expression CreateUnaryExpression(Expression operand)
        {
            throw new Exception();
        }
    }

    private sealed class ThrowingTypeConverter : MathExpressionTypeConverter<string>
    {
        protected override Expression CreateConversionExpression(Expression operand)
        {
            throw new Exception();
        }
    }

    private sealed class ThrowingBinaryOperator : MathExpressionBinaryOperator
    {
        protected override Expression CreateBinaryExpression(Expression left, Expression right)
        {
            throw new Exception();
        }
    }

    private sealed class FailingNumberParser : IMathExpressionNumberParser
    {
        public bool TryParse(ReadOnlySpan<char> text, [MaybeNullWhen( false )] out object result)
        {
            result = null;
            return false;
        }
    }
}
