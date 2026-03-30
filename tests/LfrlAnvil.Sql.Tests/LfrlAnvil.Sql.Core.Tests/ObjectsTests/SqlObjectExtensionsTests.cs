using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests.ObjectsTests;

public class SqlObjectExtensionsTests : TestsBase
{
    [Fact]
    public void CreateQuery_ShouldReturnCorrectNode()
    {
        var sut = SqlTableMock.Create<int>( "foo", [ "a", "b", "c" ], [ "a" ], schemaName: "s" );

        var result = sut.CreateQuery();

        result.ToString()
            .TestEquals(
                """
                FROM [s].[foo]
                ORDER BY ([s].[foo].[a] : System.Int32) ASC
                SELECT
                  ([s].[foo].[a] : System.Int32),
                  ([s].[foo].[b] : System.Int32),
                  ([s].[foo].[c] : System.Int32)
                """ )
            .Go();
    }

    [Fact]
    public void CreateQuery_ShouldReturnCorrectNode_WithOverriddenSelectionAndOrdering()
    {
        var sut = SqlTableMock.Create<int>( "foo", [ "a", "b", "c" ], [ "a" ], schemaName: "s" );

        var result = sut.CreateQuery(
            c =>
            {
                if ( c.Name == "b" )
                    return SqlSelectionOverride.Ignore;

                if ( c.Name == "c" )
                    return (c + SqlNode.Literal( 1 )).As( "d" );

                return SqlSelectionOverride.UseDefault;
            },
            _ => [ SqlNode.Literal( 0 ).As( "e" ) ],
            t => [ t["b"].Desc() ] );

        result.ToString()
            .TestEquals(
                """
                FROM [s].[foo]
                ORDER BY ([s].[foo].[b] : System.Int32) DESC
                SELECT
                  ([s].[foo].[a] : System.Int32),
                  (([s].[foo].[c] : System.Int32) + ("1" : System.Int32)) AS [d],
                  ("0" : System.Int32) AS [e]
                """ )
            .Go();
    }

    [Fact]
    public void CreateQuery_ForBuilder_ShouldReturnCorrectNode()
    {
        var sut = SqlTableBuilderMock.Create<int>( "foo", [ "a", "b", "c" ], [ "a" ], schemaName: "s" );

        var result = sut.CreateQuery();

        result.ToString()
            .TestEquals(
                """
                FROM [s].[foo]
                ORDER BY ([s].[foo].[a] : System.Int32) ASC
                SELECT
                  ([s].[foo].[a] : System.Int32),
                  ([s].[foo].[b] : System.Int32),
                  ([s].[foo].[c] : System.Int32)
                """ )
            .Go();
    }

    [Fact]
    public void CreateQuery_ForBuilder_ShouldReturnCorrectNode_WithOverriddenSelectionAndOrdering()
    {
        var sut = SqlTableBuilderMock.Create<int>( "foo", [ "a", "b", "c" ], [ "a" ], schemaName: "s" );

        var result = sut.CreateQuery(
            c =>
            {
                if ( c.Name == "b" )
                    return SqlSelectionOverride.Ignore;

                if ( c.Name == "c" )
                    return (c + SqlNode.Literal( 1 )).As( "d" );

                return SqlSelectionOverride.UseDefault;
            },
            _ => [ SqlNode.Literal( 0 ).As( "e" ) ],
            t => [ t["b"].Desc() ] );

        result.ToString()
            .TestEquals(
                """
                FROM [s].[foo]
                ORDER BY ([s].[foo].[b] : System.Int32) DESC
                SELECT
                  ([s].[foo].[a] : System.Int32),
                  (([s].[foo].[c] : System.Int32) + ("1" : System.Int32)) AS [d],
                  ("0" : System.Int32) AS [e]
                """ )
            .Go();
    }

    [Fact]
    public void CreateQuery_ForNew_ShouldReturnCorrectNode()
    {
        var sut = CreateNewTable<int>( "foo", [ "a", "b", "c" ], [ "a" ] );

        var result = sut.CreateQuery();

        result.ToString()
            .TestEquals(
                """
                FROM [s].[foo]
                ORDER BY ([s].[foo].[a] : System.Int32) ASC
                SELECT
                  ([s].[foo].[a] : System.Int32),
                  ([s].[foo].[b] : System.Int32),
                  ([s].[foo].[c] : System.Int32)
                """ )
            .Go();
    }

    [Fact]
    public void CreateQuery_ForNew_ShouldReturnCorrectNode_WithOverriddenSelectionAndOrdering()
    {
        var sut = CreateNewTable<int>( "foo", [ "a", "b", "c" ], [ "a" ] );

        var result = sut.CreateQuery(
            (_, c) =>
            {
                if ( c.Name == "b" )
                    return SqlSelectionOverride.Ignore;

                if ( c.Name == "c" )
                    return (c + SqlNode.Literal( 1 )).As( "d" );

                return SqlSelectionOverride.UseDefault;
            },
            _ => [ SqlNode.Literal( 0 ).As( "e" ) ],
            t => [ t["b"].Desc() ] );

        result.ToString()
            .TestEquals(
                """
                FROM [s].[foo]
                ORDER BY ([s].[foo].[b] : System.Int32) DESC
                SELECT
                  ([s].[foo].[a] : System.Int32),
                  (([s].[foo].[c] : System.Int32) + ("1" : System.Int32)) AS [d],
                  ("0" : System.Int32) AS [e]
                """ )
            .Go();
    }

