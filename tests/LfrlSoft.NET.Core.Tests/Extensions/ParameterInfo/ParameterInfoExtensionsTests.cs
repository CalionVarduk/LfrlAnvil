﻿using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using LfrlSoft.NET.Core.Extensions;
using LfrlSoft.NET.Core.Functional;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Extensions.ParameterInfo
{
    public class ParameterInfoExtensionsTests : TestsBase
    {
        [Fact]
        public void GetAttribute_ShouldReturnCorrectResult_WhenAttributeExistsAndIsUnique()
        {
            var sut = GetBaseParameter();
            var result = sut.GetAttribute<TestUniqueAttribute>();
            result.Should().BeEquivalentTo( new TestUniqueAttribute( 0 ) );
        }

        [Fact]
        public void GetAttribute_ShouldThrowAmbiguousMatchException_WhenAttributeIsDuplicated()
        {
            var sut = GetBaseParameter();
            var action = Lambda.Of( () => sut.GetAttribute<TestMultiAttribute>() );
            action.Should().ThrowExactly<AmbiguousMatchException>();
        }

        [Fact]
        public void GetAttribute_ShouldReturnNull_WhenAttributeDoesntExists()
        {
            var sut = GetBaseParameter();
            var result = sut.GetAttribute<TestUnusedAttribute>();
            result.Should().BeNull();
        }

        [Theory]
        [InlineData( true )]
        [InlineData( false )]
        public void GetAttribute_ShouldReturnCorrectResult_WhenAttributeExistsAndIsUnique_ForDerivedClass(bool inherit)
        {
            var sut = GetDerivedParameter();
            var result = sut.GetAttribute<TestUniqueAttribute>( inherit );
            result.Should().BeEquivalentTo( new TestUniqueAttribute( 4 ) );
        }

        [Fact]
        public void GetAttribute_ShouldReturnCorrectResult_WhenAttributeIsInherited_ForDerivedClassWithInheritance()
        {
            var sut = GetDerivedParameter();
            var result = sut.GetAttribute<TestBaseOnlyAttribute>( inherit: true );
            result.Should().BeEquivalentTo( new TestBaseOnlyAttribute( 3 ) );
        }

        [Fact]
        public void GetAttribute_ShouldReturnNull_WhenAttributeIsInherited_ForDerivedClassWithoutInheritance()
        {
            var sut = GetDerivedParameter();
            var result = sut.GetAttribute<TestBaseOnlyAttribute>( inherit: false );
            result.Should().BeNull();
        }

        [Fact]
        public void GetAttributeRange_ShouldReturnCorrectResult_WhenAttributeExistsAndIsUnique()
        {
            var sut = GetBaseParameter();
            var expected = new[] { new TestUniqueAttribute( 0 ) }.AsEnumerable();
            var result = sut.GetAttributeRange<TestUniqueAttribute>();
            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void GetAttributeRange_ShouldReturnCorrectResult_WhenAttributeExistsAndIsDuplicable()
        {
            var sut = GetBaseParameter();
            var expected = new[] { new TestMultiAttribute( 1 ), new TestMultiAttribute( 2 ) }.AsEnumerable();
            var result = sut.GetAttributeRange<TestMultiAttribute>();
            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void GetAttributeRange_ShouldReturnEmpty_WhenAttributeDoesntExists()
        {
            var sut = GetBaseParameter();
            var result = sut.GetAttributeRange<TestUnusedAttribute>();
            result.Should().BeEmpty();
        }

        [Theory]
        [InlineData( true )]
        [InlineData( false )]
        public void GetAttributeRange_ShouldReturnCorrectResult_WhenAttributeExistsAndIsUnique_ForDerivedClass(bool inherit)
        {
            var sut = GetDerivedParameter();
            var expected = new[] { new TestUniqueAttribute( 4 ) }.AsEnumerable();
            var result = sut.GetAttributeRange<TestUniqueAttribute>( inherit );
            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void GetAttributeRange_ShouldReturnCorrectResult_WhenAttributeIsInherited_ForDerivedClassWithInheritance()
        {
            var sut = GetDerivedParameter();
            var expected = new[] { new TestBaseOnlyAttribute( 3 ) }.AsEnumerable();
            var result = sut.GetAttributeRange<TestBaseOnlyAttribute>( inherit: true );
            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void GetAttributeRange_ShouldReturnEmpty_WhenAttributeIsInherited_ForDerivedClassWithoutInheritance()
        {
            var sut = GetDerivedParameter();
            var result = sut.GetAttributeRange<TestBaseOnlyAttribute>( inherit: false );
            result.Should().BeEmpty();
        }

        [Fact]
        public void GetAttributeRange_ShouldReturnCorrectResult_WhenAttributeIsDuplicated_ForDerivedClassWithInheritance()
        {
            var sut = GetDerivedParameter();
            var expected = new[]
                {
                    new TestMultiAttribute( 1 ),
                    new TestMultiAttribute( 2 ),
                    new TestMultiAttribute( 5 ),
                    new TestMultiAttribute( 6 )
                }
                .AsEnumerable();

            var result = sut.GetAttributeRange<TestMultiAttribute>( inherit: true );
            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void GetAttributeRange_ShouldReturnCorrectResult_WhenAttributeIsDuplicated_ForDerivedClassWithoutInheritance()
        {
            var sut = GetDerivedParameter();
            var expected = new[] { new TestMultiAttribute( 5 ), new TestMultiAttribute( 6 ) }.AsEnumerable();
            var result = sut.GetAttributeRange<TestMultiAttribute>( inherit: false );
            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void HasAttribute_ShouldReturnTrue_WhenAttributeExists()
        {
            var sut = GetBaseParameter();
            var result = sut.HasAttribute<TestMultiAttribute>();
            result.Should().BeTrue();
        }

        [Fact]
        public void HasAttribute_ShouldReturnFalse_WhenAttributeDoesntExist()
        {
            var sut = GetBaseParameter();
            var result = sut.HasAttribute<TestUnusedAttribute>();
            result.Should().BeFalse();
        }

        [Fact]
        public void HasAttribute_ShouldReturnTrue_WhenAttributeExistsOnBaseType_ForDerivedClassWithInheritance()
        {
            var sut = GetDerivedParameter();
            var result = sut.HasAttribute<TestBaseOnlyAttribute>( inherit: true );
            result.Should().BeTrue();
        }

        [Fact]
        public void HasAttribute_ShouldReturnFalse_WhenAttributeExistsOnBaseType_ForDerivedClassWithoutInheritance()
        {
            var sut = GetDerivedParameter();
            var result = sut.HasAttribute<TestBaseOnlyAttribute>( inherit: false );
            result.Should().BeFalse();
        }

        private static System.Reflection.ParameterInfo GetBaseParameter()
        {
            return typeof( BaseClass ).GetMethod( nameof( BaseClass.TestMethod ) )!.GetParameters()[0];
        }

        private static System.Reflection.ParameterInfo GetDerivedParameter()
        {
            return typeof( DerivedClass ).GetMethod( nameof( BaseClass.TestMethod ) )!.GetParameters()[0];
        }

        [AttributeUsage( AttributeTargets.Parameter )]
        public class TestBaseOnlyAttribute : Attribute
        {
            public TestBaseOnlyAttribute(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }

        [AttributeUsage( AttributeTargets.Parameter )]
        public class TestUniqueAttribute : Attribute
        {
            public TestUniqueAttribute(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }

        [AttributeUsage( AttributeTargets.Parameter, AllowMultiple = true )]
        public class TestMultiAttribute : Attribute
        {
            public TestMultiAttribute(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }

        [AttributeUsage( AttributeTargets.Parameter )]
        public class TestUnusedAttribute : Attribute { }

        public class BaseClass
        {
            public virtual void TestMethod(
                [TestUnique( 0 )] [TestMulti( 1 )] [TestMulti( 2 )] [TestBaseOnly( 3 )]
                int parameter) { }
        }

        public class DerivedClass : BaseClass
        {
            public override void TestMethod(
                [TestUnique( 4 )] [TestMulti( 5 )] [TestMulti( 6 )]
                int parameter) { }
        }
    }
}
