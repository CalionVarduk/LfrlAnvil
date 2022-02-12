using System;
using System.Linq.Expressions;
using FluentAssertions;
using LfrlSoft.NET.Core.Extensions;
using LfrlSoft.NET.Core.Functional;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.ExtensionsTests.ExpressionTests
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
        public void GetMemberName_ShouldThrowArgumentException_WhenBodyIsMethodCall()
        {
            Expression<Func<TestClass, string?>> sut = t => t.Method();
            var action = Lambda.Of( () => sut.GetMemberName() );
            action.Should().ThrowExactly<ArgumentException>();
        }

        [Fact]
        public void GetMemberName_ShouldThrowArgumentException_WhenBodyIsStaticMember()
        {
            Expression<Func<TestClass, string?>> sut = _ => TestClass.StaticProperty;
            var action = Lambda.Of( () => sut.GetMemberName() );
            action.Should().ThrowExactly<ArgumentException>();
        }

        [Fact]
        public void GetMemberName_ShouldThrowArgumentException_WhenBodyIsStaticMemberFromDifferentType()
        {
            Expression<Func<TestClass, string>> sut = _ => string.Empty;
            var action = Lambda.Of( () => sut.GetMemberName() );
            action.Should().ThrowExactly<ArgumentException>();
        }

        [Fact]
        public void GetMemberName_ShouldThrowArgumentException_WhenBodyIsAccessingMemberOfMember()
        {
            Expression<Func<TestClass, string?>> sut = t => t.Other!.Property;
            var action = Lambda.Of( () => sut.GetMemberName() );
            action.Should().ThrowExactly<ArgumentException>();
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
