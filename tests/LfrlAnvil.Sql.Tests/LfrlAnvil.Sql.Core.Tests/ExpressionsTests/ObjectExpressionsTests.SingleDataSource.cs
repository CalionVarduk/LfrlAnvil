using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Traits;

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

            result.TestRefEquals( from ).Go();
        }

        [Fact]
        public void GetRecordSet_ShouldThrowKeyNotFoundException_WhenNameDoesNotEqualFromName()
        {
            var from = SqlNode.RawRecordSet( "foo" );
            var sut = from.ToDataSource();

            var action = Lambda.Of( () => sut.GetRecordSet( "bar" ) );

            action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
        }

        [Fact]
        public void Indexer_ShouldBeEquivalentToGetRecordSet()
        {
            var from = SqlNode.RawRecordSet( "foo" );
            var sut = from.ToDataSource();

            var result = sut["foo"]["bar"];

            Assertion.All(
                    result.Name.TestEquals( "bar" ),
                    result.RecordSet.TestRefEquals( from ) )
                .Go();
        }

        [Fact]
        public void AddTrait_ShouldCreateSingleDataSourceWithTrait_WhenCalledForTheFirstTime()
        {
            var sut = SqlNode.RawRecordSet( "foo" ).ToDataSource();
            var trait = SqlNode.FilterTrait( SqlNode.RawCondition( "a > 10" ), isConjunction: true );
            var result = sut.AddTrait( trait );
            var text = result.ToString();

            Assertion.All(
                    result.TestNotRefEquals( sut ),
                    result.Traits.TestSequence( [ trait ] ),
                    text.TestEquals(
                        """
                        FROM [foo]
                        AND WHERE a > 10
                        """ ) )
                .Go();
        }

        [Fact]
        public void AddTrait_ShouldCreateSingleDataSourceWithTraits_WhenCalledForTheSecondTime()
        {
            var firstTrait = SqlNode.FilterTrait( SqlNode.RawCondition( "a > 10" ), isConjunction: true );
            var sut = SqlNode.RawRecordSet( "foo" ).ToDataSource().AddTrait( firstTrait );
            var secondTrait = SqlNode.FilterTrait( SqlNode.RawCondition( "b > 15" ), isConjunction: false );
            var result = sut.AddTrait( secondTrait );
            var text = result.ToString();

            Assertion.All(
                    result.TestNotRefEquals( sut ),
                    result.Traits.TestSequence( [ firstTrait, secondTrait ] ),
                    text.TestEquals(
                        """
                        FROM [foo]
                        AND WHERE a > 10
                        OR WHERE b > 15
                        """ ) )
                .Go();
        }

        [Fact]
        public void SetTraits_ShouldCreateSingleDataSourceWithOverriddenTraits()
        {
            var sut = SqlNode.RawRecordSet( "foo" ).ToDataSource().AddTrait( SqlNode.DistinctTrait() );
            var traits = Chain.Create<SqlTraitNode>( SqlNode.FilterTrait( SqlNode.RawCondition( "a > 10" ), isConjunction: true ) )
                .Extend( SqlNode.FilterTrait( SqlNode.RawCondition( "b > 15" ), isConjunction: false ) );

            var result = sut.SetTraits( traits );
            var text = result.ToString();

            Assertion.All(
                    result.TestNotRefEquals( sut ),
                    result.Traits.TestSequence( traits ),
                    text.TestEquals(
                        """
                        FROM [foo]
                        AND WHERE a > 10
                        OR WHERE b > 15
                        """ ) )
                .Go();
        }
    }
}
