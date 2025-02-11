using System.Linq;
using System.Reflection;
using LfrlAnvil.Extensions;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Tests.ExtensionsTests.MemberInfoTests;

public class MemberInfoExtensionsTests : TestsBase
{
    [Fact]
    public void GetAttribute_ShouldReturnCorrectResult_WhenAttributeExistsAndIsUnique()
    {
        var sut = typeof( BaseClass );
        var result = sut.GetAttribute<TestUniqueAttribute>();
        result.TestEquals( new TestUniqueAttribute( 0 ) ).Go();
    }

    [Fact]
    public void GetAttribute_ShouldThrowAmbiguousMatchException_WhenAttributeIsDuplicated()
    {
        var sut = typeof( BaseClass );
        var action = Lambda.Of( () => sut.GetAttribute<TestMultiAttribute>() );
        action.Test( exc => exc.TestType().Exact<AmbiguousMatchException>() ).Go();
    }

    [Fact]
    public void GetAttribute_ShouldReturnNull_WhenAttributeDoesntExists()
    {
        var sut = typeof( BaseClass );
        var result = sut.GetAttribute<TestUnusedAttribute>();
        result.TestNull().Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void GetAttribute_ShouldReturnCorrectResult_WhenAttributeExistsAndIsUnique_ForDerivedClass(bool inherit)
    {
        var sut = typeof( DerivedClass );
        var result = sut.GetAttribute<TestUniqueAttribute>( inherit );
        result.TestEquals( new TestUniqueAttribute( 4 ) ).Go();
    }

    [Fact]
    public void GetAttribute_ShouldReturnCorrectResult_WhenAttributeIsInherited_ForDerivedClassWithInheritance()
    {
        var sut = typeof( DerivedClass );
        var result = sut.GetAttribute<TestBaseOnlyAttribute>( inherit: true );
        result.TestEquals( new TestBaseOnlyAttribute( 3 ) ).Go();
    }

    [Fact]
    public void GetAttribute_ShouldReturnNull_WhenAttributeIsInherited_ForDerivedClassWithoutInheritance()
    {
        var sut = typeof( DerivedClass );
        var result = sut.GetAttribute<TestBaseOnlyAttribute>( inherit: false );
        result.TestNull().Go();
    }

    [Fact]
    public void GetAttributeRange_ShouldReturnCorrectResult_WhenAttributeExistsAndIsUnique()
    {
        var sut = typeof( BaseClass );
        var expected = new[] { new TestUniqueAttribute( 0 ) }.AsEnumerable();
        var result = sut.GetAttributeRange<TestUniqueAttribute>();
        result.TestSetEqual( expected ).Go();
    }

    [Fact]
    public void GetAttributeRange_ShouldReturnCorrectResult_WhenAttributeExistsAndIsDuplicable()
    {
        var sut = typeof( BaseClass );
        var expected = new[] { new TestMultiAttribute( 1 ), new TestMultiAttribute( 2 ) }.AsEnumerable();
        var result = sut.GetAttributeRange<TestMultiAttribute>();
        result.TestSetEqual( expected ).Go();
    }

    [Fact]
    public void GetAttributeRange_ShouldReturnEmpty_WhenAttributeDoesntExists()
    {
        var sut = typeof( BaseClass );
        var result = sut.GetAttributeRange<TestUnusedAttribute>();
        result.TestEmpty().Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void GetAttributeRange_ShouldReturnCorrectResult_WhenAttributeExistsAndIsUnique_ForDerivedClass(bool inherit)
    {
        var sut = typeof( DerivedClass );
        var expected = new[] { new TestUniqueAttribute( 4 ) }.AsEnumerable();
        var result = sut.GetAttributeRange<TestUniqueAttribute>( inherit );
        result.TestSetEqual( expected ).Go();
    }

    [Fact]
    public void GetAttributeRange_ShouldReturnCorrectResult_WhenAttributeIsInherited_ForDerivedClassWithInheritance()
    {
        var sut = typeof( DerivedClass );
        var expected = new[] { new TestBaseOnlyAttribute( 3 ) }.AsEnumerable();
        var result = sut.GetAttributeRange<TestBaseOnlyAttribute>( inherit: true );
        result.TestSetEqual( expected ).Go();
    }

    [Fact]
    public void GetAttributeRange_ShouldReturnEmpty_WhenAttributeIsInherited_ForDerivedClassWithoutInheritance()
    {
        var sut = typeof( DerivedClass );
        var result = sut.GetAttributeRange<TestBaseOnlyAttribute>( inherit: false );
        result.TestEmpty().Go();
    }

    [Fact]
    public void GetAttributeRange_ShouldReturnCorrectResult_WhenAttributeIsDuplicated_ForDerivedClassWithInheritance()
    {
        var sut = typeof( DerivedClass );
        var expected = new[]
            {
                new TestMultiAttribute( 1 ), new TestMultiAttribute( 2 ), new TestMultiAttribute( 5 ), new TestMultiAttribute( 6 )
            }
            .AsEnumerable();

        var result = sut.GetAttributeRange<TestMultiAttribute>( inherit: true );
        result.TestSetEqual( expected ).Go();
    }

    [Fact]
    public void GetAttributeRange_ShouldReturnCorrectResult_WhenAttributeIsDuplicated_ForDerivedClassWithoutInheritance()
    {
        var sut = typeof( DerivedClass );
        var expected = new[] { new TestMultiAttribute( 5 ), new TestMultiAttribute( 6 ) }.AsEnumerable();
        var result = sut.GetAttributeRange<TestMultiAttribute>( inherit: false );
        result.TestSetEqual( expected ).Go();
    }

    [Fact]
    public void HasAttribute_ShouldReturnTrue_WhenAttributeExists()
    {
        var sut = typeof( BaseClass );
        var result = sut.HasAttribute<TestMultiAttribute>();
        result.TestTrue().Go();
    }

    [Fact]
    public void HasAttribute_ShouldReturnFalse_WhenAttributeDoesntExist()
    {
        var sut = typeof( BaseClass );
        var result = sut.HasAttribute<TestUnusedAttribute>();
        result.TestFalse().Go();
    }

    [Fact]
    public void HasAttribute_ShouldReturnTrue_WhenAttributeExistsOnBaseType_ForDerivedClassWithInheritance()
    {
        var sut = typeof( DerivedClass );
        var result = sut.HasAttribute<TestBaseOnlyAttribute>( inherit: true );
        result.TestTrue().Go();
    }

    [Fact]
    public void HasAttribute_ShouldReturnFalse_WhenAttributeExistsOnBaseType_ForDerivedClassWithoutInheritance()
    {
        var sut = typeof( DerivedClass );
        var result = sut.HasAttribute<TestBaseOnlyAttribute>( inherit: false );
        result.TestFalse().Go();
    }

    [Fact]
    public void TryAsEvent_ShouldReturnEventInfo_WhenMemberIsEvent()
    {
        var member = typeof( DerivedClass ).GetMember( nameof( DerivedClass.TestEvent ) )[0];
        var result = member.TryAsEvent();
        result.TestEquals( member ).Go();
    }

    [Fact]
    public void TryAsEvent_ShouldReturnNull_WhenMemberIsNull()
    {
        MemberInfo? member = null;
        var result = member!.TryAsEvent();
        result.TestNull().Go();
    }

    [Theory]
    [InlineData( nameof( DerivedClass.TestField ) )]
    [InlineData( nameof( DerivedClass.TestProperty ) )]
    [InlineData( nameof( DerivedClass.TestMethod ) )]
    [InlineData( nameof( DerivedClass.TestType ) )]
    [InlineData( ".ctor" )]
    public void TryAsEvent_ShouldReturnNull_WhenMemberIsNotEvent(string memberName)
    {
        var member = typeof( DerivedClass ).GetMember( memberName )[0];
        var result = member.TryAsEvent();
        result.TestNull().Go();
    }

    [Fact]
    public void TryAsField_ShouldReturnFieldInfo_WhenMemberIsField()
    {
        var member = typeof( DerivedClass ).GetMember( nameof( DerivedClass.TestField ) )[0];
        var result = member.TryAsField();
        result.TestEquals( member ).Go();
    }

    [Fact]
    public void TryAsField_ShouldReturnNull_WhenMemberIsNull()
    {
        MemberInfo? member = null;
        var result = member!.TryAsField();
        result.TestNull().Go();
    }

    [Theory]
    [InlineData( nameof( DerivedClass.TestEvent ) )]
    [InlineData( nameof( DerivedClass.TestProperty ) )]
    [InlineData( nameof( DerivedClass.TestMethod ) )]
    [InlineData( nameof( DerivedClass.TestType ) )]
    [InlineData( ".ctor" )]
    public void TryAsField_ShouldReturnNull_WhenMemberIsNotField(string memberName)
    {
        var member = typeof( DerivedClass ).GetMember( memberName )[0];
        var result = member.TryAsField();
        result.TestNull().Go();
    }

    [Fact]
    public void TryAsProperty_ShouldReturnPropertyInfo_WhenMemberIsProperty()
    {
        var member = typeof( DerivedClass ).GetMember( nameof( DerivedClass.TestProperty ) )[0];
        var result = member.TryAsProperty();
        result.TestEquals( member ).Go();
    }

    [Fact]
    public void TryAsProperty_ShouldReturnNull_WhenMemberIsNull()
    {
        MemberInfo? member = null;
        var result = member!.TryAsProperty();
        result.TestNull().Go();
    }

    [Theory]
    [InlineData( nameof( DerivedClass.TestEvent ) )]
    [InlineData( nameof( DerivedClass.TestField ) )]
    [InlineData( nameof( DerivedClass.TestMethod ) )]
    [InlineData( nameof( DerivedClass.TestType ) )]
    [InlineData( ".ctor" )]
    public void TryAsProperty_ShouldReturnNull_WhenMemberIsNotProperty(string memberName)
    {
        var member = typeof( DerivedClass ).GetMember( memberName )[0];
        var result = member.TryAsProperty();
        result.TestNull().Go();
    }

    [Fact]
    public void TryAsType_ShouldReturnType_WhenMemberIsType()
    {
        var member = typeof( DerivedClass ).GetMember( nameof( DerivedClass.TestType ) )[0];
        var result = member.TryAsType();
        result.TestEquals( member ).Go();
    }

    [Fact]
    public void TryAsType_ShouldReturnNull_WhenMemberIsNull()
    {
        MemberInfo? member = null;
        var result = member!.TryAsType();
        result.TestNull().Go();
    }

    [Theory]
    [InlineData( nameof( DerivedClass.TestEvent ) )]
    [InlineData( nameof( DerivedClass.TestField ) )]
    [InlineData( nameof( DerivedClass.TestMethod ) )]
    [InlineData( nameof( DerivedClass.TestProperty ) )]
    [InlineData( ".ctor" )]
    public void TryAsType_ShouldReturnNull_WhenMemberIsNotType(string memberName)
    {
        var member = typeof( DerivedClass ).GetMember( memberName )[0];
        var result = member.TryAsType();
        result.TestNull().Go();
    }

    [Fact]
    public void TryAsConstructor_ShouldReturnConstructorInfo_WhenMemberIsConstructor()
    {
        var member = typeof( DerivedClass ).GetMember( ".ctor" )[0];
        var result = member.TryAsConstructor();
        result.TestEquals( member ).Go();
    }

    [Fact]
    public void TryAsConstructor_ShouldReturnNull_WhenMemberIsNull()
    {
        MemberInfo? member = null;
        var result = member!.TryAsConstructor();
        result.TestNull().Go();
    }

    [Theory]
    [InlineData( nameof( DerivedClass.TestEvent ) )]
    [InlineData( nameof( DerivedClass.TestField ) )]
    [InlineData( nameof( DerivedClass.TestMethod ) )]
    [InlineData( nameof( DerivedClass.TestProperty ) )]
    [InlineData( nameof( DerivedClass.TestType ) )]
    public void TryAsConstructor_ShouldReturnNull_WhenMemberIsNotConstructor(string memberName)
    {
        var member = typeof( DerivedClass ).GetMember( memberName )[0];
        var result = member.TryAsConstructor();
        result.TestNull().Go();
    }

    [Fact]
    public void TryAsMethod_ShouldReturnMethodInfo_WhenMemberIsMethod()
    {
        var member = typeof( DerivedClass ).GetMember( nameof( DerivedClass.TestMethod ) )[0];
        var result = member.TryAsMethod();
        result.TestEquals( member ).Go();
    }

    [Fact]
    public void TryAsMethod_ShouldReturnNull_WhenMemberIsNull()
    {
        MemberInfo? member = null;
        var result = member!.TryAsMethod();
        result.TestNull().Go();
    }

    [Theory]
    [InlineData( nameof( DerivedClass.TestEvent ) )]
    [InlineData( nameof( DerivedClass.TestField ) )]
    [InlineData( nameof( DerivedClass.TestProperty ) )]
    [InlineData( nameof( DerivedClass.TestType ) )]
    [InlineData( ".ctor" )]
    public void TryAsMethod_ShouldReturnNull_WhenMemberIsNotMethod(string memberName)
    {
        var member = typeof( DerivedClass ).GetMember( memberName )[0];
        var result = member.TryAsMethod();
        result.TestNull().Go();
    }

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
}
