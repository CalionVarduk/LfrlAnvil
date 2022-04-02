namespace LfrlAnvil.Generators
{
    public class LongSequenceGenerator : SequenceGeneratorBase<long>
    {
        public LongSequenceGenerator()
            : this( start: 0 ) { }

        public LongSequenceGenerator(long start)
            : this( start, step: 1 ) { }

        public LongSequenceGenerator(long start, long step)
            : this( new Bounds<long>( long.MinValue, long.MaxValue ), start, step ) { }

        public LongSequenceGenerator(Bounds<long> bounds)
            : this( bounds, start: bounds.Min ) { }

        public LongSequenceGenerator(Bounds<long> bounds, long start)
            : this( bounds, start, step: 1 ) { }

        public LongSequenceGenerator(Bounds<long> bounds, long start, long step)
            : base( bounds, start, step )
        {
            Ensure.NotEquals( step, 0, nameof( step ) );
        }

        protected sealed override long AddStep(long value)
        {
            return checked(value + Step);
        }
    }
}
