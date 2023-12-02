using System.Collections.Generic;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public partial class SqlQueryReaderResultTests
{
    public class Row : TestsBase
    {
        [Theory]
        [InlineData( 0, 0, "foo" )]
        [InlineData( 0, 1, 3 )]
        [InlineData( 0, 2, true )]
        [InlineData( 0, 3, 1.0 )]
        [InlineData( 1, 0, "bar" )]
        [InlineData( 1, 1, 3 )]
        [InlineData( 1, 2, false )]
        [InlineData( 1, 3, 2.0 )]
        [InlineData( 2, 0, "lorem" )]
        [InlineData( 2, 1, 5 )]
        [InlineData( 2, 2, false )]
        [InlineData( 2, 3, 5.0 )]
        public void GetValue_ByOrdinal_ShouldReturnCorrectValue(int row, int ordinal, object expected)
        {
            var resultSetFields = new[]
            {
                new SqlResultSetField( 0, "a" ),
                new SqlResultSetField( 1, "b" ),
                new SqlResultSetField( 2, "c" ),
                new SqlResultSetField( 3, "d" )
            };

            var cells = new List<object?> { "foo", 3, true, 1.0, "bar", 3, false, 2.0, "lorem", 5, false, 5.0 };
            var sut = new SqlQueryReaderResult( resultSetFields, cells ).Rows![row];

            var result = sut.GetValue( ordinal );

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( 0, "a", "foo" )]
        [InlineData( 0, "b", 3 )]
        [InlineData( 0, "c", true )]
        [InlineData( 0, "d", 1.0 )]
        [InlineData( 1, "a", "bar" )]
        [InlineData( 1, "b", 3 )]
        [InlineData( 1, "c", false )]
        [InlineData( 1, "d", 2.0 )]
        [InlineData( 2, "a", "lorem" )]
        [InlineData( 2, "b", 5 )]
        [InlineData( 2, "c", false )]
        [InlineData( 2, "d", 5.0 )]
        public void GetValue_ByFieldName_ShouldReturnCorrectValue(int row, string field, object expected)
        {
            var resultSetFields = new[]
            {
                new SqlResultSetField( 0, "a" ),
                new SqlResultSetField( 1, "b" ),
                new SqlResultSetField( 2, "c" ),
                new SqlResultSetField( 3, "d" )
            };

            var cells = new List<object?> { "foo", 3, true, 1.0, "bar", 3, false, 2.0, "lorem", 5, false, 5.0 };
            var sut = new SqlQueryReaderResult( resultSetFields, cells ).Rows![row];

            var result = sut.GetValue( field );

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( 0, "foo", 3, true, 1.0 )]
        [InlineData( 1, "bar", 3, false, 2.0 )]
        [InlineData( 2, "lorem", 5, false, 5.0 )]
        public void AsSpan_ShouldReturnSpanOfCorrectValues_EvenWhenFieldOrdinalsAreNotOrdered(
            int row,
            object v1,
            object v2,
            object v3,
            object v4)
        {
            var resultSetFields = new[]
            {
                new SqlResultSetField( 2, "c" ),
                new SqlResultSetField( 0, "a" ),
                new SqlResultSetField( 3, "d" ),
                new SqlResultSetField( 1, "b" )
            };

            var cells = new List<object?> { "foo", 3, true, 1.0, "bar", 3, false, 2.0, "lorem", 5, false, 5.0 };
            var sut = new SqlQueryReaderResult( resultSetFields, cells ).Rows![row];

            var result = sut.AsSpan();

            result.ToArray().Should().BeSequentiallyEqualTo( v1, v2, v3, v4 );
        }

        [Theory]
        [InlineData( 0, true, "foo", 1.0, 3 )]
        [InlineData( 1, false, "bar", 2.0, 3 )]
        [InlineData( 2, false, "lorem", 5.0, 5 )]
        public void ToArray_ShouldReturnArrayOfValuesOrderedByFieldOccurrence(int row, object v1, object v2, object v3, object v4)
        {
            var resultSetFields = new[]
            {
                new SqlResultSetField( 2, "c" ),
                new SqlResultSetField( 0, "a" ),
                new SqlResultSetField( 3, "d" ),
                new SqlResultSetField( 1, "b" )
            };

            var cells = new List<object?> { "foo", 3, true, 1.0, "bar", 3, false, 2.0, "lorem", 5, false, 5.0 };
            var sut = new SqlQueryReaderResult( resultSetFields, cells ).Rows![row];

            var result = sut.ToArray();

            result.Should().BeSequentiallyEqualTo( v1, v2, v3, v4 );
        }

        [Theory]
        [InlineData( 0, "foo", 3, true, 1.0 )]
        [InlineData( 1, "bar", 3, false, 2.0 )]
        [InlineData( 2, "lorem", 5, false, 5.0 )]
        public void AsSpan_ShouldReturnDictionaryWithValuesKeyedByFieldNames(int row, object v1, object v2, object v3, object v4)
        {
            var resultSetFields = new[]
            {
                new SqlResultSetField( 2, "c" ),
                new SqlResultSetField( 0, "a" ),
                new SqlResultSetField( 3, "d" ),
                new SqlResultSetField( 1, "b" )
            };

            var cells = new List<object?> { "foo", 3, true, 1.0, "bar", 3, false, 2.0, "lorem", 5, false, 5.0 };
            var sut = new SqlQueryReaderResult( resultSetFields, cells ).Rows![row];

            var result = sut.ToDictionary();

            using ( new AssertionScope() )
            {
                result.Should().HaveCount( 4 );
                result.Keys.Should().BeEquivalentTo( "a", "b", "c", "d" );
                result.GetValueOrDefault( "a" ).Should().Be( v1 );
                result.GetValueOrDefault( "b" ).Should().Be( v2 );
                result.GetValueOrDefault( "c" ).Should().Be( v3 );
                result.GetValueOrDefault( "d" ).Should().Be( v4 );
            }
        }
    }
}
