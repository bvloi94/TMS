using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using TMS.DAL;
using TMS.Services;
using TMS.Utils;
using TMS.Models;
using TMS.ViewModels;
using System.IO;

namespace TMS.Areas.HelpDesk.Controllers
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

        // GET: HelpDesk/Profile
        [CustomAuthorize(Roles = "Helpdesk")]
        [HttpGet]
        public ActionResult Index()
        {
            string userId = User.Identity.GetUserId();
            AspNetUser user = _userService.GetUserById(userId);
            ProfileHelpdeskViewModel model = new ProfileHelpdeskViewModel();
            model.FullName = user.Fullname;
            model.Address = user.Address;
            model.DayOfBirth = user.Birthday;
            model.Gender = user.Gender;
            model.Phone = user.PhoneNumber;

            ViewBag.Username = user.UserName;
            ViewBag.AvatarURL = user.AvatarURL;
            return View(model);
        }

        [HttpGet]
        public ActionResult UpdateProfile()
        {
            string userId = User.Identity.GetUserId();
            AspNetUser user = _userService.GetUserById(userId);
            ProfileHelpdeskViewModel model = new ProfileHelpdeskViewModel();
            model.FullName = user.Fullname;
            model.Phone = user.PhoneNumber;
            model.Address = user.Address;
            model.DayOfBirth = user.Birthday;
            model.Gender = user.Gender;

            ViewBag.Username = user.UserName;
            ViewBag.AvatarURL = user.AvatarURL;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProfie(ProfileHelpdeskViewModel model)
        {
            string userId = User.Identity.GetUserId();
            AspNetUser helpDesk = _userService.GetUserById(userId);
            if (ModelState.IsValid)
            {
                helpDesk.Fullname = model.FullName;
                helpDesk.Birthday = model.DayOfBirth;
                helpDesk.Address = model.Address;
                helpDesk.PhoneNumber = model.Phone;
                helpDesk.Gender = model.Gender;
                // handle avatar
                if (model.Avatar != null)
                {
                    string fileName = model.Avatar.FileName.Replace(Path.GetFileNameWithoutExtension(model.Avatar.FileName), helpDesk.Id);
                    string filePath = Path.Combine(Server.MapPath("~/Uploads/Avatar"), fileName);
                    model.Avatar.SaveAs(filePath);
                    helpDesk.AvatarURL = "/Uploads/Avatar/"+ fileName;
                }
                _userService.EditUser(helpDesk);
                return RedirectToAction("Index");
            }

            ViewBag.username = helpDesk.UserName;
            ViewBag.AvatarURL = helpDesk.AvatarURL;
            return View("UpdateProfile", model);
        }
    }
}