([root](https://github.com/CalionVarduk/LfrlAnvil/blob/main/readme.md))
[![NuGet Badge](https://buildstats.info/nuget/LfrlAnvil.Requests)](https://www.nuget.org/packages/LfrlAnvil.Requests/)

# [LfrlAnvil.Requests](https://github.com/CalionVarduk/LfrlAnvil/tree/main/src/LfrlAnvil.Requests)

This project contains an intermediate request dispatcher, as well as a factory of request handlers.

### Documentation

Technical documentation can be found [here](https://calionvarduk.github.io/LfrlAnvil/api/LfrlAnvil.Requests/LfrlAnvil.Requests.html).

### Examples

Following is an example of how to define a request type, how to create a handler of such requests and how to dispatch a request:
```csharp
// a class that defines a request,
// with int as the type of the request's result
public class FooRequest : IRequest<FooRequest, int>
{
    // request members
}

// a class that represents a handler of requests of FooRequest type,
// with int as the type of the request's result
public class FooRequestHandler : IRequestHandler<FooRequest, int>
{
    // request handling implementation
    public int Handle(FooRequest request)
    {
        // implementation goes here
        return result;
    }
}

// creates a new empty factory of request handlers, identified by request type,
// with registered FooRequest handler
var handlerFactory = new RequestHandlerFactory()
    .Register( () => new FooRequestHandler() );

// creates a new request dispatcher that uses the above request handler factory instance
var dispatcher = new RequestDispatcher( handlerFactory );

// creates a new FooRequest instance
var request = new FooRequest { ... };

// dispatches the request and returns the result returned by its handler
var result = dispatcher.Dispatch( request );
```

It's also possible to work with asynchronous requests, like so:
```csharp
// a class that defines an asynchronous request,
// with int as the type of the request's result
public class AsyncFooRequest : IAsyncTaskRequest<AsyncFooRequest, int>
{
    // request's cancellation token
    public CancellationToken CancellationToken { get; init; }
    
    // other request members
}

// a class that represents a handler of requests of AsyncFooRequest type,
// with int as the type of the request's result
public class AsyncFooRequestHandler : IRequestHandler<AsyncFooRequest, Task<int>>
{
    // request handling implementation
    public Task<int> Handle(AsyncFooRequest request)
    {
        // implementation goes here
        return result;
    }
}

// creates a new empty factory of request handlers, identified by request type,
// with registered AsyncFooRequest handler
var handlerFactory = new RequestHandlerFactory()
    .Register( () => new AsyncFooRequestHandler() );

// creates a new request dispatcher that uses the above request handler factory instance
var dispatcher = new RequestDispatcher( handlerFactory );

// creates a new AsyncFooRequest instance
var cancellationTokenSource = new CancellationTokenSource();
var request = new AsyncFooRequest { CancellationToken = cancellationTokenSource.Token, ... };

// dispatches the request and returns the result returned by its handler
var result = await dispatcher.Dispatch( request );
```
