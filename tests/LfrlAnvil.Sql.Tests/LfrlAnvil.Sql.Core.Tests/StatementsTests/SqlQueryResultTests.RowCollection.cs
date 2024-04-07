using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public partial class SqlQueryResultTests
{
    public class RowCollection : TestsBase
    {
        [Theory]
        [InlineData( "foo", true )]
        [InlineData( "bar", true )]
        [InlineData( "qux", true )]
        [InlineData( "baz", false )]
        [InlineData( "x", false )]
        public void ContainsField_ShouldReturnTrue_WhenFieldNameExists(string name, bool expected)
        {
            var resultSetFields = new[]
            {
                new SqlResultSetField( 0, "foo" ), new SqlResultSetField( 1, "bar" ), new SqlResultSetField( 2, "qux" )
            };

            var cells = new List<object?>
            {
                "foo",
                3,
                true
            };

            var sut = new SqlQueryResult( resultSetFields, cells ).Rows!;

            var result = sut.ContainsField( name );

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( "foo", 0 )]
        [InlineData( "bar", 1 )]
        [InlineData( "qux", 2 )]
        public void GetOrdinal_ShouldReturnOrdinal_WhenFieldNameExists(string name, int expected)
        {
            var resultSetFields = new[]
            {
                new SqlResultSetField( 0, "foo" ), new SqlResultSetField( 1, "bar" ), new SqlResultSetField( 2, "qux" )
            };

            var cells = new List<object?>
            {
                "foo",
                3,
                true
            };

            var sut = new SqlQueryResult( resultSetFields, cells ).Rows!;

            var result = sut.GetOrdinal( name );

            result.Should().Be( expected );
        }

        [Fact]
        public void GetOrdinal_ShouldThrowKeyNotFoundException_WhenFieldNameDoesNotExist()
        {
            var resultSetFields = new[]
            {
                new SqlResultSetField( 0, "foo" ), new SqlResultSetField( 1, "bar" ), new SqlResultSetField( 2, "qux" )
            };

            var cells = new List<object?>
            {
                "foo",
                3,
                true
            };

            var sut = new SqlQueryResult( resultSetFields, cells ).Rows!;

            var action = Lambda.Of( () => sut.GetOrdinal( "x" ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Theory]
        [InlineData( "foo", true, 0 )]
        [InlineData( "bar", true, 1 )]
        [InlineData( "qux", true, 2 )]
        [InlineData( "baz", false, 0 )]
        [InlineData( "x", false, 0 )]
        public void TryGetOrdinal_ShouldReturnTrueWithOrdinal_WhenFieldNameExists(string name, bool expected, int expectedOrdinal)
        {
            var resultSetFields = new[]
            {
                new SqlResultSetField( 0, "foo" ), new SqlResultSetField( 1, "bar" ), new SqlResultSetField( 2, "qux" )
            };

            var cells = new List<object?>
            {
                "foo",
                3,
                true
            };

            var sut = new SqlQueryResult( resultSetFields, cells ).Rows!;

            var result = sut.TryGetOrdinal( name, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().Be( expected );
                outResult.Should().Be( expectedOrdinal );
            }
        }

        [Theory]
        [InlineData( -1 )]
        [InlineData( 3 )]
        public void Indexer_ShouldThrowArgumentOutOfRangeException_WhenIndexIsOutOfRange(int index)
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

            var sut = new SqlQueryResult( resultSetFields, cells ).Rows!;

            var action = Lambda.Of( () => sut[index] );

            action.Should().ThrowExactly<ArgumentOutOfRangeException>();
        }

        [Theory]
        [InlineData( 0 )]
        [InlineData( 1 )]
        [InlineData( 2 )]
        public void Indexer_ShouldReturnCorrectRow(int index)
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

            var sut = new SqlQueryResult( resultSetFields, cells ).Rows!;

            var result = sut[index];

            using ( new AssertionScope() )
            {
                result.Source.Should().BeSameAs( sut );
                result.Index.Should().Be( index );
            }
        }

        [Fact]
        public void GetEnumerator_ShouldReturnCorrectRows()
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

            var sut = new SqlQueryResult( resultSetFields, cells ).Rows!;

            sut.Should().BeSequentiallyEqualTo( sut[0], sut[1], sut[2] );
        }
    }
}
