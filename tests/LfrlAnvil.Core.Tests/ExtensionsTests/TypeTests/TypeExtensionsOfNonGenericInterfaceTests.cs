using System;
using FluentAssertions;
using LfrlAnvil.Extensions;
using Xunit;

namespace LfrlAnvil.Tests.ExtensionsTests.TypeTests
{
    public class NonGenericInterfaceTests : TypeTestsBase
    {
        [Fact]
        public void GetImplementation_ShouldReturnCorrectResult_WhenInterfaceIsDirectlyImplemented()
        {
            var sut = typeof( INonGenericInterface );
            var result = sut.GetImplementation<IDirect>();
            result.Should().Be( typeof( IDirect ) );
        }

        [Fact]
        public void GetImplementation_ShouldReturnCorrectResult_WhenInterfaceIsImplementedThroughOtherInterface()
        {
            var sut = typeof( INonGenericInterface );
            var result = sut.GetImplementation<IIndirectFromInterface>();
            result.Should().Be( typeof( IIndirectFromInterface ) );
        }

        [Fact]
        public void GetImplementation_ShouldReturnNull_WhenInterfaceIsNotImplemented()
        {
            var sut = typeof( INonGenericInterface );
            var result = sut.GetImplementation<INotImplemented>();
            result.Should().BeNull();
        }

        [Fact]
        public void GetImplementation_ShouldReturnNull_WhenInterfaceIsSelf()
        {
            var sut = typeof( INonGenericInterface );
            var result = sut.GetImplementation<INonGenericInterface>();
            result.Should().BeNull();
        }

        [Fact]
        public void Implements_ShouldReturnTrue_WhenInterfaceIsDirectlyImplemented()
        {
            var sut = typeof( INonGenericInterface );
            var result = sut.Implements<IDirect>();
            result.Should().BeTrue();
        }

        [Fact]
        public void Implements_ShouldReturnTrue_WhenInterfaceIsImplementedThroughOtherInterface()
        {
            var sut = typeof( INonGenericInterface );
            var result = sut.Implements<IIndirectFromInterface>();
            result.Should().BeTrue();
        }

        [Fact]
        public void Implements_ShouldReturnFalse_WhenInterfaceIsNotImplemented()
        {
            var sut = typeof( INonGenericInterface );
            var result = sut.Implements<INotImplemented>();
            result.Should().BeFalse();
        }

        [Fact]
        public void Implements_ShouldReturnFalse_WhenInterfaceIsSelf()
        {
            var sut = typeof( INonGenericInterface );
            var result = sut.Implements<INonGenericInterface>();
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData( typeof( IBaseGeneric<> ) )]
        [InlineData( typeof( IDirect ) )]
        public void GetOpenGenericImplementations_ShouldReturnEmptyCollection(Type type)
        {
            var sut = typeof( INonGenericInterface );
            var result = sut.GetOpenGenericImplementations( type );
            result.Should().BeEmpty();
        }

        [Theory]
        [InlineData( typeof( IBaseGeneric<> ) )]
        [InlineData( typeof( IDirect ) )]
        public void ImplementsOpenGeneric_ShouldReturnFalse(Type type)
        {
            var sut = typeof( INonGenericInterface );
            var result = sut.ImplementsOpenGeneric( type );
            result.Should().BeFalse();
        }

        [Fact]
        public void GetAllImplementedGenericDefinitions_ShouldReturnEmpty()
        {
            var sut = typeof( INonGenericInterface );
            var result = sut.GetAllImplementedGenericDefinitions();
            result.Should().BeEmpty();
        }
    }
}
