using System;

namespace LfrlSoft.NET.Core.Functional
{
    public interface IUnsafe
    {
        bool HasError { get; }
        bool IsOk { get; }
        object GetValue();
        object? GetValueOrDefault();
        Exception GetError();
        Exception? GetErrorOrDefault();
    }
}
