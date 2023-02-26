using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Internal.Delegates;

internal sealed class InlineDelegateCollectionState
{
    private readonly LocalTermsCollection _localTerms;
    private readonly Dictionary<int, ClosureInfo> _capturedParametersByState;
    private readonly Dictionary<StringSlice, OwnedParameterExpression> _parametersMap;
    private readonly RandomAccessStack<ParameterExpression> _parameters;
    private readonly RandomAccessStack<StateRegistration> _registeredStates;
    private readonly List<FinalizedDelegate> _nestedStateFinalization;
    private int? _parentStateIdOfLastFinalizedState;
    private bool _isLastStateActive;

    internal InlineDelegateCollectionState(ExpressionBuilderState state)
    {
        _isLastStateActive = true;
        _parentStateIdOfLastFinalizedState = null;
        _localTerms = state.LocalTerms;
        _capturedParametersByState = new Dictionary<int, ClosureInfo>();
        _parametersMap = new Dictionary<StringSlice, OwnedParameterExpression>();
        _parameters = new RandomAccessStack<ParameterExpression>();
        _registeredStates = new RandomAccessStack<StateRegistration>();
        _registeredStates.Push( new StateRegistration( state.Id ) );
        _nestedStateFinalization = new List<FinalizedDelegate>();
    }

