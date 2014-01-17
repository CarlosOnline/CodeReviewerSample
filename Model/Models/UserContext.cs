using System;
using System.Collections.Generic;

namespace CodeReviewer.Models
{
    public partial class UserContext
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string ReviewerAlias { get; set; }
        public string KeyName { get; set; }
        public string Value { get; set; }
        public int Version { get; set; }
    }
}
