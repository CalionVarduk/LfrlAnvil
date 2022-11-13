namespace LfrlAnvil.Dependencies.Tests;

public abstract class DependencyTestsBase : TestsBase
{
    public interface IFoo { }

    public interface IDisposableDependency : IDisposable { }

    public interface IBar { }

    public interface IQux { }

    public class Implementor : IFoo, IBar, IQux { }
}
