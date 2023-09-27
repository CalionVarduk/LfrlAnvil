using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlViewRecordSetNode : SqlRecordSetNode
{
    private Dictionary<string, SqlViewDataFieldNode>? _fields;

    internal SqlViewRecordSetNode(ISqlView view, string? alias, bool isOptional)
        : base( SqlNodeType.ViewRecordSet, isOptional )
    {
        View = view;
        Name = alias ?? View.FullName;
        IsAliased = alias is not null;
        _fields = null;
    }

    public ISqlView View { get; }
    public override string Name { get; }
    public override bool IsAliased { get; }
    public new SqlViewDataFieldNode this[string fieldName] => GetField( fieldName );

    [Pure]
    public override IReadOnlyCollection<SqlViewDataFieldNode> GetKnownFields()
    {
        _fields ??= CreateDataFields();
        return _fields.Values;
    }

    [Pure]
    public override SqlViewRecordSetNode As(string alias)
    {
        return new SqlViewRecordSetNode( View, alias, IsOptional );
    }

    [Pure]
    public override SqlViewRecordSetNode AsSelf()
    {
        return new SqlViewRecordSetNode( View, alias: null, IsOptional );
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
    public override SqlViewRecordSetNode MarkAsOptional(bool optional = true)
    {
        return IsOptional != optional
            ? new SqlViewRecordSetNode( View, alias: IsAliased ? Name : null, isOptional: optional )
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