    [Fact]
    public void CreateInsertInto_ShouldReturnCorrectNode()
    {
        var sut = SqlTableMock.Create<int>( "foo", [ "a", "b", "c" ], [ "a" ], schemaName: "s" );

        var result = sut.CreateInsertInto();

        result.ToString()
            .TestEquals(
                """
                INSERT INTO [s].[foo] ([s].[foo].[a] : System.Int32, [s].[foo].[b] : System.Int32, [s].[foo].[c] : System.Int32)
                VALUES
                ((@a : System.Int32), (@b : System.Int32), (@c : System.Int32))
                """ )
            .Go();
    }

    [Fact]
    public void CreateInsertInto_ShouldReturnCorrectNode_WithOverriddenColumns()
    {
        var sut = SqlTableMock.Create<int>( "foo", [ "a", "b", "c" ], [ "a" ], schemaName: "s" );

        var result = sut.CreateInsertInto( c =>
        {
            if ( c.Name == "b" )
                return SqlExpressionOverride.Ignore;

            if ( c.Name == "c" )
                return SqlNode.Literal( 1 );

            return SqlExpressionOverride.UseDefault;
        } );

        result.ToString()
            .TestEquals(
                """
                INSERT INTO [s].[foo] ([s].[foo].[a] : System.Int32, [s].[foo].[c] : System.Int32)
                VALUES
                ((@a : System.Int32), ("1" : System.Int32))
                """ )
            .Go();
    }

    [Fact]
    public void CreateInsertInto_ShouldIgnoreComputedAndIdentityColumns()
    {
        var builder = SqlTableBuilderMock.Create<int>( "foo", [ "a", "b", "c" ], [ "a" ], schemaName: "s" );
        builder.Columns.Get( "b" ).SetComputation( SqlColumnComputation.Stored( SqlNode.Literal( 1 ) ) );
        builder.Columns.Get( "a" ).SetIdentity( SqlColumnIdentity.Default );
        var sut = builder.Build();

        var result = sut.CreateInsertInto();

        result.ToString()
            .TestEquals(
                """
                INSERT INTO [s].[foo] ([s].[foo].[c] : System.Int32)
                VALUES
                ((@c : System.Int32))
                """ )
            .Go();
    }

    [Fact]
    public void CreateInsertInto_ForBuilder_ShouldReturnCorrectNode()
    {
        var sut = SqlTableBuilderMock.Create<int>( "foo", [ "a", "b", "c" ], [ "a" ], schemaName: "s" );

        var result = sut.CreateInsertInto();

        result.ToString()
            .TestEquals(
                """
                INSERT INTO [s].[foo] ([s].[foo].[a] : System.Int32, [s].[foo].[b] : System.Int32, [s].[foo].[c] : System.Int32)
                VALUES
                ((@a : System.Int32), (@b : System.Int32), (@c : System.Int32))
                """ )
            .Go();
    }

    [Fact]
    public void CreateInsertInto_ForBuilder_ShouldReturnCorrectNode_WithOverriddenColumns()
    {
        var sut = SqlTableBuilderMock.Create<int>( "foo", [ "a", "b", "c" ], [ "a" ], schemaName: "s" );

        var result = sut.CreateInsertInto( c =>
        {
            if ( c.Name == "b" )
                return SqlExpressionOverride.Ignore;

            if ( c.Name == "c" )
                return SqlNode.Literal( 1 );

            return SqlExpressionOverride.UseDefault;
        } );

        result.ToString()
            .TestEquals(
                """
                INSERT INTO [s].[foo] ([s].[foo].[a] : System.Int32, [s].[foo].[c] : System.Int32)
                VALUES
                ((@a : System.Int32), ("1" : System.Int32))
                """ )
            .Go();
    }

    [Fact]
    public void CreateInsertInto_ForBuilder_ShouldIgnoreComputedAndIdentityColumns()
    {
        var sut = SqlTableBuilderMock.Create<int>( "foo", [ "a", "b", "c" ], [ "a" ], schemaName: "s" );
        sut.Columns.Get( "b" ).SetComputation( SqlColumnComputation.Stored( SqlNode.Literal( 1 ) ) );
        sut.Columns.Get( "a" ).SetIdentity( SqlColumnIdentity.Default );

        var result = sut.CreateInsertInto();

        result.ToString()
            .TestEquals(
                """
                INSERT INTO [s].[foo] ([s].[foo].[c] : System.Int32)
                VALUES
                ((@c : System.Int32))
                """ )
            .Go();
    }

    [Fact]
    public void CreateInsertInto_ForNew_ShouldReturnCorrectNode()
    {
        var sut = CreateNewTable<int>( "foo", [ "a", "b", "c" ], [ "a" ] );

        var result = sut.CreateInsertInto();

        result.ToString()
            .TestEquals(
                """
                INSERT INTO [s].[foo] ([s].[foo].[a] : System.Int32, [s].[foo].[b] : System.Int32, [s].[foo].[c] : System.Int32)
                VALUES
                ((@a : System.Int32), (@b : System.Int32), (@c : System.Int32))
                """ )
            .Go();
    }

