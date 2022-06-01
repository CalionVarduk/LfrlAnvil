namespace LfrlAnvil.Reactive.Events.Composites
{
    public readonly struct WithSender<TEvent>
    {
        public WithSender(object? sender, TEvent @event)
        {
            Sender = sender;
            Event = @event;
        }

        public object? Sender { get; }
        public TEvent Event { get; }

        public override string ToString()
        {
            return $"{Sender} => {Event}";
        }
    }
}
