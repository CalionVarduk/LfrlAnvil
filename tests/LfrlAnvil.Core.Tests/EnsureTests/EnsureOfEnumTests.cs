namespace LfrlAnvil.Tests.EnsureTests;

public class EnsureOfEnumTests : EnsureTestsBase
{
    [Theory]
    [InlineData( TestEnum.Foo )]
    [InlineData( TestEnum.Bar )]
    public void IsDefined_ShouldPass_WhenParamIsDefined(TestEnum param)
    {
        ShouldPass( () => Ensure.IsDefined( param ) );
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( ( int )TestEnum.Bar + 1 )]
    public void IsDefined_ShouldThrowArgumentException_WhenParamIsNotDefined(int param)
    {
        ShouldThrowArgumentException( () => Ensure.IsDefined( ( TestEnum )param ) );
    }
}

public enum TestEnum
{
    Foo,
    Bar
}
