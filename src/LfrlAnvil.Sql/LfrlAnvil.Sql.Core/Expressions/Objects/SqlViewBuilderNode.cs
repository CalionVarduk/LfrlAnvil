using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlViewBuilderNode : SqlRecordSetNode
{
    private readonly FieldCollection _fields;

    internal SqlViewBuilderNode(ISqlViewBuilder view, string? alias, bool isOptional)
        : base( SqlNodeType.ViewBuilder, alias, isOptional )
    {
        View = view;
        _fields = new FieldCollection( this );
    }

    public ISqlViewBuilder View { get; }
    public override string SourceSchemaName => View.Schema.Name;
    public override string SourceName => View.Name;
    public new SqlQueryDataFieldNode this[string fieldName] => GetField( fieldName );

    [Pure]
    public override IReadOnlyCollection<SqlQueryDataFieldNode> GetKnownFields()
    {
        return _fields;
    }

    [Pure]
    public override SqlViewBuilderNode As(string alias)
    {
        return new SqlViewBuilderNode( View, alias, IsOptional );
    }

    [Pure]
    public override SqlViewBuilderNode AsSelf()
    {
        return new SqlViewBuilderNode( View, alias: null, IsOptional );
    }

    [Pure]
    public override SqlDataFieldNode GetUnsafeField(string name)
    {
        return (SqlDataFieldNode?)_fields.TryGet( name ) ?? new SqlRawDataFieldNode( this, name, type: null );
    }

    [Pure]
    public override SqlQueryDataFieldNode GetField(string name)
    {
        return _fields.Get( name );
    }

    [Pure]
    public override SqlViewBuilderNode MarkAsOptional(bool optional = true)
    {
        return IsOptional != optional
            ? new SqlViewBuilderNode( View, Alias, isOptional: optional )
            : this;
    }

    private sealed class FieldCollection : IReadOnlyCollection<SqlQueryDataFieldNode>
    {
        private readonly SqlViewBuilderNode _owner;

        internal FieldCollection(SqlViewBuilderNode owner)
        {
            _owner = owner;
        }

        public int Count => _owner.View.Source.ExtractKnownDataFieldCount();

        [Pure]
        public IEnumerator<SqlQueryDataFieldNode> GetEnumerator()
        {
            return _owner.View.Source.ExtractKnownDataFields( _owner ).Values.GetEnumerator();
        }

        [Pure]
        internal SqlQueryDataFieldNode Get(string name)
        {
            var info = _owner.View.Source.TryFindKnownDataFieldInfo( name );
            if ( info is null )
                throw new KeyNotFoundException( ExceptionResources.FieldDoesNotExist( name ) );

            return GetNode( name, info.Value );
        }

        [Pure]
        internal SqlQueryDataFieldNode? TryGet(string name)
        {
            var info = _owner.View.Source.TryFindKnownDataFieldInfo( name );
            return info is not null ? GetNode( name, info.Value ) : null;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private SqlQueryDataFieldNode GetNode(string name, SqlQueryExpressionNode.KnownDataFieldInfo info)
        {
            return new SqlQueryDataFieldNode( _owner, name, info.Selection, info.Expression );
        }

        [Pure]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
