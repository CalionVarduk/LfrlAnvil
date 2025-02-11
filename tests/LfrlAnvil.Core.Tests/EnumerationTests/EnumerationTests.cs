using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.Attributes;

namespace LfrlAnvil.Tests.EnumerationTests;

[TestClass( typeof( EnumerationTestsData ) )]
public class EnumerationTests : TestsBase
{
    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var sut = ValidEnum.One;
        var result = sut.ToString();
        result.TestEquals( "'one' (1)" ).Go();
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var sut = ValidEnum.One;
        var result = sut.GetHashCode();
        result.TestEquals( sut.Value.GetHashCode() ).Go();
    }

    [Theory]
    [MethodData( nameof( EnumerationTestsData.GetEqualsData ) )]
    public void Equals_ShouldReturnCorrectResult(ValidEnum a, ValidEnum b, bool expected)
    {
        var result = a.Equals( b );
        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( EnumerationTestsData.GetCompareToData ) )]
    public void CompareTo_ShouldReturnCorrectResult(ValidEnum a, ValidEnum b, int expected)
    {
        var result = a.CompareTo( b );
        Math.Sign( result ).TestEquals( expected ).Go();
    }

    [Fact]
    public void TValueConversionOperator_ShouldReturnValue()
    {
        var sut = ValidEnum.Two;
        var result = ( int )sut;
        result.TestEquals( sut.Value ).Go();
    }

    [Theory]
    [MethodData( nameof( EnumerationTestsData.GetEqualityOperatorData ) )]
    public void EqualityOperator_ShouldReturnCorrectResult(ValidEnum? a, ValidEnum? b, bool expected)
    {
        var result = a == b;
        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( EnumerationTestsData.GetInequalityOperatorData ) )]
    public void InequalityOperator_ShouldReturnCorrectResult(ValidEnum? a, ValidEnum? b, bool expected)
    {
        var result = a != b;
        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( EnumerationTestsData.GetGreaterThanOrEqualToOperatorData ) )]
    public void GreaterThanOrEqualToOperator_ShouldReturnCorrectResult(ValidEnum? a, ValidEnum? b, bool expected)
    {
        var result = a >= b;
        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( EnumerationTestsData.GetGreaterThanOperatorData ) )]
    public void GreaterThanOperator_ShouldReturnCorrectResult(ValidEnum? a, ValidEnum? b, bool expected)
    {
        var result = a > b;
        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( EnumerationTestsData.GetLessThanOrEqualToOperatorData ) )]
    public void LessThanOrEqualToOperator_ShouldReturnCorrectResult(ValidEnum? a, ValidEnum? b, bool expected)
    {
        var result = a <= b;
        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( EnumerationTestsData.GetLessThanOperatorData ) )]
    public void LessThanOperator_ShouldReturnCorrectResult(ValidEnum? a, ValidEnum? b, bool expected)
    {
        var result = a < b;
        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetNameDictionary_ShouldIncludeOnlyNonNullPublicStaticFieldsAndAutoPropertiesOfEnumType()
    {
        var sut = ValidEnum.ByName;
        Assertion.All(
                sut.Count.TestEquals( 3 ),
                sut.Keys.TestSetEqual( [ "one", "two", "three" ] ),
                sut.GetValueOrDefault( "one" ).TestEquals( ValidEnum.One ),
                sut.GetValueOrDefault( "two" ).TestEquals( ValidEnum.Two ),
                sut.GetValueOrDefault( "three" ).TestEquals( ValidEnum.Three ) )
            .Go();
    }

    [Fact]
    public void GetNameDictionary_ShouldThrowException_WhenNamesAreDuplicated()
    {
        var action = Lambda.Of( () => DuplicateNameEnum.ByName );
        action.Test( exc => exc.TestType().Exact<TypeInitializationException>() ).Go();
    }

    [Fact]
    public void GetValueDictionary_ShouldIncludeOnlyNonNullPublicStaticFieldsAndAutoPropertiesOfEnumType()
    {
        var sut = ValidEnum.ByValue;
        Assertion.All(
                sut.Count.TestEquals( 3 ),
                sut.Keys.TestSetEqual( [ 1, 2, 3 ] ),
                sut.GetValueOrDefault( 1 ).TestEquals( ValidEnum.One ),
                sut.GetValueOrDefault( 2 ).TestEquals( ValidEnum.Two ),
                sut.GetValueOrDefault( 3 ).TestEquals( ValidEnum.Three ) )
            .Go();
    }

    [Fact]
    public void GetValueDictionary_ShouldThrowException_WhenNamesAreDuplicated()
    {
        var action = Lambda.Of( () => DuplicateValueEnum.ByValue );
        action.Test( exc => exc.TestType().Exact<TypeInitializationException>() ).Go();
    }
}
