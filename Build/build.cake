#load paths.cake
#load urls.cake
#load projects.cake

#addin nuget:?package=Cake.FileHelpers&version=5.0.0

#tool "nuget:?package=OctopusTools&version=9.0.0" 

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var octopusApiKey = Argument("octopus-api-key", "");

var version = "0.0.0";

Setup(context =>
{
    CleanDirectories("../Source/**/bin");
    CleanDirectories("../Source/**/obj");
    CleanDirectory(Paths.Work);
});

Teardown(context =>
{
    CleanDirectory(Paths.Work);
});

Task("Version")
    .Does(() =>
    {
        version = XmlPeek("../Source/Directory.Build.props", "/Project/PropertyGroup/Version");

        Information($"-> Version: {version}");
    });

Task("NuGet-Restore")
    .Does(() =>
    {
        DotNetRestore(Paths.SolutionFile.ToString());
    });
    
Task("Build")
    .IsDependentOn("NuGet-Restore")
    .Does(() =>
    {
		DotNetBuild(Paths.SolutionFile.ToString(), 
			new DotNetBuildSettings
			{
				Configuration = configuration,
				Verbosity = DotNetVerbosity.Minimal,
				NoLogo = true,
				NoRestore = true
			});
    });

Task("Test")
    .IsDependentOn("Build")
    .Does(() => 
    {
		DotNetTest(Paths.SolutionFile.ToString(),
			new DotNetTestSettings
			{
				Configuration = configuration,
				NoLogo = true,
				NoRestore = true,
				NoBuild = true
			});
    });

Task("Publish")
    .IsDependentOn("Build")
    .Does(() => 
    {
        foreach(var project in projects)
        {
            var outputDirectory = MakeAbsolute(Paths.Publish.Combine(Directory(project.Name)));

			Information($"-> Publishing: {project.Name} to {outputDirectory}");

			DotNetPublish(project.FilePath, 
				new DotNetPublishSettings
				{
					Configuration = configuration,
					NoBuild = true,
					NoRestore = true,
					NoLogo = true,
					OutputDirectory = outputDirectory
				});
        }
    });

Task("Octo-Pack")
    .IsDependentOn("Version")
    .IsDependentOn("Publish")
    .Does(() => 
    {
        CreateDirectory(Paths.OctoPackages);

        foreach(var project in projects)
        {
            var projectPublishDirectory = MakeAbsolute(Paths.Publish.Combine(Directory(project.Name)));

            OctoPack($"PeopleWhoCanCode.DatabaseVersioning.{project.Name}", 
                new OctopusPackSettings {
                    BasePath = projectPublishDirectory.FullPath,
                    Version = version,            
                    OutFolder = Paths.OctoPackages,
                    Overwrite = true,
                    Author = "People Who Can Code"
                }
            );
        }
    });

Task("Octo-Push")
    .IsDependentOn("Octo-Pack")
    .Does(() =>
    {
        OctoPush(
            Urls.Octopus,
            octopusApiKey,
            GetFiles($"{Paths.OctoPackages}/*.nupkg"), 
            new OctopusPushSettings { 
                ReplaceExisting = true 
            }
        );
    });
    
Task("Default")
    .IsDependentOn("Build");
    
RunTarget(target);