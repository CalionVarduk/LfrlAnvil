using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public partial class ObjectExpressionsTests
{
    public class MultiDataSource : TestsBase
    {
        [Fact]
        public void Ctor_ShouldThrowArgumentException_WhenMultipleRecordSetsWithSameNameExist()
        {
            var from = SqlNode.RawRecordSet( "foo" );
            var action = Lambda.Of( () => from.Join( from.InnerOn( SqlNode.True() ) ) );
            action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
        }

        [Fact]
        public void Ctor_ShouldInitializeComplexDataSourceWithDifferentJoinTypes_ByMarkingRecordSetsAsOptionalCorrectly()
        {
            var a = SqlNode.RawRecordSet( "a" );
            var b = SqlNode.RawRecordSet( "b" );
            var c = SqlNode.RawRecordSet( "c" );
            var d = SqlNode.RawRecordSet( "d" );
            var e = SqlNode.RawRecordSet( "e" );
            var f = SqlNode.RawRecordSet( "f" );
            var g = SqlNode.RawRecordSet( "g" );
            var h = SqlNode.RawRecordSet( "h" );
            var ongoingSets = new List<SqlRecordSetNode[]>();
            var ongoingInners = new List<SqlRecordSetNode>();

            var sut = a.Join(
                SqlJoinDefinition.Inner( b, p => AddOngoing( p, "a" ) ),
                SqlJoinDefinition.Left( c, p => AddOngoing( p, "a", "b" ) ),
                SqlJoinDefinition.Inner( d, p => AddOngoing( p, "a", "b", "c" ) ),
                SqlJoinDefinition.Right( e, p => AddOngoing( p, "a", "b", "c", "d" ) ),
                SqlJoinDefinition.Inner( f, p => AddOngoing( p, "a", "b", "c", "d", "e" ) ),
                SqlJoinDefinition.Full( g, p => AddOngoing( p, "a", "b", "c", "d", "e", "f" ) ),
                SqlJoinDefinition.Cross( h ) );

            Assertion.All(
                    sut.From.TestType()
                        .AssignableTo<SqlRawRecordSetNode>( s => Assertion.All(
                            s.IsOptional.TestTrue(),
                            s.Info.TestEquals( SqlRecordSetInfo.Create( "a" ) ) ) ),
                    sut.Joins.Select( j => j.JoinType )
                        .TestSequence(
                        [
                            SqlJoinType.Inner,
                            SqlJoinType.Left,
                            SqlJoinType.Inner,
                            SqlJoinType.Right,
                            SqlJoinType.Inner,
                            SqlJoinType.Full,
                            SqlJoinType.Cross
                        ] ),
                    sut.Joins.Select( j => j.InnerRecordSet ).TestSequence( [ b, c, d, e, f, g, h ] ),
                    sut.GetRecordSet( "a" )
                        .TestType()
                        .AssignableTo<SqlRawRecordSetNode>( s => Assertion.All(
                            s.IsOptional.TestTrue(),
                            s.Info.TestEquals( SqlRecordSetInfo.Create( "a" ) ) ) ),
                    sut.GetRecordSet( "b" )
                        .TestType()
                        .AssignableTo<SqlRawRecordSetNode>( s => Assertion.All(
                            s.IsOptional.TestTrue(),
                            s.Info.TestEquals( SqlRecordSetInfo.Create( "b" ) ) ) ),
                    sut.GetRecordSet( "c" )
                        .TestType()
                        .AssignableTo<SqlRawRecordSetNode>( s => Assertion.All(
                            s.IsOptional.TestTrue(),
                            s.Info.TestEquals( SqlRecordSetInfo.Create( "c" ) ) ) ),
                    sut.GetRecordSet( "d" )
                        .TestType()
                        .AssignableTo<SqlRawRecordSetNode>( s => Assertion.All(
                            s.IsOptional.TestTrue(),
                            s.Info.TestEquals( SqlRecordSetInfo.Create( "d" ) ) ) ),
                    sut.GetRecordSet( "e" )
                        .TestType()
                        .AssignableTo<SqlRawRecordSetNode>( s => Assertion.All(
                            s.IsOptional.TestTrue(),
                            s.Info.TestEquals( SqlRecordSetInfo.Create( "e" ) ) ) ),
                    sut.GetRecordSet( "f" )
                        .TestType()
                        .AssignableTo<SqlRawRecordSetNode>( s => Assertion.All(
                            s.IsOptional.TestTrue(),
                            s.Info.TestEquals( SqlRecordSetInfo.Create( "f" ) ) ) ),
                    sut.GetRecordSet( "g" )
                        .TestType()
                        .AssignableTo<SqlRawRecordSetNode>( s => Assertion.All(
                            s.IsOptional.TestTrue(),
                            s.Info.TestEquals( SqlRecordSetInfo.Create( "g" ) ) ) ),
                    sut.GetRecordSet( "h" ).TestRefEquals( h ),
                    ongoingInners.TestSequence( [ b, c, d, e, f, g ] ),
                    ongoingSets.TestCount( count => count.TestEquals( 6 ) )
                        .Then( sets =>
                            Assertion.All(
                                "ongoingSets",
                                sets[0].TestSequence( [ a ] ),
                                sets[1].TestSequence( [ a, b ] ),
                                sets[2]
                                    .TestSequence(
                                    [
                                        (s, _) => s.TestRefEquals( a ),
                                        (s, _) => s.TestRefEquals( b ),
                                        (s, _) => s.TestType()
                                            .AssignableTo<SqlRawRecordSetNode>( rs => Assertion.All(
                                                rs.IsOptional.TestTrue(),
                                                rs.Info.TestEquals( SqlRecordSetInfo.Create( "c" ) ) ) )
                                    ] ),
                                sets[3]
                                    .TestSequence(
                                    [
                                        (s, _) => s.TestRefEquals( a ),
                                        (s, _) => s.TestRefEquals( b ),
                                        (s, _) => s.TestType()
                                            .AssignableTo<SqlRawRecordSetNode>( rs => Assertion.All(
                                                rs.IsOptional.TestTrue(),
                                                rs.Info.TestEquals( SqlRecordSetInfo.Create( "c" ) ) ) ),
                                        (s, _) => s.TestRefEquals( d )
                                    ] ),
                                sets[4]
                                    .TestSequence(
                                    [
                                        (s, _) => s.TestType()
                                            .AssignableTo<SqlRawRecordSetNode>( rs => Assertion.All(
                                                rs.IsOptional.TestTrue(),
                                                rs.Info.TestEquals( SqlRecordSetInfo.Create( "a" ) ) ) ),
                                        (s, _) => s.TestType()
                                            .AssignableTo<SqlRawRecordSetNode>( rs => Assertion.All(
                                                rs.IsOptional.TestTrue(),
                                                rs.Info.TestEquals( SqlRecordSetInfo.Create( "b" ) ) ) ),
                                        (s, _) => s.TestType()
                                            .AssignableTo<SqlRawRecordSetNode>( rs => Assertion.All(
                                                rs.IsOptional.TestTrue(),
                                                rs.Info.TestEquals( SqlRecordSetInfo.Create( "c" ) ) ) ),
                                        (s, _) => s.TestType()
                                            .AssignableTo<SqlRawRecordSetNode>( rs => Assertion.All(
                                                rs.IsOptional.TestTrue(),
                                                rs.Info.TestEquals( SqlRecordSetInfo.Create( "d" ) ) ) ),
                                        (s, _) => s.TestRefEquals( e )
                                    ] ),
                                sets[5]
                                    .TestSequence(
                                    [
                                        (s, _) => s.TestType()
                                            .AssignableTo<SqlRawRecordSetNode>( rs => Assertion.All(
                                                rs.IsOptional.TestTrue(),
                                                rs.Info.TestEquals( SqlRecordSetInfo.Create( "a" ) ) ) ),
                                        (s, _) => s.TestType()
                                            .AssignableTo<SqlRawRecordSetNode>( rs => Assertion.All(
                                                rs.IsOptional.TestTrue(),
                                                rs.Info.TestEquals( SqlRecordSetInfo.Create( "b" ) ) ) ),
                                        (s, _) => s.TestType()
                                            .AssignableTo<SqlRawRecordSetNode>( rs => Assertion.All(
                                                rs.IsOptional.TestTrue(),
                                                rs.Info.TestEquals( SqlRecordSetInfo.Create( "c" ) ) ) ),
                                        (s, _) => s.TestType()
                                            .AssignableTo<SqlRawRecordSetNode>( rs => Assertion.All(
                                                rs.IsOptional.TestTrue(),
                                                rs.Info.TestEquals( SqlRecordSetInfo.Create( "d" ) ) ) ),
                                        (s, _) => s.TestRefEquals( e ),
                                        (s, _) => s.TestRefEquals( f )
                                    ] ) ) ) )
                .Go();

            SqlConditionNode AddOngoing(SqlJoinDefinition.ExpressionParams @params, params string[] outerSetNames)
            {
                ongoingInners.Add( @params.Inner );
                ongoingSets.Add( outerSetNames.Select( @params.GetOuter ).ToArray() );
                return SqlNode.True();
            }
        }

        [Fact]
        public void GetRecordSet_ShouldReturnFrom_WhenNameEqualsFromName()
        {
            var from = SqlNode.RawRecordSet( "foo" );
            var inner = SqlNode.RawRecordSet( "bar" );
            var sut = from.Join( inner.InnerOn( SqlNode.True() ) );

            var result = sut.GetRecordSet( "foo" );

            result.TestRefEquals( from ).Go();
        }

        [Fact]
        public void GetRecordSet_ShouldReturnJoinedRecordSet_WhenNameEqualsItsName()
        {
            var from = SqlNode.RawRecordSet( "foo" );
            var inner = SqlNode.RawRecordSet( "bar" );
            var sut = from.Join( inner.InnerOn( SqlNode.True() ) );

            var result = sut.GetRecordSet( "bar" );

            result.TestRefEquals( inner ).Go();
        }

        [Fact]
        public void GetRecordSet_ShouldThrowKeyNotFoundException_WhenNameDoesNotExist()
        {
            var from = SqlNode.RawRecordSet( "foo" );
            var inner = SqlNode.RawRecordSet( "bar" );
            var sut = from.Join( inner.InnerOn( SqlNode.True() ) );

            var action = Lambda.Of( () => sut.GetRecordSet( "qux" ) );

            action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
        }

        [Theory]
        [InlineData( "foo" )]
        [InlineData( "bar" )]
        public void Indexer_ShouldBeEquivalentToGetRecordSet(string name)
        {
            var from = SqlNode.RawRecordSet( "foo" );
            var inner = SqlNode.RawRecordSet( "bar" );
            var sut = from.Join( inner.InnerOn( SqlNode.True() ) );

            var result = sut[name];

            result.TestRefEquals( sut.GetRecordSet( name ) ).Go();
        }

        [Fact]
        public void AddTrait_ShouldCreateMultiDataSourceWithTrait_WhenCalledForTheFirstTime()
        {
            var sut = SqlNode.RawRecordSet( "foo" ).Join( SqlNode.RawRecordSet( "bar" ).InnerOn( SqlNode.True() ) );
            var trait = SqlNode.FilterTrait( SqlNode.RawCondition( "a > 10" ), isConjunction: true );
            var result = sut.AddTrait( trait );
            var text = result.ToString();

            Assertion.All(
                    result.TestNotRefEquals( sut ),
                    result.Traits.TestSequence( [ trait ] ),
                    text.TestEquals(
                        """
                        FROM [foo]
                        INNER JOIN [bar] ON TRUE
                        AND WHERE a > 10
                        """ ) )
                .Go();
        }

        [Fact]
        public void AddTrait_ShouldCreateMultiDataSourceWithTraits_WhenCalledForTheSecondTime()
        {
            var firstTrait = SqlNode.FilterTrait( SqlNode.RawCondition( "a > 10" ), isConjunction: true );
            var sut = SqlNode.RawRecordSet( "foo" )
                .Join( SqlNode.RawRecordSet( "bar" ).InnerOn( SqlNode.True() ) )
                .AddTrait( firstTrait );

            var secondTrait = SqlNode.FilterTrait( SqlNode.RawCondition( "b > 15" ), isConjunction: false );
            var result = sut.AddTrait( secondTrait );
            var text = result.ToString();

            Assertion.All(
                    result.TestNotRefEquals( sut ),
                    result.Traits.TestSequence( [ firstTrait, secondTrait ] ),
                    text.TestEquals(
                        """
                        FROM [foo]
                        INNER JOIN [bar] ON TRUE
                        AND WHERE a > 10
                        OR WHERE b > 15
                        """ ) )
                .Go();
        }

        [Fact]
        public void SetTraits_ShouldCreateMultiDataSourceWithOverriddenTraits()
        {
            var sut = SqlNode.RawRecordSet( "foo" )
                .Join( SqlNode.RawRecordSet( "bar" ).InnerOn( SqlNode.True() ) )
                .AddTrait( SqlNode.DistinctTrait() );

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
                        INNER JOIN [bar] ON TRUE
                        AND WHERE a > 10
                        OR WHERE b > 15
                        """ ) )
                .Go();
        }
    }
}
