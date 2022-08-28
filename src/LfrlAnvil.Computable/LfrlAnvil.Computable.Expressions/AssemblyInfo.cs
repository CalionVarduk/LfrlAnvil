using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo( "LfrlAnvil.Computable.Expressions.Tests" )]

// TODO:
// a variadic function 'reduce' could be added to consume arrays
// a variadic function 'lazy' could be added that creates a Lazy<T> instance (either from a value expr or func expr)
//
// add variable support (syntax: let NAME = BODY;)
// add macro support (syntax: macro NAME = BODY;)
// variables are calculated while macros are inlined
//
// other ideas:
// Int128, Int256, UInt128, UInt256 structs
// FixedDecimal struct
// Complex struct
// unit conversions
