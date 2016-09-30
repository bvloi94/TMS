//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TMS.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Ticket
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Ticket()
        {
            this.TicketAttachments = new HashSet<TicketAttachment>();
            this.TicketHistories = new HashSet<TicketHistory>();
        }
    
        public int ID { get; set; }
        public Nullable<int> Type { get; set; }
        public int Mode { get; set; }
        public string SolveID { get; set; }
        public string TechnicianID { get; set; }
        public Nullable<int> DepartmentID { get; set; }
        public string RequesterID { get; set; }
        public Nullable<int> ImpactID { get; set; }
        public string ImpactDetail { get; set; }
        public Nullable<int> UrgencyID { get; set; }
        public Nullable<int> PriorityID { get; set; }
        public Nullable<int> CategoryID { get; set; }
        public Nullable<int> Status { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string Solution { get; set; }
        public string UnapproveReason { get; set; }
        public Nullable<System.DateTime> ScheduleStartDate { get; set; }
        public Nullable<System.DateTime> ScheduleEndDate { get; set; }
        public Nullable<System.DateTime> ActualStartDate { get; set; }
        public Nullable<System.DateTime> ActualEndDate { get; set; }
        public Nullable<System.DateTime> SolvedDate { get; set; }
        public Nullable<System.DateTime> CreatedTime { get; set; }
        public Nullable<System.DateTime> ModifiedTime { get; set; }
        public string CreatedID { get; set; }
    
        public virtual AspNetUser AspNetUser { get; set; }
        public virtual AspNetUser AspNetUser1 { get; set; }
        public virtual AspNetUser AspNetUser2 { get; set; }
        public virtual AspNetUser AspNetUser3 { get; set; }
        public virtual Category Category { get; set; }
        public virtual Department Department { get; set; }
        public virtual Impact Impact { get; set; }
        public virtual Priority Priority { get; set; }
        public virtual Urgency Urgency { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TicketAttachment> TicketAttachments { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TicketHistory> TicketHistories { get; set; }
    }
}
