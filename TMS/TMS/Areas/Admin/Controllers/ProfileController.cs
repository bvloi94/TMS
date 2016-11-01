using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using TMS.Models;
using TMS.Services;
using TMS.ViewModels;
using TMS.DAL;
using System.IO;
using TMS.Utils;

namespace TMS.Areas.Admin.Controllers
{
    public class ProfileController : Controller
    {
        private UnitOfWork _unitOfWork;
        private UserService _userService;

        public ProfileController()
        {
            _unitOfWork = new UnitOfWork();
            _userService = new UserService(_unitOfWork);
        }

        // GET: Admin/Profile
        [CustomAuthorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult Index()
        {
            string userId = User.Identity.GetUserId();
            AspNetUser user = _userService.GetUserById(userId);
            ProfileAdminViewModel model = new ProfileAdminViewModel();
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

        //GET: /Admin/Profile/UpdateProfile
        [CustomAuthorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult UpdateProfile()
        {
            string userId = User.Identity.GetUserId();
            AspNetUser user = _userService.GetUserById(userId);
            ProfileAdminViewModel model = new ProfileAdminViewModel();
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

        // POST: Admin/Profile/UpdateProfile
        [CustomAuthorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditProfie(ProfileAdminViewModel model)
        {
            string userId = User.Identity.GetUserId();
            if (_userService.IsDuplicatedEmail(userId, model.Email))
            {
                ModelState.AddModelError("Email", String.Format("Email '{0}' is already taken.", model.Email));
            }

            AspNetUser admin = _userService.GetUserById(userId);
            if (ModelState.IsValid)
            {
                admin.Fullname = model.FullName;
                admin.Email = model.Email;
                admin.Birthday = model.DayOfBirth;
                admin.Address = model.Address;
                admin.PhoneNumber = model.Phone;
                admin.Gender = model.Gender;
                // handle avatar
                // handle avatar
                if (model.Avatar != null)
                {
                    string fileName = model.Avatar.FileName.Replace(Path.GetFileNameWithoutExtension(model.Avatar.FileName), admin.Id);
                    string filePath = Path.Combine(Server.MapPath("~/Uploads/Avatar"), fileName);
                    model.Avatar.SaveAs(filePath);
                    admin.AvatarURL = "/Uploads/Avatar/"+ fileName;
                }
               _userService.EditUser(admin);
                return RedirectToAction("Index");
            }
            ViewBag.username = admin.UserName;
            ViewBag.AvatarURL = admin.AvatarURL;
            return View("UpdateProfile", model);
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