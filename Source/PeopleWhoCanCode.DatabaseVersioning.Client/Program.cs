using System;
using System.Reflection;
using CommandLine;
using log4net;

namespace PeopleWhoCanCode.DatabaseVersioning.Client
{
    public class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            var options = new Options();

            if (Parser.Default.ParseArguments(args, options))
            {
                // Bootstrap client.
                StructureMapConfigurer.Initialize(options.ConnectionString);

                // Run versioning service.
                var versioningService = StructureMapConfigurer.GetVersioningService(options.Provider);

                try
                {
                    versioningService.Run(options.ChangeScriptsDirectory);
                }
                catch (Exception ex)
                {
                    Log.Error("Versioning service stopped due to an exception being thrown.", ex);
                }

                Log.Info("Done!");
            }
        }
    }
}
