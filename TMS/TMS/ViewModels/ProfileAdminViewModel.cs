using System;
using System.ComponentModel.DataAnnotations;
using System.Web;
using TMS.Utils;

namespace TMS.ViewModels
{
    public class ProfileAdminViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        public string FullName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        [EmailAddress(ErrorMessage = "Email is not valid.")]
        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; }
        public Nullable<bool> Gender { get; set; }
        public Nullable<DateTime> DayOfBirth { get; set; }
        [ImageFile("jpg,jpeg,png,gif", ErrorMessage = "Please upload images with [jpg|jpeg|png|gif] extension.")]
        public HttpPostedFileBase Avatar { get; set; }

    }
}