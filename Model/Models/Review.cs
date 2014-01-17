using System;
using System.Collections.Generic;

namespace CodeReviewer.Models
{
    public partial class Review
    {
        public Review()
        {
            this.CommentGroups = new List<CommentGroup>();
            this.MailReviews = new List<MailReview>();
        }

        public int Id { get; set; }
        public int ChangeListId { get; set; }
        public string UserName { get; set; }
        public string ReviewerAlias { get; set; }
        public System.DateTime TimeStamp { get; set; }
        public bool IsSubmitted { get; set; }
        public byte OverallStatus { get; set; }
        public string CommentText { get; set; }
        public virtual ChangeList ChangeList { get; set; }
        public virtual ICollection<CommentGroup> CommentGroups { get; set; }
        public virtual ICollection<MailReview> MailReviews { get; set; }
    }
}
