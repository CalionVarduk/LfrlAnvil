using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Tests.ExtensionsTests.EnumerableTests;

public abstract class GenericEnumerableExtensionsOfStructTypeTests<T> : GenericEnumerableExtensionsTests<T>
    where T : struct
{
    [Fact]
    public void WhereNotNull_ShouldFilterOutNullElements()
    {
        var expected = Fixture.CreateMany<T>().ToList();
        var sut = expected.Select( v => ( T? )v ).Append( null );

        var result = sut.WhereNotNull();

        result.TestSequence( expected ).Go();
    }

    [Fact]
    public void WhereNotNull_ShouldReturnFalseWhenSourceContainsNullElement()
    {
        var sut = Fixture.CreateMany<T>().ToList();
        var result = sut.Select( v => ( T? )v ).WhereNotNull();
        result.TestSequence( sut ).Go();
    }

    [Fact]
    public void WhereNotNull_ShouldFilterOutNullElements_WithExplicitComparer_AndNullableType()
    {
        var expected = Fixture.CreateMany<T?>().ToList();
        var sut = expected.Append( null );

        var result = sut.WhereNotNull( EqualityComparer<T?>.Default );

        result.TestSequence( expected ).Go();
    }

    [Fact]
    public void WhereNotNull_ShouldReturnFalseWhenSourceContainsNullElement_WithExplicitComparer_AndNullableType()
    {
        var sut = Fixture.CreateMany<T?>().ToList();
        var result = sut.WhereNotNull( EqualityComparer<T?>.Default );
        result.TestSequence( sut ).Go();
    }

    [Fact]
    public void WhereNotNull_ShouldReturnSource_WithExplicitComparer()
    {
        var sut = Fixture.CreateMany<T>().ToList();
        var result = sut.WhereNotNull( EqualityComparer<T>.Default );
        result.TestRefEquals( sut ).Go();
    }

    [Fact]
    public void ContainsNull_ShouldReturnTrueWhenSourceContainsNullElement()
    {
        var sut = Fixture.CreateMany<T?>().Append( null );
        var result = sut.ContainsNull();
        result.TestTrue().Go();
    }

    [Fact]
    public void ContainsNull_ShouldReturnFalseWhenSourceContainsNullElement()
    {
        var sut = Fixture.CreateMany<T?>();
        var result = sut.ContainsNull();
        result.TestFalse().Go();
    }

    [Fact]
    public void ContainsNull_ShouldReturnFalse_WithExplicitComparer()
    {
        var sut = Fixture.CreateMany<T>();
        var result = sut.ContainsNull( EqualityComparer<T>.Default );
        result.TestFalse().Go();
    }

    [Fact]
    public void ContainsNull_ShouldReturnTrueWhenSourceContainsNullElement_WithExplicitComparer_AndNullableType()
    {
        var sut = Fixture.CreateMany<T?>().Append( null );
        var result = sut.ContainsNull( EqualityComparer<T?>.Default );
        result.TestTrue().Go();
    }

    [Fact]
    public void ContainsNull_ShouldReturnFalseWhenSourceContainsNullElement_WithExplicitComparer_AndNullableType()
    {
        var sut = Fixture.CreateMany<T?>();
        var result = sut.ContainsNull( EqualityComparer<T?>.Default );
        result.TestFalse().Go();
    }

    [Fact]
    public void AsNullable_ShouldReturnCorrectResult()
    {
        var sut = Fixture.CreateMany<T>().ToList();
        var result = sut.AsNullable();
        result.Select( r => r!.Value ).TestSequence( sut ).Go();
    }
}
