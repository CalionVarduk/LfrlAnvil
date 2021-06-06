using System;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace LfrlSoft.NET.Common.Tests.Bounds
{
    public abstract class BoundsTestsRef<T> : BoundsTests<T>
        where T : class, IComparable<T>
    {
        [Fact]
        public void Ctor_ShouldThrow_WhenMinIsNull()
        {
            var max = Fixture.Create<T>();

            Action action = () => new Bounds<T>( null!, max );

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Ctor_ShouldThrow_WhenMaxIsNull()
        {
            var min = Fixture.Create<T>();

            Action action = () => new Bounds<T>( min, null! );

            action.Should().Throw<ArgumentNullException>();
        }
    }
}
