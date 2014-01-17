//#define newJquery

using System;
using System.IO;
using System.Web;
using System.Web.Optimization;

namespace CodeReviewer
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/knockout").Include(
                                   "~/Scripts/weak-map.js",
                                   "~/Scripts/knockout-2.3.0.debug.js",
                                   "~/Scripts/knockout.mapping-latest.debug.js",
                                   "~/Scripts/knockout-es5.js",
                                   "~/Scripts/knockout.es5.mapping.js",
                                   "~/Scripts/knockout.dirtyFlag.js"));

            bundles.Add(new ScriptBundle("~/bundles/qtip").Include(
                                    "~/Scripts/jquery.qtip.js"));
            //"~/Scripts/jquery.jsPlumb-1.5.2.js",
            //"~/Scripts/jquery.ui.touch - punch.js"));

            bundles.Add(new ScriptBundle("~/bundles/signalr").Include(
                                    "~/Scripts/jquery.signalR-1.1.2.js"));

            bundles.Add(new ScriptBundle("~/bundles/jquery_plugins").Include(
                                    "~/Scripts/jquery.color.js", // for jquery.animate - allow background-color to animate
                                    "~/Scripts/jquery.contextMenu.js",
                                    "~/Scripts/jquery.cookie.js",
                                    "~/Scripts/shortcut.js"));

            bundles.Add(new ScriptBundle("~/bundles/underscore").Include(
                                    "~/Scripts/underscore.js"));

            bundles.Add(new ScriptBundle("~/bundles/underscore").Include(
                                    "~/Scripts/underscore.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                        "~/Content/jquery.qtip.css",
                        "~/Content/jquery.contextMenu.css"));

            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                        "~/Content/themes/base/jquery.ui.core.css",
                        "~/Content/themes/base/jquery.ui.resizable.css",
                        "~/Content/themes/base/jquery.ui.selectable.css",
                        "~/Content/themes/base/jquery.ui.accordion.css",
                        "~/Content/themes/base/jquery.ui.autocomplete.css",
                        "~/Content/themes/base/jquery.ui.button.css",
                        "~/Content/themes/base/jquery.ui.dialog.css",
                        "~/Content/themes/base/jquery.ui.slider.css",
                        "~/Content/themes/base/jquery.ui.tabs.css",
                        "~/Content/themes/base/jquery.ui.datepicker.css",
                        "~/Content/themes/base/jquery.ui.progressbar.css",
                        "~/Content/themes/base/jquery.ui.theme.css"
                        ));

            bundles.Add(new StyleBundle("~/Content/themes/ui-lightness/css").IncludeDirectory(
                        "~/Content/themes/ui-lightness", "*.css"));

            bundles.Add(new StyleBundle("~/Content/site").Include(
                        "~/Content/Site.css"));

            bundles.Add(new StyleBundle("~/Content/app").Include(
                        "~/Content/App.css"));

            bundles.Add(new StyleBundle("~/Content/account").Include(
                        "~/Content/Account.css"));

            bundles.Add(new StyleBundle("~/Content/all").Include(
                        "~/Content/all.css"));

            bundles.Add(new StyleBundle("~/Content/diff").Include(
                        "~/Content/Diff.css"));

            bundles.Add(new ScriptBundle("~/bundles/app_Scripts")
                .IncludeDirectory("~/App_Scripts", "*.js"));
        }
    }

    public class LessTransform : IBundleTransform
    {
        public void Process(BundleContext context, BundleResponse response)
        {
            // TODO: pre-Compile this, or use temporary file
            response.Content = dotless.Core.Less.Parse(response.Content);
#if !LessDebug
            var name = context.BundleVirtualPath.Replace("/", "_");
            File.WriteAllText(@"c:\temp\" + name + ".css", response.Content);
#endif
            if (string.IsNullOrEmpty(response.Content))
                throw new Exception(string.Format("No content from less compilation of {0}", context.BundleVirtualPath));
            response.ContentType = "text/css";
        }
    }
}