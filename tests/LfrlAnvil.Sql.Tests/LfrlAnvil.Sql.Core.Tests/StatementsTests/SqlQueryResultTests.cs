using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Statements;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public partial class SqlQueryResultTests : TestsBase
{
    [Fact]
    public void Default_TypeErased_ShouldBeEmpty()
    {
        var sut = default( SqlQueryResult );

        Assertion.All(
                sut.Rows.TestNull(),
                sut.ResultSetFields.ToArray().TestEmpty(),
                sut.IsEmpty.TestTrue() )
            .Go();
    }

    [Fact]
    public void Empty_TypeErased_ShouldBeEmpty()
    {
        var sut = SqlQueryResult.Empty;

        Assertion.All(
                sut.Rows.TestNull(),
                sut.ResultSetFields.ToArray().TestEmpty(),
                sut.IsEmpty.TestTrue() )
            .Go();
    }

    [Fact]
    public void Ctor_TypeErased_ShouldCreateEmpty_WhenRowsAreEmpty()
    {
        var resultSetFields = new[] { new SqlResultSetField( 0, "a" ), new SqlResultSetField( 1, "b" ) };

        var sut = new SqlQueryResult( resultSetFields, new List<object?>() );

        Assertion.All(
                sut.Rows.TestNull(),
                sut.ResultSetFields.TestSequence( resultSetFields ),
                sut.IsEmpty.TestTrue() )
            .Go();
    }

    [Fact]
    public void Ctor_TypeErased_ShouldThrowArgumentOutOfRangeException_WhenNonEmptyWithEmptyFields()
    {
        var action = Lambda.Of( () => new SqlQueryResult(
            Array.Empty<SqlResultSetField>(),
            new List<object?>
            {
                "foo",
                3
            } ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void Ctor_TypeErased_ShouldThrowArgumentException_WhenCellCountIsNotDivisibleByFieldCount()
    {
        var resultSetFields = new[] { new SqlResultSetField( 0, "a" ), new SqlResultSetField( 1, "b" ) };

        var action = Lambda.Of( () => new SqlQueryResult(
            resultSetFields,
            new List<object?>
            {
                "foo",
                3,
                true
            } ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
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

        Assertion.All(
                sut.Rows.TestNotNull( rows => Assertion.All(
                    rows.Count.TestEquals( 3 ),
                    rows.Fields.TestSequence( resultSetFields ) ) ),
                sut.ResultSetFields.TestSequence( resultSetFields ),
                sut.IsEmpty.TestFalse() )
            .Go();
    }

    [Fact]
    public void Default_Generic_ShouldBeEmpty()
    {
        var sut = default( SqlQueryResult<object[]> );

        Assertion.All(
                sut.Rows.TestNull(),
                sut.ResultSetFields.ToArray().TestEmpty(),
                sut.IsEmpty.TestTrue() )
            .Go();
    }

    [Fact]
    public void Empty_Generic_ShouldBeEmpty()
    {
        var sut = SqlQueryResult<object[]>.Empty;

        Assertion.All(
                sut.Rows.TestNull(),
                sut.ResultSetFields.ToArray().TestEmpty(),
                sut.IsEmpty.TestTrue() )
            .Go();
    }

    [Fact]
    public void Ctor_Generic_ShouldCreateEmpty_WhenRowsAreEmpty()
    {
        var sut = new SqlQueryResult<object[]>( null, new List<object[]>() );

        Assertion.All(
                sut.Rows.TestNull(),
                sut.ResultSetFields.ToArray().TestEmpty(),
                sut.IsEmpty.TestTrue() )
            .Go();
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

        Assertion.All(
                sut.Rows.TestRefEquals( rows ),
                sut.ResultSetFields.TestSequence( resultSetFields ),
                sut.IsEmpty.TestFalse() )
            .Go();
    }
}
