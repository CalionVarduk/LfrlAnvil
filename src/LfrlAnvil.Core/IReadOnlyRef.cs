namespace LfrlAnvil;

public interface IReadOnlyRef<out T>
{
    T Value { get; }
}
