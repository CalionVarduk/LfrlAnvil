// Copyright 2026 Łukasz Furlepa
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
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using LfrlAnvil.Async;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Dependencies.Internal.Resolvers;

internal sealed class OpenGenericRangeDependencyResolver : DependencyResolver
{
    private readonly OpenGenericDependencyResolver[]? _elementResolvers;
    private readonly Action<Type, IDependencyScope>? _onResolvingCallback;

    internal OpenGenericRangeDependencyResolver(
        ulong id,
        Type implementorType,
        OpenGenericDependencyResolver[]? elementResolvers,
        Action<Type, IDependencyScope>? onResolvingCallback)
        : base( id, implementorType, DependencyImplementorDisposalStrategy.RenounceOwnership() )
    {
        _elementResolvers = elementResolvers;
        _onResolvingCallback = onResolvingCallback;
    }

    internal override DependencyLifetime Lifetime => DependencyLifetime.Transient;

    internal DependencyResolver Close(DependencyLocator dependencyLocator, Type dependencyType)
    {
        var result = CreateClosedResolver( dependencyLocator, dependencyType );
        using ( AcquireActiveWriteLock( dependencyLocator ) )
            result = dependencyLocator.Resolvers.GetOrAddResolver( dependencyType, result );

        return result;
    }

    internal override object Create(DependencyScope scope, Type dependencyType)
    {
        throw new OpenGenericDependencyException( dependencyType, Resources.OpenGenericCannotBeResolved( dependencyType ) );
    }

    [Pure]
    private DependencyResolver CreateClosedResolver(DependencyLocator dependencyLocator, Type dependencyType)
    {
        Expression<Func<DependencyScope, object>> expression;
        using ( DependencyCycleTracker.Create( this, dependencyType ) )
        {
            try
            {
                expression = CreateExpression( dependencyLocator, dependencyType );
            }
            catch ( CircularDependencyReferenceException exc )
            {
                ExceptionThrower.Throw( new CircularDependencyReferenceException( dependencyType, dependencyType, exc ) );
                return default!;
            }
        }

        var id = dependencyLocator.InternalAttachedScope.InternalContainer.GenerateResolverId();
        return _onResolvingCallback is null
            ? new TransientDependencyResolver(
                id,
                ImplementorType,
                DependencyImplementorDisposalStrategy.RenounceOwnership(),
                expression )
            : new CycleTrackingTransientDependencyResolver(
                id,
                ImplementorType,
                DependencyImplementorDisposalStrategy.RenounceOwnership(),
                _onResolvingCallback,
                expression );
    }

    [Pure]
    private Expression<Func<DependencyScope, object>> CreateExpression(DependencyLocator dependencyLocator, Type dependencyType)
    {
        Assume.True( dependencyType.IsGenericType && dependencyType.GetGenericTypeDefinition() == typeof( IEnumerable<> ) );
        var elementType = dependencyType.GetGenericArguments()[0];

        var (expressionBuilder, elementCount) = CreateExpressionBuilder( dependencyType, elementType );
        for ( var i = 0; i < elementCount; ++i )
        {
            Assume.IsNotNull( _elementResolvers );
            var resolver = _elementResolvers[i].Close( dependencyLocator, elementType );
            expressionBuilder.AddDependencyResolverResolution( elementType, $"e{i}", resolver );
        }

        var arrayInit = elementCount > 0
            ? Expression.NewArrayInit( elementType, expressionBuilder.GetVariableRange( elementCount ) )
            : ExpressionBuilder.CreateArrayEmptyCallExpression( elementType );

        var result = expressionBuilder.Build( arrayInit );
        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private (ExpressionBuilder Builder, int ElementCount) CreateExpressionBuilder(Type dependencyType, Type elementType)
    {
        var elementCount = _elementResolvers?.Length ?? 0;
        var hasRequiredValueTypeDependency = elementType.IsValueType && Nullable.GetUnderlyingType( elementType ) is null;
        var builder = new ExpressionBuilder( elementCount, 0, hasRequiredValueTypeDependency, dependencyType );
        return (builder, elementCount);
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static WriteLockSlim AcquireActiveWriteLock(DependencyLocator locator)
    {
        var @lock = WriteLockSlim.TryEnter( locator.Resolvers.Lock, out var entered );
        if ( ! entered )
            ExceptionThrower.Throw(
                new ObjectDisposedException(
                    null,
                    Resources.ScopeIsDisposed( locator.InternalAttachedScope.InternalContainer.InternalRootScope ) ) );

        return @lock;
    }
}
