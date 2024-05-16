using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public sealed class SqliteColumnBuilderCollection : SqlColumnBuilderCollection
{
    internal SqliteColumnBuilderCollection(SqliteColumnTypeDefinitionProvider typeDefinitions)
        : base( typeDefinitions.GetByDataType( SqliteDataType.Any ) ) { }

    /// <inheritdoc cref="SqlColumnBuilderCollection.Table" />
    public new SqliteTableBuilder Table => ReinterpretCast.To<SqliteTableBuilder>( base.Table );

    /// <inheritdoc cref="SqlColumnBuilderCollection.SetDefaultTypeDefinition(SqlColumnTypeDefinition)" />
    public new SqliteColumnBuilderCollection SetDefaultTypeDefinition(SqlColumnTypeDefinition definition)
    {
        base.SetDefaultTypeDefinition( definition );
        return this;
    }

    /// <inheritdoc cref="SqlColumnBuilderCollection.Get(string)" />
    [Pure]
    public new SqliteColumnBuilder Get(string name)
    {
        return ReinterpretCast.To<SqliteColumnBuilder>( base.Get( name ) );
    }

    /// <inheritdoc cref="SqlColumnBuilderCollection.TryGet(string)" />
    [Pure]
    public new SqliteColumnBuilder? TryGet(string name)
    {
        return ReinterpretCast.To<SqliteColumnBuilder>( base.TryGet( name ) );
    }

    /// <inheritdoc cref="SqlColumnBuilderCollection.Create(string)" />
    public new SqliteColumnBuilder Create(string name)
    {
        return ReinterpretCast.To<SqliteColumnBuilder>( base.Create( name ) );
    }

    /// <inheritdoc cref="SqlColumnBuilderCollection.GetOrCreate(string)" />
    public new SqliteColumnBuilder GetOrCreate(string name)
    {
        return ReinterpretCast.To<SqliteColumnBuilder>( base.GetOrCreate( name ) );
    }

    /// <summary>
    /// Creates a new <see cref="SqlObjectBuilderEnumerator{TSource,TDestination}"/> instance for this collection.
    /// </summary>
    /// <returns>New <see cref="SqlObjectBuilderEnumerator{TSource,TDestination}"/> instance.</returns>
    [Pure]
    public new SqlObjectBuilderEnumerator<SqlColumnBuilder, SqliteColumnBuilder> GetEnumerator()
    {
        return base.GetEnumerator().UnsafeReinterpretAs<SqliteColumnBuilder>();
    }

    /// <inheritdoc />
    protected override SqliteColumnBuilder CreateColumnBuilder(string name)
    {
        return new SqliteColumnBuilder( Table, name, DefaultTypeDefinition );
    }
}
