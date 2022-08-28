using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal sealed class InlineDelegateParametersMap
{
    private readonly Dictionary<int, (int StartIndex, int Count)> _stateParameterRanges;
    private readonly Dictionary<StringSlice, ParameterExpression> _map;
    private readonly List<ParameterExpression> _order;
    private int _activeStateId;

    internal InlineDelegateParametersMap(ExpressionBuilderState state)
    {
        _activeStateId = state.Id;
        _stateParameterRanges = new Dictionary<int, (int, int)> { { _activeStateId, (0, 0) } };
        _map = new Dictionary<StringSlice, ParameterExpression>();
        _order = new List<ParameterExpression>();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool IsLocked(ExpressionBuilderState state)
    {
        return state.Id != _activeStateId;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Lock(ExpressionBuilderState state)
    {
        Assume.Equals( IsLocked( state ), false, nameof( IsLocked ) );
        _activeStateId = -1;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Activate(ExpressionBuilderState state)
    {
        Assume.Equals( _activeStateId, -1, nameof( _activeStateId ) );
        _activeStateId = state.Id;
        _stateParameterRanges.Add( _activeStateId, (_order.Count, 0) );
    }

    internal bool TryAdd(Type type, StringSlice name)
    {
        Assume.NotEquals( _activeStateId, -1, nameof( _activeStateId ) );

        if ( _map.ContainsKey( name ) )
            return false;

        var range = _stateParameterRanges[_activeStateId];
        _stateParameterRanges[_activeStateId] = (range.StartIndex, range.Count + 1);

        var parameter = Expression.Parameter( type, name.ToString() );
        _map.Add( name, parameter );
        _order.Add( parameter );
        return true;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ParameterExpression? TryGet(StringSlice name)
    {
        return _map.GetValueOrDefault( name );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal IEnumerable<ParameterExpression> GetParameters(ExpressionBuilderState state)
    {
        var range = _stateParameterRanges[state.Id];
        return _order.Skip( range.StartIndex ).Take( range.Count );
    }
}
