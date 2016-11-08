using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TMS.ViewModels
{
    public class TicketHistoryViewModel
    {
        public string ActedDate { get; set; }
        public string Performer { get; set; }
        public string Type { get; set; }
        [AllowHtml]
        public string Action { get; set; }
    }
}