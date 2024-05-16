using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public sealed class SqliteTableBuilder : SqlTableBuilder
{
    internal SqliteTableBuilder(SqliteSchemaBuilder schema, string name)
        : base(
            schema,
            name,
            new SqliteColumnBuilderCollection( schema.Database.TypeDefinitions ),
            new SqliteConstraintBuilderCollection() ) { }

    /// <inheritdoc cref="SqlObjectBuilder.Database" />
    public new SqliteDatabaseBuilder Database => ReinterpretCast.To<SqliteDatabaseBuilder>( base.Database );

    /// <inheritdoc cref="SqlTableBuilder.Schema" />
    public new SqliteSchemaBuilder Schema => ReinterpretCast.To<SqliteSchemaBuilder>( base.Schema );

    /// <inheritdoc cref="SqlTableBuilder.Columns" />
    public new SqliteColumnBuilderCollection Columns => ReinterpretCast.To<SqliteColumnBuilderCollection>( base.Columns );

    /// <inheritdoc cref="SqlTableBuilder.Constraints" />
    public new SqliteConstraintBuilderCollection Constraints => ReinterpretCast.To<SqliteConstraintBuilderCollection>( base.Constraints );

    /// <summary>
    /// Returns a string representation of this <see cref="SqliteTableBuilder"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqliteHelpers.GetFullName( Schema.Name, Name )}";
    }

    /// <inheritdoc cref="SqlTableBuilder.SetName(string)" />
    public new SqliteTableBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }
}
