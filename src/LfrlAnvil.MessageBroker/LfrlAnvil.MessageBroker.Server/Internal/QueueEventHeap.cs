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

using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal struct QueueEventHeap
{
    private ListSlim<Entry> _entries;

    private QueueEventHeap(int capacity)
    {
        _entries = ListSlim<Entry>.Create( capacity );
    }

    [Pure]
    internal static QueueEventHeap Create()
    {
        return new QueueEventHeap( 0 );
    }

    internal void Add(MessageBrokerQueue queue)
    {
        Assume.Equals( queue.EventHeapIndex, -1 );
        queue.EventHeapIndex = _entries.Count;
        _entries.Add( new Entry( queue ) );
        FixUp( queue.EventHeapIndex );
    }

    internal Timestamp Update(MessageBrokerQueue queue)
    {
        var nextEventAt = TimeoutEntry.MaxTimestamp;
        if ( queue.EventHeapIndex >= 0 && queue.EventHeapIndex < _entries.Count )
        {
            ref var first = ref _entries.First();
            ref var node = ref Unsafe.Add( ref first, queue.EventHeapIndex );
            var oldNextEventAt = node.NextEventAt;
            nextEventAt = queue.MessageStore.GetNextEventTimestamp();
            if ( oldNextEventAt != nextEventAt )
            {
                node.NextEventAt = nextEventAt;
                if ( nextEventAt > oldNextEventAt )
                    FixDown( queue.EventHeapIndex );
                else
                    FixUp( queue.EventHeapIndex );
            }
        }

        return nextEventAt;
    }

    internal void Remove(MessageBrokerQueue queue)
    {
        if ( queue.EventHeapIndex >= 0 && queue.EventHeapIndex < _entries.Count )
        {
            var lastIndex = _entries.Count - 1;
            if ( queue.EventHeapIndex == lastIndex )
                _entries.RemoveLast();
            else
            {
                ref var first = ref _entries.First();
                ref var node = ref Unsafe.Add( ref first, queue.EventHeapIndex );
                ref var last = ref Unsafe.Add( ref first, lastIndex );
                var nextEventAt = node.NextEventAt;
                var lastNextEventAt = last.NextEventAt;
                last.Queue.EventHeapIndex = queue.EventHeapIndex;
                node = last;
                _entries.RemoveLast();

                if ( lastNextEventAt > nextEventAt )
                    FixDown( queue.EventHeapIndex );
                else
                    FixUp( queue.EventHeapIndex );
            }
        }

        queue.EventHeapIndex = -1;
    }

    internal void Process(Timestamp now)
    {
        if ( _entries.IsEmpty )
            return;

        ref var first = ref _entries.First();
        while ( first.NextEventAt <= now )
        {
            var queue = first.Queue;
            Assume.Equals( queue.EventHeapIndex, 0 );
            first.NextEventAt = TimeoutEntry.MaxTimestamp;
            FixDown( 0 );
            first = ref _entries.First();

            using ( queue.AcquireLock() )
            {
                if ( ! queue.ShouldCancel && ! queue.MessageStore.IsEmpty )
                    queue.QueueProcessor.SignalContinuation();
            }
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryGetNextTimestamp(out Timestamp result)
    {
        if ( _entries.IsEmpty )
        {
            result = default;
            return false;
        }

        ref var first = ref _entries.First();
        result = first.NextEventAt;
        return true;
    }

    internal void Clear()
    {
        _entries = ListSlim<Entry>.Create();
    }

    private void FixUp(int i)
    {
        var p = (i - 1) >> 1;
        ref var first = ref _entries.First();

        while ( i > 0 )
        {
            ref var node = ref Unsafe.Add( ref first, i );
            ref var parent = ref Unsafe.Add( ref first, p );
            if ( node.NextEventAt >= parent.NextEventAt )
                break;

            (node.Queue.EventHeapIndex, parent.Queue.EventHeapIndex) = (parent.Queue.EventHeapIndex, node.Queue.EventHeapIndex);
            (node, parent) = (parent, node);
            i = p;
            p = (p - 1) >> 1;
        }
    }

    private void FixDown(int i)
    {
        var l = (i << 1) + 1;
        ref var first = ref _entries.First();

        while ( l < _entries.Count )
        {
            ref var node = ref Unsafe.Add( ref first, i );
            ref var child = ref Unsafe.Add( ref first, l );

            var t = i;
            ref var target = ref node;
            if ( node.NextEventAt > child.NextEventAt )
            {
                t = l;
                target = ref child;
            }

            var r = l + 1;
            if ( r < _entries.Count )
            {
                child = ref Unsafe.Add( ref first, r );
                if ( target.NextEventAt > child.NextEventAt )
                {
                    t = r;
                    target = ref child;
                }
            }

            if ( i == t )
                break;

            (node.Queue.EventHeapIndex, target.Queue.EventHeapIndex) = (target.Queue.EventHeapIndex, node.Queue.EventHeapIndex);
            (node, target) = (target, node);
            i = t;
            l = (i << 1) + 1;
        }
    }

    private struct Entry
    {
        internal Entry(MessageBrokerQueue queue)
        {
            Queue = queue;
            NextEventAt = TimeoutEntry.MaxTimestamp;
        }

        internal readonly MessageBrokerQueue Queue;
        internal Timestamp NextEventAt;

        [Pure]
        public override string ToString()
        {
            return $"Queue = ({Queue}), Index = {Queue.EventHeapIndex}, NextEventAt = {NextEventAt}";
        }
    }
}
