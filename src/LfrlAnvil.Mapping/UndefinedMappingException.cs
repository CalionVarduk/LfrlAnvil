using System;

namespace LfrlAnvil.Mapping
{
    public class UndefinedMappingException : Exception
    {
        public UndefinedMappingException(Type sourceType, Type destinationType)
            : base( GetMessage( sourceType, destinationType ) )
        {
            SourceType = sourceType;
            DestinationType = destinationType;
        }

        public Type SourceType { get; }
        public Type DestinationType { get; }

        private static string GetMessage(Type sourceType, Type destinationType)
        {
            var sourceText = sourceType.FullName;
            var destinationText = destinationType.FullName;
            return $"Mapping from {sourceText} to {destinationText} is undefined.";
        }
    }
}
