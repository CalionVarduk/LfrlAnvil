using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Computable.Expressions.Errors;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Tests.ParsedExpressionFactoryTests;

public partial class ParsedExpressionFactoryTests
{
    private static bool MatchExpectations(
        ParsedExpressionCreationException exception,
        string input,
        params ParsedExpressionBuilderErrorType[] types)
    {
        return exception.Errors.Select( e => e.Type ).SequenceEqual( types ) && exception.Input == input;
    }

    private sealed class MockPrefixUnaryOperator : ParsedExpressionUnaryOperator
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

    private sealed class MockPrefixUnaryOperator<TArg> : ParsedExpressionUnaryOperator<TArg>
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

    private sealed class MockPostfixUnaryOperator : ParsedExpressionUnaryOperator
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

    private sealed class MockPostfixUnaryOperator<TArg> : ParsedExpressionUnaryOperator<TArg>
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

    private sealed class MockPrefixTypeConverter : ParsedExpressionTypeConverter<string>
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

    private sealed class MockPrefixTypeConverter<TSource> : ParsedExpressionTypeConverter<string, TSource>
    {
        protected override Expression CreateConversionExpression(Expression operand)
        {
            return MockPrefixTypeConverter.Handle( operand, $"Pre{typeof( TSource ).Name}Cast" );
        }
    }

    private sealed class MockPostfixTypeConverter : ParsedExpressionTypeConverter<string>
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

    private sealed class MockPostfixTypeConverter<TSource> : ParsedExpressionTypeConverter<string, TSource>
    {
        protected override Expression CreateConversionExpression(Expression operand)
        {
            return MockPostfixTypeConverter.Handle( operand, $"Post{typeof( TSource ).Name}Cast" );
        }
    }

    private sealed class MockBinaryOperator : ParsedExpressionBinaryOperator
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

    private sealed class MockBinaryOperator<TLeftArg, TRightArg> : ParsedExpressionBinaryOperator<TLeftArg, TRightArg>
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

    private sealed class MockParameterlessFunction : ParsedExpressionFunction<string>
    {
        internal MockParameterlessFunction()
            : base( () => "Func()" ) { }
    }

    private sealed class MockFunctionWithThreeParameters : ParsedExpressionFunction<string, string, string, string>
    {
        internal MockFunctionWithThreeParameters()
            : base( (a, b, c) => $"Func({a},{b},{c})" ) { }
    }

    private sealed class ThrowingUnaryOperator : ParsedExpressionUnaryOperator
    {
        protected override Expression CreateUnaryExpression(Expression operand)
        {
            throw new Exception();
        }
    }

    private sealed class ThrowingTypeConverter : ParsedExpressionTypeConverter<string>
    {
        protected override Expression CreateConversionExpression(Expression operand)
        {
            throw new Exception();
        }
    }

    private sealed class ThrowingBinaryOperator : ParsedExpressionBinaryOperator
    {
        protected override Expression CreateBinaryExpression(Expression left, Expression right)
        {
            throw new Exception();
        }
    }

    private sealed class ZeroConstant : ParsedExpressionConstant<string>
    {
        internal ZeroConstant()
            : base( "ZERO" ) { }
    }

    private sealed class FailingNumberParser : IParsedExpressionNumberParser
    {
        public bool TryParse(ReadOnlySpan<char> text, [MaybeNullWhen( false )] out object result)
        {
            result = null;
            return false;
        }
    }

    private sealed class TestParameter
    {
        private string _setOnly;

        private string _privateField;
        private string PrivateProperty { get; }
        public string PublicField;
        public string PublicProperty { get; }
        public TestParameter? Next { get; }
        public string value;
        public string Value { get; }

        public string SetOnly
        {
            set => _setOnly = value;
        }

        public string PrivateGetterProperty { private get; set; }

        public int this[int i] => i;

        internal TestParameter(string privateField, string privateProperty, string publicField, string publicProperty, TestParameter? next)
        {
            _privateField = privateField;
            PrivateProperty = privateProperty;
            PublicField = publicField;
            PublicProperty = publicProperty;
            value = publicField;
            Value = publicProperty;
            Next = next;
            _setOnly = publicProperty;
            PrivateGetterProperty = privateProperty;
        }
    }
}
