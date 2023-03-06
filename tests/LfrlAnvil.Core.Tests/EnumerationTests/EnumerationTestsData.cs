using System.Collections.Generic;
using System.Linq;

namespace LfrlAnvil.Tests.EnumerationTests;

public class EnumerationTestsData
{
    public static TheoryData<ValidEnum, ValidEnum, int> GetCompareToData(IFixture fixture)
    {
        return new TheoryData<ValidEnum, ValidEnum, int>
        {
            { ValidEnum.One, ValidEnum.One, 0 },
            { ValidEnum.One, ValidEnum.Two, -1 },
            { ValidEnum.Two, ValidEnum.One, 1 }
        };
    }

    public static IEnumerable<object?[]> GetEqualsData(IFixture fixture)
    {
        return GetCompareToData( fixture ).ConvertResult( (int r) => r == 0 );
    }

    public static IEnumerable<object?[]> GetEqualityOperatorData(IFixture fixture)
    {
        var @base = GetCompareToData( fixture ).ConvertResult( (int r) => r == 0 );
        return new TheoryData<ValidEnum?, ValidEnum?, bool>
            {
                { null, null, true },
                { ValidEnum.One, null, false },
                { null, ValidEnum.One, false }
            }
            .Concat( @base );
    }

    public static IEnumerable<object?[]> GetInequalityOperatorData(IFixture fixture)
    {
        var @base = GetCompareToData( fixture ).ConvertResult( (int r) => r != 0 );
        return new TheoryData<ValidEnum?, ValidEnum?, bool>
            {
                { null, null, false },
                { ValidEnum.One, null, true },
                { null, ValidEnum.One, true }
            }
            .Concat( @base );
    }

    public static IEnumerable<object?[]> GetGreaterThanOrEqualToOperatorData(IFixture fixture)
    {
        var @base = GetCompareToData( fixture ).ConvertResult( (int r) => r >= 0 );
        return new TheoryData<ValidEnum?, ValidEnum?, bool>
            {
                { null, null, true },
                { ValidEnum.One, null, true },
                { null, ValidEnum.One, false }
            }
            .Concat( @base );
    }

    public static IEnumerable<object?[]> GetGreaterThanOperatorData(IFixture fixture)
    {
        var @base = GetCompareToData( fixture ).ConvertResult( (int r) => r > 0 );
        return new TheoryData<ValidEnum?, ValidEnum?, bool>
            {
                { null, null, false },
                { ValidEnum.One, null, true },
                { null, ValidEnum.One, false }
            }
            .Concat( @base );
    }

    public static IEnumerable<object?[]> GetLessThanOrEqualToOperatorData(IFixture fixture)
    {
        var @base = GetCompareToData( fixture ).ConvertResult( (int r) => r <= 0 );
        return new TheoryData<ValidEnum?, ValidEnum?, bool>
            {
                { null, null, true },
                { ValidEnum.One, null, false },
                { null, ValidEnum.One, true }
            }
            .Concat( @base );
    }

    public static IEnumerable<object?[]> GetLessThanOperatorData(IFixture fixture)
    {
        var @base = GetCompareToData( fixture ).ConvertResult( (int r) => r < 0 );
        return new TheoryData<ValidEnum?, ValidEnum?, bool>
            {
                { null, null, false },
                { ValidEnum.One, null, false },
                { null, ValidEnum.One, true }
            }
            .Concat( @base );
    }
}

public sealed class ValidEnum : Enumeration<ValidEnum, int>
{
    public static readonly int SomeInt = 5;
    private static readonly ValidEnum CustomBackingField = new ValidEnum( "four", 4 );
    public static ValidEnum Four => CustomBackingField;

    public static readonly ValidEnum? NullField = null;
    public static ValidEnum? NullProperty { get; } = null;

    public static readonly ValidEnum One = new ValidEnum( "one", 1 );
    public static readonly ValidEnum Two = new ValidEnum( "two", 2 );
    public static ValidEnum Three { get; } = new ValidEnum( "three", 3 );

    public static readonly IReadOnlyDictionary<string, ValidEnum> ByName = GetNameDictionary();
    public static readonly IReadOnlyDictionary<int, ValidEnum> ByValue = GetValueDictionary();

    private ValidEnum(string name, int value)
        : base( name, value ) { }
}

public sealed class DuplicateValueEnum : Enumeration<DuplicateValueEnum, int>
{
    public static readonly DuplicateValueEnum One = new DuplicateValueEnum( "one", 1 );
    public static readonly DuplicateValueEnum Two = new DuplicateValueEnum( "two", 1 );

    public static readonly IReadOnlyDictionary<int, DuplicateValueEnum> ByValue = GetValueDictionary();

    private DuplicateValueEnum(string name, int value)
        : base( name, value ) { }
}

public sealed class DuplicateNameEnum : Enumeration<DuplicateNameEnum, int>
{
    public static readonly DuplicateNameEnum One = new DuplicateNameEnum( "one", 1 );
    public static readonly DuplicateNameEnum Two = new DuplicateNameEnum( "one", 2 );

    public static readonly IReadOnlyDictionary<string, DuplicateNameEnum> ByName = GetNameDictionary();

    private DuplicateNameEnum(string name, int value)
        : base( name, value ) { }
}
