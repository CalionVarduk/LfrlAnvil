using System;
using System.Linq;

namespace LfrlAnvil.Dependencies.Exceptions;

public class DependencyContainerBuildAggregateException : AggregateException
{
    public DependencyContainerBuildAggregateException(Chain<DependencyContainerBuildMessages> messages)
        : base(
            Resources.ContainerIsNotConfiguredCorrectly,
            messages.SelectMany(
                m => m.Errors.Select( e => new DependencyTypeConfigurationException( m.DependencyType, m.ImplementorType, e ) ) ) ) { }
}
