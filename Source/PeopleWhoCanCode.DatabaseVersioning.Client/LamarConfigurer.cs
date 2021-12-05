using System;
using System.Linq;
using Lamar;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PeopleWhoCanCode.DatabaseVersioning.Models;
using Serilog.Extensions.Logging;

namespace PeopleWhoCanCode.DatabaseVersioning.Client
{
    public static class LamarConfigurer
    {
        private static IContainer _container;

        public static void Initialize(string connectionString)
        {
            _container = new Container(x =>
            {
                // Logging.
                x.For<Serilog.ILogger>().Use(Serilog.Log.Logger);
                x.For<ILoggerFactory>().Use<SerilogLoggerFactory>().Singleton();
                x.For(typeof(ILogger<>)).Use(typeof(Logger<>)).Singleton();

                // Hook up providers.
                x.Scan(scanner =>
                {
                    scanner.AssembliesFromApplicationBaseDirectory(filter => filter.FullName.StartsWith("PeopleWhoCanCode.DatabaseVersioning"));
                    scanner.AddAllTypesOf<IDbProvider>();
                    scanner.WithDefaultConventions();
                });

                // Hook up connection string.
                x.For<IConnectionString>().Use<ConnectionString>()
                                          .Ctor<string>("value")
                                          .Is(connectionString);
            });
        }

        public static VersioningService GetVersioningService(string providerName)
        {
            SelectProviderByName(providerName);

            return _container.GetInstance<VersioningService>();
        }

        private static void SelectProviderByName(string providerName)
        {
            var providers = _container.GetAllInstances<IDbProvider>();
            var provider = providers.FirstOrDefault(x => string.Equals(x.Name, providerName, StringComparison.InvariantCultureIgnoreCase));

            _container.Configure(x => x.AddSingleton(provider));
        }
    }
}
