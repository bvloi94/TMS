using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TMS.ViewModels
{
    public class NotificationViewModel
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string NotifiedTime { get; set; }
        public string NotificationContent { get; set; }
        public bool IsRead { get; set; }
    }
}