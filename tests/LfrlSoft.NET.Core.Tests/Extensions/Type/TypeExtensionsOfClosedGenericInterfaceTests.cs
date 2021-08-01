using System.Linq;
using FluentAssertions;
using LfrlSoft.NET.Core.Extensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Extensions.Type
{
    public class ClosedGenericInterfaceTests : TypeTestsBase
    {
        [Fact]
        public void GetImplementation_ShouldReturnCorrectResultWhenInterfaceIsDirectlyImplemented()
        {
            var sut = typeof( IGenericInterface<int> );
            var result = sut.GetImplementation<IDirect>();
            result.Should().Be( typeof( IDirect ) );
        }

        [Fact]
        public void GetImplementation_ShouldReturnCorrectResultWhenInterfaceIsImplementedThroughOtherInterface()
        {
            var sut = typeof( IGenericInterface<int> );
            var result = sut.GetImplementation<IIndirectFromInterface>();
            result.Should().Be( typeof( IIndirectFromInterface ) );
        }

        [Fact]
        public void GetImplementation_ShouldReturnCorrectResultWhenImplementedInterfaceIsGeneric()
        {
            var sut = typeof( IGenericInterface<int> );
            var result = sut.GetImplementation<IBaseGeneric<int>>();
            result.Should().Be( typeof( IBaseGeneric<int> ) );
        }

        [Fact]
        public void GetImplementation_ShouldReturnNullWhenInterfaceIsNotImplemented()
        {
            var sut = typeof( IGenericInterface<int> );
            var result = sut.GetImplementation<INotImplemented>();
            result.Should().BeNull();
        }

        [Fact]
        public void GetImplementation_ShouldReturnNullWhenInterfaceIsSelf()
        {
            var sut = typeof( IGenericInterface<int> );
            var result = sut.GetImplementation<IGenericInterface<int>>();
            result.Should().BeNull();
        }

        [Fact]
        public void GetImplementation_ShouldReturnNullWhenInterfaceIsOpenGeneric()
        {
            var sut = typeof( IGenericInterface<int> );
            var result = sut.GetImplementation( typeof( IBaseGeneric<> ) );
            result.Should().BeNull();
        }

        [Fact]
        public void Implements_ShouldReturnTrueWhenInterfaceIsDirectlyImplemented()
        {
            var sut = typeof( IGenericInterface<int> );
            var result = sut.Implements<IDirect>();
            result.Should().BeTrue();
        }

        [Fact]
        public void Implements_ShouldReturnTrueWhenInterfaceIsImplementedThroughOtherInterface()
        {
            var sut = typeof( IGenericInterface<int> );
            var result = sut.Implements<IIndirectFromInterface>();
            result.Should().BeTrue();
        }

        [Fact]
        public void Implements_ShouldReturnTrueWhenImplementedInterfaceIsGeneric()
        {
            var sut = typeof( IGenericInterface<int> );
            var result = sut.Implements<IBaseGeneric<int>>();
            result.Should().BeTrue();
        }

        [Fact]
        public void Implements_ShouldReturnFalseWhenInterfaceIsNotImplemented()
        {
            var sut = typeof( IGenericInterface<int> );
            var result = sut.Implements<INotImplemented>();
            result.Should().BeFalse();
        }

        [Fact]
        public void Implements_ShouldReturnFalseWhenInterfaceIsSelf()
        {
            var sut = typeof( IGenericInterface<int> );
            var result = sut.Implements<IGenericInterface<int>>();
            result.Should().BeFalse();
        }

        [Fact]
        public void Implements_ShouldReturnFalseWhenInterfaceIsOpenGeneric()
        {
            var sut = typeof( IGenericInterface<int> );
            var result = sut.Implements( typeof( IBaseGeneric<> ) );
            result.Should().BeFalse();
        }

        [Fact]
        public void GetOpenGenericImplementations_ShouldReturnCorrectResult()
        {
            var sut = typeof( IGenericInterface<int> );
            var expected = new[] { typeof( IBaseGeneric<int> ) }.AsEnumerable();
            var result = sut.GetOpenGenericImplementations( typeof( IBaseGeneric<> ) );
            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void ImplementsOpenGeneric_ShouldReturnTrue()
        {
            var sut = typeof( IGenericInterface<int> );
            var result = sut.ImplementsOpenGeneric( typeof( IBaseGeneric<> ) );
            result.Should().BeTrue();
        }

        [Fact]
        public void GetAllImplementedGenericDefinitions_ShouldReturnCorrectResult()
        {
            var sut = typeof( IGenericInterface<int> );
            var expected = new[] { typeof( IBaseGeneric<> ) }.AsEnumerable();
            var result = sut.GetAllImplementedGenericDefinitions();
            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void GetOpenGenericImplementations_ShouldReturnCorrectResult_ForMulti()
        {
            var sut = typeof( IMultiGenericClosedInterface );
            var expected = new[] { typeof( IBaseGeneric<int> ), typeof( IBaseGeneric<string> ) }.AsEnumerable();
            var result = sut.GetOpenGenericImplementations( typeof( IBaseGeneric<> ) );
            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void ImplementsOpenGeneric_ShouldReturnTrue_ForMulti()
        {
            var sut = typeof( IMultiGenericClosedInterface );
            var result = sut.ImplementsOpenGeneric( typeof( IBaseGeneric<> ) );
            result.Should().BeTrue();
        }

        [Fact]
        public void GetAllImplementedGenericDefinitions_ShouldReturnCorrectResult_ForMulti()
        {
            var sut = typeof( IMultiGenericClosedInterface );
            var expected = new[] { typeof( IBaseGeneric<> ) }.AsEnumerable();
            var result = sut.GetAllImplementedGenericDefinitions();
            result.Should().BeEquivalentTo( expected );
        }
    }
}
