using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using TMS.Utils;

namespace TMS.ViewModels
{
    public class RequesterRegisterViewModel
    {
        [EmailAddress]
        [Required(ErrorMessage = "Please input username!")]
        public string Username { get; set; }
        [EmailAddress]
        [Required(ErrorMessage = "Please input email!")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Please input full name!")]
        public string Fullname { get; set; }
        [ImageFile("jpg, jpeg, png, gif", ErrorMessage = "Please upload images with [jpg|jpeg|png|gif] extension.")]
        public HttpPostedFileBase Avatar { get; set; }
        public string AvatarURL { get; set; }
        public string PhoneNumber { get; set; }
        public string Birthday { get; set; }
        public string Address { get; set; }
        public Nullable<bool> Gender { get; set; }
        public Nullable<int> DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public string JobTitle { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
    }

}
