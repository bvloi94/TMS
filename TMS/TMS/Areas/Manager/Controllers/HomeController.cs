using System;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using TMS.Models;
using TMS.Services;
using TMS.ViewModels;
using TMS.DAL;
using System.IO;
using TMS.Utils;

namespace TMS.Areas.Manager.Controllers
{
    [CustomAuthorize(Roles = "Manager")]
    public class HomeController : Controller
    {
        private UnitOfWork _unitOfWork;
        private UserService _userService;

        public HomeController()
        {
            _unitOfWork = new UnitOfWork();
            _userService = new UserService(_unitOfWork);
        }

        [HttpGet]
        public ActionResult Index()
        {
            var name = User.Identity.Name;
            AspNetUser currentUser = _userService.GetUserById(User.Identity.GetUserId());
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.UserInfo = currentUser;
            return View();
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string id = User.Identity.GetUserId();
            AspNetUser manager = _userService.GetUserById(id);
            if (manager != null)
            {
                ViewBag.LayoutName = manager.Fullname;
                ViewBag.LayoutAvatarURL = manager.AvatarURL;
            }
            base.OnActionExecuting(filterContext);
        }
    }
}