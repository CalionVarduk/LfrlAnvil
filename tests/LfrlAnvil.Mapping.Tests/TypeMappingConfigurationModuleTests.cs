using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Mapping.Exceptions;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Mapping.Tests;

public class TypeMappingConfigurationModuleTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldReturnEmptyModule()
    {
        var sut = new TypeMappingConfigurationModule();

        using ( new AssertionScope() )
        {
            sut.Parent.Should().BeNull();
            sut.GetSubmodules().Should().BeEmpty();
            sut.GetMappingStores().Should().BeEmpty();
        }
    }

    [Fact]
    public void Configure_ShouldAddNewConfigurationCorrectly_WhenModuleIsEmpty()
    {
        var configuration = TypeMappingConfiguration.Create( (string _, ITypeMapper _) => default( int ) );
        var expectedStores = configuration.GetMappingStores();

        var sut = new TypeMappingConfigurationModule();

        var result = sut.Configure( configuration );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.GetSubmodules().Should().BeEmpty();
            sut.GetMappingStores().Should().BeSequentiallyEqualTo( expectedStores );
        }
    }

    [Fact]
    public void Configure_ShouldAddNewConfigurationCorrectly_WhenModuleIsNotEmpty()
    {
        var firstConfiguration = TypeMappingConfiguration.Create( (string _, ITypeMapper _) => default( int ) );
        var secondConfiguration = TypeMappingConfiguration.Create( (int _, ITypeMapper _) => default( Guid ) );
        var expectedStores = firstConfiguration.GetMappingStores().Concat( secondConfiguration.GetMappingStores() );

        var sut = new TypeMappingConfigurationModule();
        sut.Configure( firstConfiguration );

        var result = sut.Configure( secondConfiguration );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.GetSubmodules().Should().BeEmpty();
            sut.GetMappingStores().Should().BeSequentiallyEqualTo( expectedStores );
        }
    }

    [Fact]
    public void Configure_ShouldAddNewModuleCorrectly_WhenModuleIsEmpty()
    {
        var configuration = TypeMappingConfiguration.Create( (string _, ITypeMapper _) => default( int ) );
        var module = new TypeMappingConfigurationModule().Configure( configuration );
        var expectedStores = module.GetMappingStores();

        var sut = new TypeMappingConfigurationModule();

        var result = sut.Configure( module );

        using ( new AssertionScope() )
        {
            module.Parent.Should().Be( sut );
            result.Should().BeSameAs( sut );
            sut.GetSubmodules().Should().BeSequentiallyEqualTo( module );
            sut.GetMappingStores().Should().BeSequentiallyEqualTo( expectedStores );
        }
    }

    [Fact]
    public void Configure_ShouldAddNewModuleCorrectly_WhenModuleIsNotEmpty()
    {
        var firstConfiguration = TypeMappingConfiguration.Create( (string _, ITypeMapper _) => default( int ) );
        var secondConfiguration = TypeMappingConfiguration.Create( (int _, ITypeMapper _) => default( Guid ) );
        var firstModule = new TypeMappingConfigurationModule().Configure( firstConfiguration );
        var secondModule = new TypeMappingConfigurationModule().Configure( secondConfiguration );
        var expectedStores = firstModule.GetMappingStores().Concat( secondModule.GetMappingStores() );

        var sut = new TypeMappingConfigurationModule();
        sut.Configure( firstModule );

        var result = sut.Configure( secondModule );

        using ( new AssertionScope() )
        {
            secondModule.Parent.Should().Be( sut );
            result.Should().BeSameAs( sut );
            sut.GetSubmodules().Should().BeSequentiallyEqualTo( firstModule, secondModule );
            sut.GetMappingStores().Should().BeSequentiallyEqualTo( expectedStores );
        }
    }

    [Fact]
    public void Configure_ShouldThrowInvalidTypeMappingSubmoduleConfigurationException_WhenAddingSelf()
    {
        var sut = new TypeMappingConfigurationModule();
        var action = Lambda.Of( () => sut.Configure( sut ) );
        action.Should().ThrowExactly<InvalidTypeMappingSubmoduleConfigurationException>();
    }

    [Fact]
    public void Configure_ShouldThrowInvalidTypeMappingSubmoduleConfigurationException_WhenAddingModuleWithParent()
    {
        var other = new TypeMappingConfigurationModule();
        var sut = new TypeMappingConfigurationModule();
        sut.Configure( other );

        var action = Lambda.Of( () => sut.Configure( other ) );

        action.Should().ThrowExactly<InvalidTypeMappingSubmoduleConfigurationException>();
    }

    [Fact]
    public void Configure_ShouldThrowInvalidTypeMappingSubmoduleConfigurationException_WhenItLeadsToCyclicReference()
    {
        var other = new TypeMappingConfigurationModule();
        var sut = new TypeMappingConfigurationModule();
        other.Configure( sut );

        var action = Lambda.Of( () => sut.Configure( other ) );

        action.Should().ThrowExactly<InvalidTypeMappingSubmoduleConfigurationException>();
    }

    [Fact]
    public void Configure_ShouldThrowInvalidTypeMappingSubmoduleConfigurationException_WhenItLeadsToIndirectCyclicReference()
    {
        var root = new TypeMappingConfigurationModule();
        var other = new TypeMappingConfigurationModule();
        var sut = new TypeMappingConfigurationModule();
        root.Configure( other );
        other.Configure( sut );

        var action = Lambda.Of( () => sut.Configure( root ) );

        action.Should().ThrowExactly<InvalidTypeMappingSubmoduleConfigurationException>();
    }
}
