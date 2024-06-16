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
