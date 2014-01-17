using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using CodeReviewer.Models;

namespace CodeReviewer
{
    public abstract class ApplicationViewPage<T> : WebViewPage<T>
    {
        protected override void InitializePage()
        {
            SetViewBagDefaultProperties();
            base.InitializePage();
        }

        private void SetViewBagDefaultProperties()
        {
            var controller = ViewContext.Controller as Controllers.BaseController;
            var userInfo = controller != null ? controller.UserName : new UserName(User.Identity.Name, null);
            ViewBag.UseLDAP = UserName.UseLDAP;
            ViewBag.UserName = userInfo.userName;
            ViewBag.ReviewerAlias = userInfo.reviewerAlias;
            ViewBag.DisplayName = userInfo.displayName;
        }
    }
}
