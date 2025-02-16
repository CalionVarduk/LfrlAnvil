using System.Linq;
using System.Threading.Tasks;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Statements.Compilers;
using LfrlAnvil.TestExtensions.Sql.Mocks;
using LfrlAnvil.TestExtensions.Sql.Mocks.System;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlAsyncMultiDataReaderTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateCorrectReader()
    {
        var reader = new DbDataReaderMock();
        var sut = reader.MultiAsync();
        sut.Reader.TestRefEquals( reader ).Go();
    }

    [Fact]
    public void Dispose_ShouldDoNothing_WhenReaderIsClosed()
    {
        var reader = new DbDataReaderMock { ThrowOnDispose = true };
        var sut = reader.MultiAsync();
        var action = Lambda.Of( () => sut.Dispose() );
        action.Test( exc => exc.TestNull() ).Go();
    }

    [Fact]
    public void Dispose_ShouldDisposeReader()
    {
        var reader = new DbDataReaderMock( new ResultSet( new[] { "A" }, new[] { new object[] { "foo" }, new object[] { "bar" } } ) );
        var sut = reader.MultiAsync();
        sut.Dispose();
        reader.IsClosed.TestTrue().Go();
    }

    [Fact]
    public async Task DisposeAsync_ShouldDoNothing_WhenReaderIsClosed()
    {
        Exception? exception = null;
        var reader = new DbDataReaderMock { ThrowOnDispose = true };
        var sut = reader.MultiAsync();
        try
        {
            await sut.DisposeAsync();
        }
        catch ( Exception e )
        {
            exception = e;
        }

        exception.TestNull().Go();
    }

    [Fact]
    public async Task DisposeAsync_ShouldDisposeReader()
    {
        var reader = new DbDataReaderMock( new ResultSet( new[] { "A" }, new[] { new object[] { "foo" }, new object[] { "bar" } } ) );
        var sut = reader.MultiAsync();
        await sut.DisposeAsync();
        reader.IsClosed.TestTrue().Go();
    }

    [Fact]
    public async Task ReadAsync_TypeErased_ShouldReadCorrectResultSetsAndCallDisposeOnceDone()
    {
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ),
            new ResultSet( new[] { "X", "Y" }, new[] { new object[] { "x1", "y1" }, new object?[] { "x2", null } } ),
            new ResultSet( new[] { "M", "N" }, new[] { new object[] { true, 5.0 }, new object?[] { false, null } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var reader = factory.CreateAsync();
        var sut = await command.MultiQueryAsync();

        var set1 = await sut.ReadAsync( reader );
        var set2 = await sut.ReadAsync( reader );
        var set3 = await sut.ReadAsync( reader );

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
    public async Task ReadAsync_Generic_ShouldReadCorrectResultSetsAndCallDisposeOnceDone()
    {
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ),
            new ResultSet( new[] { "X", "Y" }, new[] { new object[] { "x1", "y1" }, new object?[] { "x2", null } } ),
            new ResultSet( new[] { "M", "N" }, new[] { new object[] { true, 5.0 }, new object?[] { false, null } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var sut = await command.MultiQueryAsync();

        var set1 = await sut.ReadAsync( factory.CreateAsync<FirstRow>() );
        var set2 = await sut.ReadAsync( factory.CreateAsync<SecondRow>() );
        var set3 = await sut.ReadAsync( factory.CreateAsync<ThirdRow>() );

        Assertion.All(
                command.Audit.LastOrDefault().TestEquals( "DbDataReader[0].Close" ),
                set1.Rows.TestNotNull( rows => rows.TestSequence( [ new FirstRow( 1, "foo" ), new FirstRow( 2, "bar" ) ] ) ),
                set2.Rows.TestNotNull( rows => rows.TestSequence( [ new SecondRow( "x1", "y1" ), new SecondRow( "x2", null ) ] ) ),
                set3.Rows.TestNotNull( rows => rows.TestSequence( [ new ThirdRow( true, 5.0 ), new ThirdRow( false, null ) ] ) ) )
            .Go();
    }

    [Fact]
    public async Task ReadAsync_TypeErased_ShouldReadCorrectScalarsAndCallDisposeOnceDone()
    {
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ),
            new ResultSet( new[] { "X", "Y" }, new[] { new object[] { "x1", "y1" }, new object?[] { "x2", null } } ),
            new ResultSet( new[] { "M", "N" }, new[] { new object[] { true, 5.0 }, new object?[] { false, null } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var reader = factory.CreateAsyncScalar();
        var sut = await command.MultiQueryAsync();

        var result1 = await sut.ReadAsync( reader );
        var result2 = await sut.ReadAsync( reader );
        var result3 = await sut.ReadAsync( reader );

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
    public async Task ReadAsync_Generic_ShouldReadCorrectScalarsAndCallDisposeOnceDone()
    {
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ),
            new ResultSet( new[] { "X", "Y" }, new[] { new object[] { "x1", "y1" }, new object?[] { "x2", null } } ),
            new ResultSet( new[] { "M", "N" }, new[] { new object[] { true, 5.0 }, new object?[] { false, null } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var sut = await command.MultiQueryAsync();

        var result1 = await sut.ReadAsync( factory.CreateAsyncScalar<int>() );
        var result2 = await sut.ReadAsync( factory.CreateAsyncScalar<string>() );
        var result3 = await sut.ReadAsync( factory.CreateAsyncScalar<bool>() );

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
    public async Task ReadAsync_WithCustomValueTaskDelegate_ShouldReadCorrectResultSetsAndCallDisposeOnceDone()
    {
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ),
            new ResultSet( new[] { "X", "Y" }, new[] { new object[] { "x1", "y1" }, new object?[] { "x2", null } } ),
            new ResultSet( new[] { "M", "N" }, new[] { new object[] { true, 5.0 }, new object?[] { false, null } } ) );

        var sut = await command.MultiQueryAsync();

        var set1 = await sut.ReadAsync( (_, _) => ValueTask.FromResult( 1 ) );
        var set2 = await sut.ReadAsync( (_, _) => ValueTask.FromResult( "foo" ) );
        var set3 = await sut.ReadAsync( (_, _) => ValueTask.FromResult( true ) );

        Assertion.All(
                command.Audit.LastOrDefault().TestEquals( "DbDataReader[0].Close" ),
                set1.TestEquals( 1 ),
                set2.TestEquals( "foo" ),
                set3.TestEquals( true ) )
            .Go();
    }

    [Fact]
    public async Task ReadAsync_WithCustomTaskDelegate_ShouldReadCorrectResultSetsAndCallDisposeOnceDone()
    {
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ),
            new ResultSet( new[] { "X", "Y" }, new[] { new object[] { "x1", "y1" }, new object?[] { "x2", null } } ),
            new ResultSet( new[] { "M", "N" }, new[] { new object[] { true, 5.0 }, new object?[] { false, null } } ) );

        var sut = await command.MultiQueryAsync();

        var set1 = await sut.ReadAsync( (_, _) => Task.FromResult( 1 ) );
        var set2 = await sut.ReadAsync( (_, _) => Task.FromResult( "foo" ) );
        var set3 = await sut.ReadAsync( (_, _) => Task.FromResult( true ) );

        Assertion.All(
                command.Audit.LastOrDefault().TestEquals( "DbDataReader[0].Close" ),
                set1.TestEquals( 1 ),
                set2.TestEquals( "foo" ),
                set3.TestEquals( true ) )
            .Go();
    }

    [Fact]
    public async Task ReadAllAsync_ShouldReadAllAvailableResultSetsAndCallDisposeOnceDone()
    {
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ),
            new ResultSet( new[] { "X", "Y" }, new[] { new object[] { "x1", "y1" }, new object?[] { "x2", null } } ),
            new ResultSet( new[] { "M", "N" }, new[] { new object[] { true, 5.0 }, new object?[] { false, null } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var sut = await command.MultiQueryAsync();

        var result = await sut.ReadAllAsync( factory.CreateAsync() );

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
