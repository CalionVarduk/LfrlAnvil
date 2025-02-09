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
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using LfrlAnvil.Async;
using LfrlAnvil.MessageBroker.Server.Events;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal struct RequestHandler
{
    private readonly ManualResetValueTaskSource<bool> _continuation;
    private Task? _task;

    private RequestHandler(ManualResetValueTaskSource<bool> continuation)
    {
        _continuation = continuation;
        _task = null;
    }

    [Pure]
    internal static RequestHandler Create()
    {
        return new RequestHandler( new ManualResetValueTaskSource<bool>() );
    }

    [MethodImpl( MethodImplOptions.NoInlining )]
    internal static async Task StartUnderlyingTask(MessageBrokerRemoteClient client)
    {
        TaskStopReason stopReason;
        try
        {
            stopReason = await RunCore( client ).ConfigureAwait( false );
        }
        catch ( Exception exc )
        {
            client.Emit( MessageBrokerRemoteClientEvent.Unexpected( client, exc ) );
            stopReason = TaskStopReason.Error;
        }

        if ( stopReason == TaskStopReason.OwnerDisposed )
            return;

        using ( client.AcquireLock() )
            client.RequestHandler._task = null;

        await client.DisposeAsync().ConfigureAwait( false );
    }

    internal void Dispose()
    {
        if ( _continuation.Status == ValueTaskSourceStatus.Pending )
            _continuation.SetResult( false );
    }

    internal void SetUnderlyingTask(Task task)
    {
        Assume.IsNull( _task );
        _task = task;
    }

    internal Task? DiscardUnderlyingTask()
    {
        var result = _task;
        _task = null;
        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void SignalContinuation()
    {
        if ( _continuation.Status == ValueTaskSourceStatus.Pending )
            _continuation.SetResult( true );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static async ValueTask<TaskStopReason> RunCore(MessageBrokerRemoteClient client)
    {
        var pingResponseData = new byte[Protocol.PacketHeader.Length].AsMemory();
        var pingResponse = Protocol.PingResponse.Create();
        pingResponse.Serialize( pingResponseData );

        while ( true )
        {
            var @continue = await client.RequestHandler._continuation.GetTask().ConfigureAwait( false );
            if ( ! @continue )
                return TaskStopReason.OwnerDisposed;

            bool containsEnqueuedRequests;
            do
            {
                ulong contextId;
                IncomingPacketToken request;
                using ( client.AcquireLock() )
                {
                    if ( client.ShouldCancel )
                        return TaskStopReason.OwnerDisposed;

                    contextId = client.MessageContextQueue.AcquireContextId();
                    request = client.MessageContextQueue.DequeueRequest();
                }

                Assume.Equals( request.Header.GetServerEndpoint(), MessageBrokerServerEndpoint.PingRequest );
                client.Emit( MessageBrokerRemoteClientEvent.MessageReceived( client, request.Header, contextId ) );

                if ( request.Header.Payload != Protocol.Endianness.VerificationPayload )
                {
                    client.Emit(
                        MessageBrokerRemoteClientEvent.MessageRejected(
                            client,
                            request.Header,
                            Protocol.EndiannessPayloadException( client, request.Header ),
                            contextId ) );

                    return TaskStopReason.Error;
                }

                client.Emit( MessageBrokerRemoteClientEvent.MessageAccepted( client, request.Header, contextId ) );

                ManualResetValueTaskSource<bool> writerSource;
                using ( client.AcquireLock() )
                {
                    if ( client.ShouldCancel )
                        return TaskStopReason.OwnerDisposed;

                    writerSource = client.MessageContextQueue.AcquireWriterSource();
                }

                if ( ! await writerSource.GetTask().ConfigureAwait( false ) )
                    return TaskStopReason.OwnerDisposed;

                var result = await client.WriteAsync( pingResponse, pingResponseData, contextId ).ConfigureAwait( false );
                if ( result.Exception is not null )
                    return TaskStopReason.Error;

                using ( client.AcquireLock() )
                {
                    if ( client.ShouldCancel )
                        return TaskStopReason.OwnerDisposed;

                    client.MessageContextQueue.ResetOutgoingWriter( client, writerSource );
                    containsEnqueuedRequests = client.MessageContextQueue.ContainsEnqueuedRequests();
                }
            }
            while ( containsEnqueuedRequests );

            using ( client.AcquireLock() )
            {
                if ( client.ShouldCancel )
                    return TaskStopReason.OwnerDisposed;

                client.RequestHandler._continuation.Reset();
                if ( client.MessageContextQueue.ContainsEnqueuedRequests() )
                    client.RequestHandler._continuation.SetResult( true );
            }
        }
    }
}
