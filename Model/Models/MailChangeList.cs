using System;
using System.Collections.Generic;

namespace CodeReviewer.Models
{
    public partial class MailChangeList
    {
        public int Id { get; set; }
        public int ReviewerId { get; set; }
        public int ChangeListId { get; set; }
        public int RequestType { get; set; }
        public virtual ChangeList ChangeList { get; set; }
        public virtual Reviewer Reviewer { get; set; }
    }
}
