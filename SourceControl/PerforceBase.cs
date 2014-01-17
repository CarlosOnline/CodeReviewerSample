//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Sergey Solyanik for The Malevich Project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CodeReviewer;

namespace SourceControl
{
    /// <summary>
    /// Implements the base class for all perforce-type source control systems.
    /// </summary>
    public class PerforceBase : ISourceControl, ILogControl
    {
        /// <summary>
        /// The location of the perforce client .exe.
        /// </summary>
        private string ClientExe;

        /// <summary>
        /// Source control system.
        /// </summary>
        private ISourceControlSystem SourceControl;

        /// <summary>
        /// Log zones that are turned on.
        /// </summary>
        private LogOptions LogLevel;

        /// <summary>
        /// Regex parser of the title string of the p4|sd describe output.
        /// 
        /// Group[1] is the date the change was created.
        /// </summary>
        private static Regex ClientChangeParser = new Regex(
            @"Change (?<CL>\d+) on (?<date>.+) by [a-z,A-Z,0-9,@,.,\\,\-,_]+@(?<client>\S+) \*pending\* \'.+\'",
            RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

        /// <summary>
        /// Regex parser of the title string of the p4|sd describe output.
        /// 
        /// Group[1] is the date the change was created.
        /// </summary>
        private static Regex ClientDescribeTitleParser = new Regex(
            @"Change \d+ by [a-z,A-Z,0-9,@,.,\\,\-,_]+@(?<client>\S+) on (?<date>.+) \*pending\*",
            RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

        /// <summary>
        /// Regex parser of the file list part of the p4|sd describe output.
        /// 
        /// Group[1] is the name of the file (depot semantics)
        /// Group[2] is the revision of the file (Int.Parse'able)
        /// Group[3] is one of: add, edit or delete.
        /// </summary>
        private static Regex ClientDescribeFileParser = new Regex(
            @"^... (?<name>//([a-z,0-9,_,\-,/, ,\.,\$,\{,\},\+,=])+)#(?<rev>none|(\d)+) " +
            @"(?<action>add|delete|edit|branch|integrate|move|move\/add|move\/delete)$",
            RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

        /// <summary>
        /// Regex parser of the p4|sd where output.
        /// 
        /// Group[1] is the depot name
        /// Group[2] is the client name
        /// Group[3] is the local name
        /// </summary>
        private static Regex ClientWhereParser = new Regex(
            @"^(?<depot>//.+) (?<client>//.+) (?<local>[a-z]:\\.+)\r\n$",
            RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Multiline);

        /// <summary>
        /// Tests if the file is a text file.
        /// </summary>
        private static Regex ClientOpenedIsText = new Regex(@"^.*\([x,c,k]?text\)[\n,\r]*$");

        /// <summary>
        /// Matcher for the time in fstat output.
        /// </summary>
        private static Regex ClientFstatHeadTime = new Regex(@".*\.\.\. headTime (\d+)(\D).*");

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="clientExe"> The location of source depot client (e.g. sd.exe). </param>
        /// <param name="sourceControl"> The Source Control - must be either Source Depot or Perforce. </param>
        protected PerforceBase(string clientExe, ISourceControlSystem sourceControl)
        {
            SourceControl = sourceControl;
            ClientExe = clientExe;
        }

        /// <summary>
        /// Disconnects from the depot.
        /// </summary>
        public void Disconnect()
        {
            // Don't need to do anything.
        }

        /// <summary>
        /// Connects to the depot.
        /// </summary>
        public bool Connect()
        {
            // Don't need to do anything.
            return true;
        }

        /// <summary>
        /// Control the log output. Implemented here primarily to allow dumping the IO from the client utility.
        /// </summary>
        /// <param name="level"> The level of logging. </param>
        public void SetLogLevel(LogOptions level)
        {
            LogLevel = level;
        }

        /// <summary>
        /// Runs the client exe, returns the output in a string. If error happens, prints stderr and returns null.
        /// </summary>
        /// <param name="commandLine"> Command line to execute. </param>
        /// <param name="eatFirstLine"> Whether the first line should be swallowed. </param>
        /// <returns> The output as one string. </returns>
        protected virtual string RunClient(string commandLine, bool eatFirstLine)
        {
            Process client = new Process();
            client.StartInfo.UseShellExecute = false;
            client.StartInfo.RedirectStandardError = true;
            client.StartInfo.RedirectStandardOutput = true;
            client.StartInfo.CreateNoWindow = true;
            client.StartInfo.FileName = ClientExe;

            if (SourceControl is Perforce)
            {
                Perforce p = (Perforce)SourceControl;
                commandLine = "-c " + p.Client + " -p " + p.Port + (p.User != null ? " -u " + p.User : "") +
                    (p.Passwd != null ? " -P " + p.Passwd : "") + " " + commandLine;

                // We don't want any custom differs...
                if (client.StartInfo.EnvironmentVariables.ContainsKey("P4DIFF"))
                    client.StartInfo.EnvironmentVariables.Remove("P4DIFF");
            }
            else
            {
                SourceDepot s = (SourceDepot)SourceControl;
                commandLine = "-c " + s.Client + " -p " + s.Port + (s.Proxy != null ? " -R " + s.Proxy : "") +
                    " " + commandLine;

                if (client.StartInfo.EnvironmentVariables.ContainsKey("SDDIFF"))
                    client.StartInfo.EnvironmentVariables.Remove("SDDIFF");
                if (client.StartInfo.EnvironmentVariables.ContainsKey("SDUDIFF"))
                    client.StartInfo.EnvironmentVariables.Remove("SDUDIFF");

                // This is a hack to prevent the client from reading potentially conflicting
                // settings (specifically, SDDIFF and SDUDIFF) from sd.ini that might
                // be in one of the parent directories.
                client.StartInfo.WorkingDirectory = Environment.GetEnvironmentVariable("WINDIR");
            }

            Log.Info("\"{0}\" {1}", ClientExe, commandLine);
            client.StartInfo.Arguments = commandLine;

            client.Start();

            string stderr;
            string result = CodeReviewer.Util.CommonUtils.ReadProcessOutput(client, eatFirstLine, out stderr);

            if (stderr != null && !stderr.Equals(""))
            {
                Log.Error("Failed sd|p4 " + commandLine);
                Log.Error(stderr);
                result = null;
            }

            client.Dispose();

            if ((LogLevel & LogOptions.ClientUtility) == LogOptions.ClientUtility)
            {
                Log.Info("Client command line:\n");
                Log.Info(commandLine);
                Log.Info("Client command output:\n");
                Log.Info(result);
                Log.Info("----------------------");
            }
            return result;
        }

        /// <summary>
        /// A customer-friendly way to fail if we cannot parse the output of client exe.
        /// </summary>
        /// <param name="output"> The text to print with the error message. The user is advised
        /// to report it with the bug. </param>
        private void BugOut(string output)
        {
            Log.Error("I do not recognize the format of this change.\n");
            Log.Error("This is very likely to be a bug in the program.");
            Log.Error("Submit the following output with your bug:");
            Log.Error(output);

            throw new SourceControlRuntimeError("Unrecognized format - " + output);
        }

        /// <summary>
        /// For every file in the change list, fill in its local path and either diff if it is an edit, or the
        /// file itself if it is an add.
        /// </summary>
        /// <param name="change"> The change list, instantiated. </param>
        /// <param name="includeBranchedFiles"> Include the text for branched and integrated files. </param>
        private void FillInFileData(Change change, bool includeBranchedFiles)
        {
            foreach (ChangeFile file in change.Files)
            {
                string where = RunClient("where \"" + file.ServerFileName + "\"", false);
                if (string.IsNullOrEmpty(where))
                    BugOut("sd|p4 where " + file.ServerFileName);

                Match files = ClientWhereParser.Match(where);
                if (!files.Success)
                    BugOut(where);

                file.LocalFileName = files.Groups[3].Value;
                if (File.Exists(file.LocalFileName))
                    file.LastModifiedTime = File.GetLastWriteTimeUtc(file.LocalFileName);

                if (!file.IsText)
                    continue;

                if (file.Action == ChangeFile.SourceControlAction.EDIT ||
                    (file.Action == ChangeFile.SourceControlAction.INTEGRATE && includeBranchedFiles))
                {
                    if (!file.IsShelved)
                        file.Data = RunClient("diff \"" + file.ServerFileName + "\"", true);
                    else
                    {
                        // Diff shelved file with diff2
                        // diff2 file#previousRevision file@=CL 
                        file.Data = RunClient(string.Format("diff2 \"{0}#{1}\" \"{0}@={2}\"", file.ServerFileName, file.Revision, change.ChangeListId), true);
                    }
                }
                else if (file.Action == ChangeFile.SourceControlAction.ADD ||
                    (file.Action == ChangeFile.SourceControlAction.BRANCH && includeBranchedFiles))
                {
                    try
                    {
                        if (!file.IsShelved)
                        {
                            StreamReader reader = new StreamReader(file.LocalFileName);
                            file.Data = reader.ReadToEnd();
                            reader.Close();
                        }
                        else
                        {
                            file.Data = RunClient(string.Format("print -q \"{0}@={1}\"", file.ServerFileName, change.ChangeListId), false);
                        }
                    }
                    catch (FileNotFoundException)
                    {
                        Log.Error("File not found: " + file.LocalFileName);
                        throw new SourceControlRuntimeError("File not found: " + file.LocalFileName);
                    }
                }
            }  
        }

        /// <summary>
        /// Gets the change from the source control system. The change must be pending.
        /// Returns null if any error occurs, or the change is not pending.
        /// </summary>
        /// <param name="changeNo"> CL identifier. </param>
        /// <param name="changeListId"> Incude the text of branched and integrated files. </param>
        /// <param name="skipFiles"> True to skip files when not needed - faster. </param>
        /// <returns> The change. </returns>
        public Change GetChange(string changeListId, bool includeBranchedFiles, bool skipFile = false)
        {
            bool shelved = false;
            int changeNo;
            if (!Int32.TryParse(changeListId, out changeNo))
            {
                Log.Error("Change List number is not a number!");
                return null;
            }

            // Use -S for shelved changelists
            string description = RunClient("describe -s -S " + changeNo, false);
            if (string.IsNullOrEmpty(description))
                return null;

            StringReader reader = new StringReader(description);
            string firstLine = reader.ReadLine();
            if (firstLine == null)
            {
                Log.Error("The description is empty. Cannot proceed.");
                return null;
            }

            Match firstLineMatch = ClientDescribeTitleParser.Match(firstLine);
            if (!firstLineMatch.Success)
            {
                Log.Error("This change is not pending!");
                return null;
            }

            string clientName = firstLineMatch.Groups[1].Value;

            DateTime timeStamp;

            if (!DateTime.TryParse(firstLineMatch.Groups[2].Value, out timeStamp))
                BugOut(description + "\n\n[Could not parse the time stamp]");

            timeStamp = timeStamp.ToUniversalTime();

            if (!"".Equals(reader.ReadLine()))
                BugOut(description + "\n\n[No newline before change description]");

            StringBuilder changeDescription = new StringBuilder();
            for (; ; )
            {
                string line = reader.ReadLine();
                if (line == null)
                    BugOut(description + "\n\n[Unexpected EOL]");

                if (line.Equals("Affected files ..."))
                    break;

                if (line.Equals("Shelved files ...")) {
                    shelved = true;
                    break;
                }

                changeDescription.Append(line.Trim());
                changeDescription.Append('\n');
            }

            if (!"".Equals(reader.ReadLine()))
                BugOut(description + "\n\n[No newline before the list of files]");

            List<ChangeFile> files = new List<ChangeFile>();
            if (!skipFile)
            {
                for (; ; )
                {
                    string fileString = reader.ReadLine();
                    if (fileString == null || "".Equals(fileString))
                        break;

                    Match match = ClientDescribeFileParser.Match(fileString);
                    if (!match.Success)
                        BugOut(description + "\n\n[Could not match " + fileString + "]");

                    string fileName = match.Groups[1].Value;
                    string rev = match.Groups[2].Value;
                    string action = match.Groups[3].Value;

                    int revision = -1;
                    if (rev != "none" && !Int32.TryParse(rev, out revision))
                        BugOut(description + "\n\n[Could not parse revision in " + fileString + "]");

                    // Make sure the file is text.
                    bool isText = false;
                    if (!shelved)
                    {
                        var openedFile = RunClient("opened \"" + fileName + "\"", false);
                        if (string.IsNullOrEmpty(openedFile) && !shelved)
                            BugOut("opened \"" + fileName + "\"");

                        // Ignore binary files
                        isText = ClientOpenedIsText.IsMatch(openedFile);
                    }
                    else
                    {
                        var stats = RunClient(string.Format("fstat \"{0}@={1}\"", fileName, changeListId), false);
                        if (string.IsNullOrEmpty(stats) && !shelved)
                            BugOut("opened \"" + fileName + "\"");

                        // Ignore binary files
                        isText = stats.Contains("... headType text") ? true : false;
                    }

                    ChangeFile.SourceControlAction a = ChangeFile.SourceControlAction.ADD;
                    if ("edit".Equals(action))
                        a = ChangeFile.SourceControlAction.EDIT;
                    else if ("add".Equals(action) || "move/add".Equals(action))
                        a = ChangeFile.SourceControlAction.ADD;
                    else if ("delete".Equals(action) || "move/delete".Equals(action))
                        a = ChangeFile.SourceControlAction.DELETE;
                    else if ("branch".Equals(action))
                        a = ChangeFile.SourceControlAction.BRANCH;
                    else if ("integrate".Equals(action))
                        a = ChangeFile.SourceControlAction.INTEGRATE;
                    else if ("move/add".Equals(action))
                        a = ChangeFile.SourceControlAction.RENAME;
                    else
                        BugOut(description + "\n\n[Unknown file action: " + action + "]");

                    ChangeFile file = new ChangeFile(fileName, a, revision, isText, shelved);
                    files.Add(file);
                }
            }

            if (files.Count > 0 || skipFile)
            {
                Change change = new Change(SourceControl, clientName, changeListId, timeStamp,
                    description, files.ToArray());
                FillInFileData(change, includeBranchedFiles);
                return change;
            }

            return null;
        }

        /// <summary>
        /// Gets the pending changes
        /// Returns null if any error occurs, or the change is not pending.
        /// </summary>
        /// <param name="changeNo"> CL identifier. </param>
        /// <param name="changeListId"> Incude the text of branched and integrated files. </param>
        /// <returns> The change. </returns>
        public List<Change> GetChanges(string userName)
        {
            var results = new List<Change>();

            string changes = RunClient("changes -s pending -u " + userName, false);
            if (string.IsNullOrEmpty(changes))
                return results;

            StringReader reader = new StringReader(changes);
            var line = reader.ReadLine();
            do
            {
                var match = ClientChangeParser.Match(line);
                if (match.Success)
                {
                    var CL = match.Groups[1].Value.Trim();
                    var change = GetChange(CL, true, true);
                    if (change != null)
                        results.Add(change);
                }
                line = reader.ReadLine();
            } while (line != null);

            return results;
        }

        /// <summary>
        /// Reads a file from the source control system.
        /// </summary>
        /// <param name="depotFileName"> The server name of the file. </param>
        /// <param name="revision"> The revision of the file to get. </param>
        /// <returns> The string that constitutes the body of the file. </returns>
        public string GetFile(string depotFileName, int revision, out DateTime? fileTime)
        {
            fileTime = null;
            string ret = RunClient("print \"" + depotFileName + "\"#" + revision, true);
            if (string.IsNullOrEmpty(ret))
                return null;

            string stats = RunClient("fstat \"" + depotFileName + "\"#" + revision, false);
            if (string.IsNullOrEmpty(stats))
                return null;

            Match match = ClientFstatHeadTime.Match(stats);
            long fileTimeUnix;
            if (match.Success && long.TryParse(match.Groups[1].Value, out fileTimeUnix))
                fileTime = DateTime.FromFileTimeUtc(fileTimeUnix * 10000000 + 116444736000000000L);
            return ret;
        }
    }
}
