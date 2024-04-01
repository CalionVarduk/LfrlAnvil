using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Objects.Builders;

public sealed class PostgreSqlColumnBuilderCollection : SqlColumnBuilderCollection
{
    internal PostgreSqlColumnBuilderCollection(PostgreSqlColumnTypeDefinitionProvider typeDefinitions)
        : base( typeDefinitions.GetByType<object>() ) { }

    public new PostgreSqlTableBuilder Table => ReinterpretCast.To<PostgreSqlTableBuilder>( base.Table );

    public new PostgreSqlColumnBuilderCollection SetDefaultTypeDefinition(SqlColumnTypeDefinition definition)
    {
        base.SetDefaultTypeDefinition( definition );
        return this;
    }

    [Pure]
    public new PostgreSqlColumnBuilder Get(string name)
    {
        return ReinterpretCast.To<PostgreSqlColumnBuilder>( base.Get( name ) );
    }

    [Pure]
    public new PostgreSqlColumnBuilder? TryGet(string name)
    {
        return ReinterpretCast.To<PostgreSqlColumnBuilder>( base.TryGet( name ) );
    }

    public new PostgreSqlColumnBuilder Create(string name)
    {
        return ReinterpretCast.To<PostgreSqlColumnBuilder>( base.Create( name ) );
    }

    public new PostgreSqlColumnBuilder GetOrCreate(string name)
    {
        return ReinterpretCast.To<PostgreSqlColumnBuilder>( base.GetOrCreate( name ) );
    }

    [Pure]
    public new SqlObjectBuilderEnumerator<SqlColumnBuilder, PostgreSqlColumnBuilder> GetEnumerator()
    {
        return base.GetEnumerator().UnsafeReinterpretAs<PostgreSqlColumnBuilder>();
    }

    protected override PostgreSqlColumnBuilder CreateColumnBuilder(string name)
    {
        return new PostgreSqlColumnBuilder( Table, name, DefaultTypeDefinition );
    }
}
