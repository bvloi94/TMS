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
        [StringLength(200, MinimumLength = 10, ErrorMessage = "Subject length is between 10 and 200 characters")]
        public string Subject { get; set; }
        [AllowHtml]
        public string Content { get; set; }
        [CustomRequired(ErrorMessage = "Please select totpic!")]
        public int CategoryID { get; set; }
        public string CategoryPath { get; set; }
        public string Category { get; set; }
        [TagFormat(ErrorMessage = "Keyword only contain characters 'a-z', 'A-Z', '0-9' and separated by commas!")]
        public string Keyword { get; set; }
        public Nullable<DateTime> CreatedTime { get; set; }
        public Nullable<DateTime> ModifiedTime { get; set; }
        [Required(ErrorMessage = "Please input a path!")]
        [PathFormat(ErrorMessage = "Path can not contain special characters and spaces!")]
        public string Path { get; set; }
        // [MaxFileSize(5 * 1024 * 1024, ErrorMessage = "Maximum allowed file size is {0} bytes")]
        public IEnumerable<HttpPostedFileBase> SolutionAttachments { get; set; }
        public List<AttachmentViewModel> SolutionAttachmentsList { get; set; }
        public List<AttachmentViewModel> DescriptionAttachments { get; set; }
    }
}