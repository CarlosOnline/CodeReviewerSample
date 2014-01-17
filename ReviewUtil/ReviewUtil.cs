using CodeReviewer.Models;
using CodeReviewer.Util;
using SourceControl;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Web.Script.Serialization;
using ChangeFile = SourceControl.ChangeFile;

namespace CodeReviewer
{
    public static class SourceType
    {
        public static SourceControlType Instance(string type)
        {
            switch (type.ToLower())
            {
                case "tfs":
                    return SourceControlType.TFS;

                case "p4":
                    return SourceControlType.PERFORCE;

                case "sd":
                    return SourceControlType.SD;

                case "svn":
                    return SourceControlType.SUBVERSION;

                default:
                    throw new Exception(string.Format("error : source control '{0}' is not supported.", type));
            }
        }

        public static void LoadSettings(SourceControlType type, SourceControlSettings settings)
        {
            switch (type)
            {
                case SourceControlType.PERFORCE:
                    if (string.IsNullOrEmpty(settings.Client))
                        settings.Client = ConfigurationManager.AppSettings.LookupValue("P4CLIENT", "");
                    if (string.IsNullOrEmpty(settings.Port))
                        settings.Port = ConfigurationManager.AppSettings.LookupValue("P4PORT", "");
                    if (string.IsNullOrEmpty(settings.User))
                        settings.User = ConfigurationManager.AppSettings.LookupValue("P4USER", "");
                    if (string.IsNullOrEmpty(settings.Password))
                        settings.Password = ConfigurationManager.AppSettings.LookupValue("P4PASSWD", "");
                    break;

                default:
                    throw new Exception(string.Format("LoadSettings for type: {0} not yet implemented.", type));
            }
        }
    }

    public static class ReviewUtil
    {
        /// <summary>
        ///     Maximum number of integrated files before we ask for confirmation.
        /// </summary>
        private const int MaximumIntegratedFiles = 50;

        /// <summary>
        ///     The default id of our source control record.
        /// </summary>
        public static int DefaultSourceControlInstanceId = 1;

        private static ISourceControl _sc;

        /// <summary>
        ///     Database context
        /// </summary>
        public static CodeReviewerContext db = new CodeReviewerContext();

        public static string DiffExe { get; set; }

        public static ISourceControl SD
        {
            get
            {
                return _sc ?? (_sc = PerforceInterface.GetInstance(
                    ConfigurationManager.AppSettings["p4DiffExe"],
                    ConfigurationManager.AppSettings["P4PORT"],
                    ConfigurationManager.AppSettings["P4CLIENT"],
                    ConfigurationManager.AppSettings["P4USER"],
                    ConfigurationManager.AppSettings["P4PASSWD"]));
            }
        }

        public static void Log(string Message = "", params object[] args)
        {
            Util.Log.Info(Message, args);
        }

        /// <summary>
        ///     Compares two text strings that are version/file bodies. They should either be both null or
        ///     both not null and equal strings for the function to return true.
        /// </summary>
        /// <param name="s1"> First string or null. </param>
        /// <param name="s2"> Second string or null. </param>
        /// <returns> Either both nulls, or both equal strings. </returns>
        private static bool BodiesEqual(string s1, string s2)
        {
            return s1 == null ? s2 == null : s1.Equals(s2);
        }

        public static UserSettingsDto UserSettings(int id, string userName, CodeReviewerContext dbContext, bool force = false)
        {
            var context = UserContext(id, userName, "settings", () => new UserSettingsDto(), dbContext);
            var settings = UserSettingsDto.FromJS(context.value);
            if (settings.version == UserSettingsDto.CurrentVersion)
                return settings;

            // Generate new UserContext
            context = UserContext(id, userName, "settings", () => new UserSettingsDto(), dbContext, true);
            return UserSettingsDto.FromJS(context.value);
        }

        public static ChangeListSettingsDto ChangeListSettings(int id, string userName, string key, CodeReviewerContext dbContext, bool force = false)
        {
            var context = UserContext(id, userName, key, () => new ChangeListSettingsDto(), dbContext);
            var settings = ChangeListSettingsDto.FromJS(context.value);
            if (settings.version == ChangeListSettingsDto.CurrentVersion)
                return settings;

            // Generate new UserContext
            context = UserContext(id, userName, key, () => new ChangeListSettingsDto(), dbContext, true);
            return ChangeListSettingsDto.FromJS(context.value);
        }

