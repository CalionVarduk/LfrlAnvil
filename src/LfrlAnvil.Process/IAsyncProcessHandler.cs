using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Process
{
    public interface IAsyncProcessHandler<in TArgs, TResult>
        where TArgs : IProcessArgs<TResult>
    {
        ValueTask<TResult> Handle(TArgs args, CancellationToken cancellationToken);
    }
}
