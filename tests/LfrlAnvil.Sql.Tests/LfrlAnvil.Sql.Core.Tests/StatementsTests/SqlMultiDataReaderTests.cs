using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Statements.Compilers;
using LfrlAnvil.Sql.Tests.Helpers;
using LfrlAnvil.Sql.Tests.Helpers.Data;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlMultiDataReaderTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateCorrectReader()
    {
        var reader = new DbDataReader();
        var sut = reader.Multi();
        sut.Reader.Should().BeSameAs( reader );
    }

    [Fact]
    public void Dispose_ShouldDoNothing_WhenReaderIsClosed()
    {
        var reader = new DbDataReader { ThrowOnDispose = true };
        var sut = reader.Multi();
        var action = Lambda.Of( () => sut.Dispose() );
        action.Should().NotThrow();
    }

    [Fact]
    public void Dispose_ShouldDisposeReader()
    {
        var reader = new DbDataReader( new ResultSet( new[] { "A" }, new[] { new object[] { "foo" }, new object[] { "bar" } } ) );
        var sut = reader.Multi();
        sut.Dispose();
        reader.IsClosed.Should().BeTrue();
    }

    [Fact]
    public void Read_TypeErased_ShouldReadCorrectResultSetsAndCallDisposeOnceDone()
    {
        var command = new DbCommand
        {
            ResultSets = new[]
            {
                new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ),
                new ResultSet( new[] { "X", "Y" }, new[] { new object[] { "x1", "y1" }, new object?[] { "x2", null } } ),
                new ResultSet( new[] { "M", "N" }, new[] { new object[] { true, 5.0 }, new object?[] { false, null } } )
            }
        };

        var factory = new QueryFactory( new SqlDialect( "foo" ) );
        var reader = factory.Create();
        var sut = command.MultiQuery();

        var set1 = sut.Read( reader );
        var set2 = sut.Read( reader );
        var set3 = sut.Read( reader );

        using ( new AssertionScope() )
        {
            command.Audit.LastOrDefault().Should().Be( "DbDataReader.Close" );

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
    public void Read_Generic_ShouldReadCorrectResultSetsAndCallDisposeOnceDone()
    {
        var command = new DbCommand
        {
            ResultSets = new[]
            {
                new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ),
                new ResultSet( new[] { "X", "Y" }, new[] { new object[] { "x1", "y1" }, new object?[] { "x2", null } } ),
                new ResultSet( new[] { "M", "N" }, new[] { new object[] { true, 5.0 }, new object?[] { false, null } } )
            }
        };

        var factory = new QueryFactory( new SqlDialect( "foo" ) );
        var sut = command.MultiQuery();

        var set1 = sut.Read( factory.Create<FirstRow>() );
        var set2 = sut.Read( factory.Create<SecondRow>() );
        var set3 = sut.Read( factory.Create<ThirdRow>() );

        using ( new AssertionScope() )
        {
            command.Audit.LastOrDefault().Should().Be( "DbDataReader.Close" );
            set1.Rows.Should().BeSequentiallyEqualTo( new FirstRow( 1, "foo" ), new FirstRow( 2, "bar" ) );
            set2.Rows.Should().BeSequentiallyEqualTo( new SecondRow( "x1", "y1" ), new SecondRow( "x2", null ) );
            set3.Rows.Should().BeSequentiallyEqualTo( new ThirdRow( true, 5.0 ), new ThirdRow( false, null ) );
        }
    }

    [Fact]
    public void Read_WithCustomDelegate_ShouldReadCorrectResultSetsAndCallDisposeOnceDone()
    {
        var command = new DbCommand
        {
            ResultSets = new[]
            {
                new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ),
                new ResultSet( new[] { "X", "Y" }, new[] { new object[] { "x1", "y1" }, new object?[] { "x2", null } } ),
                new ResultSet( new[] { "M", "N" }, new[] { new object[] { true, 5.0 }, new object?[] { false, null } } )
            }
        };

        var sut = command.MultiQuery();

        var set1 = sut.Read( _ => 1 );
        var set2 = sut.Read( _ => "foo" );
        var set3 = sut.Read( _ => true );

        using ( new AssertionScope() )
        {
            command.Audit.LastOrDefault().Should().Be( "DbDataReader.Close" );
            set1.Should().Be( 1 );
            set2.Should().Be( "foo" );
            set3.Should().Be( true );
        }
    }

    [Fact]
    public void ReadAll_ShouldReadAllAvailableResultSetsAndCallDisposeOnceDone()
    {
        var command = new DbCommand
        {
            ResultSets = new[]
            {
                new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ),
                new ResultSet( new[] { "X", "Y" }, new[] { new object[] { "x1", "y1" }, new object?[] { "x2", null } } ),
                new ResultSet( new[] { "M", "N" }, new[] { new object[] { true, 5.0 }, new object?[] { false, null } } )
            }
        };

        var factory = new QueryFactory( new SqlDialect( "foo" ) );
        var sut = command.MultiQuery();

        var result = sut.ReadAll( factory.Create() );

        using ( new AssertionScope() )
        {
            command.Audit.LastOrDefault().Should().Be( "DbDataReader.Close" );
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

    private sealed class QueryFactory : SqlQueryReaderFactory<DbDataReader>
    {
        public QueryFactory(SqlDialect dialect)
            : base( dialect, ColumnTypeDefinitionProviderMock.Default( dialect ) ) { }
    }
}