        public static UserContextDto UserContext<T>(int id, string userName, string key, Func<T> getDefaultValueFunc, CodeReviewerContext dbContext, bool force = false)
        {
            var db = dbContext ?? ReviewUtil.db;

            UserContext data = null;
            if (!force)
            {
                if (id > 0)
                {
                    data = db.UserContexts.Find(id);
                    if (data != null)
                        return new UserContextDto(data);
                }

                var query = (from item in db.UserContexts.AsNoTracking()
                             where item.UserName == userName &&
                                     item.KeyName == key
                             select item);
                if (query.Any())
                {
                    data = query.First();
                    return new UserContextDto(data);
                }
            }

            var settingsObj = getDefaultValueFunc() as BaseSettingsDto;
            var model = GenerateUserContext(0, userName, key, settingsObj);
            db.SetUserContext(key, model.value, userName, "", settingsObj.version);

            var queryEx = (from item in db.UserContexts.AsNoTracking()
                           where item.UserName == userName &&
                                   item.KeyName == key
                           select item);
            if (queryEx.Any())
            {
                data = queryEx.First();
                return new UserContextDto(data);
            }
            return null;
        }

        public static UserContextDto GenerateUserContext(int id, string userName, string key, BaseSettingsDto settingsObj)
        {
            var js = new JavaScriptSerializer();
            var dto = new UserContextDto()
            {
                id = id,
                key = key,
                userName = userName,
                value = js.Serialize(settingsObj),
                version = settingsObj.version
            };
            return dto;
        }

        public static BaseSettingsDto GetSettingsDto(string key, string value)
        {
            if (0 == key.CompareTo("settings"))
            {
                return UserSettingsDto.FromJS(value);
            }
            else
            {
                return ChangeListSettingsDto.FromJS(value);
            }
        }

        /// <summary>
        ///     Ensures that the diffs in files can in fact be parsed by Malevich. If non-graphical characters or
        ///     incorrect (mixed: Unix + Windows, or Windows + Mac) line endings are present, this throws "sd diff"
        ///     off and it produces the results that we will not be able to read. This checks that this does not occur.
        /// </summary>
        /// <param name="change"> Change List. </param>
        /// <returns> True if the differ is intact. </returns>
        private static bool VerifyDiffIntegrity(Change change)
        {
            var diffDecoder = new Regex(@"^([0-9]+)(,[0-9]+)?([a,d,c]).*$");
            var result = true;
            foreach (var file in change.Files)
            {
                if (file.Data == null ||
                    (file.Action != ChangeFile.SourceControlAction.EDIT &&
                     file.Action != ChangeFile.SourceControlAction.INTEGRATE))
                    continue;

                var reader = new StringReader(file.Data);
                for (; ; )
                {
                    var line = reader.ReadLine();
                    if (line == null)
                        break;

                    if (line.StartsWith("> ") || line.StartsWith("< ") || line.Equals("---") ||
                        line.Equals("\\ No newline at end of file") || diffDecoder.IsMatch(line))
                        continue;

                    Log("Cannot parse the difference report for {0}.",
                        file.LocalOrServerFileName);
                    Log("{0}", file.Data);
                    Log("");
                    result = false;
                    break;
                }
            }

            if (!result)
            {
                Log("");
                Log("Found problems processing the file differences in the change list.");
                Log("");
                Log("This is typically caused by incorrect or mixed end of line markers, or other");
                Log("non-graphical characters that your source control system could not process.");
                Log("");
                Log("Please fix the files in question and resubmit the change.");
            }

            return result;
        }

        private static string NormalizeLineEndings(string str)
        {
            if (str == null)
                return str;

            str = Regex.Replace(str, "\\r\\n", "\n", RegexOptions.Multiline);
            //str = Regex.Replace(str, "\\n", "\r\n", RegexOptions.Multiline);
            return str;
        }

