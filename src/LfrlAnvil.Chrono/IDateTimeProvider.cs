using System;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Chrono
{
    public interface IDateTimeProvider : IGenerator<DateTime>
    {
        DateTimeKind Kind { get; }
        DateTime GetNow();
    }
}
