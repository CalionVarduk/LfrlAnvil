// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Computable.Expressions.Errors;
using LfrlAnvil.Computable.Expressions.Internal.Delegates;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal sealed class LocalTermsCollection
{
    private readonly List<Expression> _argumentAccessExpressions;
    private readonly IReadOnlyDictionary<StringSegment, ConstantExpression>? _boundArguments;
    private readonly Dictionary<StringSegment, VariableAssignment> _variables;
    private readonly List<VariableAssignment> _variableAssignments;
    private readonly Dictionary<StringSegment, MacroDeclaration> _macros;
    private StringSegment? _activeNewLocalTerm;
    private Dictionary<StringSegment, int>? _macroParameters;

    internal LocalTermsCollection(
        ParameterExpression parameterExpression,
        IReadOnlyDictionary<StringSegment, ConstantExpression>? boundArguments)
    {
        ParameterExpression = parameterExpression;
        _boundArguments = boundArguments;
        ArgumentIndexes = new Dictionary<StringSegment, int>();
        _argumentAccessExpressions = new List<Expression>();
        _variables = new Dictionary<StringSegment, VariableAssignment>();
        _variableAssignments = new List<VariableAssignment>();
        _macros = new Dictionary<StringSegment, MacroDeclaration>();
        _activeNewLocalTerm = null;
        _macroParameters = null;
    }

    internal ParameterExpression ParameterExpression { get; }
    internal Dictionary<StringSegment, int> ArgumentIndexes { get; }
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
    internal bool ContainsArgument(StringSegment name)
    {
        return ArgumentIndexes.ContainsKey( name );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool ContainsVariable(StringSegment name)
    {
        return _variables.ContainsKey( name );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool ContainsMacro(StringSegment name)
    {
        return _macros.ContainsKey( name );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool IsTermStarted(StringSegment name)
    {
        return _activeNewLocalTerm == name;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal Expression GetArgumentAccess(int index)
    {
        return _argumentAccessExpressions[index];
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryGetVariable(StringSegment name, [MaybeNullWhen( false )] out VariableAssignment result)
    {
        return _variables.TryGetValue( name, out result );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryGetMacro(StringSegment name, [MaybeNullWhen( false )] out MacroDeclaration result)
    {
        return _macros.TryGetValue( name, out result );
    }

    internal (Expression Result, int? Index) GetOrAddArgumentAccess(StringSegment name)
    {
        if ( _boundArguments is not null && _boundArguments.TryGetValue( name, out var constant ) )
            return (constant, null);

        ref var index = ref CollectionsMarshal.GetValueRefOrAddDefault( ArgumentIndexes, name, out var exists );
        if ( exists )
            return (GetArgumentAccess( index ), index);

        index = ArgumentIndexes.Count - 1;
        var result = ParameterExpression.CreateArgumentAccess( index );

        _argumentAccessExpressions.Add( result );
        return (result, index);
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void StartVariable(StringSegment name)
    {
        Assume.IsNull( _activeNewLocalTerm );
        _activeNewLocalTerm = name;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal Chain<ParsedExpressionBuilderError> StartMacro(IntermediateToken token)
    {
        Assume.IsNull( _activeNewLocalTerm );

        if ( _macroParameters is not null && _macroParameters.ContainsKey( token.Symbol ) )
            return Chain.Create( ParsedExpressionBuilderError.CreateDuplicatedLocalTermName( token ) );

        _activeNewLocalTerm = token.Symbol;
        var declaration = new MacroDeclaration( _macroParameters );
        _macros.Add( token.Symbol, declaration );
        _macroParameters = null;
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    internal Chain<ParsedExpressionBuilderError> FinalizeVariableAssignment(
        Expression expression,
        IReadOnlyList<InlineDelegateCollectionState.Result> delegates)
    {
        Assume.IsNotNull( _activeNewLocalTerm );

        var name = _activeNewLocalTerm.Value;
        _activeNewLocalTerm = null;

        var (delegateUsage, variableUsage) = ExpressionUsage.FindDelegateAndVariableUsage( expression, delegates, this );
        var usedDelegates = ExpressionUsage.GetUsedDelegates( delegates, delegateUsage );
        var usedVariables = ExpressionUsage.GetUsedVariables( this, variableUsage );

        return _variables.TryGetValue( name, out var assignment )
            ? FinalizeNextVariableAssignment( name, expression, usedVariables, usedDelegates, assignment )
            : FinalizeFirstVariableAssignment( name, expression, usedVariables, usedDelegates );
    }

    internal Chain<ParsedExpressionBuilderError> AddMacroToken(IntermediateToken token)
    {
        Assume.IsNotNull( _activeNewLocalTerm );

        var declaration = _macros[_activeNewLocalTerm.Value];
        declaration.AddToken( token );
        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    internal Chain<ParsedExpressionBuilderError> AddMacroParameter(IntermediateToken token)
    {
        Assume.IsNull( _activeNewLocalTerm );

        _macroParameters ??= new Dictionary<StringSegment, int>();
        var index = _macroParameters.Count;
        return _macroParameters.TryAdd( token.Symbol, index )
            ? Chain<ParsedExpressionBuilderError>.Empty
            : Chain.Create( ParsedExpressionBuilderError.CreateDuplicatedMacroParameterName( token ) );
    }

    internal Chain<ParsedExpressionBuilderError> FinalizeMacroDeclaration()
    {
        Assume.IsNotNull( _activeNewLocalTerm );

        var name = _activeNewLocalTerm.Value;
        _activeNewLocalTerm = null;
        var declaration = _macros[name];

        return declaration.IsEmpty
            ? Chain.Create( ParsedExpressionBuilderError.CreateMacroMustContainAtLeastOneToken( name ) )
            : Chain<ParsedExpressionBuilderError>.Empty;
    }

    private Chain<ParsedExpressionBuilderError> FinalizeFirstVariableAssignment(
        StringSegment name,
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

    private Chain<ParsedExpressionBuilderError> FinalizeNextVariableAssignment(
        StringSegment name,
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
