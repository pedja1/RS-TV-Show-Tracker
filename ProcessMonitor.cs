﻿namespace RoliSoft.TVShowTracker
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Provides methods to monitor a given process for open file handles.
    /// </summary>
    public static class ProcessMonitor
    {
        /// <summary>
        /// Gets or sets the list of files that have been seen to be open by the specified process.
        /// </summary>
        /// <value>The open files.</value>
        public static List<string> OpenFiles { get; set; }

        /// <summary>
        /// Initializes the <see cref="ProcessMonitor"/> class.
        /// </summary>
        static ProcessMonitor()
        {
            OpenFiles = new List<string>();
        }

        /// <summary>
        /// Gets the list of open file handles for the given processes.
        /// </summary>
        /// <param name="processes">The processes' executable name.</param>
        /// <returns>List of the open files.</returns>
        public static List<FileInfo> GetHandleList(params string[] processes)
        {
            var sb = new StringBuilder();

            foreach (var process in processes)
            {
                sb.AppendLine(Utils.RunAndRead("handle.exe", "-accepteula -p " + process));
            }

            return Regex.Matches(sb.ToString(), @"(?:D|\-)\)\s+(.+)(?:\r|$)")
                   .Cast<Match>()
                   .Select(m => new FileInfo(m.Groups[1].Value.Trim()))
                   .ToList();
        }

        /// <summary>
        /// Checks for open files on the specified processes and marks them if recognized.
        /// </summary>
        public static void CheckOpenFiles()
        {
            if (!File.Exists("handle.exe"))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(Database.XmlSetting("Processes to Monitor")))
            {
                return;
            }

            var procs = Database.XmlSetting("Processes to Monitor").Trim(',').Split(',');
            var files = GetHandleList(procs);
            var shows = Database.Query("select showid, name from tvshows order by rowid asc");

            foreach (var show in shows)
            {
                var parts = ShowNames.Normalize(show["name"]).Split(' ');

                foreach (var file in files)
                {
                    if (parts.All(part => (Regex.IsMatch(file.Name, @"\b" + part + @"\b", RegexOptions.IgnoreCase) // does it have all the words in the file name?
                                           || Regex.IsMatch(file.Directory.Name, @"\b" + part + @"\b", RegexOptions.IgnoreCase))) // or in the directory name?
                        && Regex.IsMatch(file.Name, @"\.(avi|mkv|mp4)$", RegexOptions.IgnoreCase)) // is it a known video file extension?
                    {
                        var ep = ShowNames.ExtractEpisode(file.ToString(), 1);

                        if (!string.IsNullOrWhiteSpace(ep))
                        {
                            if (!OpenFiles.Contains(file.ToString()))
                            {
                                // add to open files list
                                // 5 minutes later we'll check again, and if it's still open we'll mark it as seen
                                // the reason for this is that an episode will be marked as seen only if you're watching it for more than 10 minutes (5 minute checks twice)

                                OpenFiles.Add(file.ToString());
                            }
                            else
                            {
                                // mark it as seen

                                try
                                {
                                    var eptmp = ep.Split('x'); // we'll split the 1x01 into { 1, 01 } then parse both to int
                                    var epid  = Database.GetEpisodeID(show["showid"], int.Parse(eptmp[0]), int.Parse(eptmp[1]));

                                    if (Database.Query("select * from tracking where showid = ? and episodeid = ?", show["showid"], epid).Count == 0)
                                    {
                                        Database.Execute("insert into tracking values (?, ?)", show["showid"], epid);
                                        MainWindow.Active.DataChanged();
                                    }
                                }
                                catch
                                {
                                    continue;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
