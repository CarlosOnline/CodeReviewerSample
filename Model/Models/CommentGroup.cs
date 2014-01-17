using System;
using System.Collections.Generic;

namespace CodeReviewer.Models
{
    public partial class CommentGroup
    {
        public CommentGroup()
        {
            this.Comments = new List<Comment>();
        }

        public int Id { get; set; }
        public int ReviewId { get; set; }
        public int ChangeListId { get; set; }
        public int FileId { get; set; }
        public int FileVersionId { get; set; }
        public int Line { get; set; }
        public string LineStamp { get; set; }
        public int Status { get; set; }
        public virtual ChangeFile ChangeFile { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual FileVersion FileVersion { get; set; }
        public virtual Review Review { get; set; }
    }
}
