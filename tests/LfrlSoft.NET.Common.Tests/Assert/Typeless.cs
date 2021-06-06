using Xunit;

namespace LfrlSoft.NET.Common.Tests.Assert
{
    public class Typeless : AssertTestsBase
    {
        [Fact]
        public void True_ShouldPass_WhenConditionIsTrue()
        {
            ShouldPass( () => Common.Assert.True( true ) );
        }

        [Fact]
        public void True_ShouldThrow_WhenConditionIsFalse()
        {
            ShouldThrow( () => Common.Assert.True( false ) );
        }

        [Fact]
        public void True_ShouldPass_WhenConditionIsTrue_WithDelegate()
        {
            ShouldPass( () => Common.Assert.True( true, () => string.Empty ) );
        }

        [Fact]
        public void True_ShouldThrow_WhenConditionIsFalse_WithDelegate()
        {
            ShouldThrow( () => Common.Assert.True( false, () => string.Empty ) );
        }

        [Fact]
        public void False_ShouldPass_WhenConditionIsFalse()
        {
            ShouldPass( () => Common.Assert.False( false ) );
        }

        [Fact]
        public void False_ShouldThrow_WhenConditionIsTrue()
        {
            ShouldThrow( () => Common.Assert.False( true ) );
        }

        [Fact]
        public void False_ShouldPass_WhenConditionIsFalse_WithDelegate()
        {
            ShouldPass( () => Common.Assert.False( false, () => string.Empty ) );
        }

        [Fact]
        public void False_ShouldThrow_WhenConditionIsTrue_WithDelegate()
        {
            ShouldThrow( () => Common.Assert.False( true, () => string.Empty ) );
        }
    }
}
