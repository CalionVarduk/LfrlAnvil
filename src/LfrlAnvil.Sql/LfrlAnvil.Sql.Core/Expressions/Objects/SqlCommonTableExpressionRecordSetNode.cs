using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text;

namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlCommonTableExpressionRecordSetNode : SqlRecordSetNode
{
    private Dictionary<string, SqlQueryDataFieldNode>? _fields;

    internal SqlCommonTableExpressionRecordSetNode(SqlCommonTableExpressionNode commonTableExpression, string? alias, bool isOptional)
        : base( isOptional )
    {
        CommonTableExpression = commonTableExpression;
        Alias = alias;
        _fields = null;
    }

    public string? Alias { get; }
    public SqlCommonTableExpressionNode CommonTableExpression { get; }
    public override string Name => Alias ?? CommonTableExpression.Name;

    [MemberNotNullWhen( true, nameof( Alias ) )]
    public override bool IsAliased => Alias is not null;

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
    public override SqlCommonTableExpressionRecordSetNode As(string alias)
    {
        return new SqlCommonTableExpressionRecordSetNode( CommonTableExpression, alias, IsOptional );
    }

    [Pure]
    public override SqlCommonTableExpressionRecordSetNode AsSelf()
    {
        return IsAliased ? new SqlCommonTableExpressionRecordSetNode( CommonTableExpression, alias: null, IsOptional ) : this;
    }

    [Pure]
    public override SqlCommonTableExpressionRecordSetNode MarkAsOptional(bool optional = true)
    {
        return IsOptional != optional
            ? new SqlCommonTableExpressionRecordSetNode( CommonTableExpression, Alias, optional )
            : this;
    }

    protected override void ToString(StringBuilder builder, int indent)
    {
        builder.Append( '[' ).Append( CommonTableExpression.Name ).Append( ']' );
        if ( IsAliased )
            builder.Append( ' ' ).Append( "AS" ).Append( ' ' ).Append( '[' ).Append( Alias ).Append( ']' );
    }

    [Pure]
    private Dictionary<string, SqlQueryDataFieldNode> CreateKnownFields()
    {
        var selections = CommonTableExpression.Query.Selection.Span;
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
        private readonly SqlCommonTableExpressionRecordSetNode _recordSet;

        internal FieldConverter(SqlCommonTableExpressionRecordSetNode recordSet, int capacity)
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
            Fields.Add( name, field );
        }
    }
}
