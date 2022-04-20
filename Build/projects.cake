public class ProjectInfo 
{
    public string Name { get; }
    public string FilePath => $"../Source/PeopleWhoCanCode.DatabaseVersioning.{Name}/PeopleWhoCanCode.DatabaseVersioning.{Name}.csproj";

    public ProjectInfo(string name)
    {
        Name = name;
    }
}

var projects = new [] 
{
    new ProjectInfo("Client")
};