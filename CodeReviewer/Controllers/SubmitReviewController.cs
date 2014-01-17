using CodeReviewer;
using CodeReviewer.Models;
using CodeReviewer.Util;
using SourceControl;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace CodeReviewer.Controllers
{
    public class SubmitReviewController : BaseController
    {
        //
        // GET: /SubmitReview

        [Authorize]
        public ActionResult Index(string cl = "")
        {
            return ExecViewMethod(() =>
                {
                    var userNameInfo = UserName;
                    if (string.IsNullOrEmpty(userNameInfo.userName) || userNameInfo.userName == userNameInfo.emailAddress)
                    {
                        // require userName for listing changes - redirect to register page to update userName
                        return RedirectToAction("Register", "Account");
                    }

                    var settings = UserSettings;
                    var items = PendingChangeListItems;
                    var data = items.ToList();

                    var js = new JavaScriptSerializer();
                    ViewBag.ChangeListDisplayItems = js.Serialize(data);
                    ViewBag.Message = "Code Reviewer";
                    ViewBag.UserSettings = js.Serialize(ReviewUtil.GenerateUserContext(0, userNameInfo.userName, "settings", settings));

                    var submit = new SubmitReview();
                    return View(submit);
                });
        }

        //
        // POST: /SubmitReview

        [HttpPost]
        [Authorize]
        public ActionResult Index(SubmitReview model)
        {
            return ExecViewMethod(() =>
                {
                    if (ModelState.IsValid)
                    {
                        var reviewers = new List<string>();
                        if (model.Reviewers != null)
                            reviewers.AddRange(model.Reviewers.Split(new[] { ';', ',', ' ', '\t', '\n' }));
                        
                        var optionalReviewers = new List<string>();
                        if (model.OptionalReviewers != null)
                            optionalReviewers.AddRange(model.OptionalReviewers.Split(new[] { ';', ',', ' ', '\t', '\n' }));

                        ReviewUtil.ProcessCodeReview(model.CL,
                                                     reviewers,
                                                     optionalReviewers,
                                                     model.Title,
                                                     model.Description,
                                                     db);

                        var changeList = (from cv in db.ChangeLists.AsNoTracking()
                                          where cv.CL == model.CL
                                          select cv).FirstOrDefault();
                        if (changeList == null)
                            return
                                HttpNotFound(string.Format("Error did not insert review for change list {0}", model.CL));

                        return RedirectToAction("Index", "ChangeList", new { cl = changeList.CL });
                    }

                    return View(model.CL);
                });
        }

        private List<SourceControl.Change> PendingChangeLists
        {
            get
            {
                var results = new List<SourceControl.Change>();
                if (string.IsNullOrEmpty(UserName.userName))
                    return results;

                var p4Settings = PerforceInterface.GetSettings();
                var settings = new SourceControlSettings();
                SourceType.LoadSettings(SourceControlType.PERFORCE, settings);

                if (settings.Client != null)
                    p4Settings.Client = settings.Client;
                if (settings.Port != null)
                    p4Settings.Port = settings.Port;
                if (settings.User != null)
                    p4Settings.User = settings.User;
                if (settings.Password != null)
                    p4Settings.Password = settings.Password;
                if (settings.ClientExe != null)
                    p4Settings.ClientExe = settings.ClientExe;

                var userName = UserName.userName;
                if (string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(p4Settings.User))
                {
                    userName = p4Settings.User;
                }

                if (p4Settings.ClientExe == null)
                {
                    throw new Exception("Could not find p4.exe. Consider putting it in the path, or on the command line.");
                }

                if (p4Settings.Port == null)
                {
                    throw new Exception("Could not determine the server port. " +
                                      "Consider putting it on the command line, or in p4 config file.");
                }

                if (p4Settings.Client == null)
                {
                    throw new Exception("Could not determine the perforce client. " +
                                      "Consider putting it on the command line, or in p4 config file.");
                }

                var sourceControl = PerforceInterface.GetInstance(p4Settings.ClientExe, p4Settings.Port,
                                                              p4Settings.Client, p4Settings.User,
                                                              p4Settings.Password);

                if (!sourceControl.Connect())
                {
                    throw new Exception("Failed to connect to the source control system.");
                }

                results.AddRange(sourceControl.GetChanges(userName));

                sourceControl.Disconnect();

                return results;
            }
        }

        private List<ChangeListDisplayItem> PendingChangeListItems
        {
            get
            {
                var userNameInfo = UserName;
                var settings = UserSettings;

                var changelists = (from item in db.ChangeLists.AsNoTracking()
                                   where (item.UserName == userNameInfo.fullUserName ||
                                           item.UserName == userNameInfo.userName ||
                                           item.ReviewerAlias == userNameInfo.reviewerAlias)
                                   select item).OrderByDescending(x => x.TimeStamp);
                var existingCLs = new List<string>();
                foreach (var changeList in changelists)
                {
                    existingCLs.Add(changeList.CL);
                }

                var results = new List<ChangeListDisplayItem>();
                foreach (var pending in PendingChangeLists)
                {
                    if (existingCLs.Contains(pending.ChangeListId))
                        continue;

                    var item = new ChangeListDisplayItem();
                    item.CL = pending.ChangeListId;
                    item.description = pending.Description;
                    item.shortDescription = pending.Description.FirstLine();
                    item.server = "";
                    results.Add(item);
                }
                return results;
            }
        }
    }
}
