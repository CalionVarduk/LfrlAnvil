using LfrlAnvil.Extensions;

namespace LfrlAnvil.Tests.ExtensionsTests.EnumTests;

public class StaticEnumExtensionsTests : TestsBase
{
    [Fact]
    public void GetMaxNameLength_ShouldReturnZero_WhenEnumDoesNotHaveAnyMembers()
    {
        var result = EnumExtensions.GetMaxNameLength<EmptyEnum>();
        result.TestEquals( 0 ).Go();
    }

    [Fact]
    public void GetMaxNameLength_ShouldReturnLengthOfTheLongestEnumMemberName()
    {
        var result = EnumExtensions.GetMaxNameLength<TestEnum>();
        result.TestEquals( nameof( TestEnum.DolorSitAmet ).Length ).Go();
    }

    public enum EmptyEnum { }

    public enum TestEnum
    {
        Foo = 0,
        Lorem = 1,
        DolorSitAmet = 2,
        A = 3
    }
}
