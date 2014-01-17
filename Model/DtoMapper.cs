using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Script.Serialization;
using System.ComponentModel.DataAnnotations;

namespace CodeReviewer.Models
{
    public static class Extensions
    {
        internal static string FirstLine(this string value, int maxLen = -1)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            var str = value.Trim();
            var idx = str.IndexOf('\n');
            if (idx != -1)
                return str.Substring(0, idx).Trim();
            if (maxLen != -1)
            {
                maxLen = Math.Min(maxLen, str.Length);
                return str.Substring(0, maxLen);
            }
            return str;
        }

        public static ChangeList FindEx(this DbSet<ChangeList> dbSet, int id, bool cached = false)
        {
            if (cached)
                return dbSet.Find(id);

            var query = (from item in dbSet.AsNoTracking()
                         where item.Id == id
                         select item);
            if (query.Any())
                return query.First();

            return null;
        }

        public static CommentGroup FindEx(this DbSet<CommentGroup> dbSet, int id, bool cached = false)
        {
            if (cached)
                return dbSet.Find(id);

            var query = (from item in dbSet.AsNoTracking()
                         where item.Id == id
                         select item);
            if (query.Any())
                return query.First();

            return null;
        }

        public static Comment FindEx(this DbSet<Comment> dbSet, int id, bool cached = false)
        {
            if (cached)
                return dbSet.Find(id);

            var query = (from item in dbSet.AsNoTracking()
                         where item.Id == id
                         select item);
            if (query.Any())
                return query.First();

            return null;
        }

