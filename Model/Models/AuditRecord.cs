using System;
using System.Collections.Generic;

namespace CodeReviewer.Models
{
    public partial class AuditRecord
    {
        public int Id { get; set; }
        public System.DateTime TimeStamp { get; set; }
        public string UserName { get; set; }
        public int ChangeListId { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
    }
}
