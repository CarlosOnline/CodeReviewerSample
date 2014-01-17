using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CodeReviewer.Models
{
    public partial class UserProfile
    {
        public int Id { get; set; }
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }
}
