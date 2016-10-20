using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TMS.Enumerator;
using TMS.Models;

namespace TMS.ViewModels
{
    public class AttachmentViewModel
    {
        public string id { get; set; }
        public string name { get; set; }
    }
}