using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TMS.ViewModels
{
    public class RequesterViewModel
    {
        public string Id { get; set; }
        public string Fullname { get; set; }
        public string Email { get; set; }
        public string DepartmentName { get; set; }
        public string PhoneNumber { get; set; }
        public string JobTitle { get; set; }
    }
}