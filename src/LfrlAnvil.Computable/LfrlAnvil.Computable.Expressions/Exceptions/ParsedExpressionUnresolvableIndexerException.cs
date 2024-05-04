using System;
using System.Collections.Generic;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

/// <summary>
/// Represents an error that occurred due to a missing indexer property.
/// </summary>
public class ParsedExpressionUnresolvableIndexerException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionUnresolvableIndexerException"/> instance.
    /// </summary>
    /// <param name="targetType">Target type.</param>
    /// <param name="parameterTypes">Indexer parameter types.</param>
    public ParsedExpressionUnresolvableIndexerException(Type targetType, IReadOnlyList<Type> parameterTypes)
        : base( Resources.UnresolvableIndexer( targetType, parameterTypes ) )
    {
        TargetType = targetType;
        ParameterTypes = parameterTypes;
    }

    /// <summary>
    /// Target type.
    /// </summary>
    public Type TargetType { get; }

    /// <summary>
    /// Indexer parameter types.
    /// </summary>
    public IReadOnlyList<Type> ParameterTypes { get; }
}
