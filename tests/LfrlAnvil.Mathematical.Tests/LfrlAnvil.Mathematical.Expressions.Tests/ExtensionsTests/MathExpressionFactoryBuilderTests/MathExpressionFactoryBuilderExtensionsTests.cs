using System;
using System.Linq;
using System.Numerics;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Mathematical.Expressions.Constructs;
using LfrlAnvil.Mathematical.Expressions.Constructs.BigInt;
using LfrlAnvil.Mathematical.Expressions.Constructs.Boolean;
using LfrlAnvil.Mathematical.Expressions.Constructs.Decimal;
using LfrlAnvil.Mathematical.Expressions.Constructs.Double;
using LfrlAnvil.Mathematical.Expressions.Constructs.Float;
using LfrlAnvil.Mathematical.Expressions.Constructs.Int32;
using LfrlAnvil.Mathematical.Expressions.Constructs.Int64;
using LfrlAnvil.Mathematical.Expressions.Constructs.String;
using LfrlAnvil.Mathematical.Expressions.Extensions;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using Xunit;

namespace LfrlAnvil.Mathematical.Expressions.Tests.ExtensionsTests.MathExpressionFactoryBuilderTests;

public class MathExpressionFactoryBuilderExtensionsTests : TestsBase
{
    [Fact]
    public void AddGenericArithmeticOperators_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new MathExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("+", typeof( MathExpressionAddOperator )),
            ("-", typeof( MathExpressionSubtractOperator )),
            ("*", typeof( MathExpressionMultiplyOperator )),
            ("/", typeof( MathExpressionDivideOperator )),
            ("mod", typeof( MathExpressionModuloOperator )),
            ("-", typeof( MathExpressionNegateOperator ))
        };

        var expectedBinaryOperatorPrecedences = new (string Symbol, int Value)[]
        {
            ("+", 3),
            ("-", 3),
            ("*", 2),
            ("/", 2),
            ("mod", 2)
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("-", 1)
        };

        var result = sut.AddGenericArithmeticOperators();
        var actualConstructs = result.GetCurrentConstructs().Select( x => (x.Key.ToString(), x.Value.GetType()) );
        var actualBinaryOperatorPrecedences = result.GetCurrentBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );
        var actualPrefixUnaryConstructPrecedences =
            result.GetCurrentPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( expectedConstructs );
            actualBinaryOperatorPrecedences.Should().BeEquivalentTo( expectedBinaryOperatorPrecedences );
            actualPrefixUnaryConstructPrecedences.Should().BeEquivalentTo( expectedPrefixUnaryConstructPrecedences );
        }
    }

    [Fact]
    public void AddGenericBitwiseOperators_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new MathExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("&", typeof( MathExpressionBitwiseAndOperator )),
            ("|", typeof( MathExpressionBitwiseOrOperator )),
            ("^", typeof( MathExpressionBitwiseXorOperator )),
            ("<<", typeof( MathExpressionBitwiseLeftShiftOperator )),
            (">>", typeof( MathExpressionBitwiseRightShiftOperator )),
            ("~", typeof( MathExpressionBitwiseNotOperator ))
        };

        var expectedBinaryOperatorPrecedences = new (string Symbol, int Value)[]
        {
            ("&", 8),
            ("|", 10),
            ("^", 9),
            ("<<", 4),
            (">>", 4)
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("~", 1)
        };

        var result = sut.AddGenericBitwiseOperators();
        var actualConstructs = result.GetCurrentConstructs().Select( x => (x.Key.ToString(), x.Value.GetType()) );
        var actualBinaryOperatorPrecedences = result.GetCurrentBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );
        var actualPrefixUnaryConstructPrecedences =
            result.GetCurrentPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( expectedConstructs );
            actualBinaryOperatorPrecedences.Should().BeEquivalentTo( expectedBinaryOperatorPrecedences );
            actualPrefixUnaryConstructPrecedences.Should().BeEquivalentTo( expectedPrefixUnaryConstructPrecedences );
        }
    }

    [Fact]
    public void AddGenericLogicalOperators_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new MathExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("??", typeof( MathExpressionCoalesceOperator )),
            ("==", typeof( MathExpressionEqualToOperator )),
            ("!=", typeof( MathExpressionNotEqualToOperator )),
            (">", typeof( MathExpressionGreaterThanOperator )),
            ("<", typeof( MathExpressionLessThanOperator )),
            (">=", typeof( MathExpressionGreaterThanOrEqualToOperator )),
            ("<=", typeof( MathExpressionLessThanOrEqualToOperator )),
            ("<=>", typeof( MathExpressionCompareOperator ))
        };

        var expectedBinaryOperatorPrecedences = new (string Symbol, int Value)[]
        {
            ("??", 13),
            ("==", 7),
            ("!=", 7),
            (">", 6),
            ("<", 6),
            (">=", 6),
            ("<=", 6),
            ("<=>", 5)
        };

        var result = sut.AddGenericLogicalOperators();
        var actualConstructs = result.GetCurrentConstructs().Select( x => (x.Key.ToString(), x.Value.GetType()) );
        var actualBinaryOperatorPrecedences = result.GetCurrentBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( expectedConstructs );
            actualBinaryOperatorPrecedences.Should().BeEquivalentTo( expectedBinaryOperatorPrecedences );
        }
    }

    [Fact]
    public void AddBooleanBitwiseOperators_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new MathExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("&", typeof( MathExpressionBitwiseAndBooleanOperator )),
            ("|", typeof( MathExpressionBitwiseOrBooleanOperator )),
            ("^", typeof( MathExpressionBitwiseXorBooleanOperator ))
        };

        var expectedBinaryOperatorPrecedences = new (string Symbol, int Value)[]
        {
            ("&", 8),
            ("|", 10),
            ("^", 9)
        };

        var result = sut.AddBooleanBitwiseOperators();
        var actualConstructs = result.GetCurrentConstructs().Select( x => (x.Key.ToString(), x.Value.GetType()) );
        var actualBinaryOperatorPrecedences = result.GetCurrentBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( expectedConstructs );
            actualBinaryOperatorPrecedences.Should().BeEquivalentTo( expectedBinaryOperatorPrecedences );
        }
    }

    [Fact]
    public void AddBooleanLogicalOperators_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new MathExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("==", typeof( MathExpressionEqualToBooleanOperator )),
            ("!=", typeof( MathExpressionNotEqualToBooleanOperator )),
            ("<=>", typeof( MathExpressionCompareBooleanOperator )),
            ("and", typeof( MathExpressionAndOperator )),
            ("or", typeof( MathExpressionOrOperator )),
            ("not", typeof( MathExpressionNotOperator ))
        };

        var expectedBinaryOperatorPrecedences = new (string Symbol, int Value)[]
        {
            ("==", 7),
            ("!=", 7),
            ("<=>", 5),
            ("and", 11),
            ("or", 12)
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("not", 1)
        };

        var result = sut.AddBooleanLogicalOperators();
        var actualConstructs = result.GetCurrentConstructs().Select( x => (x.Key.ToString(), x.Value.GetType()) );
        var actualBinaryOperatorPrecedences = result.GetCurrentBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );
        var actualPrefixUnaryConstructPrecedences =
            result.GetCurrentPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( expectedConstructs );
            actualBinaryOperatorPrecedences.Should().BeEquivalentTo( expectedBinaryOperatorPrecedences );
            actualPrefixUnaryConstructPrecedences.Should().BeEquivalentTo( expectedPrefixUnaryConstructPrecedences );
        }
    }

    [Fact]
    public void AddToBooleanTypeConversion_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new MathExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("[boolean]", typeof( MathExpressionTypeConverter<bool> )),
            ("[boolean]", typeof( MathExpressionTypeConverter<bool, int> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[boolean]", 1)
        };

        var result = sut.AddToBooleanTypeConversion( specializedConverters: new MathExpressionTypeConverter<bool, int>() );
        var actualConstructs = result.GetCurrentConstructs().Select( x => (x.Key.ToString(), x.Value.GetType()) );
        var actualPrefixUnaryConstructPrecedences =
            result.GetCurrentPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( expectedConstructs );
            actualPrefixUnaryConstructPrecedences.Should().BeEquivalentTo( expectedPrefixUnaryConstructPrecedences );
        }
    }

    [Fact]
    public void AddToBooleanTypeConversion_ShouldAddPostfixTypeConverterIfSymbolIsNotNull()
    {
        var sut = new MathExpressionFactoryBuilder().AddToBooleanTypeConversion( postfixSymbol: "B" );

        using ( new AssertionScope() )
        {
            sut.GetCurrentConstructs().Select( c => c.Key.ToString() ).Should().Contain( "B" );
            sut.GetCurrentPostfixUnaryConstructPrecedences().Should().HaveCount( 1 );
        }
    }

    [Fact]
    public void AddDecimalArithmeticOperators_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new MathExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("+", typeof( MathExpressionAddDecimalOperator )),
            ("-", typeof( MathExpressionSubtractDecimalOperator )),
            ("*", typeof( MathExpressionMultiplyDecimalOperator )),
            ("/", typeof( MathExpressionDivideDecimalOperator )),
            ("mod", typeof( MathExpressionModuloDecimalOperator )),
            ("-", typeof( MathExpressionNegateDecimalOperator ))
        };

        var expectedBinaryOperatorPrecedences = new (string Symbol, int Value)[]
        {
            ("+", 3),
            ("-", 3),
            ("*", 2),
            ("/", 2),
            ("mod", 2)
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("-", 1)
        };

        var result = sut.AddDecimalArithmeticOperators();
        var actualConstructs = result.GetCurrentConstructs().Select( x => (x.Key.ToString(), x.Value.GetType()) );
        var actualBinaryOperatorPrecedences = result.GetCurrentBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );
        var actualPrefixUnaryConstructPrecedences =
            result.GetCurrentPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( expectedConstructs );
            actualBinaryOperatorPrecedences.Should().BeEquivalentTo( expectedBinaryOperatorPrecedences );
            actualPrefixUnaryConstructPrecedences.Should().BeEquivalentTo( expectedPrefixUnaryConstructPrecedences );
        }
    }

    [Fact]
    public void AddDecimalLogicalOperators_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new MathExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("==", typeof( MathExpressionEqualToDecimalOperator )),
            ("!=", typeof( MathExpressionNotEqualToDecimalOperator )),
            (">", typeof( MathExpressionGreaterThanDecimalOperator )),
            ("<", typeof( MathExpressionLessThanDecimalOperator )),
            (">=", typeof( MathExpressionGreaterThanOrEqualToDecimalOperator )),
            ("<=", typeof( MathExpressionLessThanOrEqualToDecimalOperator )),
            ("<=>", typeof( MathExpressionCompareDecimalOperator ))
        };

        var expectedBinaryOperatorPrecedences = new (string Symbol, int Value)[]
        {
            ("==", 7),
            ("!=", 7),
            (">", 6),
            ("<", 6),
            (">=", 6),
            ("<=", 6),
            ("<=>", 5)
        };

        var result = sut.AddDecimalLogicalOperators();
        var actualConstructs = result.GetCurrentConstructs().Select( x => (x.Key.ToString(), x.Value.GetType()) );
        var actualBinaryOperatorPrecedences = result.GetCurrentBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( expectedConstructs );
            actualBinaryOperatorPrecedences.Should().BeEquivalentTo( expectedBinaryOperatorPrecedences );
        }
    }

    [Fact]
    public void AddToDecimalTypeConversion_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new MathExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("[decimal]", typeof( MathExpressionTypeConverter<decimal> )),
            ("[decimal]", typeof( MathExpressionTypeConverter<decimal, double> )),
            ("M", typeof( MathExpressionTypeConverter<decimal> )),
            ("M", typeof( MathExpressionTypeConverter<decimal, double> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[decimal]", 1)
        };

        var expectedPostfixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("M", 1)
        };

        var result = sut.AddToDecimalTypeConversion( specializedConverters: new MathExpressionTypeConverter<decimal, double>() );
        var actualConstructs = result.GetCurrentConstructs().Select( x => (x.Key.ToString(), x.Value.GetType()) );
        var actualPrefixUnaryConstructPrecedences =
            result.GetCurrentPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        var actualPostfixUnaryConstructPrecedences =
            result.GetCurrentPostfixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( expectedConstructs );
            actualPrefixUnaryConstructPrecedences.Should().BeEquivalentTo( expectedPrefixUnaryConstructPrecedences );
            expectedPostfixUnaryConstructPrecedences.Should().BeEquivalentTo( actualPostfixUnaryConstructPrecedences );
        }
    }

    [Fact]
    public void AddToDecimalTypeConversion_ShouldNotAddPostfixTypeConverterIfSymbolIsNull()
    {
        var sut = new MathExpressionFactoryBuilder().AddToDecimalTypeConversion( postfixSymbol: null );

        using ( new AssertionScope() )
        {
            sut.GetCurrentConstructs().Should().HaveCount( 1 );
            sut.GetCurrentPostfixUnaryConstructPrecedences().Should().BeEmpty();
        }
    }

    [Fact]
    public void AddDoubleArithmeticOperators_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new MathExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("+", typeof( MathExpressionAddDoubleOperator )),
            ("-", typeof( MathExpressionSubtractDoubleOperator )),
            ("*", typeof( MathExpressionMultiplyDoubleOperator )),
            ("/", typeof( MathExpressionDivideDoubleOperator )),
            ("mod", typeof( MathExpressionModuloDoubleOperator )),
            ("-", typeof( MathExpressionNegateDoubleOperator ))
        };

        var expectedBinaryOperatorPrecedences = new (string Symbol, int Value)[]
        {
            ("+", 3),
            ("-", 3),
            ("*", 2),
            ("/", 2),
            ("mod", 2)
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("-", 1)
        };

        var result = sut.AddDoubleArithmeticOperators();
        var actualConstructs = result.GetCurrentConstructs().Select( x => (x.Key.ToString(), x.Value.GetType()) );
        var actualBinaryOperatorPrecedences = result.GetCurrentBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );
        var actualPrefixUnaryConstructPrecedences =
            result.GetCurrentPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( expectedConstructs );
            actualBinaryOperatorPrecedences.Should().BeEquivalentTo( expectedBinaryOperatorPrecedences );
            actualPrefixUnaryConstructPrecedences.Should().BeEquivalentTo( expectedPrefixUnaryConstructPrecedences );
        }
    }

    [Fact]
    public void AddDoubleLogicalOperators_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new MathExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("==", typeof( MathExpressionEqualToDoubleOperator )),
            ("!=", typeof( MathExpressionNotEqualToDoubleOperator )),
            (">", typeof( MathExpressionGreaterThanDoubleOperator )),
            ("<", typeof( MathExpressionLessThanDoubleOperator )),
            (">=", typeof( MathExpressionGreaterThanOrEqualToDoubleOperator )),
            ("<=", typeof( MathExpressionLessThanOrEqualToDoubleOperator )),
            ("<=>", typeof( MathExpressionCompareDoubleOperator ))
        };

        var expectedBinaryOperatorPrecedences = new (string Symbol, int Value)[]
        {
            ("==", 7),
            ("!=", 7),
            (">", 6),
            ("<", 6),
            (">=", 6),
            ("<=", 6),
            ("<=>", 5)
        };

        var result = sut.AddDoubleLogicalOperators();
        var actualConstructs = result.GetCurrentConstructs().Select( x => (x.Key.ToString(), x.Value.GetType()) );
        var actualBinaryOperatorPrecedences = result.GetCurrentBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( expectedConstructs );
            actualBinaryOperatorPrecedences.Should().BeEquivalentTo( expectedBinaryOperatorPrecedences );
        }
    }

    [Fact]
    public void AddToDoubleTypeConversion_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new MathExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("[double]", typeof( MathExpressionTypeConverter<double> )),
            ("[double]", typeof( MathExpressionTypeConverter<double, decimal> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[double]", 1)
        };

        var result = sut.AddToDoubleTypeConversion( specializedConverters: new MathExpressionTypeConverter<double, decimal>() );
        var actualConstructs = result.GetCurrentConstructs().Select( x => (x.Key.ToString(), x.Value.GetType()) );
        var actualPrefixUnaryConstructPrecedences =
            result.GetCurrentPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( expectedConstructs );
            actualPrefixUnaryConstructPrecedences.Should().BeEquivalentTo( expectedPrefixUnaryConstructPrecedences );
        }
    }

    [Fact]
    public void AddToDoubleTypeConversion_ShouldAddPostfixTypeConverterIfSymbolIsNotNull()
    {
        var sut = new MathExpressionFactoryBuilder().AddToDoubleTypeConversion( postfixSymbol: "D" );

        using ( new AssertionScope() )
        {
            sut.GetCurrentConstructs().Select( c => c.Key.ToString() ).Should().Contain( "D" );
            sut.GetCurrentPostfixUnaryConstructPrecedences().Should().HaveCount( 1 );
        }
    }

    [Fact]
    public void AddFloatArithmeticOperators_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new MathExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("+", typeof( MathExpressionAddFloatOperator )),
            ("-", typeof( MathExpressionSubtractFloatOperator )),
            ("*", typeof( MathExpressionMultiplyFloatOperator )),
            ("/", typeof( MathExpressionDivideFloatOperator )),
            ("mod", typeof( MathExpressionModuloFloatOperator )),
            ("-", typeof( MathExpressionNegateFloatOperator ))
        };

        var expectedBinaryOperatorPrecedences = new (string Symbol, int Value)[]
        {
            ("+", 3),
            ("-", 3),
            ("*", 2),
            ("/", 2),
            ("mod", 2)
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("-", 1)
        };

        var result = sut.AddFloatArithmeticOperators();
        var actualConstructs = result.GetCurrentConstructs().Select( x => (x.Key.ToString(), x.Value.GetType()) );
        var actualBinaryOperatorPrecedences = result.GetCurrentBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );
        var actualPrefixUnaryConstructPrecedences =
            result.GetCurrentPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( expectedConstructs );
            actualBinaryOperatorPrecedences.Should().BeEquivalentTo( expectedBinaryOperatorPrecedences );
            actualPrefixUnaryConstructPrecedences.Should().BeEquivalentTo( expectedPrefixUnaryConstructPrecedences );
        }
    }

    [Fact]
    public void AddFloatLogicalOperators_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new MathExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("==", typeof( MathExpressionEqualToFloatOperator )),
            ("!=", typeof( MathExpressionNotEqualToFloatOperator )),
            (">", typeof( MathExpressionGreaterThanFloatOperator )),
            ("<", typeof( MathExpressionLessThanFloatOperator )),
            (">=", typeof( MathExpressionGreaterThanOrEqualToFloatOperator )),
            ("<=", typeof( MathExpressionLessThanOrEqualToFloatOperator )),
            ("<=>", typeof( MathExpressionCompareFloatOperator ))
        };

        var expectedBinaryOperatorPrecedences = new (string Symbol, int Value)[]
        {
            ("==", 7),
            ("!=", 7),
            (">", 6),
            ("<", 6),
            (">=", 6),
            ("<=", 6),
            ("<=>", 5)
        };

        var result = sut.AddFloatLogicalOperators();
        var actualConstructs = result.GetCurrentConstructs().Select( x => (x.Key.ToString(), x.Value.GetType()) );
        var actualBinaryOperatorPrecedences = result.GetCurrentBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( expectedConstructs );
            actualBinaryOperatorPrecedences.Should().BeEquivalentTo( expectedBinaryOperatorPrecedences );
        }
    }

    [Fact]
    public void AddToFloatTypeConversion_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new MathExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("[float]", typeof( MathExpressionTypeConverter<float> )),
            ("[float]", typeof( MathExpressionTypeConverter<float, decimal> )),
            ("F", typeof( MathExpressionTypeConverter<float> )),
            ("F", typeof( MathExpressionTypeConverter<float, decimal> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[float]", 1)
        };

        var expectedPostfixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("F", 1)
        };

        var result = sut.AddToFloatTypeConversion( specializedConverters: new MathExpressionTypeConverter<float, decimal>() );
        var actualConstructs = result.GetCurrentConstructs().Select( x => (x.Key.ToString(), x.Value.GetType()) );
        var actualPrefixUnaryConstructPrecedences =
            result.GetCurrentPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        var actualPostfixUnaryConstructPrecedences =
            result.GetCurrentPostfixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( expectedConstructs );
            actualPrefixUnaryConstructPrecedences.Should().BeEquivalentTo( expectedPrefixUnaryConstructPrecedences );
            expectedPostfixUnaryConstructPrecedences.Should().BeEquivalentTo( actualPostfixUnaryConstructPrecedences );
        }
    }

    [Fact]
    public void AddToFloatTypeConversion_ShouldNotAddPostfixTypeConverterIfSymbolIsNull()
    {
        var sut = new MathExpressionFactoryBuilder().AddToFloatTypeConversion( postfixSymbol: null );

        using ( new AssertionScope() )
        {
            sut.GetCurrentConstructs().Should().HaveCount( 1 );
            sut.GetCurrentPostfixUnaryConstructPrecedences().Should().BeEmpty();
        }
    }

    [Fact]
    public void AddInt32ArithmeticOperators_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new MathExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("+", typeof( MathExpressionAddInt32Operator )),
            ("-", typeof( MathExpressionSubtractInt32Operator )),
            ("*", typeof( MathExpressionMultiplyInt32Operator )),
            ("/", typeof( MathExpressionDivideInt32Operator )),
            ("mod", typeof( MathExpressionModuloInt32Operator )),
            ("-", typeof( MathExpressionNegateInt32Operator ))
        };

        var expectedBinaryOperatorPrecedences = new (string Symbol, int Value)[]
        {
            ("+", 3),
            ("-", 3),
            ("*", 2),
            ("/", 2),
            ("mod", 2)
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("-", 1)
        };

        var result = sut.AddInt32ArithmeticOperators();
        var actualConstructs = result.GetCurrentConstructs().Select( x => (x.Key.ToString(), x.Value.GetType()) );
        var actualBinaryOperatorPrecedences = result.GetCurrentBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );
        var actualPrefixUnaryConstructPrecedences =
            result.GetCurrentPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( expectedConstructs );
            actualBinaryOperatorPrecedences.Should().BeEquivalentTo( expectedBinaryOperatorPrecedences );
            actualPrefixUnaryConstructPrecedences.Should().BeEquivalentTo( expectedPrefixUnaryConstructPrecedences );
        }
    }

    [Fact]
    public void AddInt32BitwiseOperators_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new MathExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("&", typeof( MathExpressionBitwiseAndInt32Operator )),
            ("|", typeof( MathExpressionBitwiseOrInt32Operator )),
            ("^", typeof( MathExpressionBitwiseXorInt32Operator )),
            ("<<", typeof( MathExpressionBitwiseLeftShiftInt32Operator )),
            (">>", typeof( MathExpressionBitwiseRightShiftInt32Operator )),
            ("~", typeof( MathExpressionBitwiseNotInt32Operator ))
        };

        var expectedBinaryOperatorPrecedences = new (string Symbol, int Value)[]
        {
            ("&", 8),
            ("|", 10),
            ("^", 9),
            ("<<", 4),
            (">>", 4)
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("~", 1)
        };

        var result = sut.AddInt32BitwiseOperators();
        var actualConstructs = result.GetCurrentConstructs().Select( x => (x.Key.ToString(), x.Value.GetType()) );
        var actualBinaryOperatorPrecedences = result.GetCurrentBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );
        var actualPrefixUnaryConstructPrecedences =
            result.GetCurrentPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( expectedConstructs );
            actualBinaryOperatorPrecedences.Should().BeEquivalentTo( expectedBinaryOperatorPrecedences );
            actualPrefixUnaryConstructPrecedences.Should().BeEquivalentTo( expectedPrefixUnaryConstructPrecedences );
        }
    }

    [Fact]
    public void AddInt32LogicalOperators_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new MathExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("==", typeof( MathExpressionEqualToInt32Operator )),
            ("!=", typeof( MathExpressionNotEqualToInt32Operator )),
            (">", typeof( MathExpressionGreaterThanInt32Operator )),
            ("<", typeof( MathExpressionLessThanInt32Operator )),
            (">=", typeof( MathExpressionGreaterThanOrEqualToInt32Operator )),
            ("<=", typeof( MathExpressionLessThanOrEqualToInt32Operator )),
            ("<=>", typeof( MathExpressionCompareInt32Operator ))
        };

        var expectedBinaryOperatorPrecedences = new (string Symbol, int Value)[]
        {
            ("==", 7),
            ("!=", 7),
            (">", 6),
            ("<", 6),
            (">=", 6),
            ("<=", 6),
            ("<=>", 5)
        };

        var result = sut.AddInt32LogicalOperators();
        var actualConstructs = result.GetCurrentConstructs().Select( x => (x.Key.ToString(), x.Value.GetType()) );
        var actualBinaryOperatorPrecedences = result.GetCurrentBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( expectedConstructs );
            actualBinaryOperatorPrecedences.Should().BeEquivalentTo( expectedBinaryOperatorPrecedences );
        }
    }

    [Fact]
    public void AddToInt32TypeConversion_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new MathExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("[int32]", typeof( MathExpressionTypeConverter<int> )),
            ("[int32]", typeof( MathExpressionTypeConverter<int, long> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[int32]", 1)
        };

        var result = sut.AddToInt32TypeConversion( specializedConverters: new MathExpressionTypeConverter<int, long>() );
        var actualConstructs = result.GetCurrentConstructs().Select( x => (x.Key.ToString(), x.Value.GetType()) );
        var actualPrefixUnaryConstructPrecedences =
            result.GetCurrentPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( expectedConstructs );
            actualPrefixUnaryConstructPrecedences.Should().BeEquivalentTo( expectedPrefixUnaryConstructPrecedences );
        }
    }

    [Fact]
    public void AddToInt32TypeConversion_ShouldAddPostfixTypeConverterIfSymbolIsNotNull()
    {
        var sut = new MathExpressionFactoryBuilder().AddToInt32TypeConversion( postfixSymbol: "I" );

        using ( new AssertionScope() )
        {
            sut.GetCurrentConstructs().Select( c => c.Key.ToString() ).Should().Contain( "I" );
            sut.GetCurrentPostfixUnaryConstructPrecedences().Should().HaveCount( 1 );
        }
    }

    [Fact]
    public void AddInt64ArithmeticOperators_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new MathExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("+", typeof( MathExpressionAddInt64Operator )),
            ("-", typeof( MathExpressionSubtractInt64Operator )),
            ("*", typeof( MathExpressionMultiplyInt64Operator )),
            ("/", typeof( MathExpressionDivideInt64Operator )),
            ("mod", typeof( MathExpressionModuloInt64Operator )),
            ("-", typeof( MathExpressionNegateInt64Operator ))
        };

        var expectedBinaryOperatorPrecedences = new (string Symbol, int Value)[]
        {
            ("+", 3),
            ("-", 3),
            ("*", 2),
            ("/", 2),
            ("mod", 2)
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("-", 1)
        };

        var result = sut.AddInt64ArithmeticOperators();
        var actualConstructs = result.GetCurrentConstructs().Select( x => (x.Key.ToString(), x.Value.GetType()) );
        var actualBinaryOperatorPrecedences = result.GetCurrentBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );
        var actualPrefixUnaryConstructPrecedences =
            result.GetCurrentPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( expectedConstructs );
            actualBinaryOperatorPrecedences.Should().BeEquivalentTo( expectedBinaryOperatorPrecedences );
            actualPrefixUnaryConstructPrecedences.Should().BeEquivalentTo( expectedPrefixUnaryConstructPrecedences );
        }
    }

    [Fact]
    public void AddInt64BitwiseOperators_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new MathExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("&", typeof( MathExpressionBitwiseAndInt64Operator )),
            ("|", typeof( MathExpressionBitwiseOrInt64Operator )),
            ("^", typeof( MathExpressionBitwiseXorInt64Operator )),
            ("<<", typeof( MathExpressionBitwiseLeftShiftInt64Operator )),
            (">>", typeof( MathExpressionBitwiseRightShiftInt64Operator )),
            ("~", typeof( MathExpressionBitwiseNotInt64Operator ))
        };

        var expectedBinaryOperatorPrecedences = new (string Symbol, int Value)[]
        {
            ("&", 8),
            ("|", 10),
            ("^", 9),
            ("<<", 4),
            (">>", 4)
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("~", 1)
        };

        var result = sut.AddInt64BitwiseOperators();
        var actualConstructs = result.GetCurrentConstructs().Select( x => (x.Key.ToString(), x.Value.GetType()) );
        var actualBinaryOperatorPrecedences = result.GetCurrentBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );
        var actualPrefixUnaryConstructPrecedences =
            result.GetCurrentPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( expectedConstructs );
            actualBinaryOperatorPrecedences.Should().BeEquivalentTo( expectedBinaryOperatorPrecedences );
            actualPrefixUnaryConstructPrecedences.Should().BeEquivalentTo( expectedPrefixUnaryConstructPrecedences );
        }
    }

    [Fact]
    public void AddInt64LogicalOperators_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new MathExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("==", typeof( MathExpressionEqualToInt64Operator )),
            ("!=", typeof( MathExpressionNotEqualToInt64Operator )),
            (">", typeof( MathExpressionGreaterThanInt64Operator )),
            ("<", typeof( MathExpressionLessThanInt64Operator )),
            (">=", typeof( MathExpressionGreaterThanOrEqualToInt64Operator )),
            ("<=", typeof( MathExpressionLessThanOrEqualToInt64Operator )),
            ("<=>", typeof( MathExpressionCompareInt64Operator ))
        };

        var expectedBinaryOperatorPrecedences = new (string Symbol, int Value)[]
        {
            ("==", 7),
            ("!=", 7),
            (">", 6),
            ("<", 6),
            (">=", 6),
            ("<=", 6),
            ("<=>", 5)
        };

        var result = sut.AddInt64LogicalOperators();
        var actualConstructs = result.GetCurrentConstructs().Select( x => (x.Key.ToString(), x.Value.GetType()) );
        var actualBinaryOperatorPrecedences = result.GetCurrentBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( expectedConstructs );
            actualBinaryOperatorPrecedences.Should().BeEquivalentTo( expectedBinaryOperatorPrecedences );
        }
    }

    [Fact]
    public void AddToInt64TypeConversion_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new MathExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("[int64]", typeof( MathExpressionTypeConverter<long> )),
            ("[int64]", typeof( MathExpressionTypeConverter<long, int> )),
            ("L", typeof( MathExpressionTypeConverter<long> )),
            ("L", typeof( MathExpressionTypeConverter<long, int> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[int64]", 1)
        };

        var expectedPostfixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("L", 1)
        };

        var result = sut.AddToInt64TypeConversion( specializedConverters: new MathExpressionTypeConverter<long, int>() );
        var actualConstructs = result.GetCurrentConstructs().Select( x => (x.Key.ToString(), x.Value.GetType()) );
        var actualPrefixUnaryConstructPrecedences =
            result.GetCurrentPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        var actualPostfixUnaryConstructPrecedences =
            result.GetCurrentPostfixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( expectedConstructs );
            actualPrefixUnaryConstructPrecedences.Should().BeEquivalentTo( expectedPrefixUnaryConstructPrecedences );
            actualPostfixUnaryConstructPrecedences.Should().BeEquivalentTo( expectedPostfixUnaryConstructPrecedences );
        }
    }

    [Fact]
    public void AddToInt64TypeConversion_ShouldNotAddPostfixTypeConverterIfSymbolIsNull()
    {
        var sut = new MathExpressionFactoryBuilder().AddToInt64TypeConversion( postfixSymbol: null );

        using ( new AssertionScope() )
        {
            sut.GetCurrentConstructs().Should().HaveCount( 1 );
            sut.GetCurrentPostfixUnaryConstructPrecedences().Should().BeEmpty();
        }
    }

    [Fact]
    public void AddBigIntArithmeticOperators_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new MathExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("+", typeof( MathExpressionAddBigIntOperator )),
            ("-", typeof( MathExpressionSubtractBigIntOperator )),
            ("*", typeof( MathExpressionMultiplyBigIntOperator )),
            ("/", typeof( MathExpressionDivideBigIntOperator )),
            ("mod", typeof( MathExpressionModuloBigIntOperator )),
            ("-", typeof( MathExpressionNegateBigIntOperator ))
        };

        var expectedBinaryOperatorPrecedences = new (string Symbol, int Value)[]
        {
            ("+", 3),
            ("-", 3),
            ("*", 2),
            ("/", 2),
            ("mod", 2)
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("-", 1)
        };

        var result = sut.AddBigIntArithmeticOperators();
        var actualConstructs = result.GetCurrentConstructs().Select( x => (x.Key.ToString(), x.Value.GetType()) );
        var actualBinaryOperatorPrecedences = result.GetCurrentBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );
        var actualPrefixUnaryConstructPrecedences =
            result.GetCurrentPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( expectedConstructs );
            actualBinaryOperatorPrecedences.Should().BeEquivalentTo( expectedBinaryOperatorPrecedences );
            actualPrefixUnaryConstructPrecedences.Should().BeEquivalentTo( expectedPrefixUnaryConstructPrecedences );
        }
    }

    [Fact]
    public void AddBigIntBitwiseOperators_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new MathExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("&", typeof( MathExpressionBitwiseAndBigIntOperator )),
            ("|", typeof( MathExpressionBitwiseOrBigIntOperator )),
            ("^", typeof( MathExpressionBitwiseXorBigIntOperator )),
            ("<<", typeof( MathExpressionBitwiseLeftShiftBigIntOperator )),
            (">>", typeof( MathExpressionBitwiseRightShiftBigIntOperator )),
            ("~", typeof( MathExpressionBitwiseNotBigIntOperator ))
        };

        var expectedBinaryOperatorPrecedences = new (string Symbol, int Value)[]
        {
            ("&", 8),
            ("|", 10),
            ("^", 9),
            ("<<", 4),
            (">>", 4)
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("~", 1)
        };

        var result = sut.AddBigIntBitwiseOperators();
        var actualConstructs = result.GetCurrentConstructs().Select( x => (x.Key.ToString(), x.Value.GetType()) );
        var actualBinaryOperatorPrecedences = result.GetCurrentBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );
        var actualPrefixUnaryConstructPrecedences =
            result.GetCurrentPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( expectedConstructs );
            actualBinaryOperatorPrecedences.Should().BeEquivalentTo( expectedBinaryOperatorPrecedences );
            actualPrefixUnaryConstructPrecedences.Should().BeEquivalentTo( expectedPrefixUnaryConstructPrecedences );
        }
    }

    [Fact]
    public void AddBigIntLogicalOperators_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new MathExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("==", typeof( MathExpressionEqualToBigIntOperator )),
            ("!=", typeof( MathExpressionNotEqualToBigIntOperator )),
            (">", typeof( MathExpressionGreaterThanBigIntOperator )),
            ("<", typeof( MathExpressionLessThanBigIntOperator )),
            (">=", typeof( MathExpressionGreaterThanOrEqualToBigIntOperator )),
            ("<=", typeof( MathExpressionLessThanOrEqualToBigIntOperator )),
            ("<=>", typeof( MathExpressionCompareBigIntOperator ))
        };

        var expectedBinaryOperatorPrecedences = new (string Symbol, int Value)[]
        {
            ("==", 7),
            ("!=", 7),
            (">", 6),
            ("<", 6),
            (">=", 6),
            ("<=", 6),
            ("<=>", 5)
        };

        var result = sut.AddBigIntLogicalOperators();
        var actualConstructs = result.GetCurrentConstructs().Select( x => (x.Key.ToString(), x.Value.GetType()) );
        var actualBinaryOperatorPrecedences = result.GetCurrentBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( expectedConstructs );
            actualBinaryOperatorPrecedences.Should().BeEquivalentTo( expectedBinaryOperatorPrecedences );
        }
    }

    [Fact]
    public void AddToBigIntTypeConversion_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new MathExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("[bigint]", typeof( MathExpressionTypeConverter<BigInteger> )),
            ("[bigint]", typeof( MathExpressionTypeConverter<BigInteger, long> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[bigint]", 1)
        };

        var result = sut.AddToBigIntTypeConversion( specializedConverters: new MathExpressionTypeConverter<BigInteger, long>() );
        var actualConstructs = result.GetCurrentConstructs().Select( x => (x.Key.ToString(), x.Value.GetType()) );
        var actualPrefixUnaryConstructPrecedences =
            result.GetCurrentPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( expectedConstructs );
            actualPrefixUnaryConstructPrecedences.Should().BeEquivalentTo( expectedPrefixUnaryConstructPrecedences );
        }
    }

    [Fact]
    public void AddToBigIntTypeConversion_ShouldAddPostfixTypeConverterIfSymbolIsNotNull()
    {
        var sut = new MathExpressionFactoryBuilder().AddToBigIntTypeConversion( postfixSymbol: "I" );

        using ( new AssertionScope() )
        {
            sut.GetCurrentConstructs().Select( c => c.Key.ToString() ).Should().Contain( "I" );
            sut.GetCurrentPostfixUnaryConstructPrecedences().Should().HaveCount( 1 );
        }
    }

    [Fact]
    public void AddStringArithmeticOperators_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new MathExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("+", typeof( MathExpressionAddStringOperator ))
        };

        var expectedBinaryOperatorPrecedences = new (string Symbol, int Value)[]
        {
            ("+", 3)
        };

        var result = sut.AddStringArithmeticOperators();
        var actualConstructs = result.GetCurrentConstructs().Select( x => (x.Key.ToString(), x.Value.GetType()) );
        var actualBinaryOperatorPrecedences = result.GetCurrentBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( expectedConstructs );
            actualBinaryOperatorPrecedences.Should().BeEquivalentTo( expectedBinaryOperatorPrecedences );
        }
    }

    [Fact]
    public void AddStringLogicalOperators_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new MathExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("==", typeof( MathExpressionEqualToStringOperator )),
            ("!=", typeof( MathExpressionNotEqualToStringOperator )),
            ("<=>", typeof( MathExpressionCompareStringOperator ))
        };

        var expectedBinaryOperatorPrecedences = new (string Symbol, int Value)[]
        {
            ("==", 7),
            ("!=", 7),
            ("<=>", 5)
        };

        var result = sut.AddStringLogicalOperators();
        var actualConstructs = result.GetCurrentConstructs().Select( x => (x.Key.ToString(), x.Value.GetType()) );
        var actualBinaryOperatorPrecedences = result.GetCurrentBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( expectedConstructs );
            actualBinaryOperatorPrecedences.Should().BeEquivalentTo( expectedBinaryOperatorPrecedences );
        }
    }

    [Fact]
    public void AddToStringTypeConversion_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new MathExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("[string]", typeof( MathExpressionToStringTypeConverter )),
            ("[string]", typeof( MathExpressionTypeConverter<string, int> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[string]", 1)
        };

        var result = sut.AddToStringTypeConversion( specializedConverters: new MathExpressionTypeConverter<string, int>() );
        var actualConstructs = result.GetCurrentConstructs().Select( x => (x.Key.ToString(), x.Value.GetType()) );
        var actualPrefixUnaryConstructPrecedences =
            result.GetCurrentPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( expectedConstructs );
            actualPrefixUnaryConstructPrecedences.Should().BeEquivalentTo( expectedPrefixUnaryConstructPrecedences );
        }
    }

    [Fact]
    public void AddToStringTypeConversion_ShouldAddPostfixTypeConverterIfSymbolIsNotNull()
    {
        var sut = new MathExpressionFactoryBuilder().AddToStringTypeConversion( postfixSymbol: "S" );

        using ( new AssertionScope() )
        {
            sut.GetCurrentConstructs().Select( c => c.Key.ToString() ).Should().Contain( "S" );
            sut.GetCurrentPostfixUnaryConstructPrecedences().Should().HaveCount( 1 );
        }
    }
}
