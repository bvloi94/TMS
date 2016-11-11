using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
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
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ProfileController()
        {
            _unitOfWork = new UnitOfWork();
            _userService = new UserService(_unitOfWork);
        }

        public ProfileController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
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

        [CustomAuthorize(Roles = "Admin,Helpdesk,Technician,Requester,Manager")]
        public ActionResult ChangePassword()
        {
            string role = _userService.GetUserById(User.Identity.GetUserId()).AspNetRoles.FirstOrDefault().Name.ToLower();
            ViewBag.Role = role;
            return View();
        }

        // POST: /Manage/ChangePassword
        [CustomAuthorize(Roles = "Admin,Helpdesk,Technician,Requester,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            string role = _userService.GetUserById(User.Identity.GetUserId()).AspNetRoles.FirstOrDefault().Name.ToLower();
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                switch (role)
                {
                    case "admin":
                        return RedirectToAction("Index", "Profile", new { area = "Admin" });
                    case "helpdesk":
                        return RedirectToAction("Index", "Profile", new { area = "Helpdesk" });
                    case "technician":
                        return RedirectToAction("Index", "Profile", new { area = "Technician" });
                    case "manager":
                        return RedirectToAction("Index", "Profile", new { area = "Manager" });
                    case "requester":
                        return RedirectToAction("Index", "Profile");
                }
            }
            AddErrors(result);
            ViewBag.Role = role;
            return View(model);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                if (error.StartsWith("Email", StringComparison.InvariantCultureIgnoreCase))
                {
                    ModelState.AddModelError("Email", error);
                }
                else if (error.StartsWith("Name", StringComparison.InvariantCultureIgnoreCase))
                {
                    ModelState.AddModelError("Username", error);
                }
                else
                {
                    ModelState.AddModelError("", error);
                }
            }
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string id = User.Identity.GetUserId();
            AspNetUser technician = _userService.GetUserById(id);
            if (technician != null)
            {
                ViewBag.UserRole = technician.AspNetRoles.FirstOrDefault().Name;
                ViewBag.LayoutName = technician.Fullname;
                ViewBag.LayoutAvatarURL = technician.AvatarURL;
            }
            base.OnActionExecuting(filterContext);
        }
    }
}