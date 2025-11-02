using System.Threading.Tasks;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Tests.ExtensionsTests.TaskTests;

public class TaskExtensionsTests : TestsBase
{
    [Fact]
    public void CompletedIfNull_ShouldReturnProvidedTask_IfNotNull()
    {
        var sut = Task.Factory.StartNew( () => { } );
        var result = sut.CompletedIfNull();
        result.TestRefEquals( sut ).Go();
    }

    [Fact]
    public void CompletedIfNull_ShouldReturnCompletedTask_IfProvidedTaskIsNull()
    {
        var sut = ( Task? )null;
        var result = sut.CompletedIfNull();
        result.TestRefEquals( Task.CompletedTask ).Go();
    }

    [Fact]
    public async Task AsSafe_ShouldCompleteWithoutException_WhenProvidedTaskCompletesSuccessfully()
    {
        var sut = Task.Factory.StartNew( () => { } );
        var result = await sut.AsSafe();
        result.Exception.TestNull().Go();
    }

    [Fact]
    public async Task AsSafe_ShouldCompleteWithException_WhenProvidedTaskThrows()
    {
        var exception = new Exception( "foo" );
        var sut = Task.Factory.StartNew( () => throw exception );
        var result = await sut.AsSafe();
        result.Exception.TestRefEquals( exception ).Go();
    }

    [Fact]
    public async Task AsSafe_ValueTask_ShouldCompleteWithoutException_WhenProvidedTaskCompletesSuccessfully()
    {
        var sut = ToValueTask( Task.Factory.StartNew( () => { } ) );
        var result = await sut.AsSafe();
        result.Exception.TestNull().Go();
    }

    [Fact]
    public async Task AsSafe_ValueTask_ShouldCompleteWithException_WhenProvidedTaskThrows()
    {
        var exception = new Exception( "foo" );
        var sut = ToValueTask( Task.Factory.StartNew( () => throw exception ) );
        var result = await sut.AsSafe();
        result.Exception.TestRefEquals( exception ).Go();
    }

    [Fact]
    public async Task AsSafe_WithResult_ShouldCompleteWithoutException_WhenProvidedTaskCompletesSuccessfully()
    {
        var sut = Task.FromResult( 123 );
        var result = await sut.AsSafe();
        Assertion.All( result.Exception.TestNull(), result.Value.TestEquals( 123 ) ).Go();
    }

    [Fact]
    public async Task AsSafe_WithResult_ShouldCompleteWithException_WhenProvidedTaskThrows()
    {
        var exception = new Exception( "foo" );
        var sut = Task.FromException<int>( exception );
        var result = await sut.AsSafe();
        result.Exception.TestRefEquals( exception ).Go();
    }

    [Fact]
    public async Task AsSafe_WithResult_ValueTask_ShouldCompleteWithoutException_WhenProvidedTaskCompletesSuccessfully()
    {
        var sut = ValueTask.FromResult( 123 );
        var result = await sut.AsSafe();
        Assertion.All( result.Exception.TestNull(), result.Value.TestEquals( 123 ) ).Go();
    }

    [Fact]
    public async Task AsSafe_WithResult_ValueTask_ShouldCompleteWithException_WhenProvidedTaskThrows()
    {
        var exception = new Exception( "foo" );
        var sut = ValueTask.FromException<int>( exception );
        var result = await sut.AsSafe();
        result.Exception.TestRefEquals( exception ).Go();
    }

    private static async ValueTask ToValueTask(Task task)
    {
        await task;
    }
}
