﻿using FluentAssertions;
using LfrlSoft.NET.Common.Extensions;
using Xunit;

namespace LfrlSoft.NET.Common.Tests.Extensions.Type
{
    public class NonGenericInterfaceTests : TypeTestsBase
    {
        [Fact]
        public void GetImplementation_ShouldReturnCorrectResultWhenInterfaceIsDirectlyImplemented()
        {
            var sut = typeof( INonGenericInterface );
            var result = sut.GetImplementation<IDirect>();
            result.Should().Be( typeof( IDirect ) );
        }

        [Fact]
        public void GetImplementation_ShouldReturnCorrectResultWhenInterfaceIsImplementedThroughOtherInterface()
        {
            var sut = typeof( INonGenericInterface );
            var result = sut.GetImplementation<IIndirectFromInterface>();
            result.Should().Be( typeof( IIndirectFromInterface ) );
        }

        [Fact]
        public void GetImplementation_ShouldReturnNullWhenInterfaceIsNotImplemented()
        {
            var sut = typeof( INonGenericInterface );
            var result = sut.GetImplementation<INotImplemented>();
            result.Should().BeNull();
        }

        [Fact]
        public void GetImplementation_ShouldReturnNullWhenInterfaceIsSelf()
        {
            var sut = typeof( INonGenericInterface );
            var result = sut.GetImplementation<INonGenericInterface>();
            result.Should().BeNull();
        }

        [Fact]
        public void Implements_ShouldReturnTrueWhenInterfaceIsDirectlyImplemented()
        {
            var sut = typeof( INonGenericInterface );
            var result = sut.Implements<IDirect>();
            result.Should().BeTrue();
        }

        [Fact]
        public void Implements_ShouldReturnTrueWhenInterfaceIsImplementedThroughOtherInterface()
        {
            var sut = typeof( INonGenericInterface );
            var result = sut.Implements<IIndirectFromInterface>();
            result.Should().BeTrue();
        }

        [Fact]
        public void Implements_ShouldReturnFalseWhenInterfaceIsNotImplemented()
        {
            var sut = typeof( INonGenericInterface );
            var result = sut.Implements<INotImplemented>();
            result.Should().BeFalse();
        }

        [Fact]
        public void Implements_ShouldReturnFalseWhenInterfaceIsSelf()
        {
            var sut = typeof( INonGenericInterface );
            var result = sut.Implements<INonGenericInterface>();
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData( typeof( IBaseGeneric<> ) )]
        [InlineData( typeof( IDirect ) )]
        public void GetOpenGenericImplementations_ShouldReturnEmptyCollection(System.Type type)
        {
            var sut = typeof( INonGenericInterface );
            var result = sut.GetOpenGenericImplementations( type );
            result.Should().BeEmpty();
        }

        [Theory]
        [InlineData( typeof( IBaseGeneric<> ) )]
        [InlineData( typeof( IDirect ) )]
        public void ImplementsOpenGeneric_ShouldReturnFalse(System.Type type)
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
