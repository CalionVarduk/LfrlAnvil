using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Internal;

/// <summary>
/// Represents a named <see cref="SqlObjectBuilder"/> instance, including a DB schema name.
/// </summary>
/// <param name="Name">Name of the object.</param>
/// <param name="Object">SQL object builder instance.</param>
/// <typeparam name="T">SQL object builder type.</typeparam>
public readonly record struct SqlNamedSchemaObject<T>(SqlSchemaObjectName Name, T Object)
    where T : SqlObjectBuilder;
