﻿using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Internal;

namespace LfrlAnvil;

public static class Ref
{
    [Pure]
    public static Ref<T> Create<T>(T value)
        where T : struct
    {
        return new Ref<T>( value );
    }

    [Pure]
    public static Type? GetUnderlyingType(Type? type)
    {
        var result = UnderlyingType.GetForType( type, typeof( Ref<> ) );
        return result.Length == 0 ? null : result[0];
    }
}
