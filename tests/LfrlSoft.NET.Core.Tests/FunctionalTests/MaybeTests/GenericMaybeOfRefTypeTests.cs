using System;
using FluentAssertions;
using LfrlSoft.NET.Core.Functional;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.FunctionalTests.MaybeTests
{
    public abstract class GenericMaybeOfRefTypeTests<T> : GenericMaybeTests<T>
        where T : class
    {
        [Fact]
        public void Some_ShouldThrowArgumentNullException_WhenParameterIsNull()
        {
            var action = Lambda.Of( () => Maybe.Some<T>( null ) );
            action.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public void MaybeConversionOperator_FromT_ShouldReturnNone_WhenParameterIsNull()
        {
            var sut = (Maybe<T>)null;
            sut.HasValue.Should().BeFalse();
        }
    }
}
