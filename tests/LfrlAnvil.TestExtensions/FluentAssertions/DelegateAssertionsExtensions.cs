using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions.Specialized;
using NSubstitute;

namespace LfrlAnvil.TestExtensions.FluentAssertions
{
    public static class DelegateAssertionsExtensions
    {
        public static Task AcquireLockOn(this DelegateAssertions<Action> source, object sync)
        {
            return source.AcquireLockOn( sync, TimeSpan.FromMilliseconds( 15 ) );
        }

        public static async Task AcquireLockOn(this DelegateAssertions<Action> source, object sync, TimeSpan lockTimeout)
        {
            var lockEnd = Substitute.For<Action>();
            var actionEnd = Substitute.For<Action>();

            var lockReset = new ManualResetEventSlim( false );
            var actionReset = new ManualResetEventSlim( false );
            var testCompletion = new TaskCompletionSource<int>();

#pragma warning disable CS4014
            Task.Run(
                () =>
                {
                    actionReset.Wait();
                    lockReset.Set();
                    source.Subject();
                    actionEnd();

                    testCompletion.SetResult( 0 );
                } );
#pragma warning restore CS4014

            lock ( sync )
            {
                actionReset.Set();
                lockReset.Wait();
                Thread.Sleep( lockTimeout );
                lockEnd();
            }

            await testCompletion.Task;

            Verify.CallOrder(
                () =>
                {
                    lockEnd();
                    actionEnd();
                } );
        }
    }
}
