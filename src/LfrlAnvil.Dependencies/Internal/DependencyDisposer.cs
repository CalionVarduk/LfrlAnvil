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

namespace LfrlAnvil.Dependencies.Internal;

internal readonly struct DependencyDisposer
{
    internal readonly object Dependency;
    internal readonly Action<object>? Callback;

    internal DependencyDisposer(object dependency, Action<object>? callback)
    {
        Dependency = dependency;
        Callback = callback;
    }

    internal Exception? TryDispose()
    {
        try
        {
            if ( Callback is not null )
                Callback( Dependency );
            else
                ReinterpretCast.To<IDisposable>( Dependency ).Dispose();
        }
        catch ( Exception exc )
        {
            return exc;
        }

        return null;
    }
}
