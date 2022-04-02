using System.Diagnostics.CodeAnalysis;

namespace LfrlAnvil.Generators
{
    public interface IGenerator
    {
        object? Generate();
        bool TryGenerate(out object? result);
    }

    public interface IGenerator<T> : IGenerator
    {
        new T Generate();
        bool TryGenerate([MaybeNullWhen( false )] out T result);
    }
}
