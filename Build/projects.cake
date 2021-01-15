public class ProjectInfo 
{
    public string Name { get; }
    public bool IsNestedZip { get; set; }
    public bool IsTest { get; }
    public string PublishRoot { get; set; }
    public string FilePath => $"../Source/PeopleWhoCanCode.DatabaseVersioning.{Name}/PeopleWhoCanCode.DatabaseVersioning.{Name}.csproj";

    public ProjectInfo(string name)
    {
        Name = name;
        IsTest = Name.EndsWith(".Tests");
        PublishRoot = "";
    }
}

var projects = new [] 
{
    new ProjectInfo("Client")
};