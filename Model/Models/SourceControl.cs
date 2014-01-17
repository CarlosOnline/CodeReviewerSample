using System;
using System.Collections.Generic;

namespace CodeReviewer.Models
{
    public partial class SourceControl
    {
        public SourceControl()
        {
            this.ChangeLists = new List<ChangeList>();
        }

        public int Id { get; set; }
        public int Type { get; set; }
        public string Server { get; set; }
        public string Client { get; set; }
        public string Description { get; set; }
        public string WebsiteName { get; set; }
        public virtual ICollection<ChangeList> ChangeLists { get; set; }
    }
}
