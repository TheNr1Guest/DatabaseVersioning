using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using PeopleWhoCanCode.DatabaseVersioning.Comparers;
using PeopleWhoCanCode.DatabaseVersioning.Exceptions;
using PeopleWhoCanCode.DatabaseVersioning.Models;
using Serilog;

namespace PeopleWhoCanCode.DatabaseVersioning
{
    public class ChangeScriptProvider
    {
        public IEnumerable<ChangeScript> FindAll(string databasePath, Version latestVersion, int latestChangeScriptNumber)
        {
            IList<ChangeScript> changeScripts = new List<ChangeScript>();

            // Get all versions for each database.
            var versionPaths = Directory.GetDirectories(databasePath).OrderBy(x => x, new NaturalComparer(CultureInfo.CurrentCulture));

            foreach (var versionPath in versionPaths)
            {
                string versionDirectoryName = new DirectoryInfo(versionPath).Name;

                if (Version.TryParse(versionDirectoryName, out var version))
                {
                    // Get all change scripts per version for each database since latest version.
                    if (version >= latestVersion)
                    {
                        var changeScriptPaths = Directory.GetFiles(versionPath, "*.sql").OrderBy(x => x, new NaturalComparer(CultureInfo.CurrentCulture));

                        foreach (var changeScriptPath in changeScriptPaths)
                        {
                            var changeScriptFileName = Path.GetFileNameWithoutExtension(changeScriptPath);

                            if (int.TryParse(changeScriptFileName, out var changeScriptNumber))
                            {
                                if (version == latestVersion && changeScriptNumber > latestChangeScriptNumber || version != latestVersion)
                                {
                                    changeScripts.Add(new ChangeScript(version, changeScriptNumber, File.ReadAllText(changeScriptPath)));
                                }
                                else
                                {
                                    Log.Debug($"Ignoring change script #{changeScriptNumber} of version {version}");
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
                        Log.Debug($"Ignoring version {version}");
                    }
                }
                else
                {
                    throw new InvalidVersionException(versionDirectoryName);
                }
            }

            return changeScripts.OrderBy(x => x.Version).ThenBy(x => x.Number).ToList();
        }
    }
}