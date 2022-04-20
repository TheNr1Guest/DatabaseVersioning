using CommandLine;
using Serilog;

namespace PeopleWhoCanCode.DatabaseVersioning.Client;

public class Program
{
    private static void Main(string[] args)
    {
        var exitCode = Parser.Default.ParseArguments<Options>(args)
                                     .MapResult(x => RunParsed(x),
                                                x => RunNotParsed());

        Environment.Exit(exitCode);
    }

    private static int RunParsed(Options options)
    {
        // Bootstrap client.
        Log.Logger = new LoggerConfiguration().MinimumLevel.Debug()
                                              .WriteTo.Console()
                                              .CreateLogger();

        LamarConfigurer.Initialize(options.ConnectionString);

        // Run versioning service.
        var versioningService = LamarConfigurer.GetVersioningService(options.Provider);

        try
        {
            versioningService.Run(options.ChangeScriptsDirectory ?? string.Empty);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Versioning service stopped due to an exception being thrown.");

            return -1;
        }

        Log.Information("Done!");

        return 0;
    }

    private static int RunNotParsed()
    {
        return -1;
    }
}
