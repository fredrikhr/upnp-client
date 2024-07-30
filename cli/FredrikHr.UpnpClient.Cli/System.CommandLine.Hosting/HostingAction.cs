using System.CommandLine.Invocation;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace System.CommandLine.Hosting;

internal class HostingAction<THostedService> : AsynchronousCliAction
    where THostedService : CliHostedService
{
    private readonly Action<IHostBuilder>? configureHost;

    public HostingAction(Action<IHostBuilder>? configureHost = null) : base()
    {
        this.configureHost = configureHost;
    }

    public override async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        var args = parseResult.UnmatchedTokens?.ToArray();
        var hostBuilder = Host.CreateDefaultBuilder(args);
        hostBuilder.Properties[typeof(ParseResult)] = parseResult;
        if (parseResult.Configuration.RootCommand is CliRootCommand root
            && root.Directives.FirstOrDefault(d => d.Name == CliHostedService.ConfigurationDirectiveName) is { } directive
            && parseResult.GetResult(directive) is { } directiveResult)
        {
            hostBuilder.ConfigureHostConfiguration(config =>
            {
                config.AddInMemoryCollection(directiveResult.Values.Select(s =>
                {
                    var span = s.AsSpan();
                    Span<Range> partRanges = [default, default];
                    int count = span.Split(partRanges, '=');
                    string key = span[partRanges[0]].ToString();
                    string? value = count > 0 ? span[partRanges[1]].ToString() : null;
                    return new KeyValuePair<string, string?>(key, value);
                }));
            });
        }
        hostBuilder.ConfigureServices(services =>
        {
            services.AddSingleton(parseResult);
            services.AddHostedService<THostedService>();
        });
        configureHost?.Invoke(hostBuilder);

        using var host = hostBuilder.Build();
        await host.StartAsync(cancellationToken)
            .ConfigureAwait(continueOnCapturedContext: false);
        var executeTask = host.Services.GetServices<IHostedService>()
            .OfType<THostedService>().FirstOrDefault()?
            .ExecuteTask ?? Task.FromResult<int>(default);
        await host.WaitForShutdownAsync(cancellationToken)
            .ConfigureAwait(continueOnCapturedContext: false);
        return await executeTask
            .ConfigureAwait(continueOnCapturedContext: false);
    }
}