using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.Sql.Expressions.Objects;

/// <summary>
/// Represents an SQL syntax tree node that defines a single record set based on an <see cref="ISqlView"/> instance.
/// </summary>
public sealed class SqlViewNode : SqlRecordSetNode
{
    private Dictionary<string, SqlViewDataFieldNode>? _fields;

    internal SqlViewNode(ISqlView view, string? alias, bool isOptional)
        : base( SqlNodeType.View, alias, isOptional )
    {
        View = view;
        _fields = null;
    }

    /// <summary>
    /// Underlying <see cref="ISqlView"/> instance.
    /// </summary>
    public ISqlView View { get; }

    /// <inheritdoc />
    public override SqlRecordSetInfo Info => View.Info;

    /// <inheritdoc cref="SqlRecordSetNode.this[string]" />
    public new SqlViewDataFieldNode this[string fieldName] => GetField( fieldName );

    /// <inheritdoc />
    [Pure]
    public override IReadOnlyCollection<SqlViewDataFieldNode> GetKnownFields()
    {
        _fields ??= CreateDataFields();
        return _fields.Values;
    }

    /// <inheritdoc />
    [Pure]
    public override SqlViewNode As(string alias)
    {
        return new SqlViewNode( View, alias, IsOptional );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlViewNode AsSelf()
    {
        return new SqlViewNode( View, alias: null, IsOptional );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlDataFieldNode GetUnsafeField(string name)
    {
        _fields ??= CreateDataFields();
        return _fields.TryGetValue( name, out var column ) ? column : new SqlRawDataFieldNode( this, name, type: null );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlViewDataFieldNode GetField(string name)
    {
        _fields ??= CreateDataFields();
        return _fields[name];
    }

    /// <inheritdoc />
    [Pure]
    public override SqlViewNode MarkAsOptional(bool optional = true)
    {
        return IsOptional != optional
            ? new SqlViewNode( View, Alias, isOptional: optional )
            : this;
    }

    [Pure]
    private Dictionary<string, SqlViewDataFieldNode> CreateDataFields()
    {
        var dataFields = View.DataFields;
        var result = new Dictionary<string, SqlViewDataFieldNode>( capacity: dataFields.Count, comparer: SqlHelpers.NameComparer );

        foreach ( var field in dataFields )
            result.Add( field.Name, new SqlViewDataFieldNode( this, field ) );

        return result;
    }
}
