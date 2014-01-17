using System;
using System.Collections.Generic;

namespace CodeReviewer.Models
{
    public partial class Comment
    {
        public int Id { get; set; }
        public string CommentText { get; set; }
        public int FileVersionId { get; set; }
        public int ReviewerId { get; set; }
        public int ReviewRevision { get; set; }
        public Nullable<int> GroupId { get; set; }
        public string UserName { get; set; }
        public string ReviewerAlias { get; set; }
        public virtual CommentGroup CommentGroup { get; set; }
        public virtual FileVersion FileVersion { get; set; }
        public virtual Reviewer Reviewer { get; set; }
    }
}
