namespace LfrlSoft.NET.Core.Functional
{
    public sealed class PartialEither<T1>
    {
        public readonly T1 Value;

        internal PartialEither(T1 value)
        {
            Value = value;
        }

        public Either<T1, T2> WithSecond<T2>()
        {
            return new Either<T1, T2>( Value );
        }

        public Either<T2, T1> WithFirst<T2>()
        {
            return new Either<T2, T1>( Value );
        }
    }
}
