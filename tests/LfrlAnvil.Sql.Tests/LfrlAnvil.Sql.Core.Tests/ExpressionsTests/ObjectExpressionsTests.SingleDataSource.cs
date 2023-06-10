using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Expressions;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public partial class ObjectExpressionsTests
{
    public class SingleDataSource : TestsBase
    {
        [Fact]
        public void GetRecordSet_ShouldReturnFrom_WhenNameEqualsFromName()
        {
            var from = SqlNode.RawRecordSet( "foo" );
            var sut = from.ToDataSource();

            var result = sut.GetRecordSet( "foo" );

            result.Should().BeSameAs( from );
        }

        [Fact]
        public void GetRecordSet_ShouldThrowKeyNotFoundException_WhenNameDoesNotEqualFromName()
        {
            var from = SqlNode.RawRecordSet( "foo" );
            var sut = from.ToDataSource();

            var action = Lambda.Of( () => sut.GetRecordSet( "bar" ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void Indexer_ShouldBeEquivalentToGetRecordSet()
        {
            var from = SqlNode.RawRecordSet( "foo" );
            var sut = from.ToDataSource();

            var result = sut["foo"]["bar"];

            result.Should().BeEquivalentTo( from["bar"] );
        }
    }
}
