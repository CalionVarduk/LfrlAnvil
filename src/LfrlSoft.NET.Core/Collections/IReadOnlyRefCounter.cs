using System.Collections.Generic;

namespace LfrlSoft.NET.Core.Collections
{
    public interface IReadOnlyRefCounter<TKey> : IReadOnlyDictionary<TKey, int>
        where TKey : notnull { }
}
