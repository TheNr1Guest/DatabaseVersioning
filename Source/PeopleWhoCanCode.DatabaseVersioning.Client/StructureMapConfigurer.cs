using System;
using System.Linq;
using PeopleWhoCanCode.DatabaseVersioning.Models;
using StructureMap;

namespace PeopleWhoCanCode.DatabaseVersioning.Client
{
    public static class StructureMapConfigurer
    {
        private static IContainer _container;

        public static void Initialize(string connectionString)
        {
            _container = new Container(x =>
            {
                // Hook up providers.
                x.Scan(s =>
                {
                    s.AssembliesFromApplicationBaseDirectory(f => f.FullName.StartsWith("PeopleWhoCanCode.DatabaseVersioning"));
                    s.AddAllTypesOf<IDbProvider>();
                    s.WithDefaultConventions();
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

            _container.Configure(x => x.For<IDbProvider>().Use(provider));
        }
    }
}