        public static void ProcessCodeReview(
            string changeList,
            List<string> reviewers,
            List<string> invitees,
            string title,
            string description,
            CodeReviewerContext db,
            bool force = false,
            bool includeBranchedFiles = false,
            bool preview = false)
        {
            ProcessCodeReview(ConfigurationManager.AppSettings["DatabaseServer"],
                              SD,
                              DefaultSourceControlInstanceId,
                              changeList,
                              reviewers,
                              invitees,
                              "",
                              "",
                              title,
                              description,
                              null,
                              new List<string>(),
                              force,
                              includeBranchedFiles,
                              preview,
                              ConfigurationManager.AppSettings["P4USER"],
                              db);
        }

        /// <summary>
        ///     Main driver for the code review submission tool.
        /// </summary>
        /// <param name="context"> The database context. </param>
        /// <param name="sd"> Source control client. </param>
        /// <param name="sourceControlInstanceId">
        ///     The ID of source control instance to use.
        ///     This is an ID of a record in the database that is unique for a given CL namespace.
        /// </param>
        /// <param name="changeList"> CL. </param>
        /// <param name="reviewers"> The list of people to who to send the code review request. </param>
        /// <param name="invitees">
        ///     The list of people who are invited to participate in the code review
        ///     (but need to positively acknowledge the invitation by choosing to review the code).
        /// </param>
        /// <param name="link"> Optionally, a link to a file or a web page to be displayed in CL page. </param>
        /// <param name="linkDescr"> An optional description of the said link. </param>
        /// <param name="title">title for review revision</param>
        /// <param name="description">
        ///     An optional description of the changelist, overrides any description
        ///     from the source control tool.
        /// </param>
        /// <param name="bugServer">??</param>
        /// <param name="bugIds">List of bugs to associate with review page.</param>
        /// <param name="force">
        ///     If branched files are included, confirms the submission even if there
        ///     are too many files.
        /// </param>
        /// <param name="includeBranchedFiles"> </param>
        /// <param name="preview">If true, do not commit changes.</param>
        /// <param name="impersonateUserName"></param>
        public static void ProcessCodeReview(
            string databaseServer,
            ISourceControl sd,
            int sourceControlInstanceId,
            string changeList,
            List<string> reviewers,
            List<string> invitees,
            string link,
            string linkDescr,
            string title,
            string description,
            IBugServer bugServer,
            List<string> bugIds,
            bool force,
            bool includeBranchedFiles,
            bool preview,
            string impersonateUserName,
            CodeReviewerContext db)
        {
            var change = sd.GetChange(changeList, includeBranchedFiles);
            if (change == null)
            {
                throw new Exception(String.Format("Change List {0} does not exist", changeList));
            }

            changeList = change.ChangeListFriendlyName ?? changeList;

            if (changeList == null)
                return;

            var userNameInfo = new UserName(impersonateUserName, db);

            if (!IncludeBranchFiles(force, includeBranchedFiles, change))
                return;

            if (!VerifyDiffIntegrity(change))
                return;

            var existingReviewQuery = from rv in db.ChangeLists.AsNoTracking()
                                      where rv.CL == changeList && rv.SourceControlId == sourceControlInstanceId
                                      select rv;

            // is this a new review, or a refresh of an existing one?
            var isNewReview = (!existingReviewQuery.Any());
            var changeId = 0;

            if (String.IsNullOrEmpty(title))
                title = "Change " + change.ChangeListId;

            if (String.IsNullOrEmpty(description))
                description = change.Description;

            var reviewUrl = GetReviewUrl(changeList);

            var reviewRevision = 1;
            var filesAdded = false;
            using (var scope = new TransactionScope())
            {
                // This more like "GetOrAddChangeList", as it returns the id of any pre-existing changelist
                // matching 'changeList'.
                if (String.IsNullOrEmpty(userNameInfo.userName))
                {
                    changeId = db.AddChangeList(sourceControlInstanceId, "", "", change.SdClientName, changeList, reviewUrl, title, description);
                }
                else
                {
                    var changeListDb = (from c in db.ChangeLists.AsNoTracking()
                                        where c.SourceControlId == sourceControlInstanceId &&
                                              c.UserName == userNameInfo.userName &&
                                              c.UserClient == change.SdClientName &&
                                              c.CL == changeList
                                        select c).FirstOrDefault();

                    if (changeListDb == null)
                    {
                        changeListDb = new ChangeList
                            {
                                SourceControlId = sourceControlInstanceId,
                                UserName = userNameInfo.userName,
                                ReviewerAlias = userNameInfo.reviewerAlias,
                                UserClient = change.SdClientName,
                                CL = changeList,
                                Url = reviewUrl,
                                Title = title,
                                Description = description,
                                TimeStamp = change.TimeStamp.ToUniversalTime(),
                                CurrentReviewRevision = 1,
                                Stage = 0
                            };
                        db.ChangeLists.Add(changeListDb);
                        db.SaveChanges();
                    }
                    else
                    {
                        reviewRevision = changeListDb.CurrentReviewRevision + 1;
                    }

                    changeId = changeListDb.Id;
                }

                // Get the list of files corresponding to this changelist already on the server.
                var dbChangeFiles = (from fl in db.ChangeFiles.AsNoTracking()
                                     where fl.ChangeListId == changeId && fl.IsActive
                                     select fl)
                    .OrderBy(file => file.ServerFileName)
                    .GetEnumerator();

                var inChangeFiles = (from fl in change.Files
                                     select fl)
                    .OrderBy(file => file.ServerFileName)
                    .GetEnumerator();

                var dbChangeFilesValid = dbChangeFiles.MoveNext();
                var inChangeFilesValid = inChangeFiles.MoveNext();

                // Uses bitwise OR to ensure that both MoveNext methods are invoked.
                while (dbChangeFilesValid || inChangeFilesValid)
                {
                    int comp;
                    if (!dbChangeFilesValid) // No more files in database
                        comp = 1;
                    else if (!inChangeFilesValid) // No more files in change list.
                        comp = -1;
                    else
                        comp = String.CompareOrdinal(dbChangeFiles.Current.ServerFileName,
                                                     inChangeFiles.Current.ServerFileName);

                    var existsIn = FileExistsIn.Neither;
                    if (comp < 0) // We have a file in DB, but not in source control. Delete it from DB.
                    {
                        Log("File {0} has been dropped from the change list.",
                            dbChangeFiles.Current.ServerFileName);
                        db.RemoveFile(dbChangeFiles.Current.Id);

                        dbChangeFilesValid = dbChangeFiles.MoveNext();
                        existsIn = FileExistsIn.Database;
                        filesAdded = true;
                        continue;
                    }

                    var file = inChangeFiles.Current;

                    var fid = 0;
                    if (comp > 0) // File in source control, but not in DB
                    {
                        Log("Adding file {0}", file.ServerFileName);
                        fid = db.AddFile(changeId, file.LocalFileName, file.ServerFileName, reviewRevision);
                        if (fid <= 0)
                            throw new Exception(string.Format("Failed to AddFile=={0} {1} {2} {3}", fid, changeId, file.LocalFileName, reviewRevision));
                        existsIn = FileExistsIn.Change;
                        db.SaveChanges();
                        filesAdded = true;
                    }
                    else // Both files are here. Need to check the versions.
                    {
                        fid = dbChangeFiles.Current.Id;
                        existsIn = FileExistsIn.Both;
                    }

                    var haveBase = (from bv in db.FileVersions.AsNoTracking()
                                    where bv.FileId == fid && bv.Revision == file.Revision && bv.IsRevisionBase
                                    select bv).Any();

                    var versionQuery = from fv in db.FileVersions.AsNoTracking()
                                       where fv.FileId == fid && fv.Revision == file.Revision
                                       orderby fv.Id descending
                                       select fv;

                    var version = versionQuery.FirstOrDefault();
                    var haveVersion = version != null && version.Action == (int)file.Action &&
                                      BodiesEqual(NormalizeLineEndings(file.Data), NormalizeLineEndings(version.Text));

                    int vid = 0;
                    if (!haveBase && file.IsText &&
                        (file.Action == ChangeFile.SourceControlAction.EDIT ||
                         (file.Action == ChangeFile.SourceControlAction.INTEGRATE &&
                          includeBranchedFiles)))
                    {
                        DateTime? dateTime;
                        var fileBody = sd.GetFile(
                            file.OriginalServerFileName ?? file.ServerFileName,
                            file.Revision, out dateTime);
                        if (fileBody == null)
                        {
                            Log("ERROR: Could not retrieve {0}#{1}", file.ServerFileName, file.Revision);
                            return;
                        }

                        Log("Adding base revision for {0}#{1}", file.ServerFileName, file.Revision);
                        vid = db.AddVersion(fid, file.Revision, reviewRevision, (int)file.Action, 
                            dateTime, true, true, true, fileBody);
                        db.SaveChanges();
                        filesAdded = true;
                    }
                    else
                    {
                        // Do this so we print the right thing.
                        haveBase = true;
                    }

                    if (!haveVersion)
                    {
                        if (file.Action == ChangeFile.SourceControlAction.DELETE)
                        {
                            vid = db.AddVersion(fid, file.Revision, reviewRevision, (int)file.Action, 
                                DateTime.Now, file.IsText, false, false, "");
                            filesAdded = true;
                        }
                        else if ((file.Action == ChangeFile.SourceControlAction.RENAME) || !file.IsText)
                        {
                            vid = db.AddVersion(fid, file.Revision, reviewRevision, (int)file.Action,
                                               file.LastModifiedTime, file.IsText,
                                               false, false, "");
                            filesAdded = true;
                        }
                        else if (file.Action == ChangeFile.SourceControlAction.ADD ||
                                 file.Action == ChangeFile.SourceControlAction.BRANCH)
                        {
                            vid = db.AddVersion(fid, file.Revision, reviewRevision, (int)file.Action,
                                               file.LastModifiedTime, file.IsText,
                                               true, false, file.Data);
                            filesAdded = true;
                        }
                        else if (file.Action == ChangeFile.SourceControlAction.EDIT ||
                                 file.Action == ChangeFile.SourceControlAction.INTEGRATE)
                        {
                            vid = db.AddVersion(fid, file.Revision, reviewRevision, (int)file.Action,
                                               file.LastModifiedTime, file.IsText,
                                               false, false, file.Data);
                            filesAdded = true;
                        }
                        db.SaveChanges();

                        var textFlag = file.IsText ? "text" : "binary";
                        string action;
                        switch (file.Action)
                        {
                            case ChangeFile.SourceControlAction.ADD:
                                action = "add";
                                break;

                            case ChangeFile.SourceControlAction.EDIT:
                                action = "edit";
                                break;

                            case ChangeFile.SourceControlAction.DELETE:
                                action = "delete";
                                break;

                            case ChangeFile.SourceControlAction.BRANCH:
                                action = "branch";
                                break;

                            case ChangeFile.SourceControlAction.INTEGRATE:
                                action = "integrate";
                                break;

                            case ChangeFile.SourceControlAction.RENAME:
                                action = "rename";
                                break;

                            default:
                                action = "unknown";
                                break;
                        }

                        if (version != null && vid == version.Id)
                        {
                            // The file was already there. This happens sometimes because SQL rountrip (to database
                            // and back) is not an identity: somtimes the non-graphical characters change depending
                            // on the database code page. But if the database has returned a number, and this number
                            // is the same as the previous version id, we know that the file has not really been added.
                            haveVersion = true;
                        }
                        else
                        {
                            Log("Added version for {0}#{1}({2}, {3})", file.ServerFileName, file.Revision,
                                textFlag, action);
                        }
                    }

                    if (haveBase && haveVersion)
                        Log("{0} already exists in the database.", file.ServerFileName);

                    if ((existsIn & FileExistsIn.Database) == FileExistsIn.Database)
                        dbChangeFilesValid = dbChangeFiles.MoveNext();
                    if ((existsIn & FileExistsIn.Change) == FileExistsIn.Change)
                        inChangeFilesValid = inChangeFiles.MoveNext();

                    existsIn = FileExistsIn.Neither;
                }

                if (filesAdded)
                {
                    // Update Review Revision
                    var changelist = db.ChangeLists.Find(changeId);
                    changelist.CurrentReviewRevision = reviewRevision;
                    changelist.Title = title;
                    changelist.Description = description;
                    db.Entry(changelist).State = EntityState.Modified;
                    db.SaveChanges();
                }

                var requestType = reviewRevision == 1 ? MailType.Request : MailType.Iteration;
                foreach (var reviewer in reviewers)
                {
                    var reviewId = 0;
                    var status = 0;
                    var previous = (from rv in db.Reviewers.AsNoTracking()
                                    where rv.ReviewerAlias == reviewer && rv.ChangeListId == changeId
                                    select rv).FirstOrDefault();
                    if (previous != null)
                        status = previous.Status;

                    var reviewerNameInfo = new UserName(reviewer, db);
                    Log("AddReviewer {0} {1}", reviewerNameInfo.userName, requestType);
                    reviewId = db.AddReviewer(0, reviewerNameInfo.userName, reviewerNameInfo.reviewerAlias, changeId, status, (int)requestType);
                    db.SaveChanges();
                }

                foreach (var invitee in invitees)
                {
                    db.AddReviewRequest(changeId, invitee);
                    db.SaveChanges();
                }

                if (String.IsNullOrEmpty(title))
                    title = "New Revision " + DateTime.Now.ToString(CultureInfo.InvariantCulture);

                int result = db.AddReview(userNameInfo.userName, userNameInfo.reviewerAlias, changeId, title, 4);
                db.SaveChanges();

                if (!String.IsNullOrEmpty(link))
                {
                    int? attachmentId = null;
                    attachmentId = db.AddAttachment(changeId, linkDescr, link);
                    db.SaveChanges();
                }

                db.SaveChanges();

                if (preview)
                    scope.Dispose();
                else
                    scope.Complete();
            }

            // Gen Html Files
            var gen = new DiffGenerator(db);
            var settings = UserSettings(0, userNameInfo.userName, db);
            gen.GenDiffFiles(changeId, userNameInfo.userName, settings, force: false);

            if (!String.IsNullOrEmpty(reviewUrl))
            {
                Log("Change {0} is ready for review, and may be viewed at", changeList);
                Log("   {0}", reviewUrl);

                var allBugIds = change.BugIds.Union(bugIds);
                var enumerable = allBugIds as string[] ?? allBugIds.ToArray();
                if (bugServer != null && enumerable.Any())
                {
                    Log("Connecting to TFS Work Item Server");
                    if (bugServer.Connect())
                    {
                        foreach (var bugId in enumerable)
                        {
                            var bug = bugServer.GetBug(bugId);
                            if (bug.AddLink(new Uri(reviewUrl), null))
                                Log("Bug {0} has been linked to review page.", bugId);
                        }
                    }
                }
            }
            else
            {
                Log("Change {0} is ready for review.", changeList);
                if (isNewReview)
                {
                    if (reviewers.Count == 0 && invitees.Count == 0)
                    {
                        Log("Note: no reviewers specified. You can add them later using this utility.");
                    }
                    else
                    {
                        Log("If the mail notifier is enabled, the reviewers will shortly receive mail");
                        Log("asking them to review your changes.");
                    }
                }
                else
                {
                    Log("Note: existing reviewers will not be immediately informed of this update.");
                    Log("To ask them to re-review your updated changes, you can visit the review website");
                    Log("and submit a response.");
                }
            }

            if (preview)
                Log("In preview mode -- no actual changes committed.");
        }

