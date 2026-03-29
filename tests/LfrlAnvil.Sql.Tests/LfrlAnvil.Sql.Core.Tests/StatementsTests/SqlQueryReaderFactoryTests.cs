using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Statements.Compilers;
using LfrlAnvil.TestExtensions.Sql.Mocks;
using LfrlAnvil.TestExtensions.Sql.Mocks.System;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlQueryReaderFactoryTests : TestsBase
{
    [Fact]
    public void Create_TypeErased_ShouldCreateQueryReaderThatReturnsCorrectResult_WhenDataReaderIsEmpty()
    {
        var reader = new DbDataReaderMock();
        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.Create();

        var result = queryReader.Read( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.IsEmpty.TestTrue(),
                result.Rows.TestNull(),
                result.ResultSetFields.ToArray().TestEmpty() )
            .Go();
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

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.Create();

        var result = queryReader.Read( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.IsEmpty.TestFalse(),
                result.Rows.TestNotNull( rows => rows.TestSequence(
                [
                    (r, _) => r.AsSpan().TestSequence( [ 1, "foo", 5.0, true ] ),
                    (r, _) => r.AsSpan().TestSequence( [ 2, "bar", null, false ] ),
                    (r, _) => r.AsSpan().TestSequence( [ 3, "lorem", 10.0, null ] )
                ] ) ),
                result.ResultSetFields.TestSequence(
                [
                    new SqlResultSetField( 0, "a" ), new SqlResultSetField( 1, "b" ), new SqlResultSetField( 2, "c" ),
                    new SqlResultSetField( 3, "d" )
                ] ) )
            .Go();
    }

    [Fact]
    public void Create_TypeErased_ShouldCreateQueryReaderThatReturnsCorrectResult_WhenDataReaderIsEmpty_WithIncludedFieldTypes()
    {
        var reader = new DbDataReaderMock();
        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.Create(
            SqlQueryReaderCreationOptions.Default.SetResultSetFieldsPersistenceMode(
                SqlQueryReaderResultSetFieldsPersistenceMode.PersistWithTypes ) );

        var result = queryReader.Read( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.IsEmpty.TestTrue(),
                result.Rows.TestNull(),
                result.ResultSetFields.ToArray().TestEmpty() )
            .Go();
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

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.Create(
            SqlQueryReaderCreationOptions.Default.SetResultSetFieldsPersistenceMode(
                SqlQueryReaderResultSetFieldsPersistenceMode.PersistWithTypes ) );

        var result = queryReader.Read( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.IsEmpty.TestFalse(),
                result.Rows.TestNotNull( rows => rows.TestSequence(
                [
                    (r, _) => r.AsSpan().TestSequence( [ 1, "foo", 5.0, true ] ),
                    (r, _) => r.AsSpan().TestSequence( [ 2, "bar", null, false ] ),
                    (r, _) => r.AsSpan().TestSequence( [ 3, "lorem", 10.0, null ] )
                ] ) ),
                result.ResultSetFields[0].Ordinal.TestEquals( 0 ),
                result.ResultSetFields[0].Name.TestEquals( "a" ),
                result.ResultSetFields[0].IsUsed.TestTrue(),
                result.ResultSetFields[0].TypeNames.ToArray().TestSequence( [ "int32" ] ),
                result.ResultSetFields[1].Ordinal.TestEquals( 1 ),
                result.ResultSetFields[1].Name.TestEquals( "b" ),
                result.ResultSetFields[1].IsUsed.TestTrue(),
                result.ResultSetFields[1].TypeNames.ToArray().TestSequence( [ "string" ] ),
                result.ResultSetFields[2].Ordinal.TestEquals( 2 ),
                result.ResultSetFields[2].Name.TestEquals( "c" ),
                result.ResultSetFields[2].IsUsed.TestTrue(),
                result.ResultSetFields[2].TypeNames.ToArray().TestSequence( [ "double", "NULL" ] ),
                result.ResultSetFields[3].Ordinal.TestEquals( 3 ),
                result.ResultSetFields[3].Name.TestEquals( "d" ),
                result.ResultSetFields[3].IsUsed.TestTrue(),
                result.ResultSetFields[3].TypeNames.ToArray().TestSequence( [ "boolean", "NULL" ] ) )
            .Go();
    }

    [Fact]
    public void Create_Generic_ShouldCreateQueryReaderThatReturnsCorrectResult_WhenDataReaderIsEmpty()
    {
        var reader = new DbDataReaderMock();
        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.Create<Row>();

        var result = queryReader.Read( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.IsEmpty.TestTrue(),
                result.Rows.TestNull(),
                result.ResultSetFields.ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void Create_Generic_ShouldCreateQueryReaderThatReturnsCorrectResult_WhenDataReaderIsEmpty_WithIncludedFields()
    {
        var reader = new DbDataReaderMock();
        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.Create<Row>(
            SqlQueryReaderCreationOptions.Default.SetResultSetFieldsPersistenceMode(
                SqlQueryReaderResultSetFieldsPersistenceMode.Persist ) );

        var result = queryReader.Read( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.IsEmpty.TestTrue(),
                result.Rows.TestNull(),
                result.ResultSetFields.ToArray().TestEmpty() )
            .Go();
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

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.Create<Row>();
        var result = queryReader.Read( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.IsEmpty.TestFalse(),
                result.Rows.TestNotNull( rows => rows.TestSequence(
                [
                    (r, _) => Assertion.All(
                        r.A.TestEquals( 1 ),
                        r.B.TestEquals( "foo" ),
                        r.C.TestEquals( 5.0 ),
                        r.D.TestEquals( true ) ),
                    (r, _) => Assertion.All( r.A.TestEquals( 2 ), r.B.TestEquals( "bar" ), r.C.TestNull(), r.D.TestEquals( false ) ),
                    (r, _) => Assertion.All( r.A.TestEquals( 3 ), r.B.TestEquals( "lorem" ), r.C.TestEquals( 10.0 ), r.D.TestNull() )
                ] ) ),
                result.ResultSetFields.TestEmpty() )
            .Go();
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

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.Create<Row>(
            SqlQueryReaderCreationOptions.Default.SetResultSetFieldsPersistenceMode(
                SqlQueryReaderResultSetFieldsPersistenceMode.Persist ) );

        var result = queryReader.Read( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.IsEmpty.TestFalse(),
                result.Rows.TestNotNull( rows => rows.TestSequence(
                [
                    (r, _) => Assertion.All(
                        r.A.TestEquals( 1 ),
                        r.B.TestEquals( "foo" ),
                        r.C.TestEquals( 5.0 ),
                        r.D.TestEquals( true ) ),
                    (r, _) => Assertion.All( r.A.TestEquals( 2 ), r.B.TestEquals( "bar" ), r.C.TestNull(), r.D.TestEquals( false ) ),
                    (r, _) => Assertion.All( r.A.TestEquals( 3 ), r.B.TestEquals( "lorem" ), r.C.TestEquals( 10.0 ), r.D.TestNull() )
                ] ) ),
                result.ResultSetFields.TestSequence(
                [
                    new SqlResultSetField( 0, "a" ), new SqlResultSetField( 1, "b" ), new SqlResultSetField( 2, "c" ),
                    new SqlResultSetField( 3, "d" )
                ] ) )
            .Go();
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

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.Create<Row>(
            SqlQueryReaderCreationOptions.Default.SetResultSetFieldsPersistenceMode(
                SqlQueryReaderResultSetFieldsPersistenceMode.PersistWithTypes ) );

        var result = queryReader.Read( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.IsEmpty.TestFalse(),
                result.Rows.TestNotNull( rows => rows.TestSequence(
                [
                    (r, _) => Assertion.All(
                        r.A.TestEquals( 1 ),
                        r.B.TestEquals( "foo" ),
                        r.C.TestEquals( 5.0 ),
                        r.D.TestEquals( true ) ),
                    (r, _) => Assertion.All( r.A.TestEquals( 2 ), r.B.TestEquals( "bar" ), r.C.TestNull(), r.D.TestEquals( false ) ),
                    (r, _) => Assertion.All( r.A.TestEquals( 3 ), r.B.TestEquals( "lorem" ), r.C.TestEquals( 10.0 ), r.D.TestNull() )
                ] ) ),
                result.ResultSetFields[0].Ordinal.TestEquals( 0 ),
                result.ResultSetFields[0].Name.TestEquals( "a" ),
                result.ResultSetFields[0].IsUsed.TestTrue(),
                result.ResultSetFields[0].TypeNames.ToArray().TestSequence( [ "int32" ] ),
                result.ResultSetFields[1].Ordinal.TestEquals( 1 ),
                result.ResultSetFields[1].Name.TestEquals( "b" ),
                result.ResultSetFields[1].IsUsed.TestTrue(),
                result.ResultSetFields[1].TypeNames.ToArray().TestSequence( [ "string" ] ),
                result.ResultSetFields[2].Ordinal.TestEquals( 2 ),
                result.ResultSetFields[2].Name.TestEquals( "c" ),
                result.ResultSetFields[2].IsUsed.TestTrue(),
                result.ResultSetFields[2].TypeNames.ToArray().TestSequence( [ "double", "NULL" ] ),
                result.ResultSetFields[3].Ordinal.TestEquals( 3 ),
                result.ResultSetFields[3].Name.TestEquals( "d" ),
                result.ResultSetFields[3].IsUsed.TestTrue(),
                result.ResultSetFields[3].TypeNames.ToArray().TestSequence( [ "boolean", "NULL" ] ) )
            .Go();
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

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.Create<RowRecord>();
        var result = queryReader.Read( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.IsEmpty.TestFalse(),
                result.Rows.TestNotNull( rows => rows.TestSequence(
                [
                    (r, _) => Assertion.All(
                        r.A.TestEquals( 1 ),
                        r.B.TestEquals( "foo" ),
                        r.C.TestEquals( 5.0 ),
                        r.D.TestEquals( true ) ),
                    (r, _) => Assertion.All( r.A.TestEquals( 2 ), r.B.TestEquals( "bar" ), r.C.TestNull(), r.D.TestEquals( false ) ),
                    (r, _) => Assertion.All( r.A.TestEquals( 3 ), r.B.TestEquals( "lorem" ), r.C.TestEquals( 10.0 ), r.D.TestNull() )
                ] ) ) )
            .Go();
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

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.Create<RowWithDifferentMemberTypes>();
        var result = queryReader.Read( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.IsEmpty.TestFalse(),
                result.Rows.TestNotNull( rows => rows.TestSequence(
                [
                    (r, _) => Assertion.All(
                        r.A.TestEquals( 1 ),
                        r.B.TestEquals( "foo" ),
                        r.C.TestEquals( 5.0 ),
                        r.D.TestEquals( "x" ) ),
                    (r, _) => Assertion.All( r.A.TestEquals( 2 ), r.B.TestEquals( "bar" ), r.C.TestNull(), r.D.TestEquals( "y" ) ),
                    (r, _) => Assertion.All( r.A.TestEquals( 3 ), r.B.TestEquals( "lorem" ), r.C.TestEquals( 10.0 ), r.D.TestNull() )
                ] ) ) )
            .Go();
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

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.Create<Row>();

        var result = queryReader.Read( reader, new SqlQueryReaderOptions( InitialBufferCapacity: 50 ) );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.IsEmpty.TestFalse(),
                result.Rows.TestNotNull( rows => Assertion.All(
                    rows.Capacity.TestEquals( 50 ),
                    rows.TestSequence(
                    [
                        (r, _) => Assertion.All(
                            r.A.TestEquals( 1 ),
                            r.B.TestEquals( "foo" ),
                            r.C.TestEquals( 5.0 ),
                            r.D.TestEquals( true ) ),
                        (r, _) => Assertion.All(
                            r.A.TestEquals( 2 ),
                            r.B.TestEquals( "bar" ),
                            r.C.TestNull(),
                            r.D.TestEquals( false ) ),
                        (r, _) => Assertion.All(
                            r.A.TestEquals( 3 ),
                            r.B.TestEquals( "lorem" ),
                            r.C.TestEquals( 10.0 ),
                            r.D.TestNull() )
                    ] ) ) ) )
            .Go();
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

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.Create<Row>( SqlQueryReaderCreationOptions.Default.With( SqlQueryMemberConfiguration.Ignore( "c" ) ) );

        var result = queryReader.Read( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.IsEmpty.TestFalse(),
                result.Rows.TestNotNull( rows => rows.TestSequence(
                [
                    (r, _) => Assertion.All( r.A.TestEquals( 1 ), r.B.TestEquals( "foo" ), r.C.TestNull(), r.D.TestEquals( true ) ),
                    (r, _) => Assertion.All( r.A.TestEquals( 2 ), r.B.TestEquals( "bar" ), r.C.TestNull(), r.D.TestEquals( false ) ),
                    (r, _) => Assertion.All( r.A.TestEquals( 3 ), r.B.TestEquals( "lorem" ), r.C.TestNull(), r.D.TestNull() )
                ] ) ) )
            .Go();
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

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.Create<Row>( SqlQueryReaderCreationOptions.Default.With( SqlQueryMemberConfiguration.From( "d", "e" ) ) );

        var result = queryReader.Read( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.IsEmpty.TestFalse(),
                result.Rows.TestNotNull( rows => rows.TestSequence(
                [
                    (r, _) => Assertion.All(
                        r.A.TestEquals( 1 ),
                        r.B.TestEquals( "foo" ),
                        r.C.TestEquals( 5.0 ),
                        r.D.TestEquals( true ) ),
                    (r, _) => Assertion.All( r.A.TestEquals( 2 ), r.B.TestEquals( "bar" ), r.C.TestNull(), r.D.TestEquals( false ) ),
                    (r, _) => Assertion.All( r.A.TestEquals( 3 ), r.B.TestEquals( "lorem" ), r.C.TestEquals( 10.0 ), r.D.TestNull() )
                ] ) ) )
            .Go();
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

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader =
            sut.Create<Row>(
                SqlQueryReaderCreationOptions.Default.With(
                    SqlQueryMemberConfiguration.From(
                        "c",
                        (ISqlDataRecordFacade<DbDataReaderMock> r) => ( double )(r.Record.GetInt32( r.Record.GetOrdinal( "a" ) )
                            + r.Record.GetString( r.Record.GetOrdinal( "b" ) ).Length) ) ) );

        var result = queryReader.Read( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.IsEmpty.TestFalse(),
                result.Rows.TestNotNull( rows => rows.TestSequence(
                [
                    (r, _) => Assertion.All(
                        r.A.TestEquals( 1 ),
                        r.B.TestEquals( "foo" ),
                        r.C.TestEquals( 4.0 ),
                        r.D.TestEquals( true ) ),
                    (r, _) => Assertion.All(
                        r.A.TestEquals( 2 ),
                        r.B.TestEquals( "bar" ),
                        r.C.TestEquals( 5.0 ),
                        r.D.TestEquals( false ) ),
                    (r, _) => Assertion.All( r.A.TestEquals( 3 ), r.B.TestEquals( "lorem" ), r.C.TestEquals( 8.0 ), r.D.TestNull() )
                ] ) ) )
            .Go();
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

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
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

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.IsEmpty.TestFalse(),
                result.Rows.TestNotNull( rows => rows.TestSequence(
                [
                    (r, _) => Assertion.All(
                        r.A.TestEquals( 1 ),
                        r.B.TestEquals( "foo" ),
                        r.C.TestEquals( 11.0 ),
                        r.D.TestEquals( true ) ),
                    (r, _) => Assertion.All(
                        r.A.TestEquals( 2 ),
                        r.B.TestEquals( "bar" ),
                        r.C.TestEquals( 1.0 ),
                        r.D.TestEquals( false ) ),
                    (r, _) => Assertion.All( r.A.TestEquals( 3 ), r.B.TestEquals( "lorem" ), r.C.TestEquals( 10.0 ), r.D.TestNull() )
                ] ) ) )
            .Go();
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

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.Create<UnsafeRow>(
            SqlQueryReaderCreationOptions.Default.EnableAlwaysTestingForNull()
                .SetRowTypeMemberPredicate( m =>
                    m.MemberType == MemberTypes.Property || (m.MemberType == MemberTypes.Field && (( FieldInfo )m).IsPublic) )
                .SetRowTypeConstructorPredicate( c => c.GetParameters().Length == 0 ) );

        var result = queryReader.Read( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.IsEmpty.TestFalse(),
                result.Rows.TestNotNull( rows => rows.TestSequence(
                [
                    (r, _) => Assertion.All(
                        r.A.TestEquals( 1 ),
                        r.B.TestEquals( "foo" ),
                        r.C.TestEquals( 0.0 ),
                        r.D.TestEquals( "x" ) ),
                    (r, _) => Assertion.All( r.A.TestEquals( 0 ), r.B.TestEquals( "bar" ), r.C.TestEquals( 5.0 ), r.D.TestNull() ),
                    (r, _) => Assertion.All( r.A.TestEquals( 3 ), r.B.TestNull(), r.C.TestEquals( 10.0 ), r.D.TestEquals( "y" ) )
                ] ) ) )
            .Go();
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

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.Create<UnsafeRow>( SqlQueryReaderCreationOptions.Default.EnableAlwaysTestingForNull() );

        var result = queryReader.Read( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.IsEmpty.TestFalse(),
                result.Rows.TestNotNull( rows => rows.TestSequence(
                [
                    (r, _) => Assertion.All(
                        r.A.TestEquals( 1 ),
                        r.B.TestEquals( "foo" ),
                        r.C.TestEquals( 0.0 ),
                        r.D.TestEquals( "x" ) ),
                    (r, _) => Assertion.All( r.A.TestEquals( 0 ), r.B.TestEquals( "bar" ), r.C.TestEquals( 5.0 ), r.D.TestNull() ),
                    (r, _) => Assertion.All( r.A.TestEquals( 3 ), r.B.TestNull(), r.C.TestEquals( 10.0 ), r.D.TestEquals( "y" ) )
                ] ) ) )
            .Go();
    }

    [Fact]
    public void CreateExpression_ShouldThrowSqlCompilerException_WhenRowTypeIsAbstract()
    {
        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var action = Lambda.Of( () => sut.CreateExpression<IEnumerable>() );
        action.Test( exc => exc.TestType().Exact<SqlCompilerException>( e => e.Dialect.TestEquals( sut.Dialect ) ) ).Go();
    }

    [Fact]
    public void CreateExpression_ShouldThrowSqlCompilerException_WhenRowTypeIsGenericDefinition()
    {
        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var action = Lambda.Of( () => sut.CreateExpression( typeof( IEnumerable<> ) ) );
        action.Test( exc => exc.TestType().Exact<SqlCompilerException>( e => e.Dialect.TestEquals( sut.Dialect ) ) ).Go();
    }

    [Fact]
    public void CreateExpression_ShouldThrowSqlCompilerException_WhenRowTypeIsNullableValue()
    {
        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var action = Lambda.Of( () => sut.CreateExpression( typeof( int? ) ) );
        action.Test( exc => exc.TestType().Exact<SqlCompilerException>( e => e.Dialect.TestEquals( sut.Dialect ) ) ).Go();
    }

    [Fact]
    public void CreateExpression_ShouldThrowSqlCompilerException_WhenNoValidConstructorForRowTypeIsFound()
    {
        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var action = Lambda.Of( () =>
            sut.CreateExpression<RowRecord>( SqlQueryReaderCreationOptions.Default.SetRowTypeConstructorPredicate( _ => false ) ) );

        action.Test( exc => exc.TestType().Exact<SqlCompilerException>( e => e.Dialect.TestEquals( sut.Dialect ) ) ).Go();
    }

    [Fact]
    public void CreateExpression_ShouldThrowSqlCompilerException_WhenRowTypeHasNoMembers()
    {
        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var action = Lambda.Of( () => sut.CreateExpression<object>() );
        action.Test( exc => exc.TestType().Exact<SqlCompilerException>( e => e.Dialect.TestEquals( sut.Dialect ) ) ).Go();
    }

    [Fact]
    public void CreateExpression_ShouldThrowSqlCompilerException_WhenNoValidMemberForRowTypeIsFound()
    {
        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var action = Lambda.Of( () =>
            sut.CreateExpression<Row>( SqlQueryReaderCreationOptions.Default.SetRowTypeMemberPredicate( _ => false ) ) );

        action.Test( exc => exc.TestType().Exact<SqlCompilerException>( e => e.Dialect.TestEquals( sut.Dialect ) ) ).Go();
    }

    [Fact]
    public async Task CreateAsync_TypeErased_ShouldCreateAsyncQueryReaderThatReturnsCorrectResult_WhenDataReaderIsEmpty()
    {
        var reader = new DbDataReaderMock();
        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.CreateAsync();

        var result = await queryReader.ReadAsync( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.IsEmpty.TestTrue(),
                result.Rows.TestNull(),
                result.ResultSetFields.ToArray().TestEmpty() )
            .Go();
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

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.CreateAsync();

        var result = await queryReader.ReadAsync( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.IsEmpty.TestFalse(),
                result.Rows.TestNotNull( rows => rows.TestSequence(
                [
                    (r, _) => r.AsSpan().TestSequence( [ 1, "foo", 5.0, true ] ),
                    (r, _) => r.AsSpan().TestSequence( [ 2, "bar", null, false ] ),
                    (r, _) => r.AsSpan().TestSequence( [ 3, "lorem", 10.0, null ] )
                ] ) ),
                result.ResultSetFields.TestSequence(
                [
                    new SqlResultSetField( 0, "a" ), new SqlResultSetField( 1, "b" ), new SqlResultSetField( 2, "c" ),
                    new SqlResultSetField( 3, "d" )
                ] ) )
            .Go();
    }

    [Fact]
    public async Task
        CreateAsync_TypeErased_ShouldCreateAsyncQueryReaderThatReturnsCorrectResult_WhenDataReaderIsEmpty_WithIncludedFieldTypes()
    {
        var reader = new DbDataReaderMock();
        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.CreateAsync(
            SqlQueryReaderCreationOptions.Default.SetResultSetFieldsPersistenceMode(
                SqlQueryReaderResultSetFieldsPersistenceMode.PersistWithTypes ) );

        var result = await queryReader.ReadAsync( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.IsEmpty.TestTrue(),
                result.Rows.TestNull(),
                result.ResultSetFields.ToArray().TestEmpty() )
            .Go();
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

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.CreateAsync(
            SqlQueryReaderCreationOptions.Default.SetResultSetFieldsPersistenceMode(
                SqlQueryReaderResultSetFieldsPersistenceMode.PersistWithTypes ) );

        var result = await queryReader.ReadAsync( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.IsEmpty.TestFalse(),
                result.Rows.TestNotNull( rows => rows.TestSequence(
                [
                    (r, _) => r.AsSpan().TestSequence( [ 1, "foo", 5.0, true ] ),
                    (r, _) => r.AsSpan().TestSequence( [ 2, "bar", null, false ] ),
                    (r, _) => r.AsSpan().TestSequence( [ 3, "lorem", 10.0, null ] )
                ] ) ),
                result.ResultSetFields[0].Ordinal.TestEquals( 0 ),
                result.ResultSetFields[0].Name.TestEquals( "a" ),
                result.ResultSetFields[0].IsUsed.TestTrue(),
                result.ResultSetFields[0].TypeNames.ToArray().TestSequence( [ "int32" ] ),
                result.ResultSetFields[1].Ordinal.TestEquals( 1 ),
                result.ResultSetFields[1].Name.TestEquals( "b" ),
                result.ResultSetFields[1].IsUsed.TestTrue(),
                result.ResultSetFields[1].TypeNames.ToArray().TestSequence( [ "string" ] ),
                result.ResultSetFields[2].Ordinal.TestEquals( 2 ),
                result.ResultSetFields[2].Name.TestEquals( "c" ),
                result.ResultSetFields[2].IsUsed.TestTrue(),
                result.ResultSetFields[2].TypeNames.ToArray().TestSequence( [ "double", "NULL" ] ),
                result.ResultSetFields[3].Ordinal.TestEquals( 3 ),
                result.ResultSetFields[3].Name.TestEquals( "d" ),
                result.ResultSetFields[3].IsUsed.TestTrue(),
                result.ResultSetFields[3].TypeNames.ToArray().TestSequence( [ "boolean", "NULL" ] ) )
            .Go();
    }

    [Fact]
    public async Task CreateAsync_Generic_ShouldCreateAsyncQueryReaderThatReturnsCorrectResult_WhenDataReaderIsEmpty()
    {
        var reader = new DbDataReaderMock();
        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.CreateAsync<Row>();

        var result = await queryReader.ReadAsync( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.IsEmpty.TestTrue(),
                result.Rows.TestNull(),
                result.ResultSetFields.ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public async Task CreateAsync_Generic_ShouldCreateAsyncQueryReaderThatReturnsCorrectResult_WhenDataReaderIsEmpty_WithIncludedFields()
    {
        var reader = new DbDataReaderMock();
        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.CreateAsync<Row>(
            SqlQueryReaderCreationOptions.Default.SetResultSetFieldsPersistenceMode(
                SqlQueryReaderResultSetFieldsPersistenceMode.Persist ) );

        var result = await queryReader.ReadAsync( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.IsEmpty.TestTrue(),
                result.Rows.TestNull(),
                result.ResultSetFields.ToArray().TestEmpty() )
            .Go();
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

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.CreateAsync<Row>();
        var result = await queryReader.ReadAsync( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.IsEmpty.TestFalse(),
                result.Rows.TestNotNull( rows => rows.TestSequence(
                [
                    (r, _) => Assertion.All(
                        r.A.TestEquals( 1 ),
                        r.B.TestEquals( "foo" ),
                        r.C.TestEquals( 5.0 ),
                        r.D.TestEquals( true ) ),
                    (r, _) => Assertion.All( r.A.TestEquals( 2 ), r.B.TestEquals( "bar" ), r.C.TestNull(), r.D.TestEquals( false ) ),
                    (r, _) => Assertion.All( r.A.TestEquals( 3 ), r.B.TestEquals( "lorem" ), r.C.TestEquals( 10.0 ), r.D.TestNull() )
                ] ) ),
                result.ResultSetFields.TestEmpty() )
            .Go();
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

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.CreateAsync<Row>(
            SqlQueryReaderCreationOptions.Default.SetResultSetFieldsPersistenceMode(
                SqlQueryReaderResultSetFieldsPersistenceMode.Persist ) );

        var result = await queryReader.ReadAsync( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.IsEmpty.TestFalse(),
                result.Rows.TestNotNull( rows => rows.TestSequence(
                [
                    (r, _) => Assertion.All(
                        r.A.TestEquals( 1 ),
                        r.B.TestEquals( "foo" ),
                        r.C.TestEquals( 5.0 ),
                        r.D.TestEquals( true ) ),
                    (r, _) => Assertion.All( r.A.TestEquals( 2 ), r.B.TestEquals( "bar" ), r.C.TestNull(), r.D.TestEquals( false ) ),
                    (r, _) => Assertion.All( r.A.TestEquals( 3 ), r.B.TestEquals( "lorem" ), r.C.TestEquals( 10.0 ), r.D.TestNull() )
                ] ) ),
                result.ResultSetFields.TestSequence(
                [
                    new SqlResultSetField( 0, "a" ), new SqlResultSetField( 1, "b" ), new SqlResultSetField( 2, "c" ),
                    new SqlResultSetField( 3, "d" )
                ] ) )
            .Go();
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

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.CreateAsync<Row>(
            SqlQueryReaderCreationOptions.Default.SetResultSetFieldsPersistenceMode(
                SqlQueryReaderResultSetFieldsPersistenceMode.PersistWithTypes ) );

        var result = await queryReader.ReadAsync( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.IsEmpty.TestFalse(),
                result.Rows.TestNotNull( rows => rows.TestSequence(
                [
                    (r, _) => Assertion.All(
                        r.A.TestEquals( 1 ),
                        r.B.TestEquals( "foo" ),
                        r.C.TestEquals( 5.0 ),
                        r.D.TestEquals( true ) ),
                    (r, _) => Assertion.All( r.A.TestEquals( 2 ), r.B.TestEquals( "bar" ), r.C.TestNull(), r.D.TestEquals( false ) ),
                    (r, _) => Assertion.All( r.A.TestEquals( 3 ), r.B.TestEquals( "lorem" ), r.C.TestEquals( 10.0 ), r.D.TestNull() )
                ] ) ),
                result.ResultSetFields[0].Ordinal.TestEquals( 0 ),
                result.ResultSetFields[0].Name.TestEquals( "a" ),
                result.ResultSetFields[0].IsUsed.TestTrue(),
                result.ResultSetFields[0].TypeNames.ToArray().TestSequence( [ "int32" ] ),
                result.ResultSetFields[1].Ordinal.TestEquals( 1 ),
                result.ResultSetFields[1].Name.TestEquals( "b" ),
                result.ResultSetFields[1].IsUsed.TestTrue(),
                result.ResultSetFields[1].TypeNames.ToArray().TestSequence( [ "string" ] ),
                result.ResultSetFields[2].Ordinal.TestEquals( 2 ),
                result.ResultSetFields[2].Name.TestEquals( "c" ),
                result.ResultSetFields[2].IsUsed.TestTrue(),
                result.ResultSetFields[2].TypeNames.ToArray().TestSequence( [ "double", "NULL" ] ),
                result.ResultSetFields[3].Ordinal.TestEquals( 3 ),
                result.ResultSetFields[3].Name.TestEquals( "d" ),
                result.ResultSetFields[3].IsUsed.TestTrue(),
                result.ResultSetFields[3].TypeNames.ToArray().TestSequence( [ "boolean", "NULL" ] ) )
            .Go();
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

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.CreateAsync<RowRecord>();
        var result = await queryReader.ReadAsync( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.IsEmpty.TestFalse(),
                result.Rows.TestNotNull( rows => rows.TestSequence(
                [
                    (r, _) => Assertion.All(
                        r.A.TestEquals( 1 ),
                        r.B.TestEquals( "foo" ),
                        r.C.TestEquals( 5.0 ),
                        r.D.TestEquals( true ) ),
                    (r, _) => Assertion.All( r.A.TestEquals( 2 ), r.B.TestEquals( "bar" ), r.C.TestNull(), r.D.TestEquals( false ) ),
                    (r, _) => Assertion.All( r.A.TestEquals( 3 ), r.B.TestEquals( "lorem" ), r.C.TestEquals( 10.0 ), r.D.TestNull() )
                ] ) ) )
            .Go();
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

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.CreateAsync<RowWithDifferentMemberTypes>();
        var result = await queryReader.ReadAsync( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.IsEmpty.TestFalse(),
                result.Rows.TestNotNull( rows => rows.TestSequence(
                [
                    (r, _) => Assertion.All(
                        r.A.TestEquals( 1 ),
                        r.B.TestEquals( "foo" ),
                        r.C.TestEquals( 5.0 ),
                        r.D.TestEquals( "x" ) ),
                    (r, _) => Assertion.All( r.A.TestEquals( 2 ), r.B.TestEquals( "bar" ), r.C.TestNull(), r.D.TestEquals( "y" ) ),
                    (r, _) => Assertion.All( r.A.TestEquals( 3 ), r.B.TestEquals( "lorem" ), r.C.TestEquals( 10.0 ), r.D.TestNull() )
                ] ) ) )
            .Go();
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

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.CreateAsync<Row>();

        var result = await queryReader.ReadAsync( reader, new SqlQueryReaderOptions( InitialBufferCapacity: 50 ) );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.IsEmpty.TestFalse(),
                result.Rows.TestNotNull( rows => Assertion.All(
                    rows.Capacity.TestEquals( 50 ),
                    rows.TestSequence(
                    [
                        (r, _) => Assertion.All(
                            r.A.TestEquals( 1 ),
                            r.B.TestEquals( "foo" ),
                            r.C.TestEquals( 5.0 ),
                            r.D.TestEquals( true ) ),
                        (r, _) => Assertion.All(
                            r.A.TestEquals( 2 ),
                            r.B.TestEquals( "bar" ),
                            r.C.TestNull(),
                            r.D.TestEquals( false ) ),
                        (r, _) => Assertion.All(
                            r.A.TestEquals( 3 ),
                            r.B.TestEquals( "lorem" ),
                            r.C.TestEquals( 10.0 ),
                            r.D.TestNull() )
                    ] ) ) ) )
            .Go();
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

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.CreateAsync<Row>( SqlQueryReaderCreationOptions.Default.With( SqlQueryMemberConfiguration.Ignore( "c" ) ) );

        var result = await queryReader.ReadAsync( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.IsEmpty.TestFalse(),
                result.Rows.TestNotNull( rows => rows.TestSequence(
                [
                    (r, _) => Assertion.All( r.A.TestEquals( 1 ), r.B.TestEquals( "foo" ), r.C.TestNull(), r.D.TestEquals( true ) ),
                    (r, _) => Assertion.All( r.A.TestEquals( 2 ), r.B.TestEquals( "bar" ), r.C.TestNull(), r.D.TestEquals( false ) ),
                    (r, _) => Assertion.All( r.A.TestEquals( 3 ), r.B.TestEquals( "lorem" ), r.C.TestNull(), r.D.TestNull() )
                ] ) ) )
            .Go();
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

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader =
            sut.CreateAsync<Row>( SqlQueryReaderCreationOptions.Default.With( SqlQueryMemberConfiguration.From( "d", "e" ) ) );

        var result = await queryReader.ReadAsync( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.IsEmpty.TestFalse(),
                result.Rows.TestNotNull( rows => rows.TestSequence(
                [
                    (r, _) => Assertion.All(
                        r.A.TestEquals( 1 ),
                        r.B.TestEquals( "foo" ),
                        r.C.TestEquals( 5.0 ),
                        r.D.TestEquals( true ) ),
                    (r, _) => Assertion.All( r.A.TestEquals( 2 ), r.B.TestEquals( "bar" ), r.C.TestNull(), r.D.TestEquals( false ) ),
                    (r, _) => Assertion.All( r.A.TestEquals( 3 ), r.B.TestEquals( "lorem" ), r.C.TestEquals( 10.0 ), r.D.TestNull() )
                ] ) ) )
            .Go();
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

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader =
            sut.CreateAsync<Row>(
                SqlQueryReaderCreationOptions.Default.With(
                    SqlQueryMemberConfiguration.From(
                        "c",
                        (ISqlDataRecordFacade<DbDataReaderMock> r) => ( double )(r.Record.GetInt32( r.Record.GetOrdinal( "a" ) )
                            + r.Record.GetString( r.Record.GetOrdinal( "b" ) ).Length) ) ) );

        var result = await queryReader.ReadAsync( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.IsEmpty.TestFalse(),
                result.Rows.TestNotNull( rows => rows.TestSequence(
                [
                    (r, _) => Assertion.All(
                        r.A.TestEquals( 1 ),
                        r.B.TestEquals( "foo" ),
                        r.C.TestEquals( 4.0 ),
                        r.D.TestEquals( true ) ),
                    (r, _) => Assertion.All(
                        r.A.TestEquals( 2 ),
                        r.B.TestEquals( "bar" ),
                        r.C.TestEquals( 5.0 ),
                        r.D.TestEquals( false ) ),
                    (r, _) => Assertion.All( r.A.TestEquals( 3 ), r.B.TestEquals( "lorem" ), r.C.TestEquals( 8.0 ), r.D.TestNull() )
                ] ) ) )
            .Go();
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

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
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

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.IsEmpty.TestFalse(),
                result.Rows.TestNotNull( rows => rows.TestSequence(
                [
                    (r, _) => Assertion.All(
                        r.A.TestEquals( 1 ),
                        r.B.TestEquals( "foo" ),
                        r.C.TestEquals( 11.0 ),
                        r.D.TestEquals( true ) ),
                    (r, _) => Assertion.All(
                        r.A.TestEquals( 2 ),
                        r.B.TestEquals( "bar" ),
                        r.C.TestEquals( 1.0 ),
                        r.D.TestEquals( false ) ),
                    (r, _) => Assertion.All( r.A.TestEquals( 3 ), r.B.TestEquals( "lorem" ), r.C.TestEquals( 10.0 ), r.D.TestNull() )
                ] ) ) )
            .Go();
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

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.CreateAsync<UnsafeRow>(
            SqlQueryReaderCreationOptions.Default.EnableAlwaysTestingForNull()
                .SetRowTypeMemberPredicate( m =>
                    m.MemberType == MemberTypes.Property || (m.MemberType == MemberTypes.Field && (( FieldInfo )m).IsPublic) )
                .SetRowTypeConstructorPredicate( c => c.GetParameters().Length == 0 ) );

        var result = await queryReader.ReadAsync( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.IsEmpty.TestFalse(),
                result.Rows.TestNotNull( rows => rows.TestSequence(
                [
                    (r, _) => Assertion.All(
                        r.A.TestEquals( 1 ),
                        r.B.TestEquals( "foo" ),
                        r.C.TestEquals( 0.0 ),
                        r.D.TestEquals( "x" ) ),
                    (r, _) => Assertion.All( r.A.TestEquals( 0 ), r.B.TestEquals( "bar" ), r.C.TestEquals( 5.0 ), r.D.TestNull() ),
                    (r, _) => Assertion.All( r.A.TestEquals( 3 ), r.B.TestNull(), r.C.TestEquals( 10.0 ), r.D.TestEquals( "y" ) )
                ] ) ) )
            .Go();
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

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.CreateAsync<UnsafeRow>( SqlQueryReaderCreationOptions.Default.EnableAlwaysTestingForNull() );

        var result = await queryReader.ReadAsync( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.IsEmpty.TestFalse(),
                result.Rows.TestNotNull( rows => rows.TestSequence(
                [
                    (r, _) => Assertion.All(
                        r.A.TestEquals( 1 ),
                        r.B.TestEquals( "foo" ),
                        r.C.TestEquals( 0.0 ),
                        r.D.TestEquals( "x" ) ),
                    (r, _) => Assertion.All( r.A.TestEquals( 0 ), r.B.TestEquals( "bar" ), r.C.TestEquals( 5.0 ), r.D.TestNull() ),
                    (r, _) => Assertion.All( r.A.TestEquals( 3 ), r.B.TestNull(), r.C.TestEquals( 10.0 ), r.D.TestEquals( "y" ) )
                ] ) ) )
            .Go();
    }

    [Fact]
    public void CreateAsyncExpression_ShouldThrowSqlCompilerException_WhenDataReaderTypeDoesNotSupportAsyncOperations()
    {
        var dialect = new SqlDialect( "foo" );
        var sut = new SqlQueryReaderFactory(
            typeof( IDataReader ),
            dialect,
            new SqlColumnTypeDefinitionProviderMock( new SqlColumnTypeDefinitionProviderBuilderMock() ) );

        var action = Lambda.Of( () => sut.CreateAsyncExpression<Row>() );
        action.Test( exc => exc.TestType().Exact<SqlCompilerException>( e => e.Dialect.TestEquals( sut.Dialect ) ) ).Go();
    }

    [Fact]
    public void CreateAsyncExpression_ShouldThrowSqlCompilerException_WhenRowTypeIsAbstract()
    {
        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var action = Lambda.Of( () => sut.CreateAsyncExpression<IEnumerable>() );
        action.Test( exc => exc.TestType().Exact<SqlCompilerException>( e => e.Dialect.TestEquals( sut.Dialect ) ) ).Go();
    }

    [Fact]
    public void CreateAsyncExpression_ShouldThrowSqlCompilerException_WhenRowTypeIsGenericDefinition()
    {
        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var action = Lambda.Of( () => sut.CreateAsyncExpression( typeof( IEnumerable<> ) ) );
        action.Test( exc => exc.TestType().Exact<SqlCompilerException>( e => e.Dialect.TestEquals( sut.Dialect ) ) ).Go();
    }

    [Fact]
    public void CreateAsyncExpression_ShouldThrowSqlCompilerException_WhenRowTypeIsNullableValue()
    {
        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var action = Lambda.Of( () => sut.CreateAsyncExpression( typeof( int? ) ) );
        action.Test( exc => exc.TestType().Exact<SqlCompilerException>( e => e.Dialect.TestEquals( sut.Dialect ) ) ).Go();
    }

    [Fact]
    public void CreateAsyncExpression_ShouldThrowSqlCompilerException_WhenNoValidConstructorForRowTypeIsFound()
    {
        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var action = Lambda.Of( () =>
            sut.CreateAsyncExpression<RowRecord>( SqlQueryReaderCreationOptions.Default.SetRowTypeConstructorPredicate( _ => false ) ) );

        action.Test( exc => exc.TestType().Exact<SqlCompilerException>( e => e.Dialect.TestEquals( sut.Dialect ) ) ).Go();
    }

    [Fact]
    public void CreateAsyncExpression_ShouldThrowSqlCompilerException_WhenRowTypeHasNoMembers()
    {
        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var action = Lambda.Of( () => sut.CreateAsyncExpression<object>() );
        action.Test( exc => exc.TestType().Exact<SqlCompilerException>( e => e.Dialect.TestEquals( sut.Dialect ) ) ).Go();
    }

    [Fact]
    public void CreateAsyncExpression_ShouldThrowSqlCompilerException_WhenNoValidMemberForRowTypeIsFound()
    {
        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var action = Lambda.Of( () =>
            sut.CreateAsyncExpression<Row>( SqlQueryReaderCreationOptions.Default.SetRowTypeMemberPredicate( _ => false ) ) );

        action.Test( exc => exc.TestType().Exact<SqlCompilerException>( e => e.Dialect.TestEquals( sut.Dialect ) ) ).Go();
    }

    [Fact]
    public void CreateScalar_TypeErased_ShouldCreateScalarQueryReaderThatReturnsCorrectResult_WhenDataReaderIsEmpty()
    {
        var reader = new DbDataReaderMock();
        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var scalarReader = sut.CreateScalar();

        var result = scalarReader.Read( reader );

        Assertion.All(
                scalarReader.Dialect.TestRefEquals( sut.Dialect ),
                result.TestEquals( SqlScalarQueryResult.Empty ) )
            .Go();
    }

    [Fact]
    public void CreateScalar_TypeErased_ShouldCreateScalarQueryReaderThatReturnsCorrectResult_WhenDataReaderIsNotEmpty()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[] { new object?[] { 1, "foo", 5.0, true }, new object?[] { 2, "bar", null, false } } ) );

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var scalarReader = sut.CreateScalar();

        var result = scalarReader.Read( reader );

        Assertion.All(
                scalarReader.Dialect.TestRefEquals( sut.Dialect ),
                result.HasValue.TestTrue(),
                result.Value.TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public void CreateScalar_TypeErased_ShouldCreateScalarQueryReaderThatReturnsCorrectResult_WhenDataReaderIsNotEmptyAndResultIsNull()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[] { new object?[] { null, "foo", 5.0, true }, new object?[] { 2, "bar", null, false } } ) );

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var scalarReader = sut.CreateScalar();

        var result = scalarReader.Read( reader );

        Assertion.All(
                scalarReader.Dialect.TestRefEquals( sut.Dialect ),
                result.HasValue.TestTrue(),
                result.Value.TestNull() )
            .Go();
    }

    [Fact]
    public void CreateScalar_Generic_ShouldCreateScalarQueryReaderThatReturnsCorrectResult_WhenDataReaderIsEmpty()
    {
        var reader = new DbDataReaderMock();
        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.CreateScalar<int>();

        var result = queryReader.Read( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.TestEquals( SqlScalarQueryResult<int>.Empty ) )
            .Go();
    }

    [Fact]
    public void CreateScalar_Generic_ShouldCreateScalarQueryReaderThatReturnsCorrectResult_WhenDataReaderIsNotEmpty_ForNonNullValueType()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[] { new object?[] { 1, "foo", 5.0, true }, new object?[] { 2, "bar", null, false } } ) );

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.CreateScalar<int>();
        var result = queryReader.Read( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.HasValue.TestTrue(),
                result.Value.TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public void
        CreateScalar_Generic_ShouldCreateScalarQueryReaderThatReturnsCorrectResult_WhenDataReaderIsNotEmptyAndResultIsNull_ForNonNullValueType()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[] { new object?[] { null, "foo", 5.0, true }, new object?[] { 2, "bar", null, false } } ) );

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.CreateScalar<int>( isNullable: true );
        var result = queryReader.Read( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.HasValue.TestTrue(),
                result.Value.TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void CreateScalar_Generic_ShouldCreateScalarQueryReaderThatReturnsCorrectResult_WhenDataReaderIsNotEmpty_ForNullableValueType()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[] { new object?[] { 1, "foo", 5.0, true }, new object?[] { 2, "bar", null, false } } ) );

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.CreateScalar<int?>();
        var result = queryReader.Read( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.HasValue.TestTrue(),
                result.Value.TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public void
        CreateScalar_Generic_ShouldCreateScalarQueryReaderThatReturnsCorrectResult_WhenDataReaderIsNotEmptyAndResultIsNull_ForNullableValueType()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[] { new object?[] { null, "foo", 5.0, true }, new object?[] { 2, "bar", null, false } } ) );

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.CreateScalar<int?>();
        var result = queryReader.Read( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.HasValue.TestTrue(),
                result.Value.TestNull() )
            .Go();
    }

    [Fact]
    public void CreateScalar_Generic_ShouldCreateScalarQueryReaderThatReturnsCorrectResult_WhenDataReaderIsNotEmpty_ForRefType()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[] { new object?[] { "foo", 1, 5.0, true }, new object?[] { "bar", 2, null, false } } ) );

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.CreateScalar<string>();
        var result = queryReader.Read( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.HasValue.TestTrue(),
                result.Value.TestEquals( "foo" ) )
            .Go();
    }

    [Fact]
    public void
        CreateScalar_Generic_ShouldCreateScalarQueryReaderThatReturnsCorrectResult_WhenDataReaderIsNotEmptyAndResultIsNull_ForRefType()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[] { new object?[] { null, 1, 5.0, true }, new object?[] { "bar", 2, null, false } } ) );

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.CreateScalar<string>( isNullable: true );
        var result = queryReader.Read( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.HasValue.TestTrue(),
                result.Value.TestNull() )
            .Go();
    }

    [Fact]
    public void CreateScalarExpression_ShouldThrowSqlCompilerException_WhenResultTypeIsAbstract()
    {
        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var action = Lambda.Of( () => sut.CreateScalarExpression<IEnumerable>() );
        action.Test( exc => exc.TestType().Exact<SqlCompilerException>( e => e.Dialect.TestEquals( sut.Dialect ) ) ).Go();
    }

    [Fact]
    public void CreateScalarExpression_ShouldThrowSqlCompilerException_WhenResultTypeIsGenericDefinition()
    {
        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var action = Lambda.Of( () => sut.CreateScalarExpression( typeof( IEnumerable<> ) ) );
        action.Test( exc => exc.TestType().Exact<SqlCompilerException>( e => e.Dialect.TestEquals( sut.Dialect ) ) ).Go();
    }

    [Fact]
    public async Task CreateAsyncScalar_TypeErased_ShouldCreateAsyncScalarQueryReaderThatReturnsCorrectResult_WhenDataReaderIsEmpty()
    {
        var reader = new DbDataReaderMock();
        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var scalarReader = sut.CreateAsyncScalar();

        var result = await scalarReader.ReadAsync( reader );

        Assertion.All(
                scalarReader.Dialect.TestRefEquals( sut.Dialect ),
                result.TestEquals( SqlScalarQueryResult.Empty ) )
            .Go();
    }

    [Fact]
    public async Task CreateAsyncScalar_TypeErased_ShouldCreateAsyncScalarQueryReaderThatReturnsCorrectResult_WhenDataReaderIsNotEmpty()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[] { new object?[] { 1, "foo", 5.0, true }, new object?[] { 2, "bar", null, false } } ) );

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var scalarReader = sut.CreateAsyncScalar();

        var result = await scalarReader.ReadAsync( reader );

        Assertion.All(
                scalarReader.Dialect.TestRefEquals( sut.Dialect ),
                result.HasValue.TestTrue(),
                result.Value.TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public async Task
        CreateAsyncScalar_TypeErased_ShouldCreateAsyncScalarQueryReaderThatReturnsCorrectResult_WhenDataReaderIsNotEmptyAndResultIsNull()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[] { new object?[] { null, "foo", 5.0, true }, new object?[] { 2, "bar", null, false } } ) );

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var scalarReader = sut.CreateAsyncScalar();

        var result = await scalarReader.ReadAsync( reader );

        Assertion.All(
                scalarReader.Dialect.TestRefEquals( sut.Dialect ),
                result.HasValue.TestTrue(),
                result.Value.TestNull() )
            .Go();
    }

    [Fact]
    public async Task CreateAsyncScalar_Generic_ShouldCreateAsyncScalarQueryReaderThatReturnsCorrectResult_WhenDataReaderIsEmpty()
    {
        var reader = new DbDataReaderMock();
        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.CreateAsyncScalar<int>();

        var result = await queryReader.ReadAsync( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.TestEquals( SqlScalarQueryResult<int>.Empty ) )
            .Go();
    }

    [Fact]
    public async Task
        CreateAsyncScalar_Generic_ShouldCreateAsyncScalarQueryReaderThatReturnsCorrectResult_WhenDataReaderIsNotEmpty_ForNonNullValueType()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[] { new object?[] { 1, "foo", 5.0, true }, new object?[] { 2, "bar", null, false } } ) );

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.CreateAsyncScalar<int>();
        var result = await queryReader.ReadAsync( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.HasValue.TestTrue(),
                result.Value.TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public async Task
        CreateAsyncScalar_Generic_ShouldCreateAsyncScalarQueryReaderThatReturnsCorrectResult_WhenDataReaderIsNotEmptyAndResultIsNull_ForNonNullValueType()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[] { new object?[] { null, "foo", 5.0, true }, new object?[] { 2, "bar", null, false } } ) );

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.CreateAsyncScalar<int>( isNullable: true );
        var result = await queryReader.ReadAsync( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.HasValue.TestTrue(),
                result.Value.TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public async Task
        CreateAsyncScalar_Generic_ShouldCreateAsyncScalarQueryReaderThatReturnsCorrectResult_WhenDataReaderIsNotEmpty_ForNullableValueType()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[] { new object?[] { 1, "foo", 5.0, true }, new object?[] { 2, "bar", null, false } } ) );

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.CreateAsyncScalar<int?>();
        var result = await queryReader.ReadAsync( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.HasValue.TestTrue(),
                result.Value.TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public async Task
        CreateAsyncScalar_Generic_ShouldCreateAsyncScalarQueryReaderThatReturnsCorrectResult_WhenDataReaderIsNotEmptyAndResultIsNull_ForNullableValueType()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[] { new object?[] { null, "foo", 5.0, true }, new object?[] { 2, "bar", null, false } } ) );

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.CreateAsyncScalar<int?>();
        var result = await queryReader.ReadAsync( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.HasValue.TestTrue(),
                result.Value.TestNull() )
            .Go();
    }

    [Fact]
    public async Task
        CreateAsyncScalar_Generic_ShouldCreateAsyncScalarQueryReaderThatReturnsCorrectResult_WhenDataReaderIsNotEmpty_ForRefType()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[] { new object?[] { "foo", 1, 5.0, true }, new object?[] { "bar", 2, null, false } } ) );

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.CreateAsyncScalar<string>();
        var result = await queryReader.ReadAsync( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.HasValue.TestTrue(),
                result.Value.TestEquals( "foo" ) )
            .Go();
    }

    [Fact]
    public async Task
        CreateAsyncScalar_Generic_ShouldCreateAsyncScalarQueryReaderThatReturnsCorrectResult_WhenDataReaderIsNotEmptyAndResultIsNull_ForRefType()
    {
        var reader = new DbDataReaderMock(
            new ResultSet(
                FieldNames: new[] { "a", "b", "c", "d" },
                Rows: new[] { new object?[] { null, 1, 5.0, true }, new object?[] { "bar", 2, null, false } } ) );

        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.CreateAsyncScalar<string>( isNullable: true );
        var result = await queryReader.ReadAsync( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.HasValue.TestTrue(),
                result.Value.TestNull() )
            .Go();
    }

    [Fact]
    public void CreateAsyncScalarExpression_ShouldThrowSqlCompilerException_WhenDataReaderTypeDoesNotSupportAsyncOperations()
    {
        var dialect = new SqlDialect( "foo" );
        var sut = new SqlQueryReaderFactory(
            typeof( IDataReader ),
            dialect,
            new SqlColumnTypeDefinitionProviderMock( new SqlColumnTypeDefinitionProviderBuilderMock() ) );

        var action = Lambda.Of( () => sut.CreateAsyncScalarExpression<int>() );
        action.Test( exc => exc.TestType().Exact<SqlCompilerException>( e => e.Dialect.TestEquals( sut.Dialect ) ) ).Go();
    }

    [Fact]
    public void CreateAsyncScalarExpression_ShouldThrowSqlCompilerException_WhenResultTypeIsAbstract()
    {
        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var action = Lambda.Of( () => sut.CreateAsyncScalarExpression<IEnumerable>() );
        action.Test( exc => exc.TestType().Exact<SqlCompilerException>( e => e.Dialect.TestEquals( sut.Dialect ) ) ).Go();
    }

    [Fact]
    public void CreateAsyncScalarExpression_ShouldThrowSqlCompilerException_WhenResultTypeIsGenericDefinition()
    {
        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var action = Lambda.Of( () => sut.CreateAsyncScalarExpression( typeof( IEnumerable<> ) ) );
        action.Test( exc => exc.TestType().Exact<SqlCompilerException>( e => e.Dialect.TestEquals( sut.Dialect ) ) ).Go();
    }

    [Fact]
    public void CreateAsyncScalarExpression_ShouldThrowException_WhenTargetInvocationExceptionIsThrown()
    {
        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var action = Lambda.Of( () => sut.CreateAsyncScalarExpression<Row>() );
        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void CreateForValue_ShouldCreateQueryReaderThatReturnsCorrectResult()
    {
        var reader = new DbDataReaderMock( new ResultSet( FieldNames: [ "a" ], Rows: [ [ "foo" ], [ "bar" ], [ "qux" ] ] ) );
        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.CreateForValue<string>( "a" );

        var result = queryReader.Read( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.IsEmpty.TestFalse(),
                result.Rows.TestNotNull( rows => rows.Select( r => r.Item ).TestSequence( [ "foo", "bar", "qux" ] ) ),
                result.ResultSetFields.TestEmpty() )
            .Go();
    }

    [Fact]
    public async Task CreateAsyncForValue_ShouldCreateAsyncQueryReaderThatReturnsCorrectResult()
    {
        var reader = new DbDataReaderMock( new ResultSet( FieldNames: [ "a" ], Rows: [ [ "foo" ], [ "bar" ], [ "qux" ] ] ) );
        var sut = SqlQueryReaderFactoryMock.CreateInstance();
        var queryReader = sut.CreateAsyncForValue<string>( "a" );

        var result = await queryReader.ReadAsync( reader );

        Assertion.All(
                queryReader.Dialect.TestRefEquals( sut.Dialect ),
                result.IsEmpty.TestFalse(),
                result.Rows.TestNotNull( rows => rows.Select( r => r.Item ).TestSequence( [ "foo", "bar", "qux" ] ) ),
                result.ResultSetFields.TestEmpty() )
            .Go();
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
}
