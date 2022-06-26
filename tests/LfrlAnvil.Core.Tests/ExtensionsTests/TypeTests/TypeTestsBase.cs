using LfrlAnvil.TestExtensions;

namespace LfrlAnvil.Tests.ExtensionsTests.TypeTests;

public abstract class TypeTestsBase : TestsBase
{
    public interface INotImplemented { }

    public interface IIndirectFromType { }

    public interface IIndirectFromInterface { }

    public interface IDirect : IIndirectFromInterface { }

    public interface IBaseGeneric<out T> : IIndirectFromInterface
    {
        T Value { get; }
    }

    public interface INonGenericInterface : IDirect { }

    public interface IGenericInterface<out T> : IDirect, IBaseGeneric<T> { }

    public interface IMultiGenericClosedInterface : IBaseGeneric<int>, IBaseGeneric<string> { }

    public class NotExtended { }

    public class BaseClass : IIndirectFromType { }

    public class BaseGenericClass<T> : BaseClass, IBaseGeneric<T?>
    {
        T? IBaseGeneric<T?>.Value => default;
    }

    public class NonGenericClass : BaseClass, IDirect { }

    public class GenericClass<T> : BaseGenericClass<T>, IDirect { }

    public class MultiGenericClass<T1, T2> : BaseGenericClass<T1>, IBaseGeneric<T2?>
    {
        T2? IBaseGeneric<T2?>.Value => default;
    }
}
