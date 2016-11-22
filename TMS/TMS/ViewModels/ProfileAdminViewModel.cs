using System;
using System.ComponentModel.DataAnnotations;
using System.Web;
using TMS.Utils;

namespace TMS.ViewModels
{
    public class ProfileAdminViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name length is less than 100 characters")]
        public string FullName { get; set; }
        public string Address { get; set; }
        [StringLength(15, MinimumLength = 8, ErrorMessage = "Phone length is between 8 and 15 characters")]
        public string Phone { get; set; }
        [EmailAddress(ErrorMessage = "Email is not valid.")]
        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; }
        public Nullable<bool> Gender { get; set; }
        [DataType(DataType.Date)]
        [ValidateDate(-16, ErrorMessage = "User must older than 16 years old")]
        [ValidateDateSQL(ErrorMessage = "Invalid date")]
        public Nullable<DateTime> DayOfBirth { get; set; }
        [ImageFile("jpg,jpeg,png,gif", ErrorMessage = "Please upload images with [jpg|jpeg|png|gif] extension.")]
        public HttpPostedFileBase Avatar { get; set; }

    }
}