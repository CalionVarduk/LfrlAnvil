using System.Linq;
using FluentAssertions;
using LfrlAnvil.Collections.Extensions;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Collections.Tests.ExtensionsTests.EnumerableTests
{
    public abstract class GenericEnumerableExtensionsOfNotNullTypeTests<T> : TestsBase
        where T : notnull
    {
        [Fact]
        public void ToMultiSet_ShouldReturnCorrectResult()
        {
            var distinctItems = Fixture.CreateDistinctCollection<T>( 5 );
            var items = distinctItems.SelectMany( i => new[] { i, i, i, i } ).ToList();
            var expected = distinctItems.Select( i => Pair.Create( i, 4 ) ).ToList();

            var result = items.ToMultiSet();

            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void ToMultiSet_ShouldReturnCorrectResult_WithExplicitComparer()
        {
            var comparer = EqualityComparerFactory<T>.Create( (a, b) => a!.Equals( b ) );

            var distinctItems = Fixture.CreateDistinctCollection<T>( 5 );
            var items = distinctItems.SelectMany( i => new[] { i, i, i, i } ).ToList();
            var expected = distinctItems.Select( i => Pair.Create( i, 4 ) ).ToList();

            var result = items.ToMultiSet( comparer );

            result.Should().BeEquivalentTo( expected );
        }
    }
}
