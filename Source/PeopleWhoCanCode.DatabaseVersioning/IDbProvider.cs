using PeopleWhoCanCode.DatabaseVersioning.Models;

namespace PeopleWhoCanCode.DatabaseVersioning
{
    public interface IDbProvider
    {
        string Name { get; }
        string ConnectionString { get; }

        void Connect();
        void Disconnect();

        bool DoesDatabaseExist(string name);
        void CreateDatabase(string name);
        void SelectDatabase(string name);
        bool DoesChangeLogTableExist();
        void CreateChangeLogTable();
        ChangeLogRecord FindLatestChangeLogRecord();
        void ApplyChangeScript(ChangeScript changeScript);
        void DeleteChangeLogRecord(ChangeLogRecord changeLogRecord);
        void InsertChangeLogRecord(ChangeLogRecord changeLogRecord);
    }
}
