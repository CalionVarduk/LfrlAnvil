using System;
using FluentAssertions;
using LfrlSoft.NET.Core.Functional;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Functional.Maybe
{
    public abstract class GenericMaybeOfRefTypeTests<T> : GenericMaybeTests<T>
        where T : class
    {
        [Fact]
        public void Some_ShouldThrow_WhenParameterIsNull()
        {
            Action action = () =>
            {
                var _ = Core.Functional.Maybe.Some<T>( null );
            };

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void MaybeConversionOperator_FromT_ShouldReturnNone_WhenParameterIsNull()
        {
            var sut = (Maybe<T>)null;

            sut.HasValue.Should().BeFalse();
        }
    }
}
