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
using System.Linq;
using LfrlAnvil.Dependencies.Internal.Builders;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Dependencies.Internal.Resolvers.Factories;

internal sealed class OpenGenericRangeDependencyResolverFactory : RangeDependencyResolverFactory
{
    internal OpenGenericRangeDependencyResolverFactory(
        ImplementorKey implementorKey,
        Action<Type, IDependencyScope>? onResolvingCallback,
        DependencyResolverFactory[]? factories)
        : base( implementorKey, onResolvingCallback, factories, isOpenGeneric: true )
    {
        Assume.True( implementorKey.Value.Type.ContainsGenericParameters );
    }

    internal override DependencyResolverFactory Close(
        IInternalDependencyKey dependencyKey,
        in DependencyLocatorBuilderExtractionParams @params,
        Dictionary<IDependencyKey, DependencyResolverFactory> dynamicResolverFactories)
    {
        Assume.False( dependencyKey.Type.ContainsGenericParameters );
        Assume.False( ImplementorKey.IsShared || ImplementorKey.RangeIndex is not null );
        Assume.True( Equals( ImplementorKey.Value.Key, dependencyKey.Key ) );

        if ( ! dynamicResolverFactories.TryGetValue( dependencyKey, out var closedFactory ) )
        {
            var elementKey = dependencyKey.WithType( dependencyKey.Type.GetGenericArguments()[0] );
            DependencyResolverFactory[]? factories = null;
            if ( Factories is not null )
            {
                factories = new DependencyResolverFactory[Factories.Length];
                for ( var i = 0; i < factories.Length; ++i )
                {
                    var baseFactory = Factories[i];
                    factories[i] = baseFactory.IsOpenGeneric
                        ? baseFactory.Close( elementKey, in @params, dynamicResolverFactories )
                        : CreateInvalid( ImplementorKey.Create( elementKey, baseFactory.ImplementorKey.RangeIndex ), Lifetime );
                }
            }

            closedFactory = new ClosedRangeDependencyResolverFactory(
                ImplementorKey.Create( dependencyKey ),
                OnResolvingCallback,
                factories );

            dynamicResolverFactories.Add( dependencyKey, closedFactory );
        }

        return closedFactory;
    }

    protected override DependencyResolver CreateResolver(
        UlongSequenceGenerator idGenerator,
        DependencyContainerConfigurationBuilder configuration)
    {
        Assume.Conditional(
            Factories is not null,
            () => Assume.True( Factories!.All( f => ! f.HasState( DependencyResolverFactoryState.Invalid ) && f.IsOpenGeneric ) ) );

        OpenGenericDependencyResolver[]? elementResolvers = null;
        if ( Factories is not null )
        {
            Assume.ContainsAtLeast( Factories, 1 );
            elementResolvers = new OpenGenericDependencyResolver[Factories.Length];
            for ( var i = 0; i < elementResolvers.Length; ++i )
            {
                var factory = Factories[i];
                factory.Build( idGenerator, configuration );
                elementResolvers[i] = ReinterpretCast.To<OpenGenericDependencyResolver>( factory.GetResolver() );
            }
        }

        return new OpenGenericRangeDependencyResolver(
            idGenerator.Generate(),
            ImplementorKey.Value.Type,
            elementResolvers,
            OnResolvingCallback );
    }
}
