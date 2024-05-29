([root](https://github.com/CalionVarduk/LfrlAnvil/blob/main/readme.md))
[![NuGet Badge](https://buildstats.info/nuget/LfrlAnvil.Mapping)](https://www.nuget.org/packages/LfrlAnvil.Mapping/)

# [LfrlAnvil.Mapping](https://github.com/CalionVarduk/LfrlAnvil/tree/main/src/LfrlAnvil.Mapping)

This project contains an object mapper, as well as a builder of such a mapper.

### Documentation

Technical documentation can be found [here](https://calionvarduk.github.io/LfrlAnvil/api/LfrlAnvil.Mapping/LfrlAnvil.Mapping.html).

### Examples

Following is an example of an implementation of a class that defines object mappings:
```csharp
public class MyTypeMappingConfiguration : TypeMappingConfiguration
{
    public MyTypeMappingConfiguration()
    {
        // registers int => string mapping,
        // where string instances are created with x.ToString() expression,
        // where 'x' is the source int value
        Configure<int, string>( (x, _) => x.ToString() );
        
        // registers short => string mapping,
        // that uses int => string mapping registered in the 'm' ITypeMapper instance
        Configure<short, string>( (x, m) => m.Map( ( int )x ).To<string>() );
    }
}
```

There are other, more specialized types of type mapping configurations as well.
Such configurations can be used to create a mapper, like so:
```csharp
// creates a type mapper builder with registered configuration
var builder = new TypeMapperBuilder()
    .Configure( new MyTypeMappingConfiguration() );

// crates a type mapper from the current state of the builder
var mapper = builder.Build();

// maps 5 to string, which should return "5", according to the above configuration
var intResult = mapper.Map( 5 ).To<string>();

// maps a collection of shorts to a collection of strings
// should return a collection of 3 strings: "0", "3" and "7"
// result is not materialized
var shortResult = mapper.MapMany<short>( 0, 3, 7 ).To<string>();
```

There also exist type mapping configuration modules, that allow to create type mapping configuration trees.
They can be used to group type mapping configurations together, under one type mapping configuration.
