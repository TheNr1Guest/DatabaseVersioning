using System.Reflection;
using log4net;

namespace PeopleWhoCanCode.DatabaseVersioning
{
    public class DatabaseInitializer
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IDbProvider _provider;

        public DatabaseInitializer(IDbProvider provider)
        {
            _provider = provider;
        }

        public void Initialize(string database)
        {
            // Create database if needed.
            CreateDatebaseIfNotExists(database);

            // Select database.
            SelectDatabase(database);

            // Check if the change log table exists, if not we need to create it.
            CreateChangeLogTableIfNotExists();

            Log.InfoFormat("Database '{0}' has been initialized.", database);
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

        private void CreateDatebaseIfNotExists(string database)
        {
            if (!_provider.DoesDatabaseExist(database))
            {
                _provider.CreateDatabase(database);
            }
        }
    }
}