        public static bool IncludeBranchFiles(bool force, bool includeBranchedFiles, Change change)
        {
            if (includeBranchedFiles && !force)
            {
                var branchedFiles = 0;
                foreach (var file in change.Files)
                {
                    if (file.IsText && (file.Action == ChangeFile.SourceControlAction.BRANCH ||
                                        file.Action == ChangeFile.SourceControlAction.INTEGRATE))
                        ++branchedFiles;
                }

                if (branchedFiles > MaximumIntegratedFiles)
                {
                    Log("There are {0} branched/integrated files in this change.", branchedFiles);
                    Log("Including the full text of so many files in review may increase the size");
                    Log("of the review database considerably.");
                    Console.Write("Are you sure you want to proceed (Yes/No)? ");
                    var response = Console.ReadLine();
                    Log("NOTE: In the future you can override this check by specifying --force");
                    Log("on the command line.");
                    // TODO: Exclude branch files
                    if (response != null && (response[0] != 'y' && response[0] != 'Y'))
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        ///     Verifies that there are no pending reviews for this change list.
        /// </summary>
        /// <param name="context"> Database context. </param>
        /// <param name="cid"> Change list ID. </param>
        /// <returns></returns>
        private static bool NoPendingReviews(CodeReviewerContext context, int cid)
        {
            var unsubmittedReviewsQuery = from rr in context.Reviews.AsNoTracking()
                                          where rr.ChangeListId == cid && !rr.IsSubmitted
                                          select rr;

            var printedTitle = false;
            var unsubmittedComments = 0;

            foreach (var review in unsubmittedReviewsQuery)
            {
                // TODO
                var reviewId = review.Id;
                var groups = (from cc in context.CommentGroups.AsNoTracking() where cc.ReviewId == reviewId select cc);
                foreach (var group in groups)
                {
                    var comments = (from cc in context.CommentGroups.AsNoTracking() where cc.ReviewId == review.Id select cc).Count();
                    if (comments == 0)
                        continue;

                    unsubmittedComments += comments;

                    if (!printedTitle)
                    {
                        printedTitle = true;
                        Log("Reviews are still pending for this change list:");
                    }

                    Log("{0} : {1} comments.", review.UserName, comments);
                }
            }

            return unsubmittedComments == 0;
        }

        /// <summary>
        ///     Marks review as closed.
        /// </summary>
        /// <param name="context"> Database context. </param>
        /// <param name="userName"> User alias. </param>
        /// <param name="sourceControlId"> Source control ID. </param>
        /// <param name="cl"> Review number (source control side). </param>
        /// <param name="force"> Whether to force closing the review even if there are pending changes. </param>
        /// <param name="admin"> Close the review in admin mode, regardless of the user. </param>
        public static void MarkChangeListAsClosed(CodeReviewerContext context, string userName,
                                                  int sourceControlId, string cl, bool force, bool admin)
        {
            var cid = (from ch in context.ChangeLists.AsNoTracking()
                       where ch.SourceControlId == sourceControlId && ch.CL.Equals(cl) &&
                             ch.Stage == 0 && (admin || ch.UserName.Equals(userName))
                       select ch.Id).FirstOrDefault();

            if ((object)cid == null)
            {
                Log("No active change in database.");
                return;
            }

            if (force || NoPendingReviews(context, cid))
            {
                context.SubmitChangeList(cid);
                Log("{0} closed. Use 'review reopen {0}' to reopen.", cl);
            }
            else
            {
                Log("Pending review detected. If you want to close this change list");
                Log("anyway, use the --force.");
            }
        }

        /// <summary>
        ///     Marks review as deleted.
        /// </summary>
        /// <param name="context"> Database context. </param>
        /// <param name="userName"> User alias. </param>
        /// <param name="sourceControlId"> Source control ID. </param>
        /// <param name="cl"> Review number (source control side). </param>
        /// <param name="force"> Whether to force closing the review even if there are pending changes. </param>
        /// <param name="admin"> Close the review in admin mode, regardless of the user. </param>
        public static void DeleteChangeList(CodeReviewerContext context, string userName,
                                            int sourceControlId, string cl, bool force, bool admin)
        {
            var cids = admin
                           ? (from ch in context.ChangeLists.AsNoTracking()
                              where ch.SourceControlId == sourceControlId && ch.CL.Equals(cl) && ch.Stage == 0
                              select ch.Id).ToArray()
                           : (from ch in context.ChangeLists.AsNoTracking()
                              where ch.SourceControlId == sourceControlId && ch.CL.Equals(cl) &&
                                    ch.UserName.Equals(userName) && ch.Stage == 0
                              select ch.Id).ToArray();

            if (cids.Length != 1)
            {
                Log("No active change in database.");
                return;
            }

            if (force || NoPendingReviews(context, cids[0]))
            {
                context.DeleteChangeList(cids[0]);
                Log("{0} deleted. Use 'review reopen {0}' to undelete.", cl);
            }
            else
            {
                Log("Pending review detected. If you want to delete this change list");
                Log("anyway, use the --force.");
            }
        }

        /// <summary>
        ///     Renames a change list.
        /// </summary>
        /// <param name="context"> Database context. </param>
        /// <param name="userName"> User alias. </param>
        /// <param name="sourceControlId"> Source control ID. </param>
        /// <param name="cl"> Review number (source control side). </param>
        /// <param name="newCl"> New name for the change list. </param>
        /// <param name="admin"> Close the review in admin mode, regardless of the user. </param>
        public static void RenameChangeList(CodeReviewerContext context, string userName,
                                            int sourceControlId, string cl, string newCl, bool admin)
        {
            var cids = admin
                           ? (from ch in context.ChangeLists.AsNoTracking()
                              where ch.SourceControlId == sourceControlId && ch.CL.Equals(cl)
                              select ch.Id).ToArray()
                           : (from ch in context.ChangeLists.AsNoTracking()
                              where
                                  ch.SourceControlId == sourceControlId && ch.CL.Equals(cl) &&
                                  ch.UserName.Equals(userName)
                              select ch.Id).ToArray();

            if (cids.Length != 1)
            {
                Log("No active change in database.");
                return;
            }

            context.RenameChangeList(cids[0], newCl);
            Log("{0} renamed to {1}.", cl, newCl);
        }

        public static string GetReviewUrl(string changeListId)
        {
            var reviewSiteUrl = ConfigurationManager.AppSettings["ReviewSiteUrl"];
            if (String.IsNullOrEmpty(reviewSiteUrl))
                Log("Warning: ReviewSiteUrl web.config, app.config setting is missing");
            var url = reviewSiteUrl;
            if (!url.EndsWith("/"))
                url += "/";
            url += @"ChangeList?cl=" + changeListId;
            return url;
        }

        /// <summary>
        ///     Reopens a change list.
        /// </summary>
        /// <param name="context"> Database context. </param>
        /// <param name="userName"> User alias. </param>
        /// <param name="sourceControlId"> Source control ID. </param>
        /// <param name="cl"> Review number (source control side). </param>
        /// <param name="admin"> Close the review in admin mode, regardless of the user. </param>
        public static void ReopenChangeList(CodeReviewerContext context, string userName,
                                            int sourceControlId, string cl, bool admin)
        {
            var cids = admin
                           ? (from ch in context.ChangeLists.AsNoTracking()
                              where ch.SourceControlId == sourceControlId && ch.CL.Equals(cl) && ch.Stage != 0
                              select ch.Id).ToArray()
                           : (from ch in context.ChangeLists.AsNoTracking()
                              where ch.SourceControlId == sourceControlId && ch.CL.Equals(cl) &&
                                    ch.UserName.Equals(userName) && ch.Stage != 0
                              select ch.Id).ToArray();

            if (cids.Length != 1)
            {
                Log("No inactive change in database.");
                return;
            }

            context.ReopenChangeList(cids[0]);
            Log("{0} reopened.", cl);
        }

        /// <summary>
        ///     Adds an attachment to code review.
        /// </summary>
        /// <param name="context"> Database context. </param>
        /// <param name="userName"> User alias. </param>
        /// <param name="cl"> Change list to modify. </param>
        /// <param name="link"> The file or web page URL. </param>
        /// <param name="linkDescr"> The text (optional). </param>
        public static void AddAttachment(CodeReviewerContext context, string userName, string cl,
                                         string link, string linkDescr)
        {
            var changeListQuery = from ch in context.ChangeLists.AsNoTracking()
                                  where ch.CL.Equals(cl) && ch.UserName.Equals(userName) && ch.Stage == 0
                                  select ch.Id;
            if (changeListQuery.Count() != 1)
            {
                Log("No active change in database.");
                return;
            }

            var cid = changeListQuery.Single();

            var result = context.AddAttachment(cid, linkDescr, link);

            Log("Attachment submitted.");
        }

        /// <summary>
        ///     Processes a string that is a user name or a comma or a semicolon separated array of users.
        ///     Adds them to the list. This is used to add reviewers and invitees.
        /// </summary>
        /// <param name="users"> List of users. </param>
        /// <param name="user">
        ///     A user name or a comma or semicolon separated list of user names, without domain.
        /// </param>
        /// <returns> false if there was a syntax error (user name contains invalid characters). </returns>
        public static bool AddReviewers(List<string> users, string user)
        {
            var usernames = user.Split(',', ';');
            foreach (var u in usernames)
            {
                try
                {
                    new MailAddress(u);
                    users.Add(u);
                    continue;
                }
                catch
                {
                }

                try
                {
                    new MailAddress(u + "@testdomain.com");
                    users.Add(u);
                    continue;
                }
                catch (FormatException ex)
                {
                    Log("{0} is not a valid alias! Ex: {1}", u, ex);
                    return false;
                }
            }

            return true;
        }

        public static Change GetChangeList(string changeList)
        {
            return SD.GetChange(changeList, false);
        }

        /// <summary>
        ///     Indicates if the file is in the change, the database, or both.
        /// </summary>
        [Flags]
        private enum FileExistsIn
        {
            Neither = 0,
            Change = 1,
            Database = 2,
            Both = 3,
        }
    }
}
