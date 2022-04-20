namespace PeopleWhoCanCode.DatabaseVersioning.Models;

public class ConnectionString : IConnectionString
{
    public string Value { get; }

    public ConnectionString(string value)
    {
        Value = value;
    }
}
