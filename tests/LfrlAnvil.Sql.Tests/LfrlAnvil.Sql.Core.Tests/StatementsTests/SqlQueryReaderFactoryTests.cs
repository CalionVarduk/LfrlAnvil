using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Threading.Tasks;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Statements.Compilers;
using LfrlAnvil.Sql.Tests.Helpers;
using LfrlAnvil.Sql.Tests.Helpers.Data;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlQueryReaderFactoryTests : TestsBase
{
    [Fact]
    public void Create_TypeErased_ShouldCreateQueryReaderThatReturnsCorrectResult_WhenDataReaderIsEmpty()
    {
        var reader = new DbDataReaderMock();
        var sut = Factory.CreateFactory();
        var queryReader = sut.Create();

        var result = queryReader.Read( reader );

        using ( new AssertionScope() )
        {
            queryReader.Dialect.Should().BeSameAs( sut.Dialect );
            result.IsEmpty.Should().BeTrue();
            result.Rows.Should().BeNull();
            result.ResultSetFields.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void Create_TypeErased_ShouldCreateQueryReaderThatReturnsCorrectResult_WhenDataReaderIsNotEmpty()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[]
                {
                    new object?[] { 1, "foo", 5.0, true },
                    new object?[] { 2, "bar", null, false },
                    new object?[] { 3, "lorem", 10.0, null }
                } ) );

        var sut = Factory.CreateFactory();
        var queryReader = sut.Create();

        var result = queryReader.Read( reader );

        using ( new AssertionScope() )
        {
            queryReader.Dialect.Should().BeSameAs( sut.Dialect );
            result.IsEmpty.Should().BeFalse();
            result.Rows.Should().NotBeNull();
            (result.Rows?.Count).Should().Be( 3 );
            (result.Rows?[0].AsSpan().ToArray()).Should().BeSequentiallyEqualTo( 1, "foo", 5.0, true );
            (result.Rows?[1].AsSpan().ToArray()).Should().BeSequentiallyEqualTo( 2, "bar", null, false );
            (result.Rows?[2].AsSpan().ToArray()).Should().BeSequentiallyEqualTo( 3, "lorem", 10.0, null );
            result.ResultSetFields.ToArray()
                .Should()
                .BeSequentiallyEqualTo(
                    new SqlResultSetField( 0, "a" ),
                    new SqlResultSetField( 1, "b" ),
                    new SqlResultSetField( 2, "c" ),
                    new SqlResultSetField( 3, "d" ) );
        }
    }

    [Fact]
    public void Create_TypeErased_ShouldCreateQueryReaderThatReturnsCorrectResult_WhenDataReaderIsEmpty_WithIncludedFieldTypes()
    {
        var reader = new DbDataReaderMock();
        var sut = Factory.CreateFactory();
        var queryReader = sut.Create(
            SqlQueryReaderCreationOptions.Default.SetResultSetFieldsPersistenceMode(
                SqlQueryReaderResultSetFieldsPersistenceMode.PersistWithTypes ) );

        var result = queryReader.Read( reader );

        using ( new AssertionScope() )
        {
            queryReader.Dialect.Should().BeSameAs( sut.Dialect );
            result.IsEmpty.Should().BeTrue();
            result.Rows.Should().BeNull();
            result.ResultSetFields.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void Create_TypeErased_ShouldCreateQueryReaderThatReturnsCorrectResult_WhenDataReaderIsNotEmpty_WithIncludedFieldTypes()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[]
                {
                    new object?[] { 1, "foo", 5.0, true },
                    new object?[] { 2, "bar", null, false },
                    new object?[] { 3, "lorem", 10.0, null }
                } ) );

        var sut = Factory.CreateFactory();
        var queryReader = sut.Create(
            SqlQueryReaderCreationOptions.Default.SetResultSetFieldsPersistenceMode(
                SqlQueryReaderResultSetFieldsPersistenceMode.PersistWithTypes ) );

        var result = queryReader.Read( reader );

        using ( new AssertionScope() )
        {
            queryReader.Dialect.Should().BeSameAs( sut.Dialect );
            result.IsEmpty.Should().BeFalse();
            result.Rows.Should().HaveCount( 3 );
            (result.Rows?[0].AsSpan().ToArray()).Should().BeSequentiallyEqualTo( 1, "foo", 5.0, true );
            (result.Rows?[1].AsSpan().ToArray()).Should().BeSequentiallyEqualTo( 2, "bar", null, false );
            (result.Rows?[2].AsSpan().ToArray()).Should().BeSequentiallyEqualTo( 3, "lorem", 10.0, null );
            result.ResultSetFields[0].Ordinal.Should().Be( 0 );
            result.ResultSetFields[0].Name.Should().Be( "a" );
            result.ResultSetFields[0].IsUsed.Should().BeTrue();
            result.ResultSetFields[0].TypeNames.ToArray().Should().BeSequentiallyEqualTo( "int32" );
            result.ResultSetFields[1].Ordinal.Should().Be( 1 );
            result.ResultSetFields[1].Name.Should().Be( "b" );
            result.ResultSetFields[1].IsUsed.Should().BeTrue();
            result.ResultSetFields[1].TypeNames.ToArray().Should().BeSequentiallyEqualTo( "string" );
            result.ResultSetFields[2].Ordinal.Should().Be( 2 );
            result.ResultSetFields[2].Name.Should().Be( "c" );
            result.ResultSetFields[2].IsUsed.Should().BeTrue();
            result.ResultSetFields[2].TypeNames.ToArray().Should().BeSequentiallyEqualTo( "double", "NULL" );
            result.ResultSetFields[3].Ordinal.Should().Be( 3 );
            result.ResultSetFields[3].Name.Should().Be( "d" );
            result.ResultSetFields[3].IsUsed.Should().BeTrue();
            result.ResultSetFields[3].TypeNames.ToArray().Should().BeSequentiallyEqualTo( "boolean", "NULL" );
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateQueryReaderThatReturnsCorrectResult_WhenDataReaderIsEmpty()
    {
        var reader = new DbDataReaderMock();
        var sut = Factory.CreateFactory();
        var queryReader = sut.Create<Row>();

        var result = queryReader.Read( reader );

        using ( new AssertionScope() )
        {
            queryReader.Dialect.Should().BeSameAs( sut.Dialect );
            result.IsEmpty.Should().BeTrue();
            result.Rows.Should().BeNull();
            result.ResultSetFields.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateQueryReaderThatReturnsCorrectResult_WhenDataReaderIsEmpty_WithIncludedFields()
    {
        var reader = new DbDataReaderMock();
        var sut = Factory.CreateFactory();
        var queryReader = sut.Create<Row>(
            SqlQueryReaderCreationOptions.Default.SetResultSetFieldsPersistenceMode(
                SqlQueryReaderResultSetFieldsPersistenceMode.Persist ) );

        var result = queryReader.Read( reader );

        using ( new AssertionScope() )
        {
            queryReader.Dialect.Should().BeSameAs( sut.Dialect );
            result.IsEmpty.Should().BeTrue();
            result.Rows.Should().BeNull();
            result.ResultSetFields.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateQueryReaderThatReturnsCorrectResult_WhenDataReaderIsNotEmpty()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[]
                {
                    new object?[] { 1, "foo", 5.0, true },
                    new object?[] { 2, "bar", null, false },
                    new object?[] { 3, "lorem", 10.0, null }
                } ) );

        var sut = Factory.CreateFactory();
        var queryReader = sut.Create<Row>();
        var result = queryReader.Read( reader );

        using ( new AssertionScope() )
        {
            queryReader.Dialect.Should().BeSameAs( sut.Dialect );
            result.IsEmpty.Should().BeFalse();
            result.Rows.Should().HaveCount( 3 );
            result.Rows.Should().BeInAscendingOrder( r => r.A );
            result.Rows.Should()
                .BeEquivalentTo(
                    new Row { A = 1, B = "foo", C = 5.0, D = true },
                    new Row { A = 2, B = "bar", C = null, D = false },
                    new Row { A = 3, B = "lorem", C = 10.0, D = null } );

            result.ResultSetFields.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateQueryReaderThatReturnsCorrectResult_WhenDataReaderIsNotEmpty_WithIncludedFields()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[]
                {
                    new object?[] { 1, "foo", 5.0, true },
                    new object?[] { 2, "bar", null, false },
                    new object?[] { 3, "lorem", 10.0, null }
                } ) );

        var sut = Factory.CreateFactory();
        var queryReader = sut.Create<Row>(
            SqlQueryReaderCreationOptions.Default.SetResultSetFieldsPersistenceMode(
                SqlQueryReaderResultSetFieldsPersistenceMode.Persist ) );

        var result = queryReader.Read( reader );

        using ( new AssertionScope() )
        {
            queryReader.Dialect.Should().BeSameAs( sut.Dialect );
            result.IsEmpty.Should().BeFalse();
            result.Rows.Should().HaveCount( 3 );
            result.Rows.Should().BeInAscendingOrder( r => r.A );
            result.Rows.Should()
                .BeEquivalentTo(
                    new Row { A = 1, B = "foo", C = 5.0, D = true },
                    new Row { A = 2, B = "bar", C = null, D = false },
                    new Row { A = 3, B = "lorem", C = 10.0, D = null } );

            result.ResultSetFields.ToArray()
                .Should()
                .BeSequentiallyEqualTo(
                    new SqlResultSetField( 0, "a" ),
                    new SqlResultSetField( 1, "b" ),
                    new SqlResultSetField( 2, "c" ),
                    new SqlResultSetField( 3, "d" ) );
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateQueryReaderThatReturnsCorrectResult_WhenDataReaderIsNotEmpty_WithIncludedFieldTypes()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[]
                {
                    new object?[] { 1, "foo", 5.0, true },
                    new object?[] { 2, "bar", null, false },
                    new object?[] { 3, "lorem", 10.0, null }
                } ) );

        var sut = Factory.CreateFactory();
        var queryReader = sut.Create<Row>(
            SqlQueryReaderCreationOptions.Default.SetResultSetFieldsPersistenceMode(
                SqlQueryReaderResultSetFieldsPersistenceMode.PersistWithTypes ) );

        var result = queryReader.Read( reader );

        using ( new AssertionScope() )
        {
            queryReader.Dialect.Should().BeSameAs( sut.Dialect );
            result.IsEmpty.Should().BeFalse();
            result.Rows.Should().HaveCount( 3 );
            result.Rows.Should().BeInAscendingOrder( r => r.A );
            result.Rows.Should()
                .BeEquivalentTo(
                    new Row { A = 1, B = "foo", C = 5.0, D = true },
                    new Row { A = 2, B = "bar", C = null, D = false },
                    new Row { A = 3, B = "lorem", C = 10.0, D = null } );

            result.ResultSetFields[0].Ordinal.Should().Be( 0 );
            result.ResultSetFields[0].Name.Should().Be( "a" );
            result.ResultSetFields[0].IsUsed.Should().BeTrue();
            result.ResultSetFields[0].TypeNames.ToArray().Should().BeSequentiallyEqualTo( "int32" );
            result.ResultSetFields[1].Ordinal.Should().Be( 1 );
            result.ResultSetFields[1].Name.Should().Be( "b" );
            result.ResultSetFields[1].IsUsed.Should().BeTrue();
            result.ResultSetFields[1].TypeNames.ToArray().Should().BeSequentiallyEqualTo( "string" );
            result.ResultSetFields[2].Ordinal.Should().Be( 2 );
            result.ResultSetFields[2].Name.Should().Be( "c" );
            result.ResultSetFields[2].IsUsed.Should().BeTrue();
            result.ResultSetFields[2].TypeNames.ToArray().Should().BeSequentiallyEqualTo( "double", "NULL" );
            result.ResultSetFields[3].Ordinal.Should().Be( 3 );
            result.ResultSetFields[3].Name.Should().Be( "d" );
            result.ResultSetFields[3].IsUsed.Should().BeTrue();
            result.ResultSetFields[3].TypeNames.ToArray().Should().BeSequentiallyEqualTo( "boolean", "NULL" );
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateQueryReaderThatReturnsCorrectResult_WhenRowTypeContainsParameterizedConstructor()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[]
                {
                    new object?[] { 1, "foo", 5.0, true },
                    new object?[] { 2, "bar", null, false },
                    new object?[] { 3, "lorem", 10.0, null }
                } ) );

        var sut = Factory.CreateFactory();
        var queryReader = sut.Create<RowRecord>();
        var result = queryReader.Read( reader );

        using ( new AssertionScope() )
        {
            queryReader.Dialect.Should().BeSameAs( sut.Dialect );
            result.IsEmpty.Should().BeFalse();
            result.Rows.Should().HaveCount( 3 );
            result.Rows.Should().BeInAscendingOrder( r => r.A );
            result.Rows.Should()
                .BeEquivalentTo(
                    new RowRecord( 1, "foo", 5.0, true ),
                    new RowRecord( 2, "bar", null, false ),
                    new RowRecord( 3, "lorem", 10.0, null ) );
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateQueryReaderThatReturnsCorrectResult_WhenRowTypeContainsDifferentTypesOfValidMembers()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[]
                {
                    new object?[] { 1, "foo", 5.0, "x" },
                    new object?[] { 2, "bar", null, "y" },
                    new object?[] { 3, "lorem", 10.0, null }
                } ) );

        var sut = Factory.CreateFactory();
        var queryReader = sut.Create<RowWithDifferentMemberTypes>();
        var result = queryReader.Read( reader );

        using ( new AssertionScope() )
        {
            queryReader.Dialect.Should().BeSameAs( sut.Dialect );
            result.IsEmpty.Should().BeFalse();
            result.Rows.Should().HaveCount( 3 );
            result.Rows.Should().BeInAscendingOrder( r => r.A );
            result.Rows.Should()
                .BeEquivalentTo(
                    new { A = 1, B = "foo", C = (double?)5.0, D = "x" },
                    new { A = 2, B = "bar", C = (double?)null, D = "y" },
                    new { A = 3, B = "lorem", C = (double?)10.0, D = (string?)null } );
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateQueryReaderThatReturnsCorrectResult_WhenReaderIsGivenExplicitOptions()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[]
                {
                    new object?[] { 1, "foo", 5.0, true },
                    new object?[] { 2, "bar", null, false },
                    new object?[] { 3, "lorem", 10.0, null }
                } ) );

        var sut = Factory.CreateFactory();
        var queryReader = sut.Create<Row>();

        var result = queryReader.Read( reader, new SqlQueryReaderOptions( InitialBufferCapacity: 50 ) );

        using ( new AssertionScope() )
        {
            queryReader.Dialect.Should().BeSameAs( sut.Dialect );
            result.IsEmpty.Should().BeFalse();
            (result.Rows?.Capacity).Should().Be( 50 );
            result.Rows.Should().HaveCount( 3 );
            result.Rows.Should().BeInAscendingOrder( r => r.A );
            result.Rows.Should()
                .BeEquivalentTo(
                    new Row { A = 1, B = "foo", C = 5.0, D = true },
                    new Row { A = 2, B = "bar", C = null, D = false },
                    new Row { A = 3, B = "lorem", C = 10.0, D = null } );
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateQueryReaderThatReturnsCorrectResult_WhenMemberIsIgnored()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[]
                {
                    new object?[] { 1, "foo", 5.0, true },
                    new object?[] { 2, "bar", null, false },
                    new object?[] { 3, "lorem", 10.0, null }
                } ) );

        var sut = Factory.CreateFactory();
        var queryReader = sut.Create<Row>( SqlQueryReaderCreationOptions.Default.With( SqlQueryMemberConfiguration.Ignore( "c" ) ) );

        var result = queryReader.Read( reader );

        using ( new AssertionScope() )
        {
            queryReader.Dialect.Should().BeSameAs( sut.Dialect );
            result.IsEmpty.Should().BeFalse();
            result.Rows.Should().HaveCount( 3 );
            result.Rows.Should().BeInAscendingOrder( r => r.A );
            result.Rows.Should()
                .BeEquivalentTo(
                    new Row { A = 1, B = "foo", C = null, D = true },
                    new Row { A = 2, B = "bar", C = null, D = false },
                    new Row { A = 3, B = "lorem", C = null, D = null } );
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateQueryReaderThatReturnsCorrectResult_WhenMemberIsFromOtherSourceField()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "e" },
                Rows: new[]
                {
                    new object?[] { 1, "foo", 5.0, true },
                    new object?[] { 2, "bar", null, false },
                    new object?[] { 3, "lorem", 10.0, null }
                } ) );

        var sut = Factory.CreateFactory();
        var queryReader = sut.Create<Row>( SqlQueryReaderCreationOptions.Default.With( SqlQueryMemberConfiguration.From( "d", "e" ) ) );

        var result = queryReader.Read( reader );

        using ( new AssertionScope() )
        {
            queryReader.Dialect.Should().BeSameAs( sut.Dialect );
            result.IsEmpty.Should().BeFalse();
            result.Rows.Should().HaveCount( 3 );
            result.Rows.Should().BeInAscendingOrder( r => r.A );
            result.Rows.Should()
                .BeEquivalentTo(
                    new Row { A = 1, B = "foo", C = 5.0, D = true },
                    new Row { A = 2, B = "bar", C = null, D = false },
                    new Row { A = 3, B = "lorem", C = 10.0, D = null } );
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateQueryReaderThatReturnsCorrectResult_WithCustomMappedMember()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[]
                {
                    new object?[] { 1, "foo", 5.0, true },
                    new object?[] { 2, "bar", null, false },
                    new object?[] { 3, "lorem", 10.0, null }
                } ) );

        var sut = Factory.CreateFactory();
        var queryReader =
            sut.Create<Row>(
                SqlQueryReaderCreationOptions.Default.With(
                    SqlQueryMemberConfiguration.From(
                        "c",
                        (ISqlDataRecordFacade<DbDataReaderMock> r) => (double)(r.Record.GetInt32( r.Record.GetOrdinal( "a" ) ) +
                            r.Record.GetString( r.Record.GetOrdinal( "b" ) ).Length) ) ) );

        var result = queryReader.Read( reader );

        using ( new AssertionScope() )
        {
            queryReader.Dialect.Should().BeSameAs( sut.Dialect );
            result.IsEmpty.Should().BeFalse();
            result.Rows.Should().HaveCount( 3 );
            result.Rows.Should().BeInAscendingOrder( r => r.A );
            result.Rows.Should()
                .BeEquivalentTo(
                    new Row { A = 1, B = "foo", C = 4.0, D = true },
                    new Row { A = 2, B = "bar", C = 5.0, D = false },
                    new Row { A = 3, B = "lorem", C = 8.0, D = null } );
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateQueryReaderThatReturnsCorrectResult_WithCustomMappedMemberUsingFacadeMethods()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[]
                {
                    new object?[] { 1, "foo", 5.0, true },
                    new object?[] { 2, "bar", null, false },
                    new object?[] { 3, "lorem", 10.0, null }
                } ) );

        var sut = Factory.CreateFactory();
        var queryReader =
            sut.Create<Row>(
                SqlQueryReaderCreationOptions.Default.With(
                    SqlQueryMemberConfiguration.From(
                        "c",
                        (ISqlDataRecordFacade<DbDataReaderMock> r) =>
                            r.IsNull( "d" )
                                ? r.Record.GetDouble( r.GetOrdinal( "c" ) )
                                : r.Get<int>( "a" ) + r.GetNullable( "c", -1.0 ) + r.GetNullable<double>( "c" ) ) ) );

        var result = queryReader.Read( reader );

        using ( new AssertionScope() )
        {
            queryReader.Dialect.Should().BeSameAs( sut.Dialect );
            result.IsEmpty.Should().BeFalse();
            result.Rows.Should().HaveCount( 3 );
            result.Rows.Should().BeInAscendingOrder( r => r.A );
            result.Rows.Should()
                .BeEquivalentTo(
                    new Row { A = 1, B = "foo", C = 11.0, D = true },
                    new Row { A = 2, B = "bar", C = 1.0, D = false },
                    new Row { A = 3, B = "lorem", C = 10.0, D = null } );
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateQueryReaderThatReturnsCorrectResult_WithAlwaysTestingForNullEnabled()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[]
                {
                    new object?[] { 1, "foo", null, "x" },
                    new object?[] { null, "bar", 5.0, null },
                    new object?[] { 3, null, 10.0, "y" }
                } ) );

        var sut = Factory.CreateFactory();
        var queryReader = sut.Create<UnsafeRow>(
            SqlQueryReaderCreationOptions.Default.EnableAlwaysTestingForNull()
                .SetRowTypeMemberPredicate(
                    m => m.MemberType == MemberTypes.Property || (m.MemberType == MemberTypes.Field && ((FieldInfo)m).IsPublic) )
                .SetRowTypeConstructorPredicate( c => c.GetParameters().Length == 0 ) );

        var result = queryReader.Read( reader );

        using ( new AssertionScope() )
        {
            queryReader.Dialect.Should().BeSameAs( sut.Dialect );
            result.IsEmpty.Should().BeFalse();
            result.Rows.Should().HaveCount( 3 );
            result.Rows.Should()
                .BeEquivalentTo(
                    new UnsafeRow( 1, "foo", 0.0, "x" ),
                    new UnsafeRow( 0, "bar", 5.0, null! ),
                    new UnsafeRow( 3, null!, 10.0, "y" ) );
        }
    }

    [Fact]
    public void Create_Generic_ShouldCreateQueryReaderThatReturnsCorrectResult_WithAlwaysTestingForNullEnabledAndValidParameterizedCtor()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[]
                {
                    new object?[] { 1, "foo", null, "x" },
                    new object?[] { null, "bar", 5.0, null },
                    new object?[] { 3, null, 10.0, "y" }
                } ) );

        var sut = Factory.CreateFactory();
        var queryReader = sut.Create<UnsafeRow>( SqlQueryReaderCreationOptions.Default.EnableAlwaysTestingForNull() );

        var result = queryReader.Read( reader );

        using ( new AssertionScope() )
        {
            queryReader.Dialect.Should().BeSameAs( sut.Dialect );
            result.IsEmpty.Should().BeFalse();
            result.Rows.Should().HaveCount( 3 );
            result.Rows.Should()
                .BeEquivalentTo(
                    new UnsafeRow( 1, "foo", 0.0, "x" ),
                    new UnsafeRow( 0, "bar", 5.0, null! ),
                    new UnsafeRow( 3, null!, 10.0, "y" ) );
        }
    }

    [Fact]
    public void CreateExpression_ShouldThrowSqlCompilerException_WhenRowTypeIsAbstract()
    {
        var sut = Factory.CreateFactory();
        var action = Lambda.Of( () => sut.CreateExpression<IEnumerable>() );
        action.Should().ThrowExactly<SqlCompilerException>().AndMatch( e => e.Dialect == sut.Dialect );
    }

    [Fact]
    public void CreateExpression_ShouldThrowSqlCompilerException_WhenRowTypeIsGenericDefinition()
    {
        var sut = Factory.CreateFactory();
        var action = Lambda.Of( () => sut.CreateExpression( typeof( IEnumerable<> ) ) );
        action.Should().ThrowExactly<SqlCompilerException>().AndMatch( e => e.Dialect == sut.Dialect );
    }

    [Fact]
    public void CreateExpression_ShouldThrowSqlCompilerException_WhenRowTypeIsNullableValue()
    {
        var sut = Factory.CreateFactory();
        var action = Lambda.Of( () => sut.CreateExpression( typeof( int? ) ) );
        action.Should().ThrowExactly<SqlCompilerException>().AndMatch( e => e.Dialect == sut.Dialect );
    }

    [Fact]
    public void CreateExpression_ShouldThrowSqlCompilerException_WhenNoValidConstructorForRowTypeIsFound()
    {
        var sut = Factory.CreateFactory();
        var action = Lambda.Of(
            () => sut.CreateExpression<RowRecord>( SqlQueryReaderCreationOptions.Default.SetRowTypeConstructorPredicate( _ => false ) ) );

        action.Should().ThrowExactly<SqlCompilerException>().AndMatch( e => e.Dialect == sut.Dialect );
    }

    [Fact]
    public void CreateExpression_ShouldThrowSqlCompilerException_WhenRowTypeHasNoMembers()
    {
        var sut = Factory.CreateFactory();
        var action = Lambda.Of( () => sut.CreateExpression<object>() );
        action.Should().ThrowExactly<SqlCompilerException>().AndMatch( e => e.Dialect == sut.Dialect );
    }

    [Fact]
    public void CreateExpression_ShouldThrowSqlCompilerException_WhenNoValidMemberForRowTypeIsFound()
    {
        var sut = Factory.CreateFactory();
        var action = Lambda.Of(
            () => sut.CreateExpression<Row>( SqlQueryReaderCreationOptions.Default.SetRowTypeMemberPredicate( _ => false ) ) );

        action.Should().ThrowExactly<SqlCompilerException>().AndMatch( e => e.Dialect == sut.Dialect );
    }

    [Fact]
    public async Task CreateAsync_TypeErased_ShouldCreateAsyncQueryReaderThatReturnsCorrectResult_WhenDataReaderIsEmpty()
    {
        var reader = new DbDataReaderMock();
        var sut = Factory.CreateFactory();
        var queryReader = sut.CreateAsync();

        var result = await queryReader.ReadAsync( reader );

        using ( new AssertionScope() )
        {
            queryReader.Dialect.Should().BeSameAs( sut.Dialect );
            result.IsEmpty.Should().BeTrue();
            result.Rows.Should().BeNull();
            result.ResultSetFields.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public async Task CreateAsync_TypeErased_ShouldCreateAsyncQueryReaderThatReturnsCorrectResult_WhenDataReaderIsNotEmpty()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[]
                {
                    new object?[] { 1, "foo", 5.0, true },
                    new object?[] { 2, "bar", null, false },
                    new object?[] { 3, "lorem", 10.0, null }
                } ) );

        var sut = Factory.CreateFactory();
        var queryReader = sut.CreateAsync();

        var result = await queryReader.ReadAsync( reader );

        using ( new AssertionScope() )
        {
            queryReader.Dialect.Should().BeSameAs( sut.Dialect );
            result.IsEmpty.Should().BeFalse();
            result.Rows.Should().NotBeNull();
            (result.Rows?.Count).Should().Be( 3 );
            (result.Rows?[0].AsSpan().ToArray()).Should().BeSequentiallyEqualTo( 1, "foo", 5.0, true );
            (result.Rows?[1].AsSpan().ToArray()).Should().BeSequentiallyEqualTo( 2, "bar", null, false );
            (result.Rows?[2].AsSpan().ToArray()).Should().BeSequentiallyEqualTo( 3, "lorem", 10.0, null );
            result.ResultSetFields.ToArray()
                .Should()
                .BeSequentiallyEqualTo(
                    new SqlResultSetField( 0, "a" ),
                    new SqlResultSetField( 1, "b" ),
                    new SqlResultSetField( 2, "c" ),
                    new SqlResultSetField( 3, "d" ) );
        }
    }

    [Fact]
    public async Task
        CreateAsync_TypeErased_ShouldCreateAsyncQueryReaderThatReturnsCorrectResult_WhenDataReaderIsEmpty_WithIncludedFieldTypes()
    {
        var reader = new DbDataReaderMock();
        var sut = Factory.CreateFactory();
        var queryReader = sut.CreateAsync(
            SqlQueryReaderCreationOptions.Default.SetResultSetFieldsPersistenceMode(
                SqlQueryReaderResultSetFieldsPersistenceMode.PersistWithTypes ) );

        var result = await queryReader.ReadAsync( reader );

        using ( new AssertionScope() )
        {
            queryReader.Dialect.Should().BeSameAs( sut.Dialect );
            result.IsEmpty.Should().BeTrue();
            result.Rows.Should().BeNull();
            result.ResultSetFields.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public async Task
        CreateAsync_TypeErased_ShouldCreateAsyncQueryReaderThatReturnsCorrectResult_WhenDataReaderIsNotEmpty_WithIncludedFieldTypes()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[]
                {
                    new object?[] { 1, "foo", 5.0, true },
                    new object?[] { 2, "bar", null, false },
                    new object?[] { 3, "lorem", 10.0, null }
                } ) );

        var sut = Factory.CreateFactory();
        var queryReader = sut.CreateAsync(
            SqlQueryReaderCreationOptions.Default.SetResultSetFieldsPersistenceMode(
                SqlQueryReaderResultSetFieldsPersistenceMode.PersistWithTypes ) );

        var result = await queryReader.ReadAsync( reader );

        using ( new AssertionScope() )
        {
            queryReader.Dialect.Should().BeSameAs( sut.Dialect );
            result.IsEmpty.Should().BeFalse();
            result.Rows.Should().HaveCount( 3 );
            (result.Rows?[0].AsSpan().ToArray()).Should().BeSequentiallyEqualTo( 1, "foo", 5.0, true );
            (result.Rows?[1].AsSpan().ToArray()).Should().BeSequentiallyEqualTo( 2, "bar", null, false );
            (result.Rows?[2].AsSpan().ToArray()).Should().BeSequentiallyEqualTo( 3, "lorem", 10.0, null );
            result.ResultSetFields[0].Ordinal.Should().Be( 0 );
            result.ResultSetFields[0].Name.Should().Be( "a" );
            result.ResultSetFields[0].IsUsed.Should().BeTrue();
            result.ResultSetFields[0].TypeNames.ToArray().Should().BeSequentiallyEqualTo( "int32" );
            result.ResultSetFields[1].Ordinal.Should().Be( 1 );
            result.ResultSetFields[1].Name.Should().Be( "b" );
            result.ResultSetFields[1].IsUsed.Should().BeTrue();
            result.ResultSetFields[1].TypeNames.ToArray().Should().BeSequentiallyEqualTo( "string" );
            result.ResultSetFields[2].Ordinal.Should().Be( 2 );
            result.ResultSetFields[2].Name.Should().Be( "c" );
            result.ResultSetFields[2].IsUsed.Should().BeTrue();
            result.ResultSetFields[2].TypeNames.ToArray().Should().BeSequentiallyEqualTo( "double", "NULL" );
            result.ResultSetFields[3].Ordinal.Should().Be( 3 );
            result.ResultSetFields[3].Name.Should().Be( "d" );
            result.ResultSetFields[3].IsUsed.Should().BeTrue();
            result.ResultSetFields[3].TypeNames.ToArray().Should().BeSequentiallyEqualTo( "boolean", "NULL" );
        }
    }

    [Fact]
    public async Task CreateAsync_Generic_ShouldCreateAsyncQueryReaderThatReturnsCorrectResult_WhenDataReaderIsEmpty()
    {
        var reader = new DbDataReaderMock();
        var sut = Factory.CreateFactory();
        var queryReader = sut.CreateAsync<Row>();

        var result = await queryReader.ReadAsync( reader );

        using ( new AssertionScope() )
        {
            queryReader.Dialect.Should().BeSameAs( sut.Dialect );
            result.IsEmpty.Should().BeTrue();
            result.Rows.Should().BeNull();
            result.ResultSetFields.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public async Task CreateAsync_Generic_ShouldCreateAsyncQueryReaderThatReturnsCorrectResult_WhenDataReaderIsEmpty_WithIncludedFields()
    {
        var reader = new DbDataReaderMock();
        var sut = Factory.CreateFactory();
        var queryReader = sut.CreateAsync<Row>(
            SqlQueryReaderCreationOptions.Default.SetResultSetFieldsPersistenceMode(
                SqlQueryReaderResultSetFieldsPersistenceMode.Persist ) );

        var result = await queryReader.ReadAsync( reader );

        using ( new AssertionScope() )
        {
            queryReader.Dialect.Should().BeSameAs( sut.Dialect );
            result.IsEmpty.Should().BeTrue();
            result.Rows.Should().BeNull();
            result.ResultSetFields.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public async Task CreateAsync_Generic_ShouldCreateAsyncQueryReaderThatReturnsCorrectResult_WhenDataReaderIsNotEmpty()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[]
                {
                    new object?[] { 1, "foo", 5.0, true },
                    new object?[] { 2, "bar", null, false },
                    new object?[] { 3, "lorem", 10.0, null }
                } ) );

        var sut = Factory.CreateFactory();
        var queryReader = sut.CreateAsync<Row>();
        var result = await queryReader.ReadAsync( reader );

        using ( new AssertionScope() )
        {
            queryReader.Dialect.Should().BeSameAs( sut.Dialect );
            result.IsEmpty.Should().BeFalse();
            result.Rows.Should().HaveCount( 3 );
            result.Rows.Should().BeInAscendingOrder( r => r.A );
            result.Rows.Should()
                .BeEquivalentTo(
                    new Row { A = 1, B = "foo", C = 5.0, D = true },
                    new Row { A = 2, B = "bar", C = null, D = false },
                    new Row { A = 3, B = "lorem", C = 10.0, D = null } );

            result.ResultSetFields.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public async Task CreateAsync_Generic_ShouldCreateAsyncQueryReaderThatReturnsCorrectResult_WhenDataReaderIsNotEmpty_WithIncludedFields()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[]
                {
                    new object?[] { 1, "foo", 5.0, true },
                    new object?[] { 2, "bar", null, false },
                    new object?[] { 3, "lorem", 10.0, null }
                } ) );

        var sut = Factory.CreateFactory();
        var queryReader = sut.CreateAsync<Row>(
            SqlQueryReaderCreationOptions.Default.SetResultSetFieldsPersistenceMode(
                SqlQueryReaderResultSetFieldsPersistenceMode.Persist ) );

        var result = await queryReader.ReadAsync( reader );

        using ( new AssertionScope() )
        {
            queryReader.Dialect.Should().BeSameAs( sut.Dialect );
            result.IsEmpty.Should().BeFalse();
            result.Rows.Should().HaveCount( 3 );
            result.Rows.Should().BeInAscendingOrder( r => r.A );
            result.Rows.Should()
                .BeEquivalentTo(
                    new Row { A = 1, B = "foo", C = 5.0, D = true },
                    new Row { A = 2, B = "bar", C = null, D = false },
                    new Row { A = 3, B = "lorem", C = 10.0, D = null } );

            result.ResultSetFields.ToArray()
                .Should()
                .BeSequentiallyEqualTo(
                    new SqlResultSetField( 0, "a" ),
                    new SqlResultSetField( 1, "b" ),
                    new SqlResultSetField( 2, "c" ),
                    new SqlResultSetField( 3, "d" ) );
        }
    }

    [Fact]
    public async Task
        CreateAsync_Generic_ShouldCreateAsyncQueryReaderThatReturnsCorrectResult_WhenDataReaderIsNotEmpty_WithIncludedFieldTypes()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[]
                {
                    new object?[] { 1, "foo", 5.0, true },
                    new object?[] { 2, "bar", null, false },
                    new object?[] { 3, "lorem", 10.0, null }
                } ) );

        var sut = Factory.CreateFactory();
        var queryReader = sut.CreateAsync<Row>(
            SqlQueryReaderCreationOptions.Default.SetResultSetFieldsPersistenceMode(
                SqlQueryReaderResultSetFieldsPersistenceMode.PersistWithTypes ) );

        var result = await queryReader.ReadAsync( reader );

        using ( new AssertionScope() )
        {
            queryReader.Dialect.Should().BeSameAs( sut.Dialect );
            result.IsEmpty.Should().BeFalse();
            result.Rows.Should().HaveCount( 3 );
            result.Rows.Should().BeInAscendingOrder( r => r.A );
            result.Rows.Should()
                .BeEquivalentTo(
                    new Row { A = 1, B = "foo", C = 5.0, D = true },
                    new Row { A = 2, B = "bar", C = null, D = false },
                    new Row { A = 3, B = "lorem", C = 10.0, D = null } );

            result.ResultSetFields[0].Ordinal.Should().Be( 0 );
            result.ResultSetFields[0].Name.Should().Be( "a" );
            result.ResultSetFields[0].IsUsed.Should().BeTrue();
            result.ResultSetFields[0].TypeNames.ToArray().Should().BeSequentiallyEqualTo( "int32" );
            result.ResultSetFields[1].Ordinal.Should().Be( 1 );
            result.ResultSetFields[1].Name.Should().Be( "b" );
            result.ResultSetFields[1].IsUsed.Should().BeTrue();
            result.ResultSetFields[1].TypeNames.ToArray().Should().BeSequentiallyEqualTo( "string" );
            result.ResultSetFields[2].Ordinal.Should().Be( 2 );
            result.ResultSetFields[2].Name.Should().Be( "c" );
            result.ResultSetFields[2].IsUsed.Should().BeTrue();
            result.ResultSetFields[2].TypeNames.ToArray().Should().BeSequentiallyEqualTo( "double", "NULL" );
            result.ResultSetFields[3].Ordinal.Should().Be( 3 );
            result.ResultSetFields[3].Name.Should().Be( "d" );
            result.ResultSetFields[3].IsUsed.Should().BeTrue();
            result.ResultSetFields[3].TypeNames.ToArray().Should().BeSequentiallyEqualTo( "boolean", "NULL" );
        }
    }

    [Fact]
    public async Task CreateAsync_Generic_ShouldCreateAsyncQueryReaderThatReturnsCorrectResult_WhenRowTypeContainsParameterizedConstructor()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[]
                {
                    new object?[] { 1, "foo", 5.0, true },
                    new object?[] { 2, "bar", null, false },
                    new object?[] { 3, "lorem", 10.0, null }
                } ) );

        var sut = Factory.CreateFactory();
        var queryReader = sut.CreateAsync<RowRecord>();
        var result = await queryReader.ReadAsync( reader );

        using ( new AssertionScope() )
        {
            queryReader.Dialect.Should().BeSameAs( sut.Dialect );
            result.IsEmpty.Should().BeFalse();
            result.Rows.Should().HaveCount( 3 );
            result.Rows.Should().BeInAscendingOrder( r => r.A );
            result.Rows.Should()
                .BeEquivalentTo(
                    new RowRecord( 1, "foo", 5.0, true ),
                    new RowRecord( 2, "bar", null, false ),
                    new RowRecord( 3, "lorem", 10.0, null ) );
        }
    }

    [Fact]
    public async Task
        CreateAsync_Generic_ShouldCreateAsyncQueryReaderThatReturnsCorrectResult_WhenRowTypeContainsDifferentTypesOfValidMembers()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[]
                {
                    new object?[] { 1, "foo", 5.0, "x" },
                    new object?[] { 2, "bar", null, "y" },
                    new object?[] { 3, "lorem", 10.0, null }
                } ) );

        var sut = Factory.CreateFactory();
        var queryReader = sut.CreateAsync<RowWithDifferentMemberTypes>();
        var result = await queryReader.ReadAsync( reader );

        using ( new AssertionScope() )
        {
            queryReader.Dialect.Should().BeSameAs( sut.Dialect );
            result.IsEmpty.Should().BeFalse();
            result.Rows.Should().HaveCount( 3 );
            result.Rows.Should().BeInAscendingOrder( r => r.A );
            result.Rows.Should()
                .BeEquivalentTo(
                    new { A = 1, B = "foo", C = (double?)5.0, D = "x" },
                    new { A = 2, B = "bar", C = (double?)null, D = "y" },
                    new { A = 3, B = "lorem", C = (double?)10.0, D = (string?)null } );
        }
    }

    [Fact]
    public async Task CreateAsync_Generic_ShouldCreateAsyncQueryReaderThatReturnsCorrectResult_WhenReaderIsGivenExplicitOptions()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[]
                {
                    new object?[] { 1, "foo", 5.0, true },
                    new object?[] { 2, "bar", null, false },
                    new object?[] { 3, "lorem", 10.0, null }
                } ) );

        var sut = Factory.CreateFactory();
        var queryReader = sut.CreateAsync<Row>();

        var result = await queryReader.ReadAsync( reader, new SqlQueryReaderOptions( InitialBufferCapacity: 50 ) );

        using ( new AssertionScope() )
        {
            queryReader.Dialect.Should().BeSameAs( sut.Dialect );
            result.IsEmpty.Should().BeFalse();
            (result.Rows?.Capacity).Should().Be( 50 );
            result.Rows.Should().HaveCount( 3 );
            result.Rows.Should().BeInAscendingOrder( r => r.A );
            result.Rows.Should()
                .BeEquivalentTo(
                    new Row { A = 1, B = "foo", C = 5.0, D = true },
                    new Row { A = 2, B = "bar", C = null, D = false },
                    new Row { A = 3, B = "lorem", C = 10.0, D = null } );
        }
    }

    [Fact]
    public async Task CreateAsync_Generic_ShouldCreateAsyncQueryReaderThatReturnsCorrectResult_WhenMemberIsIgnored()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[]
                {
                    new object?[] { 1, "foo", 5.0, true },
                    new object?[] { 2, "bar", null, false },
                    new object?[] { 3, "lorem", 10.0, null }
                } ) );

        var sut = Factory.CreateFactory();
        var queryReader = sut.CreateAsync<Row>( SqlQueryReaderCreationOptions.Default.With( SqlQueryMemberConfiguration.Ignore( "c" ) ) );

        var result = await queryReader.ReadAsync( reader );

        using ( new AssertionScope() )
        {
            queryReader.Dialect.Should().BeSameAs( sut.Dialect );
            result.IsEmpty.Should().BeFalse();
            result.Rows.Should().HaveCount( 3 );
            result.Rows.Should().BeInAscendingOrder( r => r.A );
            result.Rows.Should()
                .BeEquivalentTo(
                    new Row { A = 1, B = "foo", C = null, D = true },
                    new Row { A = 2, B = "bar", C = null, D = false },
                    new Row { A = 3, B = "lorem", C = null, D = null } );
        }
    }

    [Fact]
    public async Task CreateAsync_Generic_ShouldCreateAsyncQueryReaderThatReturnsCorrectResult_WhenMemberIsFromOtherSourceField()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "e" },
                Rows: new[]
                {
                    new object?[] { 1, "foo", 5.0, true },
                    new object?[] { 2, "bar", null, false },
                    new object?[] { 3, "lorem", 10.0, null }
                } ) );

        var sut = Factory.CreateFactory();
        var queryReader =
            sut.CreateAsync<Row>( SqlQueryReaderCreationOptions.Default.With( SqlQueryMemberConfiguration.From( "d", "e" ) ) );

        var result = await queryReader.ReadAsync( reader );

        using ( new AssertionScope() )
        {
            queryReader.Dialect.Should().BeSameAs( sut.Dialect );
            result.IsEmpty.Should().BeFalse();
            result.Rows.Should().HaveCount( 3 );
            result.Rows.Should().BeInAscendingOrder( r => r.A );
            result.Rows.Should()
                .BeEquivalentTo(
                    new Row { A = 1, B = "foo", C = 5.0, D = true },
                    new Row { A = 2, B = "bar", C = null, D = false },
                    new Row { A = 3, B = "lorem", C = 10.0, D = null } );
        }
    }

    [Fact]
    public async Task CreateAsync_Generic_ShouldCreateAsyncQueryReaderThatReturnsCorrectResult_WithCustomMappedMember()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[]
                {
                    new object?[] { 1, "foo", 5.0, true },
                    new object?[] { 2, "bar", null, false },
                    new object?[] { 3, "lorem", 10.0, null }
                } ) );

        var sut = Factory.CreateFactory();
        var queryReader =
            sut.CreateAsync<Row>(
                SqlQueryReaderCreationOptions.Default.With(
                    SqlQueryMemberConfiguration.From(
                        "c",
                        (ISqlDataRecordFacade<DbDataReaderMock> r) => (double)(r.Record.GetInt32( r.Record.GetOrdinal( "a" ) ) +
                            r.Record.GetString( r.Record.GetOrdinal( "b" ) ).Length) ) ) );

        var result = await queryReader.ReadAsync( reader );

        using ( new AssertionScope() )
        {
            queryReader.Dialect.Should().BeSameAs( sut.Dialect );
            result.IsEmpty.Should().BeFalse();
            result.Rows.Should().HaveCount( 3 );
            result.Rows.Should().BeInAscendingOrder( r => r.A );
            result.Rows.Should()
                .BeEquivalentTo(
                    new Row { A = 1, B = "foo", C = 4.0, D = true },
                    new Row { A = 2, B = "bar", C = 5.0, D = false },
                    new Row { A = 3, B = "lorem", C = 8.0, D = null } );
        }
    }

    [Fact]
    public async Task CreateAsync_Generic_ShouldCreateAsyncQueryReaderThatReturnsCorrectResult_WithCustomMappedMemberUsingFacadeMethods()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[]
                {
                    new object?[] { 1, "foo", 5.0, true },
                    new object?[] { 2, "bar", null, false },
                    new object?[] { 3, "lorem", 10.0, null }
                } ) );

        var sut = Factory.CreateFactory();
        var queryReader =
            sut.CreateAsync<Row>(
                SqlQueryReaderCreationOptions.Default.With(
                    SqlQueryMemberConfiguration.From(
                        "c",
                        (ISqlDataRecordFacade<DbDataReaderMock> r) =>
                            r.IsNull( "d" )
                                ? r.Record.GetDouble( r.GetOrdinal( "c" ) )
                                : r.Get<int>( "a" ) + r.GetNullable( "c", -1.0 ) + r.GetNullable<double>( "c" ) ) ) );

        var result = await queryReader.ReadAsync( reader );

        using ( new AssertionScope() )
        {
            queryReader.Dialect.Should().BeSameAs( sut.Dialect );
            result.IsEmpty.Should().BeFalse();
            result.Rows.Should().HaveCount( 3 );
            result.Rows.Should().BeInAscendingOrder( r => r.A );
            result.Rows.Should()
                .BeEquivalentTo(
                    new Row { A = 1, B = "foo", C = 11.0, D = true },
                    new Row { A = 2, B = "bar", C = 1.0, D = false },
                    new Row { A = 3, B = "lorem", C = 10.0, D = null } );
        }
    }

    [Fact]
    public async Task CreateAsync_Generic_ShouldCreateAsyncQueryReaderThatReturnsCorrectResult_WithAlwaysTestingForNullEnabled()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[]
                {
                    new object?[] { 1, "foo", null, "x" },
                    new object?[] { null, "bar", 5.0, null },
                    new object?[] { 3, null, 10.0, "y" }
                } ) );

        var sut = Factory.CreateFactory();
        var queryReader = sut.CreateAsync<UnsafeRow>(
            SqlQueryReaderCreationOptions.Default.EnableAlwaysTestingForNull()
                .SetRowTypeMemberPredicate(
                    m => m.MemberType == MemberTypes.Property || (m.MemberType == MemberTypes.Field && ((FieldInfo)m).IsPublic) )
                .SetRowTypeConstructorPredicate( c => c.GetParameters().Length == 0 ) );

        var result = await queryReader.ReadAsync( reader );

        using ( new AssertionScope() )
        {
            queryReader.Dialect.Should().BeSameAs( sut.Dialect );
            result.IsEmpty.Should().BeFalse();
            result.Rows.Should().HaveCount( 3 );
            result.Rows.Should()
                .BeEquivalentTo(
                    new UnsafeRow( 1, "foo", 0.0, "x" ),
                    new UnsafeRow( 0, "bar", 5.0, null! ),
                    new UnsafeRow( 3, null!, 10.0, "y" ) );
        }
    }

    [Fact]
    public async Task
        CreateAsync_Generic_ShouldCreateAsyncQueryReaderThatReturnsCorrectResult_WithAlwaysTestingForNullEnabledAndValidParameterizedCtor()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[]
                {
                    new object?[] { 1, "foo", null, "x" },
                    new object?[] { null, "bar", 5.0, null },
                    new object?[] { 3, null, 10.0, "y" }
                } ) );

        var sut = Factory.CreateFactory();
        var queryReader = sut.CreateAsync<UnsafeRow>( SqlQueryReaderCreationOptions.Default.EnableAlwaysTestingForNull() );

        var result = await queryReader.ReadAsync( reader );

        using ( new AssertionScope() )
        {
            queryReader.Dialect.Should().BeSameAs( sut.Dialect );
            result.IsEmpty.Should().BeFalse();
            result.Rows.Should().HaveCount( 3 );
            result.Rows.Should()
                .BeEquivalentTo(
                    new UnsafeRow( 1, "foo", 0.0, "x" ),
                    new UnsafeRow( 0, "bar", 5.0, null! ),
                    new UnsafeRow( 3, null!, 10.0, "y" ) );
        }
    }

    [Fact]
    public void CreateAsyncExpression_ShouldThrowSqlCompilerException_WhenDataReaderTypeDoesNotSupportAsyncOperations()
    {
        var dialect = new SqlDialect( "foo" );
        var sut = new SqlQueryReaderFactory( typeof( IDataReader ), dialect, ColumnTypeDefinitionProviderMock.Default( dialect ) );
        var action = Lambda.Of( () => sut.CreateAsyncExpression<Row>() );
        action.Should().ThrowExactly<SqlCompilerException>().AndMatch( e => e.Dialect == sut.Dialect );
    }

    [Fact]
    public void CreateAsyncExpression_ShouldThrowSqlCompilerException_WhenRowTypeIsAbstract()
    {
        var sut = Factory.CreateFactory();
        var action = Lambda.Of( () => sut.CreateAsyncExpression<IEnumerable>() );
        action.Should().ThrowExactly<SqlCompilerException>().AndMatch( e => e.Dialect == sut.Dialect );
    }

    [Fact]
    public void CreateAsyncExpression_ShouldThrowSqlCompilerException_WhenRowTypeIsGenericDefinition()
    {
        var sut = Factory.CreateFactory();
        var action = Lambda.Of( () => sut.CreateAsyncExpression( typeof( IEnumerable<> ) ) );
        action.Should().ThrowExactly<SqlCompilerException>().AndMatch( e => e.Dialect == sut.Dialect );
    }

    [Fact]
    public void CreateAsyncExpression_ShouldThrowSqlCompilerException_WhenRowTypeIsNullableValue()
    {
        var sut = Factory.CreateFactory();
        var action = Lambda.Of( () => sut.CreateAsyncExpression( typeof( int? ) ) );
        action.Should().ThrowExactly<SqlCompilerException>().AndMatch( e => e.Dialect == sut.Dialect );
    }

    [Fact]
    public void CreateAsyncExpression_ShouldThrowSqlCompilerException_WhenNoValidConstructorForRowTypeIsFound()
    {
        var sut = Factory.CreateFactory();
        var action = Lambda.Of(
            () => sut.CreateAsyncExpression<RowRecord>(
                SqlQueryReaderCreationOptions.Default.SetRowTypeConstructorPredicate( _ => false ) ) );

        action.Should().ThrowExactly<SqlCompilerException>().AndMatch( e => e.Dialect == sut.Dialect );
    }

    [Fact]
    public void CreateAsyncExpression_ShouldThrowSqlCompilerException_WhenRowTypeHasNoMembers()
    {
        var sut = Factory.CreateFactory();
        var action = Lambda.Of( () => sut.CreateAsyncExpression<object>() );
        action.Should().ThrowExactly<SqlCompilerException>().AndMatch( e => e.Dialect == sut.Dialect );
    }

    [Fact]
    public void CreateAsyncExpression_ShouldThrowSqlCompilerException_WhenNoValidMemberForRowTypeIsFound()
    {
        var sut = Factory.CreateFactory();
        var action = Lambda.Of(
            () => sut.CreateAsyncExpression<Row>( SqlQueryReaderCreationOptions.Default.SetRowTypeMemberPredicate( _ => false ) ) );

        action.Should().ThrowExactly<SqlCompilerException>().AndMatch( e => e.Dialect == sut.Dialect );
    }

    public sealed class Row
    {
        public int A { get; init; }
        public string B { get; init; } = string.Empty;
        public double? C { get; init; }
        public bool? D { get; init; }
    }

    public sealed class RowWithDifferentMemberTypes
    {
        public int A { get; }
        public string B { get; set; } = string.Empty;
        public double? C;
        public readonly string? D;
    }

    public sealed class UnsafeRow
    {
        private int _a;
        private string _d;

        public UnsafeRow()
            : this( 0, null!, 0.0, null! ) { }

        public UnsafeRow(int a, string b, double c, string d)
        {
            _a = a;
            B = b;
            C = c;
            _d = d;
        }

        public int A
        {
            get => _a;
            set => _a = value;
        }

        public string B;
        public double C;

        public string D
        {
            get => _d;
            set => _d = value;
        }
    }

    public sealed record RowRecord(int A, string B, double? C, bool? D);

    private sealed class Factory : SqlQueryReaderFactory<DbDataReaderMock>
    {
        private Factory(SqlDialect dialect)
            : base( dialect, ColumnTypeDefinitionProviderMock.Default( dialect ) ) { }

        [Pure]
        public static Factory CreateFactory()
        {
            var dialect = new SqlDialect( "foo" );
            return new Factory( dialect );
        }
    }
}
