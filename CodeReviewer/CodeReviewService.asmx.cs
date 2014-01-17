using System;
using System.Globalization;
using System.Web.Script.Serialization;
using System.Runtime.Serialization.Json;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using DataModel;

namespace CodeReview5
{
    /// <summary>
    /// Summary description for CodeReviewService
    /// </summary>
    [WebService(Namespace = "http://carloswin")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [System.Web.Script.Services.ScriptService]
    public class CodeReviewService : System.Web.Services.WebService
    {
        private static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["DataConnectionString"].ConnectionString;

        private CodeReviewDataContext context = new CodeReviewDataContext(ConnectionString);

        public CodeReviewService()
        {
            CodeReviewUtility.DisableFileChangesMonitor();

            context.Connection.Open();
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ReviewDto AddReviewRequest(int changeId, string reviewerAlias)
        {
            using (context.Transaction = context.Connection.BeginTransaction(System.Data.IsolationLevel.Snapshot))
            {
                context.AddReviewRequest(changeId, reviewerAlias);
            }

            var review = (from c in context.Reviews
                              where c.ChangeListId == changeId
                              select c).FirstOrDefault();

            return new ReviewDto(review, true);
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ChangeListDto GetChangeList(int changeId)
        {
            var changeList = (from c in context.ChangeLists
                              where c.Id == changeId
                              select c).FirstOrDefault();

            return new ChangeListDto(changeList, true);
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ReviewDto GetReview(int reviewId)
        {
            var result = (from c in context.Reviews
                              where c.Id == reviewId
                              select c).FirstOrDefault();

            return new ReviewDto(result, true);
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<ChangeFileDto> GetChangeFiles(int changeId)
        {
            var changeList = (from c in context.ChangeLists
                          where c.Id == changeId
                          select c).FirstOrDefault();

            var changeFiles = new List<ChangeFileDto>();

            if (changeList != null && changeList.ChangeFiles != null)
                changeList.ChangeFiles.ToList().ForEach(cf => changeFiles.Add(new ChangeFileDto(cf, true)));

            return changeFiles;
        }

        public string Serialize(object obj)
        {
            var serial = new JavaScriptSerializer();
            var data = serial.Serialize(obj);
            System.Diagnostics.Debug.WriteLine(data);
            return data;
        }
    }
}