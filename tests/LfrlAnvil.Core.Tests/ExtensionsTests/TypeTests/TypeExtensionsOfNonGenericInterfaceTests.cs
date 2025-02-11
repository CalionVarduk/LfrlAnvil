using LfrlAnvil.Extensions;

namespace LfrlAnvil.Tests.ExtensionsTests.TypeTests;

public class NonGenericInterfaceTests : TypeTestsBase
{
    [Fact]
    public void GetImplementation_ShouldReturnCorrectResult_WhenInterfaceIsDirectlyImplemented()
    {
        var sut = typeof( INonGenericInterface );
        var result = sut.GetImplementation<IDirect>();
        result.TestEquals( typeof( IDirect ) ).Go();
    }

    [Fact]
    public void GetImplementation_ShouldReturnCorrectResult_WhenInterfaceIsImplementedThroughOtherInterface()
    {
        var sut = typeof( INonGenericInterface );
        var result = sut.GetImplementation<IIndirectFromInterface>();
        result.TestEquals( typeof( IIndirectFromInterface ) ).Go();
    }

    [Fact]
    public void GetImplementation_ShouldReturnNull_WhenInterfaceIsNotImplemented()
    {
        var sut = typeof( INonGenericInterface );
        var result = sut.GetImplementation<INotImplemented>();
        result.TestNull().Go();
    }

    [Fact]
    public void GetImplementation_ShouldReturnNull_WhenInterfaceIsSelf()
    {
        var sut = typeof( INonGenericInterface );
        var result = sut.GetImplementation<INonGenericInterface>();
        result.TestNull().Go();
    }

    [Fact]
    public void Implements_ShouldReturnTrue_WhenInterfaceIsDirectlyImplemented()
    {
        var sut = typeof( INonGenericInterface );
        var result = sut.Implements<IDirect>();
        result.TestTrue().Go();
    }

    [Fact]
    public void Implements_ShouldReturnTrue_WhenInterfaceIsImplementedThroughOtherInterface()
    {
        var sut = typeof( INonGenericInterface );
        var result = sut.Implements<IIndirectFromInterface>();
        result.TestTrue().Go();
    }

    [Fact]
    public void Implements_ShouldReturnFalse_WhenInterfaceIsNotImplemented()
    {
        var sut = typeof( INonGenericInterface );
        var result = sut.Implements<INotImplemented>();
        result.TestFalse().Go();
    }

    [Fact]
    public void Implements_ShouldReturnFalse_WhenInterfaceIsSelf()
    {
        var sut = typeof( INonGenericInterface );
        var result = sut.Implements<INonGenericInterface>();
        result.TestFalse().Go();
    }

    [Theory]
    [InlineData( typeof( IBaseGeneric<> ) )]
    [InlineData( typeof( IDirect ) )]
    public void GetOpenGenericImplementations_ShouldReturnEmptyCollection(Type type)
    {
        var sut = typeof( INonGenericInterface );
        var result = sut.GetOpenGenericImplementations( type );
        result.TestEmpty().Go();
    }

    [Theory]
    [InlineData( typeof( IBaseGeneric<> ) )]
    [InlineData( typeof( IDirect ) )]
    public void ImplementsOpenGeneric_ShouldReturnFalse(Type type)
    {
        var sut = typeof( INonGenericInterface );
        var result = sut.ImplementsOpenGeneric( type );
        result.TestFalse().Go();
    }

    [Fact]
    public void GetAllImplementedGenericDefinitions_ShouldReturnEmpty()
    {
        var sut = typeof( INonGenericInterface );
        var result = sut.GetAllImplementedGenericDefinitions();
        result.TestEmpty().Go();
    }
}
