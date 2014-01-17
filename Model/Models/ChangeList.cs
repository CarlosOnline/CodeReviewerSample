using System;
using System.Collections.Generic;

namespace CodeReviewer.Models
{
    public partial class ChangeList
    {
        public ChangeList()
        {
            this.Attachments = new List<Attachment>();
            this.ChangeFiles = new List<ChangeFile>();
            this.MailChangeLists = new List<MailChangeList>();
            this.MailReviewRequests = new List<MailReviewRequest>();
            this.Reviews = new List<Review>();
            this.Reviewers = new List<Reviewer>();
        }

        public int Id { get; set; }
        public int SourceControlId { get; set; }
        public string UserName { get; set; }
        public string ReviewerAlias { get; set; }
        public string UserClient { get; set; }
        public string CL { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public System.DateTime TimeStamp { get; set; }
        public int Stage { get; set; }
        public int CurrentReviewRevision { get; set; }
        public virtual ICollection<Attachment> Attachments { get; set; }
        public virtual ICollection<ChangeFile> ChangeFiles { get; set; }
        public virtual SourceControl SourceControl { get; set; }
        public virtual ICollection<MailChangeList> MailChangeLists { get; set; }
        public virtual ICollection<MailReviewRequest> MailReviewRequests { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<Reviewer> Reviewers { get; set; }
    }
}
