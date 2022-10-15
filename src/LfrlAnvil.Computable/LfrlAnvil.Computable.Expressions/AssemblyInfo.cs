using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo( "LfrlAnvil.Computable.Expressions.Tests" )]

// TODO:
// ideas for other LfrlAnvil.Computable projects:
// Complex struct
// Structures representing values with units
// ^ values can be generic (.NET7 static interface members would be very useful, so that the structures can have math operators)
// ^ units can be convertible if they belong to the same 'category'
// ^ e.g. weight units ('t', 'kg', 'g' etc.)
// ^ there could be a base interface for units IUnitCategory with properties 'Symbol' & 'ConversionRatio'
// ^ WeightUnit class would implement IUnitCategory & declare static readonly fields like Ton, Kilogram, Gram etc. (basically a smart enum)
// ^ this could actually be implemented for decimal type, for now
// ^ and when the decision is made to move to .NET7, then add support for generic value types
