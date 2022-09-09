using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using LfrlAnvil.Computable.Expressions.Errors;
using LfrlAnvil.Computable.Expressions.Internal.Delegates;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal sealed class LocalTermsCollection
{
    private readonly List<Expression> _argumentAccessExpressions;
    private readonly IReadOnlyDictionary<StringSlice, ConstantExpression>? _boundArguments;
    private readonly Dictionary<StringSlice, VariableAssignment> _variables;
    private readonly List<VariableAssignment> _variableAssignments;
    private StringSlice? _activeNewVariable;

    internal LocalTermsCollection(
        ParameterExpression parameterExpression,
        IReadOnlyDictionary<StringSlice, ConstantExpression>? boundArguments)
    {
        ParameterExpression = parameterExpression;
        _boundArguments = boundArguments;
        ArgumentIndexes = new Dictionary<StringSlice, int>();
        _argumentAccessExpressions = new List<Expression>();
        _variables = new Dictionary<StringSlice, VariableAssignment>();
        _variableAssignments = new List<VariableAssignment>();
        _activeNewVariable = null;
    }

    internal ParameterExpression ParameterExpression { get; }
    internal Dictionary<StringSlice, int> ArgumentIndexes { get; }
    internal IReadOnlyList<VariableAssignment> VariableAssignments => _variableAssignments;

    [Pure]
    internal VariableAssignment[] GetUsedVariableAssignments()
    {
        var count = 0;
        for ( var i = 0; i < _variableAssignments.Count; ++i )
        {
            if ( _variableAssignments[i].IsUsed )
                ++count;
        }

        if ( count == 0 )
            return Array.Empty<VariableAssignment>();

        var index = 0;
        var result = new VariableAssignment[count];
        for ( var i = 0; i < _variableAssignments.Count; ++i )
        {
            if ( _variableAssignments[i].IsUsed )
                result[index++] = _variableAssignments[i];
        }

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal int GetNextArgumentIndex()
    {
        return ArgumentIndexes.Count;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool ContainsArgument(StringSlice name)
    {
        return ArgumentIndexes.ContainsKey( name );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool ContainsVariable(StringSlice name)
    {
        return IsActiveVariable( name ) || _variables.ContainsKey( name );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool IsActiveVariable(StringSlice name)
    {
        return _activeNewVariable == name;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal Expression GetArgumentAccess(int index)
    {
        return _argumentAccessExpressions[index];
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryGetVariable(StringSlice name, [MaybeNullWhen( false )] out VariableAssignment result)
    {
        return _variables.TryGetValue( name, out result );
    }

    internal (Expression Result, int? Index) GetOrAddArgumentAccess(StringSlice name)
    {
        if ( _boundArguments is not null && _boundArguments.TryGetValue( name, out var constant ) )
            return (constant, null);

        if ( ArgumentIndexes.TryGetValue( name, out var index ) )
            return (GetArgumentAccess( index ), index);

        index = GetNextArgumentIndex();
        var result = ParameterExpression.CreateArgumentAccess( index );

        ArgumentIndexes.Add( name, index );
        _argumentAccessExpressions.Add( result );
        return (result, index);
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void RegisterVariable(StringSlice name)
    {
        Assume.IsNull( _activeNewVariable, nameof( _activeNewVariable ) );
        _activeNewVariable = name;
    }

    internal Chain<ParsedExpressionBuilderError> AddVariableAssignment(
        Expression expression,
        IReadOnlyList<InlineDelegateCollectionState.Result> delegates)
    {
        Assume.IsNotNull( _activeNewVariable, nameof( _activeNewVariable ) );

        var name = _activeNewVariable.Value;
        _activeNewVariable = null;

        var (delegateUsage, variableUsage) = ExpressionUsage.FindDelegateAndVariableUsage( expression, delegates, this );
        var usedDelegates = ExpressionUsage.GetUsedDelegates( delegates, delegateUsage );
        var usedVariables = ExpressionUsage.GetUsedVariables( this, variableUsage );

        return _variables.TryGetValue( name, out var assignment )
            ? AddNextVariableAssignment( name, expression, usedVariables, usedDelegates, assignment )
            : AddFirstVariableAssignment( name, expression, usedVariables, usedDelegates );
    }

    private Chain<ParsedExpressionBuilderError> AddFirstVariableAssignment(
        StringSlice name,
        Expression expression,
        VariableAssignment[] usedVariables,
        InlineDelegateCollectionState.Result[] usedDelegates)
    {
        var variable = Expression.Variable( expression.Type, name.ToString() );
        var assignmentExpression = Expression.Assign( variable, expression );
        var assignment = new VariableAssignment( assignmentExpression, usedVariables, usedDelegates );

        _variables.Add( name, assignment );
        _variableAssignments.Add( assignment );

        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> AddNextVariableAssignment(
        StringSlice name,
        Expression expression,
        VariableAssignment[] usedVariables,
        InlineDelegateCollectionState.Result[] usedDelegates,
        VariableAssignment previous)
    {
        var variable = previous.Variable;
        if ( ReferenceEquals( variable, expression ) )
            return Chain<ParsedExpressionBuilderError>.Empty;

        BinaryExpression assignmentExpression;
        try
        {
            if ( variable.Type != expression.Type && expression.Type.IsAssignableTo( variable.Type ) )
                expression = Expression.Convert( expression, variable.Type );

            assignmentExpression = Expression.Assign( variable, expression );
        }
        catch ( Exception exc )
        {
            return Chain.Create( ParsedExpressionBuilderError.CreateLocalTermHasThrownException( name, exc ) );
        }

        var assignment = new VariableAssignment( assignmentExpression, usedVariables, usedDelegates );

        _variables[name] = assignment;
        _variableAssignments.Add( assignment );

        return Chain<ParsedExpressionBuilderError>.Empty;
    }
}
