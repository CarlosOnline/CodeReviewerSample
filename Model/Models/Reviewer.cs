using System;
using System.Collections.Generic;

namespace CodeReviewer.Models
{
    public partial class Reviewer
    {
        public Reviewer()
        {
            this.Comments = new List<Comment>();
            this.MailChangeLists = new List<MailChangeList>();
        }

        public int Id { get; set; }
        public string UserName { get; set; }
        public string ReviewerAlias { get; set; }
        public int ChangeListId { get; set; }
        public int Status { get; set; }
        public virtual ChangeList ChangeList { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<MailChangeList> MailChangeLists { get; set; }
    }
}
