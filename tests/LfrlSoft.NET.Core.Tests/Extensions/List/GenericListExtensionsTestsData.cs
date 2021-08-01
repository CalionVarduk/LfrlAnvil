using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using LfrlSoft.NET.TestExtensions;

namespace LfrlSoft.NET.Core.Tests.Extensions.List
{
    public class GenericListExtensionsTestsData<T>
    {
        public static IEnumerable<object[]> CreateSwapItemsTestData(IFixture fixture)
        {
            var source = fixture.CreateDistinctCollection<T>( 3 );

            return new[]
            {
                new object[] { source.ToList(), 0, 0, new[] { source[0], source[1], source[2] } },
                new object[] { source.ToList(), 0, 1, new[] { source[1], source[0], source[2] } },
                new object[] { source.ToList(), 0, 2, new[] { source[2], source[1], source[0] } },
                new object[] { source.ToList(), 1, 0, new[] { source[1], source[0], source[2] } },
                new object[] { source.ToList(), 1, 1, new[] { source[0], source[1], source[2] } },
                new object[] { source.ToList(), 1, 2, new[] { source[0], source[2], source[1] } },
                new object[] { source.ToList(), 2, 0, new[] { source[2], source[1], source[0] } },
                new object[] { source.ToList(), 2, 1, new[] { source[0], source[2], source[1] } },
                new object[] { source.ToList(), 2, 2, new[] { source[0], source[1], source[2] } }
            };
        }
    }
}
