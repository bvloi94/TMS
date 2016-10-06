using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using TMS.Models;

namespace TMS.Controllers
{
    public class ErrorController : Controller
    {

        public ActionResult Error403()
        {
            return View();
        }

        public ActionResult Error500()
        {
            return View();
        }
    }
}