using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects.Builders;

public sealed class SqliteColumnBuilderCollection : SqlColumnBuilderCollection
{
    internal SqliteColumnBuilderCollection(SqliteColumnTypeDefinitionProvider typeDefinitions)
        : base( typeDefinitions.GetByDataType( SqliteDataType.Any ) ) { }

    public new SqliteTableBuilder Table => ReinterpretCast.To<SqliteTableBuilder>( base.Table );

    public new SqliteColumnBuilderCollection SetDefaultTypeDefinition(SqlColumnTypeDefinition definition)
    {
        base.SetDefaultTypeDefinition( definition );
        return this;
    }

    [Pure]
    public new SqliteColumnBuilder Get(string name)
    {
        return ReinterpretCast.To<SqliteColumnBuilder>( base.Get( name ) );
    }

    [Pure]
    public new SqliteColumnBuilder? TryGet(string name)
    {
        return ReinterpretCast.To<SqliteColumnBuilder>( base.TryGet( name ) );
    }

    public new SqliteColumnBuilder Create(string name)
    {
        return ReinterpretCast.To<SqliteColumnBuilder>( base.Create( name ) );
    }

    public new SqliteColumnBuilder GetOrCreate(string name)
    {
        return ReinterpretCast.To<SqliteColumnBuilder>( base.GetOrCreate( name ) );
    }

    [Pure]
    public new SqlObjectBuilderEnumerator<SqlColumnBuilder, SqliteColumnBuilder> GetEnumerator()
    {
        return base.GetEnumerator().UnsafeReinterpretAs<SqliteColumnBuilder>();
    }

    protected override SqliteColumnBuilder CreateColumnBuilder(string name)
    {
        return new SqliteColumnBuilder( Table, name, DefaultTypeDefinition );
    }
}
