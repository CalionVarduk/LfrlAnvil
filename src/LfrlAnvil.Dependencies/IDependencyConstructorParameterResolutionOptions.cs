using System;
using System.Linq.Expressions;

namespace LfrlAnvil.Dependencies;

public interface IDependencyConstructorParameterResolutionOptions
{
    void FromFactory(Expression<Func<IDependencyScope, object>> factory);
    void FromImplementor(Type type, Action<IDependencyImplementorOptions>? configuration = null);
}
