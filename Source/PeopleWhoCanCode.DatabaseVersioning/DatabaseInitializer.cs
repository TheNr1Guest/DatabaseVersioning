using System.IO;
using Microsoft.Extensions.Logging;

namespace PeopleWhoCanCode.DatabaseVersioning
{
    public class DatabaseInitializer
    {
        private readonly IDbProvider _provider;
        private readonly ILogger<DatabaseInitializer> _logger;

        public DatabaseInitializer(IDbProvider provider,
                                   ILogger<DatabaseInitializer> logger)
        {
            _provider = provider;
            _logger = logger;
        }

        public void Initialize(string database, string afterDatabaseCreationScriptPath)
        {
            _logger.LogDebug($"Initializing database '{database}'.");

            CreateDatabaseIfNotExists(database);

            ExecuteAfterDatabaseCreationScript(afterDatabaseCreationScriptPath);

            SelectDatabase(database);

            CreateChangeLogTableIfNotExists();

            _logger.LogInformation($"Database '{database}' has been initialized.");
        }

        private void ExecuteAfterDatabaseCreationScript(string afterDatabaseCreationScriptPath)
        {
            if (!string.IsNullOrEmpty(afterDatabaseCreationScriptPath) && File.Exists(afterDatabaseCreationScriptPath))
            {
                _provider.ExecuteQuery(File.ReadAllText(afterDatabaseCreationScriptPath));

                _logger.LogInformation($"Executed '{Path.GetFileName(afterDatabaseCreationScriptPath)}'.");
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