using System;
using System.Collections.Generic;

namespace CodeReviewer.Models
{
    public partial class MailReview
    {
        public int Id { get; set; }
        public int ReviewId { get; set; }
        public virtual Review Review { get; set; }
    }
}
