using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Xml;
using CodeReviewer.Mapping;

namespace CodeReviewer
{
    public partial class CodeReviewerContext : DbContext
    {
        public void AddChangeList(int sourceControlInstanceId, string sdClientName, string changeList, string description, DateTime toUniversalTime, ref int? changeId)
        {
            throw new NotImplementedException();
        }

        public void AddAttachment(int value, string linkDescr, string link, ref int? attachmentId)
        {
            throw new NotImplementedException();
        }

        public void AddReviewRequest(int value, string invitee)
        {
            throw new NotImplementedException();
        }

        // @UserName nvarchar(50), @ChangeId INT, @Text NVARCHAR (2048)=NULL, @Status TINYINT=NULL, @Result INT OUTPUT
        public int AddReview(string userName, int cl, string title, int status, ref int result)
        {
            var pResult = new SqlParameter
            {
                ParameterName = "result",
                DbType = System.Data.DbType.Int32,
                Direction = System.Data.ParameterDirection.Output
            };
            var resp = Database.SqlQuery<int>("exec AddReview @UserName, @ChangeId, @Text, @Status, @result",
                                                   new SqlParameter("@UserName", userName),
                                                   new SqlParameter("@ChangeId", cl),
                                                   new SqlParameter("@Text", title),
                                                   new SqlParameter("@Status", status),
                                                   pResult);
            result = resp.First();
            return result;
        }

        // @FileId INT, @Revision INT, @Action INT, @TimeStamp DATETIME, @IsText BIT, @IsFullText BIT, @IsRevisionBase BIT, @Text VARCHAR (MAX), @result INT OUTPUT
        public int AddVersion(int? fid, int revision, int action, DateTime? dateTime, bool isText, bool isFullText, bool isRevisionBase, object text, ref int result)
        {
            var pResult = new SqlParameter
            {
                ParameterName = "result",
                DbType = System.Data.DbType.Int32,
                Direction = System.Data.ParameterDirection.Output
            };
            var resp = Database.SqlQuery<int>("exec AddVersion @FileId, @Revision, @Action, @TimeStamp, @IsText, @IsFullText, @IsRevisionBase, @Text, @result",
                                                   new SqlParameter("@FileId", fid),
                                                   new SqlParameter("@Revision", revision),
                                                   new SqlParameter("@Action", action),
                                                   new SqlParameter("@TimeStamp", dateTime),
                                                   new SqlParameter("@IsText", isText),
                                                   new SqlParameter("@IsFullText", isFullText),
                                                   new SqlParameter("@IsRevisionBase", isRevisionBase),
                                                   new SqlParameter("@Text", text),
                                                   pResult);
            result = resp.First();
            return result;
        }

        // @ReviewerAlias NVARCHAR (50), @ChangeListId INT, @result INT OUTPUT
        public int AddReviewer(string reviewer, int cl, ref int result)
        {
            var pResult = new SqlParameter
            {
                ParameterName = "result",
                DbType = System.Data.DbType.Int32,
                Direction = System.Data.ParameterDirection.Output
            };
            var resp = Database.SqlQuery<int>("exec AddReviewer @ReviewerAlias, @ChangeListId, @result",
                                                   new SqlParameter("@ReviewerAlias", reviewer),
                                                   new SqlParameter("@ChangeListId", cl),
                                                   pResult);
            result = resp.First();
            return result;
        }

        public void RemoveFile(int id)
        {
            throw new NotImplementedException();
        }

        //@ChangeId INT, @LocalFile NVARCHAR (512), @ServerFile NVARCHAR (512), @ReviewRevision INT, @result INT OUTPUT

        public int AddFile(int? changeId, string localFileName, string serverFileName, int reviewRevison, ref int result)
        {
            var pResult = new SqlParameter
            {
                ParameterName = "result",
                DbType = System.Data.DbType.Int32,
                Direction = System.Data.ParameterDirection.Output
            };
            var resp = Database.SqlQuery<int>("exec AddFile @ChangeId, @LocalFile, @ServerFile, @ReviewRevision, @result",
                                                   new SqlParameter("@ChangeId", changeId),
                                                   new SqlParameter("@LocalFile", localFileName),
                                                   new SqlParameter("@ServerFile", serverFileName),
                                                   new SqlParameter("@ReviewRevision", reviewRevison),
                                                   pResult);
            result = resp.First();
            return result;
        }

        public void DeleteChangeList(int i)
        {
            throw new NotImplementedException();
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
    }
   }
}
