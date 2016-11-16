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
            IEnumerable<BasicTicketViewModel> newTicketList = LoadAllTickets();
            IEnumerable<BasicTicketViewModel> requestersTicketList = LoadRequestersTickets();
            IEnumerable<BasicTicketViewModel> ticketsInLast7Days = LoadTicketsInLast7Days();
            IEnumerable<BasicTicketViewModel> warningTickets = LoadWarningTickets();

            ViewBag.AllNewTickets = newTicketList.Where(m => m.Status == ConstantUtil.TicketStatus.Open).OrderByDescending(m => m.CreatedTime);
            ViewBag.WarningTickets = warningTickets;
            ViewBag.NewRequestersTickets = requestersTicketList.Where(m => m.Status == ConstantUtil.TicketStatus.Open);
            ViewBag.NewTicketsLast7Days = ticketsInLast7Days.Where(m => m.Status == ConstantUtil.TicketStatus.Open);
            ViewBag.UnapprovedTickets = newTicketList.Where(m => m.Status == ConstantUtil.TicketStatus.Unapproved);
            return View();
        }

        public IEnumerable<BasicTicketViewModel> LoadAllTickets()
        {
            IEnumerable<Ticket> ticketList = _ticketService.GetAll().ToArray();
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
                }).ToArray().OrderBy(m => m.CreatedTime);
            return filterList;
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
                }).ToArray().OrderBy(m => m.CreatedTime);
            return filterList;
        }

        public IEnumerable<BasicTicketViewModel> LoadTicketsInLast7Days()
        {
            IEnumerable<Ticket> ticketList = _ticketService.GetAll().Where(m => DateTime.Now.Subtract(m.CreatedTime).Days < 7).ToArray();
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
                }).ToArray().OrderBy(m => m.CreatedTime);
            return filterList;
        }

        public IEnumerable<BasicTicketViewModel> LoadWarningTickets()
        {
            IEnumerable<BasicTicketViewModel> incomingTickets = _ticketService.GetAll().Where(p => p.ScheduleEndDate.HasValue &&
                    p.ScheduleEndDate.Value.Subtract(DateTime.Now).Days < 3 && p.Status == ConstantUtil.TicketStatus.Assigned)
                    .Select(m => new BasicTicketViewModel
                    {
                        Code = m.Code,
                        ID = m.ID,
                        Status = m.Status,
                        Subject = m.Subject,
                        ScheduleEndTime = m.ScheduleEndDate.Value.ToString("MMMM dd, yyyy  hh:mm"),
                    }).ToArray().OrderByDescending(m => m.ScheduleEndTime);
            return incomingTickets;
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