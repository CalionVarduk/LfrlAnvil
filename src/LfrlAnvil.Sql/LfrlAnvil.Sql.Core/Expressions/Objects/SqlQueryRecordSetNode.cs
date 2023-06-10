using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlQueryRecordSetNode : SqlRecordSetNode
{
    private Dictionary<string, SqlRawDataFieldNode>? _fields;

    internal SqlQueryRecordSetNode(SqlQueryExpressionNode query, string alias, bool isOptional)
        : base( isOptional )
    {
        Ensure.IsGreaterThan( query.Selection.Length, 0, nameof( query.Selection ) + '.' + nameof( query.Selection.Length ) );
        Query = query;
        Name = alias;
        _fields = null;
    }

    public SqlQueryExpressionNode Query { get; }
    public override string Name { get; }
    public override bool IsAliased => true;

    [Pure]
    public override IReadOnlyCollection<SqlDataFieldNode> GetKnownFields()
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
    public override SqlDataFieldNode GetField(string name)
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
    private Dictionary<string, SqlRawDataFieldNode> CreateKnownFields()
    {
        var selections = Query.Selection.Span;
        var initializer = new FieldInitializer( this, selections.Length );

        foreach ( var selection in selections )
            selection.RegisterKnownFields( initializer );

        return initializer.Fields;
    }

    public readonly struct FieldInitializer
    {
        internal FieldInitializer(SqlQueryRecordSetNode query, int capacity)
        {
            Query = query;
            Fields = new Dictionary<string, SqlRawDataFieldNode>( capacity: capacity, comparer: StringComparer.OrdinalIgnoreCase );
        }

        public SqlQueryRecordSetNode Query { get; }
        internal Dictionary<string, SqlRawDataFieldNode> Fields { get; }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void AddField(string name, SqlExpressionType? type)
        {
            var field = new SqlRawDataFieldNode( Query, name, GetType( type ) );
            Fields.Add( field.Name, field );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private SqlExpressionType? GetType(SqlExpressionType? type)
        {
            if ( type is null )
                return null;

            return Query.IsOptional ? type.Value.MakeNullable() : type;
        }
    }
}
