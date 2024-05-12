using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Expressions.Objects;

/// <summary>
/// Represents an SQL syntax tree node that defines a single record set based on an <see cref="ISqlTableBuilder"/> instance.
/// </summary>
public sealed class SqlTableBuilderNode : SqlRecordSetNode
{
    private readonly ColumnBuilderCollection _columns;

    internal SqlTableBuilderNode(ISqlTableBuilder table, string? alias, bool isOptional)
        : base( SqlNodeType.TableBuilder, alias, isOptional )
    {
        Table = table;
        _columns = new ColumnBuilderCollection( this );
    }

    /// <summary>
    /// Underlying <see cref="ISqlTableBuilder"/> instance.
    /// </summary>
    public ISqlTableBuilder Table { get; }

    /// <inheritdoc />
    public override SqlRecordSetInfo Info => Table.Info;

    /// <inheritdoc cref="SqlRecordSetNode.this[string]" />
    public new SqlColumnBuilderNode this[string fieldName] => GetField( fieldName );

    /// <inheritdoc />
    [Pure]
    public override IReadOnlyCollection<SqlColumnBuilderNode> GetKnownFields()
    {
        return _columns;
    }

    /// <inheritdoc />
    [Pure]
    public override SqlTableBuilderNode As(string alias)
    {
        return new SqlTableBuilderNode( Table, alias, IsOptional );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlTableBuilderNode AsSelf()
    {
        return new SqlTableBuilderNode( Table, alias: null, IsOptional );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlDataFieldNode GetUnsafeField(string name)
    {
        return ( SqlDataFieldNode? )_columns.TryGet( name ) ?? new SqlRawDataFieldNode( this, name, type: null );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlColumnBuilderNode GetField(string name)
    {
        return _columns.Get( name );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlTableBuilderNode MarkAsOptional(bool optional = true)
    {
        return IsOptional != optional
            ? new SqlTableBuilderNode( Table, Alias, isOptional: optional )
            : this;
    }

    private sealed class ColumnBuilderCollection : IReadOnlyCollection<SqlColumnBuilderNode>
    {
        private readonly SqlTableBuilderNode _owner;

        internal ColumnBuilderCollection(SqlTableBuilderNode owner)
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
            var column = _owner.Table.Columns.TryGet( name );
            return column is not null ? GetNode( column ) : null;
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
