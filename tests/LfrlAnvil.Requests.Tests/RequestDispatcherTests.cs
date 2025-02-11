using System.Threading.Tasks;
using LfrlAnvil.Functional;
using LfrlAnvil.Requests.Exceptions;

namespace LfrlAnvil.Requests.Tests;

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

        result.TestEquals( expectedResult ).Go();
    }

    [Fact]
    public void Dispatch_ForRequestClass_ShouldThrowMissingRequestHandlerException_WhenHandlerDoesNotExist()
    {
        var factory = Substitute.For<IRequestHandlerFactory>();
        factory.TryCreate<TestRequestClass, int>().Returns( _ => null );
        var sut = new RequestDispatcher( factory );

        var action = Lambda.Of( () => sut.Dispatch( new TestRequestClass() ) );

        action.Test( exc => exc.TestType().Exact<MissingRequestHandlerException>() ).Go();
    }

    [Fact]
    public void Dispatch_ForRequestClass_ShouldThrowInvalidRequestTypeException_WhenRequestTypeIsInvalid()
    {
        var factory = Substitute.For<IRequestHandlerFactory>();
        var sut = new RequestDispatcher( factory );

        var action = Lambda.Of( () => sut.Dispatch( new InvalidTestRequestClass() ) );

        action.Test( exc => exc.TestType().Exact<InvalidRequestTypeException>() ).Go();
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

        result.TestEquals( expectedResult ).Go();
    }

    [Fact]
    public void Dispatch_ForRequestStruct_ShouldThrowMissingRequestHandlerException_WhenHandlerDoesNotExist()
    {
        var factory = Substitute.For<IRequestHandlerFactory>();
        factory.TryCreate<TestRequestStruct, int>().Returns( _ => null );
        var sut = new RequestDispatcher( factory );

        var action = Lambda.Of( () => sut.Dispatch<TestRequestStruct, int>( new TestRequestStruct() ) );
        action.Test( exc => exc.TestType().Exact<MissingRequestHandlerException>() ).Go();
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

        Assertion.All(
                result.TestTrue(),
                outResult.TestEquals( expectedResult ) )
            .Go();
    }

    [Fact]
    public void TryDispatch_ForRequestClass_ShouldReturnFalse_WhenHandlerDoesNotExist()
    {
        var factory = Substitute.For<IRequestHandlerFactory>();
        factory.TryCreate<TestRequestClass, int>().Returns( _ => null );
        var sut = new RequestDispatcher( factory );

        var result = sut.TryDispatch( new TestRequestClass(), out var outResult );

        Assertion.All(
                result.TestFalse(),
                outResult.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void TryDispatch_ForRequestClass_ShouldThrowInvalidRequestTypeException_WhenRequestTypeIsInvalid()
    {
        var factory = Substitute.For<IRequestHandlerFactory>();
        var sut = new RequestDispatcher( factory );

        var action = Lambda.Of( () => sut.TryDispatch( new InvalidTestRequestClass(), out _ ) );

        action.Test( exc => exc.TestType().Exact<InvalidRequestTypeException>() ).Go();
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

        Assertion.All(
                result.TestTrue(),
                outResult.TestEquals( expectedResult ) )
            .Go();
    }

    [Fact]
    public void TryDispatch_ForRequestStruct_ShouldReturnFalse_WhenHandlerDoesNotExist()
    {
        var factory = Substitute.For<IRequestHandlerFactory>();
        factory.TryCreate<TestRequestStruct, int>().Returns( _ => null );
        var sut = new RequestDispatcher( factory );

        var result = sut.TryDispatch( new TestRequestStruct(), out int outResult );

        Assertion.All(
                result.TestFalse(),
                outResult.TestEquals( default ) )
            .Go();
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

        result.TestEquals( expectedResult ).Go();
    }

    [Fact]
    public async Task Dispatch_ForAsyncValueTaskRequestClass_ShouldCallRequestHandler_WhenHandlerExists()
    {
        var request = new TestAsyncValueTaskRequestClass();
        var expectedResult = Fixture.Create<int>();
        var handler = Substitute.For<IRequestHandler<TestAsyncValueTaskRequestClass, ValueTask<int>>>();
        handler.Handle( request ).Returns( _ => expectedResult );

        var factory = Substitute.For<IRequestHandlerFactory>();
        factory.TryCreate<TestAsyncValueTaskRequestClass, ValueTask<int>>().Returns( _ => handler );

        var sut = new RequestDispatcher( factory );

        var result = await sut.Dispatch( request );

        result.TestEquals( expectedResult ).Go();
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

        result.TestEquals( expectedResult ).Go();
    }

    [Fact]
    public async Task Dispatch_ForAsyncValueTaskRequestStruct_ShouldCallRequestHandler_WhenHandlerExists()
    {
        var request = new TestAsyncValueTaskRequestStruct();
        var expectedResult = Fixture.Create<int>();
        var handler = Substitute.For<IRequestHandler<TestAsyncValueTaskRequestStruct, ValueTask<int>>>();
        handler.Handle( request ).Returns( _ => expectedResult );

        var factory = Substitute.For<IRequestHandlerFactory>();
        factory.TryCreate<TestAsyncValueTaskRequestStruct, ValueTask<int>>().Returns( _ => handler );

        var sut = new RequestDispatcher( factory );

        var result = await sut.Dispatch<TestAsyncValueTaskRequestStruct, ValueTask<int>>( request );

        result.TestEquals( expectedResult ).Go();
    }
}
