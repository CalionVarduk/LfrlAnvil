using Xunit;
using System;
using FluentAssertions;
using LfrlSoft.NET.Common.Extensions;

namespace LfrlSoft.NET.Common.Tests.Extensions.Expression
{
    public class TestClass
    {
        public static string StaticProperty { get; set; }

        public string Property { get; set; }
        public string Field;
        public TestClass Other { get; set; }

        public string Method()
        {
            return Property;
        }
    }

    public class SingleParameterFuncExpressionExtensionsTests
    {
        [Fact]
        public void GetMemberName_ShouldReturnCorrectResult_WhenMemberIsField()
        {
            System.Linq.Expressions.Expression<Func<TestClass, string>> sut = t => t.Field;

            var result = sut.GetMemberName();

            result.Should().Be( nameof( TestClass.Field ) );
        }

        [Fact]
        public void GetMemberName_ShouldReturnCorrectResult_WhenMemberIsProperty()
        {
            System.Linq.Expressions.Expression<Func<TestClass, string>> sut = t => t.Property;

            var result = sut.GetMemberName();

            result.Should().Be( nameof( TestClass.Property ) );
        }

        [Fact]
        public void GetMemberName_ShouldThrowWhenBodyIsMethodCall()
        {
            System.Linq.Expressions.Expression<Func<TestClass, string>> sut = t => t.Method();

            Action action = () => sut.GetMemberName();

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GetMemberName_ShouldThrowWhenBodyIsStaticMember()
        {
            System.Linq.Expressions.Expression<Func<TestClass, string>> sut = _ => TestClass.StaticProperty;

            Action action = () => sut.GetMemberName();

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GetMemberName_ShouldThrowWhenBodyIsStaticMemberFromDifferentType()
        {
            System.Linq.Expressions.Expression<Func<TestClass, string>> sut = _ => string.Empty;

            Action action = () => sut.GetMemberName();

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GetMemberName_ShouldThrowWhenBodyIsAccessingMemberOfMember()
        {
            System.Linq.Expressions.Expression<Func<TestClass, string>> sut = t => t.Other.Property;

            Action action = () => sut.GetMemberName();

            action.Should().Throw<ArgumentException>();
        }
    }
}
