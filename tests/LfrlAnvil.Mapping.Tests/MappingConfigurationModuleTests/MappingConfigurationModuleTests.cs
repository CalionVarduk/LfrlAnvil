using System;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using Xunit;

namespace LfrlAnvil.Mapping.Tests.MappingConfigurationModuleTests
{
    public class MappingConfigurationModuleTests : TestsBase
    {
        [Fact]
        public void Ctor_ShouldReturnEmptyModule()
        {
            var sut = new MappingConfigurationModule();

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

            var sut = new MappingConfigurationModule();

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

            var sut = new MappingConfigurationModule();
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
            var module = new MappingConfigurationModule().Configure( configuration );
            var expectedStores = module.GetMappingStores();

            var sut = new MappingConfigurationModule();

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
            var firstModule = new MappingConfigurationModule().Configure( firstConfiguration );
            var secondModule = new MappingConfigurationModule().Configure( secondConfiguration );
            var expectedStores = firstModule.GetMappingStores().Concat( secondModule.GetMappingStores() );

            var sut = new MappingConfigurationModule();
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
        public void Configure_ShouldThrowArgumentException_WhenAddingSelf()
        {
            var sut = new MappingConfigurationModule();
            var action = Lambda.Of( () => sut.Configure( sut ) );
            action.Should().ThrowExactly<ArgumentException>();
        }

        [Fact]
        public void Configure_ShouldThrowArgumentException_WhenAddingModuleWithParent()
        {
            var other = new MappingConfigurationModule();
            var sut = new MappingConfigurationModule();
            sut.Configure( other );

            var action = Lambda.Of( () => sut.Configure( other ) );

            action.Should().ThrowExactly<ArgumentException>();
        }

        [Fact]
        public void Configure_ShouldThrowArgumentException_WhenItLeadsToCyclicReference()
        {
            var other = new MappingConfigurationModule();
            var sut = new MappingConfigurationModule();
            other.Configure( sut );

            var action = Lambda.Of( () => sut.Configure( other ) );

            action.Should().ThrowExactly<ArgumentException>();
        }

        [Fact]
        public void Configure_ShouldThrowArgumentException_WhenItLeadsToIndirectCyclicReference()
        {
            var root = new MappingConfigurationModule();
            var other = new MappingConfigurationModule();
            var sut = new MappingConfigurationModule();
            root.Configure( other );
            other.Configure( sut );

            var action = Lambda.Of( () => sut.Configure( root ) );

            action.Should().ThrowExactly<ArgumentException>();
        }
    }
}
