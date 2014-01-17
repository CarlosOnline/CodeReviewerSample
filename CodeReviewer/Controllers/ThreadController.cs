using CodeReviewer.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CodeReviewerContext = CodeReviewer.Models.CodeReviewerContext;

namespace CodeReviewer.Controllers
{
    public class ThreadController : BaseController
    {
        //
        // GET: /Thread/5

        public ActionResult Index(int id = 0)
        {
            return ExecApiMethod(() =>
                {
                    if (id == 0)
                    {
                        var threads = db.Comments;
                        var dtoList = new List<CommentDto>();
                        threads.ToList().ForEach(thread => dtoList.Add(new CommentDto(thread, true)));
                        return Json(dtoList, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var thread = db.Comments.Find(id);
                        if (thread != null)
                            return Json(new CommentDto(thread), JsonRequestBehavior.AllowGet);
                        else
                            return Json(new Failed(string.Format("Did not find {0}", id)));
                    }
                });
        }

        public ActionResult Group(int id)
        {
            return ExecApiMethod(() =>
                {
                    var group = db.CommentGroups.Find(id);
                    if (group != null)
                        return Json(new CommentGroupDto(group, true), JsonRequestBehavior.AllowGet);
                    else
                        return Json(new Failed(string.Format("Did not find {0}", id)), JsonRequestBehavior.AllowGet);
                });
        }

        public ActionResult All(int id = 0)
        {
            return ExecApiMethod(() =>
                {
                    if (id == 0)
                    {
                        var threads = db.Comments;
                        var dtoList = new List<CommentDto>();
                        threads.ToList().ForEach(thread => dtoList.Add(new CommentDto(thread, true)));
                        return Json(dtoList, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var threads = (from rv in db.Comments.AsNoTracking()
                                       where rv.GroupId == id
                                       select rv);

                        var dtoList = new List<CommentDto>();
                        threads.ToList().ForEach(thread => dtoList.Add(new CommentDto(thread, true)));
                        return Json(dtoList, JsonRequestBehavior.AllowGet);
                    }
                });
        }

        //
        // GET: /Thread/...
        public ActionResult Add(int id,
            string userName,
            string reviewerAlias,
            string commentText,
            int reviewRevision,
            int reviewerId,
            int fileVersionId,
            int groupId,
            int changeListId,
            string lineStamp,
            int status,
            string CL)
        {
            return ExecApiMethod(() =>
                {
                    var result = db.AddCommentEx(id, userName, reviewerAlias, commentText, reviewRevision, reviewerId, fileVersionId, groupId, changeListId, lineStamp, status);

                    if (result != 0)
                    {
                        Broadcast(result, groupId, CL);
                    }

                    return Index(result);
                });
        }

        //
        // GET: /Thread/Delete/5

        public ActionResult Delete(int id, string CL)
        {
            return ExecApiMethod(() =>
                {
                    var commentGroup = GetGroupDto(id);
                    db.DeleteCommentThread(id);
                    if (commentGroup != null)
                        Broadcast(0, commentGroup.id, CL);
                    return null;
                });
        }

        private CommentGroupDto GetGroupDto(int id, bool cached = false)
        {
            var thread = db.Comments.Find(id);
            if (thread != null && thread.GroupId.HasValue)
            {
                return db.FindCommentGroupDto(thread.GroupId.Value, cached);
            }
            return null;
        }

        private void Broadcast(int id, int groupId, string CL)
        {
            var commentGroup = GetGroupDto(id);
            if (commentGroup == null)
                commentGroup = db.FindCommentGroupDto(groupId);

            // Broadcast change to commentGroup
            if (commentGroup != null)
            {
                ChangeListHub.Broadcast(commentGroup, CL);
                return;
            }

            // Broadcast CommentGroup deleted
            if (groupId > 0)
                ChangeListHub.Broadcast(new DeleteNotification(groupId, "CommentGroup"), CL);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}