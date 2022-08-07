using System.Linq;
using System.Numerics;
using FluentAssertions.Execution;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Computable.Expressions.Constructs.BigInt;
using LfrlAnvil.Computable.Expressions.Constructs.Boolean;
using LfrlAnvil.Computable.Expressions.Constructs.Decimal;
using LfrlAnvil.Computable.Expressions.Constructs.Double;
using LfrlAnvil.Computable.Expressions.Constructs.Float;
using LfrlAnvil.Computable.Expressions.Constructs.Int32;
using LfrlAnvil.Computable.Expressions.Constructs.Int64;
using LfrlAnvil.Computable.Expressions.Constructs.String;
using LfrlAnvil.Computable.Expressions.Extensions;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Computable.Expressions.Tests.ExtensionsTests.ParsedExpressionFactoryBuilderTests;

public class ParsedExpressionFactoryBuilderExtensionsTests : TestsBase
{
    [Fact]
    public void AddGenericArithmeticOperators_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("+", typeof( ParsedExpressionAddOperator )),
            ("-", typeof( ParsedExpressionSubtractOperator )),
            ("*", typeof( ParsedExpressionMultiplyOperator )),
            ("/", typeof( ParsedExpressionDivideOperator )),
            ("mod", typeof( ParsedExpressionModuloOperator )),
            ("-", typeof( ParsedExpressionNegateOperator ))
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
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("&", typeof( ParsedExpressionBitwiseAndOperator )),
            ("|", typeof( ParsedExpressionBitwiseOrOperator )),
            ("^", typeof( ParsedExpressionBitwiseXorOperator )),
            ("<<", typeof( ParsedExpressionBitwiseLeftShiftOperator )),
            (">>", typeof( ParsedExpressionBitwiseRightShiftOperator )),
            ("~", typeof( ParsedExpressionBitwiseNotOperator ))
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
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("??", typeof( ParsedExpressionCoalesceOperator )),
            ("==", typeof( ParsedExpressionEqualToOperator )),
            ("!=", typeof( ParsedExpressionNotEqualToOperator )),
            (">", typeof( ParsedExpressionGreaterThanOperator )),
            ("<", typeof( ParsedExpressionLessThanOperator )),
            (">=", typeof( ParsedExpressionGreaterThanOrEqualToOperator )),
            ("<=", typeof( ParsedExpressionLessThanOrEqualToOperator )),
            ("<=>", typeof( ParsedExpressionCompareOperator ))
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
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("&", typeof( ParsedExpressionBitwiseAndBooleanOperator )),
            ("|", typeof( ParsedExpressionBitwiseOrBooleanOperator )),
            ("^", typeof( ParsedExpressionBitwiseXorBooleanOperator ))
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
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("==", typeof( ParsedExpressionEqualToBooleanOperator )),
            ("!=", typeof( ParsedExpressionNotEqualToBooleanOperator )),
            ("<=>", typeof( ParsedExpressionCompareBooleanOperator )),
            ("and", typeof( ParsedExpressionAndOperator )),
            ("or", typeof( ParsedExpressionOrOperator )),
            ("not", typeof( ParsedExpressionNotOperator ))
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
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("[boolean]", typeof( ParsedExpressionTypeConverter<bool> )),
            ("[boolean]", typeof( ParsedExpressionTypeConverter<bool, int> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[boolean]", 1)
        };

        var result = sut.AddToBooleanTypeConversion( specializedConverters: new ParsedExpressionTypeConverter<bool, int>() );
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
        var sut = new ParsedExpressionFactoryBuilder().AddToBooleanTypeConversion( postfixSymbol: "B" );

        using ( new AssertionScope() )
        {
            sut.GetCurrentConstructs().Select( c => c.Key.ToString() ).Should().Contain( "B" );
            sut.GetCurrentPostfixUnaryConstructPrecedences().Should().HaveCount( 1 );
        }
    }

