using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TMS.DAL;
using TMS.Models;
using TMS.Services;
using TMS.Utils;
using TMS.ViewModels;


namespace TMS.Areas.HelpDesk.Controllers
{
    [CustomAuthorize(Roles = "Helpdesk")]
    public class HomeController : Controller
    {
        UnitOfWork unitOfWork = new UnitOfWork();
        public UserService _userService { get; set; }
        public TicketService _ticketService { get; set; }
        public SolutionService _solutionService { get; set; }
        public NotificationService _notificationService { get; set; }

        public HomeController()
        {
            _userService = new UserService(unitOfWork);
            _ticketService = new TicketService(unitOfWork);
            _solutionService = new SolutionService(unitOfWork);
            _notificationService = new NotificationService(unitOfWork);
        }

        // GET: HelpDesk/Home
        public ActionResult Index()
        {
            IEnumerable<BasicTicketViewModel> newTicketList = _ticketService.LoadAllTickets();
            IEnumerable<BasicTicketViewModel> requestersTicketList = _ticketService.LoadRequestersTickets();
            IEnumerable<BasicTicketViewModel> ticketsInLast7Days = _ticketService.LoadTicketsInLast7Days();
            IEnumerable<BasicTicketViewModel> warningTickets = _ticketService.LoadWarningTickets();

            ViewBag.AllNewTickets = newTicketList.Where(m => m.Status == ConstantUtil.TicketStatus.Open).OrderByDescending(m => m.CreatedTime);
            ViewBag.WarningTickets = warningTickets;
            ViewBag.NewRequestersTickets = requestersTicketList.Where(m => m.Status == ConstantUtil.TicketStatus.Open);
            ViewBag.NewTicketsLast7Days = ticketsInLast7Days.Where(m => m.Status == ConstantUtil.TicketStatus.Open);
            ViewBag.UnapprovedTickets = newTicketList.Where(m => m.Status == ConstantUtil.TicketStatus.Unapproved);
            return View();
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string id = User.Identity.GetUserId();
            AspNetUser admin = _userService.GetUserById(id);
            if (admin != null)
            {
                ViewBag.LayoutName = admin.Fullname;
                ViewBag.LayoutAvatarURL = admin.AvatarURL;
            }
            base.OnActionExecuting(filterContext);
        }
    }
}