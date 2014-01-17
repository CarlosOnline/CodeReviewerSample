using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CodeReviewer.Models
{
    public class SubmitReview
    {
        [Required]
        [StringLength(5)]
        public string CL { get; set; }

        [DataType(DataType.Text)]
        public string Title { get; set; }

        [DataType(DataType.MultilineText)]
        [AllowHtml]
        public string Description { get; set; }

        [DataType(DataType.EmailAddress)]
        public string Reviewers { get; set; }

        [DataType(DataType.EmailAddress)]
        public string OptionalReviewers { get; set; }

        public int ChangeListId { get; set; }

        public ChangeList ChangeList { get; set; }
    }

}