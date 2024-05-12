using System;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Versioning;

/// <summary>
/// Represents a single database version.
/// </summary>
public interface ISqlDatabaseVersion
{
    /// <summary>
    /// Identifier of this version.
    /// </summary>
    Version Value { get; }

    /// <summary>
    /// Description of this version.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Applies changes defined by this version to the provided <paramref name="database"/>.
    /// </summary>
    /// <param name="database">Target SQL database builder.</param>
    void Apply(ISqlDatabaseBuilder database);
}
