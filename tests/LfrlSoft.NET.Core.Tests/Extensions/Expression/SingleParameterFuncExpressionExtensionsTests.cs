using System;
using System.Linq.Expressions;
using FluentAssertions;
using LfrlSoft.NET.Core.Extensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Extensions.Expression
{
    public class SingleParameterFuncExpressionExtensionsTests
    {
        [Fact]
        public void GetMemberName_ShouldReturnCorrectResult_WhenMemberIsField()
        {
            Expression<Func<TestClass, string?>> sut = t => t.Field;

            var result = sut.GetMemberName();

            result.Should().Be( nameof( TestClass.Field ) );
        }

        [Fact]
        public void GetMemberName_ShouldReturnCorrectResult_WhenMemberIsProperty()
        {
            Expression<Func<TestClass, string?>> sut = t => t.Property;

            var result = sut.GetMemberName();

            result.Should().Be( nameof( TestClass.Property ) );
        }

        [Fact]
        public void GetMemberName_ShouldThrowWhenBodyIsMethodCall()
        {
            Expression<Func<TestClass, string?>> sut = t => t.Method();

            System.Action action = () =>
            {
                var _ = sut.GetMemberName();
            };

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GetMemberName_ShouldThrowWhenBodyIsStaticMember()
        {
            Expression<Func<TestClass, string?>> sut = _ => TestClass.StaticProperty;

            System.Action action = () =>
            {
                var _ = sut.GetMemberName();
            };

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GetMemberName_ShouldThrowWhenBodyIsStaticMemberFromDifferentType()
        {
            Expression<Func<TestClass, string>> sut = _ => string.Empty;

            System.Action action = () =>
            {
                var _ = sut.GetMemberName();
            };

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GetMemberName_ShouldThrowWhenBodyIsAccessingMemberOfMember()
        {
            Expression<Func<TestClass, string?>> sut = t => t.Other!.Property;

            System.Action action = () =>
            {
                var _ = sut.GetMemberName();
            };

            action.Should().Throw<ArgumentException>();
        }
    }

    public class TestClass
    {
        public static string? StaticProperty { get; set; }

        public string? Property { get; set; }
        public string? Field;
        public TestClass? Other { get; set; }

        public string? Method()
        {
            return Property;
        }
    }
}
