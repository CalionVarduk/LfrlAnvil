using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

public sealed class MySqlColumnBuilderCollection : SqlColumnBuilderCollection
{
    internal MySqlColumnBuilderCollection(MySqlColumnTypeDefinitionProvider typeDefinitions)
        : base( typeDefinitions.GetByType<object>() ) { }

    public new MySqlTableBuilder Table => ReinterpretCast.To<MySqlTableBuilder>( base.Table );

    public new MySqlColumnBuilderCollection SetDefaultTypeDefinition(SqlColumnTypeDefinition definition)
    {
        base.SetDefaultTypeDefinition( definition );
        return this;
    }

    [Pure]
    public new MySqlColumnBuilder Get(string name)
    {
        return ReinterpretCast.To<MySqlColumnBuilder>( base.Get( name ) );
    }

    [Pure]
    public new MySqlColumnBuilder? TryGet(string name)
    {
        return ReinterpretCast.To<MySqlColumnBuilder>( base.TryGet( name ) );
    }

    public new MySqlColumnBuilder Create(string name)
    {
        return ReinterpretCast.To<MySqlColumnBuilder>( base.Create( name ) );
    }

    public new MySqlColumnBuilder GetOrCreate(string name)
    {
        return ReinterpretCast.To<MySqlColumnBuilder>( base.GetOrCreate( name ) );
    }

    [Pure]
    public new SqlObjectBuilderEnumerator<SqlColumnBuilder, MySqlColumnBuilder> GetEnumerator()
    {
        return base.GetEnumerator().UnsafeReinterpretAs<MySqlColumnBuilder>();
    }

    protected override MySqlColumnBuilder CreateColumnBuilder(string name)
    {
        return new MySqlColumnBuilder( Table, name, DefaultTypeDefinition );
    }
}
