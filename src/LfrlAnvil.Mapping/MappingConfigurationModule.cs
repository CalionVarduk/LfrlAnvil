using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Extensions;
using LfrlAnvil.Mapping.Internal;

namespace LfrlAnvil.Mapping
{
    public class MappingConfigurationModule : IMappingConfiguration
    {
        private readonly List<IMappingConfiguration> _configurations;

        public MappingConfigurationModule()
        {
            _configurations = new List<IMappingConfiguration>();
            Parent = null;
        }

        public MappingConfigurationModule? Parent { get; private set; }

        public MappingConfigurationModule Configure(IMappingConfiguration configuration)
        {
            if ( configuration is MappingConfigurationModule module )
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
        public IEnumerable<KeyValuePair<MappingKey, MappingStore>> GetMappingStores()
        {
            return _configurations.SelectMany( c => c.GetMappingStores() );
        }

        [Pure]
        public IEnumerable<MappingConfigurationModule> GetSubmodules()
        {
            return _configurations.OfType<MappingConfigurationModule>();
        }

        private void EnsureLackOfCyclicModuleReference(MappingConfigurationModule moduleToAdd)
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
