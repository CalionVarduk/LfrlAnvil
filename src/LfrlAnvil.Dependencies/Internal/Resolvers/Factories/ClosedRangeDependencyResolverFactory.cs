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
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Dependencies.Internal.Resolvers.Factories;

internal sealed class ClosedRangeDependencyResolverFactory : RangeDependencyResolverFactory
{
    internal ClosedRangeDependencyResolverFactory(
        ImplementorKey implementorKey,
        Action<Type, IDependencyScope>? onResolvingCallback,
        DependencyResolverFactory[]? factories)
        : base( implementorKey, onResolvingCallback, factories, isOpenGeneric: false )
    {
        Assume.False( implementorKey.Value.Type.ContainsGenericParameters );
    }

    internal override void RegisterResolver(
        IDependencyKey dependencyKey,
        in DependencyResolversStore globalResolvers,
        in KeyedDependencyResolversStore keyedResolversStore)
    {
        base.RegisterResolver( dependencyKey, in globalResolvers, in keyedResolversStore );
        if ( Factories is not null )
        {
            foreach ( var factory in Factories )
            {
                if ( ! factory.ImplementorKey.IsShared || factory is not RegisteredClosedGenericDependencyResolverFactory genericFactory )
                    continue;

                var implementorStore = ReinterpretCast.To<IInternalDependencyKey>( genericFactory.ImplementorKey.Value )
                    .GetTargetResolversStore( in globalResolvers, in keyedResolversStore );

                var resolver = genericFactory.GetResolver();
                implementorStore.SharedGenericResolvers.TryAdd(
                    new SharedGenericKey( genericFactory.Base.ImplementorKey.Value.Type, genericFactory.ImplementorKey.Value.Type ),
                    resolver );
            }
        }
    }

    protected override DependencyResolver CreateResolver(
        UlongSequenceGenerator idGenerator,
        IDependencyContainerConfigurationBuilder configuration)
    {
        Assume.Conditional(
            Factories is not null,
            () => Assume.True( Factories!.All( f => ! f.HasState( DependencyResolverFactoryState.Invalid ) ) ) );

        var expression = CreateExpression( idGenerator, configuration );

        return OnResolvingCallback is null
            ? new TransientDependencyResolver(
                idGenerator.Generate(),
                ImplementorKey.Value.Type,
                DependencyImplementorDisposalStrategy.RenounceOwnership(),
                expression )
            : new CycleTrackingTransientDependencyResolver(
                idGenerator.Generate(),
                ImplementorKey.Value.Type,
                DependencyImplementorDisposalStrategy.RenounceOwnership(),
                OnResolvingCallback,
                expression );
    }

    [Pure]
    private Expression<Func<DependencyScope, object>> CreateExpression(
        UlongSequenceGenerator idGenerator,
        IDependencyContainerConfigurationBuilder configuration)
    {
        var (expressionBuilder, factoryCount) = CreateExpressionBuilder();
        for ( var i = 0; i < factoryCount; ++i )
        {
            Assume.IsNotNull( Factories );
            expressionBuilder.AddDependencyResolverFactoryResolution( ElementType, $"e{i}", Factories[i], idGenerator, configuration );
        }

        var arrayInit = factoryCount > 0
            ? Expression.NewArrayInit( ElementType, expressionBuilder.GetVariableRange( factoryCount ) )
            : ExpressionBuilder.CreateArrayEmptyCallExpression( ElementType );

        var result = expressionBuilder.Build( arrayInit );
        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private (ExpressionBuilder Builder, int FactoryCount) CreateExpressionBuilder()
    {
        var factoryCount = Factories?.Length ?? 0;
        var hasRequiredValueTypeDependency = ElementType.IsValueType && Nullable.GetUnderlyingType( ElementType ) is null;
        var builder = new ExpressionBuilder( factoryCount, 0, hasRequiredValueTypeDependency, ImplementorKey.Value.Type );
        return (builder, factoryCount);
    }
}
