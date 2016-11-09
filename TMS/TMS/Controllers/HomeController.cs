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

namespace TMS.Controllers
{
    public class HomeController : Controller
    {
        UnitOfWork unitOfWork = new UnitOfWork();
        public UserService _userService { get; set; }
        public TicketService _ticketService { get; set; }

        public HomeController()
        {
            _userService = new UserService(unitOfWork);
            _ticketService = new TicketService(unitOfWork);
        }

        [CustomAuthorize(Roles = "Requester")]
        public ActionResult Index()
        {
            var name = User.Identity.Name;
            AspNetUser currentUser = _userService.GetUserById(User.Identity.GetUserId());
            IEnumerable<Ticket> filteredListItems = _ticketService.GetRequesterTickets(User.Identity.GetUserId())
                .Where(p => p.Status == ConstantUtil.TicketStatus.Solved).ToArray().OrderByDescending(m => m.SolvedDate);
            if (filteredListItems.Count() > 0)
            {
                IEnumerable<BasicTicketViewModel> ticketList = filteredListItems.Select(m => new BasicTicketViewModel
                {
                    Code = m.Code,
                    ID = m.ID,
                    Subject = m.Subject,
                    Category = m.Category == null ? "-" : m.Category.Name,
                    SolvedBy = m.SolveID == null ? "-" : _userService.GetUserById(m.SolveID).Fullname,
                    CreatedTime = GeneralUtil.ShowDateTime(m.CreatedTime),
                    SolvedTime = m.SolvedDate == null ? " - " : GeneralUtil.ShowDateTime(m.SolvedDate.Value) 
                }).ToArray();
                ViewBag.SolvedTicket = ticketList;
            }

            ViewBag.UserInfo = currentUser;

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