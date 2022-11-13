namespace LfrlAnvil.Dependencies;

public enum DependencyImplementorDisposalStrategyType : byte
{
    UseDisposableInterface = 0,
    UseCallback = 1,
    RenounceOwnership = 2
}
