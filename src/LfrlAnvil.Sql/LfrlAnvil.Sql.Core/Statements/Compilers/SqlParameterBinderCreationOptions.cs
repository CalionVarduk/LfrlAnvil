using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Statements.Compilers;

/// <summary>
/// Represents available options for creating parameter binder expressions through <see cref="ISqlParameterBinderFactory"/>.
/// </summary>
public readonly struct SqlParameterBinderCreationOptions
{
    /// <summary>
    /// Represents default options.
    /// </summary>
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

    /// <summary>
    /// Specifies whether or not to completely ignore source null values.
    /// </summary>
    public bool IgnoreNullValues { get; }

    /// <summary>
    /// Specifies whether or not source collection values should create separate SQL parameters for each element.
    /// <see cref="string"/> and <see cref="byte"/> arrays are excluded.
    /// </summary>
    public bool ReduceCollections { get; }

    /// <summary>
    /// Optional <see cref="SqlNodeInterpreterContext"/> instance used for further parameter validation.
    /// </summary>
    public SqlNodeInterpreterContext? Context { get; }

    /// <summary>
    /// Specifies an optional source type's field or property filter. Members that return <b>false</b> will be ignored.
    /// </summary>
    public Func<MemberInfo, bool>? SourceTypeMemberPredicate { get; }

    /// <summary>
    /// Collection of explicit <see cref="SqlParameterConfiguration"/> instances.
    /// </summary>
    public ReadOnlySpan<SqlParameterConfiguration> ParameterConfigurations =>
        CollectionsMarshal.AsSpan( _parameterConfigurations ).Slice( 0, _configurationCount );

    /// <summary>
    /// Creates a new <see cref="SqlParameterBinderCreationOptions"/> instance with changed <see cref="IgnoreNullValues"/>.
    /// </summary>
    /// <param name="enabled">Value to set. Equal to <b>true</b> by default.</param>
    /// <returns>New <see cref="SqlParameterBinderCreationOptions"/> instance.</returns>
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

    /// <summary>
    /// Creates a new <see cref="SqlParameterBinderCreationOptions"/> instance with changed <see cref="ReduceCollections"/>.
    /// </summary>
    /// <param name="enabled">Value to set. Equal to <b>true</b> by default.</param>
    /// <returns>New <see cref="SqlParameterBinderCreationOptions"/> instance.</returns>
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

    /// <summary>
    /// Creates a new <see cref="SqlParameterBinderCreationOptions"/> instance with changed <see cref="Context"/>.
    /// </summary>
    /// <param name="context">Value to set.</param>
    /// <returns>New <see cref="SqlParameterBinderCreationOptions"/> instance.</returns>
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

    /// <summary>
    /// Creates a new <see cref="SqlParameterBinderCreationOptions"/> instance with changed <see cref="SourceTypeMemberPredicate"/>.
    /// </summary>
    /// <param name="predicate">Value to set.</param>
    /// <returns>New <see cref="SqlParameterBinderCreationOptions"/> instance.</returns>
    [Pure]
    public SqlParameterBinderCreationOptions SetSourceTypeMemberPredicate(Func<MemberInfo, bool>? predicate)
    {
        return new SqlParameterBinderCreationOptions( IgnoreNullValues, ReduceCollections, Context, predicate, _parameterConfigurations );
    }

    /// <summary>
    /// Creates a new <see cref="SqlParameterBinderCreationOptions"/> instance with added <see cref="SqlParameterConfiguration"/> instance.
    /// </summary>
    /// <param name="configuration">Value to add.</param>
    /// <returns>New <see cref="SqlParameterBinderCreationOptions"/> instance.</returns>
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

    /// <summary>
    /// Creates a new <see cref="ParameterConfigurationLookups"/> instance for current <see cref="ParameterConfigurations"/>.
    /// </summary>
    /// <param name="sourceType">Parameter source type.</param>
    /// <returns>New <see cref="ParameterConfigurationLookups"/> instance.</returns>
    /// <remarks>Custom selectors will be ignored when <paramref name="sourceType"/> is null.</remarks>
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

    /// <summary>
    /// Represents lookups of <see cref="SqlParameterConfiguration"/> instances.
    /// </summary>
    /// <param name="MembersByMemberName">
    /// Contains <see cref="SqlParameterConfiguration"/> instances identifiable by <see cref="SqlParameterConfiguration.MemberName"/>.
    /// </param>
    /// <param name="SelectorsByParameterName">
    /// Contains <see cref="SqlParameterConfiguration"/> instances with <see cref="SqlParameterConfiguration.CustomSelector"/>
    /// identifiable by <see cref="SqlParameterConfiguration.TargetParameterName"/>.
    /// </param>
    public readonly record struct ParameterConfigurationLookups(
        Dictionary<string, SqlParameterConfiguration>? MembersByMemberName,
        Dictionary<string, SqlParameterConfiguration>? SelectorsByParameterName
    )
    {
        /// <summary>
        /// Returns an <see cref="SqlParameterConfiguration"/> associated with the given member <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Field or property name.</param>
        /// <returns><see cref="SqlParameterConfiguration"/> associated with the given member <paramref name="name"/>.</returns>
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
