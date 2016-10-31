using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TMS.Models;

namespace TMS.ViewModels
{
    public class KnowledgeBaseViewModels
    {
        public int? ID { get; set; }
        [Required(ErrorMessage = "Please input a subject!")]
        [StringLength(100)]
        public string Subject { get; set; }
        [AllowHtml]
        public string Content { get; set; }

        //public int CategoryID { get; set; }
        [Required(ErrorMessage = "Please select totpic!")]
        public int CategoryID { get; set; }
        public string Category { get; set; }
        public IEnumerable<HttpPostedFileBase> SolutionAttachments { get; set; }
        public string Keyword { get; set; }
        public Nullable<DateTime> CreatedTime { get; set; }
        public Nullable<DateTime> ModifiedTime { get; set; }
        [Required(ErrorMessage = "Please input a path!")]
        public string Path { get; set; }
        public string Keyword { get; set; }
    }
}