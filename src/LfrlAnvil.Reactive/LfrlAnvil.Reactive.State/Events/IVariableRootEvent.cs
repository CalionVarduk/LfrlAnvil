﻿using System;

namespace LfrlAnvil.Reactive.State.Events;

public interface IVariableRootEvent : IVariableNodeEvent
{
    Type KeyType { get; }
    new IReadOnlyVariableRoot Variable { get; }
    object NodeKey { get; }
    IVariableNodeEvent SourceEvent { get; }
}

public interface IVariableRootEvent<TKey> : IVariableRootEvent
    where TKey : notnull
{
    new IReadOnlyVariableRoot<TKey> Variable { get; }
    new TKey NodeKey { get; }
}