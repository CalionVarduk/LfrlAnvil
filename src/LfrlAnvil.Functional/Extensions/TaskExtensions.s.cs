using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace LfrlAnvil.Functional.Extensions
{
    public static class TaskExtensions
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static async Task<Nil> ToNil(this Task task)
        {
            await task;
            return Nil.Instance;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static async ValueTask<Nil> ToNil(this ValueTask task)
        {
            await task;
            return Nil.Instance;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static async Task IgnoreResult<T>(this Task<T> task)
        {
            await task;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static async ValueTask IgnoreResult<T>(this ValueTask<T> task)
        {
            await task;
        }
    }
}
