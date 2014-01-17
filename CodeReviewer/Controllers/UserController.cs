using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.Mvc;
using WebMatrix.WebData;

namespace CodeReviewer.Controllers
{
    public class UserController : BaseController
    {
        //
        // GET: /User/

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(Models.UserDto user)
        {
            if (ModelState.IsValid)
            {
                var result = WebSecurity.CreateUserAndAccount(user.email, user.password, null, false);
                if (WebSecurity.Login(user.email, user.password, user.rememberMe))
                {
                    // Login with password
                    return RedirectToAction("Index", "Home");
                }
            }
            return View(user);
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(Models.UserDto user)
        {
            if (ModelState.IsValid)
            {
                if (WebSecurity.Login(user.email, user.password, user.rememberMe))
                {
                    // Login with password
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Login data is incorrect!");
                }
#if Old
                var id = db.LoginUser(null, user.email, user.password, null);
                if (id == (int) Models.LoginResult.ExpiredToken)
                {
                    // Login with password
                    return RedirectToAction("Index");
                }
                else if (id > 0)
                {
                    FormsAuthentication.SetAuthCookie(user.userName, user.rememberMe);
                    return RedirectToAction("Index", "Home");
                }
#endif
            }
            return View(user);
        }
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }
    }
}
