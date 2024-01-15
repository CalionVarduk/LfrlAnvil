using System.Diagnostics.Contracts;
using LfrlAnvil.MySql.Objects.Builders;

namespace LfrlAnvil.MySql.Internal;

internal readonly struct MySqlDatabasePropertyChange
{
    internal MySqlDatabasePropertyChange(
        MySqlObjectBuilder @object,
        MySqlObjectChangeDescriptor descriptor,
        MySqlObjectStatus status,
        object? oldValue,
        object? newValue)
    {
        Object = @object;
        Descriptor = descriptor;
        Status = status;
        OldValue = oldValue;
        NewValue = newValue;
    }

    internal MySqlObjectBuilder Object { get; }
    internal MySqlObjectChangeDescriptor Descriptor { get; }
    internal MySqlObjectStatus Status { get; }
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
