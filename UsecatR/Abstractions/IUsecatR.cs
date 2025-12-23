namespace UsecatR.Abstractions;

public interface IUsecatR
{
    Task<TResult> ExecuteAsync<TResult>(
        IUseCaseRequest<TResult> request,
        CancellationToken ct = default);
}