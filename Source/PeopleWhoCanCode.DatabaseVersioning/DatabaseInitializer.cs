using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using PeopleWhoCanCode.DatabaseVersioning.Comparers;

namespace PeopleWhoCanCode.DatabaseVersioning
{
    public class DatabaseInitializer
    {
        public const string AfterDatabaseCreationDirectoryName = "Creation";

        private readonly IDbProvider _provider;
        private readonly ILogger<DatabaseInitializer> _logger;

        public DatabaseInitializer(IDbProvider provider,
                                   ILogger<DatabaseInitializer> logger)
        {
            _provider = provider;
            _logger = logger;
        }

        public void Initialize(string scriptsDirectoryPath, string database)
        {
            _logger.LogDebug($"Initializing database '{database}'.");

            CreateDatabaseIfNotExists(database);

            ExecuteAfterDatabaseCreationScripts(scriptsDirectoryPath, database);

            SelectDatabase(database);

            CreateChangeLogTableIfNotExists();

            _logger.LogInformation($"Database '{database}' has been initialized.");
        }

        private void ExecuteAfterDatabaseCreationScripts(string scriptsDirectoryPath, string database)
        {
            var afterDatabaseCreationScriptsDirectoryPath = Path.Combine(scriptsDirectoryPath, database, AfterDatabaseCreationDirectoryName);

            if (!Directory.Exists(afterDatabaseCreationScriptsDirectoryPath)) return;

            var scripts = Directory.GetFiles(afterDatabaseCreationScriptsDirectoryPath, "*.sql").OrderBy(x => x, new NaturalComparer(CultureInfo.CurrentCulture));

            foreach (var script in scripts)
            {
                _provider.ExecuteQuery(File.ReadAllText(script));

                _logger.LogInformation($"Executed '{Path.GetFileName(script)}'.");
            }
        }

        private void CreateChangeLogTableIfNotExists()
        {
            if (!_provider.DoesChangeLogTableExist())
            {
                _provider.CreateChangeLogTable();
            }
        }

        private void SelectDatabase(string database)
        {
            _provider.SelectDatabase(database);
        }

        private void CreateDatabaseIfNotExists(string database)
        {
            if (!_provider.DoesDatabaseExist(database))
            {
                _provider.CreateDatabase(database);
            }
        }
    }
}