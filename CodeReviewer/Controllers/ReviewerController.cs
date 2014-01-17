using CodeReviewer.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Dynamic;
using System.Linq;
using System.Web.Mvc;

namespace CodeReviewer.Controllers
{
    public class ReviewerController : BaseController
    {
        //
        // GET: /Reviewer/

        public ActionResult Index(int id)
        {
            return ExecApiMethod(() =>
                {
                    var reviewer = db.Reviewers.Find(id);
                    if (reviewer != null)
                    {
                        var dto = new ReviewerDto(reviewer, true);
                        return Json(dto, JsonRequestBehavior.AllowGet);
                    }

                    return Failed("did not find reviewer id: {0}", id);
                });
        }

        //
        // GET: /Reviewer/...

        public ActionResult Add(int id, string userName, string reviewerAlias, int changeListId, int status, int requestType, string CL)
        {
            return ExecApiMethod(() =>
                {
                    if (string.IsNullOrWhiteSpace(userName))
                    {
                        var userNameInfo = new UserName(reviewerAlias, db);
                        if (!string.IsNullOrWhiteSpace(userNameInfo.userName) && !string.IsNullOrWhiteSpace(userNameInfo.reviewerAlias))
                        {
                            userName = userNameInfo.userName;
                            reviewerAlias = userNameInfo.reviewerAlias;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(reviewerAlias))
                        throw new ApplicationException(string.Format("Invalid reviewerAlias: '{0}' or userName: '{1}'", reviewerAlias, userName));

                    var result = db.AddReviewer(id, userName, reviewerAlias, changeListId, status, requestType);
                    Broadcast(result, CL);
                    return Index(result);
                });
        }

        //
        // GET: /Reviewer/Delete/5

        public ActionResult Delete(int id, string CL)
        {
            return ExecApiMethod(() =>
            {
                db.DeleteReviewer(id);
                Broadcast(id, CL);
                return Json(new Success(), JsonRequestBehavior.AllowGet);
            });
        }

        private ReviewerDto FindReviewer(int id)
        {
            var reviewer = db.Reviewers.Find(id);
            return reviewer != null ? new ReviewerDto(reviewer, true) : null;
        }

        private void Broadcast(int id, string CL)
        {
            if (id == 0)
                return;

            var reviewer = FindReviewer(id);
            if (reviewer != null)
            {
                ChangeListHub.Broadcast(reviewer, CL);
                return;
            }

            ChangeListHub.Broadcast(new DeleteNotification(id, "Reviewer"), CL);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
