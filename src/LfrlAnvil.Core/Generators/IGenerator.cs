using System.Diagnostics.CodeAnalysis;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Generators;

/// <summary>
/// Represents a type-erased generator of objects.
/// </summary>
public interface IGenerator
{
    /// <summary>
    /// Generates a new object.
    /// </summary>
    /// <returns>Generated object.</returns>
    /// <exception cref="ValueGenerationException">When object could not be generated.</exception>
    object? Generate();

    /// <summary>
    /// Attempts to generate a new object.
    /// </summary>
    /// <param name="result"><b>out</b> parameter that returns generated object, if successful.</param>
    /// <returns><b>true</b> when object was generated successfully, otherwise <b>false</b>.</returns>
    bool TryGenerate(out object? result);
}

/// <summary>
/// Represents a generic generator of objects.
/// </summary>
/// <typeparam name="T">Object type.</typeparam>
public interface IGenerator<T> : IGenerator
{
    /// <summary>
    /// Generates a new object.
    /// </summary>
    /// <returns>Generated object.</returns>
    /// <exception cref="ValueGenerationException">When object could not be generated.</exception>
    new T Generate();

    /// <summary>
    /// Attempts to generate a new object.
    /// </summary>
    /// <param name="result"><b>out</b> parameter that returns generated object, if successful.</param>
    /// <returns><b>true</b> when object was generated successfully, otherwise <b>false</b>.</returns>
    bool TryGenerate([MaybeNullWhen( false )] out T result);
}
