using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlViewNode : SqlRecordSetNode
{
    private Dictionary<string, SqlViewDataFieldNode>? _fields;

    internal SqlViewNode(ISqlView view, string? alias, bool isOptional)
        : base( SqlNodeType.View, alias, isOptional )
    {
        View = view;
        _fields = null;
    }

    public ISqlView View { get; }
    public override string SourceSchemaName => View.Schema.Name;
    public override string SourceName => View.Name;
    public new SqlViewDataFieldNode this[string fieldName] => GetField( fieldName );

    [Pure]
    public override IReadOnlyCollection<SqlViewDataFieldNode> GetKnownFields()
    {
        _fields ??= CreateDataFields();
        return _fields.Values;
    }

    [Pure]
    public override SqlViewNode As(string alias)
    {
        return new SqlViewNode( View, alias, IsOptional );
    }

    [Pure]
    public override SqlViewNode AsSelf()
    {
        return new SqlViewNode( View, alias: null, IsOptional );
    }

    [Pure]
    public override SqlDataFieldNode GetUnsafeField(string name)
    {
        _fields ??= CreateDataFields();
        return _fields.TryGetValue( name, out var column ) ? column : new SqlRawDataFieldNode( this, name, type: null );
    }

    [Pure]
    public override SqlViewDataFieldNode GetField(string name)
    {
        _fields ??= CreateDataFields();
        return _fields[name];
    }

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
        var result = new Dictionary<string, SqlViewDataFieldNode>( capacity: dataFields.Count, comparer: StringComparer.OrdinalIgnoreCase );

        foreach ( var field in dataFields )
            result.Add( field.Name, new SqlViewDataFieldNode( this, field ) );

        return result;
    }
}
