using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Tests.EnsureTests;

public class EnsureOfStringTests : GenericEnsureOfRefTypeTests<string>
{
    [Fact]
    public void IsEmpty_ShouldPass_WhenStringIsEmpty()
    {
        var param = string.Empty;
        ShouldPass( () => Ensure.IsEmpty( param ) );
    }

    [Fact]
    public void IsEmpty_ShouldThrowArgumentException_WhenStringIsNotEmpty()
    {
        var param = Fixture.CreateNotDefault<string>();
        ShouldThrowArgumentException( () => Ensure.IsEmpty( param ) );
    }

    [Fact]
    public void IsNotEmpty_ShouldPass_WhenStringIsNotEmpty()
    {
        var param = Fixture.CreateNotDefault<string>();
        ShouldPass( () => Ensure.IsNotEmpty( param ) );
    }

    [Fact]
    public void IsNotEmpty_ShouldThrowArgumentException_WhenStringIsEmpty()
    {
        var param = string.Empty;
        ShouldThrowArgumentException( () => Ensure.IsNotEmpty( param ) );
    }

    [Fact]
    public void IsNullOrEmpty_ShouldPass_WhenStringIsNull()
    {
        var param = Fixture.CreateDefault<string>();
        ShouldPass( () => Ensure.IsNullOrEmpty( param ) );
    }

    [Fact]
    public void IsNullOrEmpty_ShouldPass_WhenStringIsEmpty()
    {
        var param = string.Empty;
        ShouldPass( () => Ensure.IsNullOrEmpty( param ) );
    }

    [Fact]
    public void IsNullOrEmpty_ShouldThrowArgumentException_WhenStringIsNotEmpty()
    {
        var param = Fixture.CreateNotDefault<string>();
        ShouldThrowArgumentException( () => Ensure.IsNullOrEmpty( param ) );
    }

    [Fact]
    public void IsNotNullOrEmpty_ShouldPass_WhenStringIsNotEmpty()
    {
        var param = Fixture.CreateNotDefault<string>();
        ShouldPass( () => Ensure.IsNotNullOrEmpty( param ) );
    }

    [Fact]
    public void IsNotNullOrEmpty_ShouldThrowArgumentException_WhenStringIsNull()
    {
        var param = Fixture.CreateDefault<string>();
        ShouldThrowArgumentException( () => Ensure.IsNotNullOrEmpty( param ) );
    }

    [Fact]
    public void IsNotNullOrEmpty_ShouldThrowArgumentException_WhenStringIsEmpty()
    {
        var param = string.Empty;
        ShouldThrowArgumentException( () => Ensure.IsNotNullOrEmpty( param ) );
    }

    [Fact]
    public void IsNotNullOrWhiteSpace_ShouldPass_WhenStringIsNotNullOrWhiteSpace()
    {
        var param = Fixture.CreateNotDefault<string>();
        ShouldPass( () => Ensure.IsNotNullOrWhiteSpace( param ) );
    }

    [Fact]
    public void IsNotNullOrWhiteSpace_ShouldThrowArgumentException_WhenStringIsNull()
    {
        var param = Fixture.CreateDefault<string>();
        ShouldThrowArgumentException( () => Ensure.IsNotNullOrWhiteSpace( param ) );
    }

    [Fact]
    public void IsNotNullOrWhiteSpace_ShouldThrowArgumentException_WhenStringIsEmpty()
    {
        var param = string.Empty;
        ShouldThrowArgumentException( () => Ensure.IsNotNullOrWhiteSpace( param ) );
    }

    [Fact]
    public void IsNotNullOrWhiteSpace_ShouldThrowArgumentException_WhenStringIsWhiteSpaceOnly()
    {
        var param = " \t\n\r";
        ShouldThrowArgumentException( () => Ensure.IsNotNullOrWhiteSpace( param ) );
    }
}