    [Fact]
    public void AddDecimalArithmeticOperators_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("+", typeof( ParsedExpressionAddDecimalOperator )),
            ("-", typeof( ParsedExpressionSubtractDecimalOperator )),
            ("*", typeof( ParsedExpressionMultiplyDecimalOperator )),
            ("/", typeof( ParsedExpressionDivideDecimalOperator )),
            ("mod", typeof( ParsedExpressionModuloDecimalOperator )),
            ("-", typeof( ParsedExpressionNegateDecimalOperator ))
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
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("==", typeof( ParsedExpressionEqualToDecimalOperator )),
            ("!=", typeof( ParsedExpressionNotEqualToDecimalOperator )),
            (">", typeof( ParsedExpressionGreaterThanDecimalOperator )),
            ("<", typeof( ParsedExpressionLessThanDecimalOperator )),
            (">=", typeof( ParsedExpressionGreaterThanOrEqualToDecimalOperator )),
            ("<=", typeof( ParsedExpressionLessThanOrEqualToDecimalOperator )),
            ("<=>", typeof( ParsedExpressionCompareDecimalOperator ))
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
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("[decimal]", typeof( ParsedExpressionTypeConverter<decimal> )),
            ("[decimal]", typeof( ParsedExpressionTypeConverter<decimal, double> )),
            ("M", typeof( ParsedExpressionTypeConverter<decimal> )),
            ("M", typeof( ParsedExpressionTypeConverter<decimal, double> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[decimal]", 1)
        };

        var expectedPostfixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("M", 1)
        };

        var result = sut.AddToDecimalTypeConversion( specializedConverters: new ParsedExpressionTypeConverter<decimal, double>() );
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
        var sut = new ParsedExpressionFactoryBuilder().AddToDecimalTypeConversion( postfixSymbol: null );