        public static CommentGroupDto FindCommentGroupDto(this Models.CodeReviewerContext db, int groupId, bool cached = false)
        {
            var commentGroup = db.CommentGroups.FindEx(groupId, cached);
            return commentGroup != null ? new CommentGroupDto(commentGroup, true) : null;
        }
    }

    public class ChangeListDto
    {
        public string type = "ChangeList";
        public List<ChangeFileDto> changeFiles = new List<ChangeFileDto>();

        //public List<MailChangeListDto> mailChangeLists = new List<MailChangeListDto>();
        //public List<MailReviewRequestDto> mailReviewRequests = new List<MailReviewRequestDto>();
        public List<ReviewerDto> reviewers = new List<ReviewerDto>();

        public List<ReviewDto> reviews = new List<ReviewDto>();
        public List<CommentGroupDto> comments = new List<CommentGroupDto>();

        public string CL { get; set; }

        //public string description { get; set; }
        public int id { get; set; }

        public int sourceControlId { get; set; }

        public int stage { get; set; }

        public DateTime timeStamp { get; set; }

        public string userClient { get; set; }

        public string userName { get; set; }

        public string reviewerAlias { get; set; }

        public ChangeListDto()
        {
        }

        public ChangeListDto(ChangeList input, bool complete)
        {
            if (input == null)
                return;

            this.CL = input.CL;
            //this.description = input.Description;
            this.id = input.Id;
            this.sourceControlId = input.SourceControlId;
            this.stage = input.Stage;
            this.timeStamp = input.TimeStamp;
            this.userClient = input.UserClient;
            this.userName = input.UserName;
            this.reviewerAlias = input.ReviewerAlias;
            input.ChangeFiles.ToList().ForEach(x => changeFiles.Add(new ChangeFileDto(x, false)));

            // All comments
            input.ChangeFiles.ToList().ForEach(item => item.FileVersions.ToList().ForEach(fileVersion =>
            {
                fileVersion.CommentGroups.ToList().ForEach(group => comments.Add(new CommentGroupDto(@group, true)));
            }));

            if (complete)
            {
                //input.MailChangeLists.ToList().ForEach(x => mailChangeLists.Add(new MailChangeListDto(x, false)));
                //input.MailReviewRequests.ToList().ForEach(x => mailReviewRequests.Add(new MailReviewRequestDto(x, false)));
                input.Reviewers.ToList().ForEach(x =>
                    {
                        if (x.Status != (int)ReviewerStatus.Deleted)
                            reviewers.Add(new ReviewerDto(x, false));
                    });
                input.Reviews.ToList().ForEach(x => reviews.Add(new ReviewDto(x, false)));
            }
        }
    }

    public class ChangeFileDto
    {
        public string type = "ChangeFile";

        public ChangeListDto changeList { get; set; }

        public List<FileVersionDto> fileVersions = new List<FileVersionDto>();
        public List<CommentGroupDto> comments = new List<CommentGroupDto>();

        public int changeListId { get; set; }

        public int id { get; set; }

        public bool isActive { get; set; }

        public string localFileName { get; set; }

        public string serverFileName { get; set; }

        public string name { get; set; }

        public ChangeFileDto()
        {
        }

        public ChangeFileDto(ChangeFile input, bool complete)
        {
            if (input == null)
                return;

            this.changeListId = input.ChangeListId;
            this.id = input.Id;
            this.isActive = input.IsActive;
            this.localFileName = input.LocalFileName;
            this.serverFileName = input.ServerFileName;
            this.name = System.IO.Path.GetFileName(input.ServerFileName);

            input.FileVersions.ToList().ForEach(x => this.fileVersions.Add(new FileVersionDto(x, false)));
            input.CommentGroups.ToList().ForEach(comment => comments.Add(new CommentGroupDto(comment, false)));

            if (complete)
            {
                this.changeList = new ChangeListDto(input.ChangeList, false);
            }
        }
    }

    public class CommentGroupDto
    {
        public string type = "CommentGroup";

        public int id { get; set; }

        public int reviewId { get; set; }

        public int changeListId { get; set; }

        public int fileVersionId { get; set; }

        public int line { get; set; }

        public string lineStamp { get; set; }

        public int status { get; set; }

        public List<CommentDto> threads = new List<CommentDto>();

        public CommentGroupDto()
        {
        }

        public CommentGroupDto(CommentGroup input, bool complete)
        {
            id = input.Id;
            reviewId = input.ReviewId;
            changeListId = input.ChangeListId;
            fileVersionId = input.FileVersionId;
            line = input.Line;
            lineStamp = input.LineStamp;
            status = input.Status;

            input.Comments.ToList().ForEach(comment =>
                threads.Add(new CommentDto(comment, false)));
        }
    }

    public class CommentDto
    {
        public string type = "Comment";

        public int id { get; set; }

        public string commentText { get; set; }

        public int reviewRevision { get; set; }

        public int reviewerId { get; set; }

        public int fileVersionId { get; set; }

        public int groupId { get; set; }

        public string userName { get; set; }

        public string reviewerAlias { get; set; }

        public CommentDto()
        {
        }

        public CommentDto(Comment input, bool complete = true)
        {
            id = input.Id;
            commentText = input.CommentText;
            reviewRevision = input.ReviewRevision;
            reviewerId = input.ReviewerId;
            fileVersionId = input.FileVersionId;
            groupId = input.GroupId.HasValue ? input.GroupId.Value : 0;
            userName = input.UserName;
            reviewerAlias = input.ReviewerAlias;
        }
    }

    public class FileVersionDto
    {
        public ChangeFileDto changeFile { get; set; }

        public string type = "FileVersion";

        public int action { get; set; }

        public int fileId { get; set; }

        public int id { get; set; }

        public bool isFullText { get; set; }

        public bool isRevisionBase { get; set; }

        public bool isText { get; set; }

        public int revision { get; set; }

        public int reviewRevision { get; set; }

        public DateTime? timeStamp { get; set; }

        public string name { get; set; }

        public string diffHtml { get; set; }

        public FileVersionDto()
        {
        }

        public FileVersionDto(FileVersion input, bool complete)
        {
            if (input == null)
                return;

            this.action = input.Action;
            this.fileId = input.FileId;
            this.id = input.Id;
            this.isFullText = input.IsFullText;
            this.isRevisionBase = input.IsRevisionBase;
            this.isText = input.IsText;
            this.revision = input.Revision;
            this.reviewRevision = input.ReviewRevision;
            this.timeStamp = input.TimeStamp;
            this.name = string.Format("{0}_{1}", System.IO.Path.GetFileName(input.ChangeFile.ServerFileName), input.Id);

            if (complete)
            {
                changeFile = new ChangeFileDto(input.ChangeFile, false);
            }
        }
    }

    public class MailChangeListDto
    {
        public string type = "MailChangeList";

        public ChangeListDto changeList { get; set; }

        public ReviewerDto reviewer { get; set; }

        public int ChangeListId { get; set; }

        public int Id { get; set; }

        public int ReviewerId { get; set; }

        public MailChangeListDto()
        {
        }

        public MailChangeListDto(MailChangeList input, bool complete)
        {
            if (input == null)
                return;

            this.ChangeListId = input.ChangeListId;
            this.Id = input.Id;
            this.ReviewerId = input.ReviewerId;

            if (complete)
            {
                this.changeList = new ChangeListDto(input.ChangeList, false);
                this.reviewer = new ReviewerDto(input.Reviewer, false);
            }
        }
    }

    public class MailReviewDto
    {
        public string type = "MailReview";

        public ReviewDto review { get; set; }

        public int Id { get; set; }

        public int ReviewId { get; set; }

        public MailReviewDto()
        {
        }

        public MailReviewDto(MailReview input, bool complete)
        {
            if (input == null)
                return;

            this.Id = input.Id;
            this.ReviewId = input.ReviewId;

            if (complete)
            {
                this.review = new ReviewDto(input.Review, false);
            }
        }
    }

    public class MailReviewRequestDto
    {
        public string type = "MailReviewRequest";

        public ChangeListDto changeList { get; set; }

        public int Id { get; set; }

        public int ChangeListId { get; set; }

        public string ReviewerAlias { get; set; }

        public MailReviewRequestDto()
        {
        }

        public MailReviewRequestDto(MailReviewRequest input, bool complete)
        {
            if (input == null)
                return;

            this.Id = input.Id;
            this.ChangeListId = input.ChangeListId;
            this.ReviewerAlias = input.ReviewerAlias;

            if (complete)
            {
                this.changeList = new ChangeListDto(input.ChangeList, false);
            }
        }
    }

    public class ReviewDto
    {
        public string type = "Review";

        public ChangeListDto changeList { get; set; }

        public List<MailReviewDto> mailReviews = new List<MailReviewDto>();

        public int changeListId { get; set; }

        public string commentText { get; set; }

        public int id { get; set; }

        public bool isSubmitted { get; set; }

        public int overallStatus { get; set; }

        public DateTime timeStamp { get; set; }

        public string userName { get; set; }

        public string reviewerAlias { get; set; }

        public ReviewDto()
        {
        }

        public ReviewDto(Review input, bool complete)
        {
            if (input == null)
                return;

            this.changeListId = input.ChangeListId;
            this.commentText = input.CommentText;
            this.id = input.Id;
            this.isSubmitted = input.IsSubmitted;
            this.overallStatus = input.OverallStatus;
            this.timeStamp = input.TimeStamp;
            this.userName = input.UserName;
            this.reviewerAlias = input.ReviewerAlias;

            if (complete)
            {
                this.changeList = new ChangeListDto(input.ChangeList, true);
                input.MailReviews.ToList().ForEach(x => mailReviews.Add(new MailReviewDto(x, false)));
            }
        }
    }

    public class ReviewerDto
    {
        public string type = "Reviewer";

        //public List<MailChangeListDto> MailChangeLists = new List<MailChangeListDto>();
        public ChangeListDto changeList { get; set; }

        public int changeListId { get; set; }

        public int id { get; set; }

        public string reviewerAlias { get; set; }

        public int status { get; set; }

        public ReviewerDto()
        {
        }

        public ReviewerDto(Reviewer input, bool complete)
        {
            if (input == null)
                return;

            this.changeList = null;
            this.changeListId = input.ChangeListId;
            this.id = input.Id;
            this.reviewerAlias = input.ReviewerAlias;
            this.status = input.Status;

            if (complete)
            {
                //input.MailChangeLists.ToList().ForEach(x => MailChangeLists.Add(new MailChangeListDto(x, false)));
            }
        }
    }

    public class ChangeListStatusDto
    {
        public string type = "ChangeListStatus";
        public int id;
        public int status;

        public ChangeListStatusDto()
        {
        }

        public ChangeListStatusDto(ChangeList input)
        {
            id = input.Id;
            status = input.Stage;
        }
    }

    public class UserContextDto
    {
        public int id;
        public string userName;
        public string reviewerAlias;
        public string key;
        public string value;
        public int version;

        public UserContextDto()
        {
        }

        public UserContextDto(UserContext input)
        {
            this.id = input.Id;
            this.userName = input.UserName;
            this.reviewerAlias = input.ReviewerAlias;
            this.key = input.KeyName;
            this.value = input.Value;
            this.version = input.Version;
        }
    }

    public class BaseSettingsDto
    {
        public int version { get; protected set; }
    }

    public class UserSettingsDto : BaseSettingsDto
    {
        public class Dynamic
        {
            public bool comments = true;
            public bool reviewers = true;
            public bool changeLists = true;
        }

        public class Diff
        {
            public bool ignoreWhiteSpace = true;
            public bool preDiff = false;
            public bool intraLineDiff = true;

            public bool DefaultSettings
            {
                get
                {
                    return ignoreWhiteSpace == true &&
                           intraLineDiff == true;
                }
            }
        }

        public class LeftPane
        {
            public bool closed = false;
            public double width = -1.0;
        }

        public string key = "settings";
        public int broadCastDelay = 0;
        public Diff diff = new Diff();
        public Dynamic dynamic = new Dynamic();
        public LeftPane leftPane = new LeftPane();
        public bool dualPane = true;
        public bool hideUnchanged = false;

        public static int CurrentVersion = 2;

        public UserSettingsDto()
        {
            version = CurrentVersion;
        }

        public static UserSettingsDto FromJS(string json)
        {
            var js = new JavaScriptSerializer();
            return js.Deserialize<UserSettingsDto>(json);
        }
    }

    public class ChangeListSettingsDto : BaseSettingsDto
    {
        public class FileSettings
        {
            public int fileId;
            public int tabIndex;
        }

        public string key = "CL";
        public bool defaultDiff = true;
        public string fontName = "";
        public int fontSize = 0;
        public int fileId;
        public int fileRevision;
        public List<FileSettings> files = new List<FileSettings>();

        public static int CurrentVersion = 1;

        public ChangeListSettingsDto()
        {
            version = CurrentVersion;
        }

        public static ChangeListSettingsDto FromJS(string json)
        {
            var js = new JavaScriptSerializer();
            return js.Deserialize<ChangeListSettingsDto>(json);
        }
    }

    public class DiffFileDto : BaseSettingsDto
    {
        public FileVersionDto left { get; set; }

        public FileVersionDto right { get; set; }

        public string CL { get; set; }

        public int baseReviewId { get; set; }

        public DiffFileDto(FileVersion left, FileVersion right, string CL, int baseReviewId)
        {
            this.left = new FileVersionDto(left, false);
            this.right = new FileVersionDto(right, false);
            this.CL = CL;
            this.baseReviewId = baseReviewId;
        }
    }

    public class ChangeListDisplayItem
    {
        public string CL { get; set; }
        public string description { get; set; }
        public string stage { get; set; }
        public string server { get; set; }
        public string shortDescription { get; set; }
        public int id { get; set; }

        public ChangeListDisplayItem(ChangeList item)
        {
            CL = item.CL;
            description = item.Description;
            id = item.Id;
            shortDescription = item.Description.FirstLine();
            stage = Enum.ToObject(typeof(ChangeListStatus), item.Stage).ToString();
            server = item.SourceControl.Server;
        }

        public ChangeListDisplayItem()
        {
        }
    }
}