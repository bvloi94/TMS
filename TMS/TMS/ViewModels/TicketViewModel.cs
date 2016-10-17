using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TMS.Enumerator;

namespace TMS.ViewModels
{
    public class TicketViewModel
    {
        public int No { get; set; }
        public int Id { get; set; }
        public string Subject { get; set; }
        [AllowHtml]
        public string Description { get; set; }
        [AllowHtml]
        public string Solution { get; set; }
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
        public string ScheduleStartDate { get; set; }
        public string ScheduleEndDate { get; set; }
        public string ActualStartDate { get; set; }
        public string ActualEndDate { get; set; }
        public string SolvedDate { get; set; }
        public string CreatedTime { get; set; }
        public string ModifiedTime { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedId { get; set; }
    }
}