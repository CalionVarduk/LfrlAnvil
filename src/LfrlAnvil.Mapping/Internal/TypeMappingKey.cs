using System;

namespace LfrlAnvil.Mapping.Internal
{
    public readonly struct TypeMappingKey : IEquatable<TypeMappingKey>
    {
        public TypeMappingKey(Type sourceType, Type destinationType)
        {
            SourceType = sourceType;
            DestinationType = destinationType;
        }

        public Type? SourceType { get; }
        public Type? DestinationType { get; }

        public override string ToString()
        {
            return $"{nameof( TypeMappingKey )}({SourceType?.FullName} => {DestinationType?.FullName})";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine( SourceType, DestinationType );
        }

        public override bool Equals(object obj)
        {
            return obj is TypeMappingKey k && Equals( k );
        }

        public bool Equals(TypeMappingKey other)
        {
            return SourceType == other.SourceType && DestinationType == other.DestinationType;
        }

        public static bool operator ==(TypeMappingKey a, TypeMappingKey b)
        {
            return a.Equals( b );
        }

        public static bool operator !=(TypeMappingKey a, TypeMappingKey b)
        {
            return ! a.Equals( b );
        }
    }
}
