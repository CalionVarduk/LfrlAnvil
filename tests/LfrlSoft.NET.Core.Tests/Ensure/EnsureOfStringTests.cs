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
        public void IsEmpty_ShouldThrow_WhenStringIsNotEmpty()
        {
            var param = Fixture.CreateNotDefault<string>();
            ShouldThrow( () => Core.Ensure.IsEmpty( param ) );
        }

        [Fact]
        public void IsNotEmpty_ShouldPass_WhenStringIsNotEmpty()
        {
            var param = Fixture.CreateNotDefault<string>();
            ShouldPass( () => Core.Ensure.IsNotEmpty( param ) );
        }

        [Fact]
        public void IsNotEmpty_ShouldThrow_WhenStringIsEmpty()
        {
            var param = string.Empty;
            ShouldThrow( () => Core.Ensure.IsNotEmpty( param ) );
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
        public void IsNullOrEmpty_ShouldThrow_WhenStringIsNotEmpty()
        {
            var param = Fixture.CreateNotDefault<string>();
            ShouldThrow( () => Core.Ensure.IsNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldPass_WhenStringIsNotEmpty()
        {
            var param = Fixture.CreateNotDefault<string>();
            ShouldPass( () => Core.Ensure.IsNotNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldThrow_WhenStringIsNull()
        {
            var param = Fixture.CreateDefault<string>();
            ShouldThrow( () => Core.Ensure.IsNotNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldThrow_WhenStringIsEmpty()
        {
            var param = string.Empty;
            ShouldThrow( () => Core.Ensure.IsNotNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrWhiteSpace_ShouldPass_WhenStringIsNotNullOrWhiteSpace()
        {
            var param = Fixture.CreateNotDefault<string>();
            ShouldPass( () => Core.Ensure.IsNotNullOrWhiteSpace( param ) );
        }

        [Fact]
        public void IsNotNullOrWhiteSpace_ShouldThrow_WhenStringIsNull()
        {
            var param = Fixture.CreateDefault<string>();
            ShouldThrow( () => Core.Ensure.IsNotNullOrWhiteSpace( param ) );
        }

        [Fact]
        public void IsNotNullOrWhiteSpace_ShouldThrow_WhenStringIsEmpty()
        {
            var param = string.Empty;
            ShouldThrow( () => Core.Ensure.IsNotNullOrWhiteSpace( param ) );
        }

        [Fact]
        public void IsNotNullOrWhiteSpace_ShouldThrow_WhenStringIsWhiteSpaceOnly()
        {
            var param = " \t\n\r";
            ShouldThrow( () => Core.Ensure.IsNotNullOrWhiteSpace( param ) );
        }
    }
}
