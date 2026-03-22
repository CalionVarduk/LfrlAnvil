using System.Diagnostics.Contracts;
using System.IO;

namespace LfrlAnvil.MessageBroker.Tests.Helpers;

internal readonly struct StorageScope : IDisposable
{
    internal StorageScope(string path)
    {
        Path = path;
    }

    internal string Path { get; }

    [Pure]
    internal static StorageScope Create()
    {
        return new StorageScope( $"_{Guid.NewGuid():N}" );
    }

    public void Dispose()
    {
        if ( Directory.Exists( Path ) )
            Directory.Delete( Path, recursive: true );
    }
}
