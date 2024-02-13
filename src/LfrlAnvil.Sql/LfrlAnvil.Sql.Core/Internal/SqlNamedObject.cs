using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Internal;

public readonly record struct SqlNamedObject<T>(string Name, T Object)
    where T : SqlObjectBuilder;
