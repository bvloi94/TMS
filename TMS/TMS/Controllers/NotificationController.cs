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
            string userRole = _userService.GetUserById(id).AspNetRoles.FirstOrDefault().Name;
            IEnumerable<NotificationViewModel> notificationList;
            if (userRole == "Helpdesk")
            {
                notificationList = _notificationService.GetAll().OrderByDescending(m => m.NotifiedTime)
                .Where(m => m.IsForHelpDesk == true).Select(m => new NotificationViewModel
                {
                    Id = m.ID,
                    TicketId = m.TicketID,
                    NotifiedTime = m.NotifiedTime.HasValue ? GeneralUtil.ShowDateTime(m.NotifiedTime.Value) : "-",
                    NotificationContent = m.NotificationContent,
                    IsRead = m.IsRead
                }).ToArray().Take(20);
            }
            else
            {
                notificationList = _notificationService.GetUserNotifications(id).OrderByDescending(m => m.NotifiedTime)
                .Select(m => new NotificationViewModel
                {
                    Id = m.ID,
                    TicketId = m.TicketID,
                    NotifiedTime = m.NotifiedTime.HasValue ? GeneralUtil.ShowDateTime(m.NotifiedTime.Value) : "-",
                    NotificationContent = m.NotificationContent,
                    IsRead = m.IsRead
                }).ToArray().Take(20);
            }

            return Json(new
            {
                data = notificationList,
                userRole = userRole
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SetNotificationToRead(int? id)
        {
            if (id.HasValue)
            {
                Notification notification = _notificationService.GetNotificationById(id.Value);
                notification.IsRead = true;
                _notificationService.EditNotification(notification);

                return Json(new
                {
                    data = true,
                });
            }

            return Json(new
            {
                data = false,
            });
        }
    }
}