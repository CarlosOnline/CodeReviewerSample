using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using CodeReviewer.Models;

namespace CodeReviewer.Controllers
{
    public class HomeController : BaseController
    {
        [Authorize]
        public ActionResult Index(string cl = "")
        {
            return ExecViewMethod(() =>
                {
                    if (!string.IsNullOrEmpty(cl))
                    {
                        var changeList = db.FindChangeListByCl(cl);
                        if (changeList != null)
                        {
                            return RedirectToAction("Index", "ChangeList", new { cl = changeList.CL });
                        }
                    }
                    return All();
                });
        }

        private ActionResult All()
        {
            var userNameInfo = UserName;
            var settings = UserSettings;

            var changelists = (from item in db.ChangeLists.AsNoTracking()
                                where (item.UserName == userNameInfo.fullUserName ||
                                        item.UserName == userNameInfo.userName ||
                                        item.ReviewerAlias == userNameInfo.reviewerAlias) &&
                                        item.Stage != (int)ChangeListStatus.Deleted
                                select item).OrderByDescending(x => x.TimeStamp);

            var model = new List<ChangeListDisplayItem>();
            foreach (var item in changelists)
            {
                model.Add(new ChangeListDisplayItem(item));
            }

            if (model.Count == 0)
            {
                return RedirectToAction("Index", "SubmitReview");
            }

            var js = new JavaScriptSerializer();
            var data = model.ToList();

            ViewBag.ChangeListDisplayItems = js.Serialize(data);
            ViewBag.Message = "Code Reviewer";
            ViewBag.UserSettings = js.Serialize(ReviewUtil.GenerateUserContext(0, userNameInfo.userName, "settings", settings));

            return View(data);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Perform your code reviews";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Contact " + ConfigurationManager.AppSettings["supportAlias"] + " for questions";

            return View();
        }
    }
}
