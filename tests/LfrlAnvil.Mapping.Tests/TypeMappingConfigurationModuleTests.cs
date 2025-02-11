using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Mapping.Exceptions;

namespace LfrlAnvil.Mapping.Tests;

public class TypeMappingConfigurationModuleTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldReturnEmptyModule()
    {
        var sut = new TypeMappingConfigurationModule();

        Assertion.All(
                sut.Parent.TestNull(),
                sut.GetSubmodules().TestEmpty(),
                sut.GetMappingStores().TestEmpty() )
            .Go();
    }

    [Fact]
    public void Configure_ShouldAddNewConfigurationCorrectly_WhenModuleIsEmpty()
    {
        var configuration = TypeMappingConfiguration.Create( (string _, ITypeMapper _) => default( int ) );
        var expectedStores = configuration.GetMappingStores();

        var sut = new TypeMappingConfigurationModule();

        var result = sut.Configure( configuration );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.GetSubmodules().TestEmpty(),
                sut.GetMappingStores().TestSequence( expectedStores ) )
            .Go();
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

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.GetSubmodules().TestEmpty(),
                sut.GetMappingStores().TestSequence( expectedStores ) )
            .Go();
    }

    [Fact]
    public void Configure_ShouldAddNewModuleCorrectly_WhenModuleIsEmpty()
    {
        var configuration = TypeMappingConfiguration.Create( (string _, ITypeMapper _) => default( int ) );
        var module = new TypeMappingConfigurationModule().Configure( configuration );
        var expectedStores = module.GetMappingStores();

        var sut = new TypeMappingConfigurationModule();

        var result = sut.Configure( module );

        Assertion.All(
                module.Parent.TestEquals( sut ),
                result.TestRefEquals( sut ),
                sut.GetSubmodules().TestSequence( [ module ] ),
                sut.GetMappingStores().TestSequence( expectedStores ) )
            .Go();
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

        Assertion.All(
                secondModule.Parent.TestEquals( sut ),
                result.TestRefEquals( sut ),
                sut.GetSubmodules().TestSequence( [ firstModule, secondModule ] ),
                sut.GetMappingStores().TestSequence( expectedStores ) )
            .Go();
    }

    [Fact]
    public void Configure_ShouldThrowInvalidTypeMappingSubmoduleConfigurationException_WhenAddingSelf()
    {
        var sut = new TypeMappingConfigurationModule();
        var action = Lambda.Of( () => sut.Configure( sut ) );
        action.Test( exc => exc.TestType().Exact<InvalidTypeMappingSubmoduleConfigurationException>() ).Go();
    }

    [Fact]
    public void Configure_ShouldThrowInvalidTypeMappingSubmoduleConfigurationException_WhenAddingModuleWithParent()
    {
        var other = new TypeMappingConfigurationModule();
        var sut = new TypeMappingConfigurationModule();
        sut.Configure( other );

        var action = Lambda.Of( () => sut.Configure( other ) );

        action.Test( exc => exc.TestType().Exact<InvalidTypeMappingSubmoduleConfigurationException>() ).Go();
    }

    [Fact]
    public void Configure_ShouldThrowInvalidTypeMappingSubmoduleConfigurationException_WhenItLeadsToCyclicReference()
    {
        var other = new TypeMappingConfigurationModule();
        var sut = new TypeMappingConfigurationModule();
        other.Configure( sut );

        var action = Lambda.Of( () => sut.Configure( other ) );

        action.Test( exc => exc.TestType().Exact<InvalidTypeMappingSubmoduleConfigurationException>() ).Go();
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

        action.Test( exc => exc.TestType().Exact<InvalidTypeMappingSubmoduleConfigurationException>() ).Go();
    }
}
