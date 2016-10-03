using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TMS.Models
{
    public class RequesterRegisterViewModel
    {
        [EmailAddress]
        [Required(ErrorMessage = "Please input username!")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Please input password!")]
        public string Password { get; set; }
        [EmailAddress]
        [Required(ErrorMessage = "Please input email!")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Please input full name!")]
        public string Fullname { get; set; }
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
