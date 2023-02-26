using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Mapping.Exceptions;
using LfrlAnvil.Mapping.Internal;

namespace LfrlAnvil.Mapping;

public class TypeMappingConfigurationModule : ITypeMappingConfiguration
{
    private readonly List<ITypeMappingConfiguration> _configurations;

    public TypeMappingConfigurationModule()
    {
        _configurations = new List<ITypeMappingConfiguration>();
        Parent = null;
    }

    public TypeMappingConfigurationModule? Parent { get; private set; }

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

    [Pure]
    public IEnumerable<KeyValuePair<TypeMappingKey, TypeMappingStore>> GetMappingStores()
    {
        return _configurations.SelectMany( static c => c.GetMappingStores() );
    }

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
