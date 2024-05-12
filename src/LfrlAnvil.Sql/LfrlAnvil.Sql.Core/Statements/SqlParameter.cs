using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Statements;

/// <summary>
/// Represents a type-erased bindable SQL parameter.
/// </summary>
/// <param name="Name">Optional parameter name.</param>
/// <param name="Value">Parameter value.</param>
public readonly record struct SqlParameter(string? Name, object? Value)
{
    /// <summary>
    /// Specifies whether or not this parameter is positional (does not have a <see cref="Name"/>).
    /// </summary>
    [MemberNotNullWhen( false, nameof( Name ) )]
    public bool IsPositional => Name is null;

    /// <summary>
    /// Creates a new named <see cref="SqlParameter"/> instance.
    /// </summary>
    /// <param name="name">Parameter name.</param>
    /// <param name="value">Parameter value.</param>
    /// <returns>New named <see cref="SqlParameter"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlParameter Named(string name, object? value)
    {
        return new SqlParameter( name, value );
    }

    /// <summary>
    /// Creates a new positional <see cref="SqlParameter"/> instance.
    /// </summary>
    /// <param name="value">Parameter value.</param>
    /// <returns>New positional <see cref="SqlParameter"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlParameter Positional(object? value)
    {
        return new SqlParameter( null, value );
    }
}
