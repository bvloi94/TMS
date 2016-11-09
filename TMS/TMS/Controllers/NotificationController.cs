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

namespace TMS.Controllers
{
    public class NotificationController : Controller
    {

        UnitOfWork unitOfWork = new UnitOfWork();
        public UserService _userService { get; set; }
        public TicketService _ticketService { get; set; }
        public SolutionService _solutionService { get; set; }
        public NotificationService _notificationService { get; set; }

        public NotificationController()
        {
            _userService = new UserService(unitOfWork);
            _ticketService = new TicketService(unitOfWork);
            _solutionService = new SolutionService(unitOfWork);
            _notificationService = new NotificationService(unitOfWork);
        }

        // GET: Notification
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetNotifications()
        {
            string id = User.Identity.GetUserId();
            IEnumerable<NotificationViewModel> notificationList = _notificationService.GetUserNotifications(id).OrderByDescending(m => m.NotifiedTime)
                .Select(m => new NotificationViewModel {
                    Id = m.ID,
                    TicketId = m.TicketID,
                    NotifiedTime = m.NotifiedTime.HasValue ? GeneralUtil.ShowDateTime(m.NotifiedTime.Value) : "-",
                    NotificationContent = m.NotificationContent
                });

            return Json(new
            {
                data = notificationList
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SetNotifications(int? id)
        {
            string UserID = User.Identity.GetUserId();
            IEnumerable<Notification> notificationList = _notificationService.GetUserNotifications(UserID);

            return Json(new
            {
                data = notificationList.OrderByDescending(m => m.NotifiedTime)
            }, JsonRequestBehavior.AllowGet);
        }
    }
}