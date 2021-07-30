namespace LfrlSoft.NET.Core.Collections
{
    public interface IRefCounter<TKey> : IReadOnlyRefCounter<TKey>
        where TKey : notnull
    {
        int Increment(TKey key);
        int IncrementBy(TKey key, int count);
        int Decrement(TKey key);
        int DecrementBy(TKey key, int count);
        bool Remove(TKey key);
        void Clear();
    }
}
