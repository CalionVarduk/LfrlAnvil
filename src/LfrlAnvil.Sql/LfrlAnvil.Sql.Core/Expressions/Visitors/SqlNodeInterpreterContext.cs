﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Sql.Expressions.Visitors;

public sealed class SqlNodeInterpreterContext
{
    private Dictionary<string, SqlExpressionType?>? _parameters;

    private SqlNodeInterpreterContext(StringBuilder sql)
    {
        Sql = sql;
        Indent = 0;
        _parameters = null;
        ParentNode = null;
    }

    public StringBuilder Sql { get; }
    public int Indent { get; private set; }
    public SqlNodeBase? ParentNode { get; private set; }

    public IReadOnlyCollection<KeyValuePair<string, SqlExpressionType?>> Parameters =>
        (IReadOnlyCollection<KeyValuePair<string, SqlExpressionType?>>?)_parameters ??
        Array.Empty<KeyValuePair<string, SqlExpressionType?>>();

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlNodeInterpreterContext Create(int capacity = 1024)
    {
        return Create( new StringBuilder( capacity ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlNodeInterpreterContext Create(StringBuilder builder)
    {
        return new SqlNodeInterpreterContext( builder );
    }

    public void IncreaseIndent()
    {
        Indent += 2;
    }

    public void DecreaseIndent()
    {
        if ( Indent <= 2 )
        {
            Indent = 0;
            return;
        }

        Indent -= 2;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TemporaryIndentIncrease TempIndentIncrease()
    {
        return new TemporaryIndentIncrease( this );
    }

    public void SetParentNode(SqlNodeBase node)
    {
        ParentNode = node;
    }

    public void ClearParentNode()
    {
        ParentNode = null;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TemporaryParentNodeUpdate TempParentNodeUpdate(SqlNodeBase node)
    {
        return new TemporaryParentNodeUpdate( this, node );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public StringBuilder AppendIndent()
    {
        return Sql.Indent( Indent );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public StringBuilder AppendShortIndent()
    {
        return Sql.Indent( Math.Max( Indent - 2, 0 ) );
    }

    public void AddParameter(string name, SqlExpressionType? type)
    {
        if ( _parameters is null )
        {
            _parameters = new Dictionary<string, SqlExpressionType?>( comparer: StringComparer.OrdinalIgnoreCase );
            _parameters.Add( name, type );
            return;
        }

        ref var typeRef = ref CollectionsMarshal.GetValueRefOrAddDefault( _parameters, name, out var exists );

        if ( ! exists )
        {
            typeRef = type;
            return;
        }

        if ( typeRef != type )
            typeRef = null;
    }

    public bool TryGetParameterType(string name, out SqlExpressionType? result)
    {
        if ( _parameters is not null )
            return _parameters.TryGetValue( name, out result );

        result = null;
        return false;
    }

    public void Clear()
    {
        Sql.Clear();
        _parameters?.Clear();
        Indent = 0;
        ClearParentNode();
    }

    public readonly struct TemporaryParentNodeUpdate : IDisposable
    {
        private readonly SqlNodeInterpreterContext _context;
        private readonly SqlNodeBase? _prevParentNode;

        internal TemporaryParentNodeUpdate(SqlNodeInterpreterContext context, SqlNodeBase parentNode)
        {
            _context = context;
            _prevParentNode = context.ParentNode;
            context.SetParentNode( parentNode );
        }

        public void Dispose()
        {
            _context.ParentNode = _prevParentNode;
        }
    }

    public readonly struct TemporaryIndentIncrease : IDisposable
    {
        private readonly SqlNodeInterpreterContext _context;

        internal TemporaryIndentIncrease(SqlNodeInterpreterContext context)
        {
            _context = context;
            context.IncreaseIndent();
        }

        public void Dispose()
        {
            _context.DecreaseIndent();
        }
    }
}