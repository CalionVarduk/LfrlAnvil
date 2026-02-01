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
using System.Threading;
using LfrlAnvil.Chrono;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal readonly struct TimeoutEntry
{
    internal static Timestamp MaxTimestamp => new Timestamp( long.MaxValue );
    private readonly CancellationTokenSource? _source;

    private TimeoutEntry(CancellationTokenSource? source, Timestamp timestamp)
    {
        _source = source;
        Timestamp = timestamp;
    }

    internal readonly Timestamp Timestamp;

    [Pure]
    public override string ToString()
    {
        var isCancellationRequestedText = _source is null ? "<null>" : _source.IsCancellationRequested.ToString();
        var timestampText = Timestamp == MaxTimestamp ? "<+inf>" : Timestamp.UtcValue.ToString( "yyyy-MM-dd HH:mm:ss.fffffff" );
        return $"[{timestampText}] {nameof( _source.IsCancellationRequested )} = {isCancellationRequestedText}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static TimeoutEntry Empty()
    {
        return new TimeoutEntry( null, MaxTimestamp );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal TimeoutEntry Prepare(Timestamp timestamp)
    {
        var source = _source ?? new CancellationTokenSource();
        return new TimeoutEntry( source, timestamp );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal CancellationToken GetPreparedToken()
    {
        Assume.IsNotNull( _source );
        return _source.Token;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal TimeoutEntry Reset()
    {
        return new TimeoutEntry( _source, MaxTimestamp );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool IsOverdue(Timestamp now)
    {
        return Timestamp <= now;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal TimeoutEntry Cancel()
    {
        if ( _source is null )
            return this;

        _source.TryCancel();
        _source.TryDispose();
        return Empty();
    }
}
