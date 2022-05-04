using System;

namespace LfrlAnvil.Mapping.Internal
{
    public readonly struct MappingKey : IEquatable<MappingKey>
    {
        public MappingKey(Type sourceType, Type destinationType)
        {
            SourceType = sourceType;
            DestinationType = destinationType;
        }

        public Type? SourceType { get; }
        public Type? DestinationType { get; }

        public override string ToString()
        {
            return $"{nameof( MappingKey )}({SourceType?.FullName} => {DestinationType?.FullName})";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine( SourceType, DestinationType );
        }

        public override bool Equals(object obj)
        {
            return obj is MappingKey k && Equals( k );
        }

        public bool Equals(MappingKey other)
        {
            return SourceType == other.SourceType && DestinationType == other.DestinationType;
        }

        public static bool operator ==(MappingKey a, MappingKey b)
        {
            return a.Equals( b );
        }

        public static bool operator !=(MappingKey a, MappingKey b)
        {
            return ! a.Equals( b );
        }
    }
}
