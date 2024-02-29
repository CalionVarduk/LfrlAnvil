using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Internal;

public readonly record struct SqlNamedSchemaObject<T>(SqlSchemaObjectName Name, T Object)
    where T : SqlObjectBuilder;
