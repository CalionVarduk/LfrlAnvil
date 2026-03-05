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
using System.Runtime.CompilerServices;
using LfrlAnvil.MessageBroker.Client.Internal;

namespace LfrlAnvil.MessageBroker.Client;

/// <summary>
/// Represents a message routing target, based on either a target client id or name.
/// </summary>
public readonly struct MessageBrokerClientRoutingTarget
{
    private readonly string? _name;

    private MessageBrokerClientRoutingTarget(int id, string name)
    {
        _name = name;
        Id = id;
    }

    /// <summary>
    /// Target client id. Equal to <b>0</b> if not used.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Target client name. Empty if not used.
    /// </summary>
    public string Name => _name ?? string.Empty;

    /// <summary>
    /// Specifies whether this routing target is based on a name.
    /// </summary>
    public bool IsFromName => Id == 0;

    /// <summary>
    /// Creates a new routing target based on an <paramref name="id"/>.
    /// </summary>
    /// <param name="id">Target client id.</param>
    /// <returns>New <see cref="MessageBrokerClientRoutingTarget"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="id"/> is less than or equal to <b>0</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MessageBrokerClientRoutingTarget FromId(int id)
    {
        Ensure.IsGreaterThan( id, 0 );
        return new MessageBrokerClientRoutingTarget( id, string.Empty );
    }

    /// <summary>
    /// Creates a new routing target based on a <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Target client name.</param>
    /// <returns>New <see cref="MessageBrokerClientRoutingTarget"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="name"/>'s length is less than <b>1</b> or greater than <b>512</b>.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MessageBrokerClientRoutingTarget FromName(string name)
    {
        Ensure.IsInRange( name.Length, Defaults.NameLengthBounds.Min, Defaults.NameLengthBounds.Max );
        return new MessageBrokerClientRoutingTarget( 0, name );
    }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerClientRoutingTarget"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return IsFromName ? $"Name = {Name}" : $"Id = {Id}";
    }

    /// <summary>
    /// Converts provided <paramref name="id"/> to a routing target.
    /// </summary>
    /// <param name="id">Target client id.</param>
    /// <returns>New <see cref="MessageBrokerClientRoutingTarget"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="id"/> is less than or equal to <b>0</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator MessageBrokerClientRoutingTarget(int id)
    {
        return FromId( id );
    }

    /// <summary>
    /// Converts provided <paramref name="name"/> to a routing target.
    /// </summary>
    /// <param name="name">Target client name.</param>
    /// <returns>New <see cref="MessageBrokerClientRoutingTarget"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="name"/>'s length is less than <b>1</b> or greater than <b>512</b>.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator MessageBrokerClientRoutingTarget(string name)
    {
        return FromName( name );
    }
}
