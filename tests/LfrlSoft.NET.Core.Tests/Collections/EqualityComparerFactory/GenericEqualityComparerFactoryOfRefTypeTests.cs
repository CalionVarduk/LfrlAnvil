using System.Collections.Generic;
using FluentAssertions;
using LfrlSoft.NET.Core.Collections;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Collections.EqualityComparerFactory
{
    public abstract class GenericEqualityComparerFactoryOfRefTypeTests<T> : GenericEqualityComparerFactoryTests<T?>
        where T : class
    {
        [Fact]
        public void Create_ShouldCreateComparerWithCorrectDefaultGetHashCodeImplementation_ForNullObject()
        {
            T? obj = null;

            var defaultComparer = EqualityComparer<T?>.Default;

            var sut = EqualityComparerFactory<T?>.Create(
                (a, b) => defaultComparer.Equals( a, b ) );

            var result = sut.GetHashCode( obj! );

            result.Should().Be( 0 );
        }
    }
}
