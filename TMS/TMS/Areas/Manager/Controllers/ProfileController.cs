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
    public class ProfileController : Controller
    {
        private UnitOfWork _unitOfWork;
        private UserService _userService;

        public ProfileController()
        {
            _unitOfWork = new UnitOfWork();
            _userService = new UserService(_unitOfWork);
        }

        // GET: Manager/Profile
        [HttpGet]
        public ActionResult Index()
        {
            string userId = User.Identity.GetUserId();
            AspNetUser user = _userService.GetUserById(userId);
            ProfileManagerViewModel model = new ProfileManagerViewModel();
            model.FullName = user.Fullname;
            model.Address = user.Address;
            model.DayOfBirth = user.Birthday;
            model.Gender = user.Gender;
            model.Phone = user.PhoneNumber;
            model.Email = user.Email;

            ViewBag.Username = user.UserName;
            ViewBag.AvatarURL = user.AvatarURL;
            return View(model);
        }

        //GET: /Manager/Profile/UpdateProfile
        [HttpGet]
        public ActionResult UpdateProfile()
        {
            string userId = User.Identity.GetUserId();
            AspNetUser user = _userService.GetUserById(userId);
            ProfileManagerViewModel model = new ProfileManagerViewModel();
            model.FullName = user.Fullname;
            model.Address = user.Address;
            model.DayOfBirth = user.Birthday;
            model.Gender = user.Gender;
            model.Phone = user.PhoneNumber;
            model.Email = user.Email;

            ViewBag.Username = user.UserName;
            ViewBag.AvatarURL = user.AvatarURL;
            return View(model);
        }

        // POST: Manager/Profile/UpdateProfile
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditProfie(ProfileManagerViewModel model)
        {
            string userId = User.Identity.GetUserId();
            if (_userService.IsDuplicatedEmail(userId, model.Email))
            {
                ModelState.AddModelError("Email", String.Format("Email '{0}' is already taken.", model.Email));
            }

            AspNetUser manager = _userService.GetUserById(userId);
            if (ModelState.IsValid)
            {
                manager.Fullname = model.FullName;
                manager.Email = model.Email;
                manager.Birthday = model.DayOfBirth;
                manager.Address = model.Address;
                manager.PhoneNumber = model.Phone;
                manager.Gender = model.Gender;
                // handle avatar
                // handle avatar
                if (model.Avatar != null)
                {
                    string fileName = model.Avatar.FileName.Replace(Path.GetFileNameWithoutExtension(model.Avatar.FileName), manager.Id);
                    string filePath = Path.Combine(Server.MapPath("~/Uploads/Avatar"), fileName);
                    model.Avatar.SaveAs(filePath);
                    manager.AvatarURL = "/Uploads/Avatar/"+ fileName;
                }
               _userService.EditUser(manager);
                return RedirectToAction("Index");
            }
            ViewBag.username = manager.UserName;
            ViewBag.AvatarURL = manager.AvatarURL;
            return View("UpdateProfile", model);
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