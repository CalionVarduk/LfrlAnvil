using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlQueryRecordSetNode : SqlRecordSetNode
{
    private Dictionary<string, SqlQueryDataFieldNode>? _fields;

    internal SqlQueryRecordSetNode(SqlQueryExpressionNode query, string alias, bool isOptional)
        : base( isOptional )
    {
        Query = query;
        Name = alias;
        _fields = null;
    }

    public SqlQueryExpressionNode Query { get; }
    public override string Name { get; }
    public override bool IsAliased => true;
    public new SqlQueryDataFieldNode this[string fieldName] => GetField( fieldName );

    [Pure]
    public override IReadOnlyCollection<SqlQueryDataFieldNode> GetKnownFields()
    {
        _fields ??= CreateKnownFields();
        return _fields.Values;
    }

    [Pure]
    public override SqlDataFieldNode GetUnsafeField(string name)
    {
        _fields ??= CreateKnownFields();
        return _fields.TryGetValue( name, out var field ) ? field : new SqlRawDataFieldNode( this, name, type: null );
    }

    [Pure]
    public override SqlQueryDataFieldNode GetField(string name)
    {
        _fields ??= CreateKnownFields();
        return _fields[name];
    }

    [Pure]
    public override SqlQueryRecordSetNode As(string alias)
    {
        return new SqlQueryRecordSetNode( Query, alias, IsOptional );
    }

    [Pure]
    public override SqlQueryRecordSetNode AsSelf()
    {
        return this;
    }

    [Pure]
    public override SqlQueryRecordSetNode MarkAsOptional(bool optional = true)
    {
        return IsOptional != optional
            ? new SqlQueryRecordSetNode( Query, Name, isOptional: optional )
            : this;
    }

    protected override void ToString(StringBuilder builder, int indent)
    {
        var queryIndent = indent + DefaultIndent;
        AppendTo( builder.Append( '(' ).Indent( queryIndent ), Query, queryIndent );
        builder.Indent( indent ).Append( ')' ).Append( ' ' ).Append( "AS" ).Append( ' ' ).Append( '[' ).Append( Name ).Append( ']' );
    }

    [Pure]
    private Dictionary<string, SqlQueryDataFieldNode> CreateKnownFields()
    {
        var selections = Query.Selection.Span;
        var converter = new FieldConverter( this, selections.Length );

        foreach ( var selection in selections )
        {
            converter.Selection = selection;
            selection.Convert( converter );
        }

        return converter.Fields;
    }

    private sealed class FieldConverter : ISqlSelectNodeConverter
    {
        private readonly SqlQueryRecordSetNode _recordSet;

        internal FieldConverter(SqlQueryRecordSetNode recordSet, int capacity)
        {
            _recordSet = recordSet;
            Fields = new Dictionary<string, SqlQueryDataFieldNode>( capacity: capacity, comparer: StringComparer.OrdinalIgnoreCase );
        }

        internal Dictionary<string, SqlQueryDataFieldNode> Fields { get; }
        internal SqlSelectNode? Selection { get; set; }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Add(string name, SqlExpressionNode? expression)
        {
            Assume.IsNotNull( Selection, nameof( Selection ) );
            var field = new SqlQueryDataFieldNode( _recordSet, name, Selection, expression );
            Fields.Add( field.Name, field );
        }
    }
}
