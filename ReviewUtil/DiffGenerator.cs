using CodeReviewer.Extensions;
using CodeReviewer.Models;
using CodeReviewer.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;

namespace CodeReviewer
{
    public class DiffGenerator
    {
        private readonly string _preDiffArgsBase = ConfigurationManager.AppSettings["preDiffArgsBase"];
        private readonly string _diffArgsBase = ConfigurationManager.AppSettings["diffArgsBase"];
        private readonly string _diffArgsIgnoreWhiteSpace = ConfigurationManager.AppSettings["diffArgsIgnoreWhiteSpace"];
        private readonly string _preDiffExe = ConfigurationManager.AppSettings["preDiffExe"];
        private readonly string _diffExe = ConfigurationManager.AppSettings["diffExe"];
        private readonly string _diffFolderRoot = ConfigurationManager.AppSettings["diffFolderRoot"];
        private Random _random = new Random();

        public CodeReviewerContext DataContext = null;

        // Max length of the description we output in the change list.

        // Max length of the review comment we output in the change list.
        private string _tabValue;

        /// <summary>
        ///     Computes the string which is used for representing tabs. The default is '\t', but it could be overridden
        ///     in user context and in web.config file.
        /// </summary>
        /// <param name="uc"> User's settings. </param>
        /// <returns> A string to be used for substituting the tabs. </returns>
        private string TabValue
        {
            get
            {
                var tabValue = "  \\t";
                var spacesPerTab = ConfigurationManager.AppSettings["spacesPerTab"];
                if (spacesPerTab != null)
                {
                    int value;
                    if (int.TryParse(spacesPerTab, out value) && value > 0)
                        tabValue = new string(' ', value);
                }
                _tabValue = tabValue;

                return _tabValue;
            }
        }

        public DiffGenerator(CodeReviewerContext db)
        {
            DataContext = db;
        }

        public void GenDiffFiles(int id, string userName, UserSettingsDto settings, bool force = false)
        {
            var changeList = (from change in DataContext.ChangeLists
                              where change.Id == id
                              select change).FirstOrDefault();

            if (changeList == null)
            {
                // Log
                return;
            }
            var baseReviewId = GetBaseReviewId(userName, changeList.Id);

            foreach (var changeFile in changeList.ChangeFiles)
            {
                var fileVersions = changeFile.FileVersions.ToArray();

                if (fileVersions.Length == 1)
                {
                    GenerateDiffFile(fileVersions[0], fileVersions[0], changeList.CL, baseReviewId, settings, force);
                    continue;
                }

                var prev = 0;
                for (var idx = prev + 1; idx < fileVersions.Length; prev = idx++)
                {
                    var vid1 = fileVersions[prev].Id;
                    var vid2 = fileVersions[idx].Id;

                    var versionQuery = from vr in DataContext.FileVersions
                                       where vr.Id == vid1 || vr.Id == vid2
                                       select vr;
                    if (versionQuery.Count() == 2)
                    {
                        var leftRight = versionQuery.ToArray();
                        FileVersion left, right;
                        if (leftRight[0].Id == vid1)
                        {
                            left = leftRight[0];
                            right = leftRight[1];
                        }
                        else
                        {
                            left = leftRight[1];
                            right = leftRight[0];
                        }

                        GenerateDiffFile(left, right, changeList.CL, baseReviewId, settings, force);
                    }
                }
            }
        }

        /// <summary>
        ///     Display the error report.
        /// </summary>
        /// <param name="errorReport"> The error to print. </param>
        private void ErrorOut(string errorReport)
        {
            Log.Error(errorReport);
        }

        /// <summary>
        ///     Returns the id for the current open review for the given user, or 0 if it does not exist.
        /// </summary>
        /// <param name="userName"> The user name. </param>
        /// <param name="changeId"> The change list for which to retrieve the base review. </param>
        /// <returns> The id of the review, or 0. </returns>
        public int GetBaseReviewId(string userName, int changeId)
        {
            var reviewQuery = from rv in DataContext.Reviews
                              where
                                  rv.ChangeListId == changeId && rv.UserName == userName && !rv.IsSubmitted
                              select rv.Id;
            if (Queryable.Count<int>(reviewQuery) != 1)
                return 0;
            return Queryable.Single<int>(reviewQuery);
        }

        /// <summary>
        ///     Adds an ACL entry on the specified file for the specified account.
        /// </summary>
        /// <param name="fileName"> File name. </param>
        /// <param name="account"> The sid for the account. </param>
        /// <param name="rights"> What rights to grant (e.g. read). </param>
        /// <param name="controlType"> Grant or deny? </param>
        public static void AddFileSecurity(string fileName, WellKnownSidType account,
                                           FileSystemRights rights, AccessControlType controlType)
        {
            // Get a FileSecurity object that represents the
            // current security settings.
            var fSecurity = File.GetAccessControl(fileName);

            // Added the FileSystemAccessRule to the security settings.
            fSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(account, null), rights, controlType));

