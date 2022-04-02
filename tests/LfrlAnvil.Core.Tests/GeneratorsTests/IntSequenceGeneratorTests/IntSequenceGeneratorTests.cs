using LfrlAnvil.Generators;

namespace LfrlAnvil.Tests.GeneratorsTests.IntSequenceGeneratorTests
{
    public class IntSequenceGeneratorTests : GenericSequenceGeneratorOfSignedTypeTestsBase<int>
    {
        protected sealed override Bounds<int> GetDefaultBounds()
        {
            return new Bounds<int>( int.MinValue, int.MaxValue );
        }

        protected sealed override int GetDefaultStep()
        {
            return 1;
        }

        protected override int Negate(int a)
        {
            return -a;
        }

        protected sealed override int Add(int a, int b)
        {
            return a + b;
        }

        protected sealed override SequenceGeneratorBase<int> Create()
        {
            return new IntSequenceGenerator();
        }

        protected sealed override SequenceGeneratorBase<int> Create(int start)
        {
            return new IntSequenceGenerator( start );
        }

        protected sealed override SequenceGeneratorBase<int> Create(int start, int step)
        {
            return new IntSequenceGenerator( start, step );
        }

        protected sealed override SequenceGeneratorBase<int> Create(Bounds<int> bounds)
        {
            return new IntSequenceGenerator( bounds );
        }

        protected sealed override SequenceGeneratorBase<int> Create(Bounds<int> bounds, int start)
        {
            return new IntSequenceGenerator( bounds, start );
        }

        protected sealed override SequenceGeneratorBase<int> Create(Bounds<int> bounds, int start, int step)
        {
            return new IntSequenceGenerator( bounds, start, step );
        }
    }
}
