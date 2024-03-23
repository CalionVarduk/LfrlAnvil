using LfrlAnvil.Functional;

namespace LfrlAnvil.MySql.Tests;

public class MySqlNodeInterpreterOptionsTests : TestsBase
{
    [Fact]
    public void Default_ShouldReturnCorrectOptions()
    {
        var sut = MySqlNodeInterpreterOptions.Default;

        using ( new AssertionScope() )
        {
            sut.TypeDefinitions.Should().BeNull();
            sut.CommonSchemaName.Should().BeNull();
            sut.IndexPrefixLength.Should().Be( 500 );
            sut.IsFullJoinParsingEnabled.Should().BeFalse();
            sut.IsIndexFilterParsingEnabled.Should().BeFalse();
            sut.AreTemporaryViewsForbidden.Should().BeFalse();
            sut.UpsertSourceAlias.Should().BeNull();
        }
    }

    [Fact]
    public void SetTypeDefinitions_ShouldReturnCorrectResult()
    {
        var typeDefinitions = new MySqlColumnTypeDefinitionProviderBuilder().Build();
        var sut = MySqlNodeInterpreterOptions.Default;
        var result = sut.SetTypeDefinitions( typeDefinitions );

        using ( new AssertionScope() )
        {
            result.TypeDefinitions.Should().BeSameAs( typeDefinitions );
            result.CommonSchemaName.Should().BeNull();
            result.IndexPrefixLength.Should().Be( 500 );
            result.IsFullJoinParsingEnabled.Should().BeFalse();
            result.IsIndexFilterParsingEnabled.Should().BeFalse();
            result.AreTemporaryViewsForbidden.Should().BeFalse();
            result.UpsertSourceAlias.Should().BeNull();
        }
    }

    [Fact]
    public void SetTypeDefinitions_WithNull_ShouldReturnCorrectResult()
    {
        var sut = MySqlNodeInterpreterOptions.Default;
        var result = sut.SetTypeDefinitions( null );

        using ( new AssertionScope() )
        {
            result.TypeDefinitions.Should().BeNull();
            result.CommonSchemaName.Should().BeNull();
            result.IndexPrefixLength.Should().Be( 500 );
            result.IsFullJoinParsingEnabled.Should().BeFalse();
            result.IsIndexFilterParsingEnabled.Should().BeFalse();
            result.AreTemporaryViewsForbidden.Should().BeFalse();
            result.UpsertSourceAlias.Should().BeNull();
        }
    }

    [Fact]
    public void SetCommonSchemaName_ShouldReturnCorrectResult()
    {
        var sut = MySqlNodeInterpreterOptions.Default;
        var result = sut.SetCommonSchemaName( "foo" );

        using ( new AssertionScope() )
        {
            result.TypeDefinitions.Should().BeNull();
            result.CommonSchemaName.Should().Be( "foo" );
            result.IndexPrefixLength.Should().Be( 500 );
            result.IsFullJoinParsingEnabled.Should().BeFalse();
            result.IsIndexFilterParsingEnabled.Should().BeFalse();
            result.AreTemporaryViewsForbidden.Should().BeFalse();
            result.UpsertSourceAlias.Should().BeNull();
        }
    }

    [Fact]
    public void SetCommonSchemaName_WithNull_ShouldReturnCorrectResult()
    {
        var sut = MySqlNodeInterpreterOptions.Default;
        var result = sut.SetCommonSchemaName( null );

        using ( new AssertionScope() )
        {
            result.TypeDefinitions.Should().BeNull();
            result.CommonSchemaName.Should().BeNull();
            result.IndexPrefixLength.Should().Be( 500 );
            result.IsFullJoinParsingEnabled.Should().BeFalse();
            result.IsIndexFilterParsingEnabled.Should().BeFalse();
            result.AreTemporaryViewsForbidden.Should().BeFalse();
            result.UpsertSourceAlias.Should().BeNull();
        }
    }

