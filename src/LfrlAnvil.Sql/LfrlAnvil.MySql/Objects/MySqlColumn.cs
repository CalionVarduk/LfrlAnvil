using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.MySql.Internal;
using LfrlAnvil.MySql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects;

public sealed class MySqlColumn : MySqlObject, ISqlColumn
{
    private string? _fullName;
    private SqlColumnNode? _node;

    internal MySqlColumn(MySqlTable table, MySqlColumnBuilder builder)
        : base( builder )
    {
        Table = table;
        TypeDefinition = builder.TypeDefinition;
        IsNullable = builder.IsNullable;
        HasDefaultValue = builder.DefaultValue is not null;
        _fullName = builder.GetCachedFullName();
        _node = null;
    }

    public MySqlTable Table { get; }
    public MySqlColumnTypeDefinition TypeDefinition { get; }
    public bool IsNullable { get; }
    public bool HasDefaultValue { get; }
    public override string FullName => _fullName ??= MySqlHelpers.GetFullFieldName( Table.FullName, Name );
    public SqlColumnNode Node => _node ??= Table.RecordSet[Name];
    public override MySqlDatabase Database => Table.Schema.Database;

    [Pure]
    public MySqlIndexColumn Asc()
    {
        return MySqlIndexColumn.Asc( this );
    }

    [Pure]
    public MySqlIndexColumn Desc()
    {
        return MySqlIndexColumn.Desc( this );
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
