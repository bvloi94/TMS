using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TMS.ViewModels
{
    public class KnowledgeBaseViewModels
    {
        [Required(ErrorMessage = "Subject is not empty!")]
        public int? ID { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public int CategoryID { get; set; }
        public IEnumerable<HttpPostedFileBase> SolutionAttachments { get; set; }
        public string Keyword { get; set; }
        public Nullable<DateTime> CreatedTime { get; set; }
        public Nullable<DateTime> ModifiedTime { get; set; }
    }
}