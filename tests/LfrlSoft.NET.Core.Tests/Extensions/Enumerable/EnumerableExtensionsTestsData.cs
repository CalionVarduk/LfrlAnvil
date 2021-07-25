using System;
using System.Collections.Generic;
using AutoFixture;
using LfrlSoft.NET.TestExtensions;

namespace LfrlSoft.NET.Core.Tests.Extensions.Enumerable
{
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

    public class EnumerableExtensionsTestsData<T>
    {
        public static IEnumerable<object?[]> CreateFlattenTestData(IFixture fixture)
        {
            var (_1, _2, _3) = fixture.CreateDistinctCollection<T>( 3 );

            return new[]
            {
                new object?[] { Array.Empty<Pair<T, IEnumerable<T>>>(), Array.Empty<Pair<T, T>>() },
                new object?[]
                {
                    new[] { new Pair<T, IEnumerable<T>>( _1, new[] { _2 } ) }, new[] { Core.Pair.Create( _1, _2 ) }
                },
                new object?[]
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

        public static IEnumerable<object?[]> CreateMinTestData(IFixture fixture)
        {
            var (_1, _2, _3) = fixture.CreateDistinctSortedCollection<T>( 3 );

            return new[]
            {
                new object?[] { new[] { _1 }, _1 },
                new object?[] { new[] { _1, _2 }, _1 },
                new object?[] { new[] { _2, _1 }, _1 },
                new object?[] { new[] { _1, _1 }, _1 },
                new object?[] { new[] { _1, _2, _3 }, _1 },
                new object?[] { new[] { _1, _3, _2 }, _1 },
                new object?[] { new[] { _3, _1, _2 }, _1 },
                new object?[] { new[] { _3, _2, _1 }, _1 },
                new object?[] { new[] { _1, _1, _1 }, _1 }
            };
        }

        public static IEnumerable<object?[]> CreateMaxTestData(IFixture fixture)
        {
            var (_1, _2, _3) = fixture.CreateDistinctSortedCollection<T>( 3 );

            return new[]
            {
                new object?[] { new[] { _1 }, _1 },
                new object?[] { new[] { _1, _2 }, _2 },
                new object?[] { new[] { _2, _1 }, _2 },
                new object?[] { new[] { _1, _1 }, _1 },
                new object?[] { new[] { _1, _2, _3 }, _3 },
                new object?[] { new[] { _1, _3, _2 }, _3 },
                new object?[] { new[] { _3, _1, _2 }, _3 },
                new object?[] { new[] { _3, _2, _1 }, _3 },
                new object?[] { new[] { _1, _1, _1 }, _1 }
            };
        }

        public static IEnumerable<object?[]> CreateContainsDuplicatesTestData(IFixture fixture)
        {
            var (_1, _2, _3) = fixture.CreateDistinctCollection<T>( 3 );

            return new[]
            {
                new object?[] { new[] { _1 }, false },
                new object?[] { new[] { _1, _1 }, true },
                new object?[] { new[] { _1, _2 }, false },
                new object?[] { new[] { _1, _1, _1 }, true },
                new object?[] { new[] { _1, _1, _2 }, true },
                new object?[] { new[] { _1, _2, _2 }, true },
                new object?[] { new[] { _1, _2, _3 }, false },
                new object?[] { new[] { _1, _1, _2, _3 }, true },
                new object?[] { new[] { _1, _2, _2, _3 }, true },
                new object?[] { new[] { _1, _2, _3, _3 }, true }
            };
        }

        public static IEnumerable<object?[]> CreateRepeatTestData(IFixture fixture)
        {
            var (_1, _2, _3) = fixture.CreateDistinctCollection<T>( 3 );

            return new[]
            {
                new object?[] { Array.Empty<T>(), 2, Array.Empty<T>() },
                new object?[] { Array.Empty<T>(), 5, Array.Empty<T>() },
                new object?[] { new[] { _1 }, 2, new[] { _1, _1 } },
                new object?[] { new[] { _1 }, 5, new[] { _1, _1, _1, _1, _1 } },
                new object?[] { new[] { _1, _2, _3 }, 2, new[] { _1, _2, _3, _1, _2, _3 } },
                new object?[] { new[] { _1, _2, _3 }, 5, new[] { _1, _2, _3, _1, _2, _3, _1, _2, _3, _1, _2, _3, _1, _2, _3 } }
            };
        }

        public static IEnumerable<object?[]> CreateSetEqualsTestData(IFixture fixture)
        {
            var (_1, _2, _3) = fixture.CreateDistinctCollection<T>( 3 );

            return new[]
            {
                new object?[] { Array.Empty<T>(), Array.Empty<T>(), true },
                new object?[] { Array.Empty<T>(), new[] { _1 }, false },
                new object?[] { new[] { _1 }, Array.Empty<T>(), false },
                new object?[] { new[] { _1 }, new[] { _1 }, true },
                new object?[] { new[] { _1, _1 }, new[] { _1 }, true },
                new object?[] { new[] { _1 }, new[] { _1, _1 }, true },
                new object?[] { new[] { _1, _1 }, new[] { _1, _1 }, true },
                new object?[] { new[] { _1 }, new[] { _2 }, false },
                new object?[] { new[] { _1 }, new[] { _1, _2 }, false },
                new object?[] { new[] { _1 }, new[] { _2, _1 }, false },
                new object?[] { new[] { _1, _2, _3 }, Array.Empty<T>(), false },
                new object?[] { new[] { _1, _2, _3 }, new[] { _1, _2, _3 }, true },
                new object?[] { new[] { _1, _3, _2 }, new[] { _2, _1, _3 }, true },
                new object?[] { new[] { _1, _1, _2, _2, _3, _3 }, new[] { _1, _2, _3 }, true },
                new object?[] { new[] { _3, _2, _1 }, new[] { _1, _2, _3, _1, _2, _3 }, true },
                new object?[] { new[] { _1, _1, _2, _2, _3, _3 }, new[] { _3, _2, _1, _3, _2, _1 }, true },
                new object?[] { new[] { _1, _1, _2, _2, _3, _3 }, new[] { _3, _2, _1, _3, _2, _1 }, true },
                new object?[] { new[] { _1, _2, _3 }, new[] { _1, _2 }, false },
                new object?[] { new[] { _1, _2, _3 }, new[] { _3, _1 }, false },
                new object?[] { new[] { _1, _2, _3 }, new[] { _2 }, false }
            };
        }

        public static IEnumerable<object?[]> CreateDistinctTestData(IFixture fixture)
        {
            var (_1, _2, _3) = fixture.CreateDistinctSortedCollection<T>( 3 );

            return new[]
            {
                new object?[] { new[] { _1 }, new[] { _1 } },
                new object?[] { new[] { _1, _2 }, new[] { _1, _2 } },
                new object?[] { new[] { _2, _1 }, new[] { _2, _1 } },
                new object?[] { new[] { _1, _1 }, new[] { _1 } },
                new object?[] { new[] { _1, _2, _3 }, new[] { _1, _2, _3 } },
                new object?[] { new[] { _1, _3, _2 }, new[] { _1, _3, _2 } },
                new object?[] { new[] { _3, _1, _2 }, new[] { _3, _1, _2 } },
                new object?[] { new[] { _3, _2, _1 }, new[] { _3, _2, _1 } },
                new object?[] { new[] { _1, _1, _1 }, new[] { _1 } },
                new object?[] { new[] { _1, _1, _3, _3, _2, _2 }, new[] { _1, _3, _2 } }
            };
        }
    }
}
