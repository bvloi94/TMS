using log4net;
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
using TMS.ViewModels;

namespace TMS.Areas.Admin.Controllers
{
    public class ManageUserController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private UnitOfWork _unitOfWork;
        private UserService _userService;
        private DepartmentService _departmentService;
        private ILog log = LogManager.GetLogger(typeof(EmailUtil));

        public ManageUserController()
        {
            _unitOfWork = new UnitOfWork();
            _userService = new UserService(_unitOfWork);
            _departmentService = new DepartmentService(_unitOfWork);
        }

        public ManageUserController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            _unitOfWork = new UnitOfWork();
            _userService = new UserService(_unitOfWork);
            _departmentService = new DepartmentService(_unitOfWork);
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
            return View();
        }

        // GET: Admin/ManageUser/HelpDesk
        public ActionResult HelpDesk()
        {
            return View();
        }

        public ActionResult Technician()
        {
            return View();
        }

        public ActionResult Admin()
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
                (p.Birthday == null) ? "-" : ((DateTime) p.Birthday).ToString("dd/MM/yyyy"),
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
                (p.Birthday == null) ? "-" : ((DateTime) p.Birthday).ToString("dd/MM/yyyy"),
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
        public ActionResult GetTechnicians(jQueryDataTableParamModel param)
        {
            var technicians = _userService.GetTechnicians();
            var default_search_key = Request["search[value]"];
            var availability_select = Request["availability_select"];
            var search_text = Request["search_text"];
            IEnumerable<AspNetUser> filteredListItems;

            if (!string.IsNullOrEmpty(default_search_key))
            {
                filteredListItems = technicians.Where(p => p.Fullname.ToLower().Contains(default_search_key.ToLower()));
            }
            else
            {
                filteredListItems = technicians;
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
                (p.Birthday == null) ? "-" : ((DateTime) p.Birthday).ToString("dd/MM/yyyy"),
                p.IsActive,
                _departmentService.GetDepartmentById((int) p.DepartmentID).Name
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
        public ActionResult GetAdmins(jQueryDataTableParamModel param)
        {
            var admins = _userService.GetAdmins();
            var default_search_key = Request["search[value]"];
            var availability_select = Request["availability_select"];
            var search_text = Request["search_text"];
            IEnumerable<AspNetUser> filteredListItems;

            if (!string.IsNullOrEmpty(default_search_key))
            {
                filteredListItems = admins.Where(p => p.Fullname.ToLower().Contains(default_search_key.ToLower()));
            }
            else
            {
                filteredListItems = admins;
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
                (p.Birthday == null) ? "-" : ((DateTime) p.Birthday).ToString("dd/MM/yyyy"),
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

        // GET: Admin/ManageUser/CreateHelpDesk
        public ActionResult CreateHelpDesk()
        {
            return View();
        }

        // GET: Admin/ManageUser/CreateTechnician
        public ActionResult CreateTechnician()
        {
            ViewBag.departmentList = new SelectList(_departmentService.GetAll(), "ID", "Name");
            return View();
        }

        // GET: Admin/ManageUser/CreateAdmin
        [HttpGet]
        public ActionResult CreateAdmin()
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
                //string generatedPassword = GeneralUtil.GeneratePassword();
                string generatedPassword = "123456";
                var result = await UserManager.CreateAsync(user, generatedPassword);
                if (result.Succeeded)
                {
                    AspNetUser requester = _userService.GetUserById(user.Id);
                    requester.Fullname = model.Fullname;
                    requester.PhoneNumber = model.PhoneNumber;
                    requester.Birthday = model.Birthday;
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
                        requester.AvatarURL = "/Uploads/Avatar/" + fileName;
                    }
                    else
                    {
                        requester.AvatarURL = "/Uploads/Avatar/avatar_male.png";
                        if (requester.Gender != null)
                        {
                            if (requester.Gender == false)
                            {
                                requester.AvatarURL = "/Uploads/Avatar/avatar_female.png";
                            }
                        }
                    }
                    try
                    {
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

                        //bool sendMailResult = await EmailUtil.SendToUserWhenCreate(model.Username, generatedPassword, model.Fullname, model.Email);
                        //if (!sendMailResult)
                        //{
                        //    log.Debug("Send email unsuccessfully!");
                        //}
                        // Send email asynchronously
                        Thread thread = new Thread(() => EmailUtil.SendToUserWhenCreate(model.Username, generatedPassword, model.Fullname, model.Email));
                        thread.Start();

                        // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                        // Send an email with this link
                        // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                        // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                        // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");
                        Response.Cookies.Add(new HttpCookie("FlashMessage", "Create Requester account successfully!") { Path = "/" });
                        Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "success") { Path = "/" });
                        return RedirectToAction("Requester", "ManageUser");
                    }
                    catch
                    {
                        Response.Cookies.Add(new HttpCookie("FlashMessage", "Create Requester account unsuccessfully!") { Path = "/" });
                        Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "error") { Path = "/" });
                        return RedirectToAction("Requester", "ManageUser");
                    }
                }
                AddErrors(result);
            }

            return View(model);
        }

        // POST: Admin/ManageUser/CreateHelpDesk
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateHelpDesk(HelpDeskRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Username, Email = model.Email };
                //string generatedPassword = GeneralUtil.GeneratePassword();
                string generatedPassword = "123456";
                var result = await UserManager.CreateAsync(user, generatedPassword);
                if (result.Succeeded)
                {
                    AspNetUser helpdesk = _userService.GetUserById(user.Id);
                    helpdesk.Fullname = model.Fullname;
                    helpdesk.PhoneNumber = model.PhoneNumber;
                    helpdesk.Birthday = model.Birthday;
                    helpdesk.Address = model.Address;
                    helpdesk.Gender = model.Gender;
                    helpdesk.IsActive = true;
                    // handle avatar
                    if (model.Avatar != null)
                    {
                        string fileName = model.Avatar.FileName.Replace(Path.GetFileNameWithoutExtension(model.Avatar.FileName), user.Id);
                        string filePath = Path.Combine(Server.MapPath("~/Uploads/Avatar"), fileName);
                        model.Avatar.SaveAs(filePath);
                        helpdesk.AvatarURL = "/Uploads/Avatar/" + fileName;
                    }
                    else
                    {
                        helpdesk.AvatarURL = "/Uploads/Avatar/avatar_male.png";
                        if (helpdesk.Gender != null)
                        {
                            if (helpdesk.Gender == false)
                            {
                                helpdesk.AvatarURL = "/Uploads/Avatar/avatar_female.png";
                            }
                        }
                    }

                    try
                    {
                        _userService.EditUser(helpdesk);

                        ApplicationDbContext context = new ApplicationDbContext();

                        var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                        if (!roleManager.RoleExists("Helpdesk"))
                        {
                            var role = new IdentityRole();
                            role.Name = "Helpdesk";
                            roleManager.Create(role);
                        }
                        UserManager.AddToRole(user.Id, "Helpdesk");
                        //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        //bool sendMailResult = await EmailUtil.SendToUserWhenCreate(model.Username, generatedPassword, model.Fullname, model.Email);
                        //if (!sendMailResult)
                        //{
                        //    log.Debug("Send email unsuccessfully!");
                        //}
                        // Send email asynchronously
                        Thread thread = new Thread(() => EmailUtil.SendToUserWhenCreate(model.Username, generatedPassword, model.Fullname, model.Email));
                        thread.Start();

                        // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                        // Send an email with this link
                        // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                        // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                        // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");
                        Response.Cookies.Add(new HttpCookie("FlashMessage", "Create Help Desk account successfully!") { Path = "/" });
                        Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "success") { Path = "/" });
                        return RedirectToAction("HelpDesk", "ManageUser");
                    }
                    catch
                    {
                        Response.Cookies.Add(new HttpCookie("FlashMessage", "Create Help Desk account unsuccessfully!") { Path = "/" });
                        Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "error") { Path = "/" });
                        return RedirectToAction("HelpDesk", "ManageUser");
                    }
                }
                AddErrors(result);
            }

            return View(model);
        }

        // POST: Admin/ManageUser/CreateTechnician
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateTechnician(TechnicianRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Username, Email = model.Email };
                //string generatedPassword = GeneralUtil.GeneratePassword();
                string generatedPassword = "123456";
                var result = await UserManager.CreateAsync(user, generatedPassword);
                if (result.Succeeded)
                {
                    AspNetUser technician = _userService.GetUserById(user.Id);
                    technician.Fullname = model.Fullname;
                    technician.PhoneNumber = model.PhoneNumber;
                    technician.Birthday = model.Birthday;
                    technician.Address = model.Address;
                    technician.Gender = model.Gender;
                    technician.DepartmentID = model.DepartmentID;
                    technician.IsActive = true;
                    // handle avatar
                    if (model.Avatar != null)
                    {
                        string fileName = model.Avatar.FileName.Replace(Path.GetFileNameWithoutExtension(model.Avatar.FileName), user.Id);
                        string filePath = Path.Combine(Server.MapPath("~/Uploads/Avatar"), fileName);
                        model.Avatar.SaveAs(filePath);
                        technician.AvatarURL = "/Uploads/Avatar/" + fileName;
                    }
                    else
                    {
                        technician.AvatarURL = "/Uploads/Avatar/avatar_male.png";
                        if (technician.Gender != null)
                        {
                            if (technician.Gender == false)
                            {
                                technician.AvatarURL = "/Uploads/Avatar/avatar_female.png";
                            }
                        }
                    }
                    try
                    {
                        _userService.EditUser(technician);

                        ApplicationDbContext context = new ApplicationDbContext();

                        var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                        if (!roleManager.RoleExists("Technician"))
                        {
                            var role = new IdentityRole();
                            role.Name = "Technician";
                            roleManager.Create(role);
                        }
                        UserManager.AddToRole(user.Id, "Technician");
                        //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        //bool sendMailResult = await EmailUtil.SendToUserWhenCreate(model.Username, generatedPassword, model.Fullname, model.Email);
                        //if (!sendMailResult)
                        //{
                        //    log.Debug("Send email unsuccessfully!");
                        //}
                        // Send email asynchronously
                        Thread thread = new Thread(() => EmailUtil.SendToUserWhenCreate(model.Username, generatedPassword, model.Fullname, model.Email));
                        thread.Start();

                        // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                        // Send an email with this link
                        // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                        // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                        // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");
                        Response.Cookies.Add(new HttpCookie("FlashMessage", "Create Technician account successfully!") { Path = "/" });
                        Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "success") { Path = "/" });
                        return RedirectToAction("Technician", "ManageUser");
                    }
                    catch
                    {
                        Response.Cookies.Add(new HttpCookie("FlashMessage", "Create Technician account unsuccessfully!") { Path = "/" });
                        Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "error") { Path = "/" });
                        return RedirectToAction("Technician", "ManageUser");
                    }
                }
                AddErrors(result);
            }

            ViewBag.departmentList = new SelectList(_departmentService.GetAll(), "ID", "Name");
            return View(model);
        }

        // POST: Admin/ManageUser/CreateAdmin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateAdmin(AdminRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Username, Email = model.Email };
                //string generatedPassword = GeneralUtil.GeneratePassword();
                string generatedPassword = "123456";
                var result = await UserManager.CreateAsync(user, generatedPassword);
                if (result.Succeeded)
                {
                    AspNetUser admin = _userService.GetUserById(user.Id);
                    admin.Fullname = model.Fullname;
                    admin.PhoneNumber = model.PhoneNumber;
                    admin.Birthday = model.Birthday;
                    admin.Address = model.Address;
                    admin.Gender = model.Gender;
                    admin.IsActive = true;
                    // handle avatar
                    if (model.Avatar != null)
                    {
                        string fileName = model.Avatar.FileName.Replace(Path.GetFileNameWithoutExtension(model.Avatar.FileName), user.Id);
                        string filePath = Path.Combine(Server.MapPath("~/Uploads/Avatar"), fileName);
                        model.Avatar.SaveAs(filePath);
                        admin.AvatarURL = "/Uploads/Avatar/" + fileName;
                    }
                    else
                    {
                        admin.AvatarURL = "/Uploads/Avatar/avatar_male.png";
                        if (admin.Gender != null)
                        {
                            if (admin.Gender == false)
                            {
                                admin.AvatarURL = "/Uploads/Avatar/avatar_female.png";
                            }
                        }
                    }
                    try
                    {
                        _userService.EditUser(admin);

                        ApplicationDbContext context = new ApplicationDbContext();

                        var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                        if (!roleManager.RoleExists("Admin"))
                        {
                            var role = new IdentityRole();
                            role.Name = "Admin";
                            roleManager.Create(role);
                        }
                        UserManager.AddToRole(user.Id, "Admin");
                        //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        //bool sendMailResult = await EmailUtil.SendToUserWhenCreate(model.Username, generatedPassword, model.Fullname, model.Email);
                        //if (!sendMailResult)
                        //{
                        //    log.Debug("Send email unsuccessfully!");
                        //}
                        // Send email asynchronously
                        Thread thread = new Thread(() => EmailUtil.SendToUserWhenCreate(model.Username, generatedPassword, model.Fullname, model.Email));
                        thread.Start();

                        // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                        // Send an email with this link
                        // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                        // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                        // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");
                        Response.Cookies.Add(new HttpCookie("FlashMessage", "Create Admin account successfully!") { Path = "/" });
                        Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "success") { Path = "/" });
                        return RedirectToAction("Admin", "ManageUser");
                    }
                    catch
                    {
                        Response.Cookies.Add(new HttpCookie("FlashMessage", "Create Admin account unsuccessfully!") { Path = "/" });
                        Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "error") { Path = "/" });
                        return RedirectToAction("Admin", "ManageUser");
                    }
                }
                AddErrors(result);
            }

            return View(model);
        }

        // GET: Admin/ManageUser/EditRequester/{id}
        public ActionResult EditRequester(string id)
        {
            AspNetUser requester = _userService.GetUserById(id);
            if (requester != null)
            {
                RequesterRegisterViewModel model = new RequesterRegisterViewModel();
                model.Fullname = requester.Fullname;
                model.PhoneNumber = requester.PhoneNumber;
                model.Email = requester.Email;
                model.Birthday = requester.Birthday;
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
            else
            {
                Response.Cookies.Add(new HttpCookie("FlashMessage", "This requester is not available!") { Path = "/" });
                Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "error") { Path = "/" });
                return RedirectToAction("Requester", "ManageUser");
            }
        }

        // GET: Admin/ManageUser/EditHelpDesk/{id}
        public ActionResult EditHelpDesk(string id)
        {
            AspNetUser helpdesk = _userService.GetUserById(id);
            if (helpdesk != null)
            {
                HelpDeskRegisterViewModel model = new HelpDeskRegisterViewModel();
                model.Fullname = helpdesk.Fullname;
                model.PhoneNumber = helpdesk.PhoneNumber;
                model.Email = helpdesk.Email;
                model.Birthday = helpdesk.Birthday;
                model.Address = helpdesk.Address;
                model.Gender = helpdesk.Gender;

                ViewBag.id = id;
                ViewBag.username = helpdesk.UserName;
                ViewBag.AvatarURL = helpdesk.AvatarURL;
                return View(model);
            }
            else
            {
                Response.Cookies.Add(new HttpCookie("FlashMessage", "This help desk is not available!") { Path = "/" });
                Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "error") { Path = "/" });
                return RedirectToAction("HelpDesk", "ManageUser");
            }
        }

        // GET: Admin/ManageUser/EditTechnician/{id}
        public ActionResult EditTechnician(string id)
        {
            AspNetUser technician = _userService.GetUserById(id);
            if (technician != null)
            {
                TechnicianRegisterViewModel model = new TechnicianRegisterViewModel();
                model.Fullname = technician.Fullname;
                model.PhoneNumber = technician.PhoneNumber;
                model.Email = technician.Email;
                model.Birthday = technician.Birthday;
                model.Address = technician.Address;
                model.Gender = technician.Gender;
                model.DepartmentID = technician.DepartmentID;

                ViewBag.id = id;
                ViewBag.username = technician.UserName;
                ViewBag.AvatarURL = technician.AvatarURL;
                ViewBag.departmentList = new SelectList(_departmentService.GetAll(), "ID", "Name");
                return View(model);
            }
            else
            {
                Response.Cookies.Add(new HttpCookie("FlashMessage", "This technician is not available!") { Path = "/" });
                Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "error") { Path = "/" });
                return RedirectToAction("Technician", "ManageUser");
            }
        }

        // GET: Admin/ManageUser/EditAdmin/{id}
        public ActionResult EditAdmin(string id)
        {
            AspNetUser admin = _userService.GetUserById(id);
            if (admin != null)
            {
                AdminRegisterViewModel model = new AdminRegisterViewModel();
                model.Fullname = admin.Fullname;
                model.PhoneNumber = admin.PhoneNumber;
                model.Email = admin.Email;
                model.Birthday = admin.Birthday;
                model.Address = admin.Address;
                model.Gender = admin.Gender;

                ViewBag.id = id;
                ViewBag.username = admin.UserName;
                ViewBag.AvatarURL = admin.AvatarURL;
                ViewBag.departmentList = new SelectList(_departmentService.GetAll(), "ID", "Name");
                return View(model);
            }
            else
            {
                Response.Cookies.Add(new HttpCookie("FlashMessage", "This technician is not available!") { Path = "/" });
                Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "error") { Path = "/" });
                return RedirectToAction("Admin", "ManageUser");
            }
        }

        // POST: Admin/ManageUser/EditRequester/{id}
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
                requester.PhoneNumber = model.PhoneNumber;
                requester.Email = model.Email;
                requester.Birthday = model.Birthday;
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
                    requester.AvatarURL = "/Uploads/Avatar/" + fileName;
                }
                try
                {
                    _userService.EditUser(requester);
                    Response.Cookies.Add(new HttpCookie("FlashMessage", "Edit Requester account successfully!") { Path = "/" });
                    Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "success") { Path = "/" });
                }
                catch
                {
                    Response.Cookies.Add(new HttpCookie("FlashMessage", "Edit Requester account unsuccessfully!") { Path = "/" });
                    Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "error") { Path = "/" });
                }
                return RedirectToAction("Requester");
            }
            ViewBag.id = id;
            ViewBag.username = requester.UserName;
            ViewBag.AvatarURL = requester.AvatarURL;
            return View(model);
        }

        // POST: Admin/ManageUser/EditHelpDesk/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditHelpDesk(HelpDeskRegisterViewModel model, string id)
        {
            ModelState.Remove("UserName");
            ModelState.Remove("Password");
            if (_userService.IsDuplicatedEmail(id, model.Email))
            {
                ModelState.AddModelError("Email", String.Format("Email '{0}' is already taken.", model.Email));
            }

            AspNetUser helpdesk = _userService.GetUserById(id);
            if (ModelState.IsValid)
            {
                helpdesk.Fullname = model.Fullname;
                helpdesk.PhoneNumber = model.PhoneNumber;
                helpdesk.Email = model.Email;
                helpdesk.Birthday = model.Birthday;
                helpdesk.Address = model.Address;
                helpdesk.Gender = model.Gender;
                // handle avatar
                if (model.Avatar != null)
                {
                    string fileName = model.Avatar.FileName.Replace(Path.GetFileNameWithoutExtension(model.Avatar.FileName), helpdesk.Id);
                    string filePath = Path.Combine(Server.MapPath("~/Uploads/Avatar"), fileName);
                    model.Avatar.SaveAs(filePath);
                    helpdesk.AvatarURL = "/Uploads/Avatar/" + fileName;
                }
                try
                {
                    _userService.EditUser(helpdesk);
                    Response.Cookies.Add(new HttpCookie("FlashMessage", "Edit Help Desk account successfully!") { Path = "/" });
                    Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "success") { Path = "/" });
                }
                catch
                {
                    Response.Cookies.Add(new HttpCookie("FlashMessage", "Edit Help Desk account unsuccessfully!") { Path = "/" });
                    Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "error") { Path = "/" });
                }
                return RedirectToAction("HelpDesk");
            }
            ViewBag.id = id;
            ViewBag.username = helpdesk.UserName;
            ViewBag.AvatarURL = helpdesk.AvatarURL;
            return View(model);
        }

        // POST: Admin/ManageUser/EditTechnician/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditTechnician(TechnicianRegisterViewModel model, string id)
        {
            ModelState.Remove("UserName");
            ModelState.Remove("Password");
            if (_userService.IsDuplicatedEmail(id, model.Email))
            {
                ModelState.AddModelError("Email", String.Format("Email '{0}' is already taken.", model.Email));
            }

            AspNetUser technician = _userService.GetUserById(id);
            if (ModelState.IsValid)
            {
                technician.Fullname = model.Fullname;
                technician.PhoneNumber = model.PhoneNumber;
                technician.Email = model.Email;
                technician.Birthday = model.Birthday;
                technician.Address = model.Address;
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
                try
                {
                    _userService.EditUser(technician);
                    Response.Cookies.Add(new HttpCookie("FlashMessage", "Edit Technician account successfully!") { Path = "/" });
                    Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "success") { Path = "/" });
                }
                catch
                {
                    Response.Cookies.Add(new HttpCookie("FlashMessage", "Edit Technician account unsuccessfully!") { Path = "/" });
                    Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "error") { Path = "/" });
                }
                return RedirectToAction("Technician");
            }
            ViewBag.id = id;
            ViewBag.username = technician.UserName;
            ViewBag.AvatarURL = technician.AvatarURL;
            ViewBag.departmentList = new SelectList(_departmentService.GetAll(), "ID", "Name");
            return View(model);
        }

        // POST: Admin/ManageUser/EditAdmin/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditAdmin(AdminRegisterViewModel model, string id)
        {
            ModelState.Remove("UserName");
            ModelState.Remove("Password");
            if (_userService.IsDuplicatedEmail(id, model.Email))
            {
                ModelState.AddModelError("Email", String.Format("Email '{0}' is already taken.", model.Email));
            }

            AspNetUser admin = _userService.GetUserById(id);
            if (ModelState.IsValid)
            {
                admin.Fullname = model.Fullname;
                admin.PhoneNumber = model.PhoneNumber;
                admin.Email = model.Email;
                admin.Birthday = model.Birthday;
                admin.Address = model.Address;
                admin.Gender = model.Gender;
                // handle avatar
                if (model.Avatar != null)
                {
                    string fileName = model.Avatar.FileName.Replace(Path.GetFileNameWithoutExtension(model.Avatar.FileName), admin.Id);
                    string filePath = Path.Combine(Server.MapPath("~/Uploads/Avatar"), fileName);
                    model.Avatar.SaveAs(filePath);
                    admin.AvatarURL = "/Uploads/Avatar/" + fileName;
                }
                try
                {
                    _userService.EditUser(admin);
                    Response.Cookies.Add(new HttpCookie("FlashMessage", "Edit Admin account successfully!") { Path = "/" });
                    Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "success") { Path = "/" });
                }
                catch
                {
                    Response.Cookies.Add(new HttpCookie("FlashMessage", "Edit Admin account unsuccessfully!") { Path = "/" });
                    Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "error") { Path = "/" });
                }
                return RedirectToAction("Admin");
            }
            ViewBag.id = id;
            ViewBag.username = admin.UserName;
            ViewBag.AvatarURL = admin.AvatarURL;
            return View(model);
        }

        [HttpPost]
        public ActionResult ToggleStatus(string id)
        {
            try
            {
                AspNetUser user = _userService.GetUserById(id);
                if (user != null)
                {
                    bool isEnable = _userService.ToggleStatus(user);
                    var message = "";
                    if (isEnable)
                    {
                        message = "Enable user successfully!";
                    }
                    else
                    {
                        message = "Disable user successfully!";
                    }
                    return Json(new
                    {
                        success = true,
                        message = message
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "This user is unavailable!"
                    });
                }
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

        [HttpPost]
        public async Task<ActionResult> ResendEmail(string id)
        {
            AspNetUser user = _userService.GetUserById(id);
            if (user == null)
            {
                return Json(new
                {
                    success = false,
                    message = "User is not found"
                });
            }
            string generatedPassword = GeneralUtil.GeneratePassword();
            // Send email asynchronously
            bool sendEmailResult = await EmailUtil.ResendToUserWhenCreate(user.UserName, generatedPassword, user.Fullname, user.Email);
            if (sendEmailResult)
            {
                return Json(new
                {
                    success = true,
                    message = "Resend email successfully!"
                });
            }
            else
            {
                return Json(new
                {
                    success = false,
                    message = "Resend email unsuccessfully!"
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