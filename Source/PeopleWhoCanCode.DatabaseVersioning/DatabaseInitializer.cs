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

        public void Initialize(string database)
        {
            _logger.LogDebug($"Initializing database '{database}'.");

            // Create database if needed.
            CreateDatabaseIfNotExists(database);

            // Select database.
            SelectDatabase(database);

            // Check if the change log table exists, if not we need to create it.
            CreateChangeLogTableIfNotExists();

            _logger.LogInformation($"Database '{database}' has been initialized.");
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