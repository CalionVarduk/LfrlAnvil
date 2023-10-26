using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Internal;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

public sealed class SqliteColumn : SqliteObject, ISqlColumn
{
    private string? _fullName;
    private SqlColumnNode? _node;

    internal SqliteColumn(SqliteTable table, SqliteColumnBuilder builder)
        : base( builder )
    {
        Table = table;
        TypeDefinition = builder.TypeDefinition;
        IsNullable = builder.IsNullable;
        _fullName = builder.GetCachedFullName();
        _node = null;
    }

    public SqliteTable Table { get; }
    public SqliteColumnTypeDefinition TypeDefinition { get; }
    public bool IsNullable { get; }
    public override string FullName => _fullName ??= SqliteHelpers.GetFullFieldName( Table.FullName, Name );
    public SqlColumnNode Node => _node ??= Table.RecordSet[Name];
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
