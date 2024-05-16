using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.MySql.Internal;

namespace LfrlAnvil.MySql;

/// <summary>
/// Represents available options for <see cref="MySqlNodeInterpreter"/>.
/// </summary>
public readonly struct MySqlNodeInterpreterOptions
{
    /// <summary>
    /// Represents default options.
    /// </summary>
    public static readonly MySqlNodeInterpreterOptions Default = new MySqlNodeInterpreterOptions()
        .EnableIndexPrefixes( MySqlHelpers.DefaultIndexPrefixLength );

    private MySqlNodeInterpreterOptions(
        MySqlColumnTypeDefinitionProvider? typeDefinitions,
        string? commonSchemaName,
        int? indexPrefixLength,
        bool isFullJoinParsingEnabled,
        bool isIndexFilterParsingEnabled,
        bool areTemporaryViewsForbidden,
        string? upsertSourceAlias)
    {
        TypeDefinitions = typeDefinitions;
        CommonSchemaName = commonSchemaName;
        IndexPrefixLength = indexPrefixLength;
        IsFullJoinParsingEnabled = isFullJoinParsingEnabled;
        IsIndexFilterParsingEnabled = isIndexFilterParsingEnabled;
        AreTemporaryViewsForbidden = areTemporaryViewsForbidden;
        UpsertSourceAlias = upsertSourceAlias;
    }

    /// <summary>
    /// Specifies custom <see cref="MySqlColumnTypeDefinitionProvider"/>.
    /// </summary>
    /// <remarks>
    /// Default <see cref="MySqlColumnTypeDefinitionProvider"/> instance built by <see cref="MySqlColumnTypeDefinitionProviderBuilder"/>
    /// will be used when this is null.
    /// </remarks>
    public MySqlColumnTypeDefinitionProvider? TypeDefinitions { get; }

    /// <summary>
    /// Name of the common schema.
    /// </summary>
    /// <remarks>
    /// This schema will contain common functions and procedures.
    /// </remarks>
    public string? CommonSchemaName { get; }

    /// <summary>
    /// Specifies an index prefix length.
    /// </summary>
    /// <remarks><see cref="Default"/> sets this value to <b>500</b>.</remarks>
    public int? IndexPrefixLength { get; }

    /// <summary>
    /// Specifies whether or not <b>FULL JOIN</b> should be included in SQL statements.
    /// </summary>
    public bool IsFullJoinParsingEnabled { get; }

    /// <summary>
    /// Specifies whether or not index filters should be included in SQL statements.
    /// </summary>
    public bool IsIndexFilterParsingEnabled { get; }

    /// <summary>
    /// Specifies whether or not an exception should be thrown when a temporary view node is visited.
    /// </summary>
    public bool AreTemporaryViewsForbidden { get; }

    /// <summary>
    /// Specifies a custom alias of a value source of <b>UPSERT</b> statements.
    /// </summary>
    /// <remarks>Equal to <b>"new"</b> when not specified.</remarks>
    public string? UpsertSourceAlias { get; }

    /// <summary>
    /// Creates a new <see cref="MySqlNodeInterpreterOptions"/> instance with changed <see cref="TypeDefinitions"/>.
    /// </summary>
    /// <param name="typeDefinitions">Value to set.</param>
    /// <returns>New <see cref="MySqlNodeInterpreterOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MySqlNodeInterpreterOptions SetTypeDefinitions(MySqlColumnTypeDefinitionProvider? typeDefinitions)
    {
        return new MySqlNodeInterpreterOptions(
            typeDefinitions,
            CommonSchemaName,
            IndexPrefixLength,
            IsFullJoinParsingEnabled,
            IsIndexFilterParsingEnabled,
            AreTemporaryViewsForbidden,
            UpsertSourceAlias );
    }

    /// <summary>
    /// Creates a new <see cref="MySqlNodeInterpreterOptions"/> instance with changed <see cref="CommonSchemaName"/>.
    /// </summary>
    /// <param name="name">Value to set.</param>
    /// <returns>New <see cref="MySqlNodeInterpreterOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MySqlNodeInterpreterOptions SetCommonSchemaName(string? name)
    {
        return new MySqlNodeInterpreterOptions(
            TypeDefinitions,
            name,
            IndexPrefixLength,
            IsFullJoinParsingEnabled,
            IsIndexFilterParsingEnabled,
            AreTemporaryViewsForbidden,
            UpsertSourceAlias );
    }

    /// <summary>
    /// Creates a new <see cref="MySqlNodeInterpreterOptions"/> instance with changed <see cref="IndexPrefixLength"/>.
    /// </summary>
    /// <param name="length">Value to set.</param>
    /// <returns>New <see cref="MySqlNodeInterpreterOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MySqlNodeInterpreterOptions EnableIndexPrefixes(int length)
    {
        Ensure.IsGreaterThan( length, 0 );
        return new MySqlNodeInterpreterOptions(
            TypeDefinitions,
            CommonSchemaName,
            length,
            IsFullJoinParsingEnabled,
            IsIndexFilterParsingEnabled,
            AreTemporaryViewsForbidden,
            UpsertSourceAlias );
    }

    /// <summary>
    /// Creates a new <see cref="MySqlNodeInterpreterOptions"/> instance with <see cref="IndexPrefixLength"/> set to null.
    /// </summary>
    /// <returns>New <see cref="MySqlNodeInterpreterOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MySqlNodeInterpreterOptions DisableIndexPrefixes()
    {
        return new MySqlNodeInterpreterOptions(
            TypeDefinitions,
            CommonSchemaName,
            null,
            IsFullJoinParsingEnabled,
            IsIndexFilterParsingEnabled,
            AreTemporaryViewsForbidden,
            UpsertSourceAlias );
    }

    /// <summary>
    /// Creates a new <see cref="MySqlNodeInterpreterOptions"/> instance with changed <see cref="IsFullJoinParsingEnabled"/>.
    /// </summary>
    /// <param name="enabled">Value to set. Equal to <b>true</b> by default.</param>
    /// <returns>New <see cref="MySqlNodeInterpreterOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MySqlNodeInterpreterOptions EnableFullJoinParsing(bool enabled = true)
    {
        return new MySqlNodeInterpreterOptions(
            TypeDefinitions,
            CommonSchemaName,
            IndexPrefixLength,
            enabled,
            IsIndexFilterParsingEnabled,
            AreTemporaryViewsForbidden,
            UpsertSourceAlias );
    }

    /// <summary>
    /// Creates a new <see cref="MySqlNodeInterpreterOptions"/> instance with changed <see cref="IsIndexFilterParsingEnabled"/>.
    /// </summary>
    /// <param name="enabled">Value to set. Equal to <b>true</b> by default.</param>
    /// <returns>New <see cref="MySqlNodeInterpreterOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MySqlNodeInterpreterOptions EnableIndexFilterParsing(bool enabled = true)
    {
        return new MySqlNodeInterpreterOptions(
            TypeDefinitions,
            CommonSchemaName,
            IndexPrefixLength,
            IsFullJoinParsingEnabled,
            enabled,
            AreTemporaryViewsForbidden,
            UpsertSourceAlias );
    }

    /// <summary>
    /// Creates a new <see cref="MySqlNodeInterpreterOptions"/> instance with changed <see cref="AreTemporaryViewsForbidden"/>.
    /// </summary>
    /// <param name="enabled">Value to set. Equal to <b>true</b> by default.</param>
    /// <returns>New <see cref="MySqlNodeInterpreterOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MySqlNodeInterpreterOptions ForbidTemporaryViews(bool enabled = true)
    {
        return new MySqlNodeInterpreterOptions(
            TypeDefinitions,
            CommonSchemaName,
            IndexPrefixLength,
            IsFullJoinParsingEnabled,
            IsIndexFilterParsingEnabled,
            enabled,
            UpsertSourceAlias );
    }

    /// <summary>
    /// Creates a new <see cref="MySqlNodeInterpreterOptions"/> instance with changed <see cref="UpsertSourceAlias"/>.
    /// </summary>
    /// <param name="alias">Value to set.</param>
    /// <returns>New <see cref="MySqlNodeInterpreterOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MySqlNodeInterpreterOptions SetUpdateSourceAlias(string? alias)
    {
        return new MySqlNodeInterpreterOptions(
            TypeDefinitions,
            CommonSchemaName,
            IndexPrefixLength,
            IsFullJoinParsingEnabled,
            IsIndexFilterParsingEnabled,
            AreTemporaryViewsForbidden,
            alias );
    }
}
