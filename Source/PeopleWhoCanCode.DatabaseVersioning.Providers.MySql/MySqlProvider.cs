using System;
using System.Data;
using MySql.Data.MySqlClient;
using PeopleWhoCanCode.DatabaseVersioning.Models;

namespace PeopleWhoCanCode.DatabaseVersioning.Providers.MySql
{
    public class MySqlProvider : IDbProvider
    {
        private const string DatabaseExistsQuery = "SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = ?DatabaseName;";
        private const string DatabaseCreationQuery = "CREATE DATABASE ?DatabaseName";
        private const string DatabaseSelectionQuery = "USE ?DatabaseName;";
        private const string ChangeLogTableExistsQuery = "SHOW TABLES LIKE 'ChangeLog';";
        private const string ChangeLogTableCreationQuery = @"CREATE TABLE  `ChangeLog` (
                                                            `Version` VARCHAR( 20 ) NOT NULL ,
                                                            `Number` INT NOT NULL ,
                                                            `ApplyDate` DATETIME NOT NULL,
                                                            `Error` TEXT NULL ,
                                                            PRIMARY KEY (  `Version` ,  `Number` )
                                                            ) ENGINE = INNODB;";
        private const string ChangeLogSelectLatestQuery = @"SELECT `Version`, `Number`, `Error`  
                                                            FROM `ChangeLog` 
                                                            WHERE `Error` IS NULL 
                                                            ORDER BY INET_ATON(SUBSTRING_INDEX(CONCAT(`Version`,'.0.0.0'), '.', 4)) DESC, `Number` DESC 
                                                            LIMIT 0, 1;";
        private const string ChangeLogInsertQuery = "INSERT INTO `ChangeLog` (`Version`, `Number`, `ApplyDate`, `Error`) VALUES (?Version, ?Number, NOW(), ?Error);";
        private const string ChangeLogDeleteQuery = "DELETE FROM `ChangeLog` WHERE `Version`= ?Version AND Number = ?Number;";

        public string Name => "MySQL";
        public string ConnectionString { get; }

        private MySqlConnection _connection;

        public MySqlProvider(IConnectionString connectionString)
        {
            ConnectionString = connectionString.Value;
        }

        public void Connect()
        {
            if (_connection == null)
            {
                _connection = new MySqlConnection(ConnectionString);
            }

            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
        }

        public void Disconnect()
        {
            _connection?.Close();
        }

        public bool DoesDatabaseExist(string name)
        {
            using (var command = new MySqlCommand(DatabaseExistsQuery, _connection))
            {
                command.Parameters.AddWithValue("?DatabaseName", name);

                using (var reader = command.ExecuteReader())
                {
                    var exists = reader.HasRows;

                    return exists;
                }
            }
        }

        public void CreateDatabase(string name)
        {
            ExecuteQuery(DatabaseCreationQuery.Replace("?DatabaseName", name));
        }

        public void SelectDatabase(string name)
        {
            ExecuteQuery(DatabaseSelectionQuery.Replace("?DatabaseName", name));
        }

        public bool DoesChangeLogTableExist()
        {
            using (var command = new MySqlCommand(ChangeLogTableExistsQuery, _connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    return reader.HasRows;
                }
            }
        }

        public void CreateChangeLogTable()
        {
            ExecuteQuery(ChangeLogTableCreationQuery);
        }

        public ChangeLogRecord FindLatestChangeLogRecord()
        {
            using (var command = new MySqlCommand(ChangeLogSelectLatestQuery, _connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    ChangeLogRecord changeLogRecord = null;

                    if (reader.HasRows)
                    {
                        reader.Read();

                        changeLogRecord = new ChangeLogRecord(
                            Version.Parse(reader.GetString("Version")),
                            reader.GetInt32("Number"),
                            reader.IsDBNull(reader.GetOrdinal("Error")) ? null : reader.GetString("Error"));
                    }

                    return changeLogRecord;
                }
            }
        }

        public void ApplyChangeScript(ChangeScript changeScript)
        {
            ExecuteQuery(changeScript.Content);
        }

        public void DeleteChangeLogRecord(ChangeLogRecord changeLogRecord)
        {
            using (var command = new MySqlCommand(ChangeLogDeleteQuery, _connection))
            {
                command.Parameters.AddWithValue("?Version", changeLogRecord.Version.ToString());
                command.Parameters.AddWithValue("?Number", changeLogRecord.Number);
                command.ExecuteNonQuery();
            }
        }

        public void InsertChangeLogRecord(ChangeLogRecord changeLogRecord)
        {
            using (var command = new MySqlCommand(ChangeLogInsertQuery, _connection))
            {
                command.Parameters.AddWithValue("?Version", changeLogRecord.Version.ToString());
                command.Parameters.AddWithValue("?Number", changeLogRecord.Number);
                command.Parameters.AddWithValue("?Error", changeLogRecord.Error);
                command.ExecuteNonQuery();
            }
        }

        public void ExecuteQuery(string query)
        {
            if (string.IsNullOrEmpty(query)) return;

            using (var command = new MySqlCommand(query, _connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}
