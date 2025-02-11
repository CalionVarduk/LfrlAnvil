namespace LfrlAnvil.Functional.Tests.NilTests;

public class NilTests : TestsBase
{
    [Fact]
    public void GetHashCode_ShouldReturnZero()
    {
        var sut = Nil.Instance;
        var result = sut.GetHashCode();
        result.TestEquals( 0 ).Go();
    }

    [Fact]
    public void Equals_ShouldReturnTrue()
    {
        var a = Nil.Instance;
        var b = Nil.Instance;
        var result = a.Equals( b );
        result.TestTrue().Go();
    }

    [Fact]
    public void CompareTo_ShouldReturnZero()
    {
        var a = Nil.Instance;
        var b = Nil.Instance;
        var result = a.CompareTo( b );
        result.TestEquals( 0 ).Go();
    }

    [Fact]
    public void EqualityOperator_ShouldReturnTrue()
    {
        var a = Nil.Instance;
        var b = Nil.Instance;
        var result = a == b;
        result.TestTrue().Go();
    }

    [Fact]
    public void InequalityOperator_ShouldReturnFalse()
    {
        var a = Nil.Instance;
        var b = Nil.Instance;
        var result = a != b;
        result.TestFalse().Go();
    }

    [Fact]
    public void LessThanOrEqualToOperator_ShouldReturnTrue()
    {
        var a = Nil.Instance;
        var b = Nil.Instance;
        var result = a <= b;
        result.TestTrue().Go();
    }

    [Fact]
    public void GreaterThanOperator_ShouldReturnFalse()
    {
        var a = Nil.Instance;
        var b = Nil.Instance;
        var result = a > b;
        result.TestFalse().Go();
    }

    [Fact]
    public void GreaterThanOrEqualToOperator_ShouldReturnTrue()
    {
        var a = Nil.Instance;
        var b = Nil.Instance;
        var result = a >= b;
        result.TestTrue().Go();
    }

    [Fact]
    public void LessThanOperator_ShouldReturnFalse()
    {
        var a = Nil.Instance;
        var b = Nil.Instance;
        var result = a < b;
        result.TestFalse().Go();
    }
}
