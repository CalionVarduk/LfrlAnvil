using LfrlAnvil.Reactive.Exceptions;

namespace LfrlAnvil.Reactive.Events
{
    public sealed class LazyEventSubscriber : IEventSubscriber
    {
        private bool _isDisposedPrematurely;

        public IEventSubscriber? Subscriber { get; private set; }
        public bool IsDisposed => Subscriber?.IsDisposed == true || _isDisposedPrematurely;

        public void Dispose()
        {
            if ( Subscriber is null )
            {
                _isDisposedPrematurely = true;
                return;
            }

            Subscriber.Dispose();
        }

        public void Initialize(IEventSubscriber subscriber)
        {
            if ( Subscriber is not null )
                throw new SubscriberInitializationException();

            Subscriber = subscriber;
            if ( _isDisposedPrematurely )
                Subscriber.Dispose();
        }
    }
}
