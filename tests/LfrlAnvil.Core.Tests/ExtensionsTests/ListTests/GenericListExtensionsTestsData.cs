using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Tests.ExtensionsTests.ListTests
{
    public class GenericListExtensionsTestsData<T>
    {
        public static TheoryData<IList<T>, int, int, IReadOnlyList<T>> CreateSwapItemsTestData(IFixture fixture)
        {
            var source = fixture.CreateDistinctCollection<T>( 3 );

            return new TheoryData<IList<T>, int, int, IReadOnlyList<T>>
            {
                { source.ToList(), 0, 0, new[] { source[0], source[1], source[2] } },
                { source.ToList(), 0, 1, new[] { source[1], source[0], source[2] } },
                { source.ToList(), 0, 2, new[] { source[2], source[1], source[0] } },
                { source.ToList(), 1, 0, new[] { source[1], source[0], source[2] } },
                { source.ToList(), 1, 1, new[] { source[0], source[1], source[2] } },
                { source.ToList(), 1, 2, new[] { source[0], source[2], source[1] } },
                { source.ToList(), 2, 0, new[] { source[2], source[1], source[0] } },
                { source.ToList(), 2, 1, new[] { source[0], source[2], source[1] } },
                { source.ToList(), 2, 2, new[] { source[0], source[1], source[2] } }
            };
        }
    }
}
