using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.PostgreSql;

public readonly struct PostgreSqlNodeInterpreterOptions
{
    public static readonly PostgreSqlNodeInterpreterOptions Default = new PostgreSqlNodeInterpreterOptions();

    private PostgreSqlNodeInterpreterOptions(
        PostgreSqlColumnTypeDefinitionProvider? typeDefinitions,
        bool isVirtualGeneratedColumnStorageParsingEnabled)
    {
        TypeDefinitions = typeDefinitions;
        IsVirtualGeneratedColumnStorageParsingEnabled = isVirtualGeneratedColumnStorageParsingEnabled;
    }

    public PostgreSqlColumnTypeDefinitionProvider? TypeDefinitions { get; }
    public bool IsVirtualGeneratedColumnStorageParsingEnabled { get; }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public PostgreSqlNodeInterpreterOptions SetTypeDefinitions(PostgreSqlColumnTypeDefinitionProvider? typeDefinitions)
    {
        return new PostgreSqlNodeInterpreterOptions( typeDefinitions, IsVirtualGeneratedColumnStorageParsingEnabled );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public PostgreSqlNodeInterpreterOptions EnableVirtualGeneratedColumnStorageParsing(bool enabled = true)
    {
        return new PostgreSqlNodeInterpreterOptions( TypeDefinitions, enabled );
    }
}
