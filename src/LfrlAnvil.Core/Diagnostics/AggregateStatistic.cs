namespace LfrlAnvil.Diagnostics;

public readonly record struct AggregateStatistic<T>(T Min, T Max, T Mean, T Variance, T StandardDeviation, T StandardError);
