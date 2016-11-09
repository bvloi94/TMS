using System;
using System.ComponentModel.DataAnnotations;
using System.Web;
using TMS.Utils;

namespace TMS.ViewModels
{
    public class ProfileRequesterViewModel 
    {
        [EmailAddress]
        [Required(ErrorMessage = "Please input email!")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Please input full name!")]
        public string Fullname { get; set; }
        [ImageFile("jpg,jpeg,png,gif", ErrorMessage = "Please upload images with [jpg|jpeg|png|gif] extension.")]
        public HttpPostedFileBase Avatar { get; set; }
        public string AvatarURL { get; set; }
        [StringLength(15, MinimumLength = 8, ErrorMessage = "Phone length is between 8 and 15 characters")]
        public string PhoneNumber { get; set; }
        [DataType(DataType.Date)]
        [ValidateDate(-16, ErrorMessage = "User must older than 16 years old")]
        [ValidateDateSQL(ErrorMessage = "Invalid date")]
        public Nullable<DateTime> Birthday { get; set; }
        public string Address { get; set; }
        public Nullable<bool> Gender { get; set; }
        public Nullable<int> DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public string JobTitle { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
    }
}
