//#define Debug
using CodeReviewer.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;

namespace CodeReviewer.Models
{
    public partial class CodeReviewerContext : DbContext
    {
        private SqlParameter GetResultParameter(ref int result)
        {
            return new SqlParameter
            {
                ParameterName = "@result",
                DbType = System.Data.DbType.Int32,
                Direction = System.Data.ParameterDirection.Output,
                Value = result,
            };
        }

        public int ExecWithResult(string query, params SqlParameter[] args)
        {
            try
            {
                var sqlArgs = args.ToList();
                int result = -1;
                var pResult = GetResultParameter(ref result);
                sqlArgs.Add(pResult);

                var names = sqlArgs.Select(arg => ((SqlParameter)arg).ParameterName).ToList();
                var queryStr = query + " " + string.Join(",", names) + " OUTPUT";
#if Debug
                var values = GetValuesList(sqlArgs.ToArray());
                Log.Info("Query:\n {0} {1} OUTPUT", query, string.Join(",", values));
#endif
                var resp = Database.ExecuteSqlCommand(queryStr, sqlArgs.ToArray());
                //Log.Info("response: {0} - query: {1}", resp, query);

                if (pResult.Value == System.DBNull.Value)
                    return result;
                return (int)pResult.Value;
            }
            catch (Exception ex)
            {
                try
                {
                    var values = GetValuesList(args);
                    Log.Info("Query exception\nquery: {0}\nvalues: {1}\nexception: {2}", query, string.Join(",", values), ex.ToString());
                }
                catch
                {
                }
                throw;
            }
        }

        public void ExecPlain(string query, params object[] args)
        {
            try
            {
                var names = args.Select(arg => ((SqlParameter)arg).ParameterName).ToList();
                var queryStr = query + " " + string.Join(",", names);
#if Debug
                var values = GetValuesList(args);
                Log.Info("Query:\n {0} {1}", query, string.Join(",", values));
#endif
                Database.ExecuteSqlCommand(queryStr, args);
            }
            catch (Exception ex)
            {
                try
                {
                    var values = GetValuesList(args);
                    Log.Info("Query exception\nquery: {0}\nvalues: {1}\nexception: {2}", query, string.Join(",", values), ex.ToString());
                }
                catch
                {
                }
                throw;
            }
        }

        public int AddCommentGroup(int groupId, int fileVersionId, int line, string lineStamp, int status)
        {
            return ExecWithResult("exec AddCommentGroup",
                   new SqlParameter("@GroupId", groupId),
                   new SqlParameter("@FileVersionId", fileVersionId),
                   new SqlParameter("@Line", line),
                   new SqlParameter("@LineStamp", lineStamp),
                   new SqlParameter("@Status", status));
        }

        public int AddComment(int commentId, string userName, string reviewerAlias, string commentText, int reviewRevision, int fileVersionId, int groupId)
        {
            return ExecWithResult("exec AddComment",
                   new SqlParameter("@CommentId", commentId),
                   new SqlParameter("@UserName", userName),
                   new SqlParameter("@ReviewerAlias", reviewerAlias),
                   new SqlParameter("@CommentText", commentText),
                   new SqlParameter("@ReviewRevision", reviewRevision),
                   new SqlParameter("@FileVersionId", fileVersionId),
                   new SqlParameter("@GroupId", groupId));
        }

        public int AddCommentEx(int commentId, string userName, string reviewerAlias, string commentText, int reviewRevision, int reviewerId, int fileVersionId, int groupId, int changeListId, string lineStamp, int status)
        {
            return ExecWithResult("exec AddCommentEx",
                   new SqlParameter("@CommentId", commentId),
                   new SqlParameter("@UserName", userName),
                   new SqlParameter("@ReviewerAlias", reviewerAlias),
                   new SqlParameter("@CommentText", commentText),
                   new SqlParameter("@ReviewRevision", reviewRevision),
                   new SqlParameter("@ReviewerId", reviewerId),
                   new SqlParameter("@FileVersionId", fileVersionId),
                   new SqlParameter("@GroupId", groupId),
                   new SqlParameter("@ChangeListId", changeListId),
                   new SqlParameter("@LineStamp", lineStamp),
                   new SqlParameter("@Status", status));
        }

        public int AddChangeList(int sourceControl, string userName, string reviewerAlias, string userClient, string CL, string url, string title, string description)
        {
            return ExecWithResult("exec AddChangeList",
                   new SqlParameter("@SourceControl", sourceControl),
                   new SqlParameter("@UserName", userName),
                   new SqlParameter("@ReviewerAlias", reviewerAlias),
                   new SqlParameter("@UserClient", userClient),
                   new SqlParameter("@CL", CL),
                   new SqlParameter("@Url", url),
                   new SqlParameter("@Title", title),
                   new SqlParameter("@Description", description));
        }

        public void SetChangeListStatus(int id, int status)
        {
            ExecPlain("exec SetChangeListStatus",
                           new SqlParameter("@Id", id),
                           new SqlParameter("@Staus", status));
        }

        public int AddAttachment(int value, string linkDescr, string link)
        {
            throw new NotImplementedException();
        }

