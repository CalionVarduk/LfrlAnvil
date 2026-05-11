// Copyright 2024-2026 Łukasz Furlepa
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
using System.Threading.Tasks;

namespace LfrlAnvil.Dependencies.Internal;

internal readonly struct DependencyDisposer
{
    internal readonly object Dependency;
    internal readonly Delegate? Callback;
    internal readonly bool IsAsync;

    internal DependencyDisposer(object dependency, Delegate? callback, bool isAsync)
    {
        Dependency = dependency;
        Callback = callback;
        IsAsync = isAsync;
    }

    internal Exception? TryDispose()
    {
        Assume.False( IsAsync );
        try
        {
            if ( Callback is not null )
                ReinterpretCast.To<Action<object>>( Callback )( Dependency );
            else
                (( IDisposable )Dependency).Dispose();
        }
        catch ( Exception exc )
        {
            return exc;
        }

        return null;
    }

    internal async ValueTask<Exception?> TryDisposeAsync()
    {
        Assume.True( IsAsync );
        try
        {
            var task = Callback is not null
                ? ReinterpretCast.To<Func<object, ValueTask>>( Callback )( Dependency )
                : (( IAsyncDisposable )Dependency).DisposeAsync();

            await task.ConfigureAwait( false );
        }
        catch ( Exception exc )
        {
            return exc;
        }

        return null;
    }
}
