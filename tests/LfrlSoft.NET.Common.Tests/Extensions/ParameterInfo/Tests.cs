using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using LfrlSoft.NET.Common.Extensions;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Common.Tests.Extensions.ParameterInfo
{
    public class Tests : TestsBase
    {
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

        [Fact]
        public void GetAttribute_ShouldReturnCorrectResultWhenAttributeExistsAndIsUnique()
        {
            var sut = GetBaseParameter();
            var result = sut.GetAttribute<TestUniqueAttribute>();
            result.Should().BeEquivalentTo( new TestUniqueAttribute( 0 ) );
        }

        [Fact]
        public void GetAttribute_ShouldThrowWhenAttributeIsDuplicated()
        {
            var sut = GetBaseParameter();
            Action action = () => sut.GetAttribute<TestMultiAttribute>();
            action.Should().Throw<AmbiguousMatchException>();
        }

        [Fact]
        public void GetAttribute_ShouldReturnNullWhenAttributeDoesntExists()
        {
            var sut = GetBaseParameter();
            var result = sut.GetAttribute<TestUnusedAttribute>();
            result.Should().BeNull();
        }

        [Theory]
        [InlineData( true )]
        [InlineData( false )]
        public void GetAttribute_ShouldReturnCorrectResultWhenAttributeExistsAndIsUnique_ForDerivedClass(bool inherit)
        {
            var sut = GetDerivedParameter();
            var result = sut.GetAttribute<TestUniqueAttribute>( inherit );
            result.Should().BeEquivalentTo( new TestUniqueAttribute( 4 ) );
        }

        [Fact]
        public void GetAttribute_ShouldReturnCorrectResultWhenAttributeIsInherited_ForDerivedClassWithInheritance()
        {
            var sut = GetDerivedParameter();
            var result = sut.GetAttribute<TestBaseOnlyAttribute>( inherit: true );
            result.Should().BeEquivalentTo( new TestBaseOnlyAttribute( 3 ) );
        }

        [Fact]
        public void GetAttribute_ShouldReturnNullWhenAttributeIsInherited_ForDerivedClassWithoutInheritance()
        {
            var sut = GetDerivedParameter();
            var result = sut.GetAttribute<TestBaseOnlyAttribute>( inherit: false );
            result.Should().BeNull();
        }

        [Fact]
        public void GetAttributeRange_ShouldReturnCorrectResultWhenAttributeExistsAndIsUnique()
        {
            var sut = GetBaseParameter();
            var expected = new[] { new TestUniqueAttribute( 0 ) }.AsEnumerable();
            var result = sut.GetAttributeRange<TestUniqueAttribute>();
            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void GetAttributeRange_ShouldReturnCorrectResultWhenAttributeExistsAndIsDuplicable()
        {
            var sut = GetBaseParameter();
            var expected = new[] { new TestMultiAttribute( 1 ), new TestMultiAttribute( 2 ) }.AsEnumerable();
            var result = sut.GetAttributeRange<TestMultiAttribute>();
            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void GetAttributeRange_ShouldReturnEmptyWhenAttributeDoesntExists()
        {
            var sut = GetBaseParameter();
            var result = sut.GetAttributeRange<TestUnusedAttribute>();
            result.Should().BeEmpty();
        }

        [Theory]
        [InlineData( true )]
        [InlineData( false )]
        public void GetAttributeRange_ShouldReturnCorrectResultWhenAttributeExistsAndIsUnique_ForDerivedClass(bool inherit)
        {
            var sut = GetDerivedParameter();
            var expected = new[] { new TestUniqueAttribute( 4 ) }.AsEnumerable();
            var result = sut.GetAttributeRange<TestUniqueAttribute>( inherit );
            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void GetAttributeRange_ShouldReturnCorrectResultWhenAttributeIsInherited_ForDerivedClassWithInheritance()
        {
            var sut = GetDerivedParameter();
            var expected = new[] { new TestBaseOnlyAttribute( 3 ) }.AsEnumerable();
            var result = sut.GetAttributeRange<TestBaseOnlyAttribute>( inherit: true );
            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void GetAttributeRange_ShouldReturnEmptyWhenAttributeIsInherited_ForDerivedClassWithoutInheritance()
        {
            var sut = GetDerivedParameter();
            var result = sut.GetAttributeRange<TestBaseOnlyAttribute>( inherit: false );
            result.Should().BeEmpty();
        }

        [Fact]
        public void GetAttributeRange_ShouldReturnCorrectResultWhenAttributeIsDuplicated_ForDerivedClassWithInheritance()
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
        public void GetAttributeRange_ShouldReturnCorrectResultWhenAttributeIsDuplicated_ForDerivedClassWithoutInheritance()
        {
            var sut = GetDerivedParameter();
            var expected = new[] { new TestMultiAttribute( 5 ), new TestMultiAttribute( 6 ) }.AsEnumerable();
            var result = sut.GetAttributeRange<TestMultiAttribute>( inherit: false );
            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void HasAttribute_ShouldReturnTrueWhenAttributeExists()
        {
            var sut = GetBaseParameter();
            var result = sut.HasAttribute<TestMultiAttribute>();
            result.Should().BeTrue();
        }

        [Fact]
        public void HasAttribute_ShouldReturnFalseWhenAttributeDoesntExist()
        {
            var sut = GetBaseParameter();
            var result = sut.HasAttribute<TestUnusedAttribute>();
            result.Should().BeFalse();
        }

        [Fact]
        public void HasAttribute_ShouldReturnTrueWhenAttributeExistsOnBaseType_ForDerivedClassWithInheritance()
        {
            var sut = GetDerivedParameter();
            var result = sut.HasAttribute<TestBaseOnlyAttribute>( inherit: true );
            result.Should().BeTrue();
        }

        [Fact]
        public void HasAttribute_ShouldReturnFalseWhenAttributeExistsOnBaseType_ForDerivedClassWithoutInheritance()
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
    }
}
