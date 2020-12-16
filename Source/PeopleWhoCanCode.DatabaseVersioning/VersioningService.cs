using System.Collections.Generic;
using System.IO;
using System.Linq;
using PeopleWhoCanCode.DatabaseVersioning.Models;

namespace PeopleWhoCanCode.DatabaseVersioning
{
    public class VersioningService
    {
        private readonly IDbProvider _provider;
        private readonly ChangeScriptProvider _changeScripts;
        private readonly ChangeScriptApplier _changeScriptApplier;
        private readonly DatabaseInitializer _databaseInitializer;

        public VersioningService(IDbProvider provider,
                                 ChangeScriptProvider changeScripts,
                                 ChangeScriptApplier changeScriptApplier,
                                 DatabaseInitializer databaseInitializer)
        {
            _provider = provider;
            _changeScripts = changeScripts;
            _changeScriptApplier = changeScriptApplier;
            _databaseInitializer = databaseInitializer;
        }

        public void Run(string path)
        {
            _provider.Connect();

            var databases = FindAllDatabases(path);

            foreach (var database in databases)
            {
                ApplyChangesToDatabase(path, database);
            }

            _provider.Disconnect();
        }

        private void ApplyChangesToDatabase(string path, string database)
        {
            // Initialize database.
            _databaseInitializer.Initialize(database);

            // Get latest applied version.
            var latestChangeLogRecord = GetLatestChangeLogRecord();

            // Get all changes since latest version.
            var changeScripts = FindAllChangeScripts(path, database, latestChangeLogRecord);

            // Apply each change.
            _changeScriptApplier.Apply(changeScripts);
        }

        private IEnumerable<ChangeScript> FindAllChangeScripts(string path, string database, ChangeLogRecord latestChangeLogRecord)
        {
            return _changeScripts.FindAll(Path.Combine(path, database),
                                          latestChangeLogRecord.Version,
                                          latestChangeLogRecord.Number);
        }

        private static IEnumerable<string> FindAllDatabases(string path)
        {
            return Directory.GetDirectories(path).Select(x => new DirectoryInfo(x).Name);
        }

        private ChangeLogRecord GetLatestChangeLogRecord()
        {
            return _provider.FindLatestChangeLogRecord() ?? new ChangeLogRecord();
        }
    }
}
