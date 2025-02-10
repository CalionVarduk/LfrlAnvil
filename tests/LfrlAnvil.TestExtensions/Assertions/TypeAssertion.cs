namespace LfrlAnvil.TestExtensions.Assertions;

internal sealed class TypeAssertion<T> : Assertion
{
    internal TypeAssertion(string context, T subject, Type expected, bool exact)
        : base( context )
    {
        Subject = subject;
        Expected = expected;
        Exact = exact;
    }

    internal T Subject { get; }
    internal Type Expected { get; }
    internal bool Exact { get; }

    public override void Go()
    {
        var type = Subject?.GetType();
        if ( Exact )
        {
            if ( type != Expected )
                Throw( $"[{Context}] should be exactly of type {Expected.FullName} but found {type?.FullName ?? "null"}." );
        }
        else if ( ! Expected.IsAssignableFrom( type ) )
            Throw( $"[{Context}] should be of type assignable to {Expected.FullName} but found {type?.FullName ?? "null"}." );
    }
}
