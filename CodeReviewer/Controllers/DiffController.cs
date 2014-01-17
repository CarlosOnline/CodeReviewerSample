using CodeReviewer.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace CodeReviewer.Controllers
{
    public class DiffController : BaseController
    {
        [Authorize]
        public ActionResult File(int id, int fileId)
        {
            return ExecApiMethod(() =>
            {
                var userNameInfo = UserName;
                var settings = UserSettings;
                var model = GetChangeListForFile(id, userNameInfo.userName, fileId, settings);

                return Json(model, JsonRequestBehavior.AllowGet);
            });
        }

        //
        // GET: /Diff/Revision/id=5,fileId=2,revision=1

        [Authorize]
        public ActionResult Revision(int id, int fileId, int revisionId)
        {
            return ExecApiMethod(() =>
            {
                var userNameInfo = UserName;
                var settings = UserSettings;
                var changeList = GetChangeList(id);
                var changeFile = GetChangeFile(changeList, fileId);

                var gen = new DiffGenerator(db);
                var baseReviewId = gen.GetBaseReviewId(userNameInfo.userName, changeList.Id);
                var fileVersions = changeFile.FileVersions.ToArray();
                var prev = 0;

                if (revisionId == 0 && fileVersions.Length == 1)
                {
                    var left = fileVersions.First();
                    var right = left;

                    var diffFile = gen.GenerateDiffFile(left, right, changeList.CL, baseReviewId, settings, force: false);
                    if (string.IsNullOrEmpty(diffFile) || !System.IO.File.Exists(diffFile))
                        throw new ApplicationException(string.Format("Failed to create a diff file for change list id: {0} file id: {1} revision id: {2}", id, fileId, revisionId));

                    return StreamFile(diffFile);
                }
                for (var idx = prev + 1; idx < fileVersions.Length; prev = idx++)
                {
                    if (revisionId != idx - 1)
                        continue;

                    var vid1 = fileVersions[prev].Id;
                    var vid2 = fileVersions[idx].Id;
                    var left = fileVersions[prev];
                    var right = fileVersions[idx];

                    var diffFile = gen.GenerateDiffFile(left, right, changeList.CL, baseReviewId, settings, force: false);
                    if (string.IsNullOrEmpty(diffFile) || !System.IO.File.Exists(diffFile))
                        throw new ApplicationException(string.Format("Failed to create a diff file for change list id: {0} file id: {1} revision id: {2}", id, fileId, revisionId));

                    return StreamFile(diffFile);
                }

                throw new ApplicationException(string.Format("Did not find revision for change list id: {0} file id: {1} revision id: {2}", id, fileId, revisionId));
            });
        }

        //
        // GET: /Diff/Ping/5

        [Authorize]
        public ActionResult Ping(int id)
        {
            return ExecApiMethod(() =>
            {
                var errors = new List<string>();

                var changeList = GetChangeList(id);
                if (changeList == null)
                    throw new ApplicationException(string.Format("Invalid change list id {0}", id));

                AddMailRequests(id, changeList, MailType.Reminder, errors);

                return errors.Count == 0 ? null : Failed(errors);
            });
        }

        //
        // GET: /Diff/Status/5

        [Authorize]
        public ActionResult Status(int id, string status, string CL)
        {
            return ExecApiMethod(() =>
            {
                if (!Utility.EnumIsValid<ChangeListStatus>(status))
                    throw new ApplicationException(string.Format("Invalid status value {0}", status));
                var statusId = Utility.ToEnum<ChangeListStatus>(status);

                var changeList = GetChangeList(id);
                if (changeList == null)
                    throw new ApplicationException(string.Format("Invalid change list id {0}", id));

                db.SetChangeListStatus(id, (int)statusId);

                AddMailRequests(id, changeList, MailType.StatusChange);

                changeList = GetChangeList(id);
                if (changeList == null)
                    throw new ApplicationException(string.Format("Invalid change list id {0} after changing status", id));

                db.Entry(changeList).Reload();
                var statusChange = new ChangeListStatusDto(changeList);
                ChangeListHub.Broadcast(statusChange, changeList.CL);

                return Json(statusChange, JsonRequestBehavior.AllowGet);
            });
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}