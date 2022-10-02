namespace LfrlAnvil.Reactive.State.Events;

public interface ICollectionVariableElementValidationSnapshot<TValidationResult> : ICollectionVariableElementSnapshot
{
    new Chain<TValidationResult> PreviousErrors { get; }
    new Chain<TValidationResult> NewErrors { get; }
    new Chain<TValidationResult> PreviousWarnings { get; }
    new Chain<TValidationResult> NewWarnings { get; }
}
