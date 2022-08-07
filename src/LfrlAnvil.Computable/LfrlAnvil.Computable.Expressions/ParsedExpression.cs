using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions;

public sealed class ParsedExpression<TArg, TResult> : IParsedExpression<TArg, TResult>
{
    private readonly IReadOnlyDictionary<StringSlice, int> _argumentIndexes;
    private readonly IReadOnlyDictionary<StringSlice, TArg?> _boundArguments;

    internal ParsedExpression(
        string input,
        Expression<Func<TArg?[], TResult>> expression,
        IReadOnlyDictionary<StringSlice, int> argumentIndexes,
        IReadOnlyDictionary<StringSlice, TArg?> boundArguments)
    {
        Input = input;
        Expression = expression;
        _argumentIndexes = argumentIndexes;
        _boundArguments = boundArguments;
    }

    public string Input { get; }
    public Expression<Func<TArg?[], TResult>> Expression { get; }

    [Pure]
    public override string ToString()
    {
        return $"[{typeof( TArg ).FullName} => {typeof( TResult ).FullName}] {Input}";
    }

    [Pure]
    public int GetArgumentCount()
    {
        return GetUnboundArgumentCount() + GetBoundArgumentCount();
    }

    [Pure]
    public int GetUnboundArgumentCount()
    {
        return _argumentIndexes.Count;
    }

    [Pure]
    public int GetBoundArgumentCount()
    {
        return _boundArguments.Count;
    }

    [Pure]
    public IEnumerable<ReadOnlyMemory<char>> GetArgumentNames()
    {
        return GetUnboundArgumentNames().Concat( GetBoundArgumentNames() );
    }

    [Pure]
    public IEnumerable<ReadOnlyMemory<char>> GetUnboundArgumentNames()
    {
        return _argumentIndexes.Select( kv => kv.Key.AsMemory() );
    }

    [Pure]
    public IEnumerable<ReadOnlyMemory<char>> GetBoundArgumentNames()
    {
        return _boundArguments.Select( kv => kv.Key.AsMemory() );
    }

    [Pure]
    public bool ContainsArgument(string argumentName)
    {
        return ContainsArgument( StringSlice.Create( argumentName ) );
    }

    [Pure]
    public bool ContainsArgument(ReadOnlyMemory<char> argumentName)
    {
        return ContainsArgument( StringSlice.Create( argumentName ) );
    }

    [Pure]
    public bool ContainsUnboundArgument(string argumentName)
    {
        return ContainsUnboundArgument( StringSlice.Create( argumentName ) );
    }

    [Pure]
    public bool ContainsUnboundArgument(ReadOnlyMemory<char> argumentName)
    {
        return ContainsUnboundArgument( StringSlice.Create( argumentName ) );
    }

    [Pure]
    public bool ContainsBoundArgument(string argumentName)
    {
        return ContainsBoundArgument( StringSlice.Create( argumentName ) );
    }

    [Pure]
    public bool ContainsBoundArgument(ReadOnlyMemory<char> argumentName)
    {
        return ContainsBoundArgument( StringSlice.Create( argumentName ) );
    }

    [Pure]
    public int GetUnboundArgumentIndex(string argumentName)
    {
        return GetUnboundArgumentIndex( StringSlice.Create( argumentName ) );
    }

    [Pure]
    public int GetUnboundArgumentIndex(ReadOnlyMemory<char> argumentName)
    {
        return GetUnboundArgumentIndex( StringSlice.Create( argumentName ) );
    }

    public bool TryGetBoundArgumentValue(string argumentName, out TArg? result)
    {
        return TryGetBoundArgumentValue( StringSlice.Create( argumentName ), out result );
    }

    public bool TryGetBoundArgumentValue(ReadOnlyMemory<char> argumentName, out TArg? result)
    {
        return TryGetBoundArgumentValue( StringSlice.Create( argumentName ), out result );
    }

