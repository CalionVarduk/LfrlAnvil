namespace LfrlAnvil.Requests;

/// <summary>
/// Represents a single generic request.
/// </summary>
/// <typeparam name="TRequest">Request type. Use the "Curiously Recurring Template Pattern" (CRTP) approach.</typeparam>
/// <typeparam name="TResult">Request's result type.</typeparam>
public interface IRequest<TRequest, TResult>
    where TRequest : IRequest<TRequest, TResult> { }