    [Fact]
    public void EnableIndexPrefixes_ShouldReturnCorrectResult()
    {
        var sut = MySqlNodeInterpreterOptions.Default;
        var result = sut.EnableIndexPrefixes( 400 );

        using ( new AssertionScope() )
        {
            result.TypeDefinitions.Should().BeNull();
            result.CommonSchemaName.Should().BeNull();
            result.IndexPrefixLength.Should().Be( 400 );
            result.IsFullJoinParsingEnabled.Should().BeFalse();
            result.IsIndexFilterParsingEnabled.Should().BeFalse();
            result.AreTemporaryViewsForbidden.Should().BeFalse();
            result.UpsertSourceAlias.Should().BeNull();
        }
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void EnableIndexPrefixes_ShouldThrowArgumentOutOfRangeException_WhenLengthIsLessThanOne(int length)
    {
        var sut = MySqlNodeInterpreterOptions.Default;
        var action = Lambda.Of( () => sut.EnableIndexPrefixes( length ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void DisableIndexPrefixes_ShouldReturnCorrectResult()
    {
        var sut = MySqlNodeInterpreterOptions.Default;
        var result = sut.DisableIndexPrefixes();

        using ( new AssertionScope() )
        {
            result.TypeDefinitions.Should().BeNull();
            result.CommonSchemaName.Should().BeNull();
            result.IndexPrefixLength.Should().BeNull();
            result.IsFullJoinParsingEnabled.Should().BeFalse();
            result.IsIndexFilterParsingEnabled.Should().BeFalse();
            result.AreTemporaryViewsForbidden.Should().BeFalse();
            result.UpsertSourceAlias.Should().BeNull();
        }
    }

    [Theory]
    [InlineData( false )]
    [InlineData( true )]
    public void EnableFullJoinParsing_ShouldReturnCorrectResult(bool enabled)
    {
        var sut = MySqlNodeInterpreterOptions.Default;
        var result = sut.EnableFullJoinParsing( enabled );

        using ( new AssertionScope() )
        {
            result.TypeDefinitions.Should().BeNull();
            result.CommonSchemaName.Should().BeNull();
            result.IndexPrefixLength.Should().Be( 500 );
            result.IsFullJoinParsingEnabled.Should().Be( enabled );
            result.IsIndexFilterParsingEnabled.Should().BeFalse();
            result.AreTemporaryViewsForbidden.Should().BeFalse();
            result.UpsertSourceAlias.Should().BeNull();
        }
    }

    [Theory]
    [InlineData( false )]
    [InlineData( true )]
    public void EnableIndexFilterParsing_ShouldReturnCorrectResult(bool enabled)
    {
        var sut = MySqlNodeInterpreterOptions.Default;
        var result = sut.EnableIndexFilterParsing( enabled );

        using ( new AssertionScope() )
        {
            result.TypeDefinitions.Should().BeNull();
            result.CommonSchemaName.Should().BeNull();
            result.IndexPrefixLength.Should().Be( 500 );
            result.IsFullJoinParsingEnabled.Should().BeFalse();
            result.IsIndexFilterParsingEnabled.Should().Be( enabled );
            result.AreTemporaryViewsForbidden.Should().BeFalse();
            result.UpsertSourceAlias.Should().BeNull();
        }
    }

    [Theory]
    [InlineData( false )]
    [InlineData( true )]
    public void ForbidTemporaryViews_ShouldReturnCorrectResult(bool enabled)
    {
        var sut = MySqlNodeInterpreterOptions.Default;
        var result = sut.ForbidTemporaryViews( enabled );

        using ( new AssertionScope() )
        {
            result.TypeDefinitions.Should().BeNull();
            result.CommonSchemaName.Should().BeNull();
            result.IndexPrefixLength.Should().Be( 500 );
            result.IsFullJoinParsingEnabled.Should().BeFalse();
            result.IsIndexFilterParsingEnabled.Should().BeFalse();
            result.AreTemporaryViewsForbidden.Should().Be( enabled );
            result.UpsertSourceAlias.Should().BeNull();
        }
    }

    [Theory]
    [InlineData( "" )]
    [InlineData( "foo" )]
    [InlineData( null )]
    public void SetUpdateSourceAlias_ShouldReturnCorrectResult(string? alias)
    {
        var sut = MySqlNodeInterpreterOptions.Default;
        var result = sut.SetUpdateSourceAlias( alias );

        using ( new AssertionScope() )
        {
            result.TypeDefinitions.Should().BeNull();
            result.CommonSchemaName.Should().BeNull();
            result.IndexPrefixLength.Should().Be( 500 );
            result.IsFullJoinParsingEnabled.Should().BeFalse();
            result.IsIndexFilterParsingEnabled.Should().BeFalse();
            result.AreTemporaryViewsForbidden.Should().BeFalse();
            result.UpsertSourceAlias.Should().BeSameAs( alias );
        }
    }
}
