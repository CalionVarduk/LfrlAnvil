using LfrlAnvil.Generators;

namespace LfrlAnvil.Chrono
{
    public interface ITimestampProvider : IGenerator<Timestamp>
    {
        Timestamp GetNow();
    }
}
