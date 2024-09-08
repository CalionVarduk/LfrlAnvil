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

using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using LfrlAnvil.Async;

namespace LfrlAnvil.Tests.AsyncTests;

public class ManualResetValueTaskSourceTests : TestsBase
{
    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void Ctor_ShouldCreateCorrectResult(bool runContinuationsAsynchronously)
    {
        var sut = new ManualResetValueTaskSource<int>( runContinuationsAsynchronously );

        using ( new AssertionScope() )
        {
            sut.RunContinuationsAsynchronously.Should().Be( runContinuationsAsynchronously );
            sut.Status.Should().Be( ValueTaskSourceStatus.Pending );
        }
    }

    [Fact]
    public async Task SetResult_ShouldCompleteCurrentOperation()
    {
        var sut = new ManualResetValueTaskSource<int>();

        _ = Task.Run(
            async () =>
            {
                await Task.Delay( 15 );
                sut.SetResult( 123 );
            } );

        var result = await sut.GetTask();

        using ( new AssertionScope() )
        {
            result.Should().Be( 123 );
            sut.Status.Should().Be( ValueTaskSourceStatus.Succeeded );
        }
    }

    [Fact]
    public async Task SetException_ShouldCompleteCurrentOperation()
    {
        var exception = new Exception( "foo" );
        var sut = new ManualResetValueTaskSource<int>();

        _ = Task.Run(
            async () =>
            {
                await Task.Delay( 15 );
                sut.SetException( exception );
            } );

        Exception? caughtException = null;
        try
        {
            _ = await sut.GetTask();
        }
        catch ( Exception exc )
        {
            caughtException = exc;
        }

        using ( new AssertionScope() )
        {
            caughtException.Should().BeSameAs( exception );
            sut.Status.Should().Be( ValueTaskSourceStatus.Faulted );
        }
    }

    [Fact]
    public async Task SetCancelled_ShouldCompleteCurrentOperation()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var token = cancellationTokenSource.Token;
        cancellationTokenSource.Cancel();

        var sut = new ManualResetValueTaskSource<int>();

        _ = Task.Run(
            async () =>
            {
                await Task.Delay( 15 );
                sut.SetCancelled( token );
            } );

        Exception? caughtException = null;
        try
        {
            _ = await sut.GetTask();
        }
        catch ( Exception exc )
        {
            caughtException = exc;
        }

        using ( new AssertionScope() )
        {
            caughtException.Should().BeOfType<OperationCanceledException>();
            ((caughtException as OperationCanceledException)?.CancellationToken).Should().Be( token );
            sut.Status.Should().Be( ValueTaskSourceStatus.Canceled );
        }
    }

    [Fact]
    public async Task Reset_ShouldStartNextOperation()
    {
        var sut = new ManualResetValueTaskSource<int>();

        _ = Task.Run(
            async () =>
            {
                await Task.Delay( 15 );
                sut.SetResult( 123 );
            } );

        _ = await sut.GetTask();
        sut.Reset();

        _ = Task.Run(
            async () =>
            {
                await Task.Delay( 15 );
                sut.SetResult( 456 );
            } );

        var result = await sut.GetTask();

        using ( new AssertionScope() )
        {
            result.Should().Be( 456 );
            sut.Status.Should().Be( ValueTaskSourceStatus.Succeeded );
        }
    }

    [Fact]
    public void ToString_ShouldReturnInformationAboutVersionAndStatus()
    {
        var sut = new ManualResetValueTaskSource<int>();
        var result = sut.ToString();
        result.Should().Be( "Version: 0, Status: Pending" );
    }
}
