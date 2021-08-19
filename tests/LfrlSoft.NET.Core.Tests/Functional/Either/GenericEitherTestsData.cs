using System.Collections.Generic;
using AutoFixture;
using LfrlSoft.NET.TestExtensions;

namespace LfrlSoft.NET.Core.Tests.Functional.Either
{
    public class GenericEitherTestsData<T1, T2>
    {
        public static IEnumerable<object?[]> CreateEqualsTestData(IFixture fixture)
        {
            var (_11, _12) = fixture.CreateDistinctCollection<T1>( 2 );
            var (_21, _22) = fixture.CreateDistinctCollection<T2>( 2 );

            return new[]
            {
                new object?[] { _11, true, _11, true, true },
                new object?[] { _11, true, _12, true, false },
                new object?[] { _21, false, _21, false, true },
                new object?[] { _21, false, _22, false, false },
                new object?[] { _11, true, _21, false, false },
                new object?[] { _21, false, _11, true, false }
            };
        }

        public static IEnumerable<object?[]> CreateNotEqualsTestData(IFixture fixture)
        {
            return CreateEqualsTestData( fixture ).ConvertResult( (bool r) => ! r );
        }
    }
}
