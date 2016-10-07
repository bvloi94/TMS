using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
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
        [Utils.Authorize(Roles = "Admin")]
        public ActionResult Requester()
        {
            return View();
        }

        // GET: Admin/ManageUser/HelpDesk
        [Utils.Authorize(Roles = "Admin")]
        public ActionResult HelpDesk()
        {
            return View();
        }

        // GET: Admin/ManageUser/GetRequesters
        [HttpGet]
        public ActionResult GetRequesters(jQueryDataTableParamModel param)
        {
            var requesters = _userService.GetRequesters();
            var default_search_key = Request["search[value]"];
            var availability_select = Request["availability_select"];
            var search_text = Request["search_text"];
            IEnumerable<AspNetUser> filteredListItems;

            if (!string.IsNullOrEmpty(default_search_key))
            {
                filteredListItems = requesters.Where(p => p.Fullname.ToLower().Contains(default_search_key.ToLower()));
            }
            else
            {
                filteredListItems = requesters;
            }
            // Search by custom
            if (!string.IsNullOrEmpty(availability_select))
            {
                switch (availability_select)
                {
                    case "0":
                        filteredListItems = filteredListItems.Where(p => p.IsActive == false);
                        break;
                    case "1":
                        filteredListItems = filteredListItems.Where(p => p.IsActive == true);
                        break;
                    case "2":
                    default:
                        break;
                }
            }

            if (!string.IsNullOrEmpty(search_text))
            {
                filteredListItems = filteredListItems.Where(p => p.UserName.ToLower().Contains(search_text.ToLower())
                    || p.Fullname.ToLower().Contains(search_text.ToLower()));
            }

            // Sort.
            var sortColumnIndex = Convert.ToInt32(Request["order[0][column]"]);
            var sortDirection = Request["order[0][dir]"];
            //var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
            //var sortDirection = Request["sSortDir_0"]; // asc or desc

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

            var displayedList = filteredListItems.Skip(param.start).Take(param.length);
            var result = displayedList.Select(p => new IConvertible[]{
                p.Id,
                p.UserName,
                p.Fullname,
                p.Email,
                (p.Birthday == null) ? "" : ((DateTime) p.Birthday).ToString("dd/MM/yyyy"),
                p.IsActive
            }.ToArray());

            return Json(new
            {
                param.sEcho,
                //iTotalRecords = result.Count(),
                //iTotalDisplayRecords = filteredListItems.Count(),
                //aaData = result
                recordsTotal = result.Count(),
                recordsFiltered = filteredListItems.Count(),
                data = result
            }, JsonRequestBehavior.AllowGet);
        }

        // GET: Admin/ManageUser/GetHelpDesks
        [HttpGet]
        public ActionResult GetHelpDesks(jQueryDataTableParamModel param)
        {
            var helpdesks = _userService.GetHelpDesks();
            var default_search_key = Request["search[value]"];
            var availability_select = Request["availability_select"];
            var search_text = Request["search_text"];
            IEnumerable<AspNetUser> filteredListItems;

            if (!string.IsNullOrEmpty(default_search_key))
            {
                filteredListItems = helpdesks.Where(p => p.Fullname.ToLower().Contains(default_search_key.ToLower()));
            }
            else
            {
                filteredListItems = helpdesks;
            }
            // Search by custom
            if (!string.IsNullOrEmpty(availability_select))
            {
                switch (availability_select)
                {
                    case "0":
                        filteredListItems = filteredListItems.Where(p => p.IsActive == false);
                        break;
                    case "1":
                        filteredListItems = filteredListItems.Where(p => p.IsActive == true);
                        break;
                    case "2":
                    default:
                        break;
                }
            }

            if (!string.IsNullOrEmpty(search_text))
            {
                filteredListItems = filteredListItems.Where(p => p.UserName.ToLower().Contains(search_text.ToLower())
                    || p.Fullname.ToLower().Contains(search_text.ToLower()));
            }

            // Sort.
            var sortColumnIndex = Convert.ToInt32(Request["order[0][column]"]);
            var sortDirection = Request["order[0][dir]"];
            //var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
            //var sortDirection = Request["sSortDir_0"]; // asc or desc

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

            var displayedList = filteredListItems.Skip(param.start).Take(param.length);
            var result = displayedList.Select(p => new IConvertible[]{
                p.Id,
                p.UserName,
                p.Fullname,
                p.Email,
                (p.Birthday == null) ? "" : ((DateTime) p.Birthday).ToString("dd/MM/yyyy"),
                p.IsActive
            }.ToArray());

            return Json(new
            {
                param.sEcho,
                //iTotalRecords = result.Count(),
                //iTotalDisplayRecords = filteredListItems.Count(),
                //aaData = result
                recordsTotal = result.Count(),
                recordsFiltered = filteredListItems.Count(),
                data = result
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
                var user = new ApplicationUser { UserName = model.Username, Email = model.Email };
                string generatedPassword = GeneralUtil.GeneratePassword();
                var result = await UserManager.CreateAsync(user, generatedPassword);
                if (result.Succeeded)
                {
                    AspNetUser requester = _userService.GetUserById(user.Id);
                    requester.Fullname = model.Fullname;
                    requester.Birthday = string.IsNullOrEmpty(model.Birthday) ? (DateTime?)null : DateTime.ParseExact(model.Birthday, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    requester.Address = model.Address;
                    requester.Gender = model.Gender;
                    requester.DepartmentName = model.DepartmentName;
                    requester.JobTitle = model.JobTitle;
                    requester.CompanyName = model.CompanyName;
                    requester.CompanyAddress = model.CompanyAddress;
                    requester.IsActive = true;
                    // handle avatar
                    if (model.Avatar != null)
                    {
                        string fileName = model.Avatar.FileName.Replace(Path.GetFileNameWithoutExtension(model.Avatar.FileName), user.Id);
                        string filePath = Path.Combine(Server.MapPath("~/Uploads/Avatar"), fileName);
                        model.Avatar.SaveAs(filePath);
                        requester.AvatarURL = fileName;
                    }
                    else
                    {
                        requester.AvatarURL = "avatar_male.png";
                        if (requester.Gender != null)
                        {
                            if (requester.Gender == false)
                            {
                                requester.AvatarURL = "avatar_female.png";
                            }
                        }
                    }
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
                    //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    // Send email asynchronously
                    Thread thread = new Thread(() => EmailUtil.SendToUserWhenCreate(model.Username, generatedPassword, model.Fullname, model.Email));
                    thread.Start();

                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Requester", "ManageUser");
                }
                AddErrors(result);
            }

            return View(model);
        }

        // GET: Admin/ManageUser/EditRequester/{id}
        public ActionResult EditRequester(string id)
        {
            try
            {
                AspNetUser requester = _userService.GetUserById(id);
                RequesterRegisterViewModel model = new RequesterRegisterViewModel();
                model.Fullname = requester.Fullname;
                model.Email = requester.Email;
                model.Birthday = (requester.Birthday == null) ? "" : ((DateTime)requester.Birthday).ToString("dd/MM/yyyy");
                model.Address = requester.Address;
                model.Gender = requester.Gender;
                model.DepartmentName = requester.DepartmentName;
                model.JobTitle = requester.JobTitle;
                model.CompanyName = requester.CompanyName;
                model.CompanyAddress = requester.CompanyAddress;

                ViewBag.id = id;
                ViewBag.username = requester.UserName;
                ViewBag.AvatarURL = requester.AvatarURL;
                return View(model);
            }
            catch
            {
                return RedirectToAction("Error500", "Error", new { area = "" });
            }
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
            if (ModelState.IsValid)
            {
                requester.Fullname = model.Fullname;
                requester.Email = model.Email;
                requester.Birthday = string.IsNullOrEmpty(model.Birthday) ? (DateTime?)null : DateTime.ParseExact(model.Birthday, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                requester.Address = model.Address;
                requester.Gender = model.Gender;
                requester.DepartmentName = model.DepartmentName;
                requester.JobTitle = model.JobTitle;
                requester.CompanyName = model.CompanyName;
                requester.CompanyAddress = model.CompanyAddress;
                // handle avatar
                if (model.Avatar != null)
                {
                    string fileName = model.Avatar.FileName.Replace(Path.GetFileNameWithoutExtension(model.Avatar.FileName), requester.Id);
                    string filePath = Path.Combine(Server.MapPath("~/Uploads/Avatar"), fileName);
                    model.Avatar.SaveAs(filePath);
                    requester.AvatarURL = fileName;
                }
                _userService.EditUser(requester);

                return RedirectToAction("Requester");
            }
            ViewBag.id = id;
            ViewBag.username = requester.UserName;
            ViewBag.AvatarURL = requester.AvatarURL;
            return View(model);
        }

        [HttpPost]
        public ActionResult ToggleStatus()
        {
            string id = Request["id"];
            try
            {
                bool isEnable = _userService.ToggleStatus(id);
                var message = "";
                if (isEnable)
                {
                    message = "Enable requester successfully!";
                }
                else
                {
                    message = "Disable requester successfully!";
                }
                return Json(new
                {
                    success = true,
                    message = message
                });
            }
            catch
            {
                return Json(new
                {
                    success = false,
                    message = "Some errors occured! Please try again later!"
                });
            }
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
            }
        }

    }
}