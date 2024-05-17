using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
public sealed class PostgreSqlColumnBuilderCollection : SqlColumnBuilderCollection
{
    internal PostgreSqlColumnBuilderCollection(PostgreSqlColumnTypeDefinitionProvider typeDefinitions)
        : base( typeDefinitions.GetByType<object>() ) { }

    /// <inheritdoc cref="SqlColumnBuilderCollection.Table" />
    public new PostgreSqlTableBuilder Table => ReinterpretCast.To<PostgreSqlTableBuilder>( base.Table );

    /// <inheritdoc cref="SqlColumnBuilderCollection.SetDefaultTypeDefinition(SqlColumnTypeDefinition)" />
    public new PostgreSqlColumnBuilderCollection SetDefaultTypeDefinition(SqlColumnTypeDefinition definition)
    {
        base.SetDefaultTypeDefinition( definition );
        return this;
    }

    /// <inheritdoc cref="SqlColumnBuilderCollection.Get(string)" />
    [Pure]
    public new PostgreSqlColumnBuilder Get(string name)
    {
        return ReinterpretCast.To<PostgreSqlColumnBuilder>( base.Get( name ) );
    }

    /// <inheritdoc cref="SqlColumnBuilderCollection.TryGet(string)" />
    [Pure]
    public new PostgreSqlColumnBuilder? TryGet(string name)
    {
        return ReinterpretCast.To<PostgreSqlColumnBuilder>( base.TryGet( name ) );
    }

    /// <inheritdoc cref="SqlColumnBuilderCollection.Create(string)" />
    public new PostgreSqlColumnBuilder Create(string name)
    {
        return ReinterpretCast.To<PostgreSqlColumnBuilder>( base.Create( name ) );
    }

    /// <inheritdoc cref="SqlColumnBuilderCollection.GetOrCreate(string)" />
    public new PostgreSqlColumnBuilder GetOrCreate(string name)
    {
        return ReinterpretCast.To<PostgreSqlColumnBuilder>( base.GetOrCreate( name ) );
    }

    /// <summary>
    /// Creates a new <see cref="SqlObjectBuilderEnumerator{TSource,TDestination}"/> instance for this collection.
    /// </summary>
    /// <returns>New <see cref="SqlObjectBuilderEnumerator{TSource,TDestination}"/> instance.</returns>
    [Pure]
    public new SqlObjectBuilderEnumerator<SqlColumnBuilder, PostgreSqlColumnBuilder> GetEnumerator()
    {
        return base.GetEnumerator().UnsafeReinterpretAs<PostgreSqlColumnBuilder>();
    }

    /// <inheritdoc />
    protected override PostgreSqlColumnBuilder CreateColumnBuilder(string name)
    {
        return new PostgreSqlColumnBuilder( Table, name, DefaultTypeDefinition );
    }
}
