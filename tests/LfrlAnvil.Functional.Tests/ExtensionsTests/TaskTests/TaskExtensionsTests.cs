using System.Threading.Tasks;
using LfrlAnvil.Functional.Extensions;

namespace LfrlAnvil.Functional.Tests.ExtensionsTests.TaskTests;

public class TaskExtensionsTests : TestsBase
{
    [Fact]
    public async Task ToNil_ShouldAwaitProvidedTask()
    {
        var action = Substitute.For<Action>();

        var result = await Task.Run( action ).ToNil();

        Assertion.All(
                result.TestEquals( Nil.Instance ),
                action.CallCount().TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public async Task ToNil_ForValueTask_ShouldAwaitProvidedTask()
    {
        var action = Substitute.For<Action>();

        var result = await new ValueTask( Task.Run( action ) ).ToNil();

        Assertion.All(
                result.TestEquals( Nil.Instance ),
                action.CallCount().TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public async Task IgnoreResult_ShouldAwaitProvidedTask()
    {
        var func = Substitute.For<Func<int>>();
        await Task.Run( func ).IgnoreResult();
        func.CallCount().TestEquals( 1 ).Go();
    }

    [Fact]
    public async Task IgnoreResult_ForValueTask_ShouldAwaitProvidedTask()
    {
        var func = Substitute.For<Func<int>>();
        await new ValueTask<int>( Task.Run( func ) ).IgnoreResult();
        func.CallCount().TestEquals( 1 ).Go();
    }
}
