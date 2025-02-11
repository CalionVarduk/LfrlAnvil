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
        result.TestEquals( typeof( IDirect ) ).Go();
    }

    [Fact]
    public void GetImplementation_ShouldReturnCorrectResult_WhenInterfaceIsImplementedThroughOtherInterface()
    {
        var sut = typeof( NonGenericClass );
        var result = sut.GetImplementation<IIndirectFromInterface>();
        result.TestEquals( typeof( IIndirectFromInterface ) ).Go();
    }

    [Fact]
    public void GetImplementation_ShouldReturnCorrectResult_WhenInterfaceIsImplementedThroughBaseType()
    {
        var sut = typeof( NonGenericClass );
        var result = sut.GetImplementation<IIndirectFromType>();
        result.TestEquals( typeof( IIndirectFromType ) ).Go();
    }

    [Fact]
    public void GetImplementation_ShouldReturnNull_WhenInterfaceIsNotImplemented()
    {
        var sut = typeof( NonGenericClass );
        var result = sut.GetImplementation<INotImplemented>();
        result.TestNull().Go();
    }

    [Fact]
    public void Implements_ShouldReturnTrue_WhenInterfaceIsDirectlyImplemented()
    {
        var sut = typeof( NonGenericClass );
        var result = sut.Implements<IDirect>();
        result.TestTrue().Go();
    }

    [Fact]
    public void Implements_ShouldReturnTrue_WhenInterfaceIsImplementedThroughOtherInterface()
    {
        var sut = typeof( NonGenericClass );
        var result = sut.Implements<IIndirectFromInterface>();
        result.TestTrue().Go();
    }

    [Fact]
    public void Implements_ShouldReturnTrue_WhenInterfaceIsImplementedThroughBaseType()
    {
        var sut = typeof( NonGenericClass );
        var result = sut.Implements<IIndirectFromType>();
        result.TestTrue().Go();
    }

    [Fact]
    public void Implements_ShouldReturnFalse_WhenInterfaceIsNotImplemented()
    {
        var sut = typeof( NonGenericClass );
        var result = sut.Implements<INotImplemented>();
        result.TestFalse().Go();
    }

    [Theory]
    [InlineData( typeof( IBaseGeneric<> ) )]
    [InlineData( typeof( IDirect ) )]
    public void GetOpenGenericImplementations_ShouldReturnEmptyCollection(Type type)
    {
        var sut = typeof( NonGenericClass );
        var result = sut.GetOpenGenericImplementations( type );
        result.TestEmpty().Go();
    }

    [Theory]
    [InlineData( typeof( IBaseGeneric<> ) )]
    [InlineData( typeof( IDirect ) )]
    public void ImplementsOpenGeneric_ShouldReturnFalse(Type type)
    {
        var sut = typeof( NonGenericClass );
        var result = sut.ImplementsOpenGeneric( type );
        result.TestFalse().Go();
    }

    [Fact]
    public void GetAllImplementedGenericDefinitions_ShouldReturnEmpty()
    {
        var sut = typeof( NonGenericClass );
        var result = sut.GetAllImplementedGenericDefinitions();
        result.TestEmpty().Go();
    }

    [Fact]
    public void GetExtension_ShouldReturnCorrectResult_WhenTypeIsDirectParent()
    {
        var sut = typeof( NonGenericClass );
        var result = sut.GetExtension<BaseClass>();
        result.TestEquals( typeof( BaseClass ) ).Go();
    }

    [Fact]
    public void GetExtension_ShouldReturnCorrectResult_WhenTypeIsIndirectAncestor()
    {
        var sut = typeof( NonGenericClass );
        var result = sut.GetExtension<object>();
        result.TestEquals( typeof( object ) ).Go();
    }

    [Fact]
    public void GetExtension_ShouldReturnNull_WhenTypeIsNotAnAncestor()
    {
        var sut = typeof( NonGenericClass );
        var result = sut.GetExtension<NotExtended>();
        result.TestNull().Go();
    }

    [Fact]
    public void GetExtension_ShouldReturnNullForObject()
    {
        var sut = typeof( object );
        var result = sut.GetExtension<object>();
        result.TestNull().Go();
    }

    [Fact]
    public void GetExtension_ShouldReturnNull_WhenTypeIsSelf()
    {
        var sut = typeof( NonGenericClass );
        var result = sut.GetExtension<NonGenericClass>();
        result.TestNull().Go();
    }

    [Fact]
    public void Extends_ShouldReturnTrue_WhenTypeIsDirectParent()
    {
        var sut = typeof( NonGenericClass );
        var result = sut.Extends<BaseClass>();
        result.TestTrue().Go();
    }

    [Fact]
    public void Extends_ShouldReturnTrue_WhenTypeIsIndirectAncestor()
    {
        var sut = typeof( NonGenericClass );
        var result = sut.Extends<object>();
        result.TestTrue().Go();
    }

    [Fact]
    public void Extends_ShouldReturnFalse_WhenTypeIsNotAnAncestor()
    {
        var sut = typeof( NonGenericClass );
        var result = sut.Extends<NotExtended>();
        result.TestFalse().Go();
    }

    [Fact]
    public void Extends_ShouldReturnFalse_WhenTypeIsSelf()
    {
        var sut = typeof( NonGenericClass );
        var result = sut.Extends<NonGenericClass>();
        result.TestFalse().Go();
    }

    [Theory]
    [InlineData( typeof( BaseGenericClass<> ) )]
    [InlineData( typeof( BaseClass ) )]
    public void GetOpenGenericExtension_ShouldReturnNull(Type type)
    {
        var sut = typeof( NonGenericClass );
        var result = sut.GetOpenGenericExtension( type );
        result.TestNull().Go();
    }

    [Theory]
    [InlineData( typeof( BaseGenericClass<> ) )]
    [InlineData( typeof( BaseClass ) )]
    public void ExtendsOpenGeneric_ShouldReturnFalse(Type type)
    {
        var sut = typeof( NonGenericClass );
        var result = sut.ExtendsOpenGeneric( type );
        result.TestFalse().Go();
    }

    [Fact]
    public void GetAllExtendedGenericDefinitions_ShouldReturnEmpty()
    {
        var sut = typeof( NonGenericClass );
        var result = sut.GetAllExtendedGenericDefinitions();
        result.TestEmpty().Go();
    }

    [Fact]
    public void FindMember_ShouldReturnCorrectMember_WhenMemberExistsDirectlyInProvidedType()
    {
        var sut = typeof( FindMemberClass );
        var result = sut.FindMember( t => t.GetMethod( "GetEnumerator", BindingFlags.Public | BindingFlags.Instance ) );

        Assertion.All(
                result.TestNotNull(),
                (result?.DeclaringType).TestEquals( sut ),
                (result?.ReturnType).TestEquals( typeof( IEnumerator<char> ) ) )
            .Go();
    }

    [Fact]
    public void FindMember_ShouldReturnCorrectMember_WhenMemberExistsInBaseType()
    {
        var sut = typeof( FindMemberSubClass );
        var result = sut.FindMember(
            t => t != typeof( FindMemberSubClass ) ? t.GetMethod( "GetEnumerator", BindingFlags.Public | BindingFlags.Instance ) : null );

        Assertion.All(
                result.TestNotNull(),
                (result?.DeclaringType).TestEquals( typeof( FindMemberClass ) ),
                (result?.ReturnType).TestEquals( typeof( IEnumerator<char> ) ) )
            .Go();
    }

    [Fact]
    public void FindMember_ShouldReturnCorrectMember_WhenMemberIsImplementedExplicitlyFromInterface()
    {
        var sut = typeof( FindMemberClass );
        var result = sut.FindMember( t => t.GetMethod( "Dispose", BindingFlags.Public | BindingFlags.Instance ) );

        Assertion.All(
                result.TestNotNull(),
                (result?.DeclaringType).TestEquals( typeof( IDisposable ) ) )
            .Go();
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

        Assertion.All(
                result.TestNotNull(),
                (result?.DeclaringType).TestEquals( typeof( IEnumerable ) ) )
            .Go();
    }

    [Fact]
    public void FindMember_ShouldReturnNull_WhenMemberDoesNotExist()
    {
        var sut = typeof( FindMemberClass );
        var result = sut.FindMember( t => t.GetMethod( "MoveNext", BindingFlags.Public | BindingFlags.Instance ) );
        result.TestNull().Go();
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
