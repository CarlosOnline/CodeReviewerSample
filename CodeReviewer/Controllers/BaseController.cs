using CodeReviewer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace CodeReviewer.Controllers
{
    public class Success
    {
        public readonly string Result = "OK";

        public string Message { get; private set; }

        public Success(string msg = "")
        {
            Message = msg;
        }
    }

    public class Failed
    {
        public string Result = "Internal Server Error";

        public string Message { get; private set; }

        public bool Error = true;

        public Failed(string msg = "")
        {
            Message = msg;
        }
    }

    public class BaseController : Controller
    {
        public Models.CodeReviewerContext db = new Models.CodeReviewerContext();

        protected UserName _userName = null;
        protected UserSettingsDto _userSettings = null;
        protected ChangeList _changeList = null;

        public UserName UserName
        {
            get
            {
                if (_userName == null)
                {
                    _userName = new UserName(User.Identity.Name, db);
                }
                return _userName;
            }
        }

        public UserSettingsDto UserSettings
        {
            get
            {
                if (_userSettings == null)
                    _userSettings = ReviewUtil.UserSettings(0, UserName.userName, db);
                return _userSettings;
            }
        }

        protected ActionResult ExecViewMethod(Func<ActionResult> worker)
        {
            var errors = new List<string>();
            try
            {
                var result = worker();
                if (result == null)
                    throw new ApplicationException(string.Format("Invalid operation - no view returned and no errors reported"));

                return result;
            }
            catch (Exception ex)
            {
                errors.Add(ex.ToString());
                var error = string.Join("\n", errors.ToArray());
                var message = string.Format("Errors: {0}", error);
                Log.Error(message);
                throw;
                //return HttpNotFound(message);
            }
        }

        protected ActionResult ExecApiMethod(Func<ActionResult> worker)
        {
            var errors = new List<string>();
            ActionResult result = null;
            try
            {
                result = worker();
            }
            catch (Exception ex)
            {
                errors.Add(ex.ToString());
            }

            if (result != null)
                return result;

            if (errors.Count == 0)
                return Json(new Success(), JsonRequestBehavior.AllowGet);

            return Failed(errors);
        }

        protected ActionResult Success(string msg = "")
        {
            return Json(new Success(msg), JsonRequestBehavior.AllowGet);
        }

        protected ActionResult Success(string msg, params object[] args)
        {
            var message = string.Format(msg, args);
            return Success(message);
        }

        protected ActionResult Failed(string msg = "")
        {
            return Json(new Failed(msg), JsonRequestBehavior.AllowGet);
        }

        protected ActionResult Failed(string msg, params object[] args)
        {
            var message = string.Format(msg, args);
            return Failed(message);
        }

        protected ActionResult Failed(string msg, List<string> errors)
        {
            var error = string.Join("\n", errors.ToArray());
            var message = string.Format("{0}\nErrors: {1}", msg, error);
            return Failed(message);
        }

        protected ActionResult Failed(List<string> errors)
        {
            var error = string.Join("\n", errors.ToArray());
            var message = string.Format("Errors: {0}", error);
            return Failed(message);
        }

        protected bool FallBackToWrite { get; set; } // get from config

        protected ActionResult StreamFile(string filePath, string contentType = "text/html", bool inline = false)
        {
            var fi = new FileInfo(filePath);
            if (!fi.Exists)
                throw new Exception(string.Format("Missing requested file: {0}", filePath));

            return File(filePath, contentType);
        }

        public FileStreamResult StreamFile(string filePath, string data, string contentType = "text/html")
        {
            FileInfo info = new FileInfo(filePath);
            if (!info.Exists)
            {
                using (StreamWriter writer = info.CreateText())
                {
                    writer.WriteLine(data);
                }
            }

            return File(info.OpenRead(), contentType);
        }

        protected ChangeList GetChangeList(int id)
        {
            if (_changeList != null && _changeList.Id == id)
                return _changeList;

            if (id > 0)
                _changeList = db.ChangeLists.Find(id);

            if (_changeList == null || _changeList.ChangeFiles.Count == 0)
                throw new ApplicationException(string.Format("Invalid change list id {0}", id));

            return _changeList;
        }

        protected ChangeFile GetChangeFile(ChangeList changeList, int fileId)
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

        protected ChangeListDto GetChangeListForFile(int id, string userName, int fileId, UserSettingsDto settings)
        {
            var changeList = GetChangeList(id);
            var changeFile = GetChangeFile(changeList, fileId);

            var dto = new ChangeListDto(changeList, true);
            var changeFileDto = dto.changeFiles.Find(item => item.id == fileId);
            if (changeFile == null || changeFile.ChangeList == null)
                throw new ApplicationException(string.Format("Error creating ChangeListDto model for change list id: {0} cl: {1} file: {2}", changeList.Id, changeList.CL, fileId));

            return new ChangeListDto(changeList, true);
        }

        protected void AddMailRequests(int id, ChangeList changeList, MailType requestType, List<string> errors = null)
        {
            var reviewers = (from cl in db.Reviewers.AsNoTracking()
                             where cl.ChangeListId == id
                             select cl);
            foreach (var reviewer in reviewers)
            {
                var result = db.AddMailRequest(reviewer.Id, reviewer.ReviewerAlias, id, (int)requestType);
                if (result == 0 && errors != null)
                    errors.Add(string.Format("Failed to add {0} request for {1} for {2}", requestType, reviewer.ReviewerAlias,
                                             changeList.CL));
            }
        }
    }
}
