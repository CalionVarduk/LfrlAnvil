using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Ensure
{
    public class EnsureOfStringTests : GenericEnsureOfRefTypeTests<string>
    {
        [Fact]
        public void IsEmpty_ShouldPass_WhenStringIsEmpty()
        {
            var param = string.Empty;
            ShouldPass( () => Core.Ensure.IsEmpty( param ) );
        }

        [Fact]
        public void IsEmpty_ShouldThrowArgumentException_WhenStringIsNotEmpty()
        {
            var param = Fixture.CreateNotDefault<string>();
            ShouldThrowArgumentException( () => Core.Ensure.IsEmpty( param ) );
        }

        [Fact]
        public void IsNotEmpty_ShouldPass_WhenStringIsNotEmpty()
        {
            var param = Fixture.CreateNotDefault<string>();
            ShouldPass( () => Core.Ensure.IsNotEmpty( param ) );
        }

        [Fact]
        public void IsNotEmpty_ShouldThrowArgumentException_WhenStringIsEmpty()
        {
            var param = string.Empty;
            ShouldThrowArgumentException( () => Core.Ensure.IsNotEmpty( param ) );
        }

        [Fact]
        public void IsNullOrEmpty_ShouldPass_WhenStringIsNull()
        {
            var param = Fixture.CreateDefault<string>();
            ShouldPass( () => Core.Ensure.IsNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNullOrEmpty_ShouldPass_WhenStringIsEmpty()
        {
            var param = string.Empty;
            ShouldPass( () => Core.Ensure.IsNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNullOrEmpty_ShouldThrowArgumentException_WhenStringIsNotEmpty()
        {
            var param = Fixture.CreateNotDefault<string>();
            ShouldThrowArgumentException( () => Core.Ensure.IsNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldPass_WhenStringIsNotEmpty()
        {
            var param = Fixture.CreateNotDefault<string>();
            ShouldPass( () => Core.Ensure.IsNotNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldThrowArgumentException_WhenStringIsNull()
        {
            var param = Fixture.CreateDefault<string>();
            ShouldThrowArgumentException( () => Core.Ensure.IsNotNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldThrowArgumentException_WhenStringIsEmpty()
        {
            var param = string.Empty;
            ShouldThrowArgumentException( () => Core.Ensure.IsNotNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrWhiteSpace_ShouldPass_WhenStringIsNotNullOrWhiteSpace()
        {
            var param = Fixture.CreateNotDefault<string>();
            ShouldPass( () => Core.Ensure.IsNotNullOrWhiteSpace( param ) );
        }

        [Fact]
        public void IsNotNullOrWhiteSpace_ShouldThrowArgumentException_WhenStringIsNull()
        {
            var param = Fixture.CreateDefault<string>();
            ShouldThrowArgumentException( () => Core.Ensure.IsNotNullOrWhiteSpace( param ) );
        }

        [Fact]
        public void IsNotNullOrWhiteSpace_ShouldThrowArgumentException_WhenStringIsEmpty()
        {
            var param = string.Empty;
            ShouldThrowArgumentException( () => Core.Ensure.IsNotNullOrWhiteSpace( param ) );
        }

        [Fact]
        public void IsNotNullOrWhiteSpace_ShouldThrowArgumentException_WhenStringIsWhiteSpaceOnly()
        {
            var param = " \t\n\r";
            ShouldThrowArgumentException( () => Core.Ensure.IsNotNullOrWhiteSpace( param ) );
        }
    }
}
