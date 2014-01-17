using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CodeReviewer
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Add route for login.aspx to make publishing easier
            routes.MapRoute(
                   "Login", // Route name
                   "login.aspx", // URL with parameters
                   new { controller = "Account", action = "Login" } // Parameter defaults
            );

            routes.MapRoute(
                   "ChangeList", // Route name
                   "ChangeList/{cl}", // URL with parameters
                   new { controller = "ChangeList", action = "CL" } // Parameter defaults
            );

            routes.MapRoute(
                   "Review", // Route name
                   "Review/{cl}", // URL with parameters
                   new { controller = "ChangeList", action = "Index" } // Parameter defaults
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}