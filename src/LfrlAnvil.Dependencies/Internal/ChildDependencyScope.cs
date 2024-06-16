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

using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies.Internal;

internal sealed class ChildDependencyScope : DependencyScope, IChildDependencyScope
{
    internal ChildDependencyScope(DependencyContainer container, DependencyScope parentScope, string? name = null)
        : base( container, parentScope, name )
    {
        PrevSibling = null;
        NextSibling = null;
    }

    internal ChildDependencyScope? PrevSibling { get; set; }
    internal ChildDependencyScope? NextSibling { get; set; }

    [Pure]
    public override string ToString()
    {
        return Name is null
            ? $"{nameof( ChildDependencyScope )} [{nameof( Level )}: {Level}, {nameof( OriginalThreadId )}: {OriginalThreadId}]"
            : $"{nameof( ChildDependencyScope )} [{nameof( Name )}: '{Name}', {nameof( Level )}: {Level}, {nameof( OriginalThreadId )}: {OriginalThreadId}]";
    }
}