    [Fact]
    public void CreateInsertInto_ForNew_ShouldReturnCorrectNode_WithOverriddenColumns()
    {
        var sut = CreateNewTable<int>( "foo", [ "a", "b", "c" ], [ "a" ] );

        var result = sut.CreateInsertInto( (c, _) =>
        {
            if ( c.Name == "b" )
                return SqlExpressionOverride.Ignore;

            if ( c.Name == "c" )
                return SqlNode.Literal( 1 );

            return SqlExpressionOverride.UseDefault;
        } );

        result.ToString()
            .TestEquals(
                """
                INSERT INTO [s].[foo] ([s].[foo].[a] : System.Int32, [s].[foo].[c] : System.Int32)
                VALUES
                ((@a : System.Int32), ("1" : System.Int32))
                """ )
            .Go();
    }

    [Fact]
    public void CreateInsertInto_ForNew_ShouldIgnoreComputedAndIdentityColumns()
    {
        var sut = CreateNewTable<int>( "foo", [ "a", "b", "c" ], [ "a" ], computedColumn: "b", identityColumn: "a" );

        var result = sut.CreateInsertInto();

        result.ToString()
            .TestEquals(
                """
                INSERT INTO [s].[foo] ([s].[foo].[c] : System.Int32)
                VALUES
                ((@c : System.Int32))
                """ )
            .Go();
    }

    [Fact]
    public void CreateDeleteFrom_ShouldReturnCorrectNode()
    {
        var sut = SqlTableMock.Create<int>( "foo", [ "a", "b", "c" ], [ "a", "b" ], schemaName: "s" );

        var result = sut.CreateDeleteFrom();

        result.ToString()
            .TestEquals(
                """
                DELETE FROM [s].[foo]
                AND WHERE (([s].[foo].[a] : System.Int32) == (@a : System.Int32)) AND (([s].[foo].[b] : System.Int32) == (@b : System.Int32))
                """ )
            .Go();
    }

    [Fact]
    public void CreateDeleteFrom_ShouldReturnCorrectNode_WithFilterExtension()
    {
        var sut = SqlTableMock.Create<int>( "foo", [ "a", "b", "c" ], [ "a" ], schemaName: "s" );

        var result = sut.CreateDeleteFrom( t => t["c"] > SqlNode.Literal( 0 ) );

        result.ToString()
            .TestEquals(
                """
                DELETE FROM [s].[foo]
                AND WHERE (([s].[foo].[a] : System.Int32) == (@a : System.Int32)) AND (([s].[foo].[c] : System.Int32) > ("0" : System.Int32))
                """ )
            .Go();
    }

    [Fact]
    public void CreateDeleteFrom_ForBuilder_ShouldReturnCorrectNode()
    {
        var sut = SqlTableBuilderMock.Create<int>( "foo", [ "a", "b", "c" ], [ "a", "b" ], schemaName: "s" );

        var result = sut.CreateDeleteFrom();

        result.ToString()
            .TestEquals(
                """
                DELETE FROM [s].[foo]
                AND WHERE (([s].[foo].[a] : System.Int32) == (@a : System.Int32)) AND (([s].[foo].[b] : System.Int32) == (@b : System.Int32))
                """ )
            .Go();
    }

    [Fact]
    public void CreateDeleteFrom_ForBuilder_ShouldReturnCorrectNode_WithFilterExtension()
    {
        var sut = SqlTableBuilderMock.Create<int>( "foo", [ "a", "b", "c" ], [ "a" ], schemaName: "s" );

        var result = sut.CreateDeleteFrom( t => t["c"] > SqlNode.Literal( 0 ) );

        result.ToString()
            .TestEquals(
                """
                DELETE FROM [s].[foo]
                AND WHERE (([s].[foo].[a] : System.Int32) == (@a : System.Int32)) AND (([s].[foo].[c] : System.Int32) > ("0" : System.Int32))
                """ )
            .Go();
    }

    [Fact]
    public void CreateDeleteFrom_ForNew_ShouldReturnCorrectNode()
    {
        var sut = CreateNewTable<int>( "foo", [ "a", "b", "c" ], [ "a", "b" ] );

        var result = sut.CreateDeleteFrom();

        result.ToString()
            .TestEquals(
                """
                DELETE FROM [s].[foo]
                AND WHERE (([s].[foo].[a] : System.Int32) == (@a : System.Int32)) AND (([s].[foo].[b] : System.Int32) == (@b : System.Int32))
                """ )
            .Go();
    }

    [Fact]
    public void CreateDeleteFrom_ForNew_ShouldReturnCorrectNode_WithFilterExtension()
    {
        var sut = CreateNewTable<int>( "foo", [ "a", "b", "c" ], [ "a" ] );

        var result = sut.CreateDeleteFrom( t => t["c"] > SqlNode.Literal( 0 ) );

        result.ToString()
            .TestEquals(
                """
                DELETE FROM [s].[foo]
                AND WHERE (([s].[foo].[a] : System.Int32) == (@a : System.Int32)) AND (([s].[foo].[c] : System.Int32) > ("0" : System.Int32))
                """ )
            .Go();
    }

