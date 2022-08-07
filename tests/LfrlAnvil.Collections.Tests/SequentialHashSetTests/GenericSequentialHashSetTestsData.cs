using System.Collections.Generic;

namespace LfrlAnvil.Collections.Tests.SequentialHashSetTests;

public class GenericSequentialHashSetTestsData<T>
{
    public static TheoryData<IEnumerable<T>, IEnumerable<T>, IEnumerable<T>> GetExceptWithData(IFixture fixture)
    {
        var (a, b, c, d, e, f) = fixture.CreateDistinctCollection<T>( 6 );

        return new TheoryData<IEnumerable<T>, IEnumerable<T>, IEnumerable<T>>
        {
            { Array.Empty<T>(), Array.Empty<T>(), Array.Empty<T>() },
            { Array.Empty<T>(), new[] { a, b }, Array.Empty<T>() },
            { new[] { a, b }, Array.Empty<T>(), new[] { a, b } },
            { new[] { a, b, c }, new[] { a, b, c }, Array.Empty<T>() },
            { new[] { a, b, c, d }, new[] { b, c }, new[] { a, d } },
            { new[] { b, c }, new[] { a, b, c, d }, Array.Empty<T>() },
            { new[] { a, b, c, d }, new[] { c, d, e, f }, new[] { a, b } },
            { new[] { a, b, c }, new[] { d, e, f }, new[] { a, b, c } }
        };
    }

    public static TheoryData<IEnumerable<T>, IEnumerable<T>, IEnumerable<T>> GetUnionWithData(IFixture fixture)
    {
        var (a, b, c, d, e, f) = fixture.CreateDistinctCollection<T>( 6 );

        return new TheoryData<IEnumerable<T>, IEnumerable<T>, IEnumerable<T>>
        {
            { Array.Empty<T>(), Array.Empty<T>(), Array.Empty<T>() },
            { Array.Empty<T>(), new[] { a, b }, new[] { a, b } },
            { new[] { a, b }, Array.Empty<T>(), new[] { a, b } },
            { new[] { a, b, c }, new[] { a, b, c }, new[] { a, b, c } },
            { new[] { a, b, c, d }, new[] { b, c }, new[] { a, b, c, d } },
            { new[] { b, c }, new[] { a, b, c, d }, new[] { b, c, a, d } },
            { new[] { a, b, c, d }, new[] { c, d, e, f }, new[] { a, b, c, d, e, f } },
            { new[] { a, b, c }, new[] { d, e, f }, new[] { a, b, c, d, e, f } }
        };
    }

    public static TheoryData<IEnumerable<T>, IEnumerable<T>, IEnumerable<T>> GetIntersectWithData(IFixture fixture)
    {
        var (a, b, c, d, e, f) = fixture.CreateDistinctCollection<T>( 6 );

        return new TheoryData<IEnumerable<T>, IEnumerable<T>, IEnumerable<T>>
        {
            { Array.Empty<T>(), Array.Empty<T>(), Array.Empty<T>() },
            { Array.Empty<T>(), new[] { a, b }, Array.Empty<T>() },
            { new[] { a, b }, Array.Empty<T>(), Array.Empty<T>() },
            { new[] { a, b, c }, new[] { a, b, c }, new[] { a, b, c } },
            { new[] { a, b, c, d }, new[] { b, c }, new[] { b, c } },
            { new[] { b, c }, new[] { a, b, c, d }, new[] { b, c } },
            { new[] { a, b, c, d }, new[] { c, d, e, f }, new[] { c, d } },
            { new[] { a, b, c }, new[] { d, e, f }, Array.Empty<T>() }
        };
    }

    public static TheoryData<IEnumerable<T>, IEnumerable<T>, IEnumerable<T>> GetSymmetricExceptWithData(IFixture fixture)
    {
        var (a, b, c, d, e, f) = fixture.CreateDistinctCollection<T>( 6 );

        return new TheoryData<IEnumerable<T>, IEnumerable<T>, IEnumerable<T>>
        {
            { Array.Empty<T>(), Array.Empty<T>(), Array.Empty<T>() },
            { Array.Empty<T>(), new[] { a, b }, new[] { a, b } },
            { new[] { a, b }, Array.Empty<T>(), new[] { a, b } },
            { new[] { a, b, c }, new[] { a, b, c }, Array.Empty<T>() },
            { new[] { a, b, c, d }, new[] { b, c }, new[] { a, d } },
            { new[] { b, c }, new[] { a, b, c, d }, new[] { a, d } },
            { new[] { a, b, c, d }, new[] { c, d, e, f }, new[] { a, b, e, f } },
            { new[] { a, b, c }, new[] { d, e, f }, new[] { a, b, c, d, e, f } }
        };
    }

