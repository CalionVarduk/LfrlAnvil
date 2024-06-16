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
using System.Threading;

namespace LfrlAnvil.Async;

/// <summary>
/// A lightweight object with a generic value and a <see cref="CancellationToken"/> instance.
/// </summary>
/// <typeparam name="T">Value's type.</typeparam>
public readonly struct Cancellable<T>
{
    /// <summary>
    /// Creates a <see cref="Cancellable{T}"/> instance.
    /// </summary>
    /// <param name="value">Value to assign.</param>
    /// <param name="token"><see cref="CancellationToken"/> to assign.</param>
    public Cancellable(T value, CancellationToken token)
    {
        Value = value;
        Token = token;
    }

    /// <summary>
    /// Assigned value.
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// Assigned <see cref="CancellationToken"/>.
    /// </summary>
    public CancellationToken Token { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="Cancellable{T}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{nameof( Value )}: {Value}, {nameof( Token.IsCancellationRequested )}: {Token.IsCancellationRequested}";
    }
}
