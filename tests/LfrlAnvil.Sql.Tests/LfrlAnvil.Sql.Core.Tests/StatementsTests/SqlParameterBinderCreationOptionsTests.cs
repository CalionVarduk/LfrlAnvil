using System.Reflection;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Statements.Compilers;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlParameterBinderCreationOptionsTests : TestsBase
{
    [Fact]
    public void Default_ShouldRepresentDefaultOptions()
    {
        var sut = SqlParameterBinderCreationOptions.Default;

        using ( new AssertionScope() )
        {
            sut.IgnoreNullValues.Should().BeTrue();
            sut.ReduceCollections.Should().BeFalse();
            sut.Context.Should().BeNull();
            sut.SourceTypeMemberPredicate.Should().BeNull();
            sut.ParameterConfigurations.ToArray().Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void EnableIgnoringOfNullValues_ShouldCreateOptionsWithNewMode(bool enabled)
    {
        var sut = SqlParameterBinderCreationOptions.Default.EnableIgnoringOfNullValues( enabled );

        using ( new AssertionScope() )
        {
            sut.IgnoreNullValues.Should().Be( enabled );
            sut.ReduceCollections.Should().BeFalse();
            sut.Context.Should().BeNull();
            sut.SourceTypeMemberPredicate.Should().BeNull();
            sut.ParameterConfigurations.ToArray().Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void EnableCollectionReduction_ShouldCreateOptionsWithNewMode(bool enabled)
    {
        var sut = SqlParameterBinderCreationOptions.Default.EnableCollectionReduction( enabled );

        using ( new AssertionScope() )
        {
            sut.IgnoreNullValues.Should().BeTrue();
            sut.ReduceCollections.Should().Be( enabled );
            sut.Context.Should().BeNull();
            sut.SourceTypeMemberPredicate.Should().BeNull();
            sut.ParameterConfigurations.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void SetContext_ShouldCreateOptionsWithNewMode()
    {
        var context = SqlNodeInterpreterContext.Create();
        var sut = SqlParameterBinderCreationOptions.Default.SetContext( context );

        using ( new AssertionScope() )
        {
            sut.IgnoreNullValues.Should().BeTrue();
            sut.ReduceCollections.Should().BeFalse();
            sut.Context.Should().BeSameAs( context );
            sut.SourceTypeMemberPredicate.Should().BeNull();
            sut.ParameterConfigurations.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void SetSourceTypeMemberPredicate_ShouldCreateOptionsWithNewMode()
    {
        var predicate = Lambda.Of( (MemberInfo member) => member.MemberType == MemberTypes.Property );
        var sut = SqlParameterBinderCreationOptions.Default.SetSourceTypeMemberPredicate( predicate );

        using ( new AssertionScope() )
        {
            sut.IgnoreNullValues.Should().BeTrue();
            sut.ReduceCollections.Should().BeFalse();
            sut.Context.Should().BeNull();
            sut.SourceTypeMemberPredicate.Should().BeSameAs( predicate );
            sut.ParameterConfigurations.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void With_ShouldCreateOptionsWithAddedParameterConfiguration_WhenCurrentParameterConfigurationsAreEmpty()
    {
        var cfg = SqlParameterConfiguration.IgnoreMember( "foo" );
        var sut = SqlParameterBinderCreationOptions.Default.With( cfg );

        using ( new AssertionScope() )
        {
            SqlParameterBinderCreationOptions.Default.ParameterConfigurations.ToArray().Should().BeEmpty();
            sut.IgnoreNullValues.Should().BeTrue();
            sut.ReduceCollections.Should().BeFalse();
            sut.Context.Should().BeNull();
            sut.SourceTypeMemberPredicate.Should().BeNull();
            sut.ParameterConfigurations.ToArray().Should().BeSequentiallyEqualTo( cfg );
        }
    }

    [Fact]
    public void With_ShouldCreateOptionsWithAddedParameterConfiguration_WhenCurrentParameterConfigurationsAreNotEmpty()
    {
        var cfg1 = SqlParameterConfiguration.IgnoreMember( "foo" );
        var cfg2 = SqlParameterConfiguration.IgnoreMember( "bar" );
        var prev = SqlParameterBinderCreationOptions.Default.With( cfg1 );
        var sut = prev.With( cfg2 );

        using ( new AssertionScope() )
        {
            prev.ParameterConfigurations.ToArray().Should().BeSequentiallyEqualTo( cfg1 );
            sut.IgnoreNullValues.Should().BeTrue();
            sut.ReduceCollections.Should().BeFalse();
            sut.Context.Should().BeNull();
            sut.SourceTypeMemberPredicate.Should().BeNull();
            sut.ParameterConfigurations.ToArray().Should().BeSequentiallyEqualTo( cfg1, cfg2 );
        }
    }

    [Theory]
    [InlineData( null )]
    [InlineData( typeof( object ) )]
    public void CreateParameterConfigurationLookups_ShouldReturnNullLookups_WhenParameterConfigurationsAreEmpty(Type? sourceType)
    {
        var sut = SqlParameterBinderCreationOptions.Default;
        var result = sut.CreateParameterConfigurationLookups( sourceType );

        using ( new AssertionScope() )
        {
            result.MembersByMemberName.Should().BeNull();
            result.SelectorsByParameterName.Should().BeNull();
        }
    }

    [Theory]
    [InlineData( null )]
    [InlineData( typeof( object ) )]
    public void
        CreateParameterConfigurationLookups_ShouldReturnCorrectLookups_WhenEachConfigurationHasDistinctMemberName_AndThereAreNoCustomSelectors(
            Type? sourceType)
    {
        var cfg1 = SqlParameterConfiguration.IgnoreMember( "foo" );
        var cfg2 = SqlParameterConfiguration.IgnoreMemberWhenNull( "bar" );
        var cfg3 = SqlParameterConfiguration.From( "qux", "lorem" );
        var sut = SqlParameterBinderCreationOptions.Default.With( cfg1 ).With( cfg2 ).With( cfg3 );

        var result = sut.CreateParameterConfigurationLookups( sourceType );

        using ( new AssertionScope() )
        {
            result.SelectorsByParameterName.Should().BeNull();
            result.MembersByMemberName.Should().NotBeNull();
            result.MembersByMemberName.Should().HaveCount( 3 );
            (result.MembersByMemberName?.Keys).Should().BeEquivalentTo( "foo", "bar", "lorem" );
            (result.MembersByMemberName?.Values).Should().BeEquivalentTo( cfg1, cfg2, cfg3 );
        }
    }

    [Theory]
    [InlineData( null )]
    [InlineData( typeof( object ) )]
    public void CreateParameterConfigurationLookups_ShouldReturnCorrectLookups_WhenMemberNameIsDuplicated(Type? sourceType)
    {
        var cfg1 = SqlParameterConfiguration.IgnoreMember( "foo" );
        var cfg2 = SqlParameterConfiguration.IgnoreMemberWhenNull( "bar" );
        var cfg3 = SqlParameterConfiguration.From( "qux", "bar" );
        var sut = SqlParameterBinderCreationOptions.Default.With( cfg1 ).With( cfg2 ).With( cfg3 );

        var result = sut.CreateParameterConfigurationLookups( sourceType );

        using ( new AssertionScope() )
        {
            result.SelectorsByParameterName.Should().BeNull();
            result.MembersByMemberName.Should().NotBeNull();
            result.MembersByMemberName.Should().HaveCount( 2 );
            (result.MembersByMemberName?.Keys).Should().BeEquivalentTo( "foo", "bar" );
            (result.MembersByMemberName?.Values).Should().BeEquivalentTo( cfg1, cfg3 );
        }
    }

    [Fact]
    public void CreateParameterConfigurationLookups_ShouldReturnCorrectLookups_WhenThereAreCustomSelectorsAndSourceTypeIsNull()
    {
        var cfg = SqlParameterConfiguration.From( "foo", (string s) => s.Length );
        var sut = SqlParameterBinderCreationOptions.Default.With( cfg );

        var result = sut.CreateParameterConfigurationLookups( null );

        using ( new AssertionScope() )
        {
            result.MembersByMemberName.Should().BeNull();
            result.SelectorsByParameterName.Should().BeNull();
        }
    }

    [Fact]
    public void CreateParameterConfigurationLookups_ShouldReturnCorrectLookups_WhenThereAreCustomSelectorsAndSourceTypeIsCompatible()
    {
        var cfg = SqlParameterConfiguration.From( "foo", (object s) => s.ToString() );
        var sut = SqlParameterBinderCreationOptions.Default.With( cfg );

        var result = sut.CreateParameterConfigurationLookups( typeof( string ) );

        using ( new AssertionScope() )
        {
            result.MembersByMemberName.Should().BeNull();
            result.SelectorsByParameterName.Should().NotBeNull();
            result.SelectorsByParameterName.Should().HaveCount( 1 );
            (result.SelectorsByParameterName?.Keys).Should().BeEquivalentTo( "foo" );
            (result.SelectorsByParameterName?.Values).Should().BeEquivalentTo( cfg );
        }
    }

    [Fact]
    public void CreateParameterConfigurationLookups_ShouldReturnCorrectLookups_WhenThereAreCustomSelectorsAndSourceTypeIsNotCompatible()
    {
        var cfg = SqlParameterConfiguration.From( "foo", (string s) => s.Length );
        var sut = SqlParameterBinderCreationOptions.Default.With( cfg );

        var result = sut.CreateParameterConfigurationLookups( typeof( object ) );

        using ( new AssertionScope() )
        {
            result.MembersByMemberName.Should().BeNull();
            result.SelectorsByParameterName.Should().BeNull();
        }
    }

    [Fact]
    public void CreateParameterConfigurationLookups_ShouldReturnCorrectLookups_WhenCustomSelectorTargetParameterNameIsDuplicated()
    {
        var cfg1 = SqlParameterConfiguration.From( "foo", (string s) => s.Length );
        var cfg2 = SqlParameterConfiguration.From( "foo", (object s) => s.ToString() );
        var sut = SqlParameterBinderCreationOptions.Default.With( cfg1 ).With( cfg2 );

        var result = sut.CreateParameterConfigurationLookups( typeof( string ) );

        using ( new AssertionScope() )
        {
            result.MembersByMemberName.Should().BeNull();
            result.SelectorsByParameterName.Should().NotBeNull();
            result.SelectorsByParameterName.Should().HaveCount( 1 );
            (result.SelectorsByParameterName?.Keys).Should().BeEquivalentTo( "foo" );
            (result.SelectorsByParameterName?.Values).Should().BeEquivalentTo( cfg2 );
        }
    }

    [Fact]
    public void CreateParameterConfigurationLookups_ShouldReturnCorrectLookups_WhenThereAreMembersAndCustomSelectors()
    {
        var cfg1 = SqlParameterConfiguration.From( "qux", "foo" );
        var cfg2 = SqlParameterConfiguration.IgnoreMemberWhenNull( "bar" );
        var cfg3 = SqlParameterConfiguration.From( "foo", (object s) => s.ToString() );
        var cfg4 = SqlParameterConfiguration.From( "qux", (string s) => s.Length );
        var sut = SqlParameterBinderCreationOptions.Default.With( cfg1 ).With( cfg2 ).With( cfg3 ).With( cfg4 );

        var result = sut.CreateParameterConfigurationLookups( typeof( string ) );

        using ( new AssertionScope() )
        {
            result.MembersByMemberName.Should().NotBeNull();
            result.MembersByMemberName.Should().HaveCount( 2 );
            (result.MembersByMemberName?.Keys).Should().BeEquivalentTo( "foo", "bar" );
            (result.MembersByMemberName?.Values).Should().BeEquivalentTo( cfg1, cfg2 );
            result.SelectorsByParameterName.Should().NotBeNull();
            result.SelectorsByParameterName.Should().HaveCount( 2 );
            (result.SelectorsByParameterName?.Keys).Should().BeEquivalentTo( "foo", "qux" );
            (result.SelectorsByParameterName?.Values).Should().BeEquivalentTo( cfg3, cfg4 );
        }
    }

    [Fact]
    public void ParameterConfigurationLookups_GetMemberConfiguration_ShouldReturnDefaultConfiguration_WhenLookupsAreNull()
    {
        var opt = SqlParameterBinderCreationOptions.Default;
        var sut = opt.CreateParameterConfigurationLookups( null );

        var result = sut.GetMemberConfiguration( "foo" );

        result.Should().BeEquivalentTo( SqlParameterConfiguration.From( "foo", "foo" ) );
    }

    [Fact]
    public void ParameterConfigurationLookups_GetMemberConfiguration_ShouldReturnExistingConfiguration_WhenSelectorsAreNull()
    {
        var cfg = SqlParameterConfiguration.From( "qux", "foo" );
        var opt = SqlParameterBinderCreationOptions.Default.With( cfg );
        var sut = opt.CreateParameterConfigurationLookups( null );

        var result = sut.GetMemberConfiguration( "foo" );

        result.Should().BeEquivalentTo( cfg );
    }

    [Fact]
    public void
        ParameterConfigurationLookups_GetMemberConfiguration_ShouldReturnDefaultConfiguration_WhenSelectorsAreNullAndMemberDoesNotExist()
    {
        var cfg = SqlParameterConfiguration.From( "qux", "foo" );
        var opt = SqlParameterBinderCreationOptions.Default.With( cfg );
        var sut = opt.CreateParameterConfigurationLookups( null );

        var result = sut.GetMemberConfiguration( "bar" );

        result.Should().BeEquivalentTo( SqlParameterConfiguration.From( "bar", "bar" ) );
    }

    [Fact]
    public void
        ParameterConfigurationLookups_GetMemberConfiguration_ShouldReturnExistingConfiguration_WhenMemberIsIgnoredAndSelectorExists()
    {
        var cfg1 = SqlParameterConfiguration.IgnoreMember( "foo" );
        var cfg2 = SqlParameterConfiguration.From( "foo", (object s) => s.ToString() );
        var opt = SqlParameterBinderCreationOptions.Default.With( cfg1 ).With( cfg2 );
        var sut = opt.CreateParameterConfigurationLookups( typeof( object ) );

        var result = sut.GetMemberConfiguration( "foo" );

        result.Should().BeEquivalentTo( cfg1 );
    }

    [Fact]
    public void
        ParameterConfigurationLookups_GetMemberConfiguration_ShouldReturnExistingConfiguration_WhenMemberIsRenamedAndSelectorExists()
    {
        var cfg1 = SqlParameterConfiguration.From( "qux", "foo" );
        var cfg2 = SqlParameterConfiguration.From( "foo", (object s) => s.ToString() );
        var opt = SqlParameterBinderCreationOptions.Default.With( cfg1 ).With( cfg2 );
        var sut = opt.CreateParameterConfigurationLookups( typeof( object ) );

        var result = sut.GetMemberConfiguration( "foo" );

        result.Should().BeEquivalentTo( cfg1 );
    }

    [Fact]
    public void
        ParameterConfigurationLookups_GetMemberConfiguration_ShouldReturnIgnoringConfiguration_WhenMemberIsNotIgnoredAndSelectorExists()
    {
        var cfg1 = SqlParameterConfiguration.IgnoreMemberWhenNull( "foo" );
        var cfg2 = SqlParameterConfiguration.From( "foo", (object s) => s.ToString() );
        var opt = SqlParameterBinderCreationOptions.Default.With( cfg1 ).With( cfg2 );
        var sut = opt.CreateParameterConfigurationLookups( typeof( object ) );

        var result = sut.GetMemberConfiguration( "foo" );

        result.Should().BeEquivalentTo( SqlParameterConfiguration.IgnoreMember( "foo" ) );
    }

    [Fact]
    public void
        ParameterConfigurationLookups_GetMemberConfiguration_ShouldReturnIgnoringConfiguration_WhenMemberDoesNotExistAndSelectorExists()
    {
        var cfg1 = SqlParameterConfiguration.IgnoreMemberWhenNull( "bar" );
        var cfg2 = SqlParameterConfiguration.From( "foo", (object s) => s.ToString() );
        var opt = SqlParameterBinderCreationOptions.Default.With( cfg1 ).With( cfg2 );
        var sut = opt.CreateParameterConfigurationLookups( typeof( object ) );

        var result = sut.GetMemberConfiguration( "foo" );

        result.Should().BeEquivalentTo( SqlParameterConfiguration.IgnoreMember( "foo" ) );
    }

    [Fact]
    public void
        ParameterConfigurationLookups_GetMemberConfiguration_ShouldReturnDefaultConfiguration_WhenMemberDoesNotExistAndSelectorDoesNotExist()
    {
        var cfg1 = SqlParameterConfiguration.IgnoreMemberWhenNull( "bar" );
        var cfg2 = SqlParameterConfiguration.From( "foo", (object s) => s.ToString() );
        var opt = SqlParameterBinderCreationOptions.Default.With( cfg1 ).With( cfg2 );
        var sut = opt.CreateParameterConfigurationLookups( typeof( object ) );

        var result = sut.GetMemberConfiguration( "qux" );

        result.Should().BeEquivalentTo( SqlParameterConfiguration.From( "qux", "qux" ) );
    }

    [Fact]
    public void
        ParameterConfigurationLookups_GetMemberConfiguration_ShouldReturnDefaultConfiguration_WhenMemberExistsAndSelectorDoesNotExist()
    {
        var cfg1 = SqlParameterConfiguration.IgnoreMemberWhenNull( "bar" );
        var cfg2 = SqlParameterConfiguration.From( "foo", (object s) => s.ToString() );
        var opt = SqlParameterBinderCreationOptions.Default.With( cfg1 ).With( cfg2 );
        var sut = opt.CreateParameterConfigurationLookups( typeof( object ) );

        var result = sut.GetMemberConfiguration( "bar" );

        result.Should().BeEquivalentTo( cfg1 );
    }
}
