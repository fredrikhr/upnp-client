using Microsoft.Extensions.Hosting;

namespace System.CommandLine.Hosting;

internal abstract class CliHostedService : BackgroundService
{
    internal const string ConfigurationDirectiveName = "config";
    
    public new Task<int>? ExecuteTask => base.ExecuteTask as Task<int>;

    protected sealed override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return InvokeAsync(stoppingToken);
    }

    protected abstract Task<int> InvokeAsync(CancellationToken cancelToken);
}