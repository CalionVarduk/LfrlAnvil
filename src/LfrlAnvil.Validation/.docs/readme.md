([root](https://github.com/CalionVarduk/LfrlAnvil/blob/main/readme.md))
[![NuGet Badge](https://buildstats.info/nuget/LfrlAnvil.Validation)](https://www.nuget.org/packages/LfrlAnvil.Validation/)

# [<img src="../../../assets/logo.png" alt="logo" height="80"/>](../../../assets/logo.png) [LfrlAnvil.Validation](https://github.com/CalionVarduk/LfrlAnvil/tree/main/src/LfrlAnvil.Validation)

This project contains definitions of various composable validators, as well as validation message formatters.

### Documentation

Technical documentation can be found [here](https://calionvarduk.github.io/LfrlAnvil/api/LfrlAnvil.Validation/LfrlAnvil.Validation.html).

### Examples

Following are examples of a few chosen formattable validators:
```csharp
// validator that always passes
var a = FormattableValidators<string>.Pass<int>();

// validator that always fails, with a formattable 'GenericFailure' message
var b = FormattableValidators<string>.Fail<int>( "GenericFailure" );

// validator that requires a string value to not be empty
var c = FormattableValidators<string>.NotEmpty( "EmptyText" );

// validator that requires a string value to match the given regular expression
var d = FormattableValidators<string>.Match( new Regex( ... ), "TextNotMatched" );

// validator that requires a nullable int value to not be null
var e = FormattableValidators<string>.NotNull<int?>( "NullValue" );

// validator that requires an int value to be greater than 42
var f = FormattableValidators<string>.GreaterThan( 42, ValidationMessage.Create( "ValueTooSmall", 42 ) );

// validator that chooses which underlying validator to use based on the given 'value >= 0' condition
// when an int value is greater than or equal to 0, then the 'f' validator will be used
// otherwise, the 'a' validator will be used
// that means that this validator requires, that an int value must be greater than 42 or be negative
var g = Validators<ValidationMessage<string>>.Conditional( value => value >= 0, f, a );

// validator that requires, that all underlying validators pass
// this means that this validator requires, that a string value is not empty
// and matches the given regular expression
// all errors from all underlying validators are included in validation result
var h = Validators<ValidationMessage<string>>.All( c, d );

// validators that requires, that at least one underlying validator passes
// this means that this validator requires, that a string value is not empty
// or matches the given regular expression
// all errors from all underlying validators are included in validation result,
// unless at least one of them passes, which will cause all errors to be discarded
var i = Validators<ValidationMessage<string>>.Any( c, d );

// a string value validator that uses an underlying validator for int values,
// which requires, that a string value's Length is greater than 42
var j = f.For( static (string x) => x.Length );

// validation message formatter that can be used to convert validation messages to strings
// an implementation of such a formatter can e.g. use a localization service,
// capable of translating message codes to messages in specific languages
IValidationMessageFormatter<string> formatter = ...;

// example of a formatted validator
var k = h.Format( formatter );
```

It's also possible to use a type different than `string`, as a type of validation message's resource.
Following is an example of an `enum` that may serve such a purpose:
```csharp
// validation message resource type definition as an enum
public enum ValidationResources
{
    GenericFailure,
    // other resource values
}

// example of a validator that uses 'ValidationResources' as its validation message's resource type
var validator = FormattableValidators<ValidationResources>.Fail<int>( ValidationResources.GenericFailure );
```
