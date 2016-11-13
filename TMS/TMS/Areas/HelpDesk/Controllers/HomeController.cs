using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TMS.DAL;
using TMS.Models;
using TMS.Services;
using TMS.Utils;
using TMS.ViewModels;


namespace TMS.Areas.HelpDesk.Controllers
{
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
            IEnumerable<BasicTicketViewModel> ticketList = LoadRequestersTickets();
            IEnumerable<BasicTicketViewModel> ticketsInLast7Days = LoadRequestersTicketsInLast7Days();

            ViewBag.NewRequestersTickets = ticketList.Where(m => m.Status == ConstantUtil.TicketStatus.New);
            ViewBag.NewRequestersTicketsLast7Days = ticketsInLast7Days.Where(m => m.Status == ConstantUtil.TicketStatus.New);
            ViewBag.UnapprovedTickets = ticketList.Where(m => m.Status == ConstantUtil.TicketStatus.Unapproved);
            return View();
        }

        public IEnumerable<BasicTicketViewModel> LoadRequestersTickets()
        {
            IEnumerable<Ticket> ticketList = _ticketService.GetAll().Where(m => _userService.GetUserById(m.CreatedID).AspNetRoles.FirstOrDefault().Name == "Requester").ToArray();
            IEnumerable<BasicTicketViewModel> filterList = ticketList.Select(
                m => new BasicTicketViewModel
                {
                    Code = m.Code,
                    ID = m.ID,
                    Status = m.Status,
                    Subject = m.Subject,
                    CreatedBy = m.CreatedID == null ? "-" : _userService.GetUserById(m.CreatedID).Fullname,
                    CreatedTime = GeneralUtil.ShowDateTime(m.CreatedTime),
                    ModifiedTime = GeneralUtil.ShowDateTime(m.ModifiedTime),
                }).ToArray();
            return filterList;
        }

        public IEnumerable<BasicTicketViewModel> LoadRequestersTicketsInLast7Days()
        {
            IEnumerable<Ticket> ticketList = _ticketService.GetAll().Where(m => DateTime.Now.Subtract(m.CreatedTime).Days < 7 && _userService.GetUserById(m.CreatedID).AspNetRoles.FirstOrDefault().Name == "Requester").ToArray();
            IEnumerable<BasicTicketViewModel> filterList = ticketList.Select(
                m => new BasicTicketViewModel
                {
                    Code = m.Code,
                    ID = m.ID,
                    Status = m.Status,
                    Subject = m.Subject,
                    CreatedBy = m.CreatedID == null ? "-" : _userService.GetUserById(m.CreatedID).Fullname,
                    CreatedTime = GeneralUtil.ShowDateTime(m.CreatedTime),
                    ModifiedTime = GeneralUtil.ShowDateTime(m.ModifiedTime),
                }).ToArray();
            return filterList;
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