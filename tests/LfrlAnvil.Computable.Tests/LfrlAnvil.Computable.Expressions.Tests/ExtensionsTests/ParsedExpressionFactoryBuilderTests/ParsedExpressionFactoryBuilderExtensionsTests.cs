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
        var actualConstructs = result.GetCurrentConstructs()
            .Select( x => (x.Key.ToString(), x.Value is Type ? typeof( Type ) : x.Value.GetType()) );

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
        var actualConstructs = result.GetCurrentConstructs()
            .Select( x => (x.Key.ToString(), x.Value is Type ? typeof( Type ) : x.Value.GetType()) );

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
        var actualConstructs = result.GetCurrentConstructs()
            .Select( x => (x.Key.ToString(), x.Value is Type ? typeof( Type ) : x.Value.GetType()) );

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
        var actualConstructs = result.GetCurrentConstructs()
            .Select( x => (x.Key.ToString(), x.Value is Type ? typeof( Type ) : x.Value.GetType()) );

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
        var actualConstructs = result.GetCurrentConstructs()
            .Select( x => (x.Key.ToString(), x.Value is Type ? typeof( Type ) : x.Value.GetType()) );

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
    public void AddBooleanTypeDefinition_WithDefaultSymbols_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("boolean", typeof( Type )),
            ("[boolean]", typeof( ParsedExpressionTypeConverter<bool> )),
            ("[boolean]", typeof( ParsedExpressionTypeConverter<bool, int> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[boolean]", 1)
        };

        var expectedPostfixUnaryConstructPrecedences = Array.Empty<(string Symbol, int Value)>();

        var result = sut.AddBooleanTypeDefinition( new ParsedExpressionTypeConverter<bool, int>() );
        var actualConstructs = result.GetCurrentConstructs()
            .Select( x => (x.Key.ToString(), x.Value is Type ? typeof( Type ) : x.Value.GetType()) );

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
    public void AddBooleanTypeDefinition_WithCustomSymbols_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder();
        var symbols = new ParsedExpressionTypeDefinitionSymbols()
            .SetName( "b" )
            .SetPostfixTypeConverter( "B" );

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("b", typeof( Type )),
            ("[b]", typeof( ParsedExpressionTypeConverter<bool> )),
            ("[b]", typeof( ParsedExpressionTypeConverter<bool, int> )),
            ("B", typeof( ParsedExpressionTypeConverter<bool> )),
            ("B", typeof( ParsedExpressionTypeConverter<bool, int> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[b]", 1)
        };

        var expectedPostfixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("B", 1)
        };

        var result = sut.AddBooleanTypeDefinition( symbols, new ParsedExpressionTypeConverter<bool, int>() );
        var actualConstructs = result.GetCurrentConstructs()
            .Select( x => (x.Key.ToString(), x.Value is Type ? typeof( Type ) : x.Value.GetType()) );

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
    public void AddBooleanTypeDefinition_WithOnlyName_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder();
        var symbols = new ParsedExpressionTypeDefinitionSymbols()
            .SetName( "b" )
            .DisablePrefixTypeConverter();

        var result = sut.AddBooleanTypeDefinition( symbols );
        var actualConstructs = result.GetCurrentConstructs().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( ("b", typeof( bool )) );
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
        var actualConstructs = result.GetCurrentConstructs()
            .Select( x => (x.Key.ToString(), x.Value is Type ? typeof( Type ) : x.Value.GetType()) );

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
        var actualConstructs = result.GetCurrentConstructs()
            .Select( x => (x.Key.ToString(), x.Value is Type ? typeof( Type ) : x.Value.GetType()) );

        var actualBinaryOperatorPrecedences = result.GetCurrentBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( expectedConstructs );
            actualBinaryOperatorPrecedences.Should().BeEquivalentTo( expectedBinaryOperatorPrecedences );
        }
    }

    [Fact]
    public void AddDecimalTypeDefinition_WithDefaultSymbols_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("decimal", typeof( Type )),
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

        var result = sut.AddDecimalTypeDefinition( new ParsedExpressionTypeConverter<decimal, double>() );
        var actualConstructs = result.GetCurrentConstructs()
            .Select( x => (x.Key.ToString(), x.Value is Type ? typeof( Type ) : x.Value.GetType()) );

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
    public void AddDecimalTypeDefinition_WithCustomSymbols_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder();
        var symbols = new ParsedExpressionTypeDefinitionSymbols()
            .SetName( "d" );

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("d", typeof( Type )),
            ("[d]", typeof( ParsedExpressionTypeConverter<decimal> )),
            ("[d]", typeof( ParsedExpressionTypeConverter<decimal, double> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[d]", 1)
        };

        var expectedPostfixUnaryConstructPrecedences = Array.Empty<(string Symbol, int Value)>();

        var result = sut.AddDecimalTypeDefinition( symbols, new ParsedExpressionTypeConverter<decimal, double>() );
        var actualConstructs = result.GetCurrentConstructs()
            .Select( x => (x.Key.ToString(), x.Value is Type ? typeof( Type ) : x.Value.GetType()) );

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
    public void AddDecimalTypeDefinition_WithOnlyName_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder();
        var symbols = new ParsedExpressionTypeDefinitionSymbols()
            .SetName( "d" )
            .DisablePrefixTypeConverter();

        var result = sut.AddDecimalTypeDefinition( symbols );
        var actualConstructs = result.GetCurrentConstructs().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( ("d", typeof( decimal )) );
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
        var actualConstructs = result.GetCurrentConstructs()
            .Select( x => (x.Key.ToString(), x.Value is Type ? typeof( Type ) : x.Value.GetType()) );

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
        var actualConstructs = result.GetCurrentConstructs()
            .Select( x => (x.Key.ToString(), x.Value is Type ? typeof( Type ) : x.Value.GetType()) );

        var actualBinaryOperatorPrecedences = result.GetCurrentBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( expectedConstructs );
            actualBinaryOperatorPrecedences.Should().BeEquivalentTo( expectedBinaryOperatorPrecedences );
        }
    }

    [Fact]
    public void AddDoubleTypeDefinition_WithDefaultSymbols_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("double", typeof( Type )),
            ("[double]", typeof( ParsedExpressionTypeConverter<double> )),
            ("[double]", typeof( ParsedExpressionTypeConverter<double, decimal> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[double]", 1)
        };

        var expectedPostfixUnaryConstructPrecedences = Array.Empty<(string Symbol, int Value)>();

        var result = sut.AddDoubleTypeDefinition( new ParsedExpressionTypeConverter<double, decimal>() );
        var actualConstructs = result.GetCurrentConstructs()
            .Select( x => (x.Key.ToString(), x.Value is Type ? typeof( Type ) : x.Value.GetType()) );

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
    public void AddDoubleTypeDefinition_WithCustomSymbols_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder();
        var symbols = new ParsedExpressionTypeDefinitionSymbols()
            .SetName( "d" )
            .SetPostfixTypeConverter( "D" );

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("d", typeof( Type )),
            ("[d]", typeof( ParsedExpressionTypeConverter<double> )),
            ("[d]", typeof( ParsedExpressionTypeConverter<double, decimal> )),
            ("D", typeof( ParsedExpressionTypeConverter<double> )),
            ("D", typeof( ParsedExpressionTypeConverter<double, decimal> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[d]", 1)
        };

        var expectedPostfixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("D", 1)
        };

        var result = sut.AddDoubleTypeDefinition( symbols, new ParsedExpressionTypeConverter<double, decimal>() );
        var actualConstructs = result.GetCurrentConstructs()
            .Select( x => (x.Key.ToString(), x.Value is Type ? typeof( Type ) : x.Value.GetType()) );

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
    public void AddDoubleTypeDefinition_WithOnlyName_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder();
        var symbols = new ParsedExpressionTypeDefinitionSymbols()
            .SetName( "d" )
            .DisablePrefixTypeConverter();

        var result = sut.AddDoubleTypeDefinition( symbols );
        var actualConstructs = result.GetCurrentConstructs().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( ("d", typeof( double )) );
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
        var actualConstructs = result.GetCurrentConstructs()
            .Select( x => (x.Key.ToString(), x.Value is Type ? typeof( Type ) : x.Value.GetType()) );

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
        var actualConstructs = result.GetCurrentConstructs()
            .Select( x => (x.Key.ToString(), x.Value is Type ? typeof( Type ) : x.Value.GetType()) );

        var actualBinaryOperatorPrecedences = result.GetCurrentBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( expectedConstructs );
            actualBinaryOperatorPrecedences.Should().BeEquivalentTo( expectedBinaryOperatorPrecedences );
        }
    }

    [Fact]
    public void AddFloatTypeDefinition_WithDefaultSymbols_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("float", typeof( Type )),
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

        var result = sut.AddFloatTypeDefinition( new ParsedExpressionTypeConverter<float, decimal>() );
        var actualConstructs = result.GetCurrentConstructs()
            .Select( x => (x.Key.ToString(), x.Value is Type ? typeof( Type ) : x.Value.GetType()) );

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
    public void AddFloatTypeDefinition_WithCustomSymbols_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder();
        var symbols = new ParsedExpressionTypeDefinitionSymbols()
            .SetName( "f" );

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("f", typeof( Type )),
            ("[f]", typeof( ParsedExpressionTypeConverter<float> )),
            ("[f]", typeof( ParsedExpressionTypeConverter<float, decimal> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[f]", 1)
        };

        var expectedPostfixUnaryConstructPrecedences = Array.Empty<(string Symbol, int Value)>();

        var result = sut.AddFloatTypeDefinition( symbols, new ParsedExpressionTypeConverter<float, decimal>() );
        var actualConstructs = result.GetCurrentConstructs()
            .Select( x => (x.Key.ToString(), x.Value is Type ? typeof( Type ) : x.Value.GetType()) );

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
    public void AddFloatTypeDefinition_WithOnlyName_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder();
        var symbols = new ParsedExpressionTypeDefinitionSymbols()
            .SetName( "f" )
            .DisablePrefixTypeConverter();

        var result = sut.AddFloatTypeDefinition( symbols );
        var actualConstructs = result.GetCurrentConstructs().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( ("f", typeof( float )) );
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
        var actualConstructs = result.GetCurrentConstructs()
            .Select( x => (x.Key.ToString(), x.Value is Type ? typeof( Type ) : x.Value.GetType()) );

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
        var actualConstructs = result.GetCurrentConstructs()
            .Select( x => (x.Key.ToString(), x.Value is Type ? typeof( Type ) : x.Value.GetType()) );

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
        var actualConstructs = result.GetCurrentConstructs()
            .Select( x => (x.Key.ToString(), x.Value is Type ? typeof( Type ) : x.Value.GetType()) );

        var actualBinaryOperatorPrecedences = result.GetCurrentBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( expectedConstructs );
            actualBinaryOperatorPrecedences.Should().BeEquivalentTo( expectedBinaryOperatorPrecedences );
        }
    }

    [Fact]
    public void AddInt32TypeDefinition_WithDefaultSymbols_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("int32", typeof( Type )),
            ("[int32]", typeof( ParsedExpressionTypeConverter<int> )),
            ("[int32]", typeof( ParsedExpressionTypeConverter<int, long> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[int32]", 1)
        };

        var expectedPostfixUnaryConstructPrecedences = Array.Empty<(string Symbol, int Value)>();

        var result = sut.AddInt32TypeDefinition( new ParsedExpressionTypeConverter<int, long>() );
        var actualConstructs = result.GetCurrentConstructs()
            .Select( x => (x.Key.ToString(), x.Value is Type ? typeof( Type ) : x.Value.GetType()) );

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
    public void AddInt32TypeDefinition_WithCustomSymbols_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder();
        var symbols = new ParsedExpressionTypeDefinitionSymbols()
            .SetName( "i" )
            .SetPostfixTypeConverter( "I" );

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("i", typeof( Type )),
            ("[i]", typeof( ParsedExpressionTypeConverter<int> )),
            ("[i]", typeof( ParsedExpressionTypeConverter<int, long> )),
            ("I", typeof( ParsedExpressionTypeConverter<int> )),
            ("I", typeof( ParsedExpressionTypeConverter<int, long> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[i]", 1)
        };

        var expectedPostfixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("I", 1)
        };

        var result = sut.AddInt32TypeDefinition( symbols, new ParsedExpressionTypeConverter<int, long>() );
        var actualConstructs = result.GetCurrentConstructs()
            .Select( x => (x.Key.ToString(), x.Value is Type ? typeof( Type ) : x.Value.GetType()) );

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
    public void AddInt32TypeDefinition_WithOnlyName_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder();
        var symbols = new ParsedExpressionTypeDefinitionSymbols()
            .SetName( "i" )
            .DisablePrefixTypeConverter();

        var result = sut.AddInt32TypeDefinition( symbols );
        var actualConstructs = result.GetCurrentConstructs().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( ("i", typeof( int )) );
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
        var actualConstructs = result.GetCurrentConstructs()
            .Select( x => (x.Key.ToString(), x.Value is Type ? typeof( Type ) : x.Value.GetType()) );

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
        var actualConstructs = result.GetCurrentConstructs()
            .Select( x => (x.Key.ToString(), x.Value is Type ? typeof( Type ) : x.Value.GetType()) );

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
        var actualConstructs = result.GetCurrentConstructs()
            .Select( x => (x.Key.ToString(), x.Value is Type ? typeof( Type ) : x.Value.GetType()) );

        var actualBinaryOperatorPrecedences = result.GetCurrentBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( expectedConstructs );
            actualBinaryOperatorPrecedences.Should().BeEquivalentTo( expectedBinaryOperatorPrecedences );
        }
    }

    [Fact]
    public void AddInt64TypeDefinition_WithDefaultSymbols_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("int64", typeof( Type )),
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

        var result = sut.AddInt64TypeDefinition( new ParsedExpressionTypeConverter<long, int>() );
        var actualConstructs = result.GetCurrentConstructs()
            .Select( x => (x.Key.ToString(), x.Value is Type ? typeof( Type ) : x.Value.GetType()) );

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
    public void AddInt64TypeDefinition_WithCustomSymbols_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder();
        var symbols = new ParsedExpressionTypeDefinitionSymbols()
            .SetName( "i" );

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("i", typeof( Type )),
            ("[i]", typeof( ParsedExpressionTypeConverter<long> )),
            ("[i]", typeof( ParsedExpressionTypeConverter<long, int> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[i]", 1)
        };

        var expectedPostfixUnaryConstructPrecedences = Array.Empty<(string Symbol, int Value)>();

        var result = sut.AddInt64TypeDefinition( symbols, new ParsedExpressionTypeConverter<long, int>() );
        var actualConstructs = result.GetCurrentConstructs()
            .Select( x => (x.Key.ToString(), x.Value is Type ? typeof( Type ) : x.Value.GetType()) );

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
    public void AddInt64TypeDefinition_WithOnlyName_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder();
        var symbols = new ParsedExpressionTypeDefinitionSymbols()
            .SetName( "i" )
            .DisablePrefixTypeConverter();

        var result = sut.AddInt64TypeDefinition( symbols );
        var actualConstructs = result.GetCurrentConstructs().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( ("i", typeof( long )) );
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
        var actualConstructs = result.GetCurrentConstructs()
            .Select( x => (x.Key.ToString(), x.Value is Type ? typeof( Type ) : x.Value.GetType()) );

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
        var actualConstructs = result.GetCurrentConstructs()
            .Select( x => (x.Key.ToString(), x.Value is Type ? typeof( Type ) : x.Value.GetType()) );

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
        var actualConstructs = result.GetCurrentConstructs()
            .Select( x => (x.Key.ToString(), x.Value is Type ? typeof( Type ) : x.Value.GetType()) );

        var actualBinaryOperatorPrecedences = result.GetCurrentBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( expectedConstructs );
            actualBinaryOperatorPrecedences.Should().BeEquivalentTo( expectedBinaryOperatorPrecedences );
        }
    }

    [Fact]
    public void AddBigIntTypeDefinition_WithDefaultSymbols_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("bigint", typeof( Type )),
            ("[bigint]", typeof( ParsedExpressionTypeConverter<BigInteger> )),
            ("[bigint]", typeof( ParsedExpressionTypeConverter<BigInteger, long> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[bigint]", 1)
        };

        var expectedPostfixUnaryConstructPrecedences = Array.Empty<(string Symbol, int Value)>();

        var result = sut.AddBigIntTypeDefinition( new ParsedExpressionTypeConverter<BigInteger, long>() );
        var actualConstructs = result.GetCurrentConstructs()
            .Select( x => (x.Key.ToString(), x.Value is Type ? typeof( Type ) : x.Value.GetType()) );

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
    public void AddBigIntTypeDefinition_WithCustomSymbols_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder();
        var symbols = new ParsedExpressionTypeDefinitionSymbols()
            .SetName( "b" )
            .SetPostfixTypeConverter( "B" );

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("b", typeof( Type )),
            ("[b]", typeof( ParsedExpressionTypeConverter<BigInteger> )),
            ("[b]", typeof( ParsedExpressionTypeConverter<BigInteger, long> )),
            ("B", typeof( ParsedExpressionTypeConverter<BigInteger> )),
            ("B", typeof( ParsedExpressionTypeConverter<BigInteger, long> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[b]", 1)
        };

        var expectedPostfixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("B", 1)
        };

        var result = sut.AddBigIntTypeDefinition( symbols, new ParsedExpressionTypeConverter<BigInteger, long>() );
        var actualConstructs = result.GetCurrentConstructs()
            .Select( x => (x.Key.ToString(), x.Value is Type ? typeof( Type ) : x.Value.GetType()) );

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
    public void AddBigIntTypeDefinition_WithOnlyName_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder();
        var symbols = new ParsedExpressionTypeDefinitionSymbols()
            .SetName( "b" )
            .DisablePrefixTypeConverter();

        var result = sut.AddBigIntTypeDefinition( symbols );
        var actualConstructs = result.GetCurrentConstructs().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( ("b", typeof( BigInteger )) );
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
        var actualConstructs = result.GetCurrentConstructs()
            .Select( x => (x.Key.ToString(), x.Value is Type ? typeof( Type ) : x.Value.GetType()) );

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
        var actualConstructs = result.GetCurrentConstructs()
            .Select( x => (x.Key.ToString(), x.Value is Type ? typeof( Type ) : x.Value.GetType()) );

        var actualBinaryOperatorPrecedences = result.GetCurrentBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( expectedConstructs );
            actualBinaryOperatorPrecedences.Should().BeEquivalentTo( expectedBinaryOperatorPrecedences );
        }
    }

    [Fact]
    public void AddStringTypeDefinition_WithDefaultSymbols_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("string", typeof( Type )),
            ("[string]", typeof( ParsedExpressionToStringTypeConverter )),
            ("[string]", typeof( ParsedExpressionTypeConverter<string, int> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[string]", 1)
        };

        var expectedPostfixUnaryConstructPrecedences = Array.Empty<(string Symbol, int Value)>();

        var result = sut.AddStringTypeDefinition( new ParsedExpressionTypeConverter<string, int>() );
        var actualConstructs = result.GetCurrentConstructs()
            .Select( x => (x.Key.ToString(), x.Value is Type ? typeof( Type ) : x.Value.GetType()) );

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
    public void AddStringTypeDefinition_WithCustomSymbols_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder();
        var symbols = new ParsedExpressionTypeDefinitionSymbols()
            .SetName( "s" )
            .SetPostfixTypeConverter( "S" );

        var expectedConstructs = new (string Symbol, Type Type)[]
        {
            ("s", typeof( Type )),
            ("[s]", typeof( ParsedExpressionToStringTypeConverter )),
            ("[s]", typeof( ParsedExpressionTypeConverter<string, int> )),
            ("S", typeof( ParsedExpressionToStringTypeConverter )),
            ("S", typeof( ParsedExpressionTypeConverter<string, int> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[s]", 1)
        };

        var expectedPostfixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("S", 1)
        };

        var result = sut.AddStringTypeDefinition( symbols, new ParsedExpressionTypeConverter<string, int>() );
        var actualConstructs = result.GetCurrentConstructs()
            .Select( x => (x.Key.ToString(), x.Value is Type ? typeof( Type ) : x.Value.GetType()) );

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
    public void AddStringTypeDefinition_WithOnlyName_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder();
        var symbols = new ParsedExpressionTypeDefinitionSymbols()
            .SetName( "s" )
            .DisablePrefixTypeConverter();

        var result = sut.AddStringTypeDefinition( symbols );
        var actualConstructs = result.GetCurrentConstructs().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( ("s", typeof( string )) );
        }
    }
}
