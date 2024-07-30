using System.CommandLine;
using System.CommandLine.Hosting;

var cliRoot = new CliRootCommand()
{
    new EnvironmentVariablesDirective(),
    new CliDirective(CliHostedService.ConfigurationDirectiveName),
};

var cliConfig = new CliConfiguration(cliRoot)
{
    ProcessTerminationTimeout = null, // Use Hosting.ConsoleLifetime
};
return await cliConfig.InvokeAsync(args ?? [])
    .ConfigureAwait(continueOnCapturedContext: false);