using System;

namespace LfrlAnvil.Diagnostics;

public readonly record struct BenchmarkStepInfo(
    AggregateStatistic<MemorySize> AllocatedBytes,
    AggregateStatistic<TimeSpan> ElapsedTimeWithOutliers,
    AggregateStatistic<TimeSpan> ElapsedTime,
    int ElapsedTimeOutliers
);
