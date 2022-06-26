namespace LfrlAnvil.Identifiers;

public enum LowValueOverflowStrategy : byte
{
    Forbidden = 0,
    AddHighValue = 1,
    SpinWait = 2,
    Sleep = 3
}