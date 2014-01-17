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
    public class CommentController : BaseController
    {
        //
        // GET: /Comment/

        public ActionResult Index(int id = 0)
        {
            return ExecApiMethod(() =>
                {
                    if (id == 0)
                    {
                        var commentgroups = db.CommentGroups.Include(c => c.FileVersion).Include(c => c.Review);
                        var comments = new List<CommentGroupDto>();
                        commentgroups.ToList().ForEach(group => comments.Add(new CommentGroupDto(group, true)));

                        return Json(comments, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var comment = db.CommentGroups.Find(id);
                        if (comment == null)
                        {
                            throw new ApplicationException(string.Format("Did not find comment id: {0}", id));
                        }
                        else
                        {
                            var dto = new CommentGroupDto(comment, true);
                            return Json(dto, JsonRequestBehavior.AllowGet);
                        }
                    }
                });
        }

        //
        // GET: /Comment/Data/5

        public ActionResult Data(int id)
        {
            return ExecApiMethod(() =>
                {
                    var comment = FindCommentGroup(id);
                    if (comment == null)
                        throw new ApplicationException(string.Format("did not find comment id: {0}", id));

                    return Json(comment, JsonRequestBehavior.AllowGet);
                });
        }

        //
        // GET: /Comment/Add/...

        public ActionResult Add(int id, int fileVersionId, int line, string lineStamp, int status, string CL)
        {
            return ExecApiMethod(() =>
                {
                    var result = db.AddCommentGroup(id, fileVersionId, line, lineStamp, status);
                    Broadcast(result, CL);
                    return Data(result);
                });
        }

        //
        // GET: /Comment/Delete/5

        public ActionResult Delete(int id, string CL)
        {
            return ExecApiMethod(() =>
                {
                    db.DeleteCommentGroup(id);
                    Broadcast(id, CL);
                    return null;
                });
        }

        private CommentGroupDto FindCommentGroup(int id)
        {
            var comment = db.CommentGroups.Find(id);
            return comment != null ? new CommentGroupDto(comment, true) : null;
        }

        private void Broadcast(int id, string CL)
        {
            if (id == 0)
                return;

            var comment = FindCommentGroup(id);
            if (comment != null)
            {
                ChangeListHub.Broadcast(comment, CL);
                return;
            }

            ChangeListHub.Broadcast(new DeleteNotification(id, "CommentGroup"), CL);
        }

        private string GetChangeListCL(int id)
        {
            var CL = "";
            if (id != 0)
            {
                var comment = db.CommentGroups.Find(id);
                if (comment != null)
                {
                    var changeList = db.ChangeLists.Find(comment.ChangeListId);
                    if (changeList != null)
                        CL = changeList.CL;
                }
            }
            return CL;
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}