using System;
using System.Collections;

namespace LfrlAnvil.Reactive.State.Events;

public interface ICollectionVariableElementSnapshot
{
    Type ElementType { get; }
    Type ValidationResultType { get; }
    object Element { get; }
    CollectionVariableElementState PreviousState { get; }
    CollectionVariableElementState NewState { get; }
    IEnumerable PreviousErrors { get; }
    IEnumerable NewErrors { get; }
    IEnumerable PreviousWarnings { get; }
    IEnumerable NewWarnings { get; }
}

public interface ICollectionVariableElementSnapshot<out TElement> : ICollectionVariableElementSnapshot
{
    new TElement Element { get; }
}
