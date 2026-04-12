// Copyright 2025-2026 Łukasz Furlepa
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
using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal struct QueueRetryHeap
{
    private ListSlim<Entry> _entries;

    private QueueRetryHeap(int capacity)
    {
        _entries = ListSlim<Entry>.Create( capacity );
    }

    internal bool IsEmpty => _entries.IsEmpty;
    internal int Count => _entries.Count;
    internal ref Entry this[int index] => ref _entries[index];

    [Pure]
    internal static QueueRetryHeap Create()
    {
        return new QueueRetryHeap( 0 );
    }

    internal void Add(QueueMessage message, int retry, int redelivery, Duration delay)
    {
        Assume.IsGreaterThanOrEqualTo( retry, 0 );
        Assume.IsGreaterThanOrEqualTo( redelivery, 0 );
        Assume.IsGreaterThanOrEqualTo( delay, Duration.Zero );
        Add( message, unchecked( retry + 1 ), redelivery, message.Listener.Owner.Client.GetTimestamp() + delay );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Add(QueueMessage message, int retry, int redelivery, Timestamp sendAt)
    {
        Assume.IsGreaterThanOrEqualTo( retry, 0 );
        Assume.IsGreaterThanOrEqualTo( redelivery, 0 );

        var index = _entries.Count;
        _entries.Add( new Entry( message, retry, redelivery, sendAt ) );
        FixUp( index );
    }

    [Pure]
    internal ref Entry First()
    {
        return ref _entries.First();
    }

    internal void Pop()
    {
        Assume.False( IsEmpty );
        ref var first = ref _entries.First();
        ref var last = ref Unsafe.Add( ref first, _entries.Count - 1 );
        first = last;
        _entries.RemoveLast();
        FixDown( 0 );
    }

    internal void Clear()
    {
        _entries = ListSlim<Entry>.Create();
    }

    private void FixUp(int i)
    {
        ref var first = ref _entries.First();

        while ( i > 0 )
        {
            var p = (i - 1) >> 1;
            ref var entry = ref Unsafe.Add( ref first, i );
            ref var parent = ref Unsafe.Add( ref first, p );
            if ( entry.SendAt >= parent.SendAt )
                break;

            (entry, parent) = (parent, entry);
            i = p;
        }
    }

    private void FixDown(int i)
    {
        ref var first = ref _entries.First();
        var l = (i << 1) + 1;

        while ( l < _entries.Count )
        {
            ref var entry = ref Unsafe.Add( ref first, i );
            ref var target = ref Unsafe.Add( ref first, l );

            var t = l;
            if ( entry.SendAt < target.SendAt )
            {
                t = i;
                target = ref entry;
            }

            var r = l + 1;
            if ( r < _entries.Count )
            {
                ref var right = ref Unsafe.Add( ref first, r );
                if ( right.SendAt < target.SendAt )
                {
                    t = r;
                    target = ref right;
                }
            }

            if ( i == t )
                break;

            (entry, target) = (target, entry);
            i = t;
            l = (i << 1) + 1;
        }
    }

    internal readonly struct Entry
    {
        internal Entry(QueueMessage message, int retry, int redelivery, Timestamp sendAt)
        {
            Message = message;
            Retry = retry;
            Redelivery = redelivery;
            SendAt = sendAt;
        }

        internal readonly QueueMessage Message;
        internal readonly int Retry;
        internal readonly int Redelivery;
        internal readonly Timestamp SendAt;

        [Pure]
        public override string ToString()
        {
            return $"Message = ({Message}), Retry = {Retry}, Redelivery = {Redelivery}, SendAt = {SendAt}";
        }
    }
}