    internal bool AreAllStatesFinalized => _registeredStates.Count == 0;

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool AreParametersLocked(ExpressionBuilderState state)
    {
        return ! _isLastStateActive || _registeredStates.Peek().Id != state.Id;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void LockParameters()
    {
        Assume.Equals( _isLastStateActive, true, nameof( _isLastStateActive ) );
        _isLastStateActive = false;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Register(ExpressionBuilderState state)
    {
        Assume.IsNotEmpty( _registeredStates, nameof( _registeredStates ) );
        Assume.Equals( _isLastStateActive, false, nameof( _isLastStateActive ) );

        _registeredStates.Push( new StateRegistration( state.Id ) );
        _isLastStateActive = true;
    }

    internal bool TryAddParameter(Type type, StringSlice name)
    {
        Assume.IsNotEmpty( _registeredStates, nameof( _registeredStates ) );
        Assume.Equals( _isLastStateActive, true, nameof( _isLastStateActive ) );

        ref var parameterExpression = ref CollectionsMarshal.GetValueRefOrAddDefault( _parametersMap, name, out var exists )!;
        if ( exists )
            return false;

        var state = _registeredStates.Peek();
        _registeredStates.Replace( state.IncrementParameterCount() );

        var parameter = Expression.Parameter( type, name.ToString() );
        parameterExpression = new OwnedParameterExpression( parameter, state.Id );
        _parameters.Push( parameter );
        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ParameterExpression? TryGetParameter(ExpressionBuilderState state, StringSlice name)
    {
        Assume.Equals( _isLastStateActive, false, nameof( _isLastStateActive ) );

        if ( ! _parametersMap.TryGetValue( name, out var parameter ) )
            return null;

        if ( state.Id == parameter.OwnerStateId )
            return parameter.Expression;

        AddParameterCapture( state.Id, parameter.Expression, parameter.OwnerStateId );
        return parameter.Expression;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void AddArgumentCapture(ExpressionBuilderState state, int index)
    {
        Assume.Equals( _isLastStateActive, false, nameof( _isLastStateActive ) );
        AddParameterCapture( state.Id, _localTerms.ParameterExpression, 0, index );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void AddVariableCapture(ExpressionBuilderState state, ParameterExpression variable)
    {
        Assume.Equals( _isLastStateActive, false, nameof( _isLastStateActive ) );
        AddParameterCapture( state.Id, variable, 0 );
    }

    internal Expression FinalizeLastState(Expression lambdaBody, bool compileWhenStatic)
    {
        Assume.IsNotEmpty( _registeredStates, nameof( _registeredStates ) );
        Assume.Equals( _isLastStateActive, false, nameof( _isLastStateActive ) );

        var state = _registeredStates.Pop();
        var parentStateId = GetParentStateId();
        var result = FinalizeDelegate( state, parentStateId, lambdaBody, compileWhenStatic );

        if ( parentStateId != _parentStateIdOfLastFinalizedState )
        {
            _parentStateIdOfLastFinalizedState = parentStateId;
            result.SetNestedFinalization( _nestedStateFinalization );
            _nestedStateFinalization.Clear();
        }

        _nestedStateFinalization.Add( result );
        return result.IsCompiled ? result.CompiledLambda : result.LambdaPlaceholder;
    }

    internal Result? CreateCompilableDelegates()
    {
        Assume.Equals( AreAllStatesFinalized, true, nameof( AreAllStatesFinalized ) );
        Assume.ContainsExactly( _nestedStateFinalization, 1, nameof( _nestedStateFinalization ) );

        _capturedParametersByState.Clear();
        var rootFinalization = _nestedStateFinalization[0];
        _nestedStateFinalization.Clear();

        if ( rootFinalization.IsCompiled )
            return null;

        var usedArgumentIndexes = new HashSet<int>();
        var usedVariables = new HashSet<StringSlice>();
        var result = CreateCompilableInlineDelegate( usedArgumentIndexes, usedVariables, rootFinalization, parent: null );
        return new Result( result, usedArgumentIndexes, usedVariables );
    }

    private static CompilableInlineDelegate CreateCompilableInlineDelegate(
        HashSet<int> usedArgumentIndexes,
        HashSet<StringSlice> usedVariables,
        FinalizedDelegate finalization,
        (FinalizedDelegate Finalization, ClosureExpressionFactory? Closure)? parent)
    {
        Assume.IsNotNull( finalization.LambdaPlaceholder, nameof( finalization.LambdaPlaceholder ) );
        Assume.Equals( finalization.IsUsed, true, nameof( finalization.IsUsed ) );

        ClosureExpressionFactory? closure = null;
        NewExpression? closureCtorCall = null;
        MethodInfo? bindClosureMethod = null;

        if ( ! finalization.IsStatic )
        {
            usedArgumentIndexes.UnionWith( finalization.CapturedParameters.Value.ArgumentIndexes );
            usedVariables.UnionWith( finalization.CapturedParameters.Value.Variables );

            closure = ClosureHelpers.CreateClosureTypeFactory(
                finalization.CapturedParameters.Value,
                parent?.Finalization.CapturedParameters );

            finalization.Parameters[0] = closure.Value.Parameter;
            bindClosureMethod = ClosureHelpers.GetBindClosureMethod( finalization.Body, finalization.Parameters );

            closureCtorCall = parent?.Closure is null
                ? closure.Value.CreateCtorCallForRootDelegate()
                : closure.Value.CreateCtorCallForNestedDelegate( parent.Value.Closure.Value );
        }

        var activeNestedCount = 0;
        var nestedFinalization = finalization.NestedFinalization;

        foreach ( var nested in nestedFinalization )
        {
            if ( ! nested.IsCompiled && nested.IsUsed )
                ++activeNestedCount;
        }

        var index = 0;
        var nestedDelegates = activeNestedCount == 0
            ? Array.Empty<CompilableInlineDelegate>()
            : new CompilableInlineDelegate[activeNestedCount];

        if ( activeNestedCount > 0 )
        {
            foreach ( var nested in nestedFinalization )
            {
                if ( nested.IsCompiled || ! nested.IsUsed )
                    continue;

                nestedDelegates[index++] = CreateCompilableInlineDelegate(
                    usedArgumentIndexes,
                    usedVariables,
                    nested,
                    (finalization, closure) );
            }
        }

        var result = new CompilableInlineDelegate(
            finalization.Body,
            finalization.Parameters,
            finalization.LambdaPlaceholder,
            closureCtorCall,
            bindClosureMethod,
            capturedParameterReplacements: closure?.CapturedParameters ?? Array.Empty<ExpressionReplacement>(),
            nestedDelegates );

        return result;
    }

    private FinalizedDelegate FinalizeDelegate(StateRegistration state, int? parentStateId, Expression body, bool compileWhenStatic)
    {
        var nestedFinalization = parentStateId == _parentStateIdOfLastFinalizedState
            ? (IReadOnlyList<FinalizedDelegate>)Array.Empty<FinalizedDelegate>()
            : _nestedStateFinalization;

        if ( ! _capturedParametersByState.TryGetValue( state.Id, out var capturedParameters ) )
        {
            if ( nestedFinalization.All( static f => f.IsStatic ) )
                return FinalizeStaticDelegate( state, body, compileWhenStatic );

            capturedParameters = new ClosureInfo( state.Id );
        }

        var capturedParameterUsageValidator = new CapturedParameterUsageValidator( _localTerms, capturedParameters, nestedFinalization );
        capturedParameterUsageValidator.Visit( body );
        capturedParameterUsageValidator.ApplyChanges( _capturedParametersByState );

        if ( capturedParameters.IsEmpty )
            return FinalizeStaticDelegate( state, body, compileWhenStatic && nestedFinalization.All( static f => f.IsCompiled ) );

        _capturedParametersByState.TryAdd( state.Id, capturedParameters );
        return FinalizeDelegateWithClosure( state, body, capturedParameters );
    }

    private FinalizedDelegate FinalizeStaticDelegate(StateRegistration state, Expression body, bool compile)
    {
        var parameters = state.ParameterCount == 0 ? Array.Empty<ParameterExpression>() : new ParameterExpression[state.ParameterCount];
        PopulateFinalizedParametersCollection( parameters, state.ParameterCount, bufferStartIndex: 0 );

        var result = FinalizedDelegate.CreateFromStaticDelegate( state.Id, body, parameters, compile );
        return result;
    }

    private FinalizedDelegate FinalizeDelegateWithClosure(StateRegistration state, Expression body, ClosureInfo capturedParameters)
    {
        var parameters = new ParameterExpression[state.ParameterCount + 1];
        PopulateFinalizedParametersCollection( parameters, state.ParameterCount, bufferStartIndex: 1 );

        var result = FinalizedDelegate.CreateFromDelegateWithClosure( body, parameters, capturedParameters );
        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private int? GetParentStateId()
    {
        return _registeredStates.Count == 0 ? null : _registeredStates.Peek().Id;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void PopulateFinalizedParametersCollection(ParameterExpression[] buffer, int count, int bufferStartIndex)
    {
        _parameters.PopInto( count, buffer, bufferStartIndex );

        for ( var i = bufferStartIndex; i < buffer.Length; ++i )
            _parametersMap.Remove( new StringSlice( buffer[i].Name! ) );
    }

    private void AddParameterCapture(int stateId, ParameterExpression expression, int ownerStateId, int argumentIndex = -1)
    {
        ref var captures = ref CollectionsMarshal.GetValueRefOrAddDefault( _capturedParametersByState, stateId, out var exists );
        if ( ! exists )
            captures = new ClosureInfo( stateId );

        if ( argumentIndex >= 0 )
            captures.ArgumentIndexes.Add( argumentIndex );

        captures.FindIndexOrAdd( expression, ownerStateId );
    }

    private sealed class CapturedParameterUsageValidator : ExpressionVisitor
    {
        private readonly LocalTermsCollection _localTerms;
        private readonly ClosureInfo _capturedParameters;
        private readonly IReadOnlyList<FinalizedDelegate> _nestedFinalization;
        private readonly int _usableArgumentCount;
        private readonly HashSet<int> _usedArgumentIndexes;
        private readonly HashSet<StringSlice> _usedVariables;
        private readonly List<OwnedParameterExpression> _usedParameters;
        private readonly List<FinalizedDelegate> _usedNestedFinalization;
        private bool _usesRootParameter;

        internal CapturedParameterUsageValidator(
            LocalTermsCollection localTerms,
            ClosureInfo capturedParameters,
            IReadOnlyList<FinalizedDelegate> nestedFinalization)
        {
            _usedArgumentIndexes = new HashSet<int>();
            _usedVariables = new HashSet<StringSlice>();
            _usedParameters = new List<OwnedParameterExpression>();
            _usedNestedFinalization = new List<FinalizedDelegate>();
            _capturedParameters = capturedParameters;
            _localTerms = localTerms;
            _nestedFinalization = nestedFinalization;
            _usableArgumentCount = capturedParameters.ArgumentIndexes.TryMax( out var maxIndex ) ? maxIndex + 1 : 0;
            _usesRootParameter = false;
        }

        [return: NotNullIfNotNull( "node" )]
        public override Expression? Visit(Expression? node)
        {
            if ( _usableArgumentCount > 0 &&
                node.TryGetArgumentAccessIndex( _localTerms.ParameterExpression, _usableArgumentCount, out var index ) )
            {
                if ( ! _usesRootParameter )
                {
                    _usesRootParameter = true;
                    _usedParameters.Add( new OwnedParameterExpression( _localTerms.ParameterExpression, 0 ) );
                }

                _usedArgumentIndexes.Add( index );
            }
            else if ( node is ParameterExpression parameter )
            {
                var ownerStateId = _capturedParameters.FindOwnerStateId( parameter );
                if ( ownerStateId > 0 || (ownerStateId == 0 && ! ReferenceEquals( parameter, _localTerms.ParameterExpression )) )
                {
                    _usedParameters.Add( new OwnedParameterExpression( parameter, ownerStateId ) );
                    _usedVariables.Add( parameter.Name!.AsSlice() );
                }
            }
            else
            {
                var finalizationIndex = -1;
                for ( var i = 0; i < _nestedFinalization.Count; ++i )
                {
                    var finalization = _nestedFinalization[i];
                    if ( ReferenceEquals( finalization.CompiledLambda ?? finalization.LambdaPlaceholder, node ) )
                    {
                        finalizationIndex = i;
                        break;
                    }
                }

                if ( finalizationIndex >= 0 )
                    _usedNestedFinalization.Add( _nestedFinalization[finalizationIndex] );
            }

            return base.Visit( node );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void ApplyChanges(Dictionary<int, ClosureInfo> capturedParametersByState)
        {
            _capturedParameters.Parameters.Clear();
            _capturedParameters.Parameters.AddRange( _usedParameters );
            _capturedParameters.ArgumentIndexes.Clear();
            _capturedParameters.ArgumentIndexes.UnionWith( _usedArgumentIndexes );
            _capturedParameters.Variables.Clear();
            _capturedParameters.Variables.UnionWith( _usedVariables );

            foreach ( var finalization in _nestedFinalization )
            {
                if ( ! _usedNestedFinalization.Contains( finalization ) )
                {
                    finalization.MarkAsUnused();
                    continue;
                }

                if ( ! capturedParametersByState.Remove( finalization.StateId, out var nestedCapturedParameters ) )
                    continue;

                foreach ( var parameter in nestedCapturedParameters.Parameters )
                {
                    if ( parameter.OwnerStateId == _capturedParameters.StateId )
                        continue;

                    _capturedParameters.FindIndexOrAdd( parameter.Expression, parameter.OwnerStateId );
                }
            }
        }
    }

    internal readonly struct Result
    {
        internal readonly CompilableInlineDelegate Delegate;
        internal readonly IReadOnlySet<int> UsedArgumentIndexes;
        internal readonly IReadOnlySet<StringSlice> UsedVariables;

        internal Result(CompilableInlineDelegate @delegate, IReadOnlySet<int> usedArgumentIndexes, IReadOnlySet<StringSlice> usedVariables)
        {
            Delegate = @delegate;
            UsedArgumentIndexes = usedArgumentIndexes;
            UsedVariables = usedVariables;
        }
    }

    private sealed class FinalizedDelegate
    {
        private FinalizedDelegate[]? _nestedFinalization;

        private FinalizedDelegate(
            int stateId,
            Expression body,
            ParameterExpression[] parameters,
            ClosureInfo? capturedParameters,
            Expression? compiledLambda,
            Expression? lambdaPlaceholder)
        {
            StateId = stateId;
            Body = body;
            Parameters = parameters;
            CapturedParameters = capturedParameters;
            CompiledLambda = compiledLambda;
            LambdaPlaceholder = lambdaPlaceholder;
            IsUsed = true;
            _nestedFinalization = null;
        }

        internal int StateId { get; }
        internal Expression Body { get; }
        internal ParameterExpression[] Parameters { get; }
        internal ClosureInfo? CapturedParameters { get; }
        internal Expression? CompiledLambda { get; }
        internal Expression? LambdaPlaceholder { get; }
        internal bool IsUsed { get; private set; }
        internal IReadOnlyList<FinalizedDelegate> NestedFinalization => _nestedFinalization ?? Array.Empty<FinalizedDelegate>();

        [MemberNotNullWhen( false, nameof( CapturedParameters ) )]
        internal bool IsStatic => CapturedParameters is null;

        [MemberNotNullWhen( true, nameof( CompiledLambda ) )]
        [MemberNotNullWhen( false, nameof( LambdaPlaceholder ) )]
        internal bool IsCompiled => CompiledLambda is not null;

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static FinalizedDelegate CreateFromStaticDelegate(
            int stateId,
            Expression body,
            ParameterExpression[] parameters,
            bool compile)
        {
            Expression? compiledLambda;
            Expression? lambdaPlaceholder;
            var lambda = Expression.Lambda( body, parameters );

            if ( compile )
            {
                compiledLambda = Expression.Constant( lambda.Compile() );
                lambdaPlaceholder = null;
            }
            else
            {
                compiledLambda = null;
                lambdaPlaceholder = ExpressionHelpers.CreateLambdaPlaceholder( lambda.Type );
            }

            return new FinalizedDelegate(
                stateId,
                body,
                parameters,
                capturedParameters: null,
                compiledLambda,
                lambdaPlaceholder );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static FinalizedDelegate CreateFromDelegateWithClosure(
            Expression body,
            ParameterExpression[] parameters,
            ClosureInfo capturedParameters)
        {
            var lambdaType = ClosureHelpers.GetDelegateWithCaptureType( body, parameters );
            var lambdaPlaceholder = ExpressionHelpers.CreateLambdaPlaceholder( lambdaType );

            return new FinalizedDelegate(
                capturedParameters.StateId,
                body,
                parameters,
                capturedParameters,
                compiledLambda: null,
                lambdaPlaceholder );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void SetNestedFinalization(IReadOnlyList<FinalizedDelegate> states)
        {
            Assume.IsNull( _nestedFinalization, nameof( _nestedFinalization ) );

            _nestedFinalization = states.Count == 0
                ? Array.Empty<FinalizedDelegate>()
                : new FinalizedDelegate[states.Count];

            for ( var i = 0; i < _nestedFinalization.Length; ++i )
                _nestedFinalization[i] = states[i];
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void MarkAsUnused()
        {
            if ( ! IsUsed )
                return;

            IsUsed = false;
            foreach ( var state in NestedFinalization )
                state.MarkAsUnused();
        }
    }

    internal readonly struct ClosureInfo
    {
        internal readonly int StateId;
        internal readonly List<OwnedParameterExpression> Parameters;
        internal readonly HashSet<int> ArgumentIndexes;
        internal readonly HashSet<StringSlice> Variables;

        internal ClosureInfo(int stateId)
        {
            StateId = stateId;
            Parameters = new List<OwnedParameterExpression>();
            ArgumentIndexes = new HashSet<int>();
            Variables = new HashSet<StringSlice>();
        }

        internal bool IsEmpty => Parameters.Count == 0 && ArgumentIndexes.Count == 0 && Variables.Count == 0;

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal int FindOwnerStateId(ParameterExpression parameter)
        {
            var index = Parameters.FindIndex( x => ReferenceEquals( x.Expression, parameter ) );
            if ( index < 0 )
                return -1;

            return Parameters[index].OwnerStateId;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal int FindIndexOrAdd(ParameterExpression parameter, int ownerStateId)
        {
            var index = FindIndex( parameter );
            if ( index >= 0 )
                return index;

            index = Parameters.Count;
            Parameters.Add( new OwnedParameterExpression( parameter, ownerStateId ) );
            return index;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal int FindIndex(ParameterExpression parameter)
        {
            return Parameters.FindIndex( x => ReferenceEquals( x.Expression, parameter ) );
        }
    }

    internal readonly struct OwnedParameterExpression
    {
        internal readonly ParameterExpression Expression;
        internal readonly int OwnerStateId;

        internal OwnedParameterExpression(ParameterExpression expression, int ownerStateId)
        {
            Expression = expression;
            OwnerStateId = ownerStateId;
        }

        [Pure]
        public override string ToString()
        {
            return $"{nameof( OwnerStateId )}: {OwnerStateId}, {nameof( Expression )}: {Expression}";
        }
    }

    private readonly struct StateRegistration
    {
        internal readonly int Id;
        internal readonly int ParameterCount;

        public StateRegistration(int id)
            : this( id, parameterCount: 0 ) { }

        private StateRegistration(int id, int parameterCount)
        {
            Id = id;
            ParameterCount = parameterCount;
        }

        [Pure]
        public override string ToString()
        {
            return $"{nameof( Id )}: {Id}, {nameof( ParameterCount )}: {ParameterCount}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal StateRegistration IncrementParameterCount()
        {
            return new StateRegistration( Id, ParameterCount + 1 );
        }
    }
}
