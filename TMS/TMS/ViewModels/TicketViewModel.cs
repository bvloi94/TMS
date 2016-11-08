using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EAGetMail;
using TMS.Enumerator;
using TMS.Models;
using TMS.Utils;

namespace TMS.ViewModels
{
    public class TicketViewModel
    {
        public int No { get; set; }
        public string Code { get; set; }
        public int Id { get; set; }
        [Required(ErrorMessage = "Ticket's subject is required!")]
        public string Subject { get; set; }
        public string Description { get; set; }
        public string Solution { get; set; }
        [Required(ErrorMessage = "Ticket's requester is required!")]
        public string RequesterId { get; set; }
        public string Requester { get; set; }
        public string TechnicianId { get; set; }
        public string Technician { get; set; }
        public int Type { get; set; }
        public int Mode { get; set; }
        public int StatusId { get; set; }
        public string Status { get; set; }
        public int UrgencyId { get; set; }
        public string Urgency { get; set; }
        public int PriorityId { get; set; }
        public string Priority { get; set; }
        public int ImpactId { get; set; }
        public string Impact { get; set; }
        public string ImpactDetail { get; set; }
        public int CategoryId { get; set; }
        public string Category { get; set; }
        public int DepartmentId { get; set; }
        public string Department { get; set; }
        public string UnapproveReason { get; set; }
        public DateTime? ScheduleStartDate { get; set; }
        [CompareValidateDate(CompareToOperation.GreaterThan, "ScheduleStartDate", ErrorMessage = "Schedule End Date must greater than Schedule Start Date")]
        public DateTime? ScheduleEndDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        [CompareValidateDate(CompareToOperation.GreaterThan, "ActualStartDate", ErrorMessage = "Actual End Date must greater than Actual Start Date")]
        public DateTime? ActualEndDate { get; set; }
        public DateTime? SolvedDate { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? ModifiedTime { get; set; }
        public string CreatedTimeString { get; set; }
        public string ModifiedTimeString { get; set; }
        public string SolvedDateString { get; set; }
        public string ScheduleStartDateString { get; set; }
        public string ScheduleEndDateString { get; set; }
        public string ActualStartDateString { get; set; }
        public string ActualEndDateString { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedId { get; set; }
        public string AssignedBy { get; set; }
        public string SolvedBy { get; set; }
        public string SolvedId { get; set; }
        public string DescriptionAttachmentsURL { get; set; }
        public string SolutionAttachmentsURL { get; set; }
        [TagFormat(ErrorMessage = "Tags only contain characters 'a-z', '0-9' and separated by commas!")]
        public string Tags { get; set; }
        public string Note { get; set; }
        [MaxFileSize(5 * 1024 * 1024, ErrorMessage = "Maximum allowed file size is {0} bytes")]
        public IEnumerable<HttpPostedFileBase> DescriptionFiles { get; set; }
        [MaxFileSize(5 * 1024 * 1024, ErrorMessage = "Maximum allowed file size is {0} bytes")]
        public IEnumerable<HttpPostedFileBase> SolutionFiles { get; set; }
        public List<AttachmentViewModel> DescriptionAttachments { get; set; }
        public List<AttachmentViewModel> SolutionAttachments { get; set; }

    }
}