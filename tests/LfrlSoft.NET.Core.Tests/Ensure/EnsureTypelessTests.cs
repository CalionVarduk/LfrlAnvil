using Xunit;

namespace LfrlSoft.NET.Core.Tests.Ensure
{
    public class EnsureTypelessTests : EnsureTestsBase
    {
        [Fact]
        public void True_ShouldPass_WhenConditionIsTrue()
        {
            ShouldPass( () => Core.Ensure.True( true ) );
        }

        [Fact]
        public void True_ShouldThrow_WhenConditionIsFalse()
        {
            ShouldThrow( () => Core.Ensure.True( false ) );
        }

        [Fact]
        public void True_ShouldPass_WhenConditionIsTrue_WithDelegate()
        {
            ShouldPass( () => Core.Ensure.True( true, () => string.Empty ) );
        }

        [Fact]
        public void True_ShouldThrow_WhenConditionIsFalse_WithDelegate()
        {
            ShouldThrow( () => Core.Ensure.True( false, () => string.Empty ) );
        }

        [Fact]
        public void False_ShouldPass_WhenConditionIsFalse()
        {
            ShouldPass( () => Core.Ensure.False( false ) );
        }

        [Fact]
        public void False_ShouldThrow_WhenConditionIsTrue()
        {
            ShouldThrow( () => Core.Ensure.False( true ) );
        }

        [Fact]
        public void False_ShouldPass_WhenConditionIsFalse_WithDelegate()
        {
            ShouldPass( () => Core.Ensure.False( false, () => string.Empty ) );
        }

        [Fact]
        public void False_ShouldThrow_WhenConditionIsTrue_WithDelegate()
        {
            ShouldThrow( () => Core.Ensure.False( true, () => string.Empty ) );
        }
    }
}
