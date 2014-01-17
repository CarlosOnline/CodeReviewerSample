using System;
using System.Collections.Generic;

namespace CodeReviewer.Models
{
    public partial class Attachment
    {
        public int Id { get; set; }
        public int ChangeListId { get; set; }
        public System.DateTime TimeStamp { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        public virtual ChangeList ChangeList { get; set; }
    }
}
