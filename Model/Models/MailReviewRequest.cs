using System;
using System.Collections.Generic;

namespace CodeReviewer.Models
{
    public partial class MailReviewRequest
    {
        public int Id { get; set; }
        public string ReviewerAlias { get; set; }
        public int ChangeListId { get; set; }
        public virtual ChangeList ChangeList { get; set; }
    }
}
