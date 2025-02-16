using System.Reflection;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Statements.Compilers;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlParameterBinderCreationOptionsTests : TestsBase
{
    [Fact]
    public void Default_ShouldRepresentDefaultOptions()
    {
        var sut = SqlParameterBinderCreationOptions.Default;

        Assertion.All(
                sut.IgnoreNullValues.TestTrue(),
                sut.ReduceCollections.TestFalse(),
                sut.Context.TestNull(),
                sut.SourceTypeMemberPredicate.TestNull(),
                sut.ParameterConfigurations.ToArray().TestEmpty() )
            .Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void EnableIgnoringOfNullValues_ShouldCreateOptionsWithNewMode(bool enabled)
    {
        var sut = SqlParameterBinderCreationOptions.Default.EnableIgnoringOfNullValues( enabled );

        Assertion.All(
                sut.IgnoreNullValues.TestEquals( enabled ),
                sut.ReduceCollections.TestFalse(),
                sut.Context.TestNull(),
                sut.SourceTypeMemberPredicate.TestNull(),
                sut.ParameterConfigurations.ToArray().TestEmpty() )
            .Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void EnableCollectionReduction_ShouldCreateOptionsWithNewMode(bool enabled)
    {
        var sut = SqlParameterBinderCreationOptions.Default.EnableCollectionReduction( enabled );

        Assertion.All(
                sut.IgnoreNullValues.TestTrue(),
                sut.ReduceCollections.TestEquals( enabled ),
                sut.Context.TestNull(),
                sut.SourceTypeMemberPredicate.TestNull(),
                sut.ParameterConfigurations.ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void SetContext_ShouldCreateOptionsWithNewMode()
    {
        var context = SqlNodeInterpreterContext.Create();
        var sut = SqlParameterBinderCreationOptions.Default.SetContext( context );

        Assertion.All(
                sut.IgnoreNullValues.TestTrue(),
                sut.ReduceCollections.TestFalse(),
                sut.Context.TestRefEquals( context ),
                sut.SourceTypeMemberPredicate.TestNull(),
                sut.ParameterConfigurations.ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void SetSourceTypeMemberPredicate_ShouldCreateOptionsWithNewMode()
    {
        var predicate = Lambda.Of( (MemberInfo member) => member.MemberType == MemberTypes.Property );
        var sut = SqlParameterBinderCreationOptions.Default.SetSourceTypeMemberPredicate( predicate );

        Assertion.All(
                sut.IgnoreNullValues.TestTrue(),
                sut.ReduceCollections.TestFalse(),
                sut.Context.TestNull(),
                sut.SourceTypeMemberPredicate.TestRefEquals( predicate ),
                sut.ParameterConfigurations.ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void With_ShouldCreateOptionsWithAddedParameterConfiguration_WhenCurrentParameterConfigurationsAreEmpty()
    {
        var cfg = SqlParameterConfiguration.IgnoreMember( "foo" );
        var sut = SqlParameterBinderCreationOptions.Default.With( cfg );

        Assertion.All(
                SqlParameterBinderCreationOptions.Default.ParameterConfigurations.ToArray().TestEmpty(),
                sut.IgnoreNullValues.TestTrue(),
                sut.ReduceCollections.TestFalse(),
                sut.Context.TestNull(),
                sut.SourceTypeMemberPredicate.TestNull(),
                sut.ParameterConfigurations.ToArray().TestSequence( [ cfg ] ) )
            .Go();
    }

    [Fact]
    public void With_ShouldCreateOptionsWithAddedParameterConfiguration_WhenCurrentParameterConfigurationsAreNotEmpty()
    {
        var cfg1 = SqlParameterConfiguration.IgnoreMember( "foo" );
        var cfg2 = SqlParameterConfiguration.IgnoreMember( "bar" );
        var prev = SqlParameterBinderCreationOptions.Default.With( cfg1 );
        var sut = prev.With( cfg2 );

        Assertion.All(
                prev.ParameterConfigurations.ToArray().TestSequence( [ cfg1 ] ),
                sut.IgnoreNullValues.TestTrue(),
                sut.ReduceCollections.TestFalse(),
                sut.Context.TestNull(),
                sut.SourceTypeMemberPredicate.TestNull(),
                sut.ParameterConfigurations.ToArray().TestSequence( [ cfg1, cfg2 ] ) )
            .Go();
    }

    [Theory]
    [InlineData( null )]
    [InlineData( typeof( object ) )]
    public void CreateParameterConfigurationLookups_ShouldReturnNullLookups_WhenParameterConfigurationsAreEmpty(Type? sourceType)
    {
        var sut = SqlParameterBinderCreationOptions.Default;
        var result = sut.CreateParameterConfigurationLookups( sourceType );

        Assertion.All(
                result.MembersByMemberName.TestNull(),
                result.SelectorsByParameterName.TestNull() )
            .Go();
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

        Assertion.All(
                result.SelectorsByParameterName.TestNull(),
                result.MembersByMemberName.TestNotNull(
                    members => Assertion.All(
                        members.Keys.TestSetEqual( [ "foo", "bar", "lorem" ] ),
                        members.Values.TestSetEqual( [ cfg1, cfg2, cfg3 ] ) ) ) )
            .Go();
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

        Assertion.All(
                result.SelectorsByParameterName.TestNull(),
                result.MembersByMemberName.TestNotNull(
                    members => Assertion.All(
                        members.Keys.TestSetEqual( [ "foo", "bar" ] ),
                        members.Values.TestSetEqual( [ cfg1, cfg3 ] ) ) ) )
            .Go();
    }

    [Fact]
    public void CreateParameterConfigurationLookups_ShouldReturnCorrectLookups_WhenThereAreCustomSelectorsAndSourceTypeIsNull()
    {
        var cfg = SqlParameterConfiguration.From( "foo", (string s) => s.Length );
        var sut = SqlParameterBinderCreationOptions.Default.With( cfg );

        var result = sut.CreateParameterConfigurationLookups( null );

        Assertion.All(
                result.MembersByMemberName.TestNull(),
                result.SelectorsByParameterName.TestNull() )
            .Go();
    }

    [Fact]
    public void CreateParameterConfigurationLookups_ShouldReturnCorrectLookups_WhenThereAreCustomSelectorsAndSourceTypeIsCompatible()
    {
        var cfg = SqlParameterConfiguration.From( "foo", (object s) => s.ToString() );
        var sut = SqlParameterBinderCreationOptions.Default.With( cfg );

        var result = sut.CreateParameterConfigurationLookups( typeof( string ) );

        Assertion.All(
                result.MembersByMemberName.TestNull(),
                result.SelectorsByParameterName.TestNotNull(
                    selectors => Assertion.All(
                        selectors.Keys.TestSetEqual( [ "foo" ] ),
                        selectors.Values.TestSetEqual( [ cfg ] ) ) ) )
            .Go();
    }

    [Fact]
    public void CreateParameterConfigurationLookups_ShouldReturnCorrectLookups_WhenThereAreCustomSelectorsAndSourceTypeIsNotCompatible()
    {
        var cfg = SqlParameterConfiguration.From( "foo", (string s) => s.Length );
        var sut = SqlParameterBinderCreationOptions.Default.With( cfg );

        var result = sut.CreateParameterConfigurationLookups( typeof( object ) );

        Assertion.All(
                result.MembersByMemberName.TestNull(),
                result.SelectorsByParameterName.TestNull() )
            .Go();
    }

    [Fact]
    public void CreateParameterConfigurationLookups_ShouldReturnCorrectLookups_WhenCustomSelectorTargetParameterNameIsDuplicated()
    {
        var cfg1 = SqlParameterConfiguration.From( "foo", (string s) => s.Length );
        var cfg2 = SqlParameterConfiguration.From( "foo", (object s) => s.ToString() );
        var sut = SqlParameterBinderCreationOptions.Default.With( cfg1 ).With( cfg2 );

        var result = sut.CreateParameterConfigurationLookups( typeof( string ) );

        Assertion.All(
                result.MembersByMemberName.TestNull(),
                result.SelectorsByParameterName.TestNotNull(),
                result.SelectorsByParameterName.TestNotNull(
                    selectors => Assertion.All(
                        selectors.Keys.TestSetEqual( [ "foo" ] ),
                        selectors.Values.TestSetEqual( [ cfg2 ] ) ) ) )
            .Go();
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

        Assertion.All(
                result.MembersByMemberName.TestNotNull(),
                result.MembersByMemberName.TestNotNull(
                    members => Assertion.All(
                        members.Keys.TestSetEqual( [ "foo", "bar" ] ),
                        members.Values.TestSetEqual( [ cfg1, cfg2 ] ) ) ),
                result.SelectorsByParameterName.TestNotNull(
                    selectors => Assertion.All(
                        selectors.Keys.TestSetEqual( [ "foo", "qux" ] ),
                        selectors.Values.TestSetEqual( [ cfg3, cfg4 ] ) ) ) )
            .Go();
    }

    [Fact]
    public void ParameterConfigurationLookups_GetMemberConfiguration_ShouldReturnDefaultConfiguration_WhenLookupsAreNull()
    {
        var opt = SqlParameterBinderCreationOptions.Default;
        var sut = opt.CreateParameterConfigurationLookups( null );

        var result = sut.GetMemberConfiguration( "foo" );

        result.TestEquals( SqlParameterConfiguration.From( "foo", "foo" ) ).Go();
    }

    [Fact]
    public void ParameterConfigurationLookups_GetMemberConfiguration_ShouldReturnExistingConfiguration_WhenSelectorsAreNull()
    {
        var cfg = SqlParameterConfiguration.From( "qux", "foo" );
        var opt = SqlParameterBinderCreationOptions.Default.With( cfg );
        var sut = opt.CreateParameterConfigurationLookups( null );

        var result = sut.GetMemberConfiguration( "foo" );

        result.TestEquals( cfg ).Go();
    }

    [Fact]
    public void
        ParameterConfigurationLookups_GetMemberConfiguration_ShouldReturnDefaultConfiguration_WhenSelectorsAreNullAndMemberDoesNotExist()
    {
        var cfg = SqlParameterConfiguration.From( "qux", "foo" );
        var opt = SqlParameterBinderCreationOptions.Default.With( cfg );
        var sut = opt.CreateParameterConfigurationLookups( null );

        var result = sut.GetMemberConfiguration( "bar" );

        result.TestEquals( SqlParameterConfiguration.From( "bar", "bar" ) ).Go();
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

        result.TestEquals( cfg1 ).Go();
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

        result.TestEquals( cfg1 ).Go();
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

        result.TestEquals( SqlParameterConfiguration.IgnoreMember( "foo" ) ).Go();
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

        result.TestEquals( SqlParameterConfiguration.IgnoreMember( "foo" ) ).Go();
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

        result.TestEquals( SqlParameterConfiguration.From( "qux", "qux" ) ).Go();
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

        result.TestEquals( cfg1 ).Go();
    }
}
