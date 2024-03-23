using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.MySql.Internal;

namespace LfrlAnvil.MySql;

public readonly struct MySqlNodeInterpreterOptions
{
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

    public MySqlColumnTypeDefinitionProvider? TypeDefinitions { get; }
    public string? CommonSchemaName { get; }
    public int? IndexPrefixLength { get; }
    public bool IsFullJoinParsingEnabled { get; }
    public bool IsIndexFilterParsingEnabled { get; }
    public bool AreTemporaryViewsForbidden { get; }
    public string? UpsertSourceAlias { get; }

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