    public static TheoryData<IEnumerable<T>, IEnumerable<T>, bool> GetOverlapsData(IFixture fixture)
    {
        var (a, b, c, d, e, f) = fixture.CreateDistinctCollection<T>( 6 );

        return new TheoryData<IEnumerable<T>, IEnumerable<T>, bool>
        {
            { Array.Empty<T>(), Array.Empty<T>(), false },
            { Array.Empty<T>(), new[] { a, b }, false },
            { new[] { a, b }, Array.Empty<T>(), false },
            { new[] { a, b, c }, new[] { a, b, c }, true },
            { new[] { a, b, c, d }, new[] { b, c }, true },
            { new[] { b, c }, new[] { a, b, c, d }, true },
            { new[] { a, b, c, d }, new[] { c, d, e, f }, true },
            { new[] { a, b, c }, new[] { d, e, f }, false }
        };
    }

    public static TheoryData<IEnumerable<T>, IEnumerable<T>, bool> GetSetEqualsData(IFixture fixture)
    {
        var (a, b, c, d, e, f) = fixture.CreateDistinctCollection<T>( 6 );

        return new TheoryData<IEnumerable<T>, IEnumerable<T>, bool>
        {
            { Array.Empty<T>(), Array.Empty<T>(), true },
            { Array.Empty<T>(), new[] { a, b }, false },
            { new[] { a, b }, Array.Empty<T>(), false },
            { new[] { a, b, c }, new[] { a, b, c }, true },
            { new[] { a, b, c, d }, new[] { b, c }, false },
            { new[] { b, c }, new[] { a, b, c, d }, false },
            { new[] { a, b, c, d }, new[] { c, d, e, f }, false },
            { new[] { a, b, c }, new[] { d, e, f }, false }
        };
    }

    public static TheoryData<IEnumerable<T>, IEnumerable<T>, bool> GetIsSupersetOfData(IFixture fixture)
    {
        var (a, b, c, d, e, f) = fixture.CreateDistinctCollection<T>( 6 );

        return new TheoryData<IEnumerable<T>, IEnumerable<T>, bool>
        {
            { Array.Empty<T>(), Array.Empty<T>(), true },
            { Array.Empty<T>(), new[] { a, b }, false },
            { new[] { a, b }, Array.Empty<T>(), true },
            { new[] { a, b, c }, new[] { a, b, c }, true },
            { new[] { a, b, c, d }, new[] { b, c }, true },
            { new[] { b, c }, new[] { a, b, c, d }, false },
            { new[] { a, b, c, d }, new[] { c, d, e, f }, false },
            { new[] { a, b, c }, new[] { d, e, f }, false }
        };
    }

    public static TheoryData<IEnumerable<T>, IEnumerable<T>, bool> GetIsProperSupersetOfData(IFixture fixture)
    {
        var (a, b, c, d, e, f) = fixture.CreateDistinctCollection<T>( 6 );

        return new TheoryData<IEnumerable<T>, IEnumerable<T>, bool>
        {
            { Array.Empty<T>(), Array.Empty<T>(), false },
            { Array.Empty<T>(), new[] { a, b }, false },
            { new[] { a, b }, Array.Empty<T>(), true },
            { new[] { a, b, c }, new[] { a, b, c }, false },
            { new[] { a, b, c, d }, new[] { b, c }, true },
            { new[] { b, c }, new[] { a, b, c, d }, false },
            { new[] { a, b, c, d }, new[] { c, d, e, f }, false },
            { new[] { a, b, c }, new[] { d, e, f }, false }
        };
    }

    public static TheoryData<IEnumerable<T>, IEnumerable<T>, bool> GetIsSubsetOfData(IFixture fixture)
    {
        var (a, b, c, d, e, f) = fixture.CreateDistinctCollection<T>( 6 );

        return new TheoryData<IEnumerable<T>, IEnumerable<T>, bool>
        {
            { Array.Empty<T>(), Array.Empty<T>(), true },
            { Array.Empty<T>(), new[] { a, b }, true },
            { new[] { a, b }, Array.Empty<T>(), false },
            { new[] { a, b, c }, new[] { a, b, c }, true },
            { new[] { a, b, c, d }, new[] { b, c }, false },
            { new[] { b, c }, new[] { a, b, c, d }, true },
            { new[] { a, b, c, d }, new[] { c, d, e, f }, false },
            { new[] { a, b, c }, new[] { d, e, f }, false }
        };
    }

    public static TheoryData<IEnumerable<T>, IEnumerable<T>, bool> GetIsProperSubsetOfData(IFixture fixture)
    {
        var (a, b, c, d, e, f) = fixture.CreateDistinctCollection<T>( 6 );

        return new TheoryData<IEnumerable<T>, IEnumerable<T>, bool>
        {
            { Array.Empty<T>(), Array.Empty<T>(), false },
            { Array.Empty<T>(), new[] { a, b }, true },
            { new[] { a, b }, Array.Empty<T>(), false },
            { new[] { a, b, c }, new[] { a, b, c }, false },
            { new[] { a, b, c, d }, new[] { b, c }, false },
            { new[] { b, c }, new[] { a, b, c, d }, true },
            { new[] { a, b, c, d }, new[] { c, d, e, f }, false },
            { new[] { a, b, c }, new[] { d, e, f }, false }
        };
    }
}
