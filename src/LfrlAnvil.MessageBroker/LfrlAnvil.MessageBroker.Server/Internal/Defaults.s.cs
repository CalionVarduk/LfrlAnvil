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
using System.Numerics;
using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono;
using LfrlAnvil.Diagnostics;
using LfrlAnvil.Extensions;

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
        internal const int DefaultNetworkPacketLength = ( int )MemorySize.BytesPerKilobyte * 16;
        private const short DefaultMaxBatchPacketCount = 100;

        internal static Bounds<MemorySize> MaxNetworkPacketLengthBounds => Bounds.Create(
            MemorySize.FromBytes( DefaultNetworkPacketLength ),
            MemorySize.FromMegabytes( 1 ) );

        internal static MemorySize MaxNetworkLargePacketLength => MemorySize.FromGigabytes( 1 );
        private static MemorySize NetworkLargePacketLength => MemorySize.FromMegabytes( 10 );

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static MemorySize GetActualMaxNetworkPacketLength(MemorySize? maxValue)
        {
            return maxValue is not null ? MaxNetworkPacketLengthBounds.Clamp( maxValue.Value ) : MaxNetworkPacketLengthBounds.Min;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static MemorySize GetActualMaxNetworkLargePacketLength(MemorySize maxNetworkPacketLength, MemorySize? maxValue)
        {
            Assume.IsInRange( maxNetworkPacketLength, MaxNetworkPacketLengthBounds.Min, MaxNetworkPacketLengthBounds.Max );
            return maxValue?.Clamp( maxNetworkPacketLength, MaxNetworkLargePacketLength ) ?? NetworkLargePacketLength;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static Bounds<short> GetActualBatchPacketCountBounds(short? value)
        {
            var max = value?.Max( ( short )0 ) ?? DefaultMaxBatchPacketCount;
            return Bounds.Create<short>( 0, max > 1 ? max : ( short )0 );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static int GetBufferCapacity(int minCapacity)
        {
            var result = BitOperations.RoundUpToPowerOf2( unchecked( ( uint )Math.Max( minCapacity, 8 ) ) );
            return result > int.MaxValue ? int.MaxValue : unchecked( ( int )result );
        }
    }
}
