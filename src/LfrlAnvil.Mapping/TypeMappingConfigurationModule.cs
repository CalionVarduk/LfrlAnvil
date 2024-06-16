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

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Mapping.Exceptions;
using LfrlAnvil.Mapping.Internal;

namespace LfrlAnvil.Mapping;

/// <summary>
/// Represents a collection of <see cref="ITypeMappingConfiguration"/> instances.
/// </summary>
public class TypeMappingConfigurationModule : ITypeMappingConfiguration
{
    private readonly List<ITypeMappingConfiguration> _configurations;

    /// <summary>
    /// Creates a new <see cref="TypeMappingConfigurationModule"/> instance without any sub-modules.
    /// </summary>
    public TypeMappingConfigurationModule()
    {
        _configurations = new List<ITypeMappingConfiguration>();
        Parent = null;
    }

    /// <summary>
    /// Parent of this module.
    /// </summary>
    public TypeMappingConfigurationModule? Parent { get; private set; }

    /// <summary>
    /// Adds <paramref name="configuration"/> to this module.
    /// </summary>
    /// <param name="configuration">Configuration instance to add.</param>
    /// <returns><b>this</b>.</returns>
    /// <exception cref="InvalidTypeMappingSubmoduleConfigurationException">
    /// When <paramref name="configuration"/> is of <see cref="TypeMappingConfigurationModule"/> type and it has already been assigned
    /// to another module or a cyclic reference between modules has been detected.
    /// </exception>
    public TypeMappingConfigurationModule Configure(ITypeMappingConfiguration configuration)
    {
        if ( configuration is TypeMappingConfigurationModule module )
        {
            if ( ReferenceEquals( module, this ) )
                throw ReferenceToSelfException( nameof( configuration ) );

            if ( module.Parent is not null )
                throw SubmoduleAlreadyOwnedException( nameof( configuration ) );

            if ( CyclicReferenceDetected( module ) )
                throw CyclicReferenceException( nameof( configuration ) );

            module.Parent = this;
        }

        _configurations.Add( configuration );
        return this;
    }

    /// <inheritdoc />
    [Pure]
    public IEnumerable<KeyValuePair<TypeMappingKey, TypeMappingStore>> GetMappingStores()
    {
        return _configurations.SelectMany( static c => c.GetMappingStores() );
    }

    /// <summary>
    /// Returns all sub-modules of this module.
    /// </summary>
    /// <returns>All <see cref="TypeMappingConfigurationModule"/> instances whose <see cref="Parent"/> is this module.</returns>
    [Pure]
    public IEnumerable<TypeMappingConfigurationModule> GetSubmodules()
    {
        return _configurations.OfType<TypeMappingConfigurationModule>();
    }

    [Pure]
    private bool CyclicReferenceDetected(TypeMappingConfigurationModule module)
    {
        var stack = new Stack<TypeMappingConfigurationModule>( module.GetSubmodules() );
        while ( stack.TryPop( out var submodule ) )
        {
            if ( ReferenceEquals( submodule, this ) )
                return true;

            foreach ( var s in submodule.GetSubmodules() )
                stack.Push( s );
        }

        return false;
    }

    [Pure]
    private static InvalidTypeMappingSubmoduleConfigurationException ReferenceToSelfException(string paramName)
    {
        return new InvalidTypeMappingSubmoduleConfigurationException(
            Resources.InvalidTypeMappingSubmoduleConfigurationReferenceToSelf,
            paramName );
    }

    [Pure]
    private static InvalidTypeMappingSubmoduleConfigurationException SubmoduleAlreadyOwnedException(string paramName)
    {
        return new InvalidTypeMappingSubmoduleConfigurationException(
            Resources.InvalidTypeMappingSubmoduleConfigurationSubmoduleAlreadyOwned,
            paramName );
    }

    [Pure]
    private static InvalidTypeMappingSubmoduleConfigurationException CyclicReferenceException(string paramName)
    {
        return new InvalidTypeMappingSubmoduleConfigurationException(
            Resources.InvalidTypeMappingSubmoduleConfigurationCyclicReference,
            paramName );
    }
}
