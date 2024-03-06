using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Objects.Builders;

public sealed class SqliteTableBuilder : SqlTableBuilder
{
    internal SqliteTableBuilder(SqliteSchemaBuilder schema, string name)
        : base(
            schema,
            name,
            new SqliteColumnBuilderCollection( schema.Database.TypeDefinitions ),
            new SqliteConstraintBuilderCollection() ) { }

    public new SqliteDatabaseBuilder Database => ReinterpretCast.To<SqliteDatabaseBuilder>( base.Database );
    public new SqliteSchemaBuilder Schema => ReinterpretCast.To<SqliteSchemaBuilder>( base.Schema );
    public new SqliteColumnBuilderCollection Columns => ReinterpretCast.To<SqliteColumnBuilderCollection>( base.Columns );
    public new SqliteConstraintBuilderCollection Constraints => ReinterpretCast.To<SqliteConstraintBuilderCollection>( base.Constraints );

    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqliteHelpers.GetFullName( Schema.Name, Name )}";
    }

    public new SqliteTableBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }
}
