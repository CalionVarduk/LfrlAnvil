using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Expressions.Visitors;

public sealed class SqlNodeInterpreterContext
{
    private Dictionary<string, TypeNullability?>? _parameters;

    private SqlNodeInterpreterContext(StringBuilder sql)
    {
        Sql = sql;
        Indent = 0;
        _parameters = null;
        ChildDepth = 0;
    }

    public StringBuilder Sql { get; }
    public int Indent { get; private set; }
    public int ChildDepth { get; private set; }

    public IReadOnlyCollection<KeyValuePair<string, TypeNullability?>> Parameters =>
        (IReadOnlyCollection<KeyValuePair<string, TypeNullability?>>?)_parameters ??
        Array.Empty<KeyValuePair<string, TypeNullability?>>();

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

    public void IncreaseChildDepth()
    {
        ++ChildDepth;
    }

    public void DecreaseChildDepth()
    {
        if ( ChildDepth > 0 )
            --ChildDepth;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TemporaryChildDepthIncrease TempChildDepthIncrease()
    {
        return new TemporaryChildDepthIncrease( this );
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

    public void AddParameter(string name, TypeNullability? type)
    {
        if ( _parameters is null )
        {
            _parameters = new Dictionary<string, TypeNullability?>( comparer: SqlHelpers.NameComparer );
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

    public bool TryGetParameterType(string name, out TypeNullability? result)
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
        ChildDepth = 0;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlNodeInterpreterContextSnapshot ToSnapshot()
    {
        return new SqlNodeInterpreterContextSnapshot( this );
    }

    public readonly struct TemporaryChildDepthIncrease : IDisposable
    {
        private readonly SqlNodeInterpreterContext _context;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal TemporaryChildDepthIncrease(SqlNodeInterpreterContext context)
        {
            _context = context;
            context.IncreaseChildDepth();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Dispose()
        {
            _context.DecreaseChildDepth();
        }
    }

    public readonly struct TemporaryIndentIncrease : IDisposable
    {
        private readonly SqlNodeInterpreterContext _context;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal TemporaryIndentIncrease(SqlNodeInterpreterContext context)
        {
            _context = context;
            context.IncreaseIndent();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Dispose()
        {
            _context.DecreaseIndent();
        }
    }
}
