using System;
using System.Collections.Generic;
using AutoFixture;
using LfrlSoft.NET.TestExtensions;

namespace LfrlSoft.NET.Core.Tests.Functional.Unsafe
{
    public class GenericUnsafeTestsData<T>
    {
        public static IEnumerable<object?[]> CreateEqualsTestData(IFixture fixture)
        {
            var (_11, _12) = fixture.CreateDistinctCollection<T>( 2 );
            var (e1, e2) = (new Exception(), new Exception());

            return new[]
            {
                new object?[] { _11, true, _11, true, true },
                new object?[] { _11, true, _12, true, false },
                new object?[] { e1, false, e1, false, true },
                new object?[] { e1, false, e2, false, false },
                new object?[] { _11, true, e1, false, false },
                new object?[] { e1, false, _11, true, false }
            };
        }

        public static IEnumerable<object?[]> CreateNotEqualsTestData(IFixture fixture)
        {
            return CreateEqualsTestData( fixture ).ConvertResult( (bool r) => ! r );
        }
    }
}
