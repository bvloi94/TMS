using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using TMS.DAL;
using TMS.Models;
using TMS.Services;
using TMS.Utils;

namespace TMS.Areas.Admin.Controllers
{
    public class ManageUserController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private UnitOfWork _unitOfWork;
        private UserService _userService;

        public ManageUserController()
        {
            _unitOfWork = new UnitOfWork();
            _userService = new UserService(_unitOfWork);
        }

        public ManageUserController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            _unitOfWork = new UnitOfWork();
            _userService = new UserService(_unitOfWork);
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

        // GET: Admin/ManageUser
        public ActionResult Index()
        {
            return View();
        }

        // GET: Admin/ManageUser/Requester
        public ActionResult Requester()
        {
            IEnumerable<AspNetUser> requesters = _userService.GetRequesters();
            //ViewBag.requesters = requesters.ToList();
            return View(requesters);
        }

        // GET: Admin/ManageUser/GetRequesters
        [HttpGet]
        public ActionResult GetRequesters(jQueryDataTableParamModel param, string email_search_key)
        {
            var requesters = _userService.GetRequesters();

            IEnumerable<AspNetUser> filteredListItems;

            if (!string.IsNullOrEmpty(param.sSearch))
            {
                filteredListItems = requesters.Where(p => p.Fullname.ToLower().Contains(param.sSearch.ToLower()));
            }
            else
            {
                filteredListItems = requesters;
            }
            // Search by custom
            if (!string.IsNullOrEmpty(email_search_key))
            {
                filteredListItems = filteredListItems.Where(p => p.Email.ToLower().Contains(email_search_key.ToLower()));
            }

            // Sort.
            var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
            var sortDirection = Request["sSortDir_0"]; // asc or desc

            switch (sortColumnIndex)
            {
                case 0:
                    filteredListItems = sortDirection == "asc"
                        ? filteredListItems.OrderBy(p => p.UserName)
                        : filteredListItems.OrderByDescending(p => p.UserName);
                    break;
                case 1:
                    filteredListItems = sortDirection == "asc"
                        ? filteredListItems.OrderBy(p => p.Fullname)
                        : filteredListItems.OrderByDescending(p => p.Fullname);
                    break;
            }

            var displayedList = filteredListItems.Skip(param.iDisplayStart).Take(param.iDisplayLength);
            var result = displayedList.Select(p => new IConvertible[]{
                p.Id,
                p.UserName,
                p.Fullname,
                p.Email,
                p.Birthday.Value.ToShortDateString()
            }.ToArray());

            return Json(new
            {
                param.sEcho,
                iTotalRecords = result.Count(),
                iTotalDisplayRecords = filteredListItems.Count(),
                aaData = result
            }, JsonRequestBehavior.AllowGet);
        }

        // GET: Admin/ManageUser/CreateRequester
        public ActionResult CreateRequester()
        {
            return View();
        }

        // POST: Admin/ManageUser/CreateRequester
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateRequester(RequesterRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    AspNetUser requester = _userService.GetUserById(user.Id);
                    requester.Fullname = model.Fullname;
                    requester.Birthday = Convert.ToDateTime(model.Birthday);
                    requester.Address = model.Address;
                    requester.Gender = model.Gender;
                    requester.DepartmentName = model.DepartmentName;
                    requester.JobTitle = model.JobTitle;
                    requester.CompanyName = model.CompanyName;
                    requester.CompanyAddress = model.CompanyAddress;
                    requester.IsActive = true;
                    _userService.EditUser(requester);

                    ApplicationDbContext context = new ApplicationDbContext();

                    var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                    if (!roleManager.RoleExists("Requester"))
                    {
                        var role = new IdentityRole();
                        role.Name = "Requester";
                        roleManager.Create(role);
                    }
                    UserManager.AddToRole(user.Id, "Requester");
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    EmailUtil emailUtil = new EmailUtil();
                    string emailMessage = System.IO.File.ReadAllText(Server.MapPath(@"~/EmailTemplates/CreateRequesterEmailTemplate.txt"));
                    emailMessage = emailMessage.Replace("$username", model.UserName);
                    emailMessage = emailMessage.Replace("$password", model.Password);
                    await emailUtil.SendEmail("huytcdse61256@fpt.edu.vn", "Test", emailMessage);

                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Requester", "ManageUser");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // GET: Admin/ManageUser/EditRequester/{id}
        public ActionResult EditRequester(string id)
        {
            AspNetUser requester = _userService.GetUserById(id);
            RequesterRegisterViewModel model = new RequesterRegisterViewModel();
            model.UserName = requester.UserName;
            model.Fullname = requester.Fullname;
            model.Email = requester.Email;
            model.Birthday = requester.Birthday.Value.ToShortDateString();
            model.Address = requester.Address;
            model.Gender = requester.Gender;
            model.DepartmentName = requester.DepartmentName;
            model.JobTitle = requester.JobTitle;
            model.CompanyName = requester.CompanyName;
            model.CompanyAddress = requester.CompanyAddress;

            ViewBag.id = id;
            return View(model);
        }

        // GET: Admin/ManageUser/EditRequester/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditRequester(RequesterRegisterViewModel model, string id)
        {
            ModelState.Remove("UserName");
            ModelState.Remove("Password");
            if (_userService.IsDuplicatedEmail(id, model.Email))
            {
                ModelState.AddModelError("Email", String.Format("Email '{0}' is already taken.", model.Email));
            }

            AspNetUser requester = _userService.GetUserById(id);
            model.UserName = requester.UserName;

            if (ModelState.IsValid)
            {
                requester.Fullname = model.Fullname;
                requester.Email = model.Email;
                requester.Birthday = Convert.ToDateTime(model.Birthday);
                requester.Address = model.Address;
                requester.Gender = model.Gender;
                requester.DepartmentName = model.DepartmentName;
                requester.JobTitle = model.JobTitle;
                requester.CompanyName = model.CompanyName;
                requester.CompanyAddress = model.CompanyAddress;
                _userService.EditUser(requester);

                return RedirectToAction("Requester");
            }
            ViewBag.id = id;
            return View(model);
        }

        [HttpPost]
        public ActionResult RemoveRequester()
        {
            string id = Request["id"];
            _userService.RemoveUser(id);
            return Json(new
            {
                success = true,
                message = "Remove requester successfully!"
            });
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
    }
}