using System;
using System.Collections.Generic;
using LfrlAnvil.Process.Extensions;
using LfrlAnvil.Process.Internal;

namespace LfrlAnvil.Process
{
    public sealed class ProcessHandlerFactory : IProcessHandlerFactory
    {
        private readonly Dictionary<Type, ProcessHandlerStore> _stores;

        public ProcessHandlerFactory()
        {
            _stores = new Dictionary<Type, ProcessHandlerStore>();
        }

        public ProcessHandlerFactory Register<TArgs, TResult>(Func<IProcessHandler<TArgs, TResult>> factory)
            where TArgs : IProcessArgs<TResult>
        {
            _stores[typeof( TArgs )] = new ProcessHandlerStore( factory, isAsync: false );
            return this;
        }

        public ProcessHandlerFactory Register<TArgs, TResult>(Func<IAsyncProcessHandler<TArgs, TResult>> factory)
            where TArgs : IProcessArgs<TResult>
        {
            _stores[typeof( TArgs )] = new ProcessHandlerStore( factory, isAsync: true );
            return this;
        }

        public IProcessHandler<TArgs, TResult>? TryCreate<TArgs, TResult>()
            where TArgs : IProcessArgs<TResult>
        {
            if ( ! _stores.TryGetValue( typeof( TArgs ), out var store ) )
                return null;

            return store.IsAsync
                ? ((Func<IAsyncProcessHandler<TArgs, TResult>>)store.Delegate)().ToSynchronous()
                : ((Func<IProcessHandler<TArgs, TResult>>)store.Delegate)();
        }

        public IAsyncProcessHandler<TArgs, TResult>? TryCreateAsync<TArgs, TResult>()
            where TArgs : IProcessArgs<TResult>
        {
            if ( ! _stores.TryGetValue( typeof( TArgs ), out var store ) )
                return null;

            return store.IsAsync
                ? ((Func<IAsyncProcessHandler<TArgs, TResult>>)store.Delegate)()
                : ((Func<IProcessHandler<TArgs, TResult>>)store.Delegate)().ToAsynchronous();
        }
    }
}
