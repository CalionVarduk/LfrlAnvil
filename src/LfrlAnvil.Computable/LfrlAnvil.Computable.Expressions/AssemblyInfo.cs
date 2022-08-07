﻿using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo( "LfrlAnvil.Computable.Expressions.Tests" )]

// TODO:
// Standardize Debug.Assert usage (I'm on the fence about creating a static Assume class, similar to Ensure...)
// Add constant constructs
// Add factory builder extensions for registering constructs in bulk
// Add function constructs
// Add member access support (include implicit null check, basically treat it as ?. operator)
// Add branching function constructs (start with if-else, then add switch, maybe also throw?)
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
// add possibility to write a const-sized array, use '[' and ']' symbols as delimiters
// values can be resolved exactly like function parameters, separated by ','
// array type is inferred from its first element
// if array is empty, then its type is inferred from the parameter type, if it is used as a function parameter
// if not, then its type will be 'System.Object' (make it configurable)
// if all array values are const, then array is also made into const
//
// type system:
// factory builder could have methods for registering types
// method would accept a CLR System.Type ref & custom type name e.g. 'int'
// it would also allow to override the default prefix type converter symbol for that type
// by default it would be equal to '[' + type_name + ']' e.g. '[int]'
// it would also allow to specify postfix type converter symbol (by default, some types have a default value, some don't use postfix at all)
// this system could be used to specify 'anonymous' delegate parameter types explicitly
// e.g. (int a) -> BODY instead of (a) -> BODY, where 'a' type would be inferred, but could also lead to unresolvable ambiguity
// e.g. when function Foo has two overloads, both accept one parameter
// first overload accepts parameter type (int a) -> ..., second accepts parameter type (double a) -> ...
//
// other ideas:
// Int128, Int256, UInt128, UInt256 structs
// FixedDecimal struct
// Complex struct
// unit conversions
