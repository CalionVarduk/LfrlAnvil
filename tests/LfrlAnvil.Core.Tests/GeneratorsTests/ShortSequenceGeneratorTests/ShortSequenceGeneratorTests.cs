using LfrlAnvil.Generators;

namespace LfrlAnvil.Tests.GeneratorsTests.ShortSequenceGeneratorTests
{
    public class ShortSequenceGeneratorTests : GenericSequenceGeneratorOfSignedTypeTestsBase<short>
    {
        protected sealed override Bounds<short> GetDefaultBounds()
        {
            return new Bounds<short>( short.MinValue, short.MaxValue );
        }

        protected sealed override short GetDefaultStep()
        {
            return 1;
        }

        protected override short Negate(short a)
        {
            return (short)-a;
        }

        protected sealed override short Add(short a, short b)
        {
            return (short)(a + b);
        }

        protected sealed override SequenceGeneratorBase<short> Create()
        {
            return new ShortSequenceGenerator();
        }

        protected sealed override SequenceGeneratorBase<short> Create(short start)
        {
            return new ShortSequenceGenerator( start );
        }

        protected sealed override SequenceGeneratorBase<short> Create(short start, short step)
        {
            return new ShortSequenceGenerator( start, step );
        }

        protected sealed override SequenceGeneratorBase<short> Create(Bounds<short> bounds)
        {
            return new ShortSequenceGenerator( bounds );
        }

        protected sealed override SequenceGeneratorBase<short> Create(Bounds<short> bounds, short start)
        {
            return new ShortSequenceGenerator( bounds, start );
        }

        protected sealed override SequenceGeneratorBase<short> Create(Bounds<short> bounds, short start, short step)
        {
            return new ShortSequenceGenerator( bounds, start, step );
        }
    }
}
