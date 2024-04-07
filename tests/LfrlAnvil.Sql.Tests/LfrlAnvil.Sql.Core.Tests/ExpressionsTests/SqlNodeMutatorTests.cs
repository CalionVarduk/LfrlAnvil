using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.TestExtensions.Attributes;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

[TestClass( typeof( SqlNodeMutatorTestsData ) )]
public class SqlNodeMutatorTests : TestsBase
{
    [Fact]
    public void DefaultContext_ShouldReturnProvidedNode()
    {
        var node = SqlNode.Literal( 1 ) + SqlNode.Literal( 2 );
        var sut = new SqlNodeMutatorContext();

        var result = sut.Visit( node );

        result.Should().BeSameAs( node );
    }

    [Theory]
    [MethodData( nameof( SqlNodeMutatorTestsData.GetReplacementData ) )]
    public void ReplacedNodes_ShouldReturnCorrectReplacement(SqlNodeBase node, bool asLeaf, string expected)
    {
        var two = SqlNode.Literal( 2 );
        var sut = new ReplaceContext( Nodes( node, two ), Nodes( SqlNode.Literal( 1 ) + two, SqlNode.Literal( "foo" ) ), asLeaf );

        var result = sut.Visit( node );

        result.ToString().Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( SqlNodeMutatorTestsData.GetLeafNodesData ) )]
    public void LeafNodes_ShouldReturnSelfWhenNotReplaced(SqlNodeBase node, SqlNodeBase? child, bool asLeaf)
    {
        var sut = new ReplaceContext( Nodes( node, child ?? node ), Nodes( node, child ?? node ), asLeaf );
        var result = sut.Visit( node );
        result.Should().BeSameAs( node );
    }

    [Theory]
    [MethodData( nameof( SqlNodeMutatorTestsData.GetDataFieldsData ) )]
    public void DataFields_ShouldVisitRecordSetWhenReturnedAsNode(
        SqlDataFieldNode field,
        SqlRecordSetNode? recordSetReplacement,
        bool asLeaf,
        string expected)
    {
        var sut = new ReplaceContext( Nodes( field, field.RecordSet ), Nodes( field, recordSetReplacement ?? field.RecordSet ), asLeaf );
        var result = sut.Visit( field );
        result.ToString().Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( SqlNodeMutatorTestsData.GetOperatorsData ) )]
    public void Operators_ShouldVisitAllArgumentsWhenReturnedAsNode(
        SqlNodeBase @operator,
        (SqlNodeBase Node, SqlNodeBase Replacement)? argument,
        bool asLeaf,
        string expected)
    {
        var sut = new ReplaceContext(
            Nodes( @operator, argument?.Node ?? @operator ),
            Nodes( @operator, argument?.Replacement ?? @operator ),
            asLeaf );

        var result = sut.Visit( @operator );
        result.ToString().Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( SqlNodeMutatorTestsData.GetFunctionsData ) )]
    public void Functions_ShouldVisitAllArgumentsWhenReturnedAsNode(
        SqlExpressionNode function,
        (SqlExpressionNode Node, SqlExpressionNode Replacement)? argument,
        bool asLeaf,
        string expected)
    {
        var sut = new ReplaceContext(
            Nodes( function, argument?.Node ?? function ),
            Nodes( function, argument?.Replacement ?? function ),
            asLeaf );

        var result = sut.Visit( function );
        result.ToString().Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( SqlNodeMutatorTestsData.GetAggregateFunctionTraitsData ) )]
    public void AggregateFunctions_ShouldVisitAllTraitsWhenReturnedAsNode(
        SqlAggregateFunctionExpressionNode function,
        (SqlTraitNode Node, SqlTraitNode Replacement)? trait,
        bool asLeaf,
        string expected)
    {
        var sut = new ReplaceContext(
            Nodes( function, ( SqlNodeBase? )trait?.Node ?? function ),
            Nodes( function, ( SqlNodeBase? )trait?.Replacement ?? function ),
            asLeaf );

        var result = sut.Visit( function );
        result.ToString().Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( SqlNodeMutatorTestsData.GetDataSourcesData ) )]
    public void DataSources_ShouldVisitFromAndAllJoinsAndAllTraitsWhenReturnedAsNode(
        SqlDataSourceNode dataSource,
        (SqlNodeBase Node, SqlNodeBase Replacement)? child,
        bool asLeaf,
        string expected)
    {
        var sut = new ReplaceContext(
            Nodes( dataSource, child?.Node ?? dataSource ),
            Nodes( dataSource, child?.Replacement ?? dataSource ),
            asLeaf );

        var result = sut.Visit( dataSource );
        result.ToString().Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( SqlNodeMutatorTestsData.GetQueriesData ) )]
    public void Queries_ShouldVisitAllTraitsAndOtherChildrenWhenReturnedAsNode(
        SqlQueryExpressionNode query,
        (SqlNodeBase Node, SqlNodeBase Replacement)? child,
        bool asLeaf,
        string expected)
    {
        var sut = new ReplaceContext(
            Nodes( query, child?.Node ?? query ),
            Nodes( query, child?.Replacement ?? query ),
            asLeaf );

        var result = sut.Visit( query );
        result.ToString().Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( SqlNodeMutatorTestsData.GetDataModificationsData ) )]
    public void DataModifications_ShouldVisitAllChildrenWhenReturnedAsNode(
        SqlNodeBase modification,
        (SqlNodeBase Node, SqlNodeBase Replacement)? child,
        bool asLeaf,
        string expected)
    {
        var sut = new ReplaceContext(
            Nodes( modification, child?.Node ?? modification ),
            Nodes( modification, child?.Replacement ?? modification ),
            asLeaf );

        var result = sut.Visit( modification );
        result.ToString().Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( SqlNodeMutatorTestsData.GetSchemaModificationsData ) )]
    public void SchemaModifications_ShouldVisitAllChildrenWhenReturnedAsNode(
        SqlNodeBase modification,
        (SqlNodeBase Node, SqlNodeBase Replacement)? child,
        bool asLeaf,
        string expected)
    {
        var sut = new ReplaceContext(
            Nodes( modification, child?.Node ?? modification ),
            Nodes( modification, child?.Replacement ?? modification ),
            asLeaf );

        var result = sut.Visit( modification );
        result.ToString().Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( SqlNodeMutatorTestsData.GetMiscData ) )]
    public void MiscNodes_ShouldVisitAllChildrenWhenReturnedAsNode(
        SqlNodeBase function,
        (SqlNodeBase Node, SqlNodeBase Replacement)? argument,
        bool asLeaf,
        string expected)
    {
        var sut = new ReplaceContext(
            Nodes( function, argument?.Node ?? function ),
            Nodes( function, argument?.Replacement ?? function ),
            asLeaf );

        var result = sut.Visit( function );
        result.ToString().Should().Be( expected );
    }

    [Fact]
    public void Visit_ShouldThrowSqlNodeMutatorException_WhenReplacedNodeIsOfInvalidType()
    {
        var node = SqlNode.Parameter( "a" );
        var replacement = SqlNode.True();
        var parent = SqlNode.Column<int>( "a", defaultValue: node );
        var sut = new ReplaceContext( Nodes( node ), Nodes( replacement ), asLeaf: Fixture.Create<bool>() );

        var action = Lambda.Of( () => sut.Visit( parent ) );

        action.Should()
            .ThrowExactly<SqlNodeMutatorException>()
            .AndMatch(
                e => ReferenceEquals( e.Parent, parent )
                    && ReferenceEquals( e.Node, node )
                    && ReferenceEquals( e.Result, replacement )
                    && e.ExpectedType == typeof( SqlExpressionNode ) );
    }

    [Fact]
    public void Visit_ShouldDynamicallyUpdateAncestorsCollection()
    {
        var nodes = new[]
        {
            SqlNode.Parameter( "a" ),
            SqlNode.Parameter( "b" ),
            SqlNode.Parameter( "c" ),
            SqlNode.Parameter( "d" ),
            SqlNode.Parameter( "e" ),
            SqlNode.Parameter( "f" ),
            SqlNode.Parameter( "g" ),
            SqlNode.Parameter( "h" ),
            SqlNode.Parameter( "i" ),
            SqlNode.Parameter( "j" )
        };

        var ab = nodes[0] + nodes[1];
        var abc = ab + nodes[2];
        var de = nodes[3] + nodes[4];
        var def = de + nodes[5];
        var ij = nodes[8] + nodes[9];
        var hij = nodes[7] + ij;
        var ghij = nodes[6] + hij;
        var abcdef = abc + def;
        var abcdefghij = abcdef + ghij;

        var sut = new ExtractAncestorsContext();

        _ = sut.Visit( abcdefghij );

        using ( new AssertionScope() )
        {
            sut.AncestorsByParameterName.GetValueOrDefault( "a" ).Should().BeSequentiallyEqualTo( ab, abc, abcdef, abcdefghij );
            sut.AncestorsByParameterName.GetValueOrDefault( "b" ).Should().BeSequentiallyEqualTo( ab, abc, abcdef, abcdefghij );
            sut.AncestorsByParameterName.GetValueOrDefault( "c" ).Should().BeSequentiallyEqualTo( abc, abcdef, abcdefghij );
            sut.AncestorsByParameterName.GetValueOrDefault( "d" ).Should().BeSequentiallyEqualTo( de, def, abcdef, abcdefghij );
            sut.AncestorsByParameterName.GetValueOrDefault( "e" ).Should().BeSequentiallyEqualTo( de, def, abcdef, abcdefghij );
            sut.AncestorsByParameterName.GetValueOrDefault( "f" ).Should().BeSequentiallyEqualTo( def, abcdef, abcdefghij );
            sut.AncestorsByParameterName.GetValueOrDefault( "g" ).Should().BeSequentiallyEqualTo( ghij, abcdefghij );
            sut.AncestorsByParameterName.GetValueOrDefault( "h" ).Should().BeSequentiallyEqualTo( hij, ghij, abcdefghij );
            sut.AncestorsByParameterName.GetValueOrDefault( "i" ).Should().BeSequentiallyEqualTo( ij, hij, ghij, abcdefghij );
            sut.AncestorsByParameterName.GetValueOrDefault( "j" ).Should().BeSequentiallyEqualTo( ij, hij, ghij, abcdefghij );
        }
    }

    [Theory]
    [InlineData( "a", 0 )]
    [InlineData( "b", 1 )]
    [InlineData( "c", 2 )]
    [InlineData( "d", -1 )]
    public void Ancestors_FindIndex_ShouldReturnCorrectResult(string name, int expected)
    {
        var parameters = new[] { SqlNode.Parameter( "a" ), SqlNode.Parameter( "b" ), SqlNode.Parameter( "c" ), SqlNode.Parameter( "d" ) };
        var sut = new SqlNodeAncestors(
            new List<SqlNodeBase>
            {
                parameters[2],
                parameters[1],
                parameters[0]
            } );

        var node = parameters.First( p => p.Name == name );

        var result = sut.FindIndex( node );

        result.Should().Be( expected );
    }

    [Pure]
    private static SqlNodeBase[] Nodes(params SqlNodeBase[] nodes)
    {
        return nodes;
    }

    private sealed class ReplaceContext : SqlNodeMutatorContext
    {
        internal ReplaceContext(SqlNodeBase[] original, SqlNodeBase[] replacement, bool asLeaf)
        {
            Original = original;
            Replacement = replacement;
            AsLeaf = asLeaf;
        }

        internal SqlNodeBase[] Original { get; }
        internal SqlNodeBase[] Replacement { get; }
        internal bool AsLeaf { get; }

        protected internal override MutationResult Mutate(SqlNodeBase node, SqlNodeAncestors ancestors)
        {
            for ( var i = 0; i < Original.Length; ++i )
            {
                if ( ReferenceEquals( Original[i], node ) )
                    return AsLeaf ? Replacement[i] : Node( Replacement[i] );
            }

            return base.Mutate( node, ancestors );
        }
    }

    private sealed class ExtractAncestorsContext : SqlNodeMutatorContext
    {
        internal readonly Dictionary<string, SqlNodeBase[]> AncestorsByParameterName = new Dictionary<string, SqlNodeBase[]>();

        protected internal override MutationResult Mutate(SqlNodeBase node, SqlNodeAncestors ancestors)
        {
            if ( node is SqlParameterNode p )
            {
                var parameterAncestors = new SqlNodeBase[ancestors.Count];
                for ( var i = 0; i < parameterAncestors.Length; ++i )
                    parameterAncestors[i] = ancestors[i];

                AncestorsByParameterName.Add( p.Name, parameterAncestors );
            }

            return base.Mutate( node, ancestors );
        }
    }
}