    [Pure]
    public ReadOnlyMemory<char> GetUnboundArgumentName(int index)
    {
        foreach ( var (name, i) in _argumentIndexes )
        {
            if ( index != i )
                continue;

            return name.AsMemory();
        }

        return string.Empty.AsMemory();
    }

    [Pure]
    public ParsedExpression<TArg, TResult> BindArguments(IEnumerable<KeyValuePair<string, TArg?>> arguments)
    {
        return BindArguments( arguments.Select( kv => KeyValuePair.Create( StringSlice.Create( kv.Key ), kv.Value ) ) );
    }

    [Pure]
    public ParsedExpression<TArg, TResult> BindArguments(params KeyValuePair<string, TArg?>[] arguments)
    {
        return BindArguments( arguments.AsEnumerable() );
    }

    [Pure]
    public ParsedExpression<TArg, TResult> BindArguments(IEnumerable<KeyValuePair<ReadOnlyMemory<char>, TArg?>> arguments)
    {
        return BindArguments( arguments.Select( kv => KeyValuePair.Create( StringSlice.Create( kv.Key ), kv.Value ) ) );
    }

    [Pure]
    public ParsedExpression<TArg, TResult> BindArguments(params KeyValuePair<ReadOnlyMemory<char>, TArg?>[] arguments)
    {
        return BindArguments( arguments.AsEnumerable() );
    }

    [Pure]
    public ParsedExpression<TArg, TResult> BindArguments(IEnumerable<KeyValuePair<int, TArg?>> arguments)
    {
        if ( arguments.TryGetNonEnumeratedCount( out var count ) && count == 0 )
            return this;

        var argumentsToBind = new Dictionary<int, TArg?>();

        foreach ( var (index, value) in arguments )
        {
            if ( index < 0 || index >= _argumentIndexes.Count )
                throw new ParsedExpressionArgumentBindingException();

            argumentsToBind[index] = value;
        }

        if ( argumentsToBind.Count == 0 )
            return this;

        var parameterBinder = new ParameterBinder( this, argumentsToBind );
        var bodyExpression = parameterBinder.Visit( Expression.Body );

        var expression = System.Linq.Expressions.Expression.Lambda<Func<TArg?[], TResult>>(
            bodyExpression,
            parameterBinder.ParameterExpression );

        return new ParsedExpression<TArg, TResult>( Input, expression, parameterBinder.ArgumentIndexes, parameterBinder.BoundArguments );
    }

    [Pure]
    public ParsedExpression<TArg, TResult> BindArguments(params KeyValuePair<int, TArg?>[] arguments)
    {
        return BindArguments( arguments.AsEnumerable() );
    }

    [Pure]
    public ParsedExpressionDelegate<TArg, TResult> Compile()
    {
        return new ParsedExpressionDelegate<TArg, TResult>( Expression.Compile(), _argumentIndexes );
    }

