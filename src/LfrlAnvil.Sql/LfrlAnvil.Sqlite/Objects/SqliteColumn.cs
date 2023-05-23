using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Internal;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

public sealed class SqliteColumn : SqliteObject, ISqlColumn
{
    private string? _fullName;

    internal SqliteColumn(SqliteTable table, SqliteColumnBuilder builder)
        : base( builder )
    {
        Table = table;
        TypeDefinition = builder.TypeDefinition;
        IsNullable = builder.IsNullable;
        DefaultValue = builder.DefaultValue;
        _fullName = builder.GetCachedFullName();
    }

    public SqliteTable Table { get; }
    public SqliteColumnTypeDefinition TypeDefinition { get; }
    public bool IsNullable { get; }
    public object? DefaultValue { get; }
    public override string FullName => _fullName ??= SqliteHelpers.GetFullColumnName( Table.FullName, Name );
    public override SqliteDatabase Database => Table.Schema.Database;

    [Pure]
    public SqliteIndexColumn Asc()
    {
        return SqliteIndexColumn.Asc( this );
    }

    [Pure]
    public SqliteIndexColumn Desc()
    {
        return SqliteIndexColumn.Desc( this );
    }

    ISqlTable ISqlColumn.Table => Table;
    ISqlColumnTypeDefinition ISqlColumn.TypeDefinition => TypeDefinition;

    [Pure]
    ISqlIndexColumn ISqlColumn.Asc()
    {
        return Asc();
    }

    [Pure]
    ISqlIndexColumn ISqlColumn.Desc()
    {
        return Desc();
    }
}
