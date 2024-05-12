using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Internal;

/// <summary>
/// Represents a named <see cref="SqlObjectBuilder"/> instance.
/// </summary>
/// <param name="Name">Name of the object.</param>
/// <param name="Object">SQL object builder.</param>
/// <typeparam name="T">SQL object builder type.</typeparam>
public readonly record struct SqlNamedObject<T>(string Name, T Object)
    where T : SqlObjectBuilder;
