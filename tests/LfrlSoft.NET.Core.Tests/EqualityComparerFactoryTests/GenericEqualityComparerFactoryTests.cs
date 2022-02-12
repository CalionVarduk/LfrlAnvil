using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.EqualityComparerFactoryTests
{
    public abstract class GenericEqualityComparerFactoryTests<T> : TestsBase
    {
        [Fact]
        public void CreateDefault_ShouldReturnDefaultComparer()
        {
            var sut = EqualityComparerFactory<T>.CreateDefault();
            sut.Should().Be( EqualityComparer<T>.Default );
        }

        [Fact]
        public void Create_ShouldCreateComparerWithCorrectEqualsImplementation()
        {
            var obj1 = Fixture.Create<T>();
            var obj2 = Fixture.Create<T>();

            var defaultComparer = EqualityComparer<T>.Default;
            var customComparerCalled = false;

            var sut = EqualityComparerFactory<T>.Create(
                (a, b) =>
                {
                    customComparerCalled = true;
                    return defaultComparer.Equals( a, b );
                } );

            var result = sut.Equals( obj1, obj2 );

            using ( new AssertionScope() )
            {
                customComparerCalled.Should().BeTrue();
                result.Should().Be( defaultComparer.Equals( obj1, obj2 ) );
            }
        }

        [Fact]
        public void Create_ShouldCreateComparerWithCorrectDefaultGetHashCodeImplementation()
        {
            var obj = Fixture.CreateNotDefault<T>();

            var defaultComparer = EqualityComparer<T>.Default;

            var sut = EqualityComparerFactory<T>.Create(
                (a, b) => defaultComparer.Equals( a, b ) );

            var result = sut.GetHashCode( obj! );

            result.Should().Be( obj!.GetHashCode() );
        }

        [Fact]
        public void Create_ShouldCreateComparerWithCorrectExplicitGetHashCodeImplementation()
        {
            var obj = Fixture.CreateNotDefault<T>();
            var expected = Fixture.Create<int>();

            var defaultComparer = EqualityComparer<T>.Default;

            int HashCodeCalculator(T _)
            {
                return expected;
            }

            var sut = EqualityComparerFactory<T>.Create(
                (a, b) => defaultComparer.Equals( a, b ),
                HashCodeCalculator );

            var result = sut.GetHashCode( obj! );

            result.Should().Be( expected );
        }
    }
}
