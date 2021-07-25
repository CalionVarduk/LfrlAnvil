using System.Linq;
using FluentAssertions;
using LfrlSoft.NET.Common.Extensions;
using Xunit;

namespace LfrlSoft.NET.Common.Tests.Extensions.Type
{
    public class OpenGenericInterfaceTests : TypeTestsBase
    {
        [Fact]
        public void GetImplementation_ShouldReturnCorrectResultWhenInterfaceIsDirectlyImplemented()
        {
            var sut = typeof( IGenericInterface<> );
            var result = sut.GetImplementation<IDirect>();
            result.Should().Be( typeof( IDirect ) );
        }

        [Fact]
        public void GetImplementation_ShouldReturnCorrectResultWhenInterfaceIsImplementedThroughOtherInterface()
        {
            var sut = typeof( IGenericInterface<> );
            var result = sut.GetImplementation<IIndirectFromInterface>();
            result.Should().Be( typeof( IIndirectFromInterface ) );
        }

        [Fact]
        public void GetImplementation_ShouldReturnNullWhenInterfaceIsNotImplemented()
        {
            var sut = typeof( IGenericInterface<> );
            var result = sut.GetImplementation<INotImplemented>();
            result.Should().BeNull();
        }

        [Fact]
        public void GetImplementation_ShouldReturnNullWhenInterfaceIsOpenGeneric()
        {
            var sut = typeof( IGenericInterface<> );
            var result = sut.GetImplementation( typeof( IBaseGeneric<> ) );
            result.Should().BeNull();
        }

        [Fact]
        public void Implements_ShouldReturnTrueWhenInterfaceIsDirectlyImplemented()
        {
            var sut = typeof( IGenericInterface<> );
            var result = sut.Implements<IDirect>();
            result.Should().BeTrue();
        }

        [Fact]
        public void Implements_ShouldReturnTrueWhenInterfaceIsImplementedThroughOtherInterface()
        {
            var sut = typeof( IGenericInterface<> );
            var result = sut.Implements<IIndirectFromInterface>();
            result.Should().BeTrue();
        }

        [Fact]
        public void Implements_ShouldReturnFalseWhenInterfaceIsNotImplemented()
        {
            var sut = typeof( IGenericInterface<> );
            var result = sut.Implements<INotImplemented>();
            result.Should().BeFalse();
        }

        [Fact]
        public void Implements_ShouldReturnFalseWhenInterfaceIsOpenGeneric()
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
