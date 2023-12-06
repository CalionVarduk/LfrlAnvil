using NSubstitute.Core;

namespace LfrlAnvil.TestExtensions.NSubstitute;

public static class DelegateSubstituteExtensions
{
    public static Func<T1> WithAnyArgs<T1>(this Func<T1> source, Func<CallInfo, T1> returnValueProvider)
    {
        source.Invoke().Returns( returnValueProvider );
        return source;
    }

    public static Func<T1, T2> WithAnyArgs<T1, T2>(this Func<T1, T2> source, Func<CallInfo, T2> returnValueProvider)
    {
        source.Invoke( Arg.Any<T1>() ).Returns( returnValueProvider );
        return source;
    }

    public static Func<T1, T2, T3> WithAnyArgs<T1, T2, T3>(this Func<T1, T2, T3> source, Func<CallInfo, T3> returnValueProvider)
    {
        source.Invoke( Arg.Any<T1>(), Arg.Any<T2>() ).Returns( returnValueProvider );
        return source;
    }

    public static Func<T1, T2, T3, T4> WithAnyArgs<T1, T2, T3, T4>(this Func<T1, T2, T3, T4> source, Func<CallInfo, T4> returnValueProvider)
    {
        source.Invoke( Arg.Any<T1>(), Arg.Any<T2>(), Arg.Any<T3>() ).Returns( returnValueProvider );
        return source;
    }
}
