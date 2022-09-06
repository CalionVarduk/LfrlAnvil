using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo( "LfrlAnvil.Computable.Expressions.Tests" )]

// TODO:
// add variable support (syntax: let NAME = BODY;)
// add macro support (syntax: macro NAME = BODY;)
// variables are calculated while macros are inlined
// constant variables will be optimized away (configurable)
//
// a variadic function 'lazy' could be added that creates a Lazy<T> instance
//
// other ideas:
// Int128, Int256, UInt128, UInt256 structs
// FixedDecimal struct
// Complex struct
// unit conversions
