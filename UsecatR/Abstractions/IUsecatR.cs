namespace UsecatR.Abstractions;

public interface IUsecatR
{
    Task<TResult> ExecuteAsync<TRequest, TResult>(TRequest request, CancellationToken ct = default)
        where TRequest : IUseCaseRequest<TResult>;
}