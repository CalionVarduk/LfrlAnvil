using System.Threading.Tasks;
using FluentAssertions.Execution;
using LfrlAnvil.Functional;
using LfrlAnvil.Requests.Exceptions;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Requests.Tests.RequestDispatcherTests;

public class RequestDispatcherTests : TestsBase
{
    [Fact]
    public void Dispatch_ForRequestClass_ShouldCallRequestHandler_WhenHandlerExists()
    {
        var request = new TestRequestClass();
        var expectedResult = Fixture.Create<int>();
        var handler = Substitute.For<IRequestHandler<TestRequestClass, int>>();
        handler.Handle( request ).Returns( _ => expectedResult );

        var factory = Substitute.For<IRequestHandlerFactory>();
        factory.TryCreate<TestRequestClass, int>().Returns( _ => handler );

        var sut = new RequestDispatcher( factory );

        var result = sut.Dispatch( request );

        result.Should().Be( expectedResult );
    }

    [Fact]
    public void Dispatch_ForRequestClass_ShouldThrowMissingRequestHandlerException_WhenHandlerDoesNotExist()
    {
        var factory = Substitute.For<IRequestHandlerFactory>();
        factory.TryCreate<TestRequestClass, int>().Returns( _ => null );
        var sut = new RequestDispatcher( factory );

        var action = Lambda.Of( () => sut.Dispatch( new TestRequestClass() ) );

        action.Should().ThrowExactly<MissingRequestHandlerException>().AndMatch( e => e.RequestType == typeof( TestRequestClass ) );
    }

    [Fact]
    public void Dispatch_ForRequestClass_ShouldThrowInvalidRequestTypeException_WhenRequestTypeIsInvalid()
    {
        var factory = Substitute.For<IRequestHandlerFactory>();
        var sut = new RequestDispatcher( factory );

        var action = Lambda.Of( () => sut.Dispatch( new InvalidTestRequestClass() ) );

        action.Should()
            .ThrowExactly<InvalidRequestTypeException>()
            .AndMatch( e => e.RequestType == typeof( InvalidTestRequestClass ) && e.ExpectedType == typeof( TestRequestClass ) );
    }

    [Fact]
    public void Dispatch_ForRequestStruct_ShouldCallRequestHandler_WhenHandlerExists()
    {
        var request = new TestRequestStruct();
        var expectedResult = Fixture.Create<int>();
        var handler = Substitute.For<IRequestHandler<TestRequestStruct, int>>();
        handler.Handle( request ).Returns( _ => expectedResult );

        var factory = Substitute.For<IRequestHandlerFactory>();
        factory.TryCreate<TestRequestStruct, int>().Returns( _ => handler );

        var sut = new RequestDispatcher( factory );

        var result = sut.Dispatch<TestRequestStruct, int>( request );

        result.Should().Be( expectedResult );
    }

    [Fact]
    public void Dispatch_ForRequestStruct_ShouldThrowMissingRequestHandlerException_WhenHandlerDoesNotExist()
    {
        var factory = Substitute.For<IRequestHandlerFactory>();
        factory.TryCreate<TestRequestStruct, int>().Returns( _ => null );
        var sut = new RequestDispatcher( factory );

        var action = Lambda.Of( () => sut.Dispatch<TestRequestStruct, int>( new TestRequestStruct() ) );
        action.Should().ThrowExactly<MissingRequestHandlerException>().AndMatch( e => e.RequestType == typeof( TestRequestStruct ) );
    }

    [Fact]
    public void TryDispatch_ForRequestClass_ShouldCallRequestHandler_WhenHandlerExists()
    {
        var request = new TestRequestClass();
        var expectedResult = Fixture.Create<int>();
        var handler = Substitute.For<IRequestHandler<TestRequestClass, int>>();
        handler.Handle( request ).Returns( _ => expectedResult );

        var factory = Substitute.For<IRequestHandlerFactory>();
        factory.TryCreate<TestRequestClass, int>().Returns( _ => handler );

        var sut = new RequestDispatcher( factory );

        var result = sut.TryDispatch( request, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().Be( expectedResult );
        }
    }

    [Fact]
    public void TryDispatch_ForRequestClass_ShouldReturnFalse_WhenHandlerDoesNotExist()
    {
        var factory = Substitute.For<IRequestHandlerFactory>();
        factory.TryCreate<TestRequestClass, int>().Returns( _ => null );
        var sut = new RequestDispatcher( factory );

        var result = sut.TryDispatch( new TestRequestClass(), out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().Be( default );
        }
    }

