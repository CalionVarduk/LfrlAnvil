// Copyright 2026 Łukasz Furlepa
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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LfrlAnvil.MessageBroker.Client.Internal;

internal struct PendingBindings
{
    private readonly Dictionary<EntryKey, EntryValue> _entries;
    private ulong _nextVersion;

    private PendingBindings(Dictionary<EntryKey, EntryValue> entries)
    {
        _entries = entries;
        _nextVersion = 0;
    }

    [Pure]
    internal static PendingBindings Create()
    {
        return new PendingBindings( new Dictionary<EntryKey, EntryValue>() );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryAddPublisherBind(string channelName, out ulong version, out bool isBinding)
    {
        return TryAdd( new EntryKey( isPublisher: true, channelName ), isBind: true, out version, out isBinding );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryAddPublisherUnbind(string channelName, out ulong version, out bool isBinding)
    {
        return TryAdd( new EntryKey( isPublisher: true, channelName ), isBind: false, out version, out isBinding );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryAddListenerBind(string channelName, out ulong version, out bool isBinding)
    {
        return TryAdd( new EntryKey( isPublisher: false, channelName ), isBind: true, out version, out isBinding );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryAddListenerUnbind(string channelName, out ulong version, out bool isBinding)
    {
        return TryAdd( new EntryKey( isPublisher: false, channelName ), isBind: false, out version, out isBinding );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryRemovePublisher(string channelName, ulong version)
    {
        return TryRemove( new EntryKey( isPublisher: true, channelName ), version );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryRemoveListener(string channelName, ulong version)
    {
        return TryRemove( new EntryKey( isPublisher: false, channelName ), version );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryDeactivatePublisherBind(string channelName)
    {
        return TryDeactivateBind( new EntryKey( isPublisher: true, channelName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryDeactivateListenerBind(string channelName)
    {
        return TryDeactivateBind( new EntryKey( isPublisher: false, channelName ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Clear()
    {
        _entries.Clear();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool TryAdd(EntryKey key, bool isBind, out ulong version, out bool isBinding)
    {
        ref var value = ref CollectionsMarshal.GetValueRefOrAddDefault( _entries, key, out var exists );
        if ( ! exists )
            value = new EntryValue( unchecked( _nextVersion++ ), isBind );

        version = value.Version;
        isBinding = value.IsBind;
        return ! exists;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool TryRemove(EntryKey key, ulong version)
    {
        if ( ! _entries.Remove( key, out var value ) )
            return false;

        if ( version == value.Version )
            return value.IsActive;

        _entries.Add( key, value );
        return false;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool TryDeactivateBind(EntryKey key)
    {
        ref var value = ref CollectionsMarshal.GetValueRefOrNullRef( _entries, key );
        if ( Unsafe.IsNullRef( ref value ) || ! value.IsBind || ! value.IsActive )
            return false;

        value.IsActive = false;
        return true;
    }

    private readonly struct EntryKey : IEquatable<EntryKey>
    {
        private readonly bool _isPublisher;
        private readonly string _name;

        internal EntryKey(bool isPublisher, string name)
        {
            _isPublisher = isPublisher;
            _name = name;
        }

        [Pure]
        public override string ToString()
        {
            return $"{(_isPublisher ? "Publisher" : "Listener")}: '{_name}'";
        }

        [Pure]
        public override int GetHashCode()
        {
            return HashCode.Combine( _isPublisher, string.GetHashCode( _name, StringComparison.OrdinalIgnoreCase ) );
        }

        [Pure]
        public override bool Equals(object? obj)
        {
            return obj is EntryKey e && Equals( e );
        }

        [Pure]
        public bool Equals(EntryKey other)
        {
            return _isPublisher == other._isPublisher && string.Equals( _name, other._name, StringComparison.OrdinalIgnoreCase );
        }
    }

    private struct EntryValue
    {
        internal readonly ulong Version;
        internal readonly bool IsBind;
        internal bool IsActive;

        internal EntryValue(ulong version, bool isBind)
        {
            Version = version;
            IsBind = isBind;
            IsActive = true;
        }

        [Pure]
        public override string ToString()
        {
            return $"Version = {Version}, IsBind = {IsBind} ({(IsActive ? "active" : "inactive")})";
        }
    }
}
