using System;
using System.Collections.Generic;
using AutoFixture;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Extensions.Enumerable
{
    public class GenericEnumerableExtensionsTestsData<T>
    {
        public static TheoryData<IEnumerable<Pair<T, IEnumerable<T>>>, IEnumerable<Pair<T, T>>> CreateFlattenTestData(IFixture fixture)
        {
            var (_1, _2, _3) = fixture.CreateDistinctCollection<T>( 3 );

            return new TheoryData<IEnumerable<Pair<T, IEnumerable<T>>>, IEnumerable<Pair<T, T>>>
            {
                { Array.Empty<Pair<T, IEnumerable<T>>>(), Array.Empty<Pair<T, T>>() },
                { new[] { new Pair<T, IEnumerable<T>>( _1, new[] { _2 } ) }, new[] { Core.Pair.Create( _1, _2 ) } },
                {
                    new[] { new Pair<T, IEnumerable<T>>( _1, new[] { _2, _3 } ), new Pair<T, IEnumerable<T>>( _2, new[] { _1, _3 } ) },
                    new[]
                    {
                        Core.Pair.Create( _1, _2 ),
                        Core.Pair.Create( _1, _3 ),
                        Core.Pair.Create( _2, _1 ),
                        Core.Pair.Create( _2, _3 )
                    }
                }
            };
        }

        public static TheoryData<IEnumerable<T>, T> CreateMinTestData(IFixture fixture)
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

        public static TheoryData<IEnumerable<T>, T> CreateMaxTestData(IFixture fixture)
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

        public static TheoryData<IEnumerable<T>, bool> CreateContainsDuplicatesTestData(IFixture fixture)
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

        public static TheoryData<IEnumerable<T>, int, IEnumerable<T>> CreateRepeatTestData(IFixture fixture)
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

        public static TheoryData<IEnumerable<T>, IEnumerable<T>, bool> CreateSetEqualsTestData(IFixture fixture)
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

        public static TheoryData<IEnumerable<T>, IEnumerable<T>> CreateDistinctTestData(IFixture fixture)
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
}
