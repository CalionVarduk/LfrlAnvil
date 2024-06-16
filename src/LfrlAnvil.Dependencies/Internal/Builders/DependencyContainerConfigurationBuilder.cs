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
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Dependencies.Internal.Builders;

internal sealed class DependencyContainerConfigurationBuilder : IDependencyContainerConfigurationBuilder
{
    internal DependencyContainerConfigurationBuilder()
    {
        InjectablePropertyType = typeof( Injected<> );
        OptionalDependencyAttributeType = typeof( OptionalDependencyAttribute );
        TreatCaptiveDependenciesAsErrors = false;
    }

    public Type InjectablePropertyType { get; private set; }
    public Type OptionalDependencyAttributeType { get; private set; }
    public bool TreatCaptiveDependenciesAsErrors { get; private set; }

    public IDependencyContainerConfigurationBuilder SetInjectablePropertyType(Type openGenericType)
    {
        if ( ! IsInjectablePropertyTypeCorrect( openGenericType ) )
        {
            throw new DependencyContainerBuilderConfigurationException(
                Resources.InvalidInjectablePropertyType( openGenericType ),
                nameof( openGenericType ) );
        }

        InjectablePropertyType = openGenericType;
        return this;
    }

    public IDependencyContainerConfigurationBuilder SetOptionalDependencyAttributeType(Type attributeType)
    {
        if ( ! IsOptionalDependencyAttributeTypeCorrect( attributeType ) )
        {
            throw new DependencyContainerBuilderConfigurationException(
                Resources.InvalidOptionalDependencyAttributeType( attributeType ),
                nameof( attributeType ) );
        }

        OptionalDependencyAttributeType = attributeType;
        return this;
    }

    public IDependencyContainerConfigurationBuilder EnableTreatingCaptiveDependenciesAsErrors(bool enabled = true)
    {
        TreatCaptiveDependenciesAsErrors = enabled;
        return this;
    }

    [Pure]
    private static bool IsInjectablePropertyTypeCorrect(Type type)
    {
        if ( ! type.IsGenericTypeDefinition )
            return false;

        var genericArgs = type.GetGenericArguments();
        if ( genericArgs.Length != 1 )
            return false;

        var instanceType = genericArgs[0];
        var ctor = type.GetConstructors( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )
            .FirstOrDefault(
                c =>
                {
                    var parameters = c.GetParameters();
                    return parameters.Length == 1 && parameters[0].ParameterType == instanceType;
                } );

        return ctor is not null;
    }

    [Pure]
    private static bool IsOptionalDependencyAttributeTypeCorrect(Type type)
    {
        if ( type.IsGenericTypeDefinition )
            return false;

        if ( type.Visit( static t => t.BaseType ).All( static t => t != typeof( Attribute ) ) )
            return false;

        var attributeUsage = type.Visit( static t => t.BaseType )
            .Prepend( type )
            .Select( static t => t.GetAttribute<AttributeUsageAttribute>( inherit: false ) )
            .FirstOrDefault( static a => a is not null );

        const AttributeTargets expectedTargets = AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property;
        return attributeUsage is not null && (attributeUsage.ValidOn & expectedTargets) == expectedTargets;
    }
}
