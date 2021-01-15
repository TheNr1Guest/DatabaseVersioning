#load paths.cake
#load urls.cake
#load projects.cake

#addin nuget:?package=Cake.FileHelpers&version=3.3.0

#tool "nuget:?package=OctopusTools&version=7.4.4" 

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var octopusApiKey = Argument("octopus-api-key", "");

var version = "0.0.0.0";

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
        foreach(var project in projects)
        {
			Information($"-> Restoring packages: {project.Name}");

            DotNetCoreRestore(project.FilePath);
        }
    });
    
Task("Build")
    .IsDependentOn("NuGet-Restore")
    .Does(() =>
    {
        foreach(var project in projects)
        {
			Information($"-> Building: {project.Name}");

			DotNetCoreBuild(project.FilePath, 
				new DotNetCoreBuildSettings
				{
					Configuration = configuration,
					Verbosity = DotNetCoreVerbosity.Minimal,
					NoLogo = true,
					NoRestore = true
				});
        }
    });

Task("Test")
    .IsDependentOn("Build")
    .Does(() => 
    {
        foreach(var project in projects.Where(x => x.IsTest))
        {
			Information($"-> Testing: {project.Name}");

            DotNetCoreTest(project.FilePath,
                new DotNetCoreTestSettings
                {
                    Configuration = configuration,
                    NoBuild = true,
                    NoLogo = true
                });
        }
    });

Task("Publish")
    .IsDependentOn("Build")
    .Does(() => 
    {
        CreateDirectory(Paths.Packages);

        foreach(var project in projects.Where(x => !x.IsTest))
        {
            var outputDirectory = MakeAbsolute(Paths.Publish.Combine(Directory(project.Name)));

			Information($"-> Publishing: {project.Name} to {outputDirectory}");

			DotNetCorePublish(project.FilePath, 
				new DotNetCorePublishSettings
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

        foreach(var project in projects.Where(x => !x.IsTest))
        {
            var projectPublishDirectory = MakeAbsolute(Paths.Publish.Combine(Directory(project.Name)).Combine(Directory(project.PublishRoot)));

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