using System.Globalization;
using Microsoft.Extensions.Logging;
using PeopleWhoCanCode.DatabaseVersioning.Comparers;
using PeopleWhoCanCode.DatabaseVersioning.Exceptions;
using PeopleWhoCanCode.DatabaseVersioning.Models;

namespace PeopleWhoCanCode.DatabaseVersioning;

public class ChangeScriptProvider
{
    private readonly ILogger<ChangeScriptProvider> _logger;

    public ChangeScriptProvider(ILogger<ChangeScriptProvider> logger)
    {
        _logger = logger;
    }

    public IEnumerable<ChangeScript> FindAll(string databasePath, Version latestVersion, int latestChangeScriptNumber)
    {
        var changeScripts = new List<ChangeScript>();

        // Get all versions for each database.
        var versionDirectories = Directory.GetDirectories(databasePath)
                                          .OrderBy(x => x, new NaturalComparer(CultureInfo.CurrentCulture))
                                          .Select(x => new DirectoryInfo(x))
                                          .Where(x => !string.Equals(x.Name, DatabaseInitializer.AfterDatabaseCreationDirectoryName, StringComparison.InvariantCulture));

        foreach (var versionDirectory in versionDirectories)
        {
            if (Version.TryParse(versionDirectory.Name, out var version))
            {
                // Get all change scripts per version for each database since latest version.
                if (version >= latestVersion)
                {
                    var changeScriptFiles = versionDirectory.GetFiles("*.sql").OrderBy(x => x.Name, new NaturalComparer(CultureInfo.CurrentCulture));

                    foreach (var changeScriptFile in changeScriptFiles)
                    {
                        var changeScriptFileName = Path.GetFileNameWithoutExtension(changeScriptFile.FullName);

                        if (int.TryParse(changeScriptFileName, out var changeScriptNumber))
                        {
                            if (version == latestVersion && changeScriptNumber > latestChangeScriptNumber || version != latestVersion)
                            {
                                changeScripts.Add(new ChangeScript
                                {
                                    Version = version,
                                    Number = changeScriptNumber,
                                    Content = File.ReadAllText(changeScriptFile.FullName),
                                });
                            }
                            else
                            {
                                _logger.LogInformation("Ignoring change script #{ChangeScriptNumber} of version {Version}", changeScriptNumber, version);
                            }
                        }
                        else
                        {
                            throw new InvalidChangeScriptNumberException(changeScriptFileName);
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("Ignoring version {Version}", version);
                }
            }
            else
            {
                throw new InvalidVersionException(versionDirectory.Name);
            }
        }

        return changeScripts.OrderBy(x => x.Version).ThenBy(x => x.Number).ToList();
    }
}
