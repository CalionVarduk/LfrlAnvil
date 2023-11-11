using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Tests.ExtensionsTests.TypeTests;

public class NonGenericClassTests : TypeTestsBase
{
    [Fact]
    public void GetImplementation_ShouldReturnCorrectResult_WhenInterfaceIsDirectlyImplemented()
    {
        var sut = typeof( NonGenericClass );
        var result = sut.GetImplementation<IDirect>();
        result.Should().Be( typeof( IDirect ) );
    }

    [Fact]
    public void GetImplementation_ShouldReturnCorrectResult_WhenInterfaceIsImplementedThroughOtherInterface()
    {
        var sut = typeof( NonGenericClass );
        var result = sut.GetImplementation<IIndirectFromInterface>();
        result.Should().Be( typeof( IIndirectFromInterface ) );
    }

    [Fact]
    public void GetImplementation_ShouldReturnCorrectResult_WhenInterfaceIsImplementedThroughBaseType()
    {
        var sut = typeof( NonGenericClass );
        var result = sut.GetImplementation<IIndirectFromType>();
        result.Should().Be( typeof( IIndirectFromType ) );
    }

    [Fact]
    public void GetImplementation_ShouldReturnNull_WhenInterfaceIsNotImplemented()
    {
        var sut = typeof( NonGenericClass );
        var result = sut.GetImplementation<INotImplemented>();
        result.Should().BeNull();
    }

    [Fact]
    public void Implements_ShouldReturnTrue_WhenInterfaceIsDirectlyImplemented()
    {
        var sut = typeof( NonGenericClass );
        var result = sut.Implements<IDirect>();
        result.Should().BeTrue();
    }

    [Fact]
    public void Implements_ShouldReturnTrue_WhenInterfaceIsImplementedThroughOtherInterface()
    {
        var sut = typeof( NonGenericClass );
        var result = sut.Implements<IIndirectFromInterface>();
        result.Should().BeTrue();
    }

    [Fact]
    public void Implements_ShouldReturnTrue_WhenInterfaceIsImplementedThroughBaseType()
    {
        var sut = typeof( NonGenericClass );
        var result = sut.Implements<IIndirectFromType>();
        result.Should().BeTrue();
    }

    [Fact]
    public void Implements_ShouldReturnFalse_WhenInterfaceIsNotImplemented()
    {
        var sut = typeof( NonGenericClass );
        var result = sut.Implements<INotImplemented>();
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData( typeof( IBaseGeneric<> ) )]
    [InlineData( typeof( IDirect ) )]
    public void GetOpenGenericImplementations_ShouldReturnEmptyCollection(Type type)
    {
        var sut = typeof( NonGenericClass );
        var result = sut.GetOpenGenericImplementations( type );
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData( typeof( IBaseGeneric<> ) )]
    [InlineData( typeof( IDirect ) )]
    public void ImplementsOpenGeneric_ShouldReturnFalse(Type type)
    {
        var sut = typeof( NonGenericClass );
        var result = sut.ImplementsOpenGeneric( type );
        result.Should().BeFalse();
    }

