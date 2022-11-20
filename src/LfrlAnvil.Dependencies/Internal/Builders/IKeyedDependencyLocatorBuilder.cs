namespace LfrlAnvil.Dependencies.Internal.Builders;

internal interface IKeyedDependencyLocatorBuilder
{
    Chain<DependencyContainerBuildMessages> BuildKeyed(DependencyLocatorBuilderParams @params);
}
