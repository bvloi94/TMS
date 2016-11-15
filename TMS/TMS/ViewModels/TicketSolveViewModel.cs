using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

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
        public string AttachmentURL { get; set; }
        public string CreatedBy { get; set; }
        public string AssignedBy { get; set; }
        public string Impact { get; set; }
        public string ImpactDetail { get; set; }
        public string Urgency { get; set; }
        public string Priority { get; set; }
        public string Category { get; set; }
        public string Subcategory { get; set; }
        public string CreateTime { get; set; }
        public string ModifiedTime { get; set; }
        public string ScheduleStartDate { get; set; }
        public string ScheduleEndDate { get; set; }
        public string ActualStartDate { get; set; }
        public string ActualEndDate { get; set; }
        [Required(ErrorMessage = "Solution cannot be empty!")]
        public string Solution { get; set; }
        public string SolvedBy { get; set; }
        public string Requester { get; set; }
        public string UnapproveReason { get; set; }
        public string Tags { get; set; }
        public string Note { get; set; }
    }
}