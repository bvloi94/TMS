using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TMS.Models;
using TMS.Utils;

namespace TMS.ViewModels
{
    public class KnowledgeBaseViewModel
    {
        public int? ID { get; set; }
        [Required(ErrorMessage = "Please input a subject!")]
        [StringLength(200)]
        public string Subject { get; set; }
        [AllowHtml]
        public string Content { get; set; }
        [Required(ErrorMessage = "Please select totpic!")]
        public int CategoryID { get; set; }
        public string CategoryPath { get; set; }
        public string Category { get; set; }
        public string Keyword { get; set; }
        public Nullable<DateTime> CreatedTime { get; set; }
        public Nullable<DateTime> ModifiedTime { get; set; }
        [Required(ErrorMessage = "Please input a path!")]
        public string Path { get; set; }
        // [MaxFileSize(5 * 1024 * 1024, ErrorMessage = "Maximum allowed file size is {0} bytes")]
        public IEnumerable<HttpPostedFileBase> SolutionAttachments { get; set; }
        public List<AttachmentViewModel> SolutionAttachmentsList { get; set; }
        public List<AttachmentViewModel> DescriptionAttachments { get; set; }
    }
}