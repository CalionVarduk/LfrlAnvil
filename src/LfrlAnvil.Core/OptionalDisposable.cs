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

namespace LfrlAnvil;

/// <summary>
/// A lightweight generic container for an optional disposable object.
/// </summary>
/// <typeparam name="T">Object type.</typeparam>
public readonly struct OptionalDisposable<T> : IDisposable
    where T : IDisposable
{
    /// <summary>
    /// Represents an empty disposable, without an underlying object.
    /// </summary>
    public static readonly OptionalDisposable<T> Empty = new OptionalDisposable<T>();

    internal OptionalDisposable(T value)
    {
        Value = value;
    }

    /// <summary>
    /// Optional underlying disposable object.
    /// </summary>
    public T? Value { get; }

    /// <inheritdoc />
    /// <remarks>Disposes the underlying <see cref="Value"/> if it exists.</remarks>
    public void Dispose()
    {
        Value?.Dispose();
    }
}
