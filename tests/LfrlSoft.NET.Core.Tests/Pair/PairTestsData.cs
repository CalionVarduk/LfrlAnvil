using System.Collections.Generic;
using AutoFixture;
using LfrlSoft.NET.TestExtensions;

namespace LfrlSoft.NET.Core.Tests.Pair
{
    public class PairTestsData<T1, T2>
    {
        public static IEnumerable<object?[]> CreateEqualsTestData(IFixture fixture)
        {
            var (f1, f2) = fixture.CreateDistinctCollection<T1>( 2 );
            var (s1, s2) = fixture.CreateDistinctCollection<T2>( 2 );

            return new[]
            {
                new object?[] { f1, s1, f1, s1, true },
                new object?[] { f1, s1, f1, s2, false },
                new object?[] { f1, s1, f2, s1, false },
                new object?[] { f1, s1, f2, s2, false }
            };
        }

        public static IEnumerable<object?[]> CreateNotEqualsTestData(IFixture fixture)
        {
            return CreateEqualsTestData( fixture ).ConvertResult( (bool r) => ! r );
        }
    }
}
