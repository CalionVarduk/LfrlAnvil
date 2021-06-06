using AutoFixture;
using LfrlSoft.NET.Common.Tests.Extensions;
using Xunit;

namespace LfrlSoft.NET.Common.Tests.Assert
{
    public class String : AssertTestsRef<string>
    {
        [Fact]
        public void IsEmpty_ShouldPass_WhenStringIsEmpty()
        {
            var param = string.Empty;
            ShouldPass( () => Common.Assert.IsEmpty( param ) );
        }

        [Fact]
        public void IsEmpty_ShouldThrow_WhenStringIsNotEmpty()
        {
            var param = Fixture.CreateNotDefault<string>();
            ShouldThrow( () => Common.Assert.IsEmpty( param ) );
        }

        [Fact]
        public void IsNotEmpty_ShouldPass_WhenStringIsNotEmpty()
        {
            var param = Fixture.CreateNotDefault<string>();
            ShouldPass( () => Common.Assert.IsNotEmpty( param ) );
        }

        [Fact]
        public void IsNotEmpty_ShouldThrow_WhenStringIsEmpty()
        {
            var param = string.Empty;
            ShouldThrow( () => Common.Assert.IsNotEmpty( param ) );
        }

        [Fact]
        public void IsNullOrEmpty_ShouldPass_WhenStringIsNull()
        {
            var param = Fixture.CreateDefault<string>();
            ShouldPass( () => Common.Assert.IsNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNullOrEmpty_ShouldPass_WhenStringIsEmpty()
        {
            var param = string.Empty;
            ShouldPass( () => Common.Assert.IsNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNullOrEmpty_ShouldThrow_WhenStringIsNotEmpty()
        {
            var param = Fixture.CreateNotDefault<string>();
            ShouldThrow( () => Common.Assert.IsNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldPass_WhenStringIsNotEmpty()
        {
            var param = Fixture.CreateNotDefault<string>();
            ShouldPass( () => Common.Assert.IsNotNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldThrow_WhenStringIsNull()
        {
            var param = Fixture.CreateDefault<string>();
            ShouldThrow( () => Common.Assert.IsNotNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldThrow_WhenStringIsEmpty()
        {
            var param = string.Empty;
            ShouldThrow( () => Common.Assert.IsNotNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrWhiteSpace_ShouldPass_WhenStringIsNotNullOrWhiteSpace()
        {
            var param = Fixture.CreateNotDefault<string>();
            ShouldPass( () => Common.Assert.IsNotNullOrWhiteSpace( param ) );
        }

        [Fact]
        public void IsNotNullOrWhiteSpace_ShouldThrow_WhenStringIsNull()
        {
            var param = Fixture.CreateDefault<string>();
            ShouldThrow( () => Common.Assert.IsNotNullOrWhiteSpace( param ) );
        }

        [Fact]
        public void IsNotNullOrWhiteSpace_ShouldThrow_WhenStringIsEmpty()
        {
            var param = string.Empty;
            ShouldThrow( () => Common.Assert.IsNotNullOrWhiteSpace( param ) );
        }

        [Fact]
        public void IsNotNullOrWhiteSpace_ShouldThrow_WhenStringIsWhiteSpaceOnly()
        {
            var param = " \t\n\r";
            ShouldThrow( () => Common.Assert.IsNotNullOrWhiteSpace( param ) );
        }
    }
}
