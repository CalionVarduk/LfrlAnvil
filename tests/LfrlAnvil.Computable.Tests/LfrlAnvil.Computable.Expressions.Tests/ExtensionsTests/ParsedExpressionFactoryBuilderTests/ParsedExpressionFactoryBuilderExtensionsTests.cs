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
using LfrlAnvil.Computable.Expressions.Constructs.Variadic;
using LfrlAnvil.Computable.Expressions.Extensions;
using LfrlAnvil.Computable.Expressions.Internal;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Computable.Expressions.Tests.ExtensionsTests.ParsedExpressionFactoryBuilderTests;

public class ParsedExpressionFactoryBuilderExtensionsTests : TestsBase
{
    [Fact]
    public void AddGenericArithmeticOperators_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, ParsedExpressionConstructType Type, Type ConstructType)[]
        {
            ("+", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionAddOperator )),
            ("-", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionSubtractOperator )),
            ("*", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionMultiplyOperator )),
            ("/", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionDivideOperator )),
            ("mod", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionModuloOperator )),
            ("-", ParsedExpressionConstructType.PrefixUnaryOperator, typeof( ParsedExpressionNegateOperator ))
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
        var actualConstructs = result.GetConstructs()
            .Select( i => (i.Symbol.ToString(), i.Type, i.Construct as Type ?? i.Construct.GetType()) );

        var actualBinaryOperatorPrecedences = result.GetBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );
        var actualPrefixUnaryConstructPrecedences =
            result.GetPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

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

        var expectedConstructs = new (string Symbol, ParsedExpressionConstructType Type, Type ConstructType)[]
        {
            ("&", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionBitwiseAndOperator )),
            ("|", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionBitwiseOrOperator )),
            ("^", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionBitwiseXorOperator )),
            ("<<", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionBitwiseLeftShiftOperator )),
            (">>", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionBitwiseRightShiftOperator )),
            ("~", ParsedExpressionConstructType.PrefixUnaryOperator, typeof( ParsedExpressionBitwiseNotOperator ))
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
        var actualConstructs = result.GetConstructs()
            .Select( i => (i.Symbol.ToString(), i.Type, i.Construct as Type ?? i.Construct.GetType()) );

        var actualBinaryOperatorPrecedences = result.GetBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );
        var actualPrefixUnaryConstructPrecedences =
            result.GetPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

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

        var expectedConstructs = new (string Symbol, ParsedExpressionConstructType Type, Type ConstructType)[]
        {
            ("??", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionCoalesceOperator )),
            ("==", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionEqualToOperator )),
            ("!=", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionNotEqualToOperator )),
            (">", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionGreaterThanOperator )),
            ("<", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionLessThanOperator )),
            (">=", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionGreaterThanOrEqualToOperator )),
            ("<=", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionLessThanOrEqualToOperator )),
            ("<=>", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionCompareOperator ))
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
        var actualConstructs = result.GetConstructs()
            .Select( i => (i.Symbol.ToString(), i.Type, i.Construct as Type ?? i.Construct.GetType()) );

        var actualBinaryOperatorPrecedences = result.GetBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );

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

        var expectedConstructs = new (string Symbol, ParsedExpressionConstructType Type, Type ConstructType)[]
        {
            ("&", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionBitwiseAndBooleanOperator )),
            ("|", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionBitwiseOrBooleanOperator )),
            ("^", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionBitwiseXorBooleanOperator ))
        };

        var expectedBinaryOperatorPrecedences = new (string Symbol, int Value)[]
        {
            ("&", 8),
            ("|", 10),
            ("^", 9)
        };

        var result = sut.AddBooleanBitwiseOperators();
        var actualConstructs = result.GetConstructs()
            .Select( i => (i.Symbol.ToString(), i.Type, i.Construct as Type ?? i.Construct.GetType()) );

        var actualBinaryOperatorPrecedences = result.GetBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );

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

        var expectedConstructs = new (string Symbol, ParsedExpressionConstructType Type, Type ConstructType)[]
        {
            ("==", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionEqualToBooleanOperator )),
            ("!=", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionNotEqualToBooleanOperator )),
            ("<=>", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionCompareBooleanOperator )),
            ("and", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionAndOperator )),
            ("or", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionOrOperator )),
            ("not", ParsedExpressionConstructType.PrefixUnaryOperator, typeof( ParsedExpressionNotOperator ))
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
        var actualConstructs = result.GetConstructs()
            .Select( i => (i.Symbol.ToString(), i.Type, i.Construct as Type ?? i.Construct.GetType()) );

        var actualBinaryOperatorPrecedences = result.GetBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );
        var actualPrefixUnaryConstructPrecedences =
            result.GetPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

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

        var expectedConstructs = new (string Symbol, ParsedExpressionConstructType Type, Type ConstructType)[]
        {
            ("boolean", ParsedExpressionConstructType.TypeDeclaration, typeof( bool )),
            ("[boolean]", ParsedExpressionConstructType.PrefixTypeConverter, typeof( ParsedExpressionTypeConverter<bool> )),
            ("[boolean]", ParsedExpressionConstructType.PrefixTypeConverter, typeof( ParsedExpressionTypeConverter<bool, int> )),
            ("BOOLEAN", ParsedExpressionConstructType.Constant, typeof( ParsedExpressionConstant<Type> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[boolean]", 1)
        };

        var expectedPostfixUnaryConstructPrecedences = Array.Empty<(string Symbol, int Value)>();

        var result = sut.AddBooleanTypeDefinition( new ParsedExpressionTypeConverter<bool, int>() );
        var actualConstructs = result.GetConstructs()
            .Select( i => (i.Symbol.ToString(), i.Type, i.Construct as Type ?? i.Construct.GetType()) );

        var actualPrefixUnaryConstructPrecedences =
            result.GetPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        var actualPostfixUnaryConstructPrecedences =
            result.GetPostfixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

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
            .SetPostfixTypeConverter( "as_b" );

        var expectedConstructs = new (string Symbol, ParsedExpressionConstructType Type, Type ConstructType)[]
        {
            ("b", ParsedExpressionConstructType.TypeDeclaration, typeof( bool )),
            ("[b]", ParsedExpressionConstructType.PrefixTypeConverter, typeof( ParsedExpressionTypeConverter<bool> )),
            ("[b]", ParsedExpressionConstructType.PrefixTypeConverter, typeof( ParsedExpressionTypeConverter<bool, int> )),
            ("as_b", ParsedExpressionConstructType.PostfixTypeConverter, typeof( ParsedExpressionTypeConverter<bool> )),
            ("as_b", ParsedExpressionConstructType.PostfixTypeConverter, typeof( ParsedExpressionTypeConverter<bool, int> )),
            ("B", ParsedExpressionConstructType.Constant, typeof( ParsedExpressionConstant<Type> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[b]", 1)
        };

        var expectedPostfixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("as_b", 1)
        };

        var result = sut.AddBooleanTypeDefinition( symbols, new ParsedExpressionTypeConverter<bool, int>() );
        var actualConstructs = result.GetConstructs()
            .Select( i => (i.Symbol.ToString(), i.Type, i.Construct as Type ?? i.Construct.GetType()) );

        var actualPrefixUnaryConstructPrecedences =
            result.GetPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        var actualPostfixUnaryConstructPrecedences =
            result.GetPostfixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

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
            .DisablePrefixTypeConverter()
            .DisableConstant();

        var result = sut.AddBooleanTypeDefinition( symbols );
        var actualConstructs = result.GetConstructs().Select( x => (x.Symbol.ToString(), x.Type, x.Construct) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( ("b", ParsedExpressionConstructType.TypeDeclaration, typeof( bool )) );
        }
    }

    [Fact]
    public void AddDecimalArithmeticOperators_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, ParsedExpressionConstructType Type, Type ConstructType)[]
        {
            ("+", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionAddDecimalOperator )),
            ("-", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionSubtractDecimalOperator )),
            ("*", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionMultiplyDecimalOperator )),
            ("/", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionDivideDecimalOperator )),
            ("mod", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionModuloDecimalOperator )),
            ("-", ParsedExpressionConstructType.PrefixUnaryOperator, typeof( ParsedExpressionNegateDecimalOperator ))
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
        var actualConstructs = result.GetConstructs()
            .Select( i => (i.Symbol.ToString(), i.Type, i.Construct as Type ?? i.Construct.GetType()) );

        var actualBinaryOperatorPrecedences = result.GetBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );
        var actualPrefixUnaryConstructPrecedences =
            result.GetPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

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

        var expectedConstructs = new (string Symbol, ParsedExpressionConstructType Type, Type ConstructType)[]
        {
            ("==", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionEqualToDecimalOperator )),
            ("!=", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionNotEqualToDecimalOperator )),
            (">", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionGreaterThanDecimalOperator )),
            ("<", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionLessThanDecimalOperator )),
            (">=", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionGreaterThanOrEqualToDecimalOperator )),
            ("<=", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionLessThanOrEqualToDecimalOperator )),
            ("<=>", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionCompareDecimalOperator ))
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
        var actualConstructs = result.GetConstructs()
            .Select( i => (i.Symbol.ToString(), i.Type, i.Construct as Type ?? i.Construct.GetType()) );

        var actualBinaryOperatorPrecedences = result.GetBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );

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

        var expectedConstructs = new (string Symbol, ParsedExpressionConstructType Type, Type ConstructType)[]
        {
            ("decimal", ParsedExpressionConstructType.TypeDeclaration, typeof( decimal )),
            ("[decimal]", ParsedExpressionConstructType.PrefixTypeConverter, typeof( ParsedExpressionTypeConverter<decimal> )),
            ("[decimal]", ParsedExpressionConstructType.PrefixTypeConverter, typeof( ParsedExpressionTypeConverter<decimal, double> )),
            ("m", ParsedExpressionConstructType.PostfixTypeConverter, typeof( ParsedExpressionTypeConverter<decimal> )),
            ("m", ParsedExpressionConstructType.PostfixTypeConverter, typeof( ParsedExpressionTypeConverter<decimal, double> )),
            ("DECIMAL", ParsedExpressionConstructType.Constant, typeof( ParsedExpressionConstant<Type> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[decimal]", 1)
        };

        var expectedPostfixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("m", 1)
        };

        var result = sut.AddDecimalTypeDefinition( new ParsedExpressionTypeConverter<decimal, double>() );
        var actualConstructs = result.GetConstructs()
            .Select( i => (i.Symbol.ToString(), i.Type, i.Construct as Type ?? i.Construct.GetType()) );

        var actualPrefixUnaryConstructPrecedences =
            result.GetPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        var actualPostfixUnaryConstructPrecedences =
            result.GetPostfixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

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

        var expectedConstructs = new (string Symbol, ParsedExpressionConstructType Type, Type ConstructType)[]
        {
            ("d", ParsedExpressionConstructType.TypeDeclaration, typeof( decimal )),
            ("[d]", ParsedExpressionConstructType.PrefixTypeConverter, typeof( ParsedExpressionTypeConverter<decimal> )),
            ("[d]", ParsedExpressionConstructType.PrefixTypeConverter, typeof( ParsedExpressionTypeConverter<decimal, double> )),
            ("D", ParsedExpressionConstructType.Constant, typeof( ParsedExpressionConstant<Type> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[d]", 1)
        };

        var expectedPostfixUnaryConstructPrecedences = Array.Empty<(string Symbol, int Value)>();

        var result = sut.AddDecimalTypeDefinition( symbols, new ParsedExpressionTypeConverter<decimal, double>() );
        var actualConstructs = result.GetConstructs()
            .Select( i => (i.Symbol.ToString(), i.Type, i.Construct as Type ?? i.Construct.GetType()) );

        var actualPrefixUnaryConstructPrecedences =
            result.GetPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        var actualPostfixUnaryConstructPrecedences =
            result.GetPostfixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

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
            .DisablePrefixTypeConverter()
            .DisableConstant();

        var result = sut.AddDecimalTypeDefinition( symbols );
        var actualConstructs = result.GetConstructs().Select( x => (x.Symbol.ToString(), x.Type, x.Construct) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( ("d", ParsedExpressionConstructType.TypeDeclaration, typeof( decimal )) );
        }
    }

    [Fact]
    public void AddDoubleArithmeticOperators_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, ParsedExpressionConstructType Type, Type ConstructType)[]
        {
            ("+", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionAddDoubleOperator )),
            ("-", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionSubtractDoubleOperator )),
            ("*", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionMultiplyDoubleOperator )),
            ("/", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionDivideDoubleOperator )),
            ("mod", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionModuloDoubleOperator )),
            ("-", ParsedExpressionConstructType.PrefixUnaryOperator, typeof( ParsedExpressionNegateDoubleOperator ))
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
        var actualConstructs = result.GetConstructs()
            .Select( i => (i.Symbol.ToString(), i.Type, i.Construct as Type ?? i.Construct.GetType()) );

        var actualBinaryOperatorPrecedences = result.GetBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );
        var actualPrefixUnaryConstructPrecedences =
            result.GetPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

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

        var expectedConstructs = new (string Symbol, ParsedExpressionConstructType Type, Type ConstructType)[]
        {
            ("==", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionEqualToDoubleOperator )),
            ("!=", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionNotEqualToDoubleOperator )),
            (">", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionGreaterThanDoubleOperator )),
            ("<", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionLessThanDoubleOperator )),
            (">=", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionGreaterThanOrEqualToDoubleOperator )),
            ("<=", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionLessThanOrEqualToDoubleOperator )),
            ("<=>", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionCompareDoubleOperator ))
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
        var actualConstructs = result.GetConstructs()
            .Select( i => (i.Symbol.ToString(), i.Type, i.Construct as Type ?? i.Construct.GetType()) );

        var actualBinaryOperatorPrecedences = result.GetBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );

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

        var expectedConstructs = new (string Symbol, ParsedExpressionConstructType Type, Type ConstructType)[]
        {
            ("double", ParsedExpressionConstructType.TypeDeclaration, typeof( double )),
            ("[double]", ParsedExpressionConstructType.PrefixTypeConverter, typeof( ParsedExpressionTypeConverter<double> )),
            ("[double]", ParsedExpressionConstructType.PrefixTypeConverter, typeof( ParsedExpressionTypeConverter<double, decimal> )),
            ("DOUBLE", ParsedExpressionConstructType.Constant, typeof( ParsedExpressionConstant<Type> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[double]", 1)
        };

        var expectedPostfixUnaryConstructPrecedences = Array.Empty<(string Symbol, int Value)>();

        var result = sut.AddDoubleTypeDefinition( new ParsedExpressionTypeConverter<double, decimal>() );
        var actualConstructs = result.GetConstructs()
            .Select( i => (i.Symbol.ToString(), i.Type, i.Construct as Type ?? i.Construct.GetType()) );

        var actualPrefixUnaryConstructPrecedences =
            result.GetPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        var actualPostfixUnaryConstructPrecedences =
            result.GetPostfixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

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
            .SetPostfixTypeConverter( "as_d" );

        var expectedConstructs = new (string Symbol, ParsedExpressionConstructType Type, Type ConstructType)[]
        {
            ("d", ParsedExpressionConstructType.TypeDeclaration, typeof( double )),
            ("[d]", ParsedExpressionConstructType.PrefixTypeConverter, typeof( ParsedExpressionTypeConverter<double> )),
            ("[d]", ParsedExpressionConstructType.PrefixTypeConverter, typeof( ParsedExpressionTypeConverter<double, decimal> )),
            ("as_d", ParsedExpressionConstructType.PostfixTypeConverter, typeof( ParsedExpressionTypeConverter<double> )),
            ("as_d", ParsedExpressionConstructType.PostfixTypeConverter, typeof( ParsedExpressionTypeConverter<double, decimal> )),
            ("D", ParsedExpressionConstructType.Constant, typeof( ParsedExpressionConstant<Type> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[d]", 1)
        };

        var expectedPostfixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("as_d", 1)
        };

        var result = sut.AddDoubleTypeDefinition( symbols, new ParsedExpressionTypeConverter<double, decimal>() );
        var actualConstructs = result.GetConstructs()
            .Select( i => (i.Symbol.ToString(), i.Type, i.Construct as Type ?? i.Construct.GetType()) );

        var actualPrefixUnaryConstructPrecedences =
            result.GetPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        var actualPostfixUnaryConstructPrecedences =
            result.GetPostfixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

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
            .DisablePrefixTypeConverter()
            .DisableConstant();

        var result = sut.AddDoubleTypeDefinition( symbols );
        var actualConstructs = result.GetConstructs().Select( x => (x.Symbol.ToString(), x.Type, x.Construct) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( ("d", ParsedExpressionConstructType.TypeDeclaration, typeof( double )) );
        }
    }

    [Fact]
    public void AddFloatArithmeticOperators_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, ParsedExpressionConstructType Type, Type ConstructType)[]
        {
            ("+", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionAddFloatOperator )),
            ("-", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionSubtractFloatOperator )),
            ("*", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionMultiplyFloatOperator )),
            ("/", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionDivideFloatOperator )),
            ("mod", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionModuloFloatOperator )),
            ("-", ParsedExpressionConstructType.PrefixUnaryOperator, typeof( ParsedExpressionNegateFloatOperator ))
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
        var actualConstructs = result.GetConstructs()
            .Select( i => (i.Symbol.ToString(), i.Type, i.Construct as Type ?? i.Construct.GetType()) );

        var actualBinaryOperatorPrecedences = result.GetBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );
        var actualPrefixUnaryConstructPrecedences =
            result.GetPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

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

        var expectedConstructs = new (string Symbol, ParsedExpressionConstructType Type, Type ConstructType)[]
        {
            ("==", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionEqualToFloatOperator )),
            ("!=", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionNotEqualToFloatOperator )),
            (">", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionGreaterThanFloatOperator )),
            ("<", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionLessThanFloatOperator )),
            (">=", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionGreaterThanOrEqualToFloatOperator )),
            ("<=", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionLessThanOrEqualToFloatOperator )),
            ("<=>", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionCompareFloatOperator ))
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
        var actualConstructs = result.GetConstructs()
            .Select( i => (i.Symbol.ToString(), i.Type, i.Construct as Type ?? i.Construct.GetType()) );

        var actualBinaryOperatorPrecedences = result.GetBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );

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

        var expectedConstructs = new (string Symbol, ParsedExpressionConstructType Type, Type ConstructType)[]
        {
            ("float", ParsedExpressionConstructType.TypeDeclaration, typeof( float )),
            ("[float]", ParsedExpressionConstructType.PrefixTypeConverter, typeof( ParsedExpressionTypeConverter<float> )),
            ("[float]", ParsedExpressionConstructType.PrefixTypeConverter, typeof( ParsedExpressionTypeConverter<float, decimal> )),
            ("f", ParsedExpressionConstructType.PostfixTypeConverter, typeof( ParsedExpressionTypeConverter<float> )),
            ("f", ParsedExpressionConstructType.PostfixTypeConverter, typeof( ParsedExpressionTypeConverter<float, decimal> )),
            ("FLOAT", ParsedExpressionConstructType.Constant, typeof( ParsedExpressionConstant<Type> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[float]", 1)
        };

        var expectedPostfixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("f", 1)
        };

        var result = sut.AddFloatTypeDefinition( new ParsedExpressionTypeConverter<float, decimal>() );
        var actualConstructs = result.GetConstructs()
            .Select( i => (i.Symbol.ToString(), i.Type, i.Construct as Type ?? i.Construct.GetType()) );

        var actualPrefixUnaryConstructPrecedences =
            result.GetPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        var actualPostfixUnaryConstructPrecedences =
            result.GetPostfixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

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

        var expectedConstructs = new (string Symbol, ParsedExpressionConstructType Type, Type ConstructType)[]
        {
            ("f", ParsedExpressionConstructType.TypeDeclaration, typeof( float )),
            ("[f]", ParsedExpressionConstructType.PrefixTypeConverter, typeof( ParsedExpressionTypeConverter<float> )),
            ("[f]", ParsedExpressionConstructType.PrefixTypeConverter, typeof( ParsedExpressionTypeConverter<float, decimal> )),
            ("F", ParsedExpressionConstructType.Constant, typeof( ParsedExpressionConstant<Type> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[f]", 1)
        };

        var expectedPostfixUnaryConstructPrecedences = Array.Empty<(string Symbol, int Value)>();

        var result = sut.AddFloatTypeDefinition( symbols, new ParsedExpressionTypeConverter<float, decimal>() );
        var actualConstructs = result.GetConstructs()
            .Select( i => (i.Symbol.ToString(), i.Type, i.Construct as Type ?? i.Construct.GetType()) );

        var actualPrefixUnaryConstructPrecedences =
            result.GetPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        var actualPostfixUnaryConstructPrecedences =
            result.GetPostfixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

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
            .DisablePrefixTypeConverter()
            .DisableConstant();

        var result = sut.AddFloatTypeDefinition( symbols );
        var actualConstructs = result.GetConstructs().Select( x => (x.Symbol.ToString(), x.Type, x.Construct) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( ("f", ParsedExpressionConstructType.TypeDeclaration, typeof( float )) );
        }
    }

    [Fact]
    public void AddInt32ArithmeticOperators_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, ParsedExpressionConstructType Type, Type ConstructType)[]
        {
            ("+", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionAddInt32Operator )),
            ("-", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionSubtractInt32Operator )),
            ("*", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionMultiplyInt32Operator )),
            ("/", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionDivideInt32Operator )),
            ("mod", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionModuloInt32Operator )),
            ("-", ParsedExpressionConstructType.PrefixUnaryOperator, typeof( ParsedExpressionNegateInt32Operator ))
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
        var actualConstructs = result.GetConstructs()
            .Select( i => (i.Symbol.ToString(), i.Type, i.Construct as Type ?? i.Construct.GetType()) );

        var actualBinaryOperatorPrecedences = result.GetBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );
        var actualPrefixUnaryConstructPrecedences =
            result.GetPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

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

        var expectedConstructs = new (string Symbol, ParsedExpressionConstructType Type, Type ConstructType)[]
        {
            ("&", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionBitwiseAndInt32Operator )),
            ("|", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionBitwiseOrInt32Operator )),
            ("^", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionBitwiseXorInt32Operator )),
            ("<<", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionBitwiseLeftShiftInt32Operator )),
            (">>", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionBitwiseRightShiftInt32Operator )),
            ("~", ParsedExpressionConstructType.PrefixUnaryOperator, typeof( ParsedExpressionBitwiseNotInt32Operator ))
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
        var actualConstructs = result.GetConstructs()
            .Select( i => (i.Symbol.ToString(), i.Type, i.Construct as Type ?? i.Construct.GetType()) );

        var actualBinaryOperatorPrecedences = result.GetBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );
        var actualPrefixUnaryConstructPrecedences =
            result.GetPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

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

        var expectedConstructs = new (string Symbol, ParsedExpressionConstructType Type, Type ConstructType)[]
        {
            ("==", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionEqualToInt32Operator )),
            ("!=", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionNotEqualToInt32Operator )),
            (">", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionGreaterThanInt32Operator )),
            ("<", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionLessThanInt32Operator )),
            (">=", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionGreaterThanOrEqualToInt32Operator )),
            ("<=", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionLessThanOrEqualToInt32Operator )),
            ("<=>", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionCompareInt32Operator ))
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
        var actualConstructs = result.GetConstructs()
            .Select( i => (i.Symbol.ToString(), i.Type, i.Construct as Type ?? i.Construct.GetType()) );

        var actualBinaryOperatorPrecedences = result.GetBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );

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

        var expectedConstructs = new (string Symbol, ParsedExpressionConstructType Type, Type ConstructType)[]
        {
            ("int32", ParsedExpressionConstructType.TypeDeclaration, typeof( int )),
            ("[int32]", ParsedExpressionConstructType.PrefixTypeConverter, typeof( ParsedExpressionTypeConverter<int> )),
            ("[int32]", ParsedExpressionConstructType.PrefixTypeConverter, typeof( ParsedExpressionTypeConverter<int, long> )),
            ("i", ParsedExpressionConstructType.PostfixTypeConverter, typeof( ParsedExpressionTypeConverter<int> )),
            ("i", ParsedExpressionConstructType.PostfixTypeConverter, typeof( ParsedExpressionTypeConverter<int, long> )),
            ("INT32", ParsedExpressionConstructType.Constant, typeof( ParsedExpressionConstant<Type> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[int32]", 1)
        };

        var expectedPostfixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("i", 1)
        };

        var result = sut.AddInt32TypeDefinition( new ParsedExpressionTypeConverter<int, long>() );
        var actualConstructs = result.GetConstructs()
            .Select( i => (i.Symbol.ToString(), i.Type, i.Construct as Type ?? i.Construct.GetType()) );

        var actualPrefixUnaryConstructPrecedences =
            result.GetPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        var actualPostfixUnaryConstructPrecedences =
            result.GetPostfixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

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
            .SetName( "i" );

        var expectedConstructs = new (string Symbol, ParsedExpressionConstructType Type, Type ConstructType)[]
        {
            ("i", ParsedExpressionConstructType.TypeDeclaration, typeof( int )),
            ("[i]", ParsedExpressionConstructType.PrefixTypeConverter, typeof( ParsedExpressionTypeConverter<int> )),
            ("[i]", ParsedExpressionConstructType.PrefixTypeConverter, typeof( ParsedExpressionTypeConverter<int, long> )),
            ("I", ParsedExpressionConstructType.Constant, typeof( ParsedExpressionConstant<Type> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[i]", 1)
        };

        var expectedPostfixUnaryConstructPrecedences = Array.Empty<(string Symbol, int Value)>();

        var result = sut.AddInt32TypeDefinition( symbols, new ParsedExpressionTypeConverter<int, long>() );
        var actualConstructs = result.GetConstructs()
            .Select( i => (i.Symbol.ToString(), i.Type, i.Construct as Type ?? i.Construct.GetType()) );

        var actualPrefixUnaryConstructPrecedences =
            result.GetPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        var actualPostfixUnaryConstructPrecedences =
            result.GetPostfixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

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
            .DisablePrefixTypeConverter()
            .DisableConstant();

        var result = sut.AddInt32TypeDefinition( symbols );
        var actualConstructs = result.GetConstructs().Select( x => (x.Symbol.ToString(), x.Type, x.Construct) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( ("i", ParsedExpressionConstructType.TypeDeclaration, typeof( int )) );
        }
    }

    [Fact]
    public void AddInt64ArithmeticOperators_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, ParsedExpressionConstructType Type, Type ConstructType)[]
        {
            ("+", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionAddInt64Operator )),
            ("-", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionSubtractInt64Operator )),
            ("*", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionMultiplyInt64Operator )),
            ("/", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionDivideInt64Operator )),
            ("mod", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionModuloInt64Operator )),
            ("-", ParsedExpressionConstructType.PrefixUnaryOperator, typeof( ParsedExpressionNegateInt64Operator ))
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
        var actualConstructs = result.GetConstructs()
            .Select( i => (i.Symbol.ToString(), i.Type, i.Construct as Type ?? i.Construct.GetType()) );

        var actualBinaryOperatorPrecedences = result.GetBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );
        var actualPrefixUnaryConstructPrecedences =
            result.GetPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

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

        var expectedConstructs = new (string Symbol, ParsedExpressionConstructType Type, Type ConstructType)[]
        {
            ("&", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionBitwiseAndInt64Operator )),
            ("|", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionBitwiseOrInt64Operator )),
            ("^", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionBitwiseXorInt64Operator )),
            ("<<", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionBitwiseLeftShiftInt64Operator )),
            (">>", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionBitwiseRightShiftInt64Operator )),
            ("~", ParsedExpressionConstructType.PrefixUnaryOperator, typeof( ParsedExpressionBitwiseNotInt64Operator ))
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
        var actualConstructs = result.GetConstructs()
            .Select( i => (i.Symbol.ToString(), i.Type, i.Construct as Type ?? i.Construct.GetType()) );

        var actualBinaryOperatorPrecedences = result.GetBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );
        var actualPrefixUnaryConstructPrecedences =
            result.GetPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

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

        var expectedConstructs = new (string Symbol, ParsedExpressionConstructType Type, Type ConstructType)[]
        {
            ("==", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionEqualToInt64Operator )),
            ("!=", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionNotEqualToInt64Operator )),
            (">", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionGreaterThanInt64Operator )),
            ("<", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionLessThanInt64Operator )),
            (">=", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionGreaterThanOrEqualToInt64Operator )),
            ("<=", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionLessThanOrEqualToInt64Operator )),
            ("<=>", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionCompareInt64Operator ))
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
        var actualConstructs = result.GetConstructs()
            .Select( i => (i.Symbol.ToString(), i.Type, i.Construct as Type ?? i.Construct.GetType()) );

        var actualBinaryOperatorPrecedences = result.GetBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );

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

        var expectedConstructs = new (string Symbol, ParsedExpressionConstructType Type, Type ConstructType)[]
        {
            ("int64", ParsedExpressionConstructType.TypeDeclaration, typeof( long )),
            ("[int64]", ParsedExpressionConstructType.PrefixTypeConverter, typeof( ParsedExpressionTypeConverter<long> )),
            ("[int64]", ParsedExpressionConstructType.PrefixTypeConverter, typeof( ParsedExpressionTypeConverter<long, int> )),
            ("l", ParsedExpressionConstructType.PostfixTypeConverter, typeof( ParsedExpressionTypeConverter<long> )),
            ("l", ParsedExpressionConstructType.PostfixTypeConverter, typeof( ParsedExpressionTypeConverter<long, int> )),
            ("INT64", ParsedExpressionConstructType.Constant, typeof( ParsedExpressionConstant<Type> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[int64]", 1)
        };

        var expectedPostfixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("l", 1)
        };

        var result = sut.AddInt64TypeDefinition( new ParsedExpressionTypeConverter<long, int>() );
        var actualConstructs = result.GetConstructs()
            .Select( i => (i.Symbol.ToString(), i.Type, i.Construct as Type ?? i.Construct.GetType()) );

        var actualPrefixUnaryConstructPrecedences =
            result.GetPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        var actualPostfixUnaryConstructPrecedences =
            result.GetPostfixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

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

        var expectedConstructs = new (string Symbol, ParsedExpressionConstructType Type, Type ConstructType)[]
        {
            ("i", ParsedExpressionConstructType.TypeDeclaration, typeof( long )),
            ("[i]", ParsedExpressionConstructType.PrefixTypeConverter, typeof( ParsedExpressionTypeConverter<long> )),
            ("[i]", ParsedExpressionConstructType.PrefixTypeConverter, typeof( ParsedExpressionTypeConverter<long, int> )),
            ("I", ParsedExpressionConstructType.Constant, typeof( ParsedExpressionConstant<Type> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[i]", 1)
        };

        var expectedPostfixUnaryConstructPrecedences = Array.Empty<(string Symbol, int Value)>();

        var result = sut.AddInt64TypeDefinition( symbols, new ParsedExpressionTypeConverter<long, int>() );
        var actualConstructs = result.GetConstructs()
            .Select( i => (i.Symbol.ToString(), i.Type, i.Construct as Type ?? i.Construct.GetType()) );

        var actualPrefixUnaryConstructPrecedences =
            result.GetPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        var actualPostfixUnaryConstructPrecedences =
            result.GetPostfixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

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
            .DisablePrefixTypeConverter()
            .DisableConstant();

        var result = sut.AddInt64TypeDefinition( symbols );
        var actualConstructs = result.GetConstructs().Select( x => (x.Symbol.ToString(), x.Type, x.Construct) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( ("i", ParsedExpressionConstructType.TypeDeclaration, typeof( long )) );
        }
    }

    [Fact]
    public void AddBigIntArithmeticOperators_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, ParsedExpressionConstructType Type, Type ConstructType)[]
        {
            ("+", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionAddBigIntOperator )),
            ("-", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionSubtractBigIntOperator )),
            ("*", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionMultiplyBigIntOperator )),
            ("/", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionDivideBigIntOperator )),
            ("mod", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionModuloBigIntOperator )),
            ("-", ParsedExpressionConstructType.PrefixUnaryOperator, typeof( ParsedExpressionNegateBigIntOperator ))
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
        var actualConstructs = result.GetConstructs()
            .Select( i => (i.Symbol.ToString(), i.Type, i.Construct as Type ?? i.Construct.GetType()) );

        var actualBinaryOperatorPrecedences = result.GetBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );
        var actualPrefixUnaryConstructPrecedences =
            result.GetPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

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

        var expectedConstructs = new (string Symbol, ParsedExpressionConstructType Type, Type ConstructType)[]
        {
            ("&", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionBitwiseAndBigIntOperator )),
            ("|", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionBitwiseOrBigIntOperator )),
            ("^", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionBitwiseXorBigIntOperator )),
            ("<<", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionBitwiseLeftShiftBigIntOperator )),
            (">>", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionBitwiseRightShiftBigIntOperator )),
            ("~", ParsedExpressionConstructType.PrefixUnaryOperator, typeof( ParsedExpressionBitwiseNotBigIntOperator ))
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
        var actualConstructs = result.GetConstructs()
            .Select( i => (i.Symbol.ToString(), i.Type, i.Construct as Type ?? i.Construct.GetType()) );

        var actualBinaryOperatorPrecedences = result.GetBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );
        var actualPrefixUnaryConstructPrecedences =
            result.GetPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

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

        var expectedConstructs = new (string Symbol, ParsedExpressionConstructType Type, Type ConstructType)[]
        {
            ("==", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionEqualToBigIntOperator )),
            ("!=", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionNotEqualToBigIntOperator )),
            (">", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionGreaterThanBigIntOperator )),
            ("<", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionLessThanBigIntOperator )),
            (">=", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionGreaterThanOrEqualToBigIntOperator )),
            ("<=", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionLessThanOrEqualToBigIntOperator )),
            ("<=>", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionCompareBigIntOperator ))
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
        var actualConstructs = result.GetConstructs()
            .Select( i => (i.Symbol.ToString(), i.Type, i.Construct as Type ?? i.Construct.GetType()) );

        var actualBinaryOperatorPrecedences = result.GetBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );

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

        var expectedConstructs = new (string Symbol, ParsedExpressionConstructType Type, Type ConstructType)[]
        {
            ("bigint", ParsedExpressionConstructType.TypeDeclaration, typeof( BigInteger )),
            ("[bigint]", ParsedExpressionConstructType.PrefixTypeConverter, typeof( ParsedExpressionTypeConverter<BigInteger> )),
            ("[bigint]", ParsedExpressionConstructType.PrefixTypeConverter, typeof( ParsedExpressionTypeConverter<BigInteger, long> )),
            ("BIGINT", ParsedExpressionConstructType.Constant, typeof( ParsedExpressionConstant<Type> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[bigint]", 1)
        };

        var expectedPostfixUnaryConstructPrecedences = Array.Empty<(string Symbol, int Value)>();

        var result = sut.AddBigIntTypeDefinition( new ParsedExpressionTypeConverter<BigInteger, long>() );
        var actualConstructs = result.GetConstructs()
            .Select( i => (i.Symbol.ToString(), i.Type, i.Construct as Type ?? i.Construct.GetType()) );

        var actualPrefixUnaryConstructPrecedences =
            result.GetPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        var actualPostfixUnaryConstructPrecedences =
            result.GetPostfixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

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
            .SetPostfixTypeConverter( "as_b" );

        var expectedConstructs = new (string Symbol, ParsedExpressionConstructType Type, Type ConstructType)[]
        {
            ("b", ParsedExpressionConstructType.TypeDeclaration, typeof( BigInteger )),
            ("[b]", ParsedExpressionConstructType.PrefixTypeConverter, typeof( ParsedExpressionTypeConverter<BigInteger> )),
            ("[b]", ParsedExpressionConstructType.PrefixTypeConverter, typeof( ParsedExpressionTypeConverter<BigInteger, long> )),
            ("as_b", ParsedExpressionConstructType.PostfixTypeConverter, typeof( ParsedExpressionTypeConverter<BigInteger> )),
            ("as_b", ParsedExpressionConstructType.PostfixTypeConverter, typeof( ParsedExpressionTypeConverter<BigInteger, long> )),
            ("B", ParsedExpressionConstructType.Constant, typeof( ParsedExpressionConstant<Type> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[b]", 1)
        };

        var expectedPostfixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("as_b", 1)
        };

        var result = sut.AddBigIntTypeDefinition( symbols, new ParsedExpressionTypeConverter<BigInteger, long>() );
        var actualConstructs = result.GetConstructs()
            .Select( i => (i.Symbol.ToString(), i.Type, i.Construct as Type ?? i.Construct.GetType()) );

        var actualPrefixUnaryConstructPrecedences =
            result.GetPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        var actualPostfixUnaryConstructPrecedences =
            result.GetPostfixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

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
            .DisablePrefixTypeConverter()
            .DisableConstant();

        var result = sut.AddBigIntTypeDefinition( symbols );
        var actualConstructs = result.GetConstructs().Select( x => (x.Symbol.ToString(), x.Type, x.Construct) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( ("b", ParsedExpressionConstructType.TypeDeclaration, typeof( BigInteger )) );
        }
    }

    [Fact]
    public void AddStringArithmeticOperators_ShouldAddCorrectConstructsAndPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder();

        var expectedConstructs = new (string Symbol, ParsedExpressionConstructType Type, Type ConstructType)[]
        {
            ("+", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionAddStringOperator ))
        };

        var expectedBinaryOperatorPrecedences = new (string Symbol, int Value)[]
        {
            ("+", 3)
        };

        var result = sut.AddStringArithmeticOperators();
        var actualConstructs = result.GetConstructs()
            .Select( i => (i.Symbol.ToString(), i.Type, i.Construct as Type ?? i.Construct.GetType()) );

        var actualBinaryOperatorPrecedences = result.GetBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );

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

        var expectedConstructs = new (string Symbol, ParsedExpressionConstructType Type, Type ConstructType)[]
        {
            ("==", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionEqualToStringOperator )),
            ("!=", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionNotEqualToStringOperator )),
            ("<=>", ParsedExpressionConstructType.BinaryOperator, typeof( ParsedExpressionCompareStringOperator ))
        };

        var expectedBinaryOperatorPrecedences = new (string Symbol, int Value)[]
        {
            ("==", 7),
            ("!=", 7),
            ("<=>", 5)
        };

        var result = sut.AddStringLogicalOperators();
        var actualConstructs = result.GetConstructs()
            .Select( i => (i.Symbol.ToString(), i.Type, i.Construct as Type ?? i.Construct.GetType()) );

        var actualBinaryOperatorPrecedences = result.GetBinaryOperatorPrecedences().Select( x => (x.Key.ToString(), x.Value) );

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

        var expectedConstructs = new (string Symbol, ParsedExpressionConstructType Type, Type ConstructType)[]
        {
            ("string", ParsedExpressionConstructType.TypeDeclaration, typeof( string )),
            ("[string]", ParsedExpressionConstructType.PrefixTypeConverter, typeof( ParsedExpressionToStringTypeConverter )),
            ("[string]", ParsedExpressionConstructType.PrefixTypeConverter, typeof( ParsedExpressionTypeConverter<string, int> )),
            ("STRING", ParsedExpressionConstructType.Constant, typeof( ParsedExpressionConstant<Type> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[string]", 1)
        };

        var expectedPostfixUnaryConstructPrecedences = Array.Empty<(string Symbol, int Value)>();

        var result = sut.AddStringTypeDefinition( new ParsedExpressionTypeConverter<string, int>() );
        var actualConstructs = result.GetConstructs()
            .Select( i => (i.Symbol.ToString(), i.Type, i.Construct as Type ?? i.Construct.GetType()) );

        var actualPrefixUnaryConstructPrecedences =
            result.GetPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        var actualPostfixUnaryConstructPrecedences =
            result.GetPostfixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

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
            .SetPostfixTypeConverter( "as_s" );

        var expectedConstructs = new (string Symbol, ParsedExpressionConstructType Type, Type ConstructType)[]
        {
            ("s", ParsedExpressionConstructType.TypeDeclaration, typeof( string )),
            ("[s]", ParsedExpressionConstructType.PrefixTypeConverter, typeof( ParsedExpressionToStringTypeConverter )),
            ("[s]", ParsedExpressionConstructType.PrefixTypeConverter, typeof( ParsedExpressionTypeConverter<string, int> )),
            ("as_s", ParsedExpressionConstructType.PostfixTypeConverter, typeof( ParsedExpressionToStringTypeConverter )),
            ("as_s", ParsedExpressionConstructType.PostfixTypeConverter, typeof( ParsedExpressionTypeConverter<string, int> )),
            ("S", ParsedExpressionConstructType.Constant, typeof( ParsedExpressionConstant<Type> ))
        };

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("[s]", 1)
        };

        var expectedPostfixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("as_s", 1)
        };

        var result = sut.AddStringTypeDefinition( symbols, new ParsedExpressionTypeConverter<string, int>() );
        var actualConstructs = result.GetConstructs()
            .Select( i => (i.Symbol.ToString(), i.Type, i.Construct as Type ?? i.Construct.GetType()) );

        var actualPrefixUnaryConstructPrecedences =
            result.GetPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        var actualPostfixUnaryConstructPrecedences =
            result.GetPostfixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

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
            .DisablePrefixTypeConverter()
            .DisableConstant();

        var result = sut.AddStringTypeDefinition( symbols );
        var actualConstructs = result.GetConstructs().Select( x => (x.Symbol.ToString(), x.Type, x.Construct) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( ("s", ParsedExpressionConstructType.TypeDeclaration, typeof( string )) );
        }
    }

    [Fact]
    public void AddBranchingVariadicFunctions_ShouldAddCorrectFunctions()
    {
        var sut = new ParsedExpressionFactoryBuilder();
        var symbols = ParsedExpressionBranchingVariadicFunctionSymbols.Default;

        var expectedConstructs = new (string Symbol, ParsedExpressionConstructType Type, Type ConstructType)[]
        {
            (symbols.If.ToString(), ParsedExpressionConstructType.VariadicFunction, typeof( ParsedExpressionIf )),
            (symbols.SwitchCase.ToString(), ParsedExpressionConstructType.VariadicFunction, typeof( ParsedExpressionSwitchCase )),
            (symbols.Switch.ToString(), ParsedExpressionConstructType.VariadicFunction, typeof( ParsedExpressionSwitch )),
            (symbols.Throw.ToString(), ParsedExpressionConstructType.VariadicFunction, typeof( ParsedExpressionThrow ))
        };

        var result = sut.AddBranchingVariadicFunctions( symbols );
        var actualConstructs = result.GetConstructs()
            .Select( i => (i.Symbol.ToString(), i.Type, i.Construct as Type ?? i.Construct.GetType()) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualConstructs.Should().BeSequentiallyEqualTo( expectedConstructs );
        }
    }

    [Fact]
    public void AddDefaultUnaryConstructPrecedences_ShouldAddCorrectMissingPrecedences()
    {
        var sut = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .AddPrefixUnaryOperator( "+", new ParsedExpressionNegateOperator() )
            .AddPostfixUnaryOperator( "+", new ParsedExpressionNegateOperator() )
            .AddPrefixUnaryOperator( "-", new ParsedExpressionNegateOperator() )
            .AddPostfixUnaryOperator( "^", new ParsedExpressionNegateOperator() )
            .AddPrefixTypeConverter( "[int]", new ParsedExpressionTypeConverter<int>() )
            .AddPostfixTypeConverter( "ToInt", new ParsedExpressionTypeConverter<int>() )
            .SetPrefixUnaryConstructPrecedence( "+", 2 )
            .SetPostfixUnaryConstructPrecedence( "+", 2 );

        var defaultPrecedence = 3;

        var expectedPrefixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("+", 2),
            ("-", defaultPrecedence),
            ("[int]", defaultPrecedence)
        };

        var expectedPostfixUnaryConstructPrecedences = new (string Symbol, int Value)[]
        {
            ("+", 2),
            ("^", defaultPrecedence),
            ("ToInt", defaultPrecedence)
        };

        var result = sut.AddDefaultUnaryConstructPrecedences( defaultPrecedence );

        var actualPrefixUnaryConstructPrecedences =
            result.GetPrefixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        var actualPostfixUnaryConstructPrecedences =
            result.GetPostfixUnaryConstructPrecedences().Select( x => (x.Key.ToString(), x.Value) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actualPrefixUnaryConstructPrecedences.Should().BeEquivalentTo( expectedPrefixUnaryConstructPrecedences );
            actualPostfixUnaryConstructPrecedences.Should().BeEquivalentTo( expectedPostfixUnaryConstructPrecedences );
        }
    }
}
