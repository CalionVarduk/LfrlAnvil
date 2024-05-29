([root](https://github.com/CalionVarduk/LfrlAnvil/blob/main/readme.md))
[![NuGet Badge](https://buildstats.info/nuget/LfrlAnvil.Computable.Expressions)](https://www.nuget.org/packages/LfrlAnvil.Computable.Expressions/)

# [LfrlAnvil.Computable.Expressions](https://github.com/CalionVarduk/LfrlAnvil/tree/main/src/LfrlAnvil.Computable/LfrlAnvil.Computable.Expressions)

This project contains a parser of string expressions that can be compiled to invocable delegates.

### Documentation

Technical documentation can be found [here](https://calionvarduk.github.io/LfrlAnvil/api/LfrlAnvil.Computable.Expressions/LfrlAnvil.Computable.Expressions.html).

### Examples

Following contains an example of a parser configuration, as well as creation,
compilation and invocation of a delegate:
```csharp
// creates a builder of a minimal parser capable of supporting most arithmetic and logical operators
// and branching functions, like 'if' and 'switch'
// in addition, it will try to parse all constant numbers as 32-bit signed integers
// instead of the default decimal type
var builder = new ParsedExpressionFactoryBuilder()
    .AddGenericArithmeticOperators()
    .AddGenericBitwiseOperators()
    .AddGenericLogicalOperators()
    .AddBooleanLogicalOperators()
    .AddStringArithmeticOperators()
    .AddBranchingVariadicFunctions()
    .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) );

// builds an expression parser
var parser = builder.Build();

// string expression to parse, that contains two external arguments: 'a' and 'b'
var input =
    """
    let x = (a + 1).ToString();
    let y = if(x.Length > 1, b * 2, b * 4).ToString();
    y + x + y;
    """;

// creates a parsed expression from the given string input
var expression = parser.Create<int, string>( input );

// compiles the expression to an invocable delegate
var func = expression.Compile();

// invokes the delegate with 'a' = 42 and 'b' = 31,
// which should return '624362'
var result = func.Invoke( 42, 31 );
```
