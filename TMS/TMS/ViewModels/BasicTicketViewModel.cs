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
        [StringLength(200, MinimumLength = 10, ErrorMessage = "Subject length is between 10 and 200 characters")]
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
        public string ScheduleStartTime { get; set; }
        public string ScheduleEndTime { get; set; }
        public string ActualStartTime { get; set; }
        public string ActualEndTime { get; set; }
        public string Solution { get; set; }
        public string SolutionAttachment { get; set; }
        public string SolvedBy { get; set; }
        public string UnapproveReason { get; set; }
        public string MergedTicketString { get; set; }
    }
}