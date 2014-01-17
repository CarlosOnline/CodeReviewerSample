using System;
using System.Collections.Generic;

namespace CodeReviewer.Models
{
    public partial class ChangeFile
    {
        public ChangeFile()
        {
            this.CommentGroups = new List<CommentGroup>();
            this.FileVersions = new List<FileVersion>();
        }

        public int Id { get; set; }
        public int ChangeListId { get; set; }
        public string LocalFileName { get; set; }
        public string ServerFileName { get; set; }
        public bool IsActive { get; set; }
        public int ReviewRevision { get; set; }
        public virtual ChangeList ChangeList { get; set; }
        public virtual ICollection<CommentGroup> CommentGroups { get; set; }
        public virtual ICollection<FileVersion> FileVersions { get; set; }
    }
}