        using ( new AssertionScope() )
        {
            sut.GetCurrentConstructs().Should().HaveCount( 1 );
            sut.GetCurrentPostfixUnaryConstructPrecedences().Should().BeEmpty();
        }
    }

    [Fact]
    public void AddDoubleArithmeticOperators_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("+", typeof( ParsedExpressionAddDoubleOperator )),
            ("-", typeof( ParsedExpressionSubtractDoubleOperator )),
            ("*", typeof( ParsedExpressionMultiplyDoubleOperator )),
            ("/", typeof( ParsedExpressionDivideDoubleOperator )),
            ("mod", typeof( ParsedExpressionModuloDoubleOperator )),
            ("-", typeof( ParsedExpressionNegateDoubleOperator ))
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
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("==", typeof( ParsedExpressionEqualToDoubleOperator )),
            ("!=", typeof( ParsedExpressionNotEqualToDoubleOperator )),
            (">", typeof( ParsedExpressionGreaterThanDoubleOperator )),
            ("<", typeof( ParsedExpressionLessThanDoubleOperator )),
            (">=", typeof( ParsedExpressionGreaterThanOrEqualToDoubleOperator )),
            ("<=", typeof( ParsedExpressionLessThanOrEqualToDoubleOperator )),
            ("<=>", typeof( ParsedExpressionCompareDoubleOperator ))
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
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("[double]", typeof( ParsedExpressionTypeConverter<double> )),
            ("[double]", typeof( ParsedExpressionTypeConverter<double, decimal> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[double]", 1)
        };

        var result = sut.AddToDoubleTypeConversion( specializedConverters: new ParsedExpressionTypeConverter<double, decimal>() );
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
        var sut = new ParsedExpressionFactoryBuilder().AddToDoubleTypeConversion( postfixSymbol: "D" );

        using ( new AssertionScope() )
        {
            sut.GetCurrentConstructs().Select( c => c.Key.ToString() ).Should().Contain( "D" );
            sut.GetCurrentPostfixUnaryConstructPrecedences().Should().HaveCount( 1 );
        }
    }

    [Fact]
    public void AddFloatArithmeticOperators_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("+", typeof( ParsedExpressionAddFloatOperator )),
            ("-", typeof( ParsedExpressionSubtractFloatOperator )),
            ("*", typeof( ParsedExpressionMultiplyFloatOperator )),
            ("/", typeof( ParsedExpressionDivideFloatOperator )),
            ("mod", typeof( ParsedExpressionModuloFloatOperator )),
            ("-", typeof( ParsedExpressionNegateFloatOperator ))
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
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("==", typeof( ParsedExpressionEqualToFloatOperator )),
            ("!=", typeof( ParsedExpressionNotEqualToFloatOperator )),
            (">", typeof( ParsedExpressionGreaterThanFloatOperator )),
            ("<", typeof( ParsedExpressionLessThanFloatOperator )),
            (">=", typeof( ParsedExpressionGreaterThanOrEqualToFloatOperator )),
            ("<=", typeof( ParsedExpressionLessThanOrEqualToFloatOperator )),
            ("<=>", typeof( ParsedExpressionCompareFloatOperator ))
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
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("[float]", typeof( ParsedExpressionTypeConverter<float> )),
            ("[float]", typeof( ParsedExpressionTypeConverter<float, decimal> )),
            ("F", typeof( ParsedExpressionTypeConverter<float> )),
            ("F", typeof( ParsedExpressionTypeConverter<float, decimal> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[float]", 1)
        };

        var expectedPostfixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("F", 1)
        };

        var result = sut.AddToFloatTypeConversion( specializedConverters: new ParsedExpressionTypeConverter<float, decimal>() );
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
        var sut = new ParsedExpressionFactoryBuilder().AddToFloatTypeConversion( postfixSymbol: null );

        using ( new AssertionScope() )
        {
            sut.GetCurrentConstructs().Should().HaveCount( 1 );
            sut.GetCurrentPostfixUnaryConstructPrecedences().Should().BeEmpty();
        }
    }

    [Fact]
    public void AddInt32ArithmeticOperators_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("+", typeof( ParsedExpressionAddInt32Operator )),
            ("-", typeof( ParsedExpressionSubtractInt32Operator )),
            ("*", typeof( ParsedExpressionMultiplyInt32Operator )),
            ("/", typeof( ParsedExpressionDivideInt32Operator )),
            ("mod", typeof( ParsedExpressionModuloInt32Operator )),
            ("-", typeof( ParsedExpressionNegateInt32Operator ))
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
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("&", typeof( ParsedExpressionBitwiseAndInt32Operator )),
            ("|", typeof( ParsedExpressionBitwiseOrInt32Operator )),
            ("^", typeof( ParsedExpressionBitwiseXorInt32Operator )),
            ("<<", typeof( ParsedExpressionBitwiseLeftShiftInt32Operator )),
            (">>", typeof( ParsedExpressionBitwiseRightShiftInt32Operator )),
            ("~", typeof( ParsedExpressionBitwiseNotInt32Operator ))
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
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("==", typeof( ParsedExpressionEqualToInt32Operator )),
            ("!=", typeof( ParsedExpressionNotEqualToInt32Operator )),
            (">", typeof( ParsedExpressionGreaterThanInt32Operator )),
            ("<", typeof( ParsedExpressionLessThanInt32Operator )),
            (">=", typeof( ParsedExpressionGreaterThanOrEqualToInt32Operator )),
            ("<=", typeof( ParsedExpressionLessThanOrEqualToInt32Operator )),
            ("<=>", typeof( ParsedExpressionCompareInt32Operator ))
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
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("[int32]", typeof( ParsedExpressionTypeConverter<int> )),
            ("[int32]", typeof( ParsedExpressionTypeConverter<int, long> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[int32]", 1)
        };

        var result = sut.AddToInt32TypeConversion( specializedConverters: new ParsedExpressionTypeConverter<int, long>() );
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
        var sut = new ParsedExpressionFactoryBuilder().AddToInt32TypeConversion( postfixSymbol: "I" );

        using ( new AssertionScope() )
        {
            sut.GetCurrentConstructs().Select( c => c.Key.ToString() ).Should().Contain( "I" );
            sut.GetCurrentPostfixUnaryConstructPrecedences().Should().HaveCount( 1 );
        }
    }

    [Fact]
    public void AddInt64ArithmeticOperators_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("+", typeof( ParsedExpressionAddInt64Operator )),
            ("-", typeof( ParsedExpressionSubtractInt64Operator )),
            ("*", typeof( ParsedExpressionMultiplyInt64Operator )),
            ("/", typeof( ParsedExpressionDivideInt64Operator )),
            ("mod", typeof( ParsedExpressionModuloInt64Operator )),
            ("-", typeof( ParsedExpressionNegateInt64Operator ))
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
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("&", typeof( ParsedExpressionBitwiseAndInt64Operator )),
            ("|", typeof( ParsedExpressionBitwiseOrInt64Operator )),
            ("^", typeof( ParsedExpressionBitwiseXorInt64Operator )),
            ("<<", typeof( ParsedExpressionBitwiseLeftShiftInt64Operator )),
            (">>", typeof( ParsedExpressionBitwiseRightShiftInt64Operator )),
            ("~", typeof( ParsedExpressionBitwiseNotInt64Operator ))
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
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("==", typeof( ParsedExpressionEqualToInt64Operator )),
            ("!=", typeof( ParsedExpressionNotEqualToInt64Operator )),
            (">", typeof( ParsedExpressionGreaterThanInt64Operator )),
            ("<", typeof( ParsedExpressionLessThanInt64Operator )),
            (">=", typeof( ParsedExpressionGreaterThanOrEqualToInt64Operator )),
            ("<=", typeof( ParsedExpressionLessThanOrEqualToInt64Operator )),
            ("<=>", typeof( ParsedExpressionCompareInt64Operator ))
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
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("[int64]", typeof( ParsedExpressionTypeConverter<long> )),
            ("[int64]", typeof( ParsedExpressionTypeConverter<long, int> )),
            ("L", typeof( ParsedExpressionTypeConverter<long> )),
            ("L", typeof( ParsedExpressionTypeConverter<long, int> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[int64]", 1)
        };

        var expectedPostfixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("L", 1)
        };

        var result = sut.AddToInt64TypeConversion( specializedConverters: new ParsedExpressionTypeConverter<long, int>() );
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
        var sut = new ParsedExpressionFactoryBuilder().AddToInt64TypeConversion( postfixSymbol: null );

        using ( new AssertionScope() )
        {
            sut.GetCurrentConstructs().Should().HaveCount( 1 );
            sut.GetCurrentPostfixUnaryConstructPrecedences().Should().BeEmpty();
        }
    }

    [Fact]
    public void AddBigIntArithmeticOperators_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("+", typeof( ParsedExpressionAddBigIntOperator )),
            ("-", typeof( ParsedExpressionSubtractBigIntOperator )),
            ("*", typeof( ParsedExpressionMultiplyBigIntOperator )),
            ("/", typeof( ParsedExpressionDivideBigIntOperator )),
            ("mod", typeof( ParsedExpressionModuloBigIntOperator )),
            ("-", typeof( ParsedExpressionNegateBigIntOperator ))
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
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("&", typeof( ParsedExpressionBitwiseAndBigIntOperator )),
            ("|", typeof( ParsedExpressionBitwiseOrBigIntOperator )),
            ("^", typeof( ParsedExpressionBitwiseXorBigIntOperator )),
            ("<<", typeof( ParsedExpressionBitwiseLeftShiftBigIntOperator )),
            (">>", typeof( ParsedExpressionBitwiseRightShiftBigIntOperator )),
            ("~", typeof( ParsedExpressionBitwiseNotBigIntOperator ))
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
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("==", typeof( ParsedExpressionEqualToBigIntOperator )),
            ("!=", typeof( ParsedExpressionNotEqualToBigIntOperator )),
            (">", typeof( ParsedExpressionGreaterThanBigIntOperator )),
            ("<", typeof( ParsedExpressionLessThanBigIntOperator )),
            (">=", typeof( ParsedExpressionGreaterThanOrEqualToBigIntOperator )),
            ("<=", typeof( ParsedExpressionLessThanOrEqualToBigIntOperator )),
            ("<=>", typeof( ParsedExpressionCompareBigIntOperator ))
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
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("[bigint]", typeof( ParsedExpressionTypeConverter<BigInteger> )),
            ("[bigint]", typeof( ParsedExpressionTypeConverter<BigInteger, long> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[bigint]", 1)
        };

        var result = sut.AddToBigIntTypeConversion( specializedConverters: new ParsedExpressionTypeConverter<BigInteger, long>() );
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
        var sut = new ParsedExpressionFactoryBuilder().AddToBigIntTypeConversion( postfixSymbol: "I" );

        using ( new AssertionScope() )
        {
            sut.GetCurrentConstructs().Select( c => c.Key.ToString() ).Should().Contain( "I" );
            sut.GetCurrentPostfixUnaryConstructPrecedences().Should().HaveCount( 1 );
        }
    }

    [Fact]
    public void AddStringArithmeticOperators_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("+", typeof( ParsedExpressionAddStringOperator ))
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
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("==", typeof( ParsedExpressionEqualToStringOperator )),
            ("!=", typeof( ParsedExpressionNotEqualToStringOperator )),
            ("<=>", typeof( ParsedExpressionCompareStringOperator ))
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
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("[string]", typeof( ParsedExpressionToStringTypeConverter )),
            ("[string]", typeof( ParsedExpressionTypeConverter<string, int> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[string]", 1)
        };

        var result = sut.AddToStringTypeConversion( specializedConverters: new ParsedExpressionTypeConverter<string, int>() );
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
        var sut = new ParsedExpressionFactoryBuilder().AddToStringTypeConversion( postfixSymbol: "S" );

        using ( new AssertionScope() )
        {
            sut.GetCurrentConstructs().Select( c => c.Key.ToString() ).Should().Contain( "S" );
            sut.GetCurrentPostfixUnaryConstructPrecedences().Should().HaveCount( 1 );
        }
    }
}