            // Set the new access settings.
            File.SetAccessControl(fileName, fSecurity);
        }

        /// <summary>
        ///     Produces a stream for a file version. This may involve reading the database for the base for the version,
        ///     if the version is a diff.
        /// </summary>
        /// <param name="version"> The version to stream. </param>
        /// <returns> The resultant full-text stream. </returns>
        private StreamCombiner GetFileStream(FileVersion version, bool ignoreBase = false)
        {
            if (version.IsFullText)
                return new StreamCombiner(version.Text);

            var baseVersionQuery = from fl in DataContext.FileVersions
                                   where
                                       fl.FileId == version.FileId &&
                                       fl.Revision == version.Revision && fl.IsRevisionBase
                                   select fl;
            if (baseVersionQuery.Count() != 1)
            {
                if (ignoreBase)
                    return new StreamCombiner(version.Text);

                throw new Exception("Base revision not found. This can happen if the file changed type from binary to text " +
                         "as part of this change. If this is not the case, it might be a bug - report it!");
            }

            var baseVersion = baseVersionQuery.Single();

            return new StreamCombiner(baseVersion.Text, version.Text);
        }

        /// <summary>
        ///     Saves the text of a file version to a temp file.
        /// </summary>
        /// <param name="version"> The version to save. </param>
        /// <returns> The name of the file, or null if it fails. </returns>
        private TempFile SaveToTempFile(FileVersion version, bool ignoreBase = false)
        {
            var reader = GetFileStream(version, ignoreBase);
            if (reader == null)
                return null;

            var file = new TempFile();

            using (reader)
            using (var writer = new StreamWriter(file.FullName))
            {
                foreach (var line in reader.ReadLines())
                    writer.WriteLine(line);
            }

            // We created the temp file, but because of the security settings for %TEMP% might not have access to it.
            // Grant it explicitly.
            AddFileSecurity(file.FullName, WellKnownSidType.AuthenticatedUserSid, FileSystemRights.Read,
                            AccessControlType.Allow);
            AddFileSecurity(file.FullName, WellKnownSidType.CreatorOwnerSid, FileSystemRights.FullControl,
                            AccessControlType.Allow);

            return file;
        }

        private TempFile EmptyTempFile()
        {
            var file = new TempFile();

            using (var writer = new StreamWriter(file.FullName))
            {
                    writer.WriteLine("");
            }

            // We created the temp file, but because of the security settings for %TEMP% might not have access to it.
            // Grant it explicitly.
            AddFileSecurity(file.FullName, WellKnownSidType.AuthenticatedUserSid, FileSystemRights.Read,
                            AccessControlType.Allow);
            AddFileSecurity(file.FullName, WellKnownSidType.CreatorOwnerSid, FileSystemRights.FullControl,
                            AccessControlType.Allow);

            return file;
        }

        public void GenerateDiffInfoFile(string clName, FileVersion left, FileVersion right, int baseReviewId)
        {
            var js = new JavaScriptSerializer();
            var jsonFile = string.Format(@"{0}\{1}\{2}\{3}.{4}.json", _diffFolderRoot, clName, left.FileId, left.Id, right.Id);
            var json = JsonConvert.SerializeObject(new DiffFileDto(left, right, clName, baseReviewId), Formatting.Indented);
            File.WriteAllText(jsonFile, json);
        }

        /// <summary>
        ///     Computes and displays the diff between two distinct versions. Uses unix "diff" command to actually
        ///     produce the diff. This is relatively slow operation and involves spooling the data into two temp
        ///     files and running an external process.
        /// </summary>
        public string GenerateDiffFile(FileVersion left, FileVersion right, string clName, int baseReviewId, UserSettingsDto settings, bool force = false)
        {
            string outputFile = null;
            if (settings.diff.DefaultSettings)
            {
                outputFile = string.Format(@"{0}\{1}\{2}\{3}.{4}.htm", _diffFolderRoot, clName, left.FileId, left.Id, right.Id);
                if (File.Exists(outputFile))
                {
                    if (!force)
                        return outputFile;
                    File.Delete(outputFile);
                }
            }

            var useLeft = true;
            var useRight = true;
            var ignoreBase = false;

            if (left.Id == right.Id)
            {
                ignoreBase = true;
                var actionType = (SourceControlAction)Enum.ToObject(typeof(SourceControlAction), left.Action);
                switch (actionType)
                {
                    case SourceControlAction.Add:
                    case SourceControlAction.Branch:
                    case SourceControlAction.Edit:
                    case SourceControlAction.Integrate:
                    case SourceControlAction.Rename:
                    default:
                        useLeft = false;
                        break;

                    case SourceControlAction.Delete:
                        useRight = false;
                        break;
                }
            }

            using (var leftFile = useLeft ? SaveToTempFile(left, ignoreBase) : EmptyTempFile())
            using (var rightFile = useRight ? SaveToTempFile(right, ignoreBase) : EmptyTempFile())
            {
                if (leftFile == null || rightFile == null)
                    throw new ApplicationException(string.Format("Could not save temporary file for diffs. change list: {0} left file id: {1} right file id: {2}", clName, left.FileId, right.FileId));

                string stderr = null;
                string result = null;

                if (ConfigurationManager.AppSettings.LookupValue("preDiff", false) && settings.diff.preDiff)
                {
                    PreDiffFile(leftFile.FullName, rightFile.FullName);
                }

                result = DiffFiles(leftFile.FullName, rightFile.FullName, settings.diff.ignoreWhiteSpace, ref stderr);
                if (!string.IsNullOrEmpty(stderr))
                {
                    ErrorOut("Diff failed.");
                    ErrorOut(stderr);

                    return "Error Failed to generate diff. " + stderr;
                }

                using (var leftStream = new StreamCombiner(new StreamReader(leftFile.FullName)))
                using (var rightStream = new StreamCombiner(new StreamReader(rightFile.FullName)))
                using (var rawDiffStream = new StreamCombiner(result))
                {
                    var leftName = string.Format("{0}_{1}", Path.GetFileName(left.ChangeFile.ServerFileName), left.Id);
                    var rightName = string.Format("{0}_{1}", Path.GetFileName(right.ChangeFile.ServerFileName),
                                                  right.Id);

                    if (string.IsNullOrEmpty(outputFile))
                        outputFile = string.Format(@"{0}\{1}\{2}\{3}.{4}.delete.{5}.htm", _diffFolderRoot, clName, left.FileId, left.Id, right.Id, this._random.Next());

                    GenerateFileDiffView(left.ReviewRevision, right.ReviewRevision, settings,
                                                leftStream, left.Id, leftName,
                                                rightStream, right.Id, rightName,
                                                rawDiffStream, clName, outputFile);

                    GenerateDiffInfoFile(clName, left, right, baseReviewId);

                    return outputFile;
                }
            }
        }

        private string DiffFiles(string leftFile, string rightFile, bool ignoreWhiteSpaces, ref string stderr)
        {
            string result;
            var args = leftFile + " " + rightFile;
            if (ignoreWhiteSpaces && !string.IsNullOrEmpty(_diffArgsIgnoreWhiteSpace))
                args = _diffArgsIgnoreWhiteSpace + " " + args;

            if (!string.IsNullOrEmpty(_diffArgsBase))
                args = _diffArgsBase + " " + args;

            using (var diff = new Process())
            {
                diff.StartInfo.UseShellExecute = false;
                diff.StartInfo.RedirectStandardError = true;
                diff.StartInfo.RedirectStandardOutput = true;
                diff.StartInfo.CreateNoWindow = true;
                diff.StartInfo.FileName = _diffExe;
                diff.StartInfo.Arguments = args;
                diff.Start();

                result = CommonUtils.ReadProcessOutput(diff, false, out stderr);
            }
            return result;
        }

        /// <summary>
        /// Uses windiff to pre-diff the file to get a nicer looking result
        /// NOTE: Need to ignore blank lines in windiff, or will get a prompt that causes a stop.
        /// </summary>
        /// <param name="baseFile"></param>
        /// <param name="diffFile"></param>
        private void PreDiffFile(string baseFile, string diffFile)
        {
            var preDiffOutputFile = Path.GetTempFileName();
            using (var diff = new Process())
            {
                var preDiffArgs = "";
                if (!string.IsNullOrEmpty(_preDiffArgsBase))
                    preDiffArgs = _preDiffArgsBase + " " + preDiffArgs;
                preDiffArgs += preDiffOutputFile + " " + baseFile + " " + diffFile;

                diff.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                diff.StartInfo.UseShellExecute = false;
                diff.StartInfo.RedirectStandardError = true;
                diff.StartInfo.RedirectStandardOutput = true;
                diff.StartInfo.CreateNoWindow = true;
                diff.StartInfo.FileName = _preDiffExe;
                diff.StartInfo.Arguments = preDiffArgs;
                diff.Start();

                string stderr;
                CommonUtils.ReadProcessOutput(diff, false, out stderr);
            }

            var leftLines = new List<string>();
            var rightLines = new List<string>();

            var lines = File.ReadAllLines(preDiffOutputFile);
            var idxLast = lines.Length - 2;
            for (var idx = 1; idx < idxLast; idx++)
            {
                var line = lines[idx].Substring(4);
                var prefix = lines[idx].Substring(0, 4);
                switch (prefix)
                {
                    case "    ": // both line
                        leftLines.Add(line);
                        rightLines.Add(line);
                        break;

                    case " <! ": // left line
                    case " <- ": // moved
                        leftLines.Add(line);
                        break;

                    case " !> ": // right line
                    case " -> ": // moved
                        rightLines.Add(line);
                        break;

                    default:
                        throw new Exception(string.Format("Unknown diff prefix: {0}", lines[idx]));
                }
            }

            File.Delete(baseFile);
            File.Delete(diffFile);
            File.WriteAllLines(baseFile, leftLines);
            File.WriteAllLines(diffFile, rightLines);
            File.Delete(preDiffOutputFile);
        }

        private DiffType DiffTypeFromPrefix(string prefix)
        {
            switch (prefix)
            {
                case "    ":
                    return DiffType.Unchanged;

                case " <! ": // left line
                case " <- ": // moved
                    return DiffType.Deleted;

                case " !> ": // right line
                case " -> ": // moved
                    return DiffType.Added;

                default:
                    return DiffType.None;
            }
        }
        class DiffInfo
        {
            public DiffType diffType;
            public int baseLine;
            public int baseEnd;
            public int diffLine;
            public int diffEnd;
            public int idxStart;
            public int idxEnd;
            public int baseCount;
            public int diffCount;
            public string text;

            private static readonly Regex _decoder = new Regex(@"^([0-9]+)(,[0-9]+)?([a,d,c])([0-9]+)?(,[0-9]+)?$");

            public int baseLineEnd
            {
                get
                {
                    return baseLine + baseCount - 1; // TODO: figure out off by 1
                }
            }


            public int diffLineEnd
            {
                get
                {
                    return diffLine + diffCount - 1; // TODO: figure out off by 1
                }
            }

            public static DiffInfo Parse(string text, string[] lines, int idx)
            {
                var m = _decoder.Match(text);
                if (!m.Success)
                    return null;

                var offsetBase = 0;
                var offsetDiff = 0;

                var line = Int32.Parse(m.Groups[1].Value);
                // 'a' adds AFTER the line, but we do processing once we get to the line.
                // So we need to really get to the next line.
                var type = m.Groups[3].Value;
                var info = new DiffInfo();
                switch (type)
                {
                    case "a":
                        // in add case "141a142,183", 
                        //      baseLine is everything before change
                        //      diffLine is first line of change
                        info.diffType = DiffType.Added;
                        offsetBase = 1;
                        break;

                    case "d":
                        info.diffType = DiffType.Deleted;
                        break;

                    case "c":
                        info.diffType = DiffType.Changed;
                        break;

                    default:
                        throw new Exception(string.Format("Unhandled diff decoding: {0}", text));
                }

                info.text = text;
                info.baseLine = int.Parse(m.Groups[1].Value) + offsetBase;
                info.diffLine = int.Parse(m.Groups[4].Value) + offsetDiff;

                if (!string.IsNullOrEmpty(m.Groups[2].Value))
                    info.baseEnd = int.Parse(m.Groups[2].Value.Replace(",", ""));
                if (!string.IsNullOrEmpty(m.Groups[5].Value))
                    info.diffEnd = int.Parse(m.Groups[5].Value.Replace(",",""));

                info.idxStart = idx;
                switch (info.diffType)
                {
                    case DiffType.Added:
                        while (true)
                        {
                            idx++;
                            if (idx >= lines.Length || _decoder.Match(lines[idx]).Success)
                            {
                                info.idxEnd = idx - 1;
                                break;
                            }
                            else
                            {
                                info.diffCount++;
                            }
                        }
                        break;

                    case DiffType.Deleted:
                        while (true)
                        {
                            idx++;
                            if (idx >= lines.Length || _decoder.Match(lines[idx]).Success)
                            {
                                info.idxEnd = idx - 1;
                                break;
                            }
                            else
                            {
                                info.baseCount++;
                            }
                        }
                        break;

                    case DiffType.Changed:
                        while (true)
                        {
                            idx++;
                            if (idx >= lines.Length || lines[idx] == "---")
                            {
                                break;
                            }
                            else
                            {
                                info.baseCount++;
                            }
                        }
                        while (true)
                        {
                            idx++;
                            if (idx >= lines.Length || _decoder.Match(lines[idx]).Success)
                            {
                                info.idxEnd = idx - 1;
                                break;
                            }
                            else
                            {
                                info.diffCount++;
                            }
                        }
                        break;
                }

                return info;
            }

            public static List<DiffInfo> Parse(string[] lines)
            {
                var results = new List<DiffInfo>();
                for (var idx = 0; idx < lines.Length; )
                {
                    var diffInfo = DiffInfo.Parse(lines[idx], lines, idx);
                    if (diffInfo == null) {
                        idx++;
                        continue;
                    }

                    results.Add(diffInfo);
                    idx = diffInfo.idxEnd + 1;
                }
                return results;
            }
        }

        private static string IsEscape(string input)
        {
            var re = new Regex("(&#[0-9]{2,5};)|(&[a-zA-Z]{2,10};).*");
            var match = re.Match(input);
            if (match.Success)
            {
                //Log.Info(match.Groups[1].Value + match.Groups[2].Value); // TODO

                if (!string.IsNullOrWhiteSpace(match.Groups[1].Value))
                {
                    return match.Groups[1].Value;
                }

                if (!string.IsNullOrWhiteSpace(match.Groups[2].Value))
                {
                    return match.Groups[2].Value;
                }
            }
            return null;
        }

        private void InterlineDiff(LineDiffInfo baseLineInfo, LineDiffInfo diffLineInfo, bool ignoreWhiteSpaces = false)
        {
            string result;
            using (var diff = new Process())
            {
                var diffArgs = "";
                if (!string.IsNullOrEmpty(_diffArgsBase))
                    diffArgs = _diffArgsBase + " " + diffArgs;
                if (ignoreWhiteSpaces && !string.IsNullOrEmpty(_diffArgsIgnoreWhiteSpace))
                    diffArgs +=  " " + _diffArgsIgnoreWhiteSpace;
                diffArgs += " " + baseLineInfo.TempFileName + " " + diffLineInfo.TempFileName;

                diff.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                diff.StartInfo.UseShellExecute = false;
                diff.StartInfo.RedirectStandardError = true;
                diff.StartInfo.RedirectStandardOutput = true;
                diff.StartInfo.CreateNoWindow = true;
                diff.StartInfo.FileName = _diffExe;
                diff.StartInfo.Arguments = diffArgs;
                diff.Start();

                string stderr;
                result = CommonUtils.ReadProcessOutput(diff, false, out stderr);
            }

            var lines = result.Replace("\r", "").Replace("> ", "").Replace("< ", "").Split(new char[] { '\n' });

            var diffInfos = DiffInfo.Parse(lines);
            foreach (var diffInfo in diffInfos)
            {
                // reset diffType to before change
                baseLineInfo.Add(DiffType.None);
                diffLineInfo.Add(DiffType.None);

                // Add lines before diffInfo starts

                while (!baseLineInfo.Done &&
                    (diffInfo == null || baseLineInfo.Position < diffInfo.baseLine - 1))
                {
                    baseLineInfo.Next();
                }

                while (!diffLineInfo.Done &&
                    (diffInfo == null || diffLineInfo.Position < diffInfo.diffLine - 1))
                {
                    diffLineInfo.Next();
                }

                // start diffType
                switch (diffInfo.diffType)
                {
                    case DiffType.Deleted: // left line
                        baseLineInfo.Add(diffInfo.diffType);
                        break;

                    case DiffType.Added: // right line
                        diffLineInfo.Add(diffInfo.diffType);
                        break;

                    case DiffType.Changed: // left line
                        baseLineInfo.Add(diffInfo.diffType);
                        diffLineInfo.Add(diffInfo.diffType);
                        break;

                    default:
                        throw new Exception(string.Format("Unknown diff prefix: {0}", diffInfo.baseLine));
                }

                // Add lines in diffInfo range

                while (!baseLineInfo.Done &&
                    (diffInfo == null || baseLineInfo.Position < diffInfo.baseLineEnd))
                {
                    baseLineInfo.Next();
                }

                while (!diffLineInfo.Done &&
                    (diffInfo == null || diffLineInfo.Position < diffInfo.diffLineEnd))
                {
                    diffLineInfo.Next();
                }
            }

            baseLineInfo.AddLast();
            diffLineInfo.AddLast();
        }

        public class LineDiffInfo
        {
            public string TempFileName = Path.GetTempFileName();
            public List<string> Lines = new List<string>();
            public List<int> LinePositions = new List<int>();
            public List<Dictionary<int, string>> Escapes = new List<Dictionary<int, string>>();
            public LineDiffInfo BaseInfo { get; set; }
            public bool BaseMode { get; set; }

            public int Line = 0;
            public int Col = 0;
            public int Position = 0;
            public string Current = "";

            public string Chars = "";
            public int Length = 0;
            public DiffType DiffType = DiffType.None;

            public LineDiffInfo(HtmlTableRowGroup rowGroup, LineDiffInfo baseInfo = null)
            {
                BaseInfo = baseInfo ?? this;
                BaseMode = baseInfo == null;

                var charsList = new List<string>();
                foreach (var item in rowGroup.Lines)
                {
                    var line = BaseMode ? item.Left.Code : item.Right.Code;
                    line = line ?? "";
                    var lineEscapes = UnEncodeLine(ref line);
                    Escapes.Add(lineEscapes);
                    Chars += line;

                    var chars = line.ToCharArray();
                    charsList.AddRange(chars.Select(ch => ch.ToString(CultureInfo.InvariantCulture)));

                    LinePositions.Add(line.Length);
                }
                Length = Chars.Length;

                File.WriteAllLines(TempFileName, charsList);
            }

            public void Dispose()
            {
                File.Delete(TempFileName);
                TempFileName = null;
            }

            public void DecodeEscapes()
            {
                if (Line < Escapes.Count)
                {
                    if (Escapes[Line].ContainsKey(Col))
                    {
                        var escape = Escapes[Line][Col];
                        Current += escape;
                        //Escapes[Line].Remove(Col); - TODO: Remo
                    }
                }
            }

            public void AdvanceLine()
            {
                while (Line < LinePositions.Count && LinePositions[Line] == Col)
                {
                    Lines.Add(Current);
                    Current = "";
                    Add(DiffType);
                    Col = 0;
                    Line++;

                    // TODO: Would get a better diff if replaced certain escapes with their real content
                    DecodeEscapes();
                }
            }

            public void AddLast()
            {
                while (Next())
                {

                }

                AdvanceLine();
                if (!string.IsNullOrEmpty(Current))
                {
                    Lines.Add(Current);
                    Current = "";
                    Col = 0;
                    Line++;
                }
                AdvanceLine();
            }

            public bool Next()
            {
                if (Position < Length)
                {
                    // Handle Position 0 line
                    AdvanceLine();
                    
                    Current += Chars[Position];
                    Position++;
                    Col++;

                    // TODO: Would get a better diff if replaced certain escapes with their real content
                    DecodeEscapes();
                    return true;
                }
                return false;
            }

            public bool Done
            {
                get
                {
                    return Position >= Length;
                }
            }

            public void Add(string value)
            {
                Current += value;
            }

            public void Add(DiffType diffType)
            {
                if (diffType == DiffType.None)
                {
                    if (DiffType != diffType)
                        Current += "</pre><pre>";
                }
                else
                {
                    Current += "</pre><pre class='" + diffType.ToString() + "'>";
                }
                DiffType = diffType;
            }
        }

        private HtmlTableLine fakeSeperatorLine = null;
        private int fakeSeperatorCount = 0;
        private int fakeSeperatorLineNumber = 0;

        private HtmlTableLine GetEmptyLine()
        {
            if (this.fakeSeperatorLine == null)
                this.fakeSeperatorLine = new HtmlTableLine();
            var line = this.fakeSeperatorLine;

            var id = this.fakeSeperatorCount++;
            line.Left.Id = id.ToString();
            line.Left.LineNum = id.ToString();
            line.Right.Id = id.ToString();
            line.Right.LineNum = id.ToString();
            return line;
        }

        private void AddSeperators(HtmlTableRowGroup rowGroup,
            HtmlTable left,
            HtmlTable right,
            HtmlTable edge)
        {
            // Unchanged seperator
            left.AddBody("Seperator Base");
            right.AddBody("Seperator Diff");
            edge.AddBody("Seperator Diff");

            var startLine = rowGroup.Lines.Count > 0 ? rowGroup.Lines.First() : GetEmptyLine();
            var endLine = rowGroup.Lines.Count > 0 ? rowGroup.Lines.Last() : GetEmptyLine();

            left.AddBodySeperator(startLine.Left.LineNum, startLine.Left.Id, string.Format("Lines {0} to {1}", startLine.Left.LineNumber, endLine.Left.LineNumber));
            right.AddBodySeperator(startLine.Right.LineNum, startLine.Right.Id, string.Format("Lines {0} to {1}", startLine.Right.LineNumber, endLine.Right.LineNumber));
            edge.AddEdgeSeperator(startLine.Right.LineNum, startLine.Right.Id);

            left.EndBody();
            right.EndBody();
            edge.EndBody();
        }

        private IEnumerable<string> EncodeRowGroups(List<HtmlTableRowGroup> rowGroups, string baseHeader, string diffHeader, string leftIdPrefix, string rightIdPrefix, string edgePrefix)
        {
            var left = new HtmlTable(leftIdPrefix, leftIdPrefix, "LeftTable");
            var right = new HtmlTable(rightIdPrefix, rightIdPrefix, "RightTable");
            var edge = new HtmlTable(edgePrefix, edgePrefix, "EdgeTable");
            left.AddDiffHeader();
            right.AddDiffHeader();

            foreach (var rowGroup in rowGroups)
            {
                left.AddBody(rowGroup.Class + " Base");
                right.AddBody(rowGroup.Class + " Diff");
                edge.AddBody(rowGroup.Class + " Diff");

                foreach (var line in rowGroup.Lines)
                {
                    var id = line.Left.Id + "-" + line.Right.Id;
                    left.AddCodeLine(line.Left.LineNumber, id, line.Left.Code);
                    right.AddCodeLine(line.Right.LineNumber, id, line.Right.Code);
                    edge.AddEdgeLine(line.Right.LineNumber, id);
                }

                left.EndBody();
                right.EndBody();
                edge.EndBody();

                // Add Seperator line after every unchanged
                if (rowGroup.DiffType == DiffType.Unchanged)
                    AddSeperators(rowGroup, left, right, edge);
            }

            left.End();
            right.End();
            edge.End();
            var testMode = false;
            var containerTable = new HtmlTable("", "DiffView", "DiffTable", testMode);
            containerTable.AddContainerHeader("ContainerTable", baseHeader, diffHeader);
            containerTable.AddTables(left, right, edge);
            containerTable.End(testMode);

            return containerTable.Html;
        }

        public static string EncodeLinePrefix(int leftRevision, int rightRevision, int leftId, int rightId, int fileId)
        {
            return string.Format("{0}-{1}-{2}-{3}", rightRevision, leftId, rightId, fileId);
        }

        private static Dictionary<int, string> UnEncodeLine(ref string line)
        {
            var escapeMap = new Dictionary<int, string>();
            var output = "";
            var found = line.IndexOf('&');
            var idxLast = 0;
            while (found != -1)
            {
                var escape = IsEscape(line.Substring(found));
                if (escape != null)
                {
                    output += line.Substring(idxLast, found - idxLast);
#if TODO // Remo
                    escapeMap.Add(found, escape);
#endif
                    if (!escapeMap.ContainsKey(output.Length))
                        escapeMap.Add(output.Length, escape);
                    else
                        escapeMap[output.Length] += escape;
                    idxLast = found + escape.Length;
                }
                else
                {
                    output += line.Substring(idxLast, found - idxLast);
                    idxLast = found;
                }
                found = line.IndexOf('&', found + 1);
            }
            output += line.Substring(idxLast);

            line = output;
            return escapeMap;
        }

        private void InterlineDiff(HtmlTableRowGroup rowGroup, bool ignoreWhiteSpaces)
        {
            var baseLineInfo = new LineDiffInfo(rowGroup);
            var diffLineInfo = new LineDiffInfo(rowGroup, baseLineInfo);
            InterlineDiff(baseLineInfo, diffLineInfo, ignoreWhiteSpaces);

            UnPackDiffedLine(rowGroup, baseLineInfo.Lines, diffLineInfo.Lines);

            baseLineInfo.Dispose();
            diffLineInfo.Dispose();
        }

        private void UnPackDiffedLine(HtmlTableRowGroup rowGroup, List<string> leftLines, List<string> rightLines)
        {
            if (leftLines.Count < rowGroup.Lines.Count || rightLines.Count < rowGroup.Lines.Count)
            {
                Log.Error("Interline Diff error: line count mismatch: {0} != {1}", leftLines.Count, rightLines.Count);
                return;
            }

            for (var idx = 0; idx < rowGroup.Lines.Count; idx++)
            {
                var line = rowGroup.Lines[idx];

                var value = leftLines[idx];
                if (value != line.Left.LineText && line.Left.LineText != null)
                {
                    var newLine = new Line { Id = line.Left.Id, LineNum = line.Left.LineNum, LineText = value };
                    line.Left = newLine;
                }

                value = rightLines[idx];
                if (value != line.Right.LineText && line.Right.LineText != null)
                {
                    var newLine = new Line { Id = line.Right.Id, LineNum = line.Right.LineNum, LineText = value };
                    line.Right = newLine;
                }
            }
        }

        /// <summary>
        ///     Generates the diff view for two file revisions.
        /// </summary>
        private string GenerateFileDiffView(
            int baseRevision, int diffRevision, UserSettingsDto settings,
            StreamCombiner baseFile, int baseId, string baseHeader,
            StreamCombiner diffFile, int diffId, string diffHeader,
            StreamCombiner rawDiff, string fileName, string outputFile)
        {
            var baseEncoder = GetEncoderForFile(fileName);
            var diffEncoder = GetEncoderForFile(fileName);

            var baseFileInfo = new DiffFileInfo(baseFile, baseEncoder, baseId, BaseOrDiff.Base);
            var diffFileInfo = new DiffFileInfo(diffFile, diffEncoder, diffId, BaseOrDiff.Diff);

            // Line Stamp format
            var baseScriptIdPrefix = "Base-" + EncodeLinePrefix(baseRevision, diffRevision, baseId, diffId, baseId);
            var diffScriptIdPrefix = "Diff-" + EncodeLinePrefix(baseRevision, diffRevision, baseId, diffId, diffId);
            var edgePrefix = "Edge-" + EncodeLinePrefix(baseRevision, diffRevision, baseId, diffId, diffId);
            var rowGroups = new List<HtmlTableRowGroup>();
            var lastBaseLine = "1";
            var lastDiffLine = "1";

            foreach (var diffItem in DiffItem.EnumerateDifferences(rawDiff))
            {
                var atEnd = diffItem.BaseLineCount == int.MaxValue;

                var baseLines = new List<Line>();
                for (var i = 0; i < diffItem.BaseLineCount && baseFileInfo.MoveNextLine(); ++i)
                    baseLines.Add(new Line
                        {
                            LineNum = baseFileInfo.CurLineNum.ToString(CultureInfo.InvariantCulture),
                            LineText = baseFileInfo.CurLine
                        });

                var diffLines = new List<Line>();
                for (var i = 0; i < diffItem.DiffLineCount && diffFileInfo.MoveNextLine(); ++i)
                    diffLines.Add(new Line
                        {
                            LineNum = diffFileInfo.CurLineNum.ToString(CultureInfo.InvariantCulture),
                            LineText = diffFileInfo.CurLine
                        });

                var baseLinesLength = baseLines.Count();
                var diffLinesLength = diffLines.Count();

                // The end is the only case where the DiffInfo line counts may be incorrect. If there are in fact
                // zero lines then just continue, which should cause the foreach block to end and we'll continue
                // like the DiffItem never existed.
                if (atEnd && diffItem.DiffType == DiffType.Unchanged && baseLinesLength == 0 && diffLinesLength == 0)
                    continue;

                var rowGroup = new HtmlTableRowGroup(diffItem.DiffType);

                for (var i = 0; i < Math.Max(baseLinesLength, diffLinesLength); ++i)
                {
                    var line = new HtmlTableLine();
                    if (i < baseLinesLength)
                    {
                        line.Left = baseLines[i];
                        line.Left.LineText = baseFileInfo.Encoder.EncodeLine(baseLines[i].LineText, int.MaxValue,
                                                                             TabValue);
                        lastBaseLine = line.Left.LineNum;
                    }
                    else
                    {
                        // new diff code - add entry for empty line with old line #
                        var idx = i - baseLinesLength;
                        line.Left = new Line { Id = lastBaseLine + '_' + idx };
                    }

                    if (i < diffLinesLength)
                    {
                        line.Right = diffLines[i];
                        line.Right.LineText = diffFileInfo.Encoder.EncodeLine(diffLines[i].LineText, int.MaxValue,
                                                                             TabValue);
                        lastDiffLine = line.Right.LineNum;
                    }
                    else
                    {
                        // new diff code - add entry for empty line with old line #
                        var idx = i - diffLinesLength;
                        line.Right = new Line { Id = lastDiffLine + '_' + idx };
                    }

                    rowGroup.Lines.Add(line);
                }

                if (diffItem.DiffType == DiffType.Changed)
                {
                    if (settings.diff.intraLineDiff)
                        InterlineDiff(rowGroup, settings.diff.ignoreWhiteSpace);
                }

                rowGroups.Add(rowGroup);
            }

            baseEncoder.Dispose();
            diffEncoder.Dispose();

            var htmlLines = EncodeRowGroups(rowGroups, baseHeader, diffHeader, baseScriptIdPrefix, diffScriptIdPrefix,
                                       edgePrefix);
            if (!string.IsNullOrWhiteSpace(outputFile))
            {
                var folder = Path.GetDirectoryName(outputFile);
                Directory.CreateDirectory(folder);
                File.WriteAllLines(outputFile, htmlLines);
                Log.Info("Generated {0}", outputFile);
                return outputFile;
            }
            else
            {
                return string.Join(Environment.NewLine, htmlLines);
            }
        }

        /// <summary>
        ///     Returns an encoder for a given file name.
        /// </summary>
        private static ILineEncoder GetEncoderForFile(string fileName)
        {
            ILineEncoder encoder = null;

            var extIndex = fileName.LastIndexOf('.');
            if (extIndex != -1)
            {
                var ext = fileName.Substring(extIndex + 1).ToLowerCultureInvariant();
                var highlighterPath = ConfigurationManager.AppSettings["encoder_" + ext];
                if (highlighterPath != null)
                {
                    var encoderAssem = Assembly.LoadFrom(highlighterPath);
                    var encoderType = encoderAssem.GetType("LineEncoderFactory");
                    var factory = (ILineEncoderFactory)Activator.CreateInstance(encoderType);
                    encoder = factory.GetLineEncoder(ext);
                }
            }

            if (encoder == null)
                encoder = new DefaultEncoder();

            return encoder;
        }

        public class HtmlTable
        {
            private const string HtmlHeader =
                @"<html>
    <head>
        <style>
            .Col0, .Col2
            {
                width: 20px;
            }
            .body
            {
            }
            table
            {
                background-color: white;
                border-collapse: collapse;
                border: 0px solid grey;
                table-layout: fixed;
                white-space: nowrap;

                width: 100%;
            }
            .LeftTable, .RightTable
            {
                overflow-x: scroll;
                overflow-y: visible;
                border: 1px solid grey;
             }
        </style>
    </head>
    <body>
";

            private const string DiffHeader =
                @"    <colgroup>
        <col align='left' class='LineNumberCol'/>
        <col align='left' class='SourceCodeCol'/>
    </colgroup>";

            private const string ContainerHeader =
                @"    <colgroup>
        <col align='left' class='BaseColumn'/>
        <col align='left' class='DiffColumn'/>
        <col align='left' class='EdgeColumn'/>
    </colgroup>";

            private const string TableThHeader = @"
    <thead>
        <tr>
            <th>&nbsp;</th>
            <th>&nbsp;</th>
            <th/>
        </tr>
    </thead>";

            //private const string TableRowGroup = @"<tbody class='RowGroup {0}'>";

            private const string IndentAdd = "    ";
            private const int IndentLength = 4;

            public List<string> Html = new List<string>();
            private readonly string _idPrefix = "";
            private string _indent = "";

            private int _lineNumber = 1;
            private int _rowGroupNumber = 1;

            public HtmlTable(string id, string idPrefix, string className, bool testMode = false)
            {
                _idPrefix = idPrefix;

                if (testMode)
                    AddHtml("{0}", HtmlHeader);

                AddHtml("<div class='{0}'>", className);
                Indent();
                id = !string.IsNullOrEmpty(id) ? string.Format("id = '{0}'", id) : "";
                AddHtml("<table {0} class='{1}'>", id, className);
                Indent();
            }

            public void AddContainerHeader(string cls, string baseName, string diffName)
            {
                AddHtml(ContainerHeader, baseName, diffName);
            }

            public void AddTables(HtmlTable left, HtmlTable right, HtmlTable edge)
            {
                AddHtml("<tr>");
                {
                    AddHtml("<td>");
                    Html.AddRange(left.Html);
                    AddHtml("</td>");

                    AddHtml("<td>");
                    Html.AddRange(right.Html);
                    AddHtml("</td>");

                    AddHtml("<td>");
#if OriginalEdgeTable
                    Html.AddRange(edge.Html);
#else
                    AddHtml("<div class='EdgeTable'>&nbsp;<div></div></div>");
#endif
                    AddHtml("</td>");
                }
                AddHtml("</tr>");
            }

            public void AddDiffHeader()
            {
                AddHtml(DiffHeader);
            }

            public void AddBody(string cls)
            {
                AddHtml(@"<tbody class='RowGroup {0}'>", cls);
                Indent();
            }

            private string Id(string tag, string id)
            {
                return _idPrefix + tag + id;
            }

            private string Id(string id)
            {
                return _idPrefix + "-" + id;
            }

            private string Id(string tag, int id)
            {
                return _idPrefix + tag + id;
            }

            public void AddRow(string cls)
            {
                AddHtml("<tr class='{0}'>", cls);
                Indent();
            }

            public void AddColumn(string cls, string contents)
            {
                AddHtml("<td id='{0}' class='{1}'>" + contents + "</td>", Id("Td", _lineNumber), cls);
            }

            public void AddCodeLine(string lineNumber, string id, string code)
            {
                var td = string.Format("<tr id='Row_{1}'><td class='Num'>{0}</td><td id='{1}' class='Code'><pre>", lineNumber, Id(id));
                AddHtml(td + code + "</pre></td></tr>");
            }

            public void AddEdgeLine(string lineNumber, string id)
            {
                AddHtml("<tr id='Row_{0}'><td id='{0}' class='Edge'><pre>&nbsp;</pre></td></tr>", Id(id));
            }

            public void AddBodySeperator(string lineNumber, string id, string code = "&nbsp;")
            {
                AddHtml("<tr id='Row_Sep_{0}'><td class='Num Seperator'>&nbsp;</td><td id='Sep_{0}' class='Code Seperator'><input class='Seperator' type='button' value='{1}'/></td></tr>", Id(id), code);
            }

            public void AddEdgeSeperator(string lineNumber, string id)
            {
                AddHtml("<tr id='Row_Sep_Edge_{0}'><td id='Sep_Edge_{0}' class='Edge Seperator'><pre>&nbsp;</pre></td></tr>", Id(id));
            }

            public void EndBody()
            {
                Dedent();
                AddHtml("</tbody>");
                _rowGroupNumber++;
            }

            public void EndRow()
            {
                Dedent();
                AddHtml("</tr>");
                _lineNumber++;
            }

            public void End(bool testMode = false)
            {
                Dedent();
                AddHtml("</table>");
                Dedent();
                AddHtml("</div>");
                if (testMode)
                    AddHtml("</body></html>");
            }

            private void Dedent()
            {
                _indent = _indent.Substring(0, Math.Max(0, _indent.Length - IndentLength));
            }

            private void Indent()
            {
                _indent += IndentAdd;
            }

            private void AddHtml(string format, params object[] args)
            {
                Html.Add(_indent + string.Format(format, args));
            }

            private void AddHtml(string html)
            {
                Html.Add(_indent + html);
            }
        }

        public class HtmlTableLine
        {
            public Line Left;
            public Line Right;
        }

        public class HtmlTableRowGroup
        {
            public string Class { get; set; }

            public List<HtmlTableLine> Lines { get; set; }

            public DiffType DiffType;

            public HtmlTableRowGroup(DiffType diffType)
            {
                Class = diffType.ToString();
                DiffType = diffType;
                Lines = new List<HtmlTableLine>();
            }
        }

        public struct Line
        {
            private string _id;
            public string LineNum;
            public string LineText;

            public string LineNumber
            {
                get { return (!string.IsNullOrEmpty(LineNum)) ? LineNum : ""; }
            }

            public string Id
            {
                get { return (!string.IsNullOrEmpty(_id)) ? _id : LineNum; }
                set { _id = value; }
            }

            public string Code
            {
                get { return (!string.IsNullOrEmpty(LineText)) ? LineText : "&nbsp;"; }
            }
        }
    }

    // class

    /// <summary>
    ///     Implements a TextReader-like class that (only two methods though: ReadLine and Close)
    ///     that wraps around either a TextReader, or a bunch of diffs.
    /// </summary>
    //@TODO: Would be nice to have this behave like a forward iteration stream, could be used
    //       with .NET stream reader classes.
    public sealed class StreamCombiner : IDisposable
    {
        // The base text reader.
        private readonly TextReader BaseTextReader;

        private readonly Regex DiffDecoder = new Regex(@"^([0-9]+)(,[0-9]+)?([a,d,c]).*$");

        // The diff reader or null if we're working with a single file.
        private readonly TextReader DiffTextReader;

        private bool InDiffArea;

        // The line we're on in the base file.
        private int LineBase;

        // The line where the next section of diff starts.
        private int NextLine;

        // We are inside the diff area.

        /// <summary>
        ///     Constructor for file and a diff case. This is where the meat of this class is used.
        /// </summary>
        /// <param name="baseText"> The base of the file. </param>
        /// <param name="diffText"> The diff. </param>
        public StreamCombiner(string baseText, string diffText)
        {
            BaseTextReader = new StringReader(baseText == null ? "" : baseText);
            DiffTextReader = new StringReader(diffText == null ? "" : diffText);
            LineBase = 1;
            //InDiffArea = false;
            var DiffLine = DiffTextReader.ReadLine();
            NextLine = ParseDiffLine(DiffLine);
        }

        /// <summary>
        ///     The wrapper case - just wraps a string.
        /// </summary>
        /// <param name="baseText"> The string to wrap. </param>
        public StreamCombiner(string baseText)
        {
            BaseTextReader = new StringReader(baseText == null ? "" : baseText);
            //DiffTextReader = null;
            LineBase = 1;
            NextLine = -1;
            //InDiffArea = false;
        }

        /// <summary>
        ///     The wrapper case - just wraps a TextReader. Will Close it on Close. Nothing intelligent.
        /// </summary>
        /// <param name="baseReader"> TextReader to wrap. </param>
        public StreamCombiner(TextReader baseReader)
        {
            BaseTextReader = baseReader;
            DiffTextReader = null;
            LineBase = 1;
            NextLine = -1;
            //InDiffArea = false;
        }

        /// <summary>
        ///     Closes the stream.
        /// </summary>
        void IDisposable.Dispose()
        {
            Close();
        }

        /// <summary>
        ///     Parses a line from the diff file which specifies the next line where the next diff section starts.
        /// </summary>
        /// <param name="text"> The line to parse. </param>
        /// <returns>
        ///     The line number, or -1 if the line is not a section header. Usually this means that the diff file
        ///     has ended.
        /// </returns>
        private int ParseDiffLine(string text)
        {
            if (text == null)
                return -1;

            var m = DiffDecoder.Match(text);
            if (m.Success)
            {
                var line = Int32.Parse(m.Groups[1].Value);
                // 'a' adds AFTER the line, but we do processing once we get to the line.
                // So we need to really get to the next line.
                if (m.Groups[3].Value.Equals("a"))
                    line += 1;

                return line;
            }

            return -1;
        }

        /// <summary>
        ///     Frees all the resources.
        /// </summary>
        public void Close()
        {
            BaseTextReader.Close();
            if (DiffTextReader != null)
                DiffTextReader.Close();
        }

        /// <summary>
        ///     Read one line of input.
        /// </summary>
        /// <returns> The line it has just read, or null for eof. </returns>
        public string ReadLine()
        {
            if (LineBase == NextLine)
                InDiffArea = true;

            if (InDiffArea)
            {
                for (; ; )
                {
                    var DiffLine = DiffTextReader.ReadLine();
                    if (DiffLine == null)
                    {
                        ++NextLine;
                        return BaseTextReader.ReadLine();
                    }
                    else if (DiffLine.StartsWith("<", StringComparison.InvariantCulture))
                    {
                        ++LineBase;
                        BaseTextReader.ReadLine();
                    }
                    else if (DiffLine.StartsWith("-", StringComparison.InvariantCulture))
                    {
                        continue;
                    }
                    else if (DiffLine.StartsWith(">", StringComparison.InvariantCulture))
                    {
                        return DiffLine.Substring(2);
                    }
                    else if (DiffLine.Equals("\\ No newline at end of file", StringComparison.InvariantCulture))
                    {
                        // This is a very annoying perforce thing. But we have to account for it.
                        continue;
                    }
                    else
                    {
                        NextLine = ParseDiffLine(DiffLine);
                        InDiffArea = false;
                        return ReadLine();
                    }
                }
            }

            ++LineBase;
            return BaseTextReader.ReadLine();
        }

        /// <summary>
        ///     Used to enumerate every line in the stream until EOF.
        /// </summary>
        /// <returns>An IEnumerable that can be used to read each line sequentially.</returns>
        public IEnumerable<string> ReadLines()
        {
            for (var line = ReadLine(); line != null; line = ReadLine())
                yield return line;
            yield break;
        }
    }

    /// <summary>
    ///     A small enum to indicate the role a file is playing in a comparison.
    /// </summary>
    internal enum BaseOrDiff
    {
        Base,
        Diff
    }

    /// <summary>
    ///     Encapsulates information for a file being diff'ed.
    /// </summary>
    internal class DiffFileInfo
    {
        /// <summary>
        ///     Indicates if this file is the base or diff in the comparison.
        /// </summary>
        public BaseOrDiff BaseOrDiff;

        /// <summary>
        ///     The text of the current line.
        /// </summary>
        public string CurLine;

        /// <summary>
        ///     The line number for the current line.
        /// </summary>
        public int CurLineNum;

        /// <summary>
        ///     The encoder for displaying the file's text.
        /// </summary>
        public ILineEncoder Encoder;

        /// <summary>
        ///     The stream from which to read the file's text.
        /// </summary>
        public StreamCombiner File;

        /// <summary>
        ///     The file ID as found in the database.
        /// </summary>
        public int Id;

        /// <summary>
        ///     The ID used in HTML for javascript to use.
        /// </summary>
        public string ScriptId;

        /// <summary>
        ///     Creates a DiffFileInfo for a file being compared.
        /// </summary>
        /// <param name="file">The file stream.</param>
        /// <param name="encoder">The line encoder.</param>
        /// <param name="id">The file ID within the database.</param>
        /// <param name="baseOrDiff">What role the file plays within the comparison.</param>
        public DiffFileInfo(
            StreamCombiner file,
            ILineEncoder encoder,
            int id,
            BaseOrDiff baseOrDiff)
        {
            File = file;
            Encoder = encoder;
            Id = id;
            ScriptId = baseOrDiff.ToString().ToLowerCultureInvariant() + "_" + Id.ToString(CultureInfo.InvariantCulture) + "_";
            //CurLineNum = 0;
            //NextCommentIndex = 0;
            BaseOrDiff = baseOrDiff;
        }

        /// <summary>
        ///     Moves to the next line in the file.
        /// </summary>
        /// <returns>false if EOF is reached; true otherwise.</returns>
        public bool MoveNextLine()
        {
            var line = File.ReadLine();
            if (line != null)
            {
                CurLine = line;
                ++CurLineNum;
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    ///     Indicates the type of difference encapsulated in a DiffItem object.
    /// </summary>
    public enum DiffType
    {
        None,
        Unchanged,
        Changed,
        Added,
        Deleted,
    };

    /// <summary>
    ///     Represents a block of lines of a particular difference type.
    /// </summary>
    internal class DiffItem : ICloneable
    {
        /// <summary>
        ///     Matches the line number of the next change.
        /// </summary>
        private static readonly Regex DiffDecoder = new Regex(@"^([0-9]+)(,[0-9]+)?([a,d,c]).*$");

        /// <summary>
        ///     The number of lines removed from the base file.
        /// </summary>
        public int BaseLineCount;

        /// <summary>
        ///     The starting line number within the base file.
        /// </summary>
        public int BaseStartLineNumber;

        /// <summary>
        ///     The number of lines added to the diff file.
        /// </summary>
        public int DiffLineCount;

        /// <summary>
        ///     The type of difference represented.
        /// </summary>
        public DiffType DiffType;

        /// <summary>
        ///     Trivial constructor.
        /// </summary>
        public DiffItem()
        {
            BaseStartLineNumber = 1;
            //DiffLineCount = 0;
            //BaseLineCount = 0;
        }

        /// <summary>
        ///     Clones the object.
        /// </summary>
        public object Clone()
        {
            return new DiffItem
                {
                    BaseLineCount = BaseLineCount,
                    BaseStartLineNumber = BaseStartLineNumber,
                    DiffLineCount = DiffLineCount,
                    DiffType = DiffType,
                };
        }

        /// <summary>
        ///     Generates a sequence of DiffItems representing differences in rawDiffStream. Includes unchanged blocks.
        /// </summary>
        /// <returns>A DiffItem generator.</returns>
        public static IEnumerable<DiffItem> EnumerateDifferences(StreamCombiner rawDiffStream)
        {
            return EnumerateDifferences(rawDiffStream, true);
        }

        /// <summary>
        ///     Generates a sequence of DiffItems representing differences in rawDiffStream.
        /// </summary>
        /// <param name="includeUnchangedBlocks">
        ///     Indicates whether to generate DiffItems for unchanged blocks.
        /// </param>
        /// <returns>A DiffItem generator.</returns>
        public static IEnumerable<DiffItem> EnumerateDifferences(StreamCombiner rawDiffStream, bool includeUnchangedBlocks)
        {
            DiffItem prevItem = null;
            DiffItem item = null;
            string line = null;
            do
            {
                line = rawDiffStream == null ? null : rawDiffStream.ReadLine();

                if (line != null && line.StartsWith("<"))
                {
                    ++item.BaseLineCount;
                }
                else if (line != null && line.StartsWith("-"))
                {
                    continue;
                }
                else if (line != null && line.StartsWith(">"))
                {
                    ++item.DiffLineCount;
                }
                else if (line != null && line.Equals("\\ No newline at end of file"))
                {
                    // This is a very annoying perforce thing. But we have to account for it.
                    continue;
                }
                else
                {
                    if (item != null)
                    {
                        if (item.DiffLineCount == 0)
                            item.DiffType = DiffType.Deleted;
                        else if (item.BaseLineCount == 0)
                            item.DiffType = DiffType.Added;
                        else
                            item.DiffType = DiffType.Changed;

                        yield return item;
                        prevItem = item;
                        item = null;
                    }

                    if (line != null)
                    {
                        item = new DiffItem();

                        var m = DiffDecoder.Match(line);
                        if (!m.Success)
                            yield break;

                        item.BaseStartLineNumber = Int32.Parse(m.Groups[1].Value);

                        // 'a' adds AFTER the line, but we do processing once we get to the line.
                        // So we need to really get to the next line.
                        if (m.Groups[3].Value.Equals("a"))
                            item.BaseStartLineNumber += 1;
                    }

                    if (includeUnchangedBlocks)
                    {
                        var unchangedItem = new DiffItem();
                        unchangedItem.DiffType = DiffType.Unchanged;
                        unchangedItem.BaseStartLineNumber =
                            prevItem == null ? 1 : prevItem.BaseStartLineNumber + prevItem.BaseLineCount;
                        unchangedItem.BaseLineCount = item == null
                                                          ? int.MaxValue
                                                          : item.BaseStartLineNumber - unchangedItem.BaseStartLineNumber;
                        unchangedItem.DiffLineCount = unchangedItem.BaseLineCount;

                        if (unchangedItem.BaseLineCount != 0)
                            yield return unchangedItem;
                    }
                }
            } while (line != null);
        }
    }

    /// <summary>
    ///     Default line encoder: no syntax highlighting, etc.
    /// </summary>
    internal class DefaultEncoder : ILineEncoder
    {
        /// <summary>
        ///     Does nothing, just satisfies ILineEncoder interface.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Forwards to static EncodeLine method.
        /// </summary>
        string ILineEncoder.EncodeLine(string line, int maxLineLength, string tabSubstitute)
        {
            return EncodeLine(line, maxLineLength, tabSubstitute);
        }

        /// <summary>
        ///     Does nothing - exists just to satisfy the interface requirements.
        /// </summary>
        /// <returns></returns>
        TextReader ILineEncoder.GetEncoderCssStream()
        {
            return null;
        }

        /// <summary>
        ///     Encodes the string. Unlike standard HtmlEncode, our custom version preserves spaces correctly.
        ///     Also, converts tabs to "\t", and breaks lines in chunks of maxLineWidth non-breaking segments.
        /// </summary>
        /// <param name="s"> The string to encode. </param>
        /// <param name="maxLineWidth"> The maximum width. </param>
        /// <param name="tabValue"> Text string to replace tabs with. </param>
        /// <returns> The string which can be safely displayed in HTML. </returns>
        public static string EncodeLine(string s, int maxLineWidth, string tabValue)
        {
            if (s == null)
                return null;

            s = s.Replace("\t", tabValue);

            var sb = new StringBuilder();
            for (var pos = 0; pos < s.Length; )
            {
                var charsToGet = s.Length - pos;
                if (charsToGet > maxLineWidth)
                    charsToGet = maxLineWidth;
                sb.Append((string)HttpUtility.HtmlEncode(s.Substring(pos, charsToGet)));
                pos += charsToGet;
                if (s.Length - pos > 0)
                    sb.Append("<br/>");
            }

            return sb.ToString();
        }

        /// <summary>
        ///     Does nothing - exists just to satisfy the interface requirements.
        /// </summary>
        /// <returns></returns>
        public TextReader GetEncoderCssStream()
        {
            return null;
        }
    }
}