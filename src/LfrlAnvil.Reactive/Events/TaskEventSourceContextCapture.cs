namespace LfrlAnvil.Reactive.Events
{
    public enum TaskEventSourceContextCapture : byte
    {
        None = 0,
        Current = 1,
        FromListener = 2
    }
}
