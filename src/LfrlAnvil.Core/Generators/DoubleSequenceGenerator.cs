using System;

namespace LfrlAnvil.Generators
{
    public class DoubleSequenceGenerator : SequenceGeneratorBase<double>
    {
        public DoubleSequenceGenerator()
            : this( start: 0 ) { }

        public DoubleSequenceGenerator(double start)
            : this( start, step: 1 ) { }

        public DoubleSequenceGenerator(double start, double step)
            : this( new Bounds<double>( double.MinValue, double.MaxValue ), start, step ) { }

        public DoubleSequenceGenerator(Bounds<double> bounds)
            : this( bounds, start: bounds.Min ) { }

        public DoubleSequenceGenerator(Bounds<double> bounds, double start)
            : this( bounds, start, step: 1 ) { }

        public DoubleSequenceGenerator(Bounds<double> bounds, double start, double step)
            : base( bounds, start, step )
        {
            Ensure.False( double.IsNaN( bounds.Min ), nameof( bounds ) + "." + nameof( bounds.Min ) + " cannot be NaN" );
            Ensure.NotEquals( bounds.Min, double.NegativeInfinity, nameof( bounds ) + "." + nameof( bounds.Min ) );
            Ensure.NotEquals( bounds.Min, double.PositiveInfinity, nameof( bounds ) + "." + nameof( bounds.Min ) );
            Ensure.False( double.IsNaN( bounds.Max ), nameof( bounds ) + "." + nameof( bounds.Max ) + " cannot be NaN" );
            Ensure.NotEquals( bounds.Max, double.NegativeInfinity, nameof( bounds ) + "." + nameof( bounds.Max ) );
            Ensure.NotEquals( bounds.Max, double.PositiveInfinity, nameof( bounds ) + "." + nameof( bounds.Max ) );
            Ensure.False( double.IsNaN( step ), nameof( step ) + " cannot be NaN" );
            Ensure.NotEquals( step, double.NegativeInfinity, nameof( step ) );
            Ensure.NotEquals( step, double.PositiveInfinity, nameof( step ) );
            Ensure.NotEquals( step, 0, nameof( step ) );
        }

        protected sealed override double AddStep(double value)
        {
            var result = value + Step;
            if ( result.Equals( value ) )
                throw new OverflowException();

            return result;
        }
    }
}
