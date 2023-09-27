using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions;

public abstract class SqlQueryExpressionNode : SqlExpressionNode
{
    internal SqlQueryExpressionNode(SqlNodeType nodeType)
        : base( nodeType ) { }

    public abstract ReadOnlyMemory<SqlSelectNode> Selection { get; }

    public virtual void ReduceKnownDataFieldExpressions(Action<KeyValuePair<string, KnownDataFieldInfo>> callback)
    {
        var visitor = new DataFieldInfoVisitor( callback );
        foreach ( var selection in Selection.Span )
        {
            visitor.Selection = selection;
            selection.VisitExpressions( visitor );
        }
    }

    [Pure]
    protected internal virtual Dictionary<string, SqlQueryDataFieldNode> ExtractKnownDataFields(SqlRecordSetNode recordSet)
    {
        var visitor = new DataFieldVisitor( recordSet, Selection.Length );
        foreach ( var selection in Selection.Span )
        {
            visitor.Selection = selection;
            selection.VisitExpressions( visitor );
        }

        return visitor.DataFields;
    }

    [Pure]
    protected internal virtual int ExtractKnownDataFieldCount()
    {
        var counter = new DataFieldCounter();
        foreach ( var selection in Selection.Span )
            selection.VisitExpressions( counter );

        return counter.Count;
    }

    [Pure]
    protected internal virtual KnownDataFieldInfo? TryFindKnownDataFieldInfo(string name)
    {
        var finder = new DataFieldFinder( name );
        foreach ( var selection in Selection.Span )
        {
            finder.Selection = selection;
            selection.VisitExpressions( finder );
        }

        return finder.FoundInfo;
    }

    public readonly record struct KnownDataFieldInfo(SqlSelectNode Selection, SqlExpressionNode? Expression);

    private sealed class DataFieldVisitor : ISqlSelectNodeExpressionVisitor
    {
        private readonly SqlRecordSetNode _recordSet;

        internal DataFieldVisitor(SqlRecordSetNode recordSet, int capacity)
        {
            _recordSet = recordSet;
            DataFields = new Dictionary<string, SqlQueryDataFieldNode>( capacity: capacity, comparer: StringComparer.OrdinalIgnoreCase );
        }

        internal Dictionary<string, SqlQueryDataFieldNode> DataFields { get; }
        internal SqlSelectNode? Selection { get; set; }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Handle(string name, SqlExpressionNode? expression)
        {
            Assume.IsNotNull( Selection, nameof( Selection ) );
            var field = new SqlQueryDataFieldNode( _recordSet, name, Selection, expression );
            DataFields.Add( field.Name, field );
        }
    }

    private sealed class DataFieldInfoVisitor : ISqlSelectNodeExpressionVisitor
    {
        internal DataFieldInfoVisitor(Action<KeyValuePair<string, KnownDataFieldInfo>> callback)
        {
            Callback = callback;
        }

        internal Action<KeyValuePair<string, KnownDataFieldInfo>> Callback { get; }
        internal SqlSelectNode? Selection { get; set; }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Handle(string name, SqlExpressionNode? expression)
        {
            Assume.IsNotNull( Selection, nameof( Selection ) );
            Callback( KeyValuePair.Create( name, new KnownDataFieldInfo( Selection, expression ) ) );
        }
    }

    private sealed class DataFieldCounter : ISqlSelectNodeExpressionVisitor
    {
        private readonly HashSet<string> _names;

        internal DataFieldCounter()
        {
            _names = new HashSet<string>( StringComparer.OrdinalIgnoreCase );
            Count = 0;
        }

        internal int Count { get; private set; }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Handle(string name, SqlExpressionNode? expression)
        {
            if ( _names.Add( name ) )
                ++Count;
        }
    }

    private sealed class DataFieldFinder : ISqlSelectNodeExpressionVisitor
    {
        internal DataFieldFinder(string name)
        {
            Name = name;
            FoundInfo = null;
        }

        internal string Name { get; }
        internal SqlSelectNode? Selection { get; set; }
        internal KnownDataFieldInfo? FoundInfo { get; private set; }

        public void Handle(string name, SqlExpressionNode? expression)
        {
            Assume.IsNotNull( Selection, nameof( Selection ) );
            if ( ! Name.Equals( name, StringComparison.OrdinalIgnoreCase ) )
                return;

            if ( FoundInfo is not null )
                throw new ArgumentException( ExceptionResources.FieldExistsMoreThanOnce( name ), nameof( name ) );

            FoundInfo = new KnownDataFieldInfo( Selection, expression );
        }
    }
}
