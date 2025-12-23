using Microsoft.Extensions.DependencyInjection;
using UsecatR.Abstractions;

namespace UsecatR.Runtime;

internal sealed class UsecatRBus : IUsecatR
{
    private readonly IServiceProvider _sp;

    public UsecatRBus(IServiceProvider sp) => _sp = sp;

    public Task<TResult> ExecuteAsync<TResult>(
        IUseCaseRequest<TResult> request,
        CancellationToken ct = default)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));

        var requestType = request.GetType();

        // Resolve handler: IUseCaseHandler<ConcreteRequest, TResult>
        var handlerContract = typeof(IUseCaseHandler<,>).MakeGenericType(requestType, typeof(TResult));
        var handler = _sp.GetRequiredService(handlerContract);

        var handlerHandleMethod = handlerContract.GetMethod(
            nameof(IUseCaseHandler<IUseCaseRequest<TResult>, TResult>.HandleAsync))!;

        // Resolve behaviors: IEnumerable<IUseCaseBehavior<ConcreteRequest, TResult>>
        var behaviorContract = typeof(IUseCaseBehavior<,>).MakeGenericType(requestType, typeof(TResult));
        var behaviors = _sp.GetServices(behaviorContract).ToList();

        var behaviorHandleMethod = behaviorContract.GetMethod(
            nameof(IUseCaseBehavior<IUseCaseRequest<TResult>, TResult>.HandleAsync))!;

        // Terminal delegate (handler)
        UseCaseHandlerDelegate<TResult> next = () =>
            (Task<TResult>)handlerHandleMethod.Invoke(handler, new object[] { request, ct })!;

        // Pipeline: primeiro registrado = mais externo
        for (int i = behaviors.Count - 1; i >= 0; i--)
        {
            var behavior = behaviors[i];
            var currentNext = next;

            next = () => (Task<TResult>)behaviorHandleMethod.Invoke(
                behavior,
                new object[] { request, currentNext, ct })!;
        }

        return next();
    }
}