    [Pure]
    private bool ContainsArgument(StringSlice argumentName)
    {
        return ContainsUnboundArgument( argumentName ) || ContainsBoundArgument( argumentName );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool ContainsUnboundArgument(StringSlice argumentName)
    {
        return _argumentIndexes.ContainsKey( argumentName );
    }

    [Pure]
    private bool ContainsBoundArgument(StringSlice argumentName)
    {
        return _boundArguments.ContainsKey( argumentName );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private int GetUnboundArgumentIndex(StringSlice argumentName)
    {
        return _argumentIndexes.TryGetValue( argumentName, out var index ) ? index : -1;
    }

    private bool TryGetBoundArgumentValue(StringSlice argumentName, out TArg? result)
    {
        return _boundArguments.TryGetValue( argumentName, out result );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ParsedExpression<TArg, TResult> BindArguments(IEnumerable<KeyValuePair<StringSlice, TArg?>> arguments)
    {
        return BindArguments( arguments.Select( kv => KeyValuePair.Create( GetUnboundArgumentIndex( kv.Key ), kv.Value ) ) );
    }

    private sealed class ParameterBinder : ExpressionVisitor
    {
        private readonly Expression[] _expressions;

        public ParameterBinder(ParsedExpression<TArg, TResult> source, IReadOnlyDictionary<int, TArg?> argumentsToBind)
        {
            ParameterExpression = source.Expression.Parameters[0];
            ArgumentIndexes = new Dictionary<StringSlice, int>();
            BoundArguments = new Dictionary<StringSlice, TArg?>( source._boundArguments );
            _expressions = new Expression[source._argumentIndexes.Count];

            var exprIndex = 0;
            foreach ( var (name, i) in source._argumentIndexes )
            {
                if ( argumentsToBind.TryGetValue( i, out var value ) )
                {
                    _expressions[exprIndex++] = System.Linq.Expressions.Expression.Constant( value );
                    BoundArguments.Add( name, value );
                    continue;
                }

                var index = ArgumentIndexes.Count;
                ArgumentIndexes.Add( name, index );

                var indexExpression = System.Linq.Expressions.Expression.Constant( index );
                var arrayIndexExpression = System.Linq.Expressions.Expression.ArrayIndex( ParameterExpression, indexExpression );
                _expressions[exprIndex++] = arrayIndexExpression;
            }
        }

        public ParameterExpression ParameterExpression { get; }
        public Dictionary<StringSlice, int> ArgumentIndexes { get; }
        public Dictionary<StringSlice, TArg?> BoundArguments { get; }

        public override Expression Visit(Expression? node)
        {
            Assume.IsNotNull( node, nameof( node ) );

            if ( node.NodeType != ExpressionType.ArrayIndex )
                return base.Visit( node );

            var arrayIndexExpression = (BinaryExpression)node;
            if ( arrayIndexExpression.Left.NodeType != ExpressionType.Parameter )
                return base.Visit( node );

            if ( arrayIndexExpression.Right.NodeType != ExpressionType.Constant )
                return base.Visit( node );

            var indexExpression = (ConstantExpression)arrayIndexExpression.Right;
            var indexValue = (int)indexExpression.Value!;

            if ( indexValue < 0 || indexValue >= _expressions.Length )
                return base.Visit( node );

            return _expressions[indexValue];
        }
    }

    [Pure]
    IParsedExpression<TArg, TResult> IParsedExpression<TArg, TResult>.BindArguments(IEnumerable<KeyValuePair<string, TArg?>> arguments)
    {
        return BindArguments( arguments );
    }

    [Pure]
    IParsedExpression<TArg, TResult> IParsedExpression<TArg, TResult>.BindArguments(params KeyValuePair<string, TArg?>[] arguments)
    {
        return BindArguments( arguments );
    }

    [Pure]
    IParsedExpression<TArg, TResult> IParsedExpression<TArg, TResult>.BindArguments(
        IEnumerable<KeyValuePair<ReadOnlyMemory<char>, TArg?>> arguments)
    {
        return BindArguments( arguments );
    }

    [Pure]
    IParsedExpression<TArg, TResult> IParsedExpression<TArg, TResult>.BindArguments(
        params KeyValuePair<ReadOnlyMemory<char>, TArg?>[] arguments)
    {
        return BindArguments( arguments );
    }

    [Pure]
    IParsedExpression<TArg, TResult> IParsedExpression<TArg, TResult>.BindArguments(IEnumerable<KeyValuePair<int, TArg?>> arguments)
    {
        return BindArguments( arguments );
    }

    [Pure]
    IParsedExpression<TArg, TResult> IParsedExpression<TArg, TResult>.BindArguments(params KeyValuePair<int, TArg?>[] arguments)
    {
        return BindArguments( arguments );
    }

    [Pure]
    IParsedExpressionDelegate<TArg, TResult> IParsedExpression<TArg, TResult>.Compile()
    {
        return Compile();
    }
}
