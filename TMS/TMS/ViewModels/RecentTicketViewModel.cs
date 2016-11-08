using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TMS.ViewModels
{
    public class FrequentlyAskedTicketViewModel
    {
        public string Subject { get; set; }
        public string Tags { get; set; }
        public int Count { get; set; }
    }
}