    [Fact]
    public void CreateDeleteFrom_ForNew_ShouldThrowInvalidOperationException_WhenPrimaryKeyIsNotDefined()
    {
        var sut = CreateNewTable<int>( "foo", [ "a" ] );
        var action = Lambda.Of( () => sut.CreateDeleteFrom() );
        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void CreateDeleteFrom_ForNew_ShouldThrowInvalidOperationException_WhenPrimaryKeyContainsInvalidColumn()
    {
        var sut = CreateNewTable<int>( "foo", [ "a", "b" ], [ "a" ], isPkInvalid: true );
        var action = Lambda.Of( () => sut.CreateDeleteFrom() );
        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void CreateUpdate_ShouldReturnCorrectNode()
    {
        var sut = SqlTableMock.Create<int>( "foo", [ "a", "b", "c", "d", "e" ], [ "a", "b" ], schemaName: "s" );

        var result = sut.CreateUpdate();

        result.ToString()
            .TestEquals(
                """
                UPDATE FROM [s].[foo]
                AND WHERE (([s].[foo].[a] : System.Int32) == (@a : System.Int32)) AND (([s].[foo].[b] : System.Int32) == (@b : System.Int32))
                SET
                  ([s].[foo].[c] : System.Int32) = (@c : System.Int32),
                  ([s].[foo].[d] : System.Int32) = (@d : System.Int32),
                  ([s].[foo].[e] : System.Int32) = (@e : System.Int32)
                """ )
            .Go();
    }

    [Fact]
    public void CreateUpdate_ShouldReturnCorrectNode_WithOverriddenColumnsAndFilterExtension()
    {
        var sut = SqlTableMock.Create<int>( "foo", [ "a", "b", "c", "d", "e" ], [ "a" ], schemaName: "s" );

        var result = sut.CreateUpdate(
            c =>
            {
                if ( c.Name == "d" )
                    return SqlExpressionOverride.Ignore;

                if ( c.Name == "e" )
                    return SqlNode.Literal( 1 );

                return SqlExpressionOverride.UseDefault;
            },
            t => t["c"] > SqlNode.Literal( 0 ) );

        result.ToString()
            .TestEquals(
                """
                UPDATE FROM [s].[foo]
                AND WHERE (([s].[foo].[a] : System.Int32) == (@a : System.Int32)) AND (([s].[foo].[c] : System.Int32) > ("0" : System.Int32))
                SET
                  ([s].[foo].[b] : System.Int32) = (@b : System.Int32),
                  ([s].[foo].[c] : System.Int32) = (@c : System.Int32),
                  ([s].[foo].[e] : System.Int32) = ("1" : System.Int32)
                """ )
            .Go();
    }

    [Fact]
    public void CreateUpdate_ShouldIgnoreComputedAndIdentityColumns()
    {
        var builder = SqlTableBuilderMock.Create<int>( "foo", [ "a", "b", "c" ], [ "a" ], schemaName: "s" );
        builder.Columns.Get( "b" ).SetComputation( SqlColumnComputation.Stored( SqlNode.Literal( 1 ) ) );
        builder.Columns.Get( "a" ).SetIdentity( SqlColumnIdentity.Default );
        var sut = builder.Build();

        var result = sut.CreateUpdate();

        result.ToString()
            .TestEquals(
                """
                UPDATE FROM [s].[foo]
                AND WHERE ([s].[foo].[a] : System.Int32) == (@a : System.Int32)
                SET
                  ([s].[foo].[c] : System.Int32) = (@c : System.Int32)
                """ )
            .Go();
    }

    [Fact]
    public void CreateUpdate_ForBuilder_ShouldReturnCorrectNode()
    {
        var sut = SqlTableBuilderMock.Create<int>( "foo", [ "a", "b", "c", "d", "e" ], [ "a", "b" ], schemaName: "s" );

        var result = sut.CreateUpdate();

        result.ToString()
            .TestEquals(
                """
                UPDATE FROM [s].[foo]
                AND WHERE (([s].[foo].[a] : System.Int32) == (@a : System.Int32)) AND (([s].[foo].[b] : System.Int32) == (@b : System.Int32))
                SET
                  ([s].[foo].[c] : System.Int32) = (@c : System.Int32),
                  ([s].[foo].[d] : System.Int32) = (@d : System.Int32),
                  ([s].[foo].[e] : System.Int32) = (@e : System.Int32)
                """ )
            .Go();
    }

    [Fact]
    public void CreateUpdate_ForBuilder_ShouldReturnCorrectNode_WithOverriddenColumnsAndFilterExtension()
    {
        var sut = SqlTableBuilderMock.Create<int>( "foo", [ "a", "b", "c", "d", "e" ], [ "a" ], schemaName: "s" );

        var result = sut.CreateUpdate(
            c =>
            {
                if ( c.Name == "d" )
                    return SqlExpressionOverride.Ignore;

                if ( c.Name == "e" )
                    return SqlNode.Literal( 1 );

                return SqlExpressionOverride.UseDefault;
            },
            t => t["c"] > SqlNode.Literal( 0 ) );

        result.ToString()
            .TestEquals(
                """
                UPDATE FROM [s].[foo]
                AND WHERE (([s].[foo].[a] : System.Int32) == (@a : System.Int32)) AND (([s].[foo].[c] : System.Int32) > ("0" : System.Int32))
                SET
                  ([s].[foo].[b] : System.Int32) = (@b : System.Int32),
                  ([s].[foo].[c] : System.Int32) = (@c : System.Int32),
                  ([s].[foo].[e] : System.Int32) = ("1" : System.Int32)
                """ )
            .Go();
    }

    [Fact]
    public void CreateUpdate_ForBuilder_ShouldIgnoreComputedAndIdentityColumns()
    {
        var sut = SqlTableBuilderMock.Create<int>( "foo", [ "a", "b", "c" ], [ "a" ], schemaName: "s" );
        sut.Columns.Get( "b" ).SetComputation( SqlColumnComputation.Stored( SqlNode.Literal( 1 ) ) );
        sut.Columns.Get( "a" ).SetIdentity( SqlColumnIdentity.Default );

        var result = sut.CreateUpdate();

        result.ToString()
            .TestEquals(
                """
                UPDATE FROM [s].[foo]
                AND WHERE ([s].[foo].[a] : System.Int32) == (@a : System.Int32)
                SET
                  ([s].[foo].[c] : System.Int32) = (@c : System.Int32)
                """ )
            .Go();
    }

    [Fact]
    public void CreateUpdate_ForNew_ShouldReturnCorrectNode()
    {
        var sut = CreateNewTable<int>( "foo", [ "a", "b", "c", "d", "e" ], [ "a", "b" ] );

        var result = sut.CreateUpdate();

        result.ToString()
            .TestEquals(
                """
                UPDATE FROM [s].[foo]
                AND WHERE (([s].[foo].[a] : System.Int32) == (@a : System.Int32)) AND (([s].[foo].[b] : System.Int32) == (@b : System.Int32))
                SET
                  ([s].[foo].[c] : System.Int32) = (@c : System.Int32),
                  ([s].[foo].[d] : System.Int32) = (@d : System.Int32),
                  ([s].[foo].[e] : System.Int32) = (@e : System.Int32)
                """ )
            .Go();
    }

    [Fact]
    public void CreateUpdate_ForNew_ShouldReturnCorrectNode_WithOverriddenColumnsAndFilterExtension()
    {
        var sut = CreateNewTable<int>( "foo", [ "a", "b", "c", "d", "e" ], [ "a" ] );

        var result = sut.CreateUpdate(
            (c, _) =>
            {
                if ( c.Name == "d" )
                    return SqlExpressionOverride.Ignore;

                if ( c.Name == "e" )
                    return SqlNode.Literal( 1 );

                return SqlExpressionOverride.UseDefault;
            },
            t => t["c"] > SqlNode.Literal( 0 ) );

        result.ToString()
            .TestEquals(
                """
                UPDATE FROM [s].[foo]
                AND WHERE (([s].[foo].[a] : System.Int32) == (@a : System.Int32)) AND (([s].[foo].[c] : System.Int32) > ("0" : System.Int32))
                SET
                  ([s].[foo].[b] : System.Int32) = (@b : System.Int32),
                  ([s].[foo].[c] : System.Int32) = (@c : System.Int32),
                  ([s].[foo].[e] : System.Int32) = ("1" : System.Int32)
                """ )
            .Go();
    }

    [Fact]
    public void CreateUpdate_ForNew_ShouldIgnoreComputedAndIdentityColumns()
    {
        var sut = CreateNewTable<int>( "foo", [ "a", "b", "c" ], [ "a" ], computedColumn: "b", identityColumn: "a" );

        var result = sut.CreateUpdate();

        result.ToString()
            .TestEquals(
                """
                UPDATE FROM [s].[foo]
                AND WHERE ([s].[foo].[a] : System.Int32) == (@a : System.Int32)
                SET
                  ([s].[foo].[c] : System.Int32) = (@c : System.Int32)
                """ )
            .Go();
    }

    [Fact]
    public void CreateUpdate_ForNew_ShouldThrowInvalidOperationException_WhenPrimaryKeyIsNotDefined()
    {
        var sut = CreateNewTable<int>( "foo", [ "a" ] );
        var action = Lambda.Of( () => sut.CreateUpdate() );
        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void CreateUpdate_ForNew_ShouldThrowInvalidOperationException_WhenPrimaryKeyContainsInvalidColumn()
    {
        var sut = CreateNewTable<int>( "foo", [ "a", "b" ], [ "a" ], isPkInvalid: true );
        var action = Lambda.Of( () => sut.CreateUpdate() );
        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void CreateUpsert_ShouldReturnCorrectNode()
    {
        var sut = SqlTableMock.Create<int>( "foo", [ "a", "b", "c", "d", "e" ], [ "a", "b" ], schemaName: "s" );

        var result = sut.CreateUpsert();

        result.ToString()
            .TestEquals(
                """
                UPSERT [s].[foo] USING
                VALUES
                ((@a : System.Int32), (@b : System.Int32), (@c : System.Int32), (@d : System.Int32), (@e : System.Int32))
                WITH CONFLICT TARGET ([s].[foo].[a] : System.Int32, [s].[foo].[b] : System.Int32)
                INSERT ([s].[foo].[a] : System.Int32, [s].[foo].[b] : System.Int32, [s].[foo].[c] : System.Int32, [s].[foo].[d] : System.Int32, [s].[foo].[e] : System.Int32)
                ON CONFLICT SET
                  ([s].[foo].[c] : System.Int32) = ([<internal>].[c] : System.Int32),
                  ([s].[foo].[d] : System.Int32) = ([<internal>].[d] : System.Int32),
                  ([s].[foo].[e] : System.Int32) = ([<internal>].[e] : System.Int32)
                """ )
            .Go();
    }

    [Fact]
    public void CreateUpsert_ShouldReturnCorrectNode_WithOverriddenColumnsAndUpdateFilter()
    {
        var sut = SqlTableMock.Create<int>( "foo", [ "a", "b", "c", "d", "e" ], [ "a" ], schemaName: "s" );

        var result = sut.CreateUpsert(
            c =>
            {
                if ( c.Name == "d" )
                    return SqlExpressionOverride.Ignore;

                if ( c.Name == "e" )
                    return SqlNode.Literal( 1 );

                return SqlExpressionOverride.UseDefault;
            },
            (c, d) =>
            {
                if ( c.Name == "b" )
                    return SqlExpressionOverride.Ignore;

                if ( c.Name == "c" )
                    return d + SqlNode.Literal( 10 );

                return SqlExpressionOverride.UseDefault;
            },
            (t, r) => t["b"] == r["b"] );

        result.ToString()
            .TestEquals(
                """
                UPSERT [s].[foo] USING
                VALUES
                ((@a : System.Int32), (@b : System.Int32), (@c : System.Int32), ("1" : System.Int32))
                WITH CONFLICT TARGET ([s].[foo].[a] : System.Int32)
                INSERT ([s].[foo].[a] : System.Int32, [s].[foo].[b] : System.Int32, [s].[foo].[c] : System.Int32, [s].[foo].[e] : System.Int32)
                ON CONFLICT SET
                  ([s].[foo].[c] : System.Int32) = (([<internal>].[c] : System.Int32) + ("10" : System.Int32)),
                  ([s].[foo].[d] : System.Int32) = ([<internal>].[d] : System.Int32),
                  ([s].[foo].[e] : System.Int32) = ([<internal>].[e] : System.Int32)
                WHERE ([s].[foo].[b] : System.Int32) == ([<internal>].[b] : System.Int32)
                """ )
            .Go();
    }

    [Fact]
    public void CreateUpsert_ShouldIgnoreComputedAndIdentityColumns()
    {
        var builder = SqlTableBuilderMock.Create<int>( "foo", [ "a", "b", "c" ], [ "a" ], schemaName: "s" );
        builder.Columns.Get( "b" ).SetComputation( SqlColumnComputation.Stored( SqlNode.Literal( 1 ) ) );
        builder.Columns.Get( "a" ).SetIdentity( SqlColumnIdentity.Default );
        var sut = builder.Build();

        var result = sut.CreateUpsert();

        result.ToString()
            .TestEquals(
                """
                UPSERT [s].[foo] USING
                VALUES
                ((@c : System.Int32))
                WITH CONFLICT TARGET ([s].[foo].[a] : System.Int32)
                INSERT ([s].[foo].[c] : System.Int32)
                ON CONFLICT SET
                  ([s].[foo].[c] : System.Int32) = ([<internal>].[c] : System.Int32)
                """ )
            .Go();
    }

    [Fact]
    public void CreateUpsert_ForBuilder_ShouldReturnCorrectNode()
    {
        var sut = SqlTableBuilderMock.Create<int>( "foo", [ "a", "b", "c", "d", "e" ], [ "a", "b" ], schemaName: "s" );

        var result = sut.CreateUpsert();

        result.ToString()
            .TestEquals(
                """
                UPSERT [s].[foo] USING
                VALUES
                ((@a : System.Int32), (@b : System.Int32), (@c : System.Int32), (@d : System.Int32), (@e : System.Int32))
                WITH CONFLICT TARGET ([s].[foo].[a] : System.Int32, [s].[foo].[b] : System.Int32)
                INSERT ([s].[foo].[a] : System.Int32, [s].[foo].[b] : System.Int32, [s].[foo].[c] : System.Int32, [s].[foo].[d] : System.Int32, [s].[foo].[e] : System.Int32)
                ON CONFLICT SET
                  ([s].[foo].[c] : System.Int32) = ([<internal>].[c] : System.Int32),
                  ([s].[foo].[d] : System.Int32) = ([<internal>].[d] : System.Int32),
                  ([s].[foo].[e] : System.Int32) = ([<internal>].[e] : System.Int32)
                """ )
            .Go();
    }

    [Fact]
    public void CreateUpsert_ForBuilder_ShouldReturnCorrectNode_WithOverriddenColumnsAndUpdateFilter()
    {
        var sut = SqlTableBuilderMock.Create<int>( "foo", [ "a", "b", "c", "d", "e" ], [ "a" ], schemaName: "s" );

        var result = sut.CreateUpsert(
            c =>
            {
                if ( c.Name == "d" )
                    return SqlExpressionOverride.Ignore;

                if ( c.Name == "e" )
                    return SqlNode.Literal( 1 );

                return SqlExpressionOverride.UseDefault;
            },
            (c, d) =>
            {
                if ( c.Name == "b" )
                    return SqlExpressionOverride.Ignore;

                if ( c.Name == "c" )
                    return d + SqlNode.Literal( 10 );

                return SqlExpressionOverride.UseDefault;
            },
            (t, r) => t["b"] == r["b"] );

        result.ToString()
            .TestEquals(
                """
                UPSERT [s].[foo] USING
                VALUES
                ((@a : System.Int32), (@b : System.Int32), (@c : System.Int32), ("1" : System.Int32))
                WITH CONFLICT TARGET ([s].[foo].[a] : System.Int32)
                INSERT ([s].[foo].[a] : System.Int32, [s].[foo].[b] : System.Int32, [s].[foo].[c] : System.Int32, [s].[foo].[e] : System.Int32)
                ON CONFLICT SET
                  ([s].[foo].[c] : System.Int32) = (([<internal>].[c] : System.Int32) + ("10" : System.Int32)),
                  ([s].[foo].[d] : System.Int32) = ([<internal>].[d] : System.Int32),
                  ([s].[foo].[e] : System.Int32) = ([<internal>].[e] : System.Int32)
                WHERE ([s].[foo].[b] : System.Int32) == ([<internal>].[b] : System.Int32)
                """ )
            .Go();
    }

    [Fact]
    public void CreateUpsert_ForBuilder_ShouldIgnoreComputedAndIdentityColumns()
    {
        var sut = SqlTableBuilderMock.Create<int>( "foo", [ "a", "b", "c" ], [ "a" ], schemaName: "s" );
        sut.Columns.Get( "b" ).SetComputation( SqlColumnComputation.Stored( SqlNode.Literal( 1 ) ) );
        sut.Columns.Get( "a" ).SetIdentity( SqlColumnIdentity.Default );

        var result = sut.CreateUpsert();

        result.ToString()
            .TestEquals(
                """
                UPSERT [s].[foo] USING
                VALUES
                ((@c : System.Int32))
                WITH CONFLICT TARGET ([s].[foo].[a] : System.Int32)
                INSERT ([s].[foo].[c] : System.Int32)
                ON CONFLICT SET
                  ([s].[foo].[c] : System.Int32) = ([<internal>].[c] : System.Int32)
                """ )
            .Go();
    }

    [Fact]
    public void CreateUpsert_ForNew_ShouldReturnCorrectNode()
    {
        var sut = CreateNewTable<int>( "foo", [ "a", "b", "c", "d", "e" ], [ "a", "b" ] );

        var result = sut.CreateUpsert();

        result.ToString()
            .TestEquals(
                """
                UPSERT [s].[foo] USING
                VALUES
                ((@a : System.Int32), (@b : System.Int32), (@c : System.Int32), (@d : System.Int32), (@e : System.Int32))
                WITH CONFLICT TARGET ([s].[foo].[a] : System.Int32, [s].[foo].[b] : System.Int32)
                INSERT ([s].[foo].[a] : System.Int32, [s].[foo].[b] : System.Int32, [s].[foo].[c] : System.Int32, [s].[foo].[d] : System.Int32, [s].[foo].[e] : System.Int32)
                ON CONFLICT SET
                  ([s].[foo].[c] : System.Int32) = ([<internal>].[c] : System.Int32),
                  ([s].[foo].[d] : System.Int32) = ([<internal>].[d] : System.Int32),
                  ([s].[foo].[e] : System.Int32) = ([<internal>].[e] : System.Int32)
                """ )
            .Go();
    }

    [Fact]
    public void CreateUpsert_ForNew_ShouldReturnCorrectNode_WithOverriddenColumnsAndUpdateFilter()
    {
        var sut = CreateNewTable<int>( "foo", [ "a", "b", "c", "d", "e" ], [ "a" ] );

        var result = sut.CreateUpsert(
            (c, _) =>
            {
                if ( c.Name == "d" )
                    return SqlExpressionOverride.Ignore;

                if ( c.Name == "e" )
                    return SqlNode.Literal( 1 );

                return SqlExpressionOverride.UseDefault;
            },
            (c, _, d) =>
            {
                if ( c.Name == "b" )
                    return SqlExpressionOverride.Ignore;

                if ( c.Name == "c" )
                    return d + SqlNode.Literal( 10 );

                return SqlExpressionOverride.UseDefault;
            },
            (t, r) => t["b"] == r["b"] );

        result.ToString()
            .TestEquals(
                """
                UPSERT [s].[foo] USING
                VALUES
                ((@a : System.Int32), (@b : System.Int32), (@c : System.Int32), ("1" : System.Int32))
                WITH CONFLICT TARGET ([s].[foo].[a] : System.Int32)
                INSERT ([s].[foo].[a] : System.Int32, [s].[foo].[b] : System.Int32, [s].[foo].[c] : System.Int32, [s].[foo].[e] : System.Int32)
                ON CONFLICT SET
                  ([s].[foo].[c] : System.Int32) = (([<internal>].[c] : System.Int32) + ("10" : System.Int32)),
                  ([s].[foo].[d] : System.Int32) = ([<internal>].[d] : System.Int32),
                  ([s].[foo].[e] : System.Int32) = ([<internal>].[e] : System.Int32)
                WHERE ([s].[foo].[b] : System.Int32) == ([<internal>].[b] : System.Int32)
                """ )
            .Go();
    }

    [Fact]
    public void CreateUpsert_ForNew_ShouldIgnoreComputedAndIdentityColumns()
    {
        var sut = CreateNewTable<int>( "foo", [ "a", "b", "c" ], [ "a" ], computedColumn: "b", identityColumn: "a" );

        var result = sut.CreateUpsert();

        result.ToString()
            .TestEquals(
                """
                UPSERT [s].[foo] USING
                VALUES
                ((@c : System.Int32))
                WITH CONFLICT TARGET ([s].[foo].[a] : System.Int32)
                INSERT ([s].[foo].[c] : System.Int32)
                ON CONFLICT SET
                  ([s].[foo].[c] : System.Int32) = ([<internal>].[c] : System.Int32)
                """ )
            .Go();
    }

    [Fact]
    public void CreateUpsert_ForNew_ShouldThrowInvalidOperationException_WhenPrimaryKeyIsNotDefined()
    {
        var sut = CreateNewTable<int>( "foo", [ "a" ] );
        var action = Lambda.Of( () => sut.CreateUpsert() );
        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void CreateUpsert_ForNew_ShouldThrowInvalidOperationException_WhenPrimaryKeyContainsInvalidColumn()
    {
        var sut = CreateNewTable<int>( "foo", [ "a", "b" ], [ "a" ], isPkInvalid: true );
        var action = Lambda.Of( () => sut.CreateUpsert() );
        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void CreateTempTable_ShouldReturnCorrectNode()
    {
        var sut = SqlTableMock.Create<int>( "foo", [ "a", "b", "c" ], [ "a" ] );

        var result = sut.CreateTempTable( "bar" );

        result.ToString()
            .TestEquals(
                """
                CREATE TABLE TEMP.[bar] (
                  [a] : System.Int32,
                  [b] : System.Int32,
                  [c] : System.Int32
                )
                """ )
            .Go();
    }

    [Fact]
    public void CreateTempTable_ShouldReturnCorrectNode_WithOverriddenColumnsAndConstraints()
    {
        var sut = SqlTableMock.Create<int>( "foo", [ "a", "b", "c", "d" ], [ "a" ] );

        var result = sut.CreateTempTable(
            "bar",
            c =>
            {
                if ( c.Name == "c" )
                    return SqlColumnDefinitionOverride.Ignore;

                if ( c.Name == "d" )
                    return SqlNode.Column<string>( "e" );

                return SqlColumnDefinitionOverride.UseDefault;
            },
            ifNotExists: true,
            (_, t) => SqlCreateTableConstraints.Empty.WithPrimaryKey(
                SqlNode.PrimaryKey( SqlSchemaObjectName.Create( "PK_bar" ), new[] { t["a"].Asc() } ) ) );

        result.ToString()
            .TestEquals(
                """
                CREATE TABLE IF NOT EXISTS TEMP.[bar] (
                  [a] : System.Int32,
                  [b] : System.Int32,
                  [e] : System.String,
                  PRIMARY KEY [PK_bar] ((TEMP.[bar].[a] : System.Int32) ASC)
                )
                """ )
            .Go();
    }

    [Fact]
    public void CreateTempTable_ForBuilder_ShouldReturnCorrectNode()
    {
        var sut = SqlTableBuilderMock.Create<int>( "foo", [ "a", "b", "c" ], [ "a" ] );

        var result = sut.CreateTempTable( "bar" );

        result.ToString()
            .TestEquals(
                """
                CREATE TABLE TEMP.[bar] (
                  [a] : System.Int32,
                  [b] : System.Int32,
                  [c] : System.Int32
                )
                """ )
            .Go();
    }

    [Fact]
    public void CreateTempTable_ForBuilder_ShouldReturnCorrectNode_WithOverriddenColumnsAndConstraints()
    {
        var sut = SqlTableBuilderMock.Create<int>( "foo", [ "a", "b", "c", "d" ], [ "a" ] );

        var result = sut.CreateTempTable(
            "bar",
            c =>
            {
                if ( c.Name == "c" )
                    return SqlColumnDefinitionOverride.Ignore;

                if ( c.Name == "d" )
                    return SqlNode.Column<string>( "e" );

                return SqlColumnDefinitionOverride.UseDefault;
            },
            ifNotExists: true,
            (_, t) => SqlCreateTableConstraints.Empty.WithPrimaryKey(
                SqlNode.PrimaryKey( SqlSchemaObjectName.Create( "PK_bar" ), new[] { t["a"].Asc() } ) ) );

        result.ToString()
            .TestEquals(
                """
                CREATE TABLE IF NOT EXISTS TEMP.[bar] (
                  [a] : System.Int32,
                  [b] : System.Int32,
                  [e] : System.String,
                  PRIMARY KEY [PK_bar] ((TEMP.[bar].[a] : System.Int32) ASC)
                )
                """ )
            .Go();
    }

    [Pure]
    private static SqlNewTableNode CreateNewTable<T>(
        string name,
        string[] columns,
        string[]? pkColumns = null,
        string? computedColumn = null,
        string? identityColumn = null,
        bool isPkInvalid = false)
        where T : notnull
    {
        return SqlNode.CreateTable(
                SqlRecordSetInfo.Create( "s", name ),
                columns.Select( c => SqlNode.Column<T>(
                        c,
                        computation: c == computedColumn ? SqlColumnComputation.Stored( SqlNode.Literal( 1 ) ) : null,
                        identity: c == identityColumn ? SqlColumnIdentity.Default : null ) )
                    .ToArray(),
                constraintsProvider: t =>
                {
                    var result = SqlCreateTableConstraints.Empty;
                    if ( pkColumns is not null && pkColumns.Length > 0 )
                        result = result.WithPrimaryKey(
                            SqlNode.PrimaryKey(
                                SqlSchemaObjectName.Create( "s", $"PK_{name}" ),
                                pkColumns.Select( c => isPkInvalid ? SqlNode.Literal( 0 ).Asc() : t[c].Asc() ).ToArray() ) );

                    return result;
                } )
            .RecordSet;
    }
}
