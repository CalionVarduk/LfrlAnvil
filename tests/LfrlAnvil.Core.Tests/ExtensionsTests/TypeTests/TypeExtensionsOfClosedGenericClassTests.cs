using System.Linq;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Tests.ExtensionsTests.TypeTests;

public class TypeExtensionsOfClosedGenericClassTests : TypeTestsBase
{
    [Fact]
    public void GetImplementation_ShouldReturnCorrectResult_WhenInterfaceIsDirectlyImplemented()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.GetImplementation<IDirect>();
        result.TestEquals( typeof( IDirect ) ).Go();
    }

    [Fact]
    public void GetImplementation_ShouldReturnCorrectResult_WhenInterfaceIsImplementedThroughOtherInterface()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.GetImplementation<IIndirectFromInterface>();
        result.TestEquals( typeof( IIndirectFromInterface ) ).Go();
    }

    [Fact]
    public void GetImplementation_ShouldReturnCorrectResult_WhenInterfaceIsImplementedThroughBaseType()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.GetImplementation<IIndirectFromType>();
        result.TestEquals( typeof( IIndirectFromType ) ).Go();
    }

    [Fact]
    public void GetImplementation_ShouldReturnCorrectResult_WhenImplementedInterfaceIsGeneric()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.GetImplementation<IBaseGeneric<int>>();
        result.TestEquals( typeof( IBaseGeneric<int> ) ).Go();
    }

    [Fact]
    public void GetImplementation_ShouldReturnNull_WhenInterfaceIsNotImplemented()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.GetImplementation<INotImplemented>();
        result.TestNull().Go();
    }

    [Fact]
    public void GetImplementation_ShouldReturnNull_WhenInterfaceIsOpenGeneric()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.GetImplementation( typeof( IBaseGeneric<> ) );
        result.TestNull().Go();
    }

    [Fact]
    public void Implements_ShouldReturnTrue_WhenInterfaceIsDirectlyImplemented()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.Implements<IDirect>();
        result.TestTrue().Go();
    }

    [Fact]
    public void Implements_ShouldReturnTrue_WhenInterfaceIsImplementedThroughOtherInterface()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.Implements<IIndirectFromInterface>();
        result.TestTrue().Go();
    }

    [Fact]
    public void Implements_ShouldReturnTrue_WhenInterfaceIsImplementedThroughBaseType()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.Implements<IIndirectFromType>();
        result.TestTrue().Go();
    }

    [Fact]
    public void Implements_ShouldReturnTrue_WhenImplementedInterfaceIsGeneric()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.Implements<IBaseGeneric<int>>();
        result.TestTrue().Go();
    }

    [Fact]
    public void Implements_ShouldReturnFalse_WhenInterfaceIsNotImplemented()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.Implements<INotImplemented>();
        result.TestFalse().Go();
    }

    [Fact]
    public void Implements_ShouldReturnFalse_WhenInterfaceIsOpenGeneric()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.Implements( typeof( IBaseGeneric<> ) );
        result.TestFalse().Go();
    }

    [Fact]
    public void GetOpenGenericImplementations_ShouldReturnCorrectResult()
    {
        var sut = typeof( GenericClass<int> );
        var expected = new[] { typeof( IBaseGeneric<int> ) }.AsEnumerable();
        var result = sut.GetOpenGenericImplementations( typeof( IBaseGeneric<> ) );
        result.TestSetEqual( expected ).Go();
    }

    [Fact]
    public void ImplementsOpenGeneric_ShouldReturnTrue()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.ImplementsOpenGeneric( typeof( IBaseGeneric<> ) );
        result.TestTrue().Go();
    }

    [Fact]
    public void GetAllImplementedGenericDefinitions_ShouldReturnCorrectResult()
    {
        var sut = typeof( GenericClass<int> );
        var expected = new[] { typeof( IBaseGeneric<> ) }.AsEnumerable();
        var result = sut.GetAllImplementedGenericDefinitions();
        result.TestSetEqual( expected ).Go();
    }

    [Fact]
    public void GetOpenGenericImplementations_ShouldReturnCorrectResult_ForMulti()
    {
        var sut = typeof( MultiGenericClass<int, string> );
        var expected = new[] { typeof( IBaseGeneric<int> ), typeof( IBaseGeneric<string> ) }.AsEnumerable();
        var result = sut.GetOpenGenericImplementations( typeof( IBaseGeneric<> ) );
        result.TestSetEqual( expected ).Go();
    }

    [Fact]
    public void GetOpenGenericImplementations_ShouldReturnCorrectResult_ForMultiWithTheSameGenericArgs()
    {
        var sut = typeof( MultiGenericClass<int, int> );
        var expected = new[] { typeof( IBaseGeneric<int> ), typeof( IBaseGeneric<int> ) }.AsEnumerable();
        var result = sut.GetOpenGenericImplementations( typeof( IBaseGeneric<> ) );
        result.TestSetEqual( expected ).Go();
    }

    [Fact]
    public void ImplementsOpenGeneric_ShouldReturnTrue_ForMulti()
    {
        var sut = typeof( MultiGenericClass<int, string> );
        var result = sut.ImplementsOpenGeneric( typeof( IBaseGeneric<> ) );
        result.TestTrue().Go();
    }

    [Fact]
    public void GetAllImplementedGenericDefinitions_ShouldReturnCorrectResult_ForMulti()
    {
        var sut = typeof( MultiGenericClass<int, string> );
        var expected = new[] { typeof( IBaseGeneric<> ) }.AsEnumerable();
        var result = sut.GetAllImplementedGenericDefinitions();
        result.TestSetEqual( expected ).Go();
    }

    [Fact]
    public void GetExtension_ShouldReturnCorrectResult_WhenTypeIsDirectParent()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.GetExtension<BaseGenericClass<int>>();
        result.TestEquals( typeof( BaseGenericClass<int> ) ).Go();
    }

    [Fact]
    public void GetExtension_ShouldReturnCorrectResult_WhenTypeIsIndirectAncestor()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.GetExtension<BaseClass>();
        result.TestEquals( typeof( BaseClass ) ).Go();
    }

    [Fact]
    public void GetExtension_ShouldReturnNull_WhenTypeIsNotAnAncestor()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.GetExtension<NotExtended>();
        result.TestNull().Go();
    }

    [Fact]
    public void GetExtension_ShouldReturnNull_WhenTypeIsOpenGeneric()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.GetExtension( typeof( BaseGenericClass<> ) );
        result.TestNull().Go();
    }

    [Fact]
    public void GetExtension_ShouldReturnNull_WhenTypeIsSelf()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.GetExtension<GenericClass<int>>();
        result.TestNull().Go();
    }

    [Fact]
    public void Extends_ShouldReturnTrue_WhenTypeIsDirectParent()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.Extends<BaseGenericClass<int>>();
        result.TestTrue().Go();
    }

    [Fact]
    public void Extends_ShouldReturnTrue_WhenTypeIsIndirectAncestor()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.Extends<BaseClass>();
        result.TestTrue().Go();
    }

    [Fact]
    public void Extends_ShouldReturnFalse_WhenTypeIsNotAnAncestor()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.Extends<NotExtended>();
        result.TestFalse().Go();
    }

    [Fact]
    public void Extends_ShouldReturnFalse_WhenTypeIsOpenGeneric()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.Extends( typeof( BaseGenericClass<> ) );
        result.TestFalse().Go();
    }

    [Fact]
    public void Extends_ShouldReturnFalse_WhenTypeIsSelf()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.Extends<GenericClass<int>>();
        result.TestFalse().Go();
    }

    [Fact]
    public void GetOpenGenericExtension_ShouldReturnCorrectResult()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.GetOpenGenericExtension( typeof( BaseGenericClass<> ) );
        result.TestEquals( typeof( BaseGenericClass<int> ) ).Go();
    }

    [Fact]
    public void ExtendsOpenGeneric_ShouldReturnTrue()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.ExtendsOpenGeneric( typeof( BaseGenericClass<> ) );
        result.TestTrue().Go();
    }

    [Fact]
    public void GetAllExtendedGenericDefinitions_ShouldReturnCorrectResult()
    {
        var sut = typeof( GenericClass<int> );
        var expected = new[] { typeof( BaseGenericClass<> ) }.AsEnumerable();
        var result = sut.GetAllExtendedGenericDefinitions();
        result.TestSetEqual( expected ).Go();
    }
}
