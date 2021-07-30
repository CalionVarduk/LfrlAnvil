using System.Diagnostics.Contracts;

namespace LfrlSoft.NET.Core
{
    public sealed class Ref<T> : IReadOnlyRef<T>
        where T : struct
    {
        public T Value { get; set; }

        public Ref()
            : this( default ) { }

        public Ref(T value)
        {
            Value = value;
        }

        [Pure]
        public override string ToString()
        {
            return $"{nameof( Ref )}({Value})";
        }

        [Pure]
        public static implicit operator T(Ref<T> obj)
        {
            return obj.Value;
        }

        [Pure]
        public static explicit operator Ref<T>(T value)
        {
            return new Ref<T>( value );
        }
    }
}
