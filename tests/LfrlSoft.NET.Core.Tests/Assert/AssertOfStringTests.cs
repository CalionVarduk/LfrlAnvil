using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Assert
{
    public class AssertOfStringTests : GenericAssertOfRefTypeTests<string>
    {
        [Fact]
        public void IsEmpty_ShouldPass_WhenStringIsEmpty()
        {
            var param = string.Empty;
            ShouldPass( () => Core.Assert.IsEmpty( param ) );
        }

        [Fact]
        public void IsEmpty_ShouldThrow_WhenStringIsNotEmpty()
        {
            var param = Fixture.CreateNotDefault<string>();
            ShouldThrow( () => Core.Assert.IsEmpty( param ) );
        }

        [Fact]
        public void IsNotEmpty_ShouldPass_WhenStringIsNotEmpty()
        {
            var param = Fixture.CreateNotDefault<string>();
            ShouldPass( () => Core.Assert.IsNotEmpty( param ) );
        }

        [Fact]
        public void IsNotEmpty_ShouldThrow_WhenStringIsEmpty()
        {
            var param = string.Empty;
            ShouldThrow( () => Core.Assert.IsNotEmpty( param ) );
        }

        [Fact]
        public void IsNullOrEmpty_ShouldPass_WhenStringIsNull()
        {
            var param = Fixture.CreateDefault<string>();
            ShouldPass( () => Core.Assert.IsNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNullOrEmpty_ShouldPass_WhenStringIsEmpty()
        {
            var param = string.Empty;
            ShouldPass( () => Core.Assert.IsNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNullOrEmpty_ShouldThrow_WhenStringIsNotEmpty()
        {
            var param = Fixture.CreateNotDefault<string>();
            ShouldThrow( () => Core.Assert.IsNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldPass_WhenStringIsNotEmpty()
        {
            var param = Fixture.CreateNotDefault<string>();
            ShouldPass( () => Core.Assert.IsNotNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldThrow_WhenStringIsNull()
        {
            var param = Fixture.CreateDefault<string>();
            ShouldThrow( () => Core.Assert.IsNotNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldThrow_WhenStringIsEmpty()
        {
            var param = string.Empty;
            ShouldThrow( () => Core.Assert.IsNotNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrWhiteSpace_ShouldPass_WhenStringIsNotNullOrWhiteSpace()
        {
            var param = Fixture.CreateNotDefault<string>();
            ShouldPass( () => Core.Assert.IsNotNullOrWhiteSpace( param ) );
        }

        [Fact]
        public void IsNotNullOrWhiteSpace_ShouldThrow_WhenStringIsNull()
        {
            var param = Fixture.CreateDefault<string>();
            ShouldThrow( () => Core.Assert.IsNotNullOrWhiteSpace( param ) );
        }

        [Fact]
        public void IsNotNullOrWhiteSpace_ShouldThrow_WhenStringIsEmpty()
        {
            var param = string.Empty;
            ShouldThrow( () => Core.Assert.IsNotNullOrWhiteSpace( param ) );
        }

        [Fact]
        public void IsNotNullOrWhiteSpace_ShouldThrow_WhenStringIsWhiteSpaceOnly()
        {
            var param = " \t\n\r";
            ShouldThrow( () => Core.Assert.IsNotNullOrWhiteSpace( param ) );
        }
    }
}
