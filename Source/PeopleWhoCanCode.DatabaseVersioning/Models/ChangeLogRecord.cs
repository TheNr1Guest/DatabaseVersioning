namespace PeopleWhoCanCode.DatabaseVersioning.Models;

public class ChangeLogRecord
{
    public Version Version { get; set; }
    public int Number { get; set; }
    public string? Error { get; set; }
    public bool IsSuccessful => string.IsNullOrEmpty(Error);
    public Exception? Exception { get; set; }

    public ChangeLogRecord()
    {
        Version = new Version(0, 0, 0);
        Number = 0;
    }

    public ChangeLogRecord(ChangeScript changeScript)
    {
        Version = changeScript.Version;
        Number = changeScript.Number;
    }

    public ChangeLogRecord(ChangeScript changeScript, Exception? exception) : this(changeScript)
    {
        Exception = exception;

        if (exception != null)
        {
            Error = exception.ToString();
        }
    }

    public ChangeLogRecord(Version version, int number, string? error)
    {
        Version = version;
        Number = number;
        Error = error;
    }
}
