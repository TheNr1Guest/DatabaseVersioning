using Serilog;

namespace PeopleWhoCanCode.DatabaseVersioning
{
    public class DatabaseInitializer
    {
        private readonly IDbProvider _provider;

        public DatabaseInitializer(IDbProvider provider)
        {
            _provider = provider;
        }

        public void Initialize(string database)
        {
            Log.Debug($"Initializing database '{database}'.");

            // Create database if needed.
            CreateDatabaseIfNotExists(database);

            // Select database.
            SelectDatabase(database);

            // Check if the change log table exists, if not we need to create it.
            CreateChangeLogTableIfNotExists();

            Log.Information($"Database '{database}' has been initialized.");
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