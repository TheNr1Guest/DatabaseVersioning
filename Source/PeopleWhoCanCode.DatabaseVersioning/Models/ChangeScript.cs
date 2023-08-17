namespace PeopleWhoCanCode.DatabaseVersioning.Models;

public class ChangeScript
{
    public required Version Version { get; init; }
    public required int Number { get; init; }
    public required string Content { get; init; }
}
