using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using TMS.Utils;

namespace TMS.ViewModels
{
    public class TicketSolveViewModel
    {
        public int? ID { get; set; }
        public string Code { get; set; }
        public string Type { get; set; }
        public string Mode { get; set; }
        public string Status { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string DescriptionAttachmentsURL { get; set; }
        public string CreatedBy { get; set; }
        public string AssignedBy { get; set; }
        public string Impact { get; set; }
        public string ImpactDetail { get; set; }
        public string Urgency { get; set; }
        public string Priority { get; set; }
        public string Category { get; set; }
        public string MergedTicketString { get; set; }
        public string CreatedTimeString { get; set; }
        public string ModifiedTimeString { get; set; }
        public string SolvedDateString { get; set; }
        public string ScheduleStartDateString { get; set; }
        public string ScheduleEndDateString { get; set; }
        public string ActualStartDateString { get; set; }
        public string ActualEndDateString { get; set; }
        public string OverdueDateString { get; set; }
        [Required(ErrorMessage = "Solution cannot be empty!")]
        public string Solution { get; set; }
        public string SolutionAttachmentsURL { get; set; }
        public string SolvedBy { get; set; }
        public string Requester { get; set; }
        public string UnapproveReason { get; set; }
        public string Tags { get; set; }
        public string Note { get; set; }
        public string Command { get; set; }
        [MaxFileSize(5 * 1024 * 1024, ErrorMessage = "Maximum allowed file size is {0} bytes")]
        public IEnumerable<HttpPostedFileBase> DescriptionFiles { get; set; }
        [MaxFileSize(5 * 1024 * 1024, ErrorMessage = "Maximum allowed file size is {0} bytes")]
        public IEnumerable<HttpPostedFileBase> SolutionFiles { get; set; }
        public List<AttachmentViewModel> DescriptionAttachments { get; set; }
        public List<AttachmentViewModel> SolutionAttachments { get; set; }
    }
}