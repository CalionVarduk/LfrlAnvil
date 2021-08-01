using System.Linq;
using FluentAssertions;
using LfrlSoft.NET.Core.Extensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Extensions.Type
{
    public class OpenGenericClassTests : TypeTestsBase
    {
        [Fact]
        public void GetImplementation_ShouldReturnCorrectResultWhenInterfaceIsDirectlyImplemented()
        {
            var sut = typeof( GenericClass<> );
            var result = sut.GetImplementation<IDirect>();
            result.Should().Be( typeof( IDirect ) );
        }

        [Fact]
        public void GetImplementation_ShouldReturnCorrectResultWhenInterfaceIsImplementedThroughOtherInterface()
        {
            var sut = typeof( GenericClass<> );
            var result = sut.GetImplementation<IIndirectFromInterface>();
            result.Should().Be( typeof( IIndirectFromInterface ) );
        }

        [Fact]
        public void GetImplementation_ShouldReturnCorrectResultWhenInterfaceIsImplementedThroughBaseType()
        {
            var sut = typeof( GenericClass<> );
            var result = sut.GetImplementation<IIndirectFromType>();
            result.Should().Be( typeof( IIndirectFromType ) );
        }

        [Fact]
        public void GetImplementation_ShouldReturnNullWhenInterfaceIsNotImplemented()
        {
            var sut = typeof( GenericClass<> );
            var result = sut.GetImplementation<INotImplemented>();
            result.Should().BeNull();
        }

        [Fact]
        public void GetImplementation_ShouldReturnNullWhenInterfaceIsOpenGeneric()
        {
            var sut = typeof( GenericClass<> );
            var result = sut.GetImplementation( typeof( IBaseGeneric<> ) );
            result.Should().BeNull();
        }

        [Fact]
        public void Implements_ShouldReturnTrueWhenInterfaceIsDirectlyImplemented()
        {
            var sut = typeof( GenericClass<> );
            var result = sut.Implements<IDirect>();
            result.Should().BeTrue();
        }

        [Fact]
        public void Implements_ShouldReturnTrueWhenInterfaceIsImplementedThroughOtherInterface()
        {
            var sut = typeof( GenericClass<> );
            var result = sut.Implements<IIndirectFromInterface>();
            result.Should().BeTrue();
        }

        [Fact]
        public void Implements_ShouldReturnTrueWhenInterfaceIsImplementedThroughBaseType()
        {
            var sut = typeof( GenericClass<> );
            var result = sut.Implements<IIndirectFromType>();
            result.Should().BeTrue();
        }

        [Fact]
        public void Implements_ShouldReturnFalseWhenInterfaceIsNotImplemented()
        {
            var sut = typeof( GenericClass<> );
            var result = sut.Implements<INotImplemented>();
            result.Should().BeFalse();
        }

        [Fact]
        public void Implements_ShouldReturnFalseWhenInterfaceIsOpenGeneric()
        {
            var sut = typeof( GenericClass<> );
            var result = sut.Implements( typeof( IBaseGeneric<> ) );
            result.Should().BeFalse();
        }

        [Fact]
        public void GetOpenGenericImplementations_ShouldReturnCorrectResult()
        {
            var sut = typeof( GenericClass<> );
            var expected = new[] { typeof( IBaseGeneric<> ).MakeGenericType( sut.GetGenericArguments()[0] ) }.AsEnumerable();
            var result = sut.GetOpenGenericImplementations( typeof( IBaseGeneric<> ) );
            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void ImplementsOpenGeneric_ShouldReturnTrue()
        {
            var sut = typeof( GenericClass<> );
            var result = sut.ImplementsOpenGeneric( typeof( IBaseGeneric<> ) );
            result.Should().BeTrue();
        }

        [Fact]
        public void GetAllImplementedGenericDefinitions_ShouldReturnCorrectResult()
        {
            var sut = typeof( GenericClass<> );
            var expected = new[] { typeof( IBaseGeneric<> ) }.AsEnumerable();
            var result = sut.GetAllImplementedGenericDefinitions();
            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void GetOpenGenericImplementations_ShouldReturnCorrectResult_ForMulti()
        {
            var sut = typeof( MultiGenericClass<,> );
            var expected = new[]
            {
                typeof( IBaseGeneric<> ).MakeGenericType( sut.GetGenericArguments()[0] ),
                typeof( IBaseGeneric<> ).MakeGenericType( sut.GetGenericArguments()[1] )
            }.AsEnumerable();

            var result = sut.GetOpenGenericImplementations( typeof( IBaseGeneric<> ) );
            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void ImplementsOpenGeneric_ShouldReturnTrue_ForMulti()
        {
            var sut = typeof( MultiGenericClass<,> );
            var result = sut.ImplementsOpenGeneric( typeof( IBaseGeneric<> ) );
            result.Should().BeTrue();
        }

        [Fact]
        public void GetAllImplementedGenericDefinitions_ShouldReturnCorrectResult_ForMulti()
        {
            var sut = typeof( MultiGenericClass<,> );
            var expected = new[] { typeof( IBaseGeneric<> ) }.AsEnumerable();
            var result = sut.GetAllImplementedGenericDefinitions();
            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void GetExtension_ShouldReturnCorrectResultWhenTypeIsDirectParent()
        {
            var sut = typeof( BaseGenericClass<> );
            var result = sut.GetExtension<BaseClass>();
            result.Should().Be( typeof( BaseClass ) );
        }

        [Fact]
        public void GetExtension_ShouldReturnCorrectResultWhenTypeIsIndirectAncestor()
        {
            var sut = typeof( GenericClass<> );
            var result = sut.GetExtension<BaseClass>();
            result.Should().Be( typeof( BaseClass ) );
        }

        [Fact]
        public void GetExtension_ShouldReturnNullWhenTypeIsNotAnAncestor()
        {
            var sut = typeof( GenericClass<> );
            var result = sut.GetExtension<NotExtended>();
            result.Should().BeNull();
        }

        [Fact]
        public void GetExtension_ShouldReturnNullWhenTypeIsOpenGeneric()
        {
            var sut = typeof( GenericClass<> );
            var result = sut.GetExtension( typeof( BaseGenericClass<> ) );
            result.Should().BeNull();
        }

        [Fact]
        public void Extends_ShouldReturnTrueWhenTypeIsDirectParent()
        {
            var sut = typeof( BaseGenericClass<> );
            var result = sut.Extends<BaseClass>();
            result.Should().BeTrue();
        }

        [Fact]
        public void Extends_ShouldReturnTrueWhenTypeIsIndirectAncestor()
        {
            var sut = typeof( GenericClass<> );
            var result = sut.Extends<BaseClass>();
            result.Should().BeTrue();
        }

        [Fact]
        public void Extends_ShouldReturnFalseWhenTypeIsNotAnAncestor()
        {
            var sut = typeof( GenericClass<> );
            var result = sut.Extends<NotExtended>();
            result.Should().BeFalse();
        }

        [Fact]
        public void Extends_ShouldReturnFalseWhenInterfaceIsOpenGeneric()
        {
            var sut = typeof( GenericClass<> );
            var result = sut.Extends( typeof( BaseGenericClass<> ) );
            result.Should().BeFalse();
        }

        [Fact]
        public void GetOpenGenericExtension_ShouldReturnCorrectResult()
        {
            var sut = typeof( GenericClass<> );
            var result = sut.GetOpenGenericExtension( typeof( BaseGenericClass<> ) );
            result.Should().Be( typeof( BaseGenericClass<> ).MakeGenericType( sut.GetGenericArguments()[0] ) );
        }

        [Fact]
        public void ExtendsOpenGeneric_ShouldReturnTrue()
        {
            var sut = typeof( GenericClass<> );
            var result = sut.ExtendsOpenGeneric( typeof( BaseGenericClass<> ) );
            result.Should().BeTrue();
        }

        [Fact]
        public void GetAllExtendedGenericDefinitions_ShouldReturnCorrectResult()
        {
            var sut = typeof( GenericClass<> );
            var expected = new[] { typeof( BaseGenericClass<> ) }.AsEnumerable();
            var result = sut.GetAllExtendedGenericDefinitions();
            result.Should().BeEquivalentTo( expected );
        }
    }
}
