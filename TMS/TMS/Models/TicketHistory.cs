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
    
    public partial class TicketHistory
    {
        public int ID { get; set; }
        public int Type { get; set; }
        public string ActID { get; set; }
        public string Action { get; set; }
        public Nullable<System.DateTime> ActedTime { get; set; }
        public int TicketID { get; set; }
    
        public virtual AspNetUser AspNetUser { get; set; }
        public virtual Ticket Ticket { get; set; }
    }
}
