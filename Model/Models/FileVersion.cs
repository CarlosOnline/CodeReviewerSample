using System;
using System.Collections.Generic;

namespace CodeReviewer.Models
{
    public partial class FileVersion
    {
        public FileVersion()
        {
            this.Comments = new List<Comment>();
            this.CommentGroups = new List<CommentGroup>();
        }

        public int Id { get; set; }
        public int FileId { get; set; }
        public int Revision { get; set; }
        public int ReviewRevision { get; set; }
        public int Action { get; set; }
        public Nullable<System.DateTime> TimeStamp { get; set; }
        public bool IsText { get; set; }
        public bool IsFullText { get; set; }
        public bool IsRevisionBase { get; set; }
        public string Text { get; set; }
        public virtual ChangeFile ChangeFile { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<CommentGroup> CommentGroups { get; set; }
    }
}
