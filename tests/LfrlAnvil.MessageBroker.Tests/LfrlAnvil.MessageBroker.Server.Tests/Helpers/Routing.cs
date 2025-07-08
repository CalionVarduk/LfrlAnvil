using System.Diagnostics.Contracts;
using LfrlAnvil.MessageBroker.Server.Internal;

namespace LfrlAnvil.MessageBroker.Server.Tests.Helpers;

internal readonly struct Routing
{
    internal readonly int Id;
    internal readonly EncodeableText Name;
    internal readonly bool IsName;

    public Routing(int id, EncodeableText name, bool isName)
    {
        Id = id;
        Name = name;
        IsName = isName;
    }

    internal int Length => IsName ? 2 + Name.ByteCount : 5;

    [Pure]
    internal static Routing FromId(int id)
    {
        return new Routing( id, TextEncoding.Prepare( string.Empty ).GetValueOrThrow(), false );
    }

    [Pure]
    internal static Routing FromName(string name)
    {
        return new Routing( 0, TextEncoding.Prepare( name ).GetValueOrThrow(), true );
    }
}
