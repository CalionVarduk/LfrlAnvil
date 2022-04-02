using System;
using AutoFixture;
using FluentAssertions;
using LfrlAnvil.Functional;
using LfrlAnvil.Generators;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Tests.GeneratorsTests.FloatSequenceGeneratorTests
{
    public class FloatSequenceGeneratorTests : GenericSequenceGeneratorOfSignedTypeTestsBase<float>
    {
        [Theory]
        [InlineData( float.NaN )]
        [InlineData( float.NegativeInfinity )]
        [InlineData( float.PositiveInfinity )]
        public void Ctor_WithStart_ShouldThrowArgumentOutOfRangeException_WhenStartIsNotFinite(float start)
        {
            var action = Lambda.Of( () => Create( start ) );
            action.Should().ThrowExactly<ArgumentOutOfRangeException>();
        }

        [Theory]
        [InlineData( float.NaN )]
        [InlineData( float.NegativeInfinity )]
        [InlineData( float.PositiveInfinity )]
        public void Ctor_WithStartAndStep_ShouldThrowArgumentOutOfRangeException_WhenStartIsNotFinite(float start)
        {
            var action = Lambda.Of( () => Create( start ) );
            action.Should().ThrowExactly<ArgumentOutOfRangeException>();
        }

        [Theory]
        [InlineData( float.NaN )]
        [InlineData( float.NegativeInfinity )]
        [InlineData( float.PositiveInfinity )]
        public void Ctor_WithStartAndStep_ShouldThrowArgumentException_WhenStepIsNotFinite(float step)
        {
            var start = Fixture.Create<float>();
            var action = Lambda.Of( () => Create( start, step ) );
            action.Should().ThrowExactly<ArgumentException>();
        }

        [Theory]
        [InlineData( float.NaN )]
        [InlineData( float.NegativeInfinity )]
        [InlineData( float.PositiveInfinity )]
        public void Ctor_WithBounds_ShouldThrowArgumentException_WhenMinIsNotFinite(float min)
        {
            var max = Fixture.Create<float>();
            var action = Lambda.Of( () => Create( Bounds.Create( min, max ) ) );
            action.Should().ThrowExactly<ArgumentException>();
        }

        [Theory]
        [InlineData( float.NaN )]
        [InlineData( float.NegativeInfinity )]
        [InlineData( float.PositiveInfinity )]
        public void Ctor_WithBounds_ShouldThrowArgumentException_WhenMaxIsNotFinite(float max)
        {
            var min = Fixture.Create<float>();
            var action = Lambda.Of( () => Create( Bounds.Create( min, max ) ) );
            action.Should().ThrowExactly<ArgumentException>();
        }

        [Theory]
        [InlineData( float.NaN )]
        [InlineData( float.NegativeInfinity )]
        [InlineData( float.PositiveInfinity )]
        public void Ctor_WithBoundsAndStart_ShouldThrowArgumentException_WhenMinIsNotFinite(float min)
        {
            var (start, max) = Fixture.CreateDistinctSortedCollection<float>( 2 );
            var action = Lambda.Of( () => Create( Bounds.Create( min, max ), start ) );
            action.Should().ThrowExactly<ArgumentException>();
        }

        [Theory]
        [InlineData( float.NaN )]
        [InlineData( float.NegativeInfinity )]
        [InlineData( float.PositiveInfinity )]
        public void Ctor_WithBoundsAndStart_ShouldThrowArgumentException_WhenMaxIsNotFinite(float max)
        {
            var (min, start) = Fixture.CreateDistinctSortedCollection<float>( 2 );
            var action = Lambda.Of( () => Create( Bounds.Create( min, max ), start ) );
            action.Should().ThrowExactly<ArgumentException>();
        }

        [Theory]
        [InlineData( float.NaN )]
        [InlineData( float.NegativeInfinity )]
        [InlineData( float.PositiveInfinity )]
        public void Ctor_WithBoundsAndStart_ShouldThrowArgumentOutOfRangeException_WhenStartIsNotFinite(float start)
        {
            var (min, max) = Fixture.CreateDistinctSortedCollection<float>( 2 );
            var action = Lambda.Of( () => Create( Bounds.Create( min, max ), start ) );
            action.Should().ThrowExactly<ArgumentOutOfRangeException>();
        }

        [Theory]
        [InlineData( float.NaN )]
        [InlineData( float.NegativeInfinity )]
        [InlineData( float.PositiveInfinity )]
        public void Ctor_WithBoundsAndStartAndStep_ShouldThrowArgumentException_WhenMinIsNotFinite(float min)
        {
            var (start, max) = Fixture.CreateDistinctSortedCollection<float>( 2 );
            var step = GetDefaultStep();

            var action = Lambda.Of( () => Create( Bounds.Create( min, max ), start, step ) );

            action.Should().ThrowExactly<ArgumentException>();
        }

        [Theory]
        [InlineData( float.NaN )]
        [InlineData( float.NegativeInfinity )]
        [InlineData( float.PositiveInfinity )]
        public void Ctor_WithBoundsAndStartAndStep_ShouldThrowArgumentException_WhenMaxIsNotFinite(float max)
        {
            var (min, start) = Fixture.CreateDistinctSortedCollection<float>( 2 );
            var step = GetDefaultStep();

            var action = Lambda.Of( () => Create( Bounds.Create( min, max ), start, step ) );

            action.Should().ThrowExactly<ArgumentException>();
        }

        [Theory]
        [InlineData( float.NaN )]
        [InlineData( float.NegativeInfinity )]
        [InlineData( float.PositiveInfinity )]
        public void Ctor_WithBoundsAndStartAndStep_ShouldThrowArgumentOutOfRangeException_WhenStartIsNotFinite(float start)
        {
            var (min, max) = Fixture.CreateDistinctSortedCollection<float>( 2 );
            var step = GetDefaultStep();

            var action = Lambda.Of( () => Create( Bounds.Create( min, max ), start, step ) );

            action.Should().ThrowExactly<ArgumentOutOfRangeException>();
        }

        [Theory]
        [InlineData( float.NaN )]
        [InlineData( float.NegativeInfinity )]
        [InlineData( float.PositiveInfinity )]
        public void Ctor_WithBoundsAndStartAndStep_ShouldThrowArgumentException_WhenStepIsNotFinite(float step)
        {
            var (min, start, max) = Fixture.CreateDistinctSortedCollection<float>( 3 );
            var action = Lambda.Of( () => Create( Bounds.Create( min, max ), start, step ) );
            action.Should().ThrowExactly<ArgumentException>();
        }

        protected sealed override Bounds<float> GetDefaultBounds()
        {
            return new Bounds<float>( float.MinValue, float.MaxValue );
        }

        protected sealed override float GetDefaultStep()
        {
            return 1;
        }

        protected override float Negate(float a)
        {
            return -a;
        }

        protected sealed override float Add(float a, float b)
        {
            return a + b;
        }

        protected sealed override SequenceGeneratorBase<float> Create()
        {
            return new FloatSequenceGenerator();
        }

        protected sealed override SequenceGeneratorBase<float> Create(float start)
        {
            return new FloatSequenceGenerator( start );
        }

        protected sealed override SequenceGeneratorBase<float> Create(float start, float step)
        {
            return new FloatSequenceGenerator( start, step );
        }

        protected sealed override SequenceGeneratorBase<float> Create(Bounds<float> bounds)
        {
            return new FloatSequenceGenerator( bounds );
        }

        protected sealed override SequenceGeneratorBase<float> Create(Bounds<float> bounds, float start)
        {
            return new FloatSequenceGenerator( bounds, start );
        }

        protected sealed override SequenceGeneratorBase<float> Create(Bounds<float> bounds, float start, float step)
        {
            return new FloatSequenceGenerator( bounds, start, step );
        }
    }
}
