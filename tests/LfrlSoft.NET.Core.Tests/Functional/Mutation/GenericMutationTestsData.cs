using System.Collections.Generic;
using AutoFixture;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Functional.Mutation
{
    public class GenericMutationTestsData<T>
    {
        public static TheoryData<T, T, T, T, bool> CreateEqualsTestData(IFixture fixture)
        {
            var (old1, old2, new1, new2) = fixture.CreateDistinctCollection<T>( 4 );

            return new TheoryData<T, T, T, T, bool>
            {
                { old1, new1, old1, new1, true },
                { old1, new1, old1, new2, false },
                { old1, new1, old2, new1, false },
                { old1, new1, old2, new2, false }
            };
        }

        public static IEnumerable<object?[]> CreateNotEqualsTestData(IFixture fixture)
        {
            return CreateEqualsTestData( fixture ).ConvertResult( (bool r) => ! r );
        }
    }
}
