namespace LfrlSoft.NET.Common
{
    public sealed class Ref<T>
        where T : struct
    {
        public T Value;

        public Ref()
            : this( default ) { }

        public Ref(T value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"{nameof( Ref )}({Value})";
        }

        public static implicit operator T(Ref<T> obj)
        {
            return obj.Value;
        }

        public static explicit operator Ref<T>(T value)
        {
            return new Ref<T>( value );
        }
    }
}
