using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Mapping.Exceptions;

internal static class Resources
{
    internal const string InvalidTypeMappingSubmoduleConfigurationReferenceToSelf =
        "Failed to configure type mapping submodule due to self reference.";

    internal const string InvalidTypeMappingSubmoduleConfigurationSubmoduleAlreadyOwned =
        "Failed to configure type mapping submodule because it already has a parent module.";

    internal const string InvalidTypeMappingSubmoduleConfigurationCyclicReference =
        "Failed to configure type mapping submodule due to a cyclic reference.";

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string UndefinedTypeMapping(Type sourceType, Type destinationType)
    {
        var sourceText = sourceType.FullName;
        var destinationText = destinationType.FullName;
        return $"Type mapping from {sourceText} to {destinationText} is undefined.";
    }
}
