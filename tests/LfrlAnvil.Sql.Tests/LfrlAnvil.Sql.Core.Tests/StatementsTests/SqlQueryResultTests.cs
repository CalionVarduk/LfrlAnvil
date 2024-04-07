using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public partial class SqlQueryResultTests : TestsBase
{
    [Fact]
    public void Default_TypeErased_ShouldBeEmpty()
    {
        var sut = default( SqlQueryResult );

        using ( new AssertionScope() )
        {
            sut.Rows.Should().BeNull();
            sut.ResultSetFields.ToArray().Should().BeEmpty();
            sut.IsEmpty.Should().BeTrue();
        }
    }

    [Fact]
    public void Empty_TypeErased_ShouldBeEmpty()
    {
        var sut = SqlQueryResult.Empty;

        using ( new AssertionScope() )
        {
            sut.Rows.Should().BeNull();
            sut.ResultSetFields.ToArray().Should().BeEmpty();
            sut.IsEmpty.Should().BeTrue();
        }
    }

    [Fact]
    public void Ctor_TypeErased_ShouldCreateEmpty_WhenRowsAreEmpty()
    {
        var resultSetFields = new[] { new SqlResultSetField( 0, "a" ), new SqlResultSetField( 1, "b" ) };

        var sut = new SqlQueryResult( resultSetFields, new List<object?>() );

        using ( new AssertionScope() )
        {
            sut.Rows.Should().BeNull();
            sut.ResultSetFields.ToArray().Should().BeSequentiallyEqualTo( resultSetFields );
            sut.IsEmpty.Should().BeTrue();
        }
    }

    [Fact]
    public void Ctor_TypeErased_ShouldThrowArgumentOutOfRangeException_WhenNonEmptyWithEmptyFields()
    {
        var action = Lambda.Of(
            () => new SqlQueryResult(
                Array.Empty<SqlResultSetField>(),
                new List<object?>
                {
                    "foo",
                    3
                } ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Ctor_TypeErased_ShouldThrowArgumentException_WhenCellCountIsNotDivisibleByFieldCount()
    {
        var resultSetFields = new[] { new SqlResultSetField( 0, "a" ), new SqlResultSetField( 1, "b" ) };

        var action = Lambda.Of(
            () => new SqlQueryResult(
                resultSetFields,
                new List<object?>
                {
                    "foo",
                    3,
                    true
                } ) );

        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void Ctor_TypeErased_ShouldCreateNonEmpty()
    {
        var resultSetFields = new[]
        {
            new SqlResultSetField( 0, "a" ),
            new SqlResultSetField( 1, "b" ),
            new SqlResultSetField( 2, "c" ),
            new SqlResultSetField( 3, "d" )
        };

        var cells = new List<object?>
        {
            "foo",
            3,
            true,
            1.0,
            "bar",
            3,
            false,
            2.0,
            "lorem",
            5,
            false,
            5.0
        };

        var sut = new SqlQueryResult( resultSetFields, cells );

        using ( new AssertionScope() )
        {
            sut.Rows.Should().NotBeNull();
            (sut.Rows?.Count).Should().Be( 3 );
            (sut.Rows?.Fields.ToArray()).Should().BeSequentiallyEqualTo( resultSetFields );
            sut.ResultSetFields.ToArray().Should().BeSequentiallyEqualTo( resultSetFields );
            sut.IsEmpty.Should().BeFalse();
        }
    }

    [Fact]
    public void Default_Generic_ShouldBeEmpty()
    {
        var sut = default( SqlQueryResult<object[]> );

        using ( new AssertionScope() )
        {
            sut.Rows.Should().BeNull();
            sut.ResultSetFields.ToArray().Should().BeEmpty();
            sut.IsEmpty.Should().BeTrue();
        }
    }

    [Fact]
    public void Empty_Generic_ShouldBeEmpty()
    {
        var sut = SqlQueryResult<object[]>.Empty;

        using ( new AssertionScope() )
        {
            sut.Rows.Should().BeNull();
            sut.ResultSetFields.ToArray().Should().BeEmpty();
            sut.IsEmpty.Should().BeTrue();
        }
    }

    [Fact]
    public void Ctor_Generic_ShouldCreateEmpty_WhenRowsAreEmpty()
    {
        var sut = new SqlQueryResult<object[]>( null, new List<object[]>() );

        using ( new AssertionScope() )
        {
            sut.Rows.Should().BeNull();
            sut.ResultSetFields.ToArray().Should().BeEmpty();
            sut.IsEmpty.Should().BeTrue();
        }
    }

    [Fact]
    public void Ctor_Generic_ShouldCreateNonEmpty_WhenRowsAreNotEmpty()
    {
        var resultSetFields = new[] { new SqlResultSetField( 0, "a" ), new SqlResultSetField( 1, "b" ) };

        var rows = new List<object[]>
        {
            new object[] { "foo", 3 },
            new object[] { "lorem", 5 }
        };

        var sut = new SqlQueryResult<object[]>( resultSetFields, rows );

        using ( new AssertionScope() )
        {
            sut.Rows.Should().BeSameAs( rows );
            sut.ResultSetFields.ToArray().Should().BeSequentiallyEqualTo( resultSetFields );
            sut.IsEmpty.Should().BeFalse();
        }
    }
}
