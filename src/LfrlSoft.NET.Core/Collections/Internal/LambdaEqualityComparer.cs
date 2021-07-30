using System;
using System.Collections.Generic;
using LfrlSoft.NET.Core.Internal;

namespace LfrlSoft.NET.Core.Collections.Internal
{
    internal sealed class LambdaEqualityComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> _equals;
        private readonly Func<T, int> _getHashCode;

        internal LambdaEqualityComparer(Func<T, T, bool> equals)
        {
            _equals = equals;
            _getHashCode = Generic<T>.CreateHashCode;
        }

        internal LambdaEqualityComparer(Func<T, T, bool> equals, Func<T, int> getHashCode)
        {
            _equals = equals;
            _getHashCode = getHashCode;
        }

        public bool Equals(T x, T y)
        {
            return _equals( x, y );
        }

        public int GetHashCode(T obj)
        {
            return _getHashCode( obj );
        }
    }
}
