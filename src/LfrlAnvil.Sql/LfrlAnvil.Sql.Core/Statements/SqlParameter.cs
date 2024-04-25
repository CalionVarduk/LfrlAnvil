using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Statements;

public readonly record struct SqlParameter(string? Name, object? Value)
{
    [MemberNotNullWhen( false, nameof( Name ) )]
    public bool IsPositional => Name is null;

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlParameter Named(string name, object? value)
    {
        return new SqlParameter( name, value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlParameter Positional(object? value)
    {
        return new SqlParameter( null, value );
    }
}
