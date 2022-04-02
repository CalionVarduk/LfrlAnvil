namespace LfrlAnvil.Generators
{
    public class SbyteSequenceGenerator : SequenceGeneratorBase<sbyte>
    {
        public SbyteSequenceGenerator()
            : this( start: 0 ) { }

        public SbyteSequenceGenerator(sbyte start)
            : this( start, step: 1 ) { }

        public SbyteSequenceGenerator(sbyte start, sbyte step)
            : this( new Bounds<sbyte>( sbyte.MinValue, sbyte.MaxValue ), start, step ) { }

        public SbyteSequenceGenerator(Bounds<sbyte> bounds)
            : this( bounds, start: bounds.Min ) { }

        public SbyteSequenceGenerator(Bounds<sbyte> bounds, sbyte start)
            : this( bounds, start, step: 1 ) { }

        public SbyteSequenceGenerator(Bounds<sbyte> bounds, sbyte start, sbyte step)
            : base( bounds, start, step )
        {
            Ensure.NotEquals( step, 0, nameof( step ) );
        }

        protected sealed override sbyte AddStep(sbyte value)
        {
            return checked( (sbyte)(value + Step) );
        }
    }
}
