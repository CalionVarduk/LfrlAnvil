using System.Linq;
using FluentAssertions;
using LfrlSoft.NET.Core.Extensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Extensions.Type
{
    public class TypeExtensionsOfClosedGenericClassTests : TypeTestsBase
    {
        [Fact]
        public void GetImplementation_ShouldReturnCorrectResultWhenInterfaceIsDirectlyImplemented()
        {
            var sut = typeof( GenericClass<int> );
            var result = sut.GetImplementation<IDirect>();
            result.Should().Be( typeof( IDirect ) );
        }

        [Fact]
        public void GetImplementation_ShouldReturnCorrectResultWhenInterfaceIsImplementedThroughOtherInterface()
        {
            var sut = typeof( GenericClass<int> );
            var result = sut.GetImplementation<IIndirectFromInterface>();
            result.Should().Be( typeof( IIndirectFromInterface ) );
        }

        [Fact]
        public void GetImplementation_ShouldReturnCorrectResultWhenInterfaceIsImplementedThroughBaseType()
        {
            var sut = typeof( GenericClass<int> );
            var result = sut.GetImplementation<IIndirectFromType>();
            result.Should().Be( typeof( IIndirectFromType ) );
        }

        [Fact]
        public void GetImplementation_ShouldReturnCorrectResultWhenImplementedInterfaceIsGeneric()
        {
            var sut = typeof( GenericClass<int> );
            var result = sut.GetImplementation<IBaseGeneric<int>>();
            result.Should().Be( typeof( IBaseGeneric<int> ) );
        }

        [Fact]
        public void GetImplementation_ShouldReturnNullWhenInterfaceIsNotImplemented()
        {
            var sut = typeof( GenericClass<int> );
            var result = sut.GetImplementation<INotImplemented>();
            result.Should().BeNull();
        }

        [Fact]
        public void GetImplementation_ShouldReturnNullWhenInterfaceIsOpenGeneric()
        {
            var sut = typeof( GenericClass<int> );
            var result = sut.GetImplementation( typeof( IBaseGeneric<> ) );
            result.Should().BeNull();
        }

        [Fact]
        public void Implements_ShouldReturnTrueWhenInterfaceIsDirectlyImplemented()
        {
            var sut = typeof( GenericClass<int> );
            var result = sut.Implements<IDirect>();
            result.Should().BeTrue();
        }

        [Fact]
        public void Implements_ShouldReturnTrueWhenInterfaceIsImplementedThroughOtherInterface()
        {
            var sut = typeof( GenericClass<int> );
            var result = sut.Implements<IIndirectFromInterface>();
            result.Should().BeTrue();
        }

        [Fact]
        public void Implements_ShouldReturnTrueWhenInterfaceIsImplementedThroughBaseType()
        {
            var sut = typeof( GenericClass<int> );
            var result = sut.Implements<IIndirectFromType>();
            result.Should().BeTrue();
        }

        [Fact]
        public void Implements_ShouldReturnTrueWhenImplementedInterfaceIsGeneric()
        {
            var sut = typeof( GenericClass<int> );
            var result = sut.Implements<IBaseGeneric<int>>();
            result.Should().BeTrue();
        }

        [Fact]
        public void Implements_ShouldReturnFalseWhenInterfaceIsNotImplemented()
        {
            var sut = typeof( GenericClass<int> );
            var result = sut.Implements<INotImplemented>();
            result.Should().BeFalse();
        }

        [Fact]
        public void Implements_ShouldReturnFalseWhenInterfaceIsOpenGeneric()
        {
            var sut = typeof( GenericClass<int> );
            var result = sut.Implements( typeof( IBaseGeneric<> ) );
            result.Should().BeFalse();
        }

        [Fact]
        public void GetOpenGenericImplementations_ShouldReturnCorrectResult()
        {
            var sut = typeof( GenericClass<int> );
            var expected = new[] { typeof( IBaseGeneric<int> ) }.AsEnumerable();
            var result = sut.GetOpenGenericImplementations( typeof( IBaseGeneric<> ) );
            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void ImplementsOpenGeneric_ShouldReturnTrue()
        {
            var sut = typeof( GenericClass<int> );
            var result = sut.ImplementsOpenGeneric( typeof( IBaseGeneric<> ) );
            result.Should().BeTrue();
        }

        [Fact]
        public void GetAllImplementedGenericDefinitions_ShouldReturnCorrectResult()
        {
            var sut = typeof( GenericClass<int> );
            var expected = new[] { typeof( IBaseGeneric<> ) }.AsEnumerable();
            var result = sut.GetAllImplementedGenericDefinitions();
            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void GetOpenGenericImplementations_ShouldReturnCorrectResult_ForMulti()
        {
            var sut = typeof( MultiGenericClass<int, string> );
            var expected = new[] { typeof( IBaseGeneric<int> ), typeof( IBaseGeneric<string> ) }.AsEnumerable();
            var result = sut.GetOpenGenericImplementations( typeof( IBaseGeneric<> ) );
            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void GetOpenGenericImplementations_ShouldReturnCorrectResult_ForMultiWithTheSameGenericArgs()
        {
            var sut = typeof( MultiGenericClass<int, int> );
            var expected = new[] { typeof( IBaseGeneric<int> ), typeof( IBaseGeneric<int> ) }.AsEnumerable();
            var result = sut.GetOpenGenericImplementations( typeof( IBaseGeneric<> ) );
            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void ImplementsOpenGeneric_ShouldReturnTrue_ForMulti()
        {
            var sut = typeof( MultiGenericClass<int, string> );
            var result = sut.ImplementsOpenGeneric( typeof( IBaseGeneric<> ) );
            result.Should().BeTrue();
        }

        [Fact]
        public void GetAllImplementedGenericDefinitions_ShouldReturnCorrectResult_ForMulti()
        {
            var sut = typeof( MultiGenericClass<int, string> );
            var expected = new[] { typeof( IBaseGeneric<> ) }.AsEnumerable();
            var result = sut.GetAllImplementedGenericDefinitions();
            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void GetExtension_ShouldReturnCorrectResultWhenTypeIsDirectParent()
        {
            var sut = typeof( GenericClass<int> );
            var result = sut.GetExtension<BaseGenericClass<int>>();
            result.Should().Be( typeof( BaseGenericClass<int> ) );
        }

        [Fact]
        public void GetExtension_ShouldReturnCorrectResultWhenTypeIsIndirectAncestor()
        {
            var sut = typeof( GenericClass<int> );
            var result = sut.GetExtension<BaseClass>();
            result.Should().Be( typeof( BaseClass ) );
        }

        [Fact]
        public void GetExtension_ShouldReturnNullWhenTypeIsNotAnAncestor()
        {
            var sut = typeof( GenericClass<int> );
            var result = sut.GetExtension<NotExtended>();
            result.Should().BeNull();
        }

        [Fact]
        public void GetExtension_ShouldReturnNullWhenTypeIsOpenGeneric()
        {
            var sut = typeof( GenericClass<int> );
            var result = sut.GetExtension( typeof( BaseGenericClass<> ) );
            result.Should().BeNull();
        }

        [Fact]
        public void GetExtension_ShouldReturnNullWhenTypeIsSelf()
        {
            var sut = typeof( GenericClass<int> );
            var result = sut.GetExtension<GenericClass<int>>();
            result.Should().BeNull();
        }

        [Fact]
        public void Extends_ShouldReturnTrueWhenTypeIsDirectParent()
        {
            var sut = typeof( GenericClass<int> );
            var result = sut.Extends<BaseGenericClass<int>>();
            result.Should().BeTrue();
        }

        [Fact]
        public void Extends_ShouldReturnTrueWhenTypeIsIndirectAncestor()
        {
            var sut = typeof( GenericClass<int> );
            var result = sut.Extends<BaseClass>();
            result.Should().BeTrue();
        }

        [Fact]
        public void Extends_ShouldReturnFalseWhenTypeIsNotAnAncestor()
        {
            var sut = typeof( GenericClass<int> );
            var result = sut.Extends<NotExtended>();
            result.Should().BeFalse();
        }

        [Fact]
        public void Extends_ShouldReturnFalseWhenInterfaceIsOpenGeneric()
        {
            var sut = typeof( GenericClass<int> );
            var result = sut.Extends( typeof( BaseGenericClass<> ) );
            result.Should().BeFalse();
        }

        [Fact]
        public void Extends_ShouldReturnFalseWhenTypeIsSelf()
        {
            var sut = typeof( GenericClass<int> );
            var result = sut.Extends<GenericClass<int>>();
            result.Should().BeFalse();
        }

        [Fact]
        public void GetOpenGenericExtension_ShouldReturnCorrectResult()
        {
            var sut = typeof( GenericClass<int> );
            var result = sut.GetOpenGenericExtension( typeof( BaseGenericClass<> ) );
            result.Should().Be( typeof( BaseGenericClass<int> ) );
        }

        [Fact]
        public void ExtendsOpenGeneric_ShouldReturnTrue()
        {
            var sut = typeof( GenericClass<int> );
            var result = sut.ExtendsOpenGeneric( typeof( BaseGenericClass<> ) );
            result.Should().BeTrue();
        }

        [Fact]
        public void GetAllExtendedGenericDefinitions_ShouldReturnCorrectResult()
        {
            var sut = typeof( GenericClass<int> );
            var expected = new[] { typeof( BaseGenericClass<> ) }.AsEnumerable();
            var result = sut.GetAllExtendedGenericDefinitions();
            result.Should().BeEquivalentTo( expected );
        }
    }
}
