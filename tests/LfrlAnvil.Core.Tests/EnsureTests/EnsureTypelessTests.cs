using Xunit;

namespace LfrlAnvil.Tests.EnsureTests
{
    public class EnsureTypelessTests : EnsureTestsBase
    {
        [Fact]
        public void True_ShouldPass_WhenConditionIsTrue()
        {
            ShouldPass( () => Ensure.True( true ) );
        }

        [Fact]
        public void True_ShouldThrowArgumentException_WhenConditionIsFalse()
        {
            ShouldThrowArgumentException( () => Ensure.True( false ) );
        }

        [Fact]
        public void True_ShouldPass_WhenConditionIsTrue_WithDelegate()
        {
            ShouldPass( () => Ensure.True( true, () => string.Empty ) );
        }

        [Fact]
        public void True_ShouldThrowArgumentException_WhenConditionIsFalse_WithDelegate()
        {
            ShouldThrowArgumentException( () => Ensure.True( false, () => string.Empty ) );
        }

        [Fact]
        public void False_ShouldPass_WhenConditionIsFalse()
        {
            ShouldPass( () => Ensure.False( false ) );
        }

        [Fact]
        public void False_ShouldThrowArgumentException_WhenConditionIsTrue()
        {
            ShouldThrowArgumentException( () => Ensure.False( true ) );
        }

        [Fact]
        public void False_ShouldPass_WhenConditionIsFalse_WithDelegate()
        {
            ShouldPass( () => Ensure.False( false, () => string.Empty ) );
        }

        [Fact]
        public void False_ShouldThrowArgumentException_WhenConditionIsTrue_WithDelegate()
        {
            ShouldThrowArgumentException( () => Ensure.False( true, () => string.Empty ) );
        }
    }
}
