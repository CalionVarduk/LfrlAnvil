using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlTableBuilderRecordSetNode : SqlRecordSetNode
{
    private readonly string? _alias;
    private readonly ColumnBuilderCollection _columns;

    internal SqlTableBuilderRecordSetNode(ISqlTableBuilder table, string? alias, bool isOptional)
        : base( SqlNodeType.TableBuilderRecordSet, isOptional )
    {
        Table = table;
        _alias = alias;
        IsAliased = _alias is not null;
        _columns = new ColumnBuilderCollection( this );
    }

    public ISqlTableBuilder Table { get; }
    public override bool IsAliased { get; }
    public override string Name => _alias ?? Table.FullName;
    public new SqlColumnBuilderNode this[string fieldName] => GetField( fieldName );

    [Pure]
    public override IReadOnlyCollection<SqlColumnBuilderNode> GetKnownFields()
    {
        return _columns;
    }

    [Pure]
    public override SqlTableBuilderRecordSetNode As(string alias)
    {
        return new SqlTableBuilderRecordSetNode( Table, alias, IsOptional );
    }

    [Pure]
    public override SqlTableBuilderRecordSetNode AsSelf()
    {
        return new SqlTableBuilderRecordSetNode( Table, alias: null, IsOptional );
    }

    [Pure]
    public override SqlDataFieldNode GetUnsafeField(string name)
    {
        return (SqlDataFieldNode?)_columns.TryGet( name ) ?? new SqlRawDataFieldNode( this, name, type: null );
    }

    [Pure]
    public override SqlColumnBuilderNode GetField(string name)
    {
        return _columns.Get( name );
    }

    [Pure]
    public override SqlTableBuilderRecordSetNode MarkAsOptional(bool optional = true)
    {
        return IsOptional != optional
            ? new SqlTableBuilderRecordSetNode( Table, IsAliased ? Name : null, isOptional: optional )
            : this;
    }

    private sealed class ColumnBuilderCollection : IReadOnlyCollection<SqlColumnBuilderNode>
    {
        private readonly SqlTableBuilderRecordSetNode _owner;

        internal ColumnBuilderCollection(SqlTableBuilderRecordSetNode owner)
        {
            _owner = owner;
        }

        public int Count => _owner.Table.Columns.Count;

        [Pure]
        public IEnumerator<SqlColumnBuilderNode> GetEnumerator()
        {
            return ToEnumerable().GetEnumerator();
        }

        [Pure]
        internal SqlColumnBuilderNode Get(string name)
        {
            return GetNode( _owner.Table.Columns.Get( name ) );
        }

        [Pure]
        internal SqlColumnBuilderNode? TryGet(string name)
        {
            return _owner.Table.Columns.TryGet( name, out var column ) ? GetNode( column ) : null;
        }

        [Pure]
        private IEnumerable<SqlColumnBuilderNode> ToEnumerable()
        {
            foreach ( var column in _owner.Table.Columns )
                yield return GetNode( column );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private SqlColumnBuilderNode GetNode(ISqlColumnBuilder column)
        {
            return new SqlColumnBuilderNode( _owner, column, _owner.IsOptional );
        }

        [Pure]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
