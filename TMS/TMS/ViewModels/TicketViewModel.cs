using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TMS.Enumerator;

namespace TMS.ViewModels
{
    public class TicketViewModel
    {
        public int No { get; set; }
        public int Id { get; set; }
        public string Subject { get; set; }
        public string Requester { get; set; }
        public string AssignedTo { get; set; }
        public string Department { get; set; }
        public string SolvedDate { get; set; }
        public string Status { get; set; }
        public string CreatedTime { get; set; }
    }
}