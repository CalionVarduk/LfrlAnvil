using System.Text;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.TestExtensions.FluentAssertions;

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
            sut.ChildDepth.Should().Be( 0 );
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
            sut.ChildDepth.Should().Be( 0 );
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
    public void IncreaseChildDepth_ShouldSetChildDepthToOne_WhenCurrentChildDepthIsZero()
    {
        var sut = SqlNodeInterpreterContext.Create();
        sut.IncreaseChildDepth();
        sut.ChildDepth.Should().Be( 1 );
    }

    [Fact]
    public void IncreaseChildDepth_ShouldIncrementCurrentChildDepth()
    {
        var sut = SqlNodeInterpreterContext.Create();
        sut.IncreaseChildDepth();

        sut.IncreaseChildDepth();

        sut.ChildDepth.Should().Be( 2 );
    }

    [Fact]
    public void DecreaseChildDepth_ShouldDoNothing_WhenCurrentChildDepthIsZero()
    {
        var sut = SqlNodeInterpreterContext.Create();
        sut.DecreaseChildDepth();
        sut.ChildDepth.Should().Be( 0 );
    }

    [Fact]
    public void DecreaseChildDepth_ShouldSetChildDepthToZero_WhenCurrentChildDepthIsEqualToOn()
    {
        var sut = SqlNodeInterpreterContext.Create();
        sut.IncreaseChildDepth();

        sut.DecreaseChildDepth();

        sut.ChildDepth.Should().Be( 0 );
    }

    [Fact]
    public void DecreaseChildDepth_ShouldRemoveDecrementCurrentChildDepth_WhenCurrentChildDepthIsGreaterThanZero()
    {
        var sut = SqlNodeInterpreterContext.Create();
        sut.IncreaseChildDepth();
        sut.IncreaseChildDepth();

        sut.DecreaseChildDepth();

        sut.ChildDepth.Should().Be( 1 );
    }

    [Fact]
    public void TempChildDepthIncrease_ShouldIncrementChildDepthAndReturnObjectThatDecrementsChildDepthWhenDisposed()
    {
        var sut = SqlNodeInterpreterContext.Create();

        int depth;
        using ( sut.TempChildDepthIncrease() )
            depth = sut.ChildDepth;

        using ( new AssertionScope() )
        {
            sut.ChildDepth.Should().Be( 0 );
            depth.Should().Be( 1 );
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

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( null )]
    public void AddParameter_ShouldAddNewParameter(int? index)
    {
        var name = "foo";
        var type = TypeNullability.Create<int>();
        var sut = SqlNodeInterpreterContext.Create();

        sut.AddParameter( name, type, index );

        sut.Parameters.Should().BeSequentiallyEqualTo( new SqlNodeInterpreterContextParameter( name, type, index ) );
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( null )]
    public void AddParameter_ShouldDoNothing_WhenParameterWithExactlyTheSameTypeAndIndexAlreadyExists(int? index)
    {
        var name = "foo";
        var type = TypeNullability.Create<int>();
        var sut = SqlNodeInterpreterContext.Create();
        sut.AddParameter( name, type, index );

        sut.AddParameter( name, type, index );

        sut.Parameters.Should().BeSequentiallyEqualTo( new SqlNodeInterpreterContextParameter( name, type, index ) );
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( null )]
    public void AddParameter_ShouldChangeTypeToNull_WhenParameterAlreadyExistsAndHasDifferentType(int? index)
    {
        var name = "foo";
        var originalType = TypeNullability.Create<int>();
        var newType = TypeNullability.Create<int>( isNullable: true );
        var sut = SqlNodeInterpreterContext.Create();
        sut.AddParameter( name, originalType, index );

        sut.AddParameter( name, newType, index );

        sut.Parameters.Should().BeSequentiallyEqualTo( new SqlNodeInterpreterContextParameter( name, null, index ) );
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( null )]
    public void AddParameter_ShouldChangeIndexToNull_WhenParameterAlreadyExistsAndHasDifferentIndex(int? index)
    {
        var name = "foo";
        var type = TypeNullability.Create<int>();
        var sut = SqlNodeInterpreterContext.Create();
        sut.AddParameter( name, type, index );

        sut.AddParameter( name, type, index: 2 );

        sut.Parameters.Should().BeSequentiallyEqualTo( new SqlNodeInterpreterContextParameter( name, type, null ) );
    }

    [Fact]
    public void AddParameter_ShouldAddSecondParameterCorrectly()
    {
        var firstName = "foo";
        var firstType = TypeNullability.Create<int>();
        var secondName = "bar";
        var secondType = TypeNullability.Create<string>();
        var sut = SqlNodeInterpreterContext.Create();
        sut.AddParameter( firstName, firstType, null );

        sut.AddParameter( secondName, secondType, null );

        sut.Parameters.Should()
            .BeSequentiallyEqualTo(
                new SqlNodeInterpreterContextParameter( firstName, firstType, null ),
                new SqlNodeInterpreterContextParameter( secondName, secondType, null ) );
    }

    [Fact]
    public void TryGetParameter_ShouldReturnFalse_WhenParametersAreEmpty()
    {
        var sut = SqlNodeInterpreterContext.Create();

        var result = sut.TryGetParameter( "foo", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().Be( default( SqlNodeInterpreterContextParameter ) );
        }
    }

    [Fact]
    public void TryGetParameter_ShouldReturnFalse_WhenParameterDoesNotExist()
    {
        var sut = SqlNodeInterpreterContext.Create();
        sut.AddParameter( "foo", TypeNullability.Create<int>(), index: 1 );

        var result = sut.TryGetParameter( "bar", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().Be( default( SqlNodeInterpreterContextParameter ) );
        }
    }

    [Fact]
    public void TryGetParameter_ShouldReturnTrue_WhenParameterExists()
    {
        var name = "foo";
        var type = TypeNullability.Create<int>();
        var sut = SqlNodeInterpreterContext.Create();
        sut.AddParameter( name, type, index: 2 );

        var result = sut.TryGetParameter( name, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().Be( new SqlNodeInterpreterContextParameter( name, type, Index: 2 ) );
        }
    }

    [Fact]
    public void Clear_ShouldResetAllContextData()
    {
        var sut = SqlNodeInterpreterContext.Create();
        sut.IncreaseIndent();
        sut.IncreaseChildDepth();
        sut.AddParameter( "foo", TypeNullability.Create<int>(), null );
        sut.Sql.Append( "SELECT * FROM bar" );

        sut.Clear();

        using ( new AssertionScope() )
        {
            sut.Indent.Should().Be( 0 );
            sut.ChildDepth.Should().Be( 0 );
            sut.Sql.Length.Should().Be( 0 );
            sut.Parameters.Should().BeEmpty();
        }
    }

    [Fact]
    public void ToSnapshot_ShouldCreateReadonlySnapshotOfCurrentContextState()
    {
        var sut = SqlNodeInterpreterContext.Create();
        sut.AddParameter( "a", TypeNullability.Create<int>(), null );
        sut.AddParameter( "b", TypeNullability.Create<string>( isNullable: true ), index: 1 );
        sut.AddParameter( "c", null, index: 2 );
        sut.Sql.Append( "SELECT * FROM bar" );

        var result = sut.ToSnapshot();

        using ( new AssertionScope() )
        {
            result.Sql.Should().Be( "SELECT * FROM bar" );
            result.Parameters.ToArray().Should().HaveCount( 3 );
            result.Parameters[0].Should().BeEquivalentTo( SqlNode.Parameter( "a", TypeNullability.Create<int>() ) );
            result.Parameters[1]
                .Should()
                .BeEquivalentTo( SqlNode.Parameter( "b", TypeNullability.Create<string>( isNullable: true ), index: 1 ) );

            result.Parameters[2].Should().BeEquivalentTo( SqlNode.Parameter( "c", index: 2 ) );
            result.ToString().Should().Be( result.Sql );
            sut.Sql.ToString().Should().Be( result.Sql );
            sut.Parameters.Should().HaveCount( 3 );
        }
    }

    [Fact]
    public void Snapshot_ToExpression_ShouldCreateRawExpressionNode()
    {
        var context = SqlNodeInterpreterContext.Create();
        context.AddParameter( "a", TypeNullability.Create<int>(), null );
        context.Sql.Append( "SELECT * FROM bar" );
        var sut = context.ToSnapshot();

        var result = sut.ToExpression();

        using ( new AssertionScope() )
        {
            result.Sql.Should().Be( "SELECT * FROM bar" );
            result.Parameters.Should().BeSequentiallyEqualTo( result.Parameters );
            result.Type.Should().BeNull();
        }
    }

    [Fact]
    public void Snapshot_ToExpression_ShouldCreateRawExpressionNode_WithType()
    {
        var context = SqlNodeInterpreterContext.Create();
        context.AddParameter( "a", TypeNullability.Create<int>(), null );
        context.Sql.Append( "SELECT * FROM bar" );
        var sut = context.ToSnapshot();

        var result = sut.ToExpression( TypeNullability.Create<string>() );

        using ( new AssertionScope() )
        {
            result.Sql.Should().Be( "SELECT * FROM bar" );
            result.Parameters.Should().BeSequentiallyEqualTo( result.Parameters );
            result.Type.Should().Be( TypeNullability.Create<string>() );
        }
    }

    [Fact]
    public void Snapshot_ToCondition_ShouldCreateRawConditionNode()
    {
        var context = SqlNodeInterpreterContext.Create();
        context.AddParameter( "a", TypeNullability.Create<int>(), null );
        context.Sql.Append( "SELECT * FROM bar" );
        var sut = context.ToSnapshot();

        var result = sut.ToCondition();

        using ( new AssertionScope() )
        {
            result.Sql.Should().Be( "SELECT * FROM bar" );
            result.Parameters.Should().BeSequentiallyEqualTo( result.Parameters );
        }
    }

    [Fact]
    public void Snapshot_ToStatement_ShouldCreateRawStatementNode()
    {
        var context = SqlNodeInterpreterContext.Create();
        context.AddParameter( "a", TypeNullability.Create<int>(), null );
        context.Sql.Append( "SELECT * FROM bar" );
        var sut = context.ToSnapshot();

        var result = sut.ToStatement();

        using ( new AssertionScope() )
        {
            result.Sql.Should().Be( "SELECT * FROM bar" );
            result.Parameters.Should().BeSequentiallyEqualTo( result.Parameters );
        }
    }

    [Fact]
    public void Snapshot_ToQuery_ShouldCreateRawQueryNode()
    {
        var context = SqlNodeInterpreterContext.Create();
        context.AddParameter( "a", TypeNullability.Create<int>(), null );
        context.Sql.Append( "SELECT * FROM bar" );
        var sut = context.ToSnapshot();

        var result = sut.ToQuery();

        using ( new AssertionScope() )
        {
            result.Sql.Should().Be( "SELECT * FROM bar" );
            result.Parameters.Should().BeSequentiallyEqualTo( result.Parameters );
        }
    }
}
