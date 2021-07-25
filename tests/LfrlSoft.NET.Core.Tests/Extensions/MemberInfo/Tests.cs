using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using LfrlSoft.NET.Core.Extensions;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Extensions.MemberInfo
{
    public class Tests : TestsBase
    {
        [AttributeUsage( AttributeTargets.Class )]
        public class TestBaseOnlyAttribute : Attribute
        {
            public TestBaseOnlyAttribute(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }

        [AttributeUsage( AttributeTargets.Class )]
        public class TestUniqueAttribute : Attribute
        {
            public TestUniqueAttribute(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }

        [AttributeUsage( AttributeTargets.Class, AllowMultiple = true )]
        public class TestMultiAttribute : Attribute
        {
            public TestMultiAttribute(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }

        [AttributeUsage( AttributeTargets.Class )]
        public class TestUnusedAttribute : Attribute { }

        [TestUnique( 0 )]
        [TestMulti( 1 )]
        [TestMulti( 2 )]
        [TestBaseOnly( 3 )]
        public class BaseClass { }

        [TestUnique( 4 )]
        [TestMulti( 5 )]
        [TestMulti( 6 )]
        public class DerivedClass : BaseClass
        {
            public event EventHandler? TestEvent;
            public int TestField;
            public int TestProperty { get; }
            public void TestMethod() { }

            public class TestType { }
        }

        [Fact]
        public void GetAttribute_ShouldReturnCorrectResultWhenAttributeExistsAndIsUnique()
        {
            var sut = typeof( BaseClass );
            var result = sut.GetAttribute<TestUniqueAttribute>();
            result.Should().BeEquivalentTo( new TestUniqueAttribute( 0 ) );
        }

        [Fact]
        public void GetAttribute_ShouldThrowWhenAttributeIsDuplicated()
        {
            var sut = typeof( BaseClass );
            Action action = () => sut.GetAttribute<TestMultiAttribute>();
            action.Should().Throw<AmbiguousMatchException>();
        }

        [Fact]
        public void GetAttribute_ShouldReturnNullWhenAttributeDoesntExists()
        {
            var sut = typeof( BaseClass );
            var result = sut.GetAttribute<TestUnusedAttribute>();
            result.Should().BeNull();
        }

        [Theory]
        [InlineData( true )]
        [InlineData( false )]
        public void GetAttribute_ShouldReturnCorrectResultWhenAttributeExistsAndIsUnique_ForDerivedClass(bool inherit)
        {
            var sut = typeof( DerivedClass );
            var result = sut.GetAttribute<TestUniqueAttribute>( inherit );
            result.Should().BeEquivalentTo( new TestUniqueAttribute( 4 ) );
        }

        [Fact]
        public void GetAttribute_ShouldReturnCorrectResultWhenAttributeIsInherited_ForDerivedClassWithInheritance()
        {
            var sut = typeof( DerivedClass );
            var result = sut.GetAttribute<TestBaseOnlyAttribute>( inherit: true );
            result.Should().BeEquivalentTo( new TestBaseOnlyAttribute( 3 ) );
        }

        [Fact]
        public void GetAttribute_ShouldReturnNullWhenAttributeIsInherited_ForDerivedClassWithoutInheritance()
        {
            var sut = typeof( DerivedClass );
            var result = sut.GetAttribute<TestBaseOnlyAttribute>( inherit: false );
            result.Should().BeNull();
        }

        [Fact]
        public void GetAttributeRange_ShouldReturnCorrectResultWhenAttributeExistsAndIsUnique()
        {
            var sut = typeof( BaseClass );
            var expected = new[] { new TestUniqueAttribute( 0 ) }.AsEnumerable();
            var result = sut.GetAttributeRange<TestUniqueAttribute>();
            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void GetAttributeRange_ShouldReturnCorrectResultWhenAttributeExistsAndIsDuplicable()
        {
            var sut = typeof( BaseClass );
            var expected = new[] { new TestMultiAttribute( 1 ), new TestMultiAttribute( 2 ) }.AsEnumerable();
            var result = sut.GetAttributeRange<TestMultiAttribute>();
            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void GetAttributeRange_ShouldReturnEmptyWhenAttributeDoesntExists()
        {
            var sut = typeof( BaseClass );
            var result = sut.GetAttributeRange<TestUnusedAttribute>();
            result.Should().BeEmpty();
        }

        [Theory]
        [InlineData( true )]
        [InlineData( false )]
        public void GetAttributeRange_ShouldReturnCorrectResultWhenAttributeExistsAndIsUnique_ForDerivedClass(bool inherit)
        {
            var sut = typeof( DerivedClass );
            var expected = new[] { new TestUniqueAttribute( 4 ) }.AsEnumerable();
            var result = sut.GetAttributeRange<TestUniqueAttribute>( inherit );
            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void GetAttributeRange_ShouldReturnCorrectResultWhenAttributeIsInherited_ForDerivedClassWithInheritance()
        {
            var sut = typeof( DerivedClass );
            var expected = new[] { new TestBaseOnlyAttribute( 3 ) }.AsEnumerable();
            var result = sut.GetAttributeRange<TestBaseOnlyAttribute>( inherit: true );
            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void GetAttributeRange_ShouldReturnEmptyWhenAttributeIsInherited_ForDerivedClassWithoutInheritance()
        {
            var sut = typeof( DerivedClass );
            var result = sut.GetAttributeRange<TestBaseOnlyAttribute>( inherit: false );
            result.Should().BeEmpty();
        }

        [Fact]
        public void GetAttributeRange_ShouldReturnCorrectResultWhenAttributeIsDuplicated_ForDerivedClassWithInheritance()
        {
            var sut = typeof( DerivedClass );
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
            var sut = typeof( DerivedClass );
            var expected = new[] { new TestMultiAttribute( 5 ), new TestMultiAttribute( 6 ) }.AsEnumerable();
            var result = sut.GetAttributeRange<TestMultiAttribute>( inherit: false );
            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void HasAttribute_ShouldReturnTrueWhenAttributeExists()
        {
            var sut = typeof( BaseClass );
            var result = sut.HasAttribute<TestMultiAttribute>();
            result.Should().BeTrue();
        }

        [Fact]
        public void HasAttribute_ShouldReturnFalseWhenAttributeDoesntExist()
        {
            var sut = typeof( BaseClass );
            var result = sut.HasAttribute<TestUnusedAttribute>();
            result.Should().BeFalse();
        }

        [Fact]
        public void HasAttribute_ShouldReturnTrueWhenAttributeExistsOnBaseType_ForDerivedClassWithInheritance()
        {
            var sut = typeof( DerivedClass );
            var result = sut.HasAttribute<TestBaseOnlyAttribute>( inherit: true );
            result.Should().BeTrue();
        }

        [Fact]
        public void HasAttribute_ShouldReturnFalseWhenAttributeExistsOnBaseType_ForDerivedClassWithoutInheritance()
        {
            var sut = typeof( DerivedClass );
            var result = sut.HasAttribute<TestBaseOnlyAttribute>( inherit: false );
            result.Should().BeFalse();
        }

        [Fact]
        public void TryAsEvent_ShouldReturnEventInfoWhenMemberIsEvent()
        {
            var member = typeof( DerivedClass ).GetMember( nameof( DerivedClass.TestEvent ) )[0];
            var result = member.TryAsEvent();
            result.Should().Be( member );
        }

        [Fact]
        public void TryAsEvent_ShouldReturnNullWhenMemberIsNull()
        {
            System.Reflection.MemberInfo? member = null;
            var result = member!.TryAsEvent();
            result.Should().BeNull();
        }

        [Theory]
        [InlineData( nameof( DerivedClass.TestField ) )]
        [InlineData( nameof( DerivedClass.TestProperty ) )]
        [InlineData( nameof( DerivedClass.TestMethod ) )]
        [InlineData( nameof( DerivedClass.TestType ) )]
        [InlineData( ".ctor" )]
        public void TryAsEvent_ShouldReturnNullWhenMemberIsNotEvent(string memberName)
        {
            var member = typeof( DerivedClass ).GetMember( memberName )[0];
            var result = member.TryAsEvent();
            result.Should().BeNull();
        }

        [Fact]
        public void TryAsField_ShouldReturnFieldInfoWhenMemberIsField()
        {
            var member = typeof( DerivedClass ).GetMember( nameof( DerivedClass.TestField ) )[0];
            var result = member.TryAsField();
            result.Should().Be( member );
        }

        [Fact]
        public void TryAsField_ShouldReturnNullWhenMemberIsNull()
        {
            System.Reflection.MemberInfo? member = null;
            var result = member!.TryAsField();
            result.Should().BeNull();
        }

        [Theory]
        [InlineData( nameof( DerivedClass.TestEvent ) )]
        [InlineData( nameof( DerivedClass.TestProperty ) )]
        [InlineData( nameof( DerivedClass.TestMethod ) )]
        [InlineData( nameof( DerivedClass.TestType ) )]
        [InlineData( ".ctor" )]
        public void TryAsField_ShouldReturnNullWhenMemberIsNotField(string memberName)
        {
            var member = typeof( DerivedClass ).GetMember( memberName )[0];
            var result = member.TryAsField();
            result.Should().BeNull();
        }

        [Fact]
        public void TryAsProperty_ShouldReturnPropertyInfoWhenMemberIsProperty()
        {
            var member = typeof( DerivedClass ).GetMember( nameof( DerivedClass.TestProperty ) )[0];
            var result = member.TryAsProperty();
            (result as object).Should().Be( member );
        }

        [Fact]
        public void TryAsProperty_ShouldReturnNullWhenMemberIsNull()
        {
            System.Reflection.MemberInfo? member = null;
            var result = member!.TryAsProperty();
            result.Should().BeNull();
        }

        [Theory]
        [InlineData( nameof( DerivedClass.TestEvent ) )]
        [InlineData( nameof( DerivedClass.TestField ) )]
        [InlineData( nameof( DerivedClass.TestMethod ) )]
        [InlineData( nameof( DerivedClass.TestType ) )]
        [InlineData( ".ctor" )]
        public void TryAsProperty_ShouldReturnNullWhenMemberIsNotProperty(string memberName)
        {
            var member = typeof( DerivedClass ).GetMember( memberName )[0];
            var result = member.TryAsProperty();
            result.Should().BeNull();
        }

        [Fact]
        public void TryAsType_ShouldReturnTypeWhenMemberIsType()
        {
            var member = typeof( DerivedClass ).GetMember( nameof( DerivedClass.TestType ) )[0];
            var result = member.TryAsType();
            (result as object).Should().Be( member );
        }

        [Fact]
        public void TryAsType_ShouldReturnNullWhenMemberIsNull()
        {
            System.Reflection.MemberInfo? member = null;
            var result = member!.TryAsType();
            result.Should().BeNull();
        }

        [Theory]
        [InlineData( nameof( DerivedClass.TestEvent ) )]
        [InlineData( nameof( DerivedClass.TestField ) )]
        [InlineData( nameof( DerivedClass.TestMethod ) )]
        [InlineData( nameof( DerivedClass.TestProperty ) )]
        [InlineData( ".ctor" )]
        public void TryAsType_ShouldReturnNullWhenMemberIsNotType(string memberName)
        {
            var member = typeof( DerivedClass ).GetMember( memberName )[0];
            var result = member.TryAsType();
            result.Should().BeNull();
        }

        [Fact]
        public void TryAsConstructor_ShouldReturnConstructorInfoWhenMemberIsConstructor()
        {
            var member = typeof( DerivedClass ).GetMember( ".ctor" )[0];
            var result = member.TryAsConstructor();
            (result as object).Should().Be( member );
        }

        [Fact]
        public void TryAsConstructor_ShouldReturnNullWhenMemberIsNull()
        {
            System.Reflection.MemberInfo? member = null;
            var result = member!.TryAsConstructor();
            result.Should().BeNull();
        }

        [Theory]
        [InlineData( nameof( DerivedClass.TestEvent ) )]
        [InlineData( nameof( DerivedClass.TestField ) )]
        [InlineData( nameof( DerivedClass.TestMethod ) )]
        [InlineData( nameof( DerivedClass.TestProperty ) )]
        [InlineData( nameof( DerivedClass.TestType ) )]
        public void TryAsConstructor_ShouldReturnNullWhenMemberIsNotConstructor(string memberName)
        {
            var member = typeof( DerivedClass ).GetMember( memberName )[0];
            var result = member.TryAsConstructor();
            result.Should().BeNull();
        }

        [Fact]
        public void TryAsMethod_ShouldReturnMethodInfoWhenMemberIsMethod()
        {
            var member = typeof( DerivedClass ).GetMember( nameof( DerivedClass.TestMethod ) )[0];
            var result = member.TryAsMethod();
            (result as object).Should().Be( member );
        }

        [Fact]
        public void TryAsMethod_ShouldReturnNullWhenMemberIsNull()
        {
            System.Reflection.MemberInfo? member = null;
            var result = member!.TryAsMethod();
            result.Should().BeNull();
        }

        [Theory]
        [InlineData( nameof( DerivedClass.TestEvent ) )]
        [InlineData( nameof( DerivedClass.TestField ) )]
        [InlineData( nameof( DerivedClass.TestProperty ) )]
        [InlineData( nameof( DerivedClass.TestType ) )]
        [InlineData( ".ctor" )]
        public void TryAsMethod_ShouldReturnNullWhenMemberIsNotMethod(string memberName)
        {
            var member = typeof( DerivedClass ).GetMember( memberName )[0];
            var result = member.TryAsMethod();
            result.Should().BeNull();
        }
    }
}
