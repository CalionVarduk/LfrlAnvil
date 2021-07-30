﻿namespace LfrlSoft.NET.Core.Collections
{
    public interface IRing<T> : IReadOnlyRing<T>
    {
        new T? this[int index] { get; set; }
        new int StartIndex { get; set; }
        void SetNext(T item);
        void Clear();
    }
}
