namespace PeopleWhoCanCode.DatabaseVersioning.Exceptions;

public class InvalidVersionException : Exception
{
    public string InvalidVersion { get; }

    public InvalidVersionException(string invalidVersion)
    {
        InvalidVersion = invalidVersion;
    }
}
