using System.Threading.Tasks;
using LfrlAnvil.Diagnostics;

namespace LfrlAnvil.Reactive.Chrono.Internal;

internal readonly record struct ActiveTaskEntry(Task Task, ReactiveTaskInvocationParams Invocation, StopwatchSlim Stopwatch);
