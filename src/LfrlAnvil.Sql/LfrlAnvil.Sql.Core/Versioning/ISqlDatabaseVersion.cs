using System;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Versioning;

public interface ISqlDatabaseVersion
{
    Version Value { get; }
    string Description { get; }
    void Apply(ISqlDatabaseBuilder database);
}