    [Fact]
    public void GetAllImplementedGenericDefinitions_ShouldReturnEmpty()
    {
        var sut = typeof( NonGenericClass );
        var result = sut.GetAllImplementedGenericDefinitions();
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetExtension_ShouldReturnCorrectResult_WhenTypeIsDirectParent()
    {
        var sut = typeof( NonGenericClass );
        var result = sut.GetExtension<BaseClass>();
        result.Should().Be( typeof( BaseClass ) );
    }

    [Fact]
    public void GetExtension_ShouldReturnCorrectResult_WhenTypeIsIndirectAncestor()
    {
        var sut = typeof( NonGenericClass );
        var result = sut.GetExtension<object>();
        result.Should().Be( typeof( object ) );
    }

    [Fact]
    public void GetExtension_ShouldReturnNull_WhenTypeIsNotAnAncestor()
    {
        var sut = typeof( NonGenericClass );
        var result = sut.GetExtension<NotExtended>();
        result.Should().BeNull();
    }

    [Fact]
    public void GetExtension_ShouldReturnNullForObject()
    {
        var sut = typeof( object );
        var result = sut.GetExtension<object>();
        result.Should().BeNull();
    }

    [Fact]
    public void GetExtension_ShouldReturnNull_WhenTypeIsSelf()
    {
        var sut = typeof( NonGenericClass );
        var result = sut.GetExtension<NonGenericClass>();
        result.Should().BeNull();
    }

    [Fact]
    public void Extends_ShouldReturnTrue_WhenTypeIsDirectParent()
    {
        var sut = typeof( NonGenericClass );
        var result = sut.Extends<BaseClass>();
        result.Should().BeTrue();
    }

    [Fact]
    public void Extends_ShouldReturnTrue_WhenTypeIsIndirectAncestor()
    {
        var sut = typeof( NonGenericClass );
        var result = sut.Extends<object>();
        result.Should().BeTrue();
    }

    [Fact]
    public void Extends_ShouldReturnFalse_WhenTypeIsNotAnAncestor()
    {
        var sut = typeof( NonGenericClass );
        var result = sut.Extends<NotExtended>();
        result.Should().BeFalse();
    }

    [Fact]
    public void Extends_ShouldReturnFalse_WhenTypeIsSelf()
    {
        var sut = typeof( NonGenericClass );
        var result = sut.Extends<NonGenericClass>();
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData( typeof( BaseGenericClass<> ) )]
    [InlineData( typeof( BaseClass ) )]
    public void GetOpenGenericExtension_ShouldReturnNull(Type type)
    {
        var sut = typeof( NonGenericClass );
        var result = sut.GetOpenGenericExtension( type );
        result.Should().BeNull();
    }

    [Theory]
    [InlineData( typeof( BaseGenericClass<> ) )]
    [InlineData( typeof( BaseClass ) )]
    public void ExtendsOpenGeneric_ShouldReturnFalse(Type type)
    {
        var sut = typeof( NonGenericClass );
        var result = sut.ExtendsOpenGeneric( type );
        result.Should().BeFalse();
    }

    [Fact]
    public void GetAllExtendedGenericDefinitions_ShouldReturnEmpty()
    {
        var sut = typeof( NonGenericClass );
        var result = sut.GetAllExtendedGenericDefinitions();
        result.Should().BeEmpty();
    }

    [Fact]
    public void FindMember_ShouldReturnCorrectMember_WhenMemberExistsDirectlyInProvidedType()
    {
        var sut = typeof( FindMemberClass );
        var result = sut.FindMember( t => t.GetMethod( "GetEnumerator", BindingFlags.Public | BindingFlags.Instance ) );

        using ( new AssertionScope() )
        {
            result.Should().NotBeNull();
            (result?.DeclaringType).Should().Be( sut );
            (result?.ReturnType).Should().Be( typeof( IEnumerator<char> ) );
        }
    }

    [Fact]
    public void FindMember_ShouldReturnCorrectMember_WhenMemberExistsInBaseType()
    {
        var sut = typeof( FindMemberSubClass );
        var result = sut.FindMember(
            t => t != typeof( FindMemberSubClass ) ? t.GetMethod( "GetEnumerator", BindingFlags.Public | BindingFlags.Instance ) : null );

        using ( new AssertionScope() )
        {
            result.Should().NotBeNull();
            (result?.DeclaringType).Should().Be( typeof( FindMemberClass ) );
            (result?.ReturnType).Should().Be( typeof( IEnumerator<char> ) );
        }
    }

    [Fact]
    public void FindMember_ShouldReturnCorrectMember_WhenMemberIsImplementedExplicitlyFromInterface()
    {
        var sut = typeof( FindMemberClass );
        var result = sut.FindMember( t => t.GetMethod( "Dispose", BindingFlags.Public | BindingFlags.Instance ) );

        using ( new AssertionScope() )
        {
            result.Should().NotBeNull();
            (result?.DeclaringType).Should().Be( typeof( IDisposable ) );
        }
    }

    [Fact]
    public void FindMember_ShouldReturnCorrectMember_WhenMemberIsImplementedExplicitlyFromNestedInterface()
    {
        var sut = typeof( FindMemberClass );
        var result = sut.FindMember(
            t =>
            {
                var m = t.GetMethod( "GetEnumerator", BindingFlags.Public | BindingFlags.Instance );
                return m is not null && m.ReturnType == typeof( IEnumerator ) ? m : null;
            } );

        using ( new AssertionScope() )
        {
            result.Should().NotBeNull();
            (result?.DeclaringType).Should().Be( typeof( IEnumerable ) );
        }
    }

    [Fact]
    public void FindMember_ShouldReturnNull_WhenMemberDoesNotExist()
    {
        var sut = typeof( FindMemberClass );
        var result = sut.FindMember( t => t.GetMethod( "MoveNext", BindingFlags.Public | BindingFlags.Instance ) );
        result.Should().BeNull();
    }

    private class FindMemberClass : IEnumerable<char>, IDisposable
    {
        void IDisposable.Dispose() { }

        public IEnumerator<char> GetEnumerator()
        {
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    private sealed class FindMemberSubClass : FindMemberClass { }
}
