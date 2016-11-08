using System;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace TMS.ViewModels
{
    public class ChangPaswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }
}
