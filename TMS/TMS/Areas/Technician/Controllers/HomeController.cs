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

namespace TMS.Areas.Technician.Controllers
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

        // GET: Technician/Home
        public ActionResult Index()
        {
            var userID = User.Identity.GetUserId();
            int assignedTickets = 0, solvedTickets = 0, closedTickets = 0, newSolutions = 0;

            IEnumerable<Ticket> technicianList = _ticketService.GetTechnicianTickets(userID).
                Where(p => DateTime.Now.Subtract(p.CreatedTime).Days <= 7).ToArray().OrderByDescending(m => m.ModifiedTime);
            if (technicianList != null)
            {
                IEnumerable<BasicTicketViewModel> ticketList = technicianList.Select(m => new BasicTicketViewModel
                {
                    Code = m.Code,
                    ID = m.ID,
                    Status = m.Status,
                    Subject = m.Subject,
                    Category = m.Category == null ? "-" : m.Category.Name,
                    CreatedBy = m.CreatedID == null ? "-" : _userService.GetUserById(m.CreatedID).Fullname,
                    SolvedTime = m.SolvedDate == null ? "-" : GeneralUtil.ShowDateTime(m.SolvedDate.Value),
                    CreatedTime = GeneralUtil.ShowDateTime(m.CreatedTime),
                    ModifiedTime = GeneralUtil.ShowDateTime(m.ModifiedTime),
                }).ToArray();
                ViewBag.TechnicianTicket = ticketList;
                assignedTickets = technicianList.Where(p => p.Status == ConstantUtil.TicketStatus.Assigned).Count();
                solvedTickets = technicianList.Where(p => p.Status == ConstantUtil.TicketStatus.Solved).Count();
                closedTickets = technicianList.Where(p => p.Status == ConstantUtil.TicketStatus.Closed).Count();
            }

            IEnumerable<Solution> solutionList = _solutionService.GetAllSolutions().Where(p => DateTime.Now.Subtract(p.CreatedTime.Value).Days <= 7);
            newSolutions = solutionList.Count();
            ViewBag.SolutionList = solutionList;

            ViewBag.AssignedTicketsNo = assignedTickets;
            ViewBag.SolvedTicketsNo = solvedTickets;
            ViewBag.ClosedTicketsNo = closedTickets;
            ViewBag.NewSolutionsNo = newSolutions;

            return View();
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string id = User.Identity.GetUserId();
            AspNetUser requester = _userService.GetUserById(id);
            if (requester != null)
            {
                ViewBag.LayoutName = requester.Fullname;
                ViewBag.LayoutAvatarURL = requester.AvatarURL;
            }
            base.OnActionExecuting(filterContext);
        }
    }
}