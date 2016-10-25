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
        public string Subject { get; set; }
        public string Content { get; set; }
        public int CategoryID { get; set; }
        public IEnumerable<HttpPostedFileBase> SolutionAttachments { get; set; }
    }
}