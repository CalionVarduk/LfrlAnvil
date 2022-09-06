namespace LfrlAnvil.TestExtensions.FluentAssertions;

public static class ObjectExtensions
{
    public static NSubstituteObjectMockAssertions<T> VerifyCalls<T>(this T source)
        where T : class
    {
        return new NSubstituteObjectMockAssertions<T>( source );
    }
}
