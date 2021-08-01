using Xunit;

namespace LfrlSoft.NET.Core.Tests.Assert
{
    public class AssertTypelessTests : AssertTestsBase
    {
        [Fact]
        public void True_ShouldPass_WhenConditionIsTrue()
        {
            ShouldPass( () => Core.Assert.True( true ) );
        }

        [Fact]
        public void True_ShouldThrow_WhenConditionIsFalse()
        {
            ShouldThrow( () => Core.Assert.True( false ) );
        }

        [Fact]
        public void True_ShouldPass_WhenConditionIsTrue_WithDelegate()
        {
            ShouldPass( () => Core.Assert.True( true, () => string.Empty ) );
        }

        [Fact]
        public void True_ShouldThrow_WhenConditionIsFalse_WithDelegate()
        {
            ShouldThrow( () => Core.Assert.True( false, () => string.Empty ) );
        }

        [Fact]
        public void False_ShouldPass_WhenConditionIsFalse()
        {
            ShouldPass( () => Core.Assert.False( false ) );
        }

        [Fact]
        public void False_ShouldThrow_WhenConditionIsTrue()
        {
            ShouldThrow( () => Core.Assert.False( true ) );
        }

        [Fact]
        public void False_ShouldPass_WhenConditionIsFalse_WithDelegate()
        {
            ShouldPass( () => Core.Assert.False( false, () => string.Empty ) );
        }

        [Fact]
        public void False_ShouldThrow_WhenConditionIsTrue_WithDelegate()
        {
            ShouldThrow( () => Core.Assert.False( true, () => string.Empty ) );
        }
    }
}
