using Microsoft.Extensions.DependencyInjection;
using UsecatR.Abstractions;
using UsecatR.DependencyInjection;

namespace UsecatR.Tests;

public sealed class UsecatRTests
{
    private sealed record Ping(string Message) : IUseCaseRequest<string>;

    private sealed class PingHandler : IUseCaseHandler<Ping, string>
    {
        public Task<string> HandleAsync(Ping request, CancellationToken ct)
            => Task.FromResult($"pong:{request.Message}");
    }

    private sealed class SpyBehavior<TRequest, TResult> : IUseCaseBehavior<TRequest, TResult>
        where TRequest : IUseCaseRequest<TResult>
    {
        private readonly List<string> _events;
        private readonly string _name;

        public SpyBehavior(List<string> events, string name)
        {
            _events = events;
            _name = name;
        }

        public async Task<TResult> HandleAsync(TRequest request, UseCaseHandlerDelegate<TResult> next,
            CancellationToken ct)
        {
            _events.Add($"{_name}:before");
            var result = await next();
            _events.Add($"{_name}:after");
            return result;
        }
    }

    [Fact]
    public async Task ExecuteAsync_calls_handler()
    {
        var services = new ServiceCollection();

        services.AddUsecatR(typeof(PingHandler).Assembly);

        using var sp = services.BuildServiceProvider();
        using var scope = sp.CreateScope();

        var usecatr = scope.ServiceProvider.GetRequiredService<IUsecatR>();

        var result = await usecatr.ExecuteAsync(new Ping("hi"));

        Assert.Equal("pong:hi", result);
    }

    [Fact]
    public async Task ExecuteAsync_runs_behaviors_in_order()
    {
        var events = new List<string>();
        var services = new ServiceCollection();

        services.AddUsecatR(typeof(PingHandler).Assembly);

        // Ordem importa: primeiro registrado = mais externo
        services.AddScoped<IUseCaseBehavior<Ping, string>>(_ => new SpyBehavior<Ping, string>(events, "A"));
        services.AddScoped<IUseCaseBehavior<Ping, string>>(_ => new SpyBehavior<Ping, string>(events, "B"));

        using var sp = services.BuildServiceProvider();
        using var scope = sp.CreateScope();

        var usecatr = scope.ServiceProvider.GetRequiredService<IUsecatR>();

        var result = await usecatr.ExecuteAsync(new Ping("order"));

        Assert.Equal("pong:order", result);
        Assert.Equal(new[] { "A:before", "B:before", "B:after", "A:after" }, events);
    }

    [Fact]
    public async Task ExecuteAsync_throws_when_request_is_null()
    {
        var services = new ServiceCollection();
        services.AddUsecatR(typeof(PingHandler).Assembly);

        using var sp = services.BuildServiceProvider();
        using var scope = sp.CreateScope();

        var usecatr = scope.ServiceProvider.GetRequiredService<IUsecatR>();

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await usecatr.ExecuteAsync<object>(null!, CancellationToken.None);
        });
    }
}