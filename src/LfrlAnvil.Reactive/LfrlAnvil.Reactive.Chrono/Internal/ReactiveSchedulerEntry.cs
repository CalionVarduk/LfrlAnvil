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
using LfrlAnvil.Chrono;

namespace LfrlAnvil.Reactive.Chrono.Internal;

internal readonly struct ReactiveSchedulerEntry<TKey> : IComparable<ReactiveSchedulerEntry<TKey>>
    where TKey : notnull
{
    internal ReactiveSchedulerEntry(ScheduleTaskContainer<TKey> container, Timestamp timestamp, Duration interval, int repetitions)
    {
        Container = container;
        Timestamp = timestamp;
        Interval = interval;
        Repetitions = repetitions;
    }

    public ScheduleTaskContainer<TKey> Container { get; }
    public Timestamp Timestamp { get; }
    public Duration Interval { get; }
    public int Repetitions { get; }
    public bool IsInfinite => Repetitions == -1;
    public bool IsFinished => Repetitions == 0;
    public bool IsDisposed => Repetitions == int.MinValue;

    [Pure]
    public int CompareTo(ReactiveSchedulerEntry<TKey> other)
    {
        return Timestamp.CompareTo( other.Timestamp );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ReactiveSchedulerEntry<TKey> Next()
    {
        Assume.IsGreaterThanOrEqualTo( Repetitions, -1 );
        Assume.NotEquals( Repetitions, 0 );

        return Repetitions switch
        {
            -1 => new ReactiveSchedulerEntry<TKey>( Container, Timestamp + Interval, Interval, Repetitions ),
            1 => new ReactiveSchedulerEntry<TKey>( Container, new Timestamp( long.MaxValue ), Interval, 0 ),
            _ => new ReactiveSchedulerEntry<TKey>( Container, Timestamp + Interval, Interval, Repetitions - 1 )
        };
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ReactiveSchedulerEntry<TKey> AsDisposed()
    {
        return new ReactiveSchedulerEntry<TKey>( Container, new Timestamp( long.MaxValue ), Interval, int.MinValue );
    }
}
