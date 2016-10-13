﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TMS.ViewModels
{
    public class NewTicketViewModel
    {
        public string Subject { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Urgency { get; set; }
        public string Priority { get; set; }
        public string Impact { get; set; }
        public string ImpactDetail { get; set; }
        public string Category { get; set; }
        public string Department { get; set; }
        public string AssignedTo { get; set; }
        public string Requester { get; set; }
        public string Solution { get; set; }
    }
}