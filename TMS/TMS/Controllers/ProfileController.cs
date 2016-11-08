using Microsoft.AspNet.Identity;
using System;
using System.IO;
using System.Web.Mvc;
using TMS.DAL;
using TMS.Models;
using TMS.Services;
using TMS.Utils;
using TMS.ViewModels;

namespace TMS.Controllers
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
        // GET: Profile
        //[CustomAuthorize(Roles = "Requester")]
        public ActionResult Index()
        {
            string userId = User.Identity.GetUserId();
            AspNetUser user = _userService.GetUserById(userId);
            ProfileRequesterViewModel model = new ProfileRequesterViewModel();
            model.Email = user.Email;
            model.Fullname = user.Fullname;
            model.Address = user.Address;
            model.PhoneNumber = user.PhoneNumber;
            model.Birthday = user.Birthday;
            model.Gender = user.Gender;
            model.JobTitle = user.JobTitle;
            model.DepartmentID = user.DepartmentID;
            model.DepartmentName = user.DepartmentName;
            model.CompanyName = user.CompanyName;
            model.CompanyAddress = user.CompanyAddress;

            ViewBag.Username = user.UserName;
            ViewBag.AvatarURL = user.AvatarURL;
            return View(model);
        }

        // GET: UpdateProfile
        public ActionResult UpdateProfile()
        {
            string userId = User.Identity.GetUserId();
            AspNetUser user = _userService.GetUserById(userId);
            ProfileRequesterViewModel model = new ProfileRequesterViewModel();
            model.Email = user.Email;
            model.Fullname = user.Fullname;
            model.Address = user.Address;
            model.PhoneNumber = user.PhoneNumber;
            model.Birthday = user.Birthday;
            model.Gender = user.Gender;
            model.JobTitle = user.JobTitle;
            model.DepartmentID = user.DepartmentID;
            model.DepartmentName = user.DepartmentName;
            model.CompanyName = user.CompanyName;
            model.CompanyAddress = user.CompanyAddress;

            ViewBag.Username = user.UserName;
            ViewBag.AvatarURL = user.AvatarURL;
            return View(model);
        }

        public ActionResult EditProfile(ProfileRequesterViewModel model)
        {
            string userId = User.Identity.GetUserId();
            if (_userService.IsDuplicatedEmail(userId, model.Email))
            {
                ModelState.AddModelError("Email", String.Format("Email '{0}' is already taken.", model.Email));
            }

            AspNetUser requester = _userService.GetUserById(userId);
            if (ModelState.IsValid)
            {
                requester.Email = model.Email;
                requester.Fullname = model.Fullname;
                requester.Address = model.Address;
                requester.PhoneNumber = model.PhoneNumber;
                requester.Birthday = model.Birthday;
                requester.Gender = model.Gender;
                requester.JobTitle = model.JobTitle;
                requester.DepartmentID = model.DepartmentID;
                requester.DepartmentName = model.DepartmentName;
                requester.CompanyName = model.CompanyName;
                requester.CompanyAddress = model.CompanyAddress;

                // handle avatar
                if (model.Avatar != null)
                {
                    string fileName = model.Avatar.FileName.Replace(Path.GetFileNameWithoutExtension(model.Avatar.FileName), requester.Id);
                    string filePath = Path.Combine(Server.MapPath("~/Uploads/Avatar"), fileName);
                    model.Avatar.SaveAs(filePath);
                    requester.AvatarURL = "/Uploads/Avatar/" + fileName;
                }
                _userService.EditUser(requester);
                return RedirectToAction("Index");
            }
            ViewBag.username = requester.UserName;
            ViewBag.AvatarURL = requester.AvatarURL;
            return View("UpdateProfile", model);
        }
    }
}