namespace LfrlAnvil.Identifiers;

/// <summary>
/// Represents possible strategies for resolving low value overflow in <see cref="IdentifierGenerator"/> instances.
/// </summary>
public enum LowValueOverflowStrategy : byte
{
    /// <summary>
    /// Specifies that generator should not generate new identifiers as long as the low value overflow is in progress.
    /// </summary>
    Forbidden = 0,

    /// <summary>
    /// Specifies that generator will increment its high value by <b>1</b> regardless of the current time,
    /// which will reset the low value counter.
    /// </summary>
    AddHighValue = 1,

    /// <summary>
    /// Specifies that generator will <see cref="System.Threading.SpinWait"/> as long as the low value overflow is in progress.
    /// </summary>
    SpinWait = 2,

    /// <summary>
    /// Specifies that generator will <see cref="System.Threading.Thread.Sleep(int)"/> as long as the low value overflow is in progress.
    /// </summary>
    Sleep = 3
}
