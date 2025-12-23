using Microsoft.Extensions.DependencyInjection;
using UsecatR.Abstractions;

namespace UsecatR.Runtime;

internal sealed class UsecatRBus : IUsecatR
{
    private readonly IServiceProvider _sp;

    public UsecatRBus(IServiceProvider sp) => _sp = sp;

    public Task<TResult> ExecuteAsync<TRequest, TResult>(TRequest request, CancellationToken ct = default)
        where TRequest : IUseCaseRequest<TResult>
    {
        if (request is null) throw new ArgumentNullException(nameof(request));

        var handler = _sp.GetRequiredService<IUseCaseHandler<TRequest, TResult>>();
        var behaviors = _sp.GetServices<IUseCaseBehavior<TRequest, TResult>>().ToList();

        UseCaseHandlerDelegate<TResult> terminal = () => handler.HandleAsync(request, ct);

        var pipeline = behaviors
            .Reverse<IUseCaseBehavior<TRequest, TResult>>()
            .Aggregate(terminal, (next, behavior) => () => behavior.HandleAsync(request, next, ct));

        return pipeline();
    }
}