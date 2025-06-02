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

using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.MessageBroker.Client;

/// <summary>
/// Represents basic information about a server-side message broker object.
/// </summary>
public readonly struct MessageBrokerExternalObject : IEquatable<MessageBrokerExternalObject>
{
    /// <summary>
    /// Creates a new <see cref="MessageBrokerExternalObject"/> instance.
    /// </summary>
    /// <param name="id">Unique object id.</param>
    /// <param name="name">Optional object name. Equal to <b>null</b> by default.</param>
    public MessageBrokerExternalObject(int id, string? name = null)
    {
        Id = id;
        Name = name;
    }

    /// <summary>
    /// Unique object id.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Optional object name.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerExternalObject"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"Id = {Id}{(Name is not null ? $", Name = '{Name}'" : string.Empty)}";
    }

    /// <inheritdoc/>
    [Pure]
    public override int GetHashCode()
    {
        return HashCode.Combine( Id, Name );
    }

    /// <inheritdoc/>
    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is MessageBrokerExternalObject o && Equals( o );
    }

    /// <inheritdoc/>
    [Pure]
    public bool Equals(MessageBrokerExternalObject other)
    {
        return Id == other.Id && string.Equals( Name, other.Name, StringComparison.Ordinal );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator ==(MessageBrokerExternalObject a, MessageBrokerExternalObject b)
    {
        return a.Equals( b );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is not equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are not equal, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator !=(MessageBrokerExternalObject a, MessageBrokerExternalObject b)
    {
        return ! a.Equals( b );
    }
}
