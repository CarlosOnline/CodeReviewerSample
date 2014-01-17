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
    public class ChangeListController : BaseController
    {
        //
        // GET: /ChangeList/

        [Authorize]
        public ActionResult Index(string cl = "")
        {
            return ExecViewMethod(() =>
                {
                    if (string.IsNullOrEmpty(cl))
                    {
                        return RedirectToAction("All");
                    }

                    var changeList = db.FindChangeListByCl(cl);
                    if (changeList == null)
                    {
                        var connectionString = db.Database.Connection.ConnectionString;
                        throw new ApplicationException(string.Format("Did not find change list {0} - {1}", cl, connectionString));
                    }

                    if (changeList.Stage == (int)ChangeListStatus.Deleted)
                        return RedirectToAction("All");

                    return Diff(changeList);
                });
        }

        //
        // GET: /ChangeList/Review/123

        [Authorize]
        public ActionResult Review(string cl)
        {
            if (string.IsNullOrEmpty(cl))
            {
                return RedirectToAction("All");
            }

            var changeList = db.FindChangeListByCl(cl);

            if (changeList == null)
            {
                var connectionString = db.Database.Connection.ConnectionString;
                throw new ApplicationException(string.Format("Did not find change list {0} - {1}", cl, connectionString));
            }

            if (changeList.Stage == (int)ChangeListStatus.Deleted)
                return RedirectToAction("All");

            return Diff(changeList);
        }

        //
        // GET: /ChangeList/Diff/5

        [Authorize]
        public ActionResult Diff(int id = 0, int fileId = 0)
        {
            return ExecViewMethod(() =>
            {
                var userNameInfo = UserName;
                var settings = UserSettings;
                var model = GetChangeListForFile(id, userNameInfo.userName, fileId, settings);

                var changeListSettings = ReviewUtil.ChangeListSettings(0, userNameInfo.userName, model.CL, db);
                var js = new JavaScriptSerializer();
                ViewBag.ChangeList = js.Serialize(model);
                ViewBag.Comments = js.Serialize(model.comments);
                ViewBag.UserSettings = js.Serialize(ReviewUtil.GenerateUserContext(0, userNameInfo.userName, "settings", settings));
                ViewBag.ChangeListSettings = js.Serialize(ReviewUtil.GenerateUserContext(0, userNameInfo.userName, model.CL, changeListSettings));

                //return RedirectToAction("Index", new { cl = model.CL });
                return View(model);
            });
        }

        //
        // GET: /ChangeList/Diff/5

        private ActionResult Diff(ChangeList changeList)
        {
            var userNameInfo = UserName;
            var settings = UserSettings;
            var changeListSettings = ReviewUtil.ChangeListSettings(0, userNameInfo.userName, changeList.CL, db);
            var model = GetChangeListForFile(changeList.Id, userNameInfo.userName, changeListSettings.fileId, settings);

            var js = new JavaScriptSerializer();
            ViewBag.ChangeList = js.Serialize(model);
            ViewBag.Comments = js.Serialize(model.comments);
            ViewBag.UserSettings = js.Serialize(ReviewUtil.GenerateUserContext(0, userNameInfo.userName, "settings", settings));
            ViewBag.ChangeListSettings = js.Serialize(ReviewUtil.GenerateUserContext(0, userNameInfo.userName, model.CL, changeListSettings));

            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}