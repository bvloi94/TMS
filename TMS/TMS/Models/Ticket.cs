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
        public int id { get; set; }
        public string subject { get; set; }
        public string description { get; set; }
        public Nullable<int> type { get; set; }
        public Nullable<int> status { get; set; }
        public Nullable<int> assignee { get; set; }
    }
}