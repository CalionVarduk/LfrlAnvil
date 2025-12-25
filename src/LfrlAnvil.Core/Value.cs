// Copyright 2025 Łukasz Furlepa
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

namespace LfrlAnvil;

/// <summary>
/// Represents a single generic value.
/// </summary>
/// <param name="Item">Underlying value.</param>
/// <typeparam name="T">Value type.</typeparam>
public readonly record struct Value<T>(T Item)
{
    /// <summary>
    /// Converts provided <paramref name="value"/> to the underlying value type.
    /// </summary>
    /// <param name="value">Object to convert.</param>
    /// <returns><see cref="Item"/> from the <paramref name="value"/>.</returns>
    public static implicit operator T(Value<T> value)
    {
        return value.Item;
    }

    /// <summary>
    /// Converts provided <paramref name="item"/> to <see cref="Value{T}"/>.
    /// </summary>
    /// <param name="item">Object to convert.</param>
    /// <returns>New <see cref="Value{T}"/> instance.</returns>
    public static implicit operator Value<T>(T item)
    {
        return new Value<T>( item );
    }
}
