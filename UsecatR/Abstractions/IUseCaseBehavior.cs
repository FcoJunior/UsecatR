namespace UsecatR.Abstractions;

public interface IUseCaseBehavior<in TRequest, TResult>
    where TRequest : IUseCaseRequest<TResult>
{
    Task<TResult> HandleAsync(
        TRequest request,
        UseCaseHandlerDelegate<TResult> next,
        CancellationToken ct);
}