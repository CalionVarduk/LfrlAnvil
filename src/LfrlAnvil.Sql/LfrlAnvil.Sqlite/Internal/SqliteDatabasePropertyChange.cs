using System.Diagnostics.Contracts;
using LfrlAnvil.Sqlite.Builders;

namespace LfrlAnvil.Sqlite.Internal;

internal readonly struct SqliteDatabasePropertyChange
{
    internal SqliteDatabasePropertyChange(
        SqliteObjectBuilder @object,
        SqliteObjectChangeDescriptor descriptor,
        SqliteObjectStatus status,
        object? oldValue,
        object? newValue)
    {
        Object = @object;
        Descriptor = descriptor;
        Status = status;
        OldValue = oldValue;
        NewValue = newValue;
    }

    internal SqliteObjectBuilder Object { get; }
    internal SqliteObjectChangeDescriptor Descriptor { get; }
    internal SqliteObjectStatus Status { get; }
    internal object? OldValue { get; }
    internal object? NewValue { get; }

    [Pure]
    public override string ToString()
    {
        var oldValueText = OldValue is not null ? OldValue.ToString() ?? string.Empty : "<null>";
        var newValueText = NewValue is not null ? NewValue.ToString() ?? string.Empty : "<null>";
        return $"[{Status}] {Object} ([{Descriptor}]: {{{oldValueText}}} => {{{newValueText}}})";
    }
}
