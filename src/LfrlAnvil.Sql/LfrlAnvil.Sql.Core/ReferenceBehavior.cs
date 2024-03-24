using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql;

public sealed class ReferenceBehavior : Enumeration<ReferenceBehavior, ReferenceBehavior.Values>
{
    public enum Values : byte
    {
        Restrict = 0,
        Cascade = 1,
        SetNull = 2,
        NoAction = 3
    }

    public static readonly ReferenceBehavior Restrict = new ReferenceBehavior( "RESTRICT", Values.Restrict );
    public static readonly ReferenceBehavior Cascade = new ReferenceBehavior( "CASCADE", Values.Cascade );
    public static readonly ReferenceBehavior SetNull = new ReferenceBehavior( "SET NULL", Values.SetNull );
    public static readonly ReferenceBehavior NoAction = new ReferenceBehavior( "NO ACTION", Values.NoAction );

    private ReferenceBehavior(string name, Values value)
        : base( name, value ) { }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ReferenceBehavior GetBehavior(Values value)
    {
        Ensure.IsDefined( value );
        return value switch
        {
            Values.Restrict => Restrict,
            Values.Cascade => Cascade,
            Values.SetNull => SetNull,
            _ => NoAction
        };
    }
}
