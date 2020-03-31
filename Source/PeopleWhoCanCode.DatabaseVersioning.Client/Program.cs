using System;
using CommandLine;
using Serilog;

namespace PeopleWhoCanCode.DatabaseVersioning.Client
{
    public class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(options =>
                {
                    // Bootstrap client.
                    Log.Logger = new LoggerConfiguration().MinimumLevel.Debug()
                                                          .WriteTo.Console()
                                                          .CreateLogger();

                    StructureMapConfigurer.Initialize(options.ConnectionString);

                    // Run versioning service.
                    var versioningService = StructureMapConfigurer.GetVersioningService(options.Provider);

                    try
                    {
                        versioningService.Run(options.ChangeScriptsDirectory);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Versioning service stopped due to an exception being thrown.");
                    }

                    Log.Information("Done!");
                });
        }
    }
}
