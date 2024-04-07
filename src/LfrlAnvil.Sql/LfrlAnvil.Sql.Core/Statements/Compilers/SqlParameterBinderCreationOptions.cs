using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Statements.Compilers;

public readonly struct SqlParameterBinderCreationOptions
{
    public static readonly SqlParameterBinderCreationOptions Default = new SqlParameterBinderCreationOptions().EnableIgnoringOfNullValues();

    private readonly List<SqlParameterConfiguration>? _parameterConfigurations;
    private readonly int _configurationCount;

    private SqlParameterBinderCreationOptions(
        bool ignoreNullValues,
        bool reduceCollections,
        SqlNodeInterpreterContext? context,
        Func<MemberInfo, bool>? sourceTypeMemberPredicate,
        List<SqlParameterConfiguration>? parameterConfigurations)
    {
        IgnoreNullValues = ignoreNullValues;
        ReduceCollections = reduceCollections;
        Context = context;
        SourceTypeMemberPredicate = sourceTypeMemberPredicate;
        _parameterConfigurations = parameterConfigurations;
        _configurationCount = _parameterConfigurations?.Count ?? 0;
    }

    public bool IgnoreNullValues { get; }
    public bool ReduceCollections { get; }
    public SqlNodeInterpreterContext? Context { get; }
    public Func<MemberInfo, bool>? SourceTypeMemberPredicate { get; }

    public ReadOnlySpan<SqlParameterConfiguration> ParameterConfigurations =>
        CollectionsMarshal.AsSpan( _parameterConfigurations ).Slice( 0, _configurationCount );

    [Pure]
    public SqlParameterBinderCreationOptions EnableIgnoringOfNullValues(bool enabled = true)
    {
        return new SqlParameterBinderCreationOptions(
            enabled,
            ReduceCollections,
            Context,
            SourceTypeMemberPredicate,
            _parameterConfigurations );
    }

    [Pure]
    public SqlParameterBinderCreationOptions EnableCollectionReduction(bool enabled = true)
    {
        return new SqlParameterBinderCreationOptions(
            IgnoreNullValues,
            enabled,
            Context,
            SourceTypeMemberPredicate,
            _parameterConfigurations );
    }

    [Pure]
    public SqlParameterBinderCreationOptions SetContext(SqlNodeInterpreterContext? context)
    {
        return new SqlParameterBinderCreationOptions(
            IgnoreNullValues,
            ReduceCollections,
            context,
            SourceTypeMemberPredicate,
            _parameterConfigurations );
    }

    [Pure]
    public SqlParameterBinderCreationOptions SetSourceTypeMemberPredicate(Func<MemberInfo, bool>? predicate)
    {
        return new SqlParameterBinderCreationOptions( IgnoreNullValues, ReduceCollections, Context, predicate, _parameterConfigurations );
    }

    [Pure]
    public SqlParameterBinderCreationOptions With(SqlParameterConfiguration configuration)
    {
        var configurations = _parameterConfigurations ?? new List<SqlParameterConfiguration>();
        configurations.Add( configuration );
        return new SqlParameterBinderCreationOptions(
            IgnoreNullValues,
            ReduceCollections,
            Context,
            SourceTypeMemberPredicate,
            configurations );
    }

    [Pure]
    public ParameterConfigurationLookups CreateParameterConfigurationLookups(Type? sourceType)
    {
        var configurations = ParameterConfigurations;
        if ( configurations.Length == 0 )
            return new ParameterConfigurationLookups( null, null );

        var members = new Dictionary<string, SqlParameterConfiguration>(
            capacity: configurations.Length,
            comparer: SqlHelpers.NameComparer );

        if ( sourceType is null )
        {
            foreach ( var cfg in configurations )
            {
                if ( cfg.MemberName is not null )
                    members[cfg.MemberName] = cfg;
            }

            return new ParameterConfigurationLookups( members.Count == 0 ? null : members, null );
        }

        var selectors = new Dictionary<string, SqlParameterConfiguration>(
            capacity: configurations.Length,
            comparer: SqlHelpers.NameComparer );

        foreach ( var cfg in configurations )
        {
            if ( cfg.MemberName is not null )
            {
                members[cfg.MemberName] = cfg;
                continue;
            }

            if ( ! sourceType.IsAssignableTo( cfg.CustomSelectorSourceType ) )
                continue;

            Assume.IsNotNull( cfg.TargetParameterName );
            selectors[cfg.TargetParameterName] = cfg;
        }

        return new ParameterConfigurationLookups( members.Count == 0 ? null : members, selectors.Count == 0 ? null : selectors );
    }

    public readonly record struct ParameterConfigurationLookups(
        Dictionary<string, SqlParameterConfiguration>? MembersByMemberName,
        Dictionary<string, SqlParameterConfiguration>? SelectorsByParameterName
    )
    {
        [Pure]
        public SqlParameterConfiguration GetMemberConfiguration(string name)
        {
            return SelectorsByParameterName is null
                ? GetMemberConfigurationWithoutSelectors( name )
                : GetMemberConfigurationWithSelectors( name );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private SqlParameterConfiguration GetMemberConfigurationWithoutSelectors(string name)
        {
            return MembersByMemberName is null || ! MembersByMemberName.TryGetValue( name, out var cfg )
                ? SqlParameterConfiguration.From( name, name )
                : cfg;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private SqlParameterConfiguration GetMemberConfigurationWithSelectors(string name)
        {
            Assume.IsNotNull( SelectorsByParameterName );
            var cfg = GetMemberConfigurationWithoutSelectors( name );

            Assume.IsNotNull( cfg.MemberName );
            return ! cfg.IsIgnored && SelectorsByParameterName.ContainsKey( cfg.TargetParameterName )
                ? SqlParameterConfiguration.IgnoreMember( cfg.MemberName )
                : cfg;
        }
    }
}
