// Copyright 2024-2026 Łukasz Furlepa
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
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using LfrlAnvil.Dependencies.Internal.Builders;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Dependencies.Internal.Resolvers.Factories;

internal abstract class RegisteredConstructableDependencyResolverFactory : RegisteredDependencyResolverFactory
{
    protected RegisteredConstructableDependencyResolverFactory(ImplementorKey implementorKey, DependencyLifetime lifetime)
        : base( implementorKey, lifetime, isOpenGeneric: false )
    {
        Warnings = Chain<string>.Empty;
    }

    protected abstract Action<object, Type, IDependencyScope>? OnCreatedCallback { get; }

    protected override DependencyResolver CreateResolver(
        UlongSequenceGenerator idGenerator,
        DependencyContainerConfigurationBuilder configuration)
    {
        Assume.IsNotNull( ConstructorInfo );
        var (expressionBuilder, parameterCount, memberCount) = CreateExpressionBuilder();

        for ( var i = 0; i < parameterCount; ++i )
        {
            Assume.IsNotNull( ParameterResolutions );
            var (parameter, resolution) = ParameterResolutions[i];
            var (instanceType, name) = (parameter.ParameterType, $"p{i}");

            if ( resolution is null )
                expressionBuilder.AddDefaultResolution( instanceType, name, parameter.HasDefaultValue, parameter.DefaultValue );
            else if ( resolution is Expression<Func<IDependencyScope, object>> expression )
                expressionBuilder.AddExpressionResolution( instanceType, name, expression );
            else
            {
                var factory = ReinterpretCast.To<DependencyResolverFactory>( resolution );
                expressionBuilder.AddDependencyResolverFactoryResolution( instanceType, name, factory, idGenerator, configuration );
            }
        }

        var memberBindings = memberCount > 0 ? new MemberBinding[memberCount] : null;
        for ( var i = 0; i < memberCount; ++i )
        {
            Assume.IsNotNull( MemberResolutions );
            Assume.IsNotNull( memberBindings );

            var (member, resolution) = MemberResolutions[i];
            var memberType = member.GetInjectableMemberType();
            var (instanceType, name) = (memberType.GetGenericArguments()[0], $"m{i}");

            if ( resolution is null )
                expressionBuilder.AddDefaultResolution( instanceType, name );
            else if ( resolution is Expression<Func<IDependencyScope, object>> expression )
                expressionBuilder.AddExpressionResolution( instanceType, name, expression );
            else
            {
                var factory = ReinterpretCast.To<DependencyResolverFactory>( resolution );
                expressionBuilder.AddDependencyResolverFactoryResolution( instanceType, name, factory, idGenerator, configuration );
            }

            var memberCtor = memberType.FindInjectableMemberCtor( instanceType );
            memberBindings[i] = expressionBuilder.CreateMemberBindingForLastVariable( member, memberCtor );
        }

        var ctorParameters = expressionBuilder.GetVariableRange( parameterCount );
        var ctorCall = parameterCount > 0 ? Expression.New( ConstructorInfo, ctorParameters ) : Expression.New( ConstructorInfo );
        Expression instance = memberBindings is not null ? Expression.MemberInit( ctorCall, memberBindings ) : ctorCall;
        var result = expressionBuilder.Build( instance );
        return CreateFromExpression( result, idGenerator );
    }

    protected abstract DependencyResolver CreateFromExpression(
        Expression<Func<DependencyScope, object>> expression,
        UlongSequenceGenerator idGenerator);

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private (ExpressionBuilder Builder, int ParameterCount, int MemberCount) CreateExpressionBuilder()
    {
        var parameterCount = ParameterResolutions?.Length ?? 0;
        var memberCount = MemberResolutions?.Length ?? 0;
        var defaultResolutionCount = 0;
        var hasRequiredValueTypeDependency = false;

        for ( var i = 0; i < parameterCount; ++i )
        {
            Assume.IsNotNull( ParameterResolutions );
            var (parameter, resolution) = ParameterResolutions[i];

            if ( resolution is null )
            {
                ++defaultResolutionCount;
                continue;
            }

            if ( ! hasRequiredValueTypeDependency
                && parameter.ParameterType.IsValueType
                && Nullable.GetUnderlyingType( parameter.ParameterType ) is null )
                hasRequiredValueTypeDependency = true;
        }

        for ( var i = 0; i < memberCount; ++i )
        {
            Assume.IsNotNull( MemberResolutions );
            var (member, resolution) = MemberResolutions[i];

            if ( resolution is null )
            {
                ++defaultResolutionCount;
                continue;
            }

            if ( hasRequiredValueTypeDependency )
                continue;

            var memberType = member.GetInjectableMemberType().GetGenericArguments()[0];
            if ( memberType.IsValueType && Nullable.GetUnderlyingType( memberType ) is null )
                hasRequiredValueTypeDependency = true;
        }

        var builder = new ExpressionBuilder(
            parameterCount + memberCount,
            defaultResolutionCount,
            hasRequiredValueTypeDependency,
            ImplementorKey.Value.Type,
            OnCreatedCallback );

        return (builder, parameterCount, memberCount);
    }
}
