using CodeReviewer.Models;
using CodeReviewer.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeReviewer
{
    class Args
    {
        public Args(string[] args)
        {
            if (!Parser.ParseArgumentsWithUsage(args, this))
                throw new Exception("Failed to parse args");
        }
    }

    class GenDiffArgs
    {
        [Argument(ArgumentType.AtMostOnce, HelpText = "changelist")]
        public string CL;
        [Argument(ArgumentType.AtMostOnce, HelpText = "whole / partial file name")]
        public string file;
        [Argument(ArgumentType.AtMostOnce, HelpText = "fileId")]
        public int fileId;
        [Argument(ArgumentType.AtMostOnce, HelpText = "revision")]
        public int revisionId = -1;
        [DefaultArgument(ArgumentType.MultipleUnique, HelpText = "Input files to diff in changeList.")]
        public string[] files;
    }

    class GenDiffFiles
    {
        public static CodeReviewer.Models.CodeReviewerContext db = new CodeReviewer.Models.CodeReviewerContext();
        public static UserName UserName { get; set; }
        public static UserSettingsDto UserSettings { get; set; }
        public static bool force = true;

        static void Main(string[] args)
        {
            Log.ConsoleMode = true;

            int id = 0;
            var changeFiles = new List<ChangeFile>();

            var options = new GenDiffArgs();
            if (Parser.ParseArgumentsWithUsage(args, options))
            {
                if (!string.IsNullOrEmpty(options.CL))
                {
                    //     insert application code here
                    var changeList = (from change in db.ChangeLists
                                      where change.CL == options.CL
                                      select change).FirstOrDefault();
                    if (changeList != null)
                        id = changeList.Id;
                }

                if (!string.IsNullOrEmpty(options.file))
                {
                    var fileName = options.file.ToLower();
                    var files = (from file in db.ChangeFiles
                                       where file.ServerFileName.ToLower().Contains(fileName) &&
                                            (id == 0 || file.ChangeListId == id)
                                       select file);
                    foreach (var file in files)
                    {
                        changeFiles.Add(file);
                    }
                }

                if (options.fileId > 0 && options.revisionId < 0)
                {
                    var fileName = options.file.ToLower();
                    var files = (from file in db.ChangeFiles
                                 where file.Id == options.fileId &&
                                      (id == 0 || file.ChangeListId == id)
                                 select file);
                    foreach (var file in files)
                    {
                        changeFiles.Add(file);
                    }
                }
            }

            var userName = Environment.GetEnvironmentVariable("USERNAME").Trim();
            UserName = new UserName(userName, db);
            UserSettings = ReviewUtil.UserSettings(0, UserName.userName, db);

            if (changeFiles.Count > 0)
            {
                foreach (var changeFile in changeFiles)
                {
                    DiffRevision(changeFile.ChangeListId, changeFile.Id, changeFile.ReviewRevision - 1, force);
                }
            }
            else if (id != 0)
            {
                if (options.fileId != 0 && options.revisionId >= 0)
                {
                    DiffRevision(id, options.fileId, options.revisionId, force);
                }
                else
                {
                    var gen = new DiffGenerator(db);
                    gen.GenDiffFiles(id, UserName.userName, UserSettings, force);
                }
            }
            else
            {
                var gen = new DiffGenerator(db);

                var changelists = (from item in db.ChangeLists.AsNoTracking()
                                   where (item.Stage != (int)ChangeListStatus.Deleted)
                                   select item).OrderByDescending(x => x.TimeStamp);
                foreach (var item in changelists)
                {
                    Log.Info("Generate diff files for {0} {1}", item.CL, item.Id);
                    gen.GenDiffFiles(item.Id, UserName.userName, UserSettings, force);
                }
            }
        }

        public static void DiffRevision(int id, int fileId, int revisionId, bool force)
        {
            var changeList = GetChangeList(id);
            var changeFile = GetChangeFile(changeList, fileId);

            var gen = new DiffGenerator(db);
            var baseReviewId = gen.GetBaseReviewId(UserName.userName, changeList.Id);
            var fileVersions = changeFile.FileVersions.ToArray();
            var prev = 0;
            for (var idx = prev + 1; idx < fileVersions.Length; prev = idx++)
            {
                if (revisionId != idx - 1)
                    continue;

                var vid1 = fileVersions[prev].Id;
                var vid2 = fileVersions[idx].Id;
                var left = fileVersions[prev];
                var right = fileVersions[idx];

                var diffFile = gen.GenerateDiffFile(left, right, changeList.CL, baseReviewId, UserSettings, force);
                if (string.IsNullOrEmpty(diffFile) || !System.IO.File.Exists(diffFile))
                    throw new ApplicationException(string.Format("Failed to create a diff file for change list id: {0} file id: {1} revision id: {2}", id, fileId, revisionId));

                Console.WriteLine("Generated {0}", diffFile);
                return;
            }

            throw new ApplicationException(string.Format("Did not find revision for change list id: {0} file id: {1} revision id: {2}", id, fileId, revisionId));
        }

        private static ChangeList GetChangeList(int id)
        {
            ChangeList changeList = null;

            if (id > 0)
                changeList = db.ChangeLists.Find(id);

            if (changeList == null || changeList.ChangeFiles.Count == 0)
                throw new ApplicationException(string.Format("Invalid change list id {0}", id));

            return changeList;
        }

        private static ChangeFile GetChangeFile(ChangeList changeList, int fileId)
        {
            if (fileId == 0)
                fileId = changeList.ChangeFiles.First().Id;

            var changeFile = (from rv in db.ChangeFiles.AsNoTracking()
                              where rv.Id == fileId
                              select rv).FirstOrDefault();
            if (changeFile == null)
                throw new ApplicationException(string.Format("did not find change list id: {0} cl: {1} file: {2}", changeList.Id, changeList.CL, fileId));

            return changeFile;
        }

    }
}
