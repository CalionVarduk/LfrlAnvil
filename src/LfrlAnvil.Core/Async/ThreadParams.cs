using System.Globalization;
using System.Threading;

namespace LfrlAnvil.Async;

public struct ThreadParams
{
    public CultureInfo? Culture;
    public CultureInfo? UICulture;
    public string? Name;
    public ThreadPriority? Priority;
}
