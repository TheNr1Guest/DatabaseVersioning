using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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

            foreach (string versionPath in versionPaths)
            {
                string versionDirectoryName = new DirectoryInfo(versionPath).Name;

                Version version;

                if (Version.TryParse(versionDirectoryName, out version))
                {
                    // Get all change scripts per version for each database since latest version.
                    if (version >= latestVersion)
                    {
                        var changeScriptPaths = Directory.GetFiles(versionPath, "*.sql").OrderBy(x => x, new NaturalComparer(CultureInfo.CurrentCulture));

                        foreach (string changeScriptPath in changeScriptPaths)
                        {
                            string changeScriptFileName = Path.GetFileNameWithoutExtension(changeScriptPath);
                            int changeScriptNumber;

                            if (int.TryParse(changeScriptFileName, out changeScriptNumber))
                            {
                                if (version == latestVersion && changeScriptNumber > latestChangeScriptNumber || version != latestVersion)
                                {
                                    changeScripts.Add(new ChangeScript(version, changeScriptNumber, File.ReadAllText(changeScriptPath)));
                                }
                                else
                                {
                                    Log.Debug(string.Format("Ignoring change script #{0} of version {1}", changeScriptNumber, version));
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