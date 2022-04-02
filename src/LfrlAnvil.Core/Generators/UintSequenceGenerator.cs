namespace LfrlAnvil.Generators
{
    public class UintSequenceGenerator : SequenceGeneratorBase<uint>
    {
        public UintSequenceGenerator()
            : this( start: 0 ) { }

        public UintSequenceGenerator(uint start)
            : this( start, step: 1 ) { }

        public UintSequenceGenerator(uint start, uint step)
            : this( new Bounds<uint>( uint.MinValue, uint.MaxValue ), start, step ) { }

        public UintSequenceGenerator(Bounds<uint> bounds)
            : this( bounds, start: bounds.Min ) { }

        public UintSequenceGenerator(Bounds<uint> bounds, uint start)
            : this( bounds, start, step: 1 ) { }

        public UintSequenceGenerator(Bounds<uint> bounds, uint start, uint step)
            : base( bounds, start, step )
        {
            Ensure.NotEquals( step, 0U, nameof( step ) );
        }

        protected sealed override uint AddStep(uint value)
        {
            return checked(value + Step);
        }
    }
}
