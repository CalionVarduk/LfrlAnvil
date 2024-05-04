using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Computable.Expressions.Internal;
using LfrlAnvil.Computable.Expressions.Internal.Delegates;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions;

/// <inheritdoc />
public sealed class ParsedExpression<TArg, TResult> : IParsedExpression<TArg, TResult>
{
    private readonly ParsedExpressionFactory _factory;
    private readonly IReadOnlyList<CompilableInlineDelegate> _delegates;

    internal ParsedExpression(
        ParsedExpressionFactory factory,
        string input,
        Expression body,
        ParameterExpression parameter,
        IReadOnlyList<CompilableInlineDelegate> delegates,
        ParsedExpressionUnboundArguments unboundArguments,
        ParsedExpressionBoundArguments<TArg> boundArguments,
        ParsedExpressionDiscardedArguments discardedArguments)
    {
        _factory = factory;
        Input = input;
        Body = body;
        Parameter = parameter;
        _delegates = delegates;
        UnboundArguments = unboundArguments;
        BoundArguments = boundArguments;
        DiscardedArguments = discardedArguments;
    }

    /// <inheritdoc />
    public string Input { get; }

    /// <inheritdoc />
    public Expression Body { get; }

    /// <inheritdoc />
    public ParameterExpression Parameter { get; }

    /// <inheritdoc />
    public ParsedExpressionUnboundArguments UnboundArguments { get; }

    /// <inheritdoc />
    public ParsedExpressionBoundArguments<TArg> BoundArguments { get; }

    /// <inheritdoc />
    public ParsedExpressionDiscardedArguments DiscardedArguments { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="ParsedExpression{TArg,TResult}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{typeof( TArg ).GetDebugString()} => {typeof( TResult ).GetDebugString()}] {Input}";
    }

    /// <inheritdoc cref="IParsedExpression{TArg,TResult}.BindArguments(IEnumerable{KeyValuePair{String,TArg}})" />
    [Pure]
    public ParsedExpression<TArg, TResult> BindArguments(IEnumerable<KeyValuePair<string, TArg?>> arguments)
    {
        return BindArguments( arguments.Select( kv => KeyValuePair.Create( ( StringSegment )kv.Key, kv.Value ) ) );
    }

    /// <inheritdoc cref="IParsedExpression{TArg,TResult}.BindArguments(KeyValuePair{String,TArg}[])" />
    [Pure]
    public ParsedExpression<TArg, TResult> BindArguments(params KeyValuePair<string, TArg?>[] arguments)
    {
        return BindArguments( arguments.AsEnumerable() );
    }

    /// <inheritdoc cref="IParsedExpression{TArg,TResult}.BindArguments(IEnumerable{KeyValuePair{StringSegment,TArg}})" />
    [Pure]
    public ParsedExpression<TArg, TResult> BindArguments(IEnumerable<KeyValuePair<StringSegment, TArg?>> arguments)
    {
        if ( arguments.TryGetNonEnumeratedCount( out var count ) && count == 0 )
            return this;

        var argumentsToBind = new Dictionary<StringSegment, TArg?>( BoundArguments );

        foreach ( var (name, value) in arguments )
        {
            var index = UnboundArguments.GetIndex( name );
            if ( index < 0 || index >= UnboundArguments.Count )
                throw new ParsedExpressionArgumentBindingException();

            argumentsToBind.Add( name, value );
        }

        if ( argumentsToBind.Count == 0 )
            return this;

        if ( _factory.TryCreateInternal<TArg, TResult>( Input, (this, argumentsToBind), out var result, out var errors ) )
            return result;

        throw new ParsedExpressionCreationException( Input, errors );
    }

    /// <inheritdoc cref="IParsedExpression{TArg,TResult}.BindArguments(KeyValuePair{StringSegment,TArg}[])" />
    [Pure]
    public ParsedExpression<TArg, TResult> BindArguments(params KeyValuePair<StringSegment, TArg?>[] arguments)
    {
        return BindArguments( arguments.AsEnumerable() );
    }

    /// <inheritdoc cref="IParsedExpression{TArg,TResult}.BindArguments(IEnumerable{KeyValuePair{int,TArg}})" />
    [Pure]
    public ParsedExpression<TArg, TResult> BindArguments(IEnumerable<KeyValuePair<int, TArg?>> arguments)
    {
        return BindArguments( arguments.Select( kv => KeyValuePair.Create( UnboundArguments.GetName( kv.Key ), kv.Value ) ) );
    }

    /// <inheritdoc cref="IParsedExpression{TArg,TResult}.BindArguments(KeyValuePair{int,TArg}[])" />
    [Pure]
    public ParsedExpression<TArg, TResult> BindArguments(params KeyValuePair<int, TArg?>[] arguments)
    {
        return BindArguments( arguments.AsEnumerable() );
    }

    /// <inheritdoc cref="IParsedExpression{TArg,TResult}.Compile()" />
    [Pure]
    public ParsedExpressionDelegate<TArg, TResult> Compile()
    {
        var body = Body;

        if ( _delegates.Count != 0 )
        {
            var replacements = new ExpressionReplacement[_delegates.Count];
            for ( var i = 0; i < replacements.Length; ++i )
                replacements[i] = _delegates[i].Compile();

            var replacer = new DelegatePlaceholderReplacer( replacements );
            body = replacer.Visit( Body );
        }

        var lambda = Expression.Lambda<Func<TArg?[], TResult>>( body, Parameter );
        return new ParsedExpressionDelegate<TArg, TResult>( lambda.Compile(), UnboundArguments );
    }

    private sealed class DelegatePlaceholderReplacer : ExpressionVisitor
    {
        private readonly ExpressionReplacement[] _replacements;

        internal DelegatePlaceholderReplacer(ExpressionReplacement[] replacements)
        {
            _replacements = replacements;
        }

        [Pure]
        [return: NotNullIfNotNull( "node" )]
        public override Expression? Visit(Expression? node)
        {
            if ( ! ExpressionHelpers.IsLambdaPlaceholder( node ) )
                return base.Visit( node );

            var index = Array.FindIndex( _replacements, x => x.IsMatched( node ) );
            if ( index < 0 )
                return base.Visit( node );

            var result = _replacements[index].Replacement;
            return result;
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
        IEnumerable<KeyValuePair<StringSegment, TArg?>> arguments)
    {
        return BindArguments( arguments );
    }

    [Pure]
    IParsedExpression<TArg, TResult> IParsedExpression<TArg, TResult>.BindArguments(
        params KeyValuePair<StringSegment, TArg?>[] arguments)
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
