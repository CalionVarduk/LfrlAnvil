using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation;

public interface IValidator<in T, TResult>
{
    [Pure]
    Chain<TResult> Validate(T obj);
}
