using System.Linq;
using System.Threading.Tasks;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Statements.Compilers;
using LfrlAnvil.TestExtensions.FluentAssertions;
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
        sut.Reader.Should().BeSameAs( reader );
    }

    [Fact]
    public void Dispose_ShouldDoNothing_WhenReaderIsClosed()
    {
        var reader = new DbDataReaderMock { ThrowOnDispose = true };
        var sut = reader.MultiAsync();
        var action = Lambda.Of( () => sut.Dispose() );
        action.Should().NotThrow();
    }

    [Fact]
    public void Dispose_ShouldDisposeReader()
    {
        var reader = new DbDataReaderMock( new ResultSet( new[] { "A" }, new[] { new object[] { "foo" }, new object[] { "bar" } } ) );
        var sut = reader.MultiAsync();
        sut.Dispose();
        reader.IsClosed.Should().BeTrue();
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

        exception.Should().BeNull();
    }

    [Fact]
    public async Task DisposeAsync_ShouldDisposeReader()
    {
        var reader = new DbDataReaderMock( new ResultSet( new[] { "A" }, new[] { new object[] { "foo" }, new object[] { "bar" } } ) );
        var sut = reader.MultiAsync();
        await sut.DisposeAsync();
        reader.IsClosed.Should().BeTrue();
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

        using ( new AssertionScope() )
        {
            command.Audit.LastOrDefault().Should().Be( "DbDataReader[0].Close" );

            set1.Rows.Should().NotBeNull();
            (set1.Rows?.Count).Should().Be( 2 );
            (set1.Rows?[0].AsSpan().ToArray()).Should().BeSequentiallyEqualTo( 1, "foo" );
            (set1.Rows?[1].AsSpan().ToArray()).Should().BeSequentiallyEqualTo( 2, "bar" );

            set2.Rows.Should().NotBeNull();
            (set2.Rows?.Count).Should().Be( 2 );
            (set2.Rows?[0].AsSpan().ToArray()).Should().BeSequentiallyEqualTo( "x1", "y1" );
            (set2.Rows?[1].AsSpan().ToArray()).Should().BeSequentiallyEqualTo( "x2", null );

            set3.Rows.Should().NotBeNull();
            (set3.Rows?.Count).Should().Be( 2 );
            (set3.Rows?[0].AsSpan().ToArray()).Should().BeSequentiallyEqualTo( true, 5.0 );
            (set3.Rows?[1].AsSpan().ToArray()).Should().BeSequentiallyEqualTo( false, null );
        }
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

        using ( new AssertionScope() )
        {
            command.Audit.LastOrDefault().Should().Be( "DbDataReader[0].Close" );
            set1.Rows.Should().BeSequentiallyEqualTo( new FirstRow( 1, "foo" ), new FirstRow( 2, "bar" ) );
            set2.Rows.Should().BeSequentiallyEqualTo( new SecondRow( "x1", "y1" ), new SecondRow( "x2", null ) );
            set3.Rows.Should().BeSequentiallyEqualTo( new ThirdRow( true, 5.0 ), new ThirdRow( false, null ) );
        }
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

        using ( new AssertionScope() )
        {
            command.Audit.LastOrDefault().Should().Be( "DbDataReader[0].Close" );
            set1.Should().Be( 1 );
            set2.Should().Be( "foo" );
            set3.Should().Be( true );
        }
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

        using ( new AssertionScope() )
        {
            command.Audit.LastOrDefault().Should().Be( "DbDataReader[0].Close" );
            set1.Should().Be( 1 );
            set2.Should().Be( "foo" );
            set3.Should().Be( true );
        }
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

        using ( new AssertionScope() )
        {
            command.Audit.LastOrDefault().Should().Be( "DbDataReader[0].Close" );
            result.Should().HaveCount( 3 );
            var set1 = result.ElementAtOrDefault( 0 );
            var set2 = result.ElementAtOrDefault( 1 );
            var set3 = result.ElementAtOrDefault( 2 );

            set1.Rows.Should().NotBeNull();
            (set1.Rows?.Count).Should().Be( 2 );
            (set1.Rows?[0].AsSpan().ToArray()).Should().BeSequentiallyEqualTo( 1, "foo" );
            (set1.Rows?[1].AsSpan().ToArray()).Should().BeSequentiallyEqualTo( 2, "bar" );

            set2.Rows.Should().NotBeNull();
            (set2.Rows?.Count).Should().Be( 2 );
            (set2.Rows?[0].AsSpan().ToArray()).Should().BeSequentiallyEqualTo( "x1", "y1" );
            (set2.Rows?[1].AsSpan().ToArray()).Should().BeSequentiallyEqualTo( "x2", null );

            set3.Rows.Should().NotBeNull();
            (set3.Rows?.Count).Should().Be( 2 );
            (set3.Rows?[0].AsSpan().ToArray()).Should().BeSequentiallyEqualTo( true, 5.0 );
            (set3.Rows?[1].AsSpan().ToArray()).Should().BeSequentiallyEqualTo( false, null );
        }
    }

    public sealed record FirstRow(int A, string B);

    public sealed record SecondRow(string X, string? Y);

    public sealed record ThirdRow(bool M, double? N);
}
