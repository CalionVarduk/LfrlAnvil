using System.Collections.Generic;
using System.Text;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Visitors;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public class SqlNodeInterpreterContextTests : TestsBase
{
    [Fact]
    public void Create_ShouldCreateWithStringBuilderWithDefaultCapacity()
    {
        var sut = SqlNodeInterpreterContext.Create();

        using ( new AssertionScope() )
        {
            sut.Indent.Should().Be( 0 );
            sut.ParentNode.Should().BeNull();
            sut.Parameters.Should().BeEmpty();
            sut.Sql.Length.Should().Be( 0 );
            sut.Sql.Capacity.Should().Be( 1024 );
        }
    }

    [Fact]
    public void Create_ShouldCreateWithExplicitStringBuilder()
    {
        var builder = new StringBuilder();
        var sut = SqlNodeInterpreterContext.Create( builder );

        using ( new AssertionScope() )
        {
            sut.Indent.Should().Be( 0 );
            sut.ParentNode.Should().BeNull();
            sut.Parameters.Should().BeEmpty();
            sut.Sql.Should().BeSameAs( builder );
        }
    }

    [Fact]
    public void IncreaseIndent_ShouldSetIndentationToTwoSpaces_WhenCurrentIndentIsEmpty()
    {
        var sut = SqlNodeInterpreterContext.Create();
        sut.IncreaseIndent();
        sut.Indent.Should().Be( 2 );
    }

    [Fact]
    public void IncreaseIndent_ShouldAddTwoSpacesToCurrentNonEmptyIndent()
    {
        var sut = SqlNodeInterpreterContext.Create();
        sut.IncreaseIndent();

        sut.IncreaseIndent();

        sut.Indent.Should().Be( 4 );
    }

    [Fact]
    public void DecreaseIndent_ShouldDoNothing_WhenCurrentIndentIsEmpty()
    {
        var sut = SqlNodeInterpreterContext.Create();
        sut.DecreaseIndent();
        sut.Indent.Should().Be( 0 );
    }

    [Fact]
    public void DecreaseIndent_ShouldSetIndentToEmpty_WhenCurrentIndentIsEqualToTwoSpaces()
    {
        var sut = SqlNodeInterpreterContext.Create();
        sut.IncreaseIndent();

        sut.DecreaseIndent();

        sut.Indent.Should().Be( 0 );
    }

    [Fact]
    public void DecreaseIndent_ShouldRemoveTwoSpacesFromCurrentIndentation_WhenCurrentIndentIsGreaterThanTwoSpaces()
    {
        var sut = SqlNodeInterpreterContext.Create();
        sut.IncreaseIndent();
        sut.IncreaseIndent();

        sut.DecreaseIndent();

        sut.Indent.Should().Be( 2 );
    }

    [Fact]
    public void TempIndentIncrease_ShouldIncreaseIndentByTwoSpacesAndReturnObjectThatReducesIndentByTwoSpacesWhenDisposed()
    {
        var sut = SqlNodeInterpreterContext.Create();

        int indent;
        using ( sut.TempIndentIncrease() )
            indent = sut.Indent;

        using ( new AssertionScope() )
        {
            sut.Indent.Should().Be( 0 );
            indent.Should().Be( 2 );
        }
    }

    [Fact]
    public void SetParentNode_ShouldUpdateParentNode()
    {
        var parentNode = SqlNode.Null();
        var sut = SqlNodeInterpreterContext.Create();

        sut.SetParentNode( parentNode );

        sut.ParentNode.Should().BeSameAs( parentNode );
    }

    [Fact]
    public void ClearParentNode_ShouldResetParentNodeToNull()
    {
        var parentNode = SqlNode.Null();
        var sut = SqlNodeInterpreterContext.Create();
        sut.SetParentNode( parentNode );

        sut.ClearParentNode();

        sut.ParentNode.Should().BeNull();
    }

    [Fact]
    public void
        TempParentNodeUpdate_ShouldUpdateParentNodeAndReturnObjectThatResetsParentNodeToPreviousOneWhenDisposed_WhenCurrentParentNodeIsNotNull()
    {
        var node = SqlNode.Null();
        var current = SqlNode.True();
        var sut = SqlNodeInterpreterContext.Create();
        sut.SetParentNode( current );

        SqlNodeBase? parentNode;
        using ( sut.TempParentNodeUpdate( node ) )
            parentNode = sut.ParentNode;

        using ( new AssertionScope() )
        {
            sut.ParentNode.Should().BeSameAs( current );
            parentNode.Should().BeSameAs( node );
        }
    }

    [Fact]
    public void
        TempParentNodeUpdate_ShouldUpdateParentNodeAndReturnObjectThatResetsParentNodeToPreviousOneWhenDisposed_WhenCurrentParentNodeIsNull()
    {
        var node = SqlNode.Null();
        var sut = SqlNodeInterpreterContext.Create();

        SqlNodeBase? parentNode;
        using ( sut.TempParentNodeUpdate( node ) )
            parentNode = sut.ParentNode;

        using ( new AssertionScope() )
        {
            sut.ParentNode.Should().BeNull();
            parentNode.Should().BeSameAs( node );
        }
    }

    [Fact]
    public void AppendIndent_ShouldAppendNewLineWithCurrentIndentToStringBuilder()
    {
        var sut = SqlNodeInterpreterContext.Create();
        sut.IncreaseIndent();

        var result = sut.AppendIndent();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut.Sql );
            result.ToString().Should().Be( $"{Environment.NewLine}  " );
        }
    }

    [Fact]
    public void AppendShortIndent_ShouldAppendNewLineWithCurrentIndentReducedByTwoSpacesToStringBuilder()
    {
        var sut = SqlNodeInterpreterContext.Create();
        sut.IncreaseIndent();
        sut.IncreaseIndent();

        var result = sut.AppendShortIndent();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut.Sql );
            result.ToString().Should().Be( $"{Environment.NewLine}  " );
        }
    }

    [Fact]
    public void AppendShortIndent_ShouldAppendNewLineToStringBuilder_WhenCurrentIndentIsNotGreaterThanTwoSpaces()
    {
        var sut = SqlNodeInterpreterContext.Create();

        var result = sut.AppendShortIndent();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut.Sql );
            result.ToString().Should().Be( Environment.NewLine );
        }
    }

    [Fact]
    public void AddParameter_ShouldAddNewParameter()
    {
        var name = "foo";
        var type = SqlExpressionType.Create<int>();
        var sut = SqlNodeInterpreterContext.Create();

        sut.AddParameter( name, type );

        using ( new AssertionScope() )
        {
            sut.Parameters.Should().HaveCount( 1 );
            sut.Parameters.Should().BeEquivalentTo( KeyValuePair.Create( name, (SqlExpressionType?)type ) );
        }
    }

    [Fact]
    public void AddParameter_ShouldDoNothing_WhenParameterWithExactlyTheSameTypeAlreadyExists()
    {
        var name = "foo";
        var type = SqlExpressionType.Create<int>();
        var sut = SqlNodeInterpreterContext.Create();
        sut.AddParameter( name, type );

        sut.AddParameter( name, type );

        using ( new AssertionScope() )
        {
            sut.Parameters.Should().HaveCount( 1 );
            sut.Parameters.Should().BeEquivalentTo( KeyValuePair.Create( name, (SqlExpressionType?)type ) );
        }
    }

    [Fact]
    public void AddParameter_ShouldChangeTypeToNull_WhenParameterAlreadyExistsAndHasDifferentType()
    {
        var name = "foo";
        var originalType = SqlExpressionType.Create<int>();
        var newType = SqlExpressionType.Create<int>( isNullable: true );
        var sut = SqlNodeInterpreterContext.Create();
        sut.AddParameter( name, originalType );

        sut.AddParameter( name, newType );

        using ( new AssertionScope() )
        {
            sut.Parameters.Should().HaveCount( 1 );
            sut.Parameters.Should().BeEquivalentTo( KeyValuePair.Create( name, (SqlExpressionType?)null ) );
        }
    }

    [Fact]
    public void AddParameter_ShouldAddSecondParameterCorrectly()
    {
        var firstName = "foo";
        var firstType = SqlExpressionType.Create<int>();
        var secondName = "bar";
        var secondType = SqlExpressionType.Create<string>();
        var sut = SqlNodeInterpreterContext.Create();
        sut.AddParameter( firstName, firstType );

        sut.AddParameter( secondName, secondType );

        using ( new AssertionScope() )
        {
            sut.Parameters.Should().HaveCount( 2 );
            sut.Parameters.Should()
                .BeEquivalentTo(
                    KeyValuePair.Create( firstName, (SqlExpressionType?)firstType ),
                    KeyValuePair.Create( secondName, (SqlExpressionType?)secondType ) );
        }
    }

    [Fact]
    public void TryGetParameterType_ShouldReturnFalse_WhenParametersAreEmpty()
    {
        var sut = SqlNodeInterpreterContext.Create();

        var result = sut.TryGetParameterType( "foo", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
        }
    }

    [Fact]
    public void TryGetParameterType_ShouldReturnFalse_WhenParameterDoesNotExist()
    {
        var sut = SqlNodeInterpreterContext.Create();
        sut.AddParameter( "foo", SqlExpressionType.Create<int>() );

        var result = sut.TryGetParameterType( "bar", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
        }
    }

    [Fact]
    public void TryGetParameterType_ShouldReturnTrue_WhenParameterExists()
    {
        var name = "foo";
        var type = SqlExpressionType.Create<int>();
        var sut = SqlNodeInterpreterContext.Create();
        sut.AddParameter( name, type );

        var result = sut.TryGetParameterType( name, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().Be( type );
        }
    }

    [Fact]
    public void Clear_ShouldResetAllContextData()
    {
        var sut = SqlNodeInterpreterContext.Create();
        sut.IncreaseIndent();
        sut.SetParentNode( SqlNode.Null() );
        sut.AddParameter( "foo", SqlExpressionType.Create<int>() );
        sut.Sql.Append( "SELECT * FROM bar" );

        sut.Clear();

        using ( new AssertionScope() )
        {
            sut.Indent.Should().Be( 0 );
            sut.ParentNode.Should().BeNull();
            sut.Sql.Length.Should().Be( 0 );
            sut.Parameters.Should().BeEmpty();
        }
    }
}
