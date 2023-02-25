namespace LfrlAnvil.Dependencies.Tests;

public abstract class DependencyTestsBase : TestsBase
{
    public interface IFoo { }

    public interface IDisposableDependency : IDisposable { }

    public interface IBar { }

    public interface IQux { }

    public interface IWithText
    {
        string? Text { get; }
    }

    public interface IBuiltIn
    {
        IDependencyContainer Container { get; }
        IDependencyScope Scope { get; }
    }

    public class Implementor : IFoo, IBar, IQux { }

    public class ChainableFoo : IFoo
    {
        public ChainableFoo(IBar bar)
        {
            Bar = bar;
        }

        public IBar Bar { get; }
    }

    public class ChainableBar : IBar
    {
        public ChainableBar(IQux qux)
        {
            Qux = qux;
        }

        public IQux Qux { get; }
    }

    public class ChainableQux : IQux
    {
        public ChainableQux(IFoo foo)
        {
            Foo = foo;
        }

        public IFoo Foo { get; }
    }

    public class DecoratorWithText : IWithText
    {
        public DecoratorWithText(IWithText text)
        {
            Text = text.Text;
        }

        public string? Text { get; }
    }

    public abstract class AbstractFoo : IFoo { }

    public class ChainableFieldFoo : IFoo
    {
        private readonly Injected<IBar> _bar = default;
        public IBar Bar => _bar.Instance;
    }

    public class ChainableFieldBar : IBar
    {
        private readonly Injected<IQux> _qux = default;
        public IQux Qux => _qux.Instance;
    }

    public class ChainableFieldQux : IQux
    {
        private readonly Injected<IFoo> _foo = default;
        public IFoo Foo => _foo.Instance;
    }

    public class ChainablePropertyFoo : IFoo
    {
        private Injected<IBar> _bar { get; } = default;
        public IBar Bar => _bar.Instance;
    }

    public class ChainablePropertyBar : IBar
    {
        private Injected<IQux> _qux { get; } = default;
        public IQux Qux => _qux.Instance;
    }

    public class ChainablePropertyQux : IQux
    {
        private Injected<IFoo> _foo { get; } = default;
        public IFoo Foo => _foo.Instance;
    }

    public class ComplexFoo : IFoo
    {
        public ComplexFoo(IBar bar, IQux qux, IBar otherBar)
        {
            Bar = bar;
            Qux = qux;
            OtherBar = otherBar;
        }

        public IBar Bar { get; }
        public IQux Qux { get; }
        public IBar OtherBar { get; }
    }

    public class ComplexBar : IBar
    {
        public ComplexBar(IFoo foo)
        {
            Foo = foo;
        }

        public IFoo Foo { get; }
    }

    public class ComplexQux : IQux
    {
        public ComplexQux(IFoo foo, IWithText text)
        {
            Foo = foo;
            Text = text;
        }

        public IFoo Foo { get; }
        public IWithText Text { get; }
    }

    public class ExplicitCtorImplementor : IWithText
    {
        public ExplicitCtorImplementor(string text)
        {
            Text = text;
        }

        public string Text { get; }
    }

    public class DefaultCtorParamImplementor : IWithText
    {
        public DefaultCtorParamImplementor(string text = "foo")
        {
            Text = text;
        }

        public string Text { get; }
    }

    public class OptionalCtorParamImplementor : IWithText
    {
        public OptionalCtorParamImplementor([OptionalDependency] string? text)
        {
            Text = text;
        }

        public string? Text { get; }
    }

    public class FieldImplementor : IWithText
    {
        private readonly Injected<string> _text = default;

        public string Text => _text.Instance;
    }

    public class BackedPropertyImplementor : IWithText
    {
        private Injected<string> _text { get; set; } = default;

        public string Text => _text.Instance;
    }

    public class BackedReadOnlyPropertyImplementor : IWithText
    {
        private Injected<string> _text { get; } = default;

        public string Text => _text.Instance;
    }

    public class CustomPropertyImplementor : IWithText
    {
        private string _value = string.Empty;

        private Injected<string> _text
        {
            get => new Injected<string>( _value );
            set => _value = value.Instance;
        }

        public string Text => _text.Instance;
    }

    public class OptionalFieldImplementor : IWithText
    {
        [OptionalDependency]
        private readonly Injected<string?> _text = default;

        public string? Text => _text.Instance;
    }

    public class OptionalBackedPropertyImplementor : IWithText
    {
        [OptionalDependency]
        private Injected<string?> _text { get; set; } = default;

        public string? Text => _text.Instance;
    }

    public class OptionalBackedReadOnlyPropertyImplementor : IWithText
    {
        [OptionalDependency]
        private Injected<string?> _text { get; } = default;

        public string? Text => _text.Instance;
    }

    public class OptionalCustomPropertyImplementor : IWithText
    {
        private string? _value = default;

        [OptionalDependency]
        private Injected<string?> _text
        {
            get => new Injected<string?>( _value );
            set => _value = value.Instance;
        }

        public string? Text => _text.Instance;
    }

    public class CtorAndRefMemberImplementor : IWithText
    {
        private readonly int _param;
        private readonly Injected<string> _member = default;

        public CtorAndRefMemberImplementor(int value)
        {
            _param = value;
        }

        public string Text => $"{_member.Instance}{_param}";
    }

    public class CtorAndValueMemberImplementor : IWithText
    {
        private readonly byte? _param;
        private readonly Injected<int> _member = default;

        public CtorAndValueMemberImplementor(byte? value)
        {
            _param = value;
        }

        public string Text => $"{_member.Instance}{_param}";
    }

    public class BuiltInCtorParamImplementor : IBuiltIn
    {
        public BuiltInCtorParamImplementor(IDependencyContainer container, IDependencyScope scope)
        {
            Container = container;
            Scope = scope;
        }

        public IDependencyContainer Container { get; }
        public IDependencyScope Scope { get; }
    }

    public class BuiltInCtorMemberImplementor : IBuiltIn
    {
        private readonly Injected<IDependencyContainer> _container = default;
        private readonly Injected<IDependencyScope> _scope = default;

        public IDependencyContainer Container => _container.Instance;
        public IDependencyScope Scope => _scope.Instance;
    }
}
