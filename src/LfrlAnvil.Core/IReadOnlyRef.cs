namespace LfrlAnvil;

public interface IReadOnlyRef<out T>
    where T : struct
{
    T Value { get; }
}
