namespace UsecatR.Abstractions;

public interface IUseCaseHandler<in TRequest, TResult>
    where TRequest : IUseCaseRequest<TResult>
{
    Task<TResult> HandleAsync(TRequest request, CancellationToken ct);
}