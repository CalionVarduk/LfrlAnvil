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
using System.Net.Sockets;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Chrono;
using LfrlAnvil.Diagnostics;
using LfrlAnvil.Extensions;
using LfrlAnvil.Memory;

namespace LfrlAnvil.MessageBroker.Client.Internal;

internal static class Defaults
{
    internal static Bounds<int> NameLengthBounds => Bounds.Create( 1, 512 );

    internal static class Tcp
    {
        private const bool NoDelay = false;

        private static Bounds<MemorySize> SocketBufferSizeBounds => Bounds.Create(
            MemorySize.FromBytes( 1 ),
            MemorySize.FromBytes( ushort.MaxValue ) );

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static TcpClient CreateClient(MessageBrokerClientTcpOptions options)
        {
            var actualSocketBufferSize = options.SocketBufferSize is not null
                ? SocketBufferSizeBounds.Clamp( options.SocketBufferSize.Value )
                : SocketBufferSizeBounds.Max;

            var bufferSize = unchecked( ( int )actualSocketBufferSize.Bytes );
            var result = options.LocalEndPoint is not null ? new TcpClient( options.LocalEndPoint ) : new TcpClient();
            result.NoDelay = options.NoDelay ?? NoDelay;
            result.ReceiveBufferSize = bufferSize;
            result.SendBufferSize = bufferSize;

            if ( options.LocalEndPoint is not null && RuntimeInformation.IsOSPlatform( OSPlatform.Linux ) )
                result.Client.SetSocketOption( SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true );

            return result;
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
        internal static Duration GetActualPingInterval(Duration? value)
        {
            return value is not null ? PingIntervalBounds.Clamp( value.Value.TrimToMillisecond() ) : Delay;
        }
    }

    internal static class Memory
    {
        private const short DefaultBatchPacketCount = 30;

        internal static Bounds<MemorySize> MaxNetworkPacketLengthBounds => Bounds.Create(
            MemorySize.FromKilobytes( 16 ),
            MemorySize.FromMegabytes( 1 ) );

        internal static Bounds<MemorySize> MaxNetworkLargePacketLengthBounds => Bounds.Create(
            MemorySize.FromKilobytes( 16 ),
            MemorySize.FromGigabytes( 1 ) );

        private static Bounds<MemorySize> PoolSegmentLengthBounds => Bounds.Create(
            MemorySize.FromKilobytes( 16 ),
            MemorySize.FromBytes( int.MaxValue ) );

        private static MemorySize NetworkLargePacketLength => MemorySize.FromMegabytes( 10 );

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static Bounds<MemorySize> GetNetworkLargePacketLengthBounds(MemorySize min)
        {
            return Bounds.Create( MaxNetworkPacketLengthBounds.Clamp( min ), MemorySize.FromGigabytes( 1 ) );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static MemoryPool<byte> CreatePool(MemorySize? minSegmentLength)
        {
            var actualMinSegmentLength = minSegmentLength is not null
                ? PoolSegmentLengthBounds.Clamp( minSegmentLength.Value )
                : PoolSegmentLengthBounds.Min;

            return new MemoryPool<byte>( unchecked( ( int )actualMinSegmentLength.Bytes ) );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static short GetActualMaxBatchPacketCount(short? value)
        {
            var result = value?.Max( ( short )0 ) ?? DefaultBatchPacketCount;
            return result > 1 ? result : ( short )0;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static int GetActualMaxNetworkBatchPacketLength(MemorySize? value)
        {
            var result = value is not null ? MaxNetworkLargePacketLengthBounds.Clamp( value.Value ) : NetworkLargePacketLength;
            return unchecked( ( int )result.Bytes );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static int GetInitialBufferCapacity(long minCapacity)
        {
            var actualMinCapacity = minCapacity.Clamp( MemorySize.BytesPerKilobyte, int.MaxValue );
            return GetBufferCapacity( unchecked( ( int )actualMinCapacity ) );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static int GetBufferCapacity(int minCapacity)
        {
            Assume.IsGreaterThanOrEqualTo( minCapacity, MemorySize.BytesPerKilobyte );
            var result = BitOperations.RoundUpToPowerOf2( unchecked( ( uint )minCapacity ) );
            return result > int.MaxValue ? int.MaxValue : unchecked( ( int )result );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static int GetRoutingBufferCapacity(int minCapacity)
        {
            var result = BitOperations.RoundUpToPowerOf2( unchecked( ( uint )Math.Max( minCapacity, 64 ) ) );
            return result > int.MaxValue ? int.MaxValue : unchecked( ( int )result );
        }
    }
}
