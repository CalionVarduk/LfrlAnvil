using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo( "LfrlAnvil.Computable.Expressions.Tests" )]

// TODO:
// add AddDefaultUnaryConstructPrecedences(int default) extension to factory builder
// add built-in configurable variadic functions for various syntactic sugars:
// - MEMBER_ACCESS(target, memberName)
// - INDEXER_ACCESS(target, ...parameters)
// - METHOD_CALL(target, methodName, ...parameters)
// - DELEGATE_CALL(target, ...parameters)
// - MAKE_ARRAY(elementType, ...values)
// - MAKE_DELEGATE(...parameters, body)
//
// add delegate call support
// ^ use [TYPE arg, Type arg, ...] BODY syntax
//
// Add inline function support (defining named function in the same input, that can be used e.g. as branching function's argument)
// ^ this would go very well together with the thing below
//
// add possibility to write an inline Func as an argument to a function/member access
// syntax proposition: (a, b, c, ..., z) -> BODY, or () -> BODY, if the Func doesn't have any parameters
// delegate's actual type would have to be inferred from the usage
// probable token handling changes:
// Func can only appear as an argument to another callable object, this could be leveraged
// if '(' is the first token & ')' is encountered next, then read the whole parameter as a parameterless Func
// if '(' is the first token & an argument token is encountered next, then resolve that argument token during next token resolution:
// - if the next token is ',', then treat the whole thing as a Func with some amount of parameters greater than 0
// - if the next token is not ',' & not '(', then treat the whole thing as a "normal", non-callable value
// - if the next token is '(', then there is a possibility, that this is still a func, whose first parameter is also a func, ad infinitum
//
// a variadic function 'reduce' could be added to consume arrays
//
// type declaration:
// this can be used to specify 'anonymous' delegate parameter types explicitly
// e.g. (int a) -> BODY instead of (a) -> BODY, where 'a' type would be inferred, but could also lead to unresolvable ambiguity
// e.g. when function Foo has two overloads, both accept one parameter
// first overload accepts parameter type (int a) -> ..., second accepts parameter type (double a) -> ...
//
// other ideas:
// Int128, Int256, UInt128, UInt256 structs
// FixedDecimal struct
// Complex struct
// unit conversions
