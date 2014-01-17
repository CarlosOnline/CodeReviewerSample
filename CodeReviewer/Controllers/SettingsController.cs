using CodeReviewer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CodeReviewer.Controllers
{
    public class SettingsController : BaseController
    {
        //
        // GET: /Settings/

        public ActionResult Index(int id, string userName, string key)
        {
            return ExecViewMethod(() =>
                {
                    var userNameInfo = UserName;
                    var data = ReviewUtil.UserContext(id, userNameInfo.userName, key, () => new UserSettingsDto(), db);

                    if (data != null)
                    {
                        return Json(data, JsonRequestBehavior.AllowGet);
                    }

                    return Failed("Failed to find data for {0} {1}", userName, key);
                });
        }

        public ActionResult Update(string userName, string key, string value)
        {
            return ExecApiMethod(() =>
                {
                    var settings = ReviewUtil.GetSettingsDto(key, value);
                    var userNameInfo = new UserName(userName, db);
                    var id = db.SetUserContext(key, value, userNameInfo.userName, "", settings.version);
                    return Index(id, userName, key);
                });
        }
    }
}
