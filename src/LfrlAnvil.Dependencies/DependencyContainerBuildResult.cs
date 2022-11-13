using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using LfrlAnvil.Dependencies.Exceptions;

namespace LfrlAnvil.Dependencies;

public readonly struct DependencyContainerBuildResult<TContainer>
    where TContainer : class, IDisposableDependencyContainer
{
    public DependencyContainerBuildResult(TContainer? container, Chain<DependencyContainerBuildMessages> messages)
    {
        Container = container;
        Messages = messages;
    }

    [MemberNotNullWhen( true, nameof( Container ) )]
    public bool IsOk => Container is not null;

    public TContainer? Container { get; }
    public Chain<DependencyContainerBuildMessages> Messages { get; }

    [Pure]
    public TContainer GetContainerOrThrow()
    {
        if ( IsOk )
            return Container;

        throw new DependencyContainerBuildAggregateException( Messages );
    }
}