    [Fact]
    public void TryDispatch_ForRequestClass_ShouldThrowInvalidRequestTypeException_WhenRequestTypeIsInvalid()
    {
        var factory = Substitute.For<IRequestHandlerFactory>();
        var sut = new RequestDispatcher( factory );

        var action = Lambda.Of( () => sut.TryDispatch( new InvalidTestRequestClass(), out _ ) );

        action.Should()
            .ThrowExactly<InvalidRequestTypeException>()
            .AndMatch( e => e.RequestType == typeof( InvalidTestRequestClass ) && e.ExpectedType == typeof( TestRequestClass ) );
    }

    [Fact]
    public void TryDispatch_ForRequestStruct_ShouldCallRequestHandler_WhenHandlerExists()
    {
        var request = new TestRequestStruct();
        var expectedResult = Fixture.Create<int>();
        var handler = Substitute.For<IRequestHandler<TestRequestStruct, int>>();
        handler.Handle( request ).Returns( _ => expectedResult );

        var factory = Substitute.For<IRequestHandlerFactory>();
        factory.TryCreate<TestRequestStruct, int>().Returns( _ => handler );

        var sut = new RequestDispatcher( factory );

        var result = sut.TryDispatch( request, out int outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().Be( expectedResult );
        }
    }

    [Fact]
    public void TryDispatch_ForRequestStruct_ShouldReturnFalse_WhenHandlerDoesNotExist()
    {
        var factory = Substitute.For<IRequestHandlerFactory>();
        factory.TryCreate<TestRequestStruct, int>().Returns( _ => null );
        var sut = new RequestDispatcher( factory );

        var result = sut.TryDispatch( new TestRequestStruct(), out int outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().Be( default );
        }
    }

    [Fact]
    public async Task Dispatch_ForAsyncTaskRequestClass_ShouldCallRequestHandler_WhenHandlerExists()
    {
        var request = new TestAsyncTaskRequestClass();
        var expectedResult = Fixture.Create<int>();
        var handler = Substitute.For<IRequestHandler<TestAsyncTaskRequestClass, Task<int>>>();
        handler.Handle( request ).Returns( _ => expectedResult );

        var factory = Substitute.For<IRequestHandlerFactory>();
        factory.TryCreate<TestAsyncTaskRequestClass, Task<int>>().Returns( _ => handler );

        var sut = new RequestDispatcher( factory );

        var result = await sut.Dispatch( request );

        result.Should().Be( expectedResult );
    }

    [Fact]
    public async ValueTask Dispatch_ForAsyncValueTaskRequestClass_ShouldCallRequestHandler_WhenHandlerExists()
    {
        var request = new TestAsyncValueTaskRequestClass();
        var expectedResult = Fixture.Create<int>();
        var handler = Substitute.For<IRequestHandler<TestAsyncValueTaskRequestClass, ValueTask<int>>>();
        handler.Handle( request ).Returns( _ => expectedResult );

        var factory = Substitute.For<IRequestHandlerFactory>();
        factory.TryCreate<TestAsyncValueTaskRequestClass, ValueTask<int>>().Returns( _ => handler );

        var sut = new RequestDispatcher( factory );

        var result = await sut.Dispatch( request );

        result.Should().Be( expectedResult );
    }

    [Fact]
    public async Task Dispatch_ForAsyncTaskRequestStruct_ShouldCallRequestHandler_WhenHandlerExists()
    {
        var request = new TestAsyncTaskRequestStruct();
        var expectedResult = Fixture.Create<int>();
        var handler = Substitute.For<IRequestHandler<TestAsyncTaskRequestStruct, Task<int>>>();
        handler.Handle( request ).Returns( _ => expectedResult );

        var factory = Substitute.For<IRequestHandlerFactory>();
        factory.TryCreate<TestAsyncTaskRequestStruct, Task<int>>().Returns( _ => handler );

        var sut = new RequestDispatcher( factory );

        var result = await sut.Dispatch<TestAsyncTaskRequestStruct, Task<int>>( request );

        result.Should().Be( expectedResult );
    }

    [Fact]
    public async ValueTask Dispatch_ForAsyncValueTaskRequestStruct_ShouldCallRequestHandler_WhenHandlerExists()
    {
        var request = new TestAsyncValueTaskRequestStruct();
        var expectedResult = Fixture.Create<int>();
        var handler = Substitute.For<IRequestHandler<TestAsyncValueTaskRequestStruct, ValueTask<int>>>();
        handler.Handle( request ).Returns( _ => expectedResult );

        var factory = Substitute.For<IRequestHandlerFactory>();
        factory.TryCreate<TestAsyncValueTaskRequestStruct, ValueTask<int>>().Returns( _ => handler );

        var sut = new RequestDispatcher( factory );

        var result = await sut.Dispatch<TestAsyncValueTaskRequestStruct, ValueTask<int>>( request );

        result.Should().Be( expectedResult );
    }
}
