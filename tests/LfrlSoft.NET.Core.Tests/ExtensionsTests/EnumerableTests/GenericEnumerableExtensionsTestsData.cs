using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.ExtensionsTests.EnumerableTests
{
    public class GenericEnumerableExtensionsTestsData<T>
    {
        public static TheoryData<int> GetIsEmptyData(IFixture fixture)
        {
            return new TheoryData<int>
            {
                1,
                3
            };
        }

        public static TheoryData<int, int, bool> GetContainsAtLeastData(IFixture fixture)
        {
            return new TheoryData<int, int, bool>
            {
                { 0, -1, true },
                { 0, 0, true },
                { 0, 1, false },
                { 1, -1, true },
                { 1, 0, true },
                { 1, 1, true },
                { 1, 2, false },
                { 3, -1, true },
                { 3, 0, true },
                { 3, 1, true },
                { 3, 2, true },
                { 3, 3, true },
                { 3, 4, false }
            };
        }

        public static TheoryData<int, int, bool> GetContainsAtMostData(IFixture fixture)
        {
            return new TheoryData<int, int, bool>
            {
                { 0, -1, false },
                { 0, 0, true },
                { 0, 1, true },
                { 1, -1, false },
                { 1, 0, false },
                { 1, 1, true },
                { 1, 2, true },
                { 3, -1, false },
                { 3, 0, false },
                { 3, 1, false },
                { 3, 2, false },
                { 3, 3, true },
                { 3, 4, true }
            };
        }

        public static TheoryData<int> GetContainsInRangeForMaxCountLessThanMinCountData(IFixture fixture)
        {
            return new TheoryData<int>
            {
                0,
                1,
                3
            };
        }

        public static TheoryData<int, int, bool> GetContainsInRangeForZeroMinCountData(IFixture fixture)
        {
            return new TheoryData<int, int, bool>
            {
                { 0, -1, false },
                { 0, 0, true },
                { 0, 1, true },
                { 1, -1, false },
                { 1, 0, false },
                { 1, 1, true },
                { 1, 2, true },
                { 3, -1, false },
                { 3, 0, false },
                { 3, 1, false },
                { 3, 2, false },
                { 3, 3, true },
                { 3, 4, true }
            };
        }

        public static TheoryData<int, int, bool> GetContainsInRangeForNegativeMinCountData(IFixture fixture)
        {
            return new TheoryData<int, int, bool>
            {
                { 0, -1, false },
                { 0, 0, true },
                { 0, 1, true },
                { 1, -1, false },
                { 1, 0, false },
                { 1, 1, true },
                { 1, 2, true },
                { 3, -1, false },
                { 3, 0, false },
                { 3, 1, false },
                { 3, 2, false },
                { 3, 3, true },
                { 3, 4, true }
            };
        }

        public static TheoryData<int, int> GetContainsInRangeForCountLessThanMinCountData(IFixture fixture)
        {
            return new TheoryData<int, int>
            {
                { 0, 1 },
                { 0, 2 },
                { 1, 2 },
                { 1, 3 },
                { 3, 4 },
                { 3, 5 }
            };
        }

        public static TheoryData<int, int> GetContainsInRangeForCountGreaterThanMaxCountData(IFixture fixture)
        {
            return new TheoryData<int, int>
            {
                { 3, 2 },
                { 4, 3 },
                { 4, 2 },
                { 5, 4 },
                { 5, 3 }
            };
        }

        public static TheoryData<int, int, int> GetContainsInRangeForCountBetweenMinAndMaxData(IFixture fixture)
        {
            return new TheoryData<int, int, int>
            {
                { 1, 1, 1 },
                { 1, 1, 2 },
                { 1, 1, 3 },
                { 3, 1, 3 },
                { 3, 1, 4 },
                { 3, 1, 5 },
                { 3, 2, 3 },
                { 3, 2, 4 },
                { 3, 2, 5 },
                { 3, 3, 3 },
                { 3, 3, 4 },
                { 3, 3, 5 }
            };
        }

        public static TheoryData<int> GetContainsExactlyForNegativeCountData(IFixture fixture)
        {
            return new TheoryData<int>
            {
                0,
                1,
                3
            };
        }

        public static TheoryData<int, int, bool> GetContainsExactlyForNonNegativeCountData(IFixture fixture)
        {
            return new TheoryData<int, int, bool>
            {
                { 0, 0, true },
                { 0, 1, false },
                { 1, 0, false },
                { 1, 1, true },
                { 1, 2, false },
                { 3, 2, false },
                { 3, 3, true },
                { 3, 4, false }
            };
        }

        public static TheoryData<IReadOnlyList<Pair<T, IEnumerable<T>>>, IEnumerable<Pair<T, T>>> GetFlattenData(IFixture fixture)
        {
            var (_1, _2, _3) = fixture.CreateDistinctCollection<T>( 3 );

            return new TheoryData<IReadOnlyList<Pair<T, IEnumerable<T>>>, IEnumerable<Pair<T, T>>>
            {
                { Array.Empty<Pair<T, IEnumerable<T>>>(), Array.Empty<Pair<T, T>>() },
                { new[] { new Pair<T, IEnumerable<T>>( _1, new[] { _2 } ) }, new[] { Pair.Create( _1, _2 ) } },
                {
                    new[] { new Pair<T, IEnumerable<T>>( _1, new[] { _2, _3 } ), new Pair<T, IEnumerable<T>>( _2, new[] { _1, _3 } ) },
                    new[]
                    {
                        Pair.Create( _1, _2 ),
                        Pair.Create( _1, _3 ),
                        Pair.Create( _2, _1 ),
                        Pair.Create( _2, _3 )
                    }
                }
            };
        }

        public static TheoryData<IEnumerable<T>, T> GetMinData(IFixture fixture)
        {
            var (_1, _2, _3) = fixture.CreateDistinctSortedCollection<T>( 3 );

            return new TheoryData<IEnumerable<T>, T>
            {
                { new[] { _1 }, _1 },
                { new[] { _1, _2 }, _1 },
                { new[] { _2, _1 }, _1 },
                { new[] { _1, _1 }, _1 },
                { new[] { _1, _2, _3 }, _1 },
                { new[] { _1, _3, _2 }, _1 },
                { new[] { _3, _1, _2 }, _1 },
                { new[] { _3, _2, _1 }, _1 },
                { new[] { _1, _1, _1 }, _1 }
            };
        }

        public static TheoryData<IEnumerable<T>, T> GetMaxData(IFixture fixture)
        {
            var (_1, _2, _3) = fixture.CreateDistinctSortedCollection<T>( 3 );

            return new TheoryData<IEnumerable<T>, T>
            {
                { new[] { _1 }, _1 },
                { new[] { _1, _2 }, _2 },
                { new[] { _2, _1 }, _2 },
                { new[] { _1, _1 }, _1 },
                { new[] { _1, _2, _3 }, _3 },
                { new[] { _1, _3, _2 }, _3 },
                { new[] { _3, _1, _2 }, _3 },
                { new[] { _3, _2, _1 }, _3 },
                { new[] { _1, _1, _1 }, _1 }
            };
        }

        public static TheoryData<IEnumerable<T>, bool> GetContainsDuplicatesData(IFixture fixture)
        {
            var (_1, _2, _3) = fixture.CreateDistinctCollection<T>( 3 );

            return new TheoryData<IEnumerable<T>, bool>
            {
                { new[] { _1 }, false },
                { new[] { _1, _1 }, true },
                { new[] { _1, _2 }, false },
                { new[] { _1, _1, _1 }, true },
                { new[] { _1, _1, _2 }, true },
                { new[] { _1, _2, _2 }, true },
                { new[] { _1, _2, _3 }, false },
                { new[] { _1, _1, _2, _3 }, true },
                { new[] { _1, _2, _2, _3 }, true },
                { new[] { _1, _2, _3, _3 }, true }
            };
        }

        public static TheoryData<int> GetRepeatForZeroOrOneCountData(IFixture fixture)
        {
            return new TheoryData<int>
            {
                0,
                1,
                3
            };
        }

        public static TheoryData<IEnumerable<T>, int, IEnumerable<T>> GetRepeatForCountGreaterThanOneData(IFixture fixture)
        {
            var (_1, _2, _3) = fixture.CreateDistinctCollection<T>( 3 );

            return new TheoryData<IEnumerable<T>, int, IEnumerable<T>>
            {
                { Array.Empty<T>(), 2, Array.Empty<T>() },
                { Array.Empty<T>(), 5, Array.Empty<T>() },
                { new[] { _1 }, 2, new[] { _1, _1 } },
                { new[] { _1 }, 5, new[] { _1, _1, _1, _1, _1 } },
                { new[] { _1, _2, _3 }, 2, new[] { _1, _2, _3, _1, _2, _3 } },
                { new[] { _1, _2, _3 }, 5, new[] { _1, _2, _3, _1, _2, _3, _1, _2, _3, _1, _2, _3, _1, _2, _3 } }
            };
        }

        public static TheoryData<int> GetRepeatForMemoizationWithCountGreaterThanOneData(IFixture fixture)
        {
            return new TheoryData<int>
            {
                2,
                3,
                5
            };
        }

        public static TheoryData<int, int> GetMemoizeData(IFixture fixture)
        {
            return new TheoryData<int, int>
            {
                { 0, 0 },
                { 0, 1 },
                { 0, 3 },
                { 0, 5 },
                { 1, 0 },
                { 1, 1 },
                { 1, 3 },
                { 1, 5 },
                { 3, 0 },
                { 3, 1 },
                { 3, 3 },
                { 3, 5 }
            };
        }

        public static TheoryData<IEnumerable<T>, IEnumerable<T>, bool> GetSetEqualsData(IFixture fixture)
        {
            var (_1, _2, _3) = fixture.CreateDistinctCollection<T>( 3 );

            return new TheoryData<IEnumerable<T>, IEnumerable<T>, bool>
            {
                { Array.Empty<T>(), Array.Empty<T>(), true },
                { Array.Empty<T>(), new[] { _1 }, false },
                { new[] { _1 }, Array.Empty<T>(), false },
                { new[] { _1 }, new[] { _1 }, true },
                { new[] { _1, _1 }, new[] { _1 }, true },
                { new[] { _1 }, new[] { _1, _1 }, true },
                { new[] { _1, _1 }, new[] { _1, _1 }, true },
                { new[] { _1 }, new[] { _2 }, false },
                { new[] { _1 }, new[] { _1, _2 }, false },
                { new[] { _1 }, new[] { _2, _1 }, false },
                { new[] { _1, _2, _3 }, Array.Empty<T>(), false },
                { new[] { _1, _2, _3 }, new[] { _1, _2, _3 }, true },
                { new[] { _1, _3, _2 }, new[] { _2, _1, _3 }, true },
                { new[] { _1, _1, _2, _2, _3, _3 }, new[] { _1, _2, _3 }, true },
                { new[] { _3, _2, _1 }, new[] { _1, _2, _3, _1, _2, _3 }, true },
                { new[] { _1, _1, _2, _2, _3, _3 }, new[] { _3, _2, _1, _3, _2, _1 }, true },
                { new[] { _1, _1, _2, _2, _3, _3 }, new[] { _3, _2, _1, _3, _2, _1 }, true },
                { new[] { _1, _2, _3 }, new[] { _1, _2 }, false },
                { new[] { _1, _2, _3 }, new[] { _3, _1 }, false },
                { new[] { _1, _2, _3 }, new[] { _2 }, false },
                { new[] { _1, _2 }, new[] { _2, _3 }, false }
            };
        }

        public static TheoryData<IEnumerable<T>, IEnumerable<T>> GetDistinctData(IFixture fixture)
        {
            var (_1, _2, _3) = fixture.CreateDistinctSortedCollection<T>( 3 );

            return new TheoryData<IEnumerable<T>, IEnumerable<T>>
            {
                { new[] { _1 }, new[] { _1 } },
                { new[] { _1, _2 }, new[] { _1, _2 } },
                { new[] { _2, _1 }, new[] { _2, _1 } },
                { new[] { _1, _1 }, new[] { _1 } },
                { new[] { _1, _2, _3 }, new[] { _1, _2, _3 } },
                { new[] { _1, _3, _2 }, new[] { _1, _3, _2 } },
                { new[] { _3, _1, _2 }, new[] { _3, _1, _2 } },
                { new[] { _3, _2, _1 }, new[] { _3, _2, _1 } },
                { new[] { _1, _1, _1 }, new[] { _1 } },
                { new[] { _1, _1, _3, _3, _2, _2 }, new[] { _1, _3, _2 } }
            };
        }

        public static TheoryData<IEnumerable<T>, bool> GetIsOrderedData(IFixture fixture)
        {
            var (_1, _2, _3) = fixture.CreateDistinctSortedCollection<T>( 3 );

            return new TheoryData<IEnumerable<T>, bool>
            {
                { new[] { _1, _2 }, true },
                { new[] { _1, _1 }, true },
                { new[] { _1, _1, _2 }, true },
                { new[] { _1, _2, _2 }, true },
                { new[] { _1, _2, _3 }, true },
                { new[] { _2, _1 }, false },
                { new[] { _2, _2, _1 }, false },
                { new[] { _2, _1, _1 }, false },
                { new[] { _3, _2, _1 }, false },
                { new[] { _1, _3, _2 }, false },
                { new[] { _2, _1, _3 }, false }
            };
        }

        public static TheoryData<int> GetDivideForDivisibleSourceCountData(IFixture fixture)
        {
            return new TheoryData<int>
            {
                1,
                2,
                3,
                4,
                6,
                12
            };
        }

        public static TheoryData<int, int> GetDivideForNonDivisibleSourceCountData(IFixture fixture)
        {
            return new TheoryData<int, int>
            {
                { 5, 2 },
                { 7, 5 },
                { 8, 4 },
                { 9, 3 },
                { 10, 2 },
                { 11, 1 },
                { 13, 12 },
                { 24, 12 }
            };
        }

        public static TheoryData<int> GetDivideThrowData(IFixture fixture)
        {
            return new TheoryData<int>
            {
                0,
                -1
            };
        }
    }

    public sealed class VisitManyNode<T>
    {
        public T? Value { get; init; }
        public List<VisitManyNode<T>> Children { get; init; } = new List<VisitManyNode<T>>();

        public override string ToString()
        {
            return $"{{{Value}}} -> {{{string.Join( ", ", Children )}}}";
        }
    }

    public sealed class Contained<T>
    {
        public T? Value { get; init; }

        public override string ToString()
        {
            return $"{{{Value}}}";
        }
    }

    public sealed class TestCollection<T> : IReadOnlyCollection<T>
    {
        public int Count => 0;

        public IEnumerator<T> GetEnumerator()
        {
            return Enumerable.Empty<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
