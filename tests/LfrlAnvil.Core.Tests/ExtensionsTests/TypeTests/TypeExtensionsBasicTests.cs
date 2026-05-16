using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Tests.ExtensionsTests.TypeTests;

public class TypeExtensionsBasicTests : TestsBase
{
    [Fact]
    public void IsConstructable_ShouldReturnTrue_WhenTypeIsStruct()
    {
        var type = typeof( int );
        var result = type.IsConstructable();
        result.TestTrue().Go();
    }

    [Fact]
    public void IsConstructable_ShouldReturnTrue_WhenTypeIsClosedGenericStruct()
    {
        var type = typeof( KeyValuePair<string, int> );
        var result = type.IsConstructable();
        result.TestTrue().Go();
    }

    [Fact]
    public void IsConstructable_ShouldReturnFalse_WhenTypeIsOpenGenericStruct()
    {
        var type = typeof( KeyValuePair<,> );
        var result = type.IsConstructable();
        result.TestFalse().Go();
    }

    [Fact]
    public void IsConstructable_ShouldReturnTrue_WhenTypeIsClass()
    {
        var type = typeof( string );
        var result = type.IsConstructable();
        result.TestTrue().Go();
    }

    [Fact]
    public void IsConstructable_ShouldReturnTrue_WhenTypeIsClosedGenericClass()
    {
        var type = typeof( Dictionary<string, int> );
        var result = type.IsConstructable();
        result.TestTrue().Go();
    }

    [Fact]
    public void IsConstructable_ShouldReturnFalse_WhenTypeIsOpenGenericClass()
    {
        var type = typeof( Dictionary<,> );
        var result = type.IsConstructable();
        result.TestFalse().Go();
    }

    [Fact]
    public void IsConstructable_ShouldReturnFalse_WhenTypeContainsGenericParameters()
    {
        var type = typeof( Dictionary<,> ).GetOpenGenericImplementations( typeof( IDictionary<,> ) ).Single();
        var result = type.IsConstructable();
        result.TestFalse().Go();
    }

    [Fact]
    public void IsConstructable_ShouldReturnFalse_WhenTypeIsInterface()
    {
        var type = typeof( IEnumerable );
        var result = type.IsConstructable();
        result.TestFalse().Go();
    }

    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_ForNonNestedNonGenericType()
    {
        var type = typeof( IEnumerable );
        var result = type.GetDebugString();
        result.TestEquals( "System.Collections.IEnumerable" ).Go();
    }

    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_ForNonNestedClosedGenericType()
    {
        var type = typeof( Dictionary<int, string> );
        var result = type.GetDebugString();
        result.TestEquals( "System.Collections.Generic.Dictionary`2[TKey is System.Int32, TValue is System.String]" ).Go();
    }

    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_ForNonNestedOpenGenericType()
    {
        var type = typeof( Dictionary<,> );
        var result = type.GetDebugString();
        result.TestEquals( "System.Collections.Generic.Dictionary`2[TKey, TValue]" ).Go();
    }

    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_ForNestedType()
    {
        var type = typeof( Foo.Bar );
        var result = type.GetDebugString();
        result.TestEquals( "LfrlAnvil.Tests.ExtensionsTests.TypeTests.TypeExtensionsBasicTests+Foo+Bar" ).Go();
    }

    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_ForByRefType()
    {
        var type = typeof( Dictionary<int, string> )
            .GetMethod( nameof( Dictionary<,>.TryGetValue ) )!
            .GetParameters()[1]
            .ParameterType;

        var result = type.GetDebugString();
        result.TestEquals( "System.String&" ).Go();
    }

    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_ForOpenGenericTypeWithCovariantAndContravariantArguments()
    {
        var type = typeof( Func<,> );
        var result = type.GetDebugString();
        result.TestEquals( "System.Func`2[T [in], TResult [out]]" ).Go();
    }

    [Fact]
    public void GetDebugString_ShouldReturnCorrectResult_ForClosedGenericTypeWithCovariantAndContravariantArguments()
    {
        var type = typeof( Func<int, string> );
        var result = type.GetDebugString();
        result.TestEquals( "System.Func`2[T [in] is System.Int32, TResult [out] is System.String]" ).Go();
    }

    [Fact]
    public void SubstituteGenericArguments_ShouldDoNothing_WhenTypeIsNotGeneric()
    {
        var type = typeof( string );
        var result = type.SubstituteGenericArguments( typeof( int ) );
        result.TestRefEquals( type ).Go();
    }

    [Fact]
    public void SubstituteGenericArguments_ShouldDoNothing_WhenProvidedArgsAreEmpty()
    {
        var type = typeof( Func<int, string> );
        var result = type.SubstituteGenericArguments();
        result.TestEquals( type ).Go();
    }

    [Fact]
    public void SubstituteGenericArguments_ShouldDoNothing_WhenProvidedArgsAreAllNull()
    {
        var type = typeof( Func<int, string> );
        var result = type.SubstituteGenericArguments( null, null );
        result.TestEquals( type ).Go();
    }

    [Theory]
    [InlineData( typeof( Guid ), null, typeof( Func<Guid, string> ) )]
    [InlineData( null, typeof( Guid ), typeof( Func<int, Guid> ) )]
    [InlineData( typeof( int ), typeof( string ), typeof( Func<int, string> ) )]
    [InlineData( typeof( Guid ), typeof( double ), typeof( Func<Guid, double> ) )]
    public void SubstituteGenericArguments_ShouldReturnSubstitutedType_WhenSomeArgumentsAreSubstituted(
        Type? first,
        Type? second,
        Type expected)
    {
        var type = typeof( Func<int, string> );
        var result = type.SubstituteGenericArguments( first, second );
        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void SubstituteGenericArguments_ShouldReturnSubstitutedType_WhenProvidedArgsAreShorterThanActualArgs()
    {
        var type = typeof( Func<int, string> );
        var result = type.SubstituteGenericArguments( typeof( Guid ) );
        result.TestEquals( typeof( Func<Guid, string> ) ).Go();
    }

    [Fact]
    public void SubstituteGenericArguments_ShouldReturnSubstitutedType_WhenProvidedArgsAreLongerThanActualArgs()
    {
        var type = typeof( Func<int, string> );
        var result = type.SubstituteGenericArguments( typeof( Guid ), typeof( double ), typeof( long ) );
        result.TestEquals( typeof( Func<Guid, double> ) ).Go();
    }

    private sealed class Foo
    {
        internal sealed class Bar { }
    }
}
