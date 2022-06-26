using BenchmarkDotNet.Attributes;
using LfrlAnvil.Requests;

namespace LfrlAnvil.Benchmarks;

[MemoryDiagnoser]
public class RequestDispatcherBenchmark
{
    private static readonly ClassRequestHandler ClassRequestHandlerInstance = new ClassRequestHandler();
    private static readonly StructRequestHandler StructRequestHandlerInstance = new StructRequestHandler();
    private readonly ClassRequest _classRequest = new ClassRequest();
    private readonly StructRequest _structRequest = new StructRequest();

    private readonly RequestDispatcher _dispatcher = new RequestDispatcher(
        new RequestHandlerFactory()
            .Register( () => ClassRequestHandlerInstance )
            .Register( () => StructRequestHandlerInstance ) );

    [Benchmark]
    public int HandleClassRequest()
    {
        return _dispatcher.Dispatch( _classRequest );
    }

    [Benchmark]
    public int HandleStructRequest()
    {
        return _dispatcher.Dispatch<StructRequest, int>( _structRequest );
    }

    // [Benchmark]
    // public int HandleStructRequest_Boxing()
    // {
    //     return _dispatcher.Dispatch( _structRequest );
    // }

    private sealed class ClassRequest : IRequest<ClassRequest, int> { }

    private readonly struct StructRequest : IRequest<StructRequest, int> { }

    private sealed class ClassRequestHandler : IRequestHandler<ClassRequest, int>
    {
        public int Handle(ClassRequest request)
        {
            return 0;
        }
    }

    private sealed class StructRequestHandler : IRequestHandler<StructRequest, int>
    {
        public int Handle(StructRequest request)
        {
            return 0;
        }
    }
}
