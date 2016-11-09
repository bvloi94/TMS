using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using TMS.DAL;
using TMS.Models;
using TMS.Services;
using TMS.ViewModels;
using TMS.Utils;
using System;
using System.IO;

namespace TMS.Areas.Technician.Controllers
{
    public class ProfileController : Controller
    {
        private UserService _userService;
        private DepartmentService _departmentService;
        private UnitOfWork _unitOfWork;


        public ProfileController()
        {
            _unitOfWork = new UnitOfWork();
            _departmentService = new DepartmentService(_unitOfWork);
            _userService = new UserService(_unitOfWork);
        }
        // GET: Technician/Profile
        [CustomAuthorize(Roles = "Technician")]
        [HttpGet]
        public ActionResult Index()
        {
            string userId = User.Identity.GetUserId();
            AspNetUser user = _userService.GetUserById(userId);
            ProfileTechnicianViewModel model = new ProfileTechnicianViewModel();
            model.Fullname = user.Fullname;
            model.Address = user.Address;
            model.Birthday = user.Birthday;
            model.Gender = user.Gender;
            model.PhoneNumber = user.PhoneNumber;
            model.Email = user.Email;
            model.DepartmentID = user.DepartmentID;
            model.DepartmentName = _departmentService.GetDepartmentById(model.DepartmentID.Value).Name;

            ViewBag.departmentList = new SelectList(_departmentService.GetAll(), "ID", "Name");
            ViewBag.Username = user.UserName;
            ViewBag.AvatarURL = user.AvatarURL;
            return View(model);
        }

        // GET: Technician/UpdateProfile
        [CustomAuthorize(Roles = "Technician")]
        [HttpGet]
        public ActionResult UpdateProfile()
        {
            string userId = User.Identity.GetUserId();
            AspNetUser user = _userService.GetUserById(userId);
            ProfileTechnicianViewModel model = new ProfileTechnicianViewModel();
            model.Fullname = user.Fullname;
            model.Address = user.Address;
            model.Birthday = user.Birthday;
            model.Gender = user.Gender;
            model.PhoneNumber = user.PhoneNumber;
            model.Email = user.Email;
            model.DepartmentID = user.DepartmentID;
            model.DepartmentName = _departmentService.GetDepartmentById(model.DepartmentID.Value).Name;

            ViewBag.departmentList = new SelectList(_departmentService.GetAll(), "ID", "Name");
            ViewBag.Username = user.UserName;
            ViewBag.AvatarURL = user.AvatarURL;
            return View(model);
        }
        // POST: Technician/UpdateProfile
        [CustomAuthorize(Roles = "Technician")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditProfie(ProfileTechnicianViewModel model)
        {
            string userId = User.Identity.GetUserId();
            if (_userService.IsDuplicatedEmail(userId, model.Email))
            {
                ModelState.AddModelError("Email", String.Format("Email '{0}' is already taken.", model.Email));
            }

            AspNetUser technician = _userService.GetUserById(userId);
            if (ModelState.IsValid)
            {
                technician.Fullname = model.Fullname;
                technician.Email = model.Email;
                technician.Birthday = model.Birthday;
                technician.Address = model.Address;
                technician.PhoneNumber = model.PhoneNumber;
                technician.Gender = model.Gender;
                technician.DepartmentID = model.DepartmentID;
                // handle avatar
                if (model.Avatar != null)
                {
                    string fileName = model.Avatar.FileName.Replace(Path.GetFileNameWithoutExtension(model.Avatar.FileName), technician.Id);
                    string filePath = Path.Combine(Server.MapPath("~/Uploads/Avatar"), fileName);
                    model.Avatar.SaveAs(filePath);
                    technician.AvatarURL = "/Uploads/Avatar/" + fileName;
                }
                _userService.EditUser(technician);
                return RedirectToAction("Index");
            }
            ViewBag.departmentList = new SelectList(_departmentService.GetAll(), "ID", "Name");
            ViewBag.Username = technician.UserName;
            ViewBag.AvatarURL = technician.AvatarURL;
            return View("UpdateProfile", model);
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string id = User.Identity.GetUserId();
            AspNetUser technician = _userService.GetUserById(id);
            if (technician != null)
            {
                ViewBag.LayoutName = technician.Fullname;
                ViewBag.LayoutAvatarURL = technician.AvatarURL;
            }
            base.OnActionExecuting(filterContext);
        }
    }
}