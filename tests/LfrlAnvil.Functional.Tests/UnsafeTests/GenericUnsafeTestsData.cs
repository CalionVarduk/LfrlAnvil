using System;
using System.Collections.Generic;
using AutoFixture;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Functional.Tests.UnsafeTests;

public class GenericUnsafeTestsData<T>
{
    public static TheoryData<object, bool, object, bool, bool> CreateEqualsTestData(IFixture fixture)
    {
        var (_11, _12) = fixture.CreateDistinctCollection<T>( 2 );
        var (e1, e2) = (new Exception(), new Exception());

        return new TheoryData<object, bool, object, bool, bool>
        {
            { _11!, true, _11!, true, true },
            { _11!, true, _12!, true, false },
            { e1, false, e1, false, true },
            { e1, false, e2, false, false },
            { _11!, true, e1, false, false },
            { e1, false, _11!, true, false }
        };
    }

    public static IEnumerable<object?[]> CreateNotEqualsTestData(IFixture fixture)
    {
        return CreateEqualsTestData( fixture ).ConvertResult( (bool r) => ! r );
    }
}
