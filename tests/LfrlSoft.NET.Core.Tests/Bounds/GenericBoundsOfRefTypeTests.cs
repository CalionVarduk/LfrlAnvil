using System;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Bounds
{
    public abstract class GenericBoundsOfRefTypeTests<T> : GenericBoundsTests<T>
        where T : class, IComparable<T>
    {
        [Fact]
        public void Ctor_ShouldThrow_WhenMinIsNull()
        {
            var max = Fixture.Create<T>();

            Action action = () =>
            {
                var _ = new Bounds<T>( null!, max );
            };

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Ctor_ShouldThrow_WhenMaxIsNull()
        {
            var min = Fixture.Create<T>();

            Action action = () =>
            {
                var _ = new Bounds<T>( min, null! );
            };

            action.Should().Throw<ArgumentNullException>();
        }
    }
}
