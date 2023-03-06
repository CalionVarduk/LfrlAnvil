using System.Collections.Generic;
using FluentAssertions.Execution;
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
        result.Should().Be( "'one' (1)" );
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var sut = ValidEnum.One;
        var result = sut.GetHashCode();
        result.Should().Be( sut.Value.GetHashCode() );
    }

    [Theory]
    [MethodData( nameof( EnumerationTestsData.GetEqualsData ) )]
    public void Equals_ShouldReturnCorrectResult(ValidEnum a, ValidEnum b, bool expected)
    {
        var result = a.Equals( b );
        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( EnumerationTestsData.GetCompareToData ) )]
    public void CompareTo_ShouldReturnCorrectResult(ValidEnum a, ValidEnum b, int expected)
    {
        var result = a.CompareTo( b );
        Math.Sign( result ).Should().Be( expected );
    }

    [Fact]
    public void TValueConversionOperator_ShouldReturnValue()
    {
        var sut = ValidEnum.Two;
        var result = (int)sut;
        result.Should().Be( sut.Value );
    }

    [Theory]
    [MethodData( nameof( EnumerationTestsData.GetEqualityOperatorData ) )]
    public void EqualityOperator_ShouldReturnCorrectResult(ValidEnum? a, ValidEnum? b, bool expected)
    {
        var result = a == b;
        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( EnumerationTestsData.GetInequalityOperatorData ) )]
    public void InequalityOperator_ShouldReturnCorrectResult(ValidEnum? a, ValidEnum? b, bool expected)
    {
        var result = a != b;
        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( EnumerationTestsData.GetGreaterThanOrEqualToOperatorData ) )]
    public void GreaterThanOrEqualToOperator_ShouldReturnCorrectResult(ValidEnum? a, ValidEnum? b, bool expected)
    {
        var result = a >= b;
        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( EnumerationTestsData.GetGreaterThanOperatorData ) )]
    public void GreaterThanOperator_ShouldReturnCorrectResult(ValidEnum? a, ValidEnum? b, bool expected)
    {
        var result = a > b;
        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( EnumerationTestsData.GetLessThanOrEqualToOperatorData ) )]
    public void LessThanOrEqualToOperator_ShouldReturnCorrectResult(ValidEnum? a, ValidEnum? b, bool expected)
    {
        var result = a <= b;
        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( EnumerationTestsData.GetLessThanOperatorData ) )]
    public void LessThanOperator_ShouldReturnCorrectResult(ValidEnum? a, ValidEnum? b, bool expected)
    {
        var result = a < b;
        result.Should().Be( expected );
    }

    [Fact]
    public void GetNameDictionary_ShouldIncludeOnlyNonNullPublicStaticFieldsAndAutoPropertiesOfEnumType()
    {
        var sut = ValidEnum.ByName;
        using ( new AssertionScope() )
        {
            sut.Should().HaveCount( 3 );
            sut.Should()
                .BeEquivalentTo(
                    KeyValuePair.Create( "one", ValidEnum.One ),
                    KeyValuePair.Create( "two", ValidEnum.Two ),
                    KeyValuePair.Create( "three", ValidEnum.Three ) );
        }
    }

    [Fact]
    public void GetNameDictionary_ShouldThrowException_WhenNamesAreDuplicated()
    {
        var action = Lambda.Of( () => DuplicateNameEnum.ByName );
        action.Should().ThrowExactly<TypeInitializationException>();
    }

    [Fact]
    public void GetValueDictionary_ShouldIncludeOnlyNonNullPublicStaticFieldsAndAutoPropertiesOfEnumType()
    {
        var sut = ValidEnum.ByValue;
        using ( new AssertionScope() )
        {
            sut.Should().HaveCount( 3 );
            sut.Should()
                .BeEquivalentTo(
                    KeyValuePair.Create( 1, ValidEnum.One ),
                    KeyValuePair.Create( 2, ValidEnum.Two ),
                    KeyValuePair.Create( 3, ValidEnum.Three ) );
        }
    }

    [Fact]
    public void GetValueDictionary_ShouldThrowException_WhenNamesAreDuplicated()
    {
        var action = Lambda.Of( () => DuplicateValueEnum.ByValue );
        action.Should().ThrowExactly<TypeInitializationException>();
    }
}
