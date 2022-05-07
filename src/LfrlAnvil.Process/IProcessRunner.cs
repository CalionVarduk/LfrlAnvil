using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Process
{
    public interface IProcessRunner
    {
        TResult Run<TArgs, TResult>(TArgs args)
            where TArgs : IProcessArgs<TResult>;

        bool TryRun<TArgs, TResult>(TArgs args, [MaybeNullWhen( false )] out TResult result)
            where TArgs : IProcessArgs<TResult>;

        ValueTask<TResult> RunAsync<TArgs, TResult>(TArgs args, CancellationToken cancellationToken)
            where TArgs : IProcessArgs<TResult>;

        ValueTask<TResult>? TryRunAsync<TArgs, TResult>(TArgs args, CancellationToken cancellationToken)
            where TArgs : IProcessArgs<TResult>;
    }
}
