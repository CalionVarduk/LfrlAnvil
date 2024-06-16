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
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Reactive.Exceptions;

internal static class Resources
{
    internal const string DisposedEventSource = "Event source is disposed.";
    internal const string DisposedEventExchange = "Event exchange is disposed.";
    internal const string CurrentSynchronizationContextCannotBeNull = "Current synchronization context cannot be null.";

    internal const string InvalidEventPublisherDisposal =
        "Disposal subscriber of a publisher owned by an event exchange cannot be manually disposed.";

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidArgumentType(object? argument, Type expectedType)
    {
        var argumentTypeName = argument is null ? "<null>" : argument.GetType().GetDebugString();
        return $"Expected argument of type {expectedType.GetDebugString()} but found {argumentTypeName}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string EventPublisherNotFound(Type eventType)
    {
        return $"Event publisher for event type {eventType.GetDebugString()} was not found.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string EventPublisherAlreadyExists(Type eventType)
    {
        return $"Event publisher for event type {eventType.GetDebugString()} already exists.";
    }
}
