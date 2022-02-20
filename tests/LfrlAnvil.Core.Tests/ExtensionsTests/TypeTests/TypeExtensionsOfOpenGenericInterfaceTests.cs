using System.Linq;
using FluentAssertions;
using LfrlAnvil.Extensions;
using Xunit;

namespace LfrlAnvil.Tests.ExtensionsTests.TypeTests
{
    public class OpenGenericInterfaceTests : TypeTestsBase
    {
        [Fact]
        public void GetImplementation_ShouldReturnCorrectResult_WhenInterfaceIsDirectlyImplemented()
        {
            var sut = typeof( IGenericInterface<> );
            var result = sut.GetImplementation<IDirect>();
            result.Should().Be( typeof( IDirect ) );
        }

        [Fact]
        public void GetImplementation_ShouldReturnCorrectResult_WhenInterfaceIsImplementedThroughOtherInterface()
        {
            var sut = typeof( IGenericInterface<> );
            var result = sut.GetImplementation<IIndirectFromInterface>();
            result.Should().Be( typeof( IIndirectFromInterface ) );
        }

        [Fact]
        public void GetImplementation_ShouldReturnNull_WhenInterfaceIsNotImplemented()
        {
            var sut = typeof( IGenericInterface<> );
            var result = sut.GetImplementation<INotImplemented>();
            result.Should().BeNull();
        }

        [Fact]
        public void GetImplementation_ShouldReturnNull_WhenInterfaceIsOpenGeneric()
        {
            var sut = typeof( IGenericInterface<> );
            var result = sut.GetImplementation( typeof( IBaseGeneric<> ) );
            result.Should().BeNull();
        }

        [Fact]
        public void Implements_ShouldReturnTrue_WhenInterfaceIsDirectlyImplemented()
        {
            var sut = typeof( IGenericInterface<> );
            var result = sut.Implements<IDirect>();
            result.Should().BeTrue();
        }

        [Fact]
        public void Implements_ShouldReturnTrue_WhenInterfaceIsImplementedThroughOtherInterface()
        {
            var sut = typeof( IGenericInterface<> );
            var result = sut.Implements<IIndirectFromInterface>();
            result.Should().BeTrue();
        }

        [Fact]
        public void Implements_ShouldReturnFalse_WhenInterfaceIsNotImplemented()
        {
            var sut = typeof( IGenericInterface<> );
            var result = sut.Implements<INotImplemented>();
            result.Should().BeFalse();
        }

        [Fact]
        public void Implements_ShouldReturnFalse_WhenInterfaceIsOpenGeneric()
        {
            var sut = typeof( IGenericInterface<> );
            var result = sut.Implements( typeof( IBaseGeneric<> ) );
            result.Should().BeFalse();
        }

        [Fact]
        public void GetOpenGenericImplementations_ShouldReturnCorrectResult()
        {
            var sut = typeof( IGenericInterface<> );
            var expected = new[] { typeof( IBaseGeneric<> ).MakeGenericType( sut.GetGenericArguments()[0] ) }.AsEnumerable();
            var result = sut.GetOpenGenericImplementations( typeof( IBaseGeneric<> ) );
            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void ImplementsOpenGeneric_ShouldReturnTrue()
        {
            var sut = typeof( IGenericInterface<> );
            var result = sut.ImplementsOpenGeneric( typeof( IBaseGeneric<> ) );
            result.Should().BeTrue();
        }

        [Fact]
        public void GetAllImplementedGenericDefinitions_ShouldReturnCorrectResult()
        {
            var sut = typeof( IGenericInterface<> );
            var expected = new[] { typeof( IBaseGeneric<> ) }.AsEnumerable();
            var result = sut.GetAllImplementedGenericDefinitions();
            result.Should().BeEquivalentTo( expected );
        }
    }
}
