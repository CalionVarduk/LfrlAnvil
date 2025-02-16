using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Statements.Compilers;
using LfrlAnvil.TestExtensions.Sql.Mocks;
using LfrlAnvil.TestExtensions.Sql.Mocks.System;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlMultiDataReaderTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateCorrectReader()
    {
        var reader = new DbDataReaderMock();
        var sut = reader.Multi();
        sut.Reader.TestRefEquals( reader ).Go();
    }

    [Fact]
    public void Dispose_ShouldDoNothing_WhenReaderIsClosed()
    {
        var reader = new DbDataReaderMock { ThrowOnDispose = true };
        var sut = reader.Multi();
        var action = Lambda.Of( () => sut.Dispose() );
        action.Test( exc => exc.TestNull() ).Go();
    }

    [Fact]
    public void Dispose_ShouldDisposeReader()
    {
        var reader = new DbDataReaderMock( new ResultSet( new[] { "A" }, new[] { new object[] { "foo" }, new object[] { "bar" } } ) );
        var sut = reader.Multi();
        sut.Dispose();
        reader.IsClosed.TestTrue().Go();
    }

    [Fact]
    public void Read_TypeErased_ShouldReadCorrectResultSetsAndCallDisposeOnceDone()
    {
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ),
            new ResultSet( new[] { "X", "Y" }, new[] { new object[] { "x1", "y1" }, new object?[] { "x2", null } } ),
            new ResultSet( new[] { "M", "N" }, new[] { new object[] { true, 5.0 }, new object?[] { false, null } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var reader = factory.Create();
        var sut = command.MultiQuery();

        var set1 = sut.Read( reader );
        var set2 = sut.Read( reader );
        var set3 = sut.Read( reader );

        Assertion.All(
                command.Audit.LastOrDefault().TestEquals( "DbDataReader[0].Close" ),
                set1.Rows.TestNotNull(
                    rows => rows.TestSequence(
                    [
                        (r, _) => r.AsSpan().TestSequence( [ 1, "foo" ] ),
                        (r, _) => r.AsSpan().TestSequence( [ 2, "bar" ] )
                    ] ) ),
                set2.Rows.TestNotNull(
                    rows => rows.TestSequence(
                    [
                        (r, _) => r.AsSpan().TestSequence( [ "x1", "y1" ] ),
                        (r, _) => r.AsSpan().TestSequence( [ "x2", null ] )
                    ] ) ),
                set3.Rows.TestNotNull(
                    rows => rows.TestSequence(
                    [
                        (r, _) => r.AsSpan().TestSequence( [ true, 5.0 ] ),
                        (r, _) => r.AsSpan().TestSequence( [ false, null ] )
                    ] ) ) )
            .Go();
    }

    [Fact]
    public void Read_Generic_ShouldReadCorrectResultSetsAndCallDisposeOnceDone()
    {
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ),
            new ResultSet( new[] { "X", "Y" }, new[] { new object[] { "x1", "y1" }, new object?[] { "x2", null } } ),
            new ResultSet( new[] { "M", "N" }, new[] { new object[] { true, 5.0 }, new object?[] { false, null } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var sut = command.MultiQuery();

        var set1 = sut.Read( factory.Create<FirstRow>() );
        var set2 = sut.Read( factory.Create<SecondRow>() );
        var set3 = sut.Read( factory.Create<ThirdRow>() );

        Assertion.All(
                command.Audit.LastOrDefault().TestEquals( "DbDataReader[0].Close" ),
                set1.Rows.TestNotNull( rows => rows.TestSequence( [ new FirstRow( 1, "foo" ), new FirstRow( 2, "bar" ) ] ) ),
                set2.Rows.TestNotNull( rows => rows.TestSequence( [ new SecondRow( "x1", "y1" ), new SecondRow( "x2", null ) ] ) ),
                set3.Rows.TestNotNull( rows => rows.TestSequence( [ new ThirdRow( true, 5.0 ), new ThirdRow( false, null ) ] ) ) )
            .Go();
    }

    [Fact]
    public void Read_TypeErased_ShouldReadCorrectScalarsAndCallDisposeOnceDone()
    {
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ),
            new ResultSet( new[] { "X", "Y" }, new[] { new object[] { "x1", "y1" }, new object?[] { "x2", null } } ),
            new ResultSet( new[] { "M", "N" }, new[] { new object[] { true, 5.0 }, new object?[] { false, null } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var reader = factory.CreateScalar();
        var sut = command.MultiQuery();

        var result1 = sut.Read( reader );
        var result2 = sut.Read( reader );
        var result3 = sut.Read( reader );

        Assertion.All(
                command.Audit.LastOrDefault().TestEquals( "DbDataReader[0].Close" ),
                result1.HasValue.TestTrue(),
                result1.Value.TestEquals( 1 ),
                result2.HasValue.TestTrue(),
                result2.Value.TestEquals( "x1" ),
                result3.HasValue.TestTrue(),
                result3.Value.TestEquals( true ) )
            .Go();
    }

    [Fact]
    public void Read_Generic_ShouldReadCorrectScalarsAndCallDisposeOnceDone()
    {
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ),
            new ResultSet( new[] { "X", "Y" }, new[] { new object[] { "x1", "y1" }, new object?[] { "x2", null } } ),
            new ResultSet( new[] { "M", "N" }, new[] { new object[] { true, 5.0 }, new object?[] { false, null } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var sut = command.MultiQuery();

        var result1 = sut.Read( factory.CreateScalar<int>() );
        var result2 = sut.Read( factory.CreateScalar<string>() );
        var result3 = sut.Read( factory.CreateScalar<bool>() );

        Assertion.All(
                command.Audit.LastOrDefault().TestEquals( "DbDataReader[0].Close" ),
                result1.HasValue.TestTrue(),
                result1.Value.TestEquals( 1 ),
                result2.HasValue.TestTrue(),
                result2.Value.TestEquals( "x1" ),
                result3.HasValue.TestTrue(),
                result3.Value.TestEquals( true ) )
            .Go();
    }

    [Fact]
    public void Read_WithCustomDelegate_ShouldReadCorrectResultSetsAndCallDisposeOnceDone()
    {
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ),
            new ResultSet( new[] { "X", "Y" }, new[] { new object[] { "x1", "y1" }, new object?[] { "x2", null } } ),
            new ResultSet( new[] { "M", "N" }, new[] { new object[] { true, 5.0 }, new object?[] { false, null } } ) );

        var sut = command.MultiQuery();

        var set1 = sut.Read( _ => 1 );
        var set2 = sut.Read( _ => "foo" );
        var set3 = sut.Read( _ => true );

        Assertion.All(
                command.Audit.LastOrDefault().TestEquals( "DbDataReader[0].Close" ),
                set1.TestEquals( 1 ),
                set2.TestEquals( "foo" ),
                set3.TestEquals( true ) )
            .Go();
    }

    [Fact]
    public void ReadAll_ShouldReadAllAvailableResultSetsAndCallDisposeOnceDone()
    {
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ),
            new ResultSet( new[] { "X", "Y" }, new[] { new object[] { "x1", "y1" }, new object?[] { "x2", null } } ),
            new ResultSet( new[] { "M", "N" }, new[] { new object[] { true, 5.0 }, new object?[] { false, null } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var sut = command.MultiQuery();

        var result = sut.ReadAll( factory.Create() );

        Assertion.All(
                command.Audit.LastOrDefault().TestEquals( "DbDataReader[0].Close" ),
                result.TestSequence(
                [
                    (set1, _) => set1.Rows.TestNotNull(
                        rows => rows.TestSequence(
                        [
                            (r, _) => r.AsSpan().TestSequence( [ 1, "foo" ] ),
                            (r, _) => r.AsSpan().TestSequence( [ 2, "bar" ] )
                        ] ) ),
                    (set2, _) => set2.Rows.TestNotNull(
                        rows => rows.TestSequence(
                        [
                            (r, _) => r.AsSpan().TestSequence( [ "x1", "y1" ] ),
                            (r, _) => r.AsSpan().TestSequence( [ "x2", null ] )
                        ] ) ),
                    (set3, _) => set3.Rows.TestNotNull(
                        rows => rows.TestSequence(
                        [
                            (r, _) => r.AsSpan().TestSequence( [ true, 5.0 ] ),
                            (r, _) => r.AsSpan().TestSequence( [ false, null ] )
                        ] ) )
                ] ) )
            .Go();
    }

    public sealed record FirstRow(int A, string B);

    public sealed record SecondRow(string X, string? Y);

    public sealed record ThirdRow(bool M, double? N);
}
