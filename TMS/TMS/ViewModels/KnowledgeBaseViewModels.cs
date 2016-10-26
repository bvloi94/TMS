using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using TMS.Models;

namespace TMS.ViewModels
{
    public class KnowledgeBaseViewModels
    {
        [Required(ErrorMessage = "Subject is not empty!")]
        public int? ID { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        [Required(ErrorMessage = "Please select totpic!")]
        public int CategoryID { get; set; }
        public IEnumerable<HttpPostedFileBase> SolutionAttachments { get; set; }
        public Nullable<DateTime> CreatedTime { get; set; }
        public Nullable<DateTime> ModifiedTime { get; set; }
    }
}