using System.Collections.Generic;
using System.Linq;

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

    public interface IGenericFoo<T> { }

    public interface IGenericBar<T> { }

    public interface IGenericQux<T> { }

    public interface IGenericMulti<T1, T2, T3> { }

    public interface IBuiltIn
    {
        IDependencyContainer Container { get; }
        IDependencyScope Scope { get; }
        IDependencyScopeFactory ScopeFactory { get; }
    }

    public class DisposableDependency : IDisposableDependency
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    public class Implementor : IFoo, IBar, IQux { }

    public class GenericImplementor<T> : IGenericFoo<T>, IGenericBar<T>, IGenericQux<T> { }

    public class ChainableGenericFoo<T> : IGenericFoo<T>
    {
        public ChainableGenericFoo(IGenericBar<T> bar)
        {
            Bar = bar;
        }

        public IGenericBar<T> Bar { get; }
    }

    public class ChainableGenericBar<T> : IGenericBar<T>
    {
        public ChainableGenericBar(IGenericQux<T> qux)
        {
            Qux = qux;
        }

        public IGenericQux<T> Qux { get; }
    }

    public class ChainableGenericQux<T> : IGenericQux<T>
    {
        public ChainableGenericQux(IGenericFoo<T> foo)
        {
            Foo = foo;
        }

        public IGenericFoo<T> Foo { get; }
    }

    public class ChainableFieldGenericFoo<T> : IGenericFoo<T>
    {
        private readonly Injected<IGenericBar<T>> _bar = default;
        public IGenericBar<T> Bar => _bar.Instance;
    }

    public class ChainableFieldGenericBar<T> : IGenericBar<T>
    {
        private readonly Injected<IGenericQux<T>> _qux = default;
        public IGenericQux<T> Qux => _qux.Instance;
    }

    public class ChainableFieldGenericQux<T> : IGenericQux<T>
    {
        private readonly Injected<IGenericFoo<T>> _foo = default;
        public IGenericFoo<T> Foo => _foo.Instance;
    }

    public class ChainablePropertyGenericFoo<T> : IGenericFoo<T>
    {
        private Injected<IGenericBar<T>> _bar { get; } = default;
        public IGenericBar<T> Bar => _bar.Instance;
    }

    public class ChainablePropertyGenericBar<T> : IGenericBar<T>
    {
        private Injected<IGenericQux<T>> _qux { get; } = default;
        public IGenericQux<T> Qux => _qux.Instance;
    }

    public class ChainablePropertyGenericQux<T> : IGenericQux<T>
    {
        private Injected<IGenericFoo<T>> _foo { get; } = default;
        public IGenericFoo<T> Foo => _foo.Instance;
    }

    public class DecoratedGenericFoo<T> : IGenericFoo<T>
    {
        public DecoratedGenericFoo(IGenericFoo<T> inner)
        {
            Inner = inner;
        }

        public IGenericFoo<T> Inner { get; }
    }

    public abstract class GenericAbstractFoo<T> : IGenericFoo<T> { }

    public class ExplicitCtorGenericImplementor<T> : IGenericFoo<T>
    {
        public ExplicitCtorGenericImplementor(string text)
        {
            Text = text;
        }

        public string Text { get; }
    }

    public class FieldGenericImplementor<T> : IGenericFoo<T>
    {
        private readonly Injected<string> _text;
        public string Text => _text.Instance;
    }

    public class CtorAndRefMemberGenericImplementor<T> : IGenericFoo<T>
    {
        private readonly int _param;
        private readonly Injected<string> _member = default;

        public CtorAndRefMemberGenericImplementor(int value)
        {
            _param = value;
        }

        public string Text => $"{_member.Instance}{_param}";
    }

    public class CtorAndValueMemberGenericImplementor<T> : IGenericFoo<T>
    {
        private readonly byte? _param;
        private readonly Injected<int> _member = default;

        public CtorAndValueMemberGenericImplementor(byte? value)
        {
            _param = value;
        }

        public string Text => $"{_member.Instance}{_param}";
    }

    public class DefaultCtorParamGenericImplementor<T> : IGenericFoo<T>
    {
        public DefaultCtorParamGenericImplementor(IGenericBar<T>? bar = null)
        {
            Bar = bar;
        }

        public IGenericBar<T>? Bar { get; }
    }

    public class OptionalCtorParamGenericImplementor<T> : IGenericFoo<T>
    {
        public OptionalCtorParamGenericImplementor([OptionalDependency] IGenericBar<T>? bar)
        {
            Bar = bar;
        }

        public IGenericBar<T>? Bar { get; }
    }

    public class OptionalMemberGenericImplementor<T> : IGenericFoo<T>
    {
        [OptionalDependency]
        private readonly Injected<IGenericBar<T>?> _bar;

        public IGenericBar<T>? Bar => _bar.Instance;
    }

    public class GenericFreeFoo<T1, T2> : IGenericFoo<T1> { }

    public class MultiCtorGenericImplementor<T> : IGenericFoo<T>
    {
        public MultiCtorGenericImplementor(IGenericBar<T> bar)
        {
            Bar = bar;
            Qux = null;
        }

        public MultiCtorGenericImplementor([OptionalDependency] IGenericQux<T>? qux)
        {
            Bar = null;
            Qux = qux ?? new GenericImplementor<T>();
        }

        public IGenericBar<T>? Bar { get; }
        public IGenericQux<T>? Qux { get; }
    }

    public class Parameterized<T>
    {
        public Parameterized(T inner)
        {
            Inner = inner;
        }

        public T Inner { get; }
    }

    public class ParameterizedMember<T>
    {
        public Injected<T> Inner { get; }
    }

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
        public BuiltInCtorParamImplementor(IDependencyContainer container, IDependencyScope scope, IDependencyScopeFactory scopeFactory)
        {
            Container = container;
            Scope = scope;
            ScopeFactory = scopeFactory;
        }

        public IDependencyContainer Container { get; }
        public IDependencyScope Scope { get; }
        public IDependencyScopeFactory ScopeFactory { get; }
    }

    public class BuiltInCtorMemberImplementor : IBuiltIn
    {
        private readonly Injected<IDependencyContainer> _container = default;
        private readonly Injected<IDependencyScope> _scope = default;
        private readonly Injected<IDependencyScopeFactory> _scopeFactory = default;

        public IDependencyContainer Container => _container.Instance;
        public IDependencyScope Scope => _scope.Instance;
        public IDependencyScopeFactory ScopeFactory => _scopeFactory.Instance;
    }

    public class MultiCtorImplementor : IFoo
    {
        public MultiCtorImplementor(IBar bar)
        {
            Bar = bar;
            Qux = null;
        }

        public MultiCtorImplementor([OptionalDependency] IQux? qux)
        {
            Bar = null;
            Qux = qux ?? new Implementor();
        }

        public IBar? Bar { get; }
        public IQux? Qux { get; }
    }

    public class SameCtorScoreImplementor : IFoo
    {
        public SameCtorScoreImplementor(IBar bar1, IQux qux1)
        {
            Bar = bar1;
            Qux = qux1;
            Text = null;
        }

        public SameCtorScoreImplementor(IBar bar2, IQux qux2, IWithText text2)
        {
            Bar = bar2;
            Qux = qux2;
            Text = text2;
        }

        public IBar Bar { get; }
        public IQux Qux { get; }
        public IWithText? Text { get; }
    }

    public class RangeFoo : IFoo
    {
        public RangeFoo(IEnumerable<string> texts)
        {
            Texts = texts;
        }

        public IEnumerable<string> Texts { get; }
    }

    public class GenericRangeFoo<T> : IGenericFoo<T>
    {
        public GenericRangeFoo(IEnumerable<string> texts)
        {
            Texts = texts;
        }

        public IEnumerable<string> Texts { get; }
    }

    public class RangeBar : IBar
    {
        private readonly Injected<IEnumerable<string>> _texts = default;
        public IEnumerable<string> Texts => _texts.Instance;
    }

    public class RangeDecorator : IWithText
    {
        public RangeDecorator(IEnumerable<IWithText> range)
        {
            Range = range;
        }

        public IEnumerable<IWithText> Range { get; }
        public string Text => string.Join( '|', Range.Select( e => e.Text ) );
    }

    public class GenericFooRangeDecorator<T> : IGenericFoo<T>
    {
        public GenericFooRangeDecorator(IEnumerable<IGenericFoo<T>> range)
        {
            Range = range;
        }

        public IEnumerable<IGenericFoo<T>> Range { get; }
    }

    public class GenericBarRangeDecorator<T> : IGenericBar<T>
    {
        public GenericBarRangeDecorator(IEnumerable<IGenericBar<T>> range)
        {
            Range = range;
        }

        public IEnumerable<IGenericBar<T>> Range { get; }
    }

    public class ChainableGenericFooRange<T> : IGenericFoo<T>
    {
        public ChainableGenericFooRange(IEnumerable<IGenericBar<T>> bars)
        {
            Bars = bars;
        }

        public IEnumerable<IGenericBar<T>> Bars { get; }
    }

    public class ChainableGenericFooMemberRange<T> : IGenericFoo<T>
    {
        private readonly Injected<IEnumerable<IGenericBar<T>>> _bars;
        public IEnumerable<IGenericBar<T>> Bars => _bars.Instance;
    }

    public class ChainableGenericRange<T> : IGenericBar<T>
    {
        public ChainableGenericRange(IGenericFoo<T> foo)
        {
            Foo = foo;
        }

        public IGenericFoo<T> Foo { get; }
    }

    public class ChainableRange : IWithText
    {
        public ChainableRange(IFoo foo)
        {
            Foo = foo;
        }

        public IFoo Foo { get; }
        public string Text => string.Empty;
    }

    public class TextFoo : IFoo
    {
        public TextFoo(IEnumerable<IWithText> texts)
        {
            Texts = texts;
        }

        public IEnumerable<IWithText> Texts { get; }
    }

    public class NestedMemberBase
    {
        private readonly Injected<IFoo> _foo;

        public IFoo Foo => _foo.Instance;
    }

    public class NestedMember : NestedMemberBase
    {
        private readonly Injected<IBar> _foo;
        public IBar Bar => _foo.Instance;
    }

    public class GenericNestedMemberBase<T>
    {
        private readonly Injected<IGenericFoo<T>> _foo;

        public IGenericFoo<T> Foo => _foo.Instance;
    }

    public class GenericNestedMember<T> : GenericNestedMemberBase<T>
    {
        private readonly Injected<IGenericBar<T>> _foo;
        public IGenericBar<T> Bar => _foo.Instance;
    }
}
