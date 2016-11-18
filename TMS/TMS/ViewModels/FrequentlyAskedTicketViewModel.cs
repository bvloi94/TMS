using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TMS.Models;

namespace TMS.ViewModels
{
    public class FrequentlyAskedTicketViewModel
    {
        public Ticket Ticket { get; set; }
        public int Frequency { get; set; }
    }
}