        public void AddReviewRequest(int value, string invitee)
        {
            throw new NotImplementedException();
        }

        public int AddReview(string userName, string reviewerAlias, int cl, string title, int status)
        {
            return ExecWithResult("exec AddReview",
                            new SqlParameter("@UserName", userName),
                            new SqlParameter("@ReviewerAlias", reviewerAlias),
                            new SqlParameter("@ChangeId", cl),
                            new SqlParameter("@Text", title),
                            new SqlParameter("@Status", status));
        }

        public int AddVersion(int? fid, int revision, int reviewRevision, int action, DateTime? dateTime, bool isText, bool isFullText, bool isRevisionBase, object text)
        {
            return ExecWithResult("exec AddVersion",
                            new SqlParameter("@FileId", fid),
                            new SqlParameter("@Revision", revision),
                            new SqlParameter("@ReviewRevision", reviewRevision),
                            new SqlParameter("@Action", action),
                            new SqlParameter("@TimeStamp", dateTime),
                            new SqlParameter("@IsText", isText),
                            new SqlParameter("@IsFullText", isFullText),
                            new SqlParameter("@IsRevisionBase", isRevisionBase),
                            new SqlParameter("@Text", text));
        }

        public int AddReviewer(int id, string userName, string reviewerAlias, int changeListId, int status, int requestType)
        {
            return ExecWithResult("exec AddReviewer",
                            new SqlParameter("@Id", id),
                            new SqlParameter("@UserName", userName),
                            new SqlParameter("@ReviewerAlias", reviewerAlias),
                            new SqlParameter("@ChangeListId", changeListId),
                            new SqlParameter("@Status", status),
                            new SqlParameter("@RequestType", requestType));
        }

        public int AddMailRequest(int id, string reviewerAlias, int changeListId, int requestType)
        {
            return ExecWithResult("exec AddMailRequest",
                            new SqlParameter("@Id", id),
                            new SqlParameter("@ReviewerAlias", reviewerAlias),
                            new SqlParameter("@ChangeListId", changeListId),
                            new SqlParameter("@RequestType", requestType));
        }

        public void DeleteReviewer(int id)
        {
            ExecPlain("exec DeleteReviewer",
                            new SqlParameter("@Id", id));
        }

        public void RemoveFile(int id)
        {
            throw new NotImplementedException();
        }

        public int AddFile(int? changeId, string localFileName, string serverFileName, int reviewRevison)
        {
            return ExecWithResult("exec AddFile",
                            new SqlParameter("@ChangeId", changeId),
                            new SqlParameter("@LocalFile", localFileName),
                            new SqlParameter("@ServerFile", serverFileName),
                            new SqlParameter("@ReviewRevision", reviewRevison));
        }

        public void DeleteChangeList(int i)
        {
            throw new NotImplementedException();
        }

        public void DeleteCommentGroup(int id)
        {
            Database.ExecuteSqlCommand("exec DeleteCommentGroup @GroupId",
                                            new SqlParameter("@GroupId", id));
        }

        public int DeleteCommentThread(int id)
        {
            return ExecWithResult("exec DeleteComment",
                                new SqlParameter("@CommentId", id));
        }

        public void RenameChangeList(int i, string newCl)
        {
            throw new NotImplementedException();
        }

        public void ReopenChangeList(int i)
        {
            throw new NotImplementedException();
        }

        public void SubmitChangeList(int cid)
        {
            throw new NotImplementedException();
        }

        public int SetUserContext(string key, string value, string userName, string reviewerAlias, int version)
        {
            return ExecWithResult("exec SetUserContext",
                   new SqlParameter("@Key", key),
                   new SqlParameter("@Value", value),
                   new SqlParameter("@UserName", userName),
                   new SqlParameter("@ReviewerAlias", reviewerAlias),
                   new SqlParameter("@Version", version));
        }

        public ChangeList FindChangeListByCl(string cl)
        {
            try
            {
                var changeList = (from item in this.ChangeLists
                                  where item.CL == cl
                                  select item).FirstOrDefault();
                return changeList;
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("FindChangeListByCl({0}) exception", ex.ToString()));
                return null;
            }
        }

        public string Left(string value, int length = 50)
        {
            return value.Substring(0, Math.Min(value.Length, length));
        }

        private List<string> GetValuesList(object[] args)
        {
            var values = new List<string>();
            args.ToList().ForEach(arg =>
            {
                var param = (SqlParameter)arg;
                var value = param.Value;
                if (value == null)
                    value = "NULL";
                else if (value.GetType() != typeof(int))
                    value = "'" + value + "'";
                values.Add(Left(value.ToString()));
            });
            return values;
        }

        private List<string> GetValuesList(SqlParameter[] args)
        {
            var values = new List<string>();
            args.ToList().ForEach(arg =>
            {
                var param = arg;
                var value = param.Value;
                if (value == null)
                    value = "NULL";
                else if (value.GetType() != typeof(int))
                    value = "'" + value + "'";
                values.Add(Left(value.ToString()));
            });
            return values;
        }
    }
}
