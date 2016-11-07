using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TMS.ViewModels
{
    public class BasicTicketViewModel
    {
        public int? ID { get; set; }
        public int? Status { get; set; }
        [Required(ErrorMessage = "Subject cannot be empty!")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Subject length is between 8 and 100 characters")]
        public string Subject { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string DescriptionAttachment { get; set; }
        public string CreatedBy { get; set; }
        public string Mode { get; set; }
        public string Category { get; set; }
        public string Subcategory { get; set; }
        public string ModifiedTime { get; set; }
        public string CreatedTime { get; set; }
        public string SolvedTime { get; set; }
        public string Solution { get; set; }
        public string SolutionAttachment { get; set; }
        public string SolvedBy { get; set; }
        public string UnapproveReason { get; set; }
    }
}