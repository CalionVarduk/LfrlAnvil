using LfrlAnvil.Generators;

namespace LfrlAnvil.Tests.GeneratorsTests.UintSequenceGeneratorTests
{
    public class UintSequenceGeneratorTests : GenericSequenceGeneratorTestsBase<uint>
    {
        protected sealed override Bounds<uint> GetDefaultBounds()
        {
            return new Bounds<uint>( uint.MinValue, uint.MaxValue );
        }

        protected sealed override uint GetDefaultStep()
        {
            return 1;
        }

        protected sealed override uint Add(uint a, uint b)
        {
            return a + b;
        }

        protected sealed override SequenceGeneratorBase<uint> Create()
        {
            return new UintSequenceGenerator();
        }

        protected sealed override SequenceGeneratorBase<uint> Create(uint start)
        {
            return new UintSequenceGenerator( start );
        }

        protected sealed override SequenceGeneratorBase<uint> Create(uint start, uint step)
        {
            return new UintSequenceGenerator( start, step );
        }

        protected sealed override SequenceGeneratorBase<uint> Create(Bounds<uint> bounds)
        {
            return new UintSequenceGenerator( bounds );
        }

        protected sealed override SequenceGeneratorBase<uint> Create(Bounds<uint> bounds, uint start)
        {
            return new UintSequenceGenerator( bounds, start );
        }

        protected sealed override SequenceGeneratorBase<uint> Create(Bounds<uint> bounds, uint start, uint step)
        {
            return new UintSequenceGenerator( bounds, start, step );
        }
    }
}
