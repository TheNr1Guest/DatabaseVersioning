using CommandLine;

namespace PeopleWhoCanCode.DatabaseVersioning.Client
{
    public class Options
    {
        [Option('c', "connectionstring", Required = true, HelpText = "The connection string used to connect to the database server.")]
        public string ConnectionString { get; set; }

        [Option('s', "scripts", Required = true, HelpText = "The path where the change scripts are located.")]
        public string ChangeScriptsDirectory { get; set; }

        [Option('a', "after", Required = false)]
        public string AfterDatabaseCreationScript { get; set; }

        [Option('p', "provider", Required = true, HelpText = "The provider to be used, can be: MySQL")]
        public string Provider { get; set; }
    }
}
