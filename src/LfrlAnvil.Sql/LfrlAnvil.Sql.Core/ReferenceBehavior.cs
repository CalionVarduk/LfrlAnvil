namespace LfrlAnvil.Sql;

public sealed class ReferenceBehavior : Enumeration<ReferenceBehavior, ReferenceBehavior.Values>
{
    public enum Values : byte
    {
        Restrict = 0,
        Cascade = 1
    }

    public static readonly ReferenceBehavior Restrict = new ReferenceBehavior( "RESTRICT", Values.Restrict );
    public static readonly ReferenceBehavior Cascade = new ReferenceBehavior( "CASCADE", Values.Cascade );

    private ReferenceBehavior(string name, Values value)
        : base( name, value ) { }
}
