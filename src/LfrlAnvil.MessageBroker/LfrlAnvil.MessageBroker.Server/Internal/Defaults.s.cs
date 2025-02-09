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
using LfrlAnvil.Diagnostics;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal static class Defaults
{
    internal static Bounds<int> NameLengthBounds => Bounds.Create( 1, 512 );

    internal static class Tcp
    {
        internal const bool NoDelay = false;

        private static Bounds<MemorySize> SocketBufferSizeBounds => Bounds.Create(
            MemorySize.FromBytes( 1 ),
            MemorySize.FromBytes( ushort.MaxValue ) );

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static int GetActualSocketBufferSize(MemorySize? value)
        {
            var result = value is not null ? SocketBufferSizeBounds.Clamp( value.Value ) : SocketBufferSizeBounds.Max;
            return unchecked( ( int )result.Bytes );
        }
    }

    internal static class Temporal
    {
        internal static Duration Delay => Duration.FromSeconds( 15 );
        internal static Duration MaxTimeout => Duration.FromMilliseconds( int.MaxValue );

        internal static Bounds<Duration> PingIntervalBounds => Bounds.Create(
            Duration.FromMilliseconds( 1 ),
            Duration.FromHours( ChronoConstants.HoursPerStandardDay ) );

        internal static Bounds<Duration> TimeoutBounds => Bounds.Create( Duration.FromMilliseconds( 1 ), MaxTimeout );

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static Duration GetActualTimeout(Duration? value)
        {
            return value is not null ? TimeoutBounds.Clamp( value.Value.TrimToMillisecond() ) : Delay;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static Bounds<Duration> GetActualTimeoutBounds(Bounds<Duration>? value)
        {
            return value is not null
                ? Bounds.Create(
                    TimeoutBounds.Clamp( value.Value.Min.TrimToMillisecond() ),
                    TimeoutBounds.Clamp( value.Value.Max.TrimToMillisecond() ) )
                : TimeoutBounds;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static Bounds<Duration> GetActualPingIntervalBounds(Bounds<Duration>? value)
        {
            return value is not null
                ? Bounds.Create(
                    PingIntervalBounds.Clamp( value.Value.Min.TrimToMillisecond() ),
                    PingIntervalBounds.Clamp( value.Value.Max.TrimToMillisecond() ) )
                : PingIntervalBounds;
        }
    }

    internal static class Memory
    {
        private static Bounds<MemorySize> PoolSegmentLengthBounds => Bounds.Create(
            MemorySize.FromKilobytes( 16 ),
            MemorySize.FromBytes( int.MaxValue ) );

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static int GetActualMinSegmentLength(MemorySize? value)
        {
            var result = value is not null ? PoolSegmentLengthBounds.Clamp( value.Value ) : PoolSegmentLengthBounds.Min;
            return unchecked( ( int )result.Bytes );
        }
    }
}
