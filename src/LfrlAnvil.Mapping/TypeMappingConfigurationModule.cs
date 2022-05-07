using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Extensions;
using LfrlAnvil.Mapping.Internal;

namespace LfrlAnvil.Mapping
{
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
                Ensure.NotRefEquals( module, this, nameof( module ) );
                Ensure.IsNull( module.Parent, nameof( module ) + "." + nameof( module.Parent ) );
                EnsureLackOfCyclicModuleReference( module );
                module.Parent = this;
            }

            _configurations.Add( configuration );
            return this;
        }

        [Pure]
        public IEnumerable<KeyValuePair<TypeMappingKey, TypeMappingStore>> GetMappingStores()
        {
            return _configurations.SelectMany( c => c.GetMappingStores() );
        }

        [Pure]
        public IEnumerable<TypeMappingConfigurationModule> GetSubmodules()
        {
            return _configurations.OfType<TypeMappingConfigurationModule>();
        }

        private void EnsureLackOfCyclicModuleReference(TypeMappingConfigurationModule moduleToAdd)
        {
            var submoduleTree = moduleToAdd.VisitMany( m => m.GetSubmodules() );
            if ( submoduleTree.Any( m => m == this ) )
                throw CyclicModuleReferenceException();
        }

        [Pure]
        private static ArgumentException CyclicModuleReferenceException()
        {
            return new ArgumentException( "Failed to configure mapping submodule due to a cyclic reference." );
        }
    }
}
