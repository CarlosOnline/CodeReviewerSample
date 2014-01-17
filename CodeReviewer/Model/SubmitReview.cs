using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CodeReviewer.Models
{
    public class SubmitReview
    {
        [Required]
        [StringLength(8)]
        public string CL { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Title - optional")]
        public string Title { get; set; }

        [DataType(DataType.MultilineText)]
        [AllowHtml]
        public string Description { get; set; }

        [DataType(DataType.EmailAddress)]
        [Display(Name = "Required reviewers")]
        public string Reviewers { get; set; }

        [DataType(DataType.EmailAddress)]
        [Display(Name = "Optional reviewers")]
        public string OptionalReviewers { get; set; }

        public int ChangeListId { get; set; }

        public ChangeList ChangeList { get; set; }
    }

}