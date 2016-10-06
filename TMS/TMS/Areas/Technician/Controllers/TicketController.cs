using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TMS.Areas.Technician.Controllers
{
    public class TicketController : Controller
    {
        // GET: Technician/Ticket
        public ActionResult Index()
        {
            return View();
        }
    }
}