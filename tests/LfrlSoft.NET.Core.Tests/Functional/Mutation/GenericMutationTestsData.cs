using System.Collections.Generic;
using AutoFixture;
using LfrlSoft.NET.TestExtensions;

namespace LfrlSoft.NET.Core.Tests.Functional.Mutation
{
    public class GenericMutationTestsData<T>
    {
        public static IEnumerable<object?[]> CreateEqualsTestData(IFixture fixture)
        {
            var (old1, old2, new1, new2) = fixture.CreateDistinctCollection<T>( 4 );

            return new[]
            {
                new object?[] { old1, new1, old1, new1, true },
                new object?[] { old1, new1, old1, new2, false },
                new object?[] { old1, new1, old2, new1, false },
                new object?[] { old1, new1, old2, new2, false }
            };
        }

        public static IEnumerable<object?[]> CreateNotEqualsTestData(IFixture fixture)
        {
            return CreateEqualsTestData( fixture ).ConvertResult( (bool r) => ! r );
        }
    }
}
