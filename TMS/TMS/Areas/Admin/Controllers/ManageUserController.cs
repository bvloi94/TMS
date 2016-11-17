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
    [CustomAuthorize(Roles = "Admin")]
    public class ManageUserController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private UnitOfWork _unitOfWork;
        private UserService _userService;
        private GroupService _groupService;
        private ILog log = LogManager.GetLogger(typeof(EmailUtil));

        public ManageUserController()
        {
            _unitOfWork = new UnitOfWork();
            _userService = new UserService(_unitOfWork);
            _groupService = new GroupService(_unitOfWork);
        }

        public ManageUserController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            _unitOfWork = new UnitOfWork();
            _userService = new UserService(_unitOfWork);
            _groupService = new GroupService(_unitOfWork);
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

        public ActionResult Manager()
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
            var sortColumnIndex = TMSUtils.StrToIntDef(Request["order[0][column]"], 0);
            var sortDirection = Request["order[0][dir]"];

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
            var sortColumnIndex = TMSUtils.StrToIntDef(Request["order[0][column]"], 0);
            var sortDirection = Request["order[0][dir]"];

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
            var sortColumnIndex = TMSUtils.StrToIntDef(Request["order[0][column]"], 0);
            var sortDirection = Request["order[0][dir]"];

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
                _groupService.GetGroupById((int) p.GroupID).Name
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
            string currentUserId = User.Identity.GetUserId();
            var admins = _userService.GetAdmins(currentUserId);
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
            var sortColumnIndex = TMSUtils.StrToIntDef(Request["order[0][column]"], 0);
            var sortDirection = Request["order[0][dir]"];

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

        [HttpGet]
        public ActionResult GetManagers(jQueryDataTableParamModel param)
        {
            var managers = _userService.GetManagers();
            var default_search_key = Request["search[value]"];
            var availability_select = Request["availability_select"];
            var search_text = Request["search_text"];
            IEnumerable<AspNetUser> filteredListItems = managers;

            if (!string.IsNullOrEmpty(default_search_key))
            {
                filteredListItems = filteredListItems.Where(p => p.Fullname.ToLower().Contains(default_search_key.ToLower()));
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
            var sortColumnIndex = TMSUtils.StrToIntDef(Request["order[0][column]"], 0);
            var sortDirection = Request["order[0][dir]"];

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
            ViewBag.groupList = new SelectList(_groupService.GetAll(), "ID", "Name");
            return View();
        }

        // GET: Admin/ManageUser/CreateAdmin
        [HttpGet]
        public ActionResult CreateAdmin()
        {
            return View();
        }

        // GET: Admin/ManageUser/CreateManager
        [HttpGet]
        public ActionResult CreateManager()
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

                    bool editResult = _userService.EditUser(requester);
                    if (editResult)
                    {
                        ApplicationDbContext context = new ApplicationDbContext();

                        var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                        if (!roleManager.RoleExists("Requester"))
                        {
                            var role = new IdentityRole();
                            role.Name = "Requester";
                            roleManager.Create(role);
                        }
                        UserManager.AddToRole(user.Id, "Requester");

                        Thread thread = new Thread(() => EmailUtil.SendToUserWhenCreate(model.Username, generatedPassword, model.Fullname, model.Email));
                        thread.Start();

                        Response.Cookies.Add(new HttpCookie("FlashMessage", "Create Requester account successfully!") { Path = "/" });
                        Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "success") { Path = "/" });
                        return RedirectToAction("Requester", "ManageUser");
                    }
                    else
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
                string generatedPassword = GeneralUtil.GeneratePassword();
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

                    bool editResult = _userService.EditUser(helpdesk);
                    if (editResult)
                    {
                        ApplicationDbContext context = new ApplicationDbContext();

                        var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                        if (!roleManager.RoleExists("Helpdesk"))
                        {
                            var role = new IdentityRole();
                            role.Name = "Helpdesk";
                            roleManager.Create(role);
                        }
                        UserManager.AddToRole(user.Id, "Helpdesk");

                        Thread thread = new Thread(() => EmailUtil.SendToUserWhenCreate(model.Username, generatedPassword, model.Fullname, model.Email));
                        thread.Start();

                        Response.Cookies.Add(new HttpCookie("FlashMessage", "Create Help Desk account successfully!") { Path = "/" });
                        Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "success") { Path = "/" });
                        return RedirectToAction("HelpDesk", "ManageUser");
                    }
                    else
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
                string generatedPassword = GeneralUtil.GeneratePassword();
                var result = await UserManager.CreateAsync(user, generatedPassword);
                if (result.Succeeded)
                {
                    AspNetUser technician = _userService.GetUserById(user.Id);
                    technician.Fullname = model.Fullname;
                    technician.PhoneNumber = model.PhoneNumber;
                    technician.Birthday = model.Birthday;
                    technician.Address = model.Address;
                    technician.Gender = model.Gender;
                    technician.GroupID = model.GroupID;
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

                    bool editResult = _userService.EditUser(technician);
                    if (editResult)
                    {
                        ApplicationDbContext context = new ApplicationDbContext();

                        var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                        if (!roleManager.RoleExists("Technician"))
                        {
                            var role = new IdentityRole();
                            role.Name = "Technician";
                            roleManager.Create(role);
                        }
                        UserManager.AddToRole(user.Id, "Technician");

                        Thread thread = new Thread(() => EmailUtil.SendToUserWhenCreate(model.Username, generatedPassword, model.Fullname, model.Email));
                        thread.Start();

                        Response.Cookies.Add(new HttpCookie("FlashMessage", "Create Technician account successfully!") { Path = "/" });
                        Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "success") { Path = "/" });
                        return RedirectToAction("Technician", "ManageUser");
                    }
                    else
                    {
                        Response.Cookies.Add(new HttpCookie("FlashMessage", "Create Technician account unsuccessfully!") { Path = "/" });
                        Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "error") { Path = "/" });
                        return RedirectToAction("Technician", "ManageUser");
                    }
                }
                AddErrors(result);
            }

            ViewBag.groupList = new SelectList(_groupService.GetAll(), "ID", "Name");
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
                string generatedPassword = GeneralUtil.GeneratePassword();
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

                    bool editResult = _userService.EditUser(admin);
                    if (editResult)
                    {
                        ApplicationDbContext context = new ApplicationDbContext();

                        var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                        if (!roleManager.RoleExists("Admin"))
                        {
                            var role = new IdentityRole();
                            role.Name = "Admin";
                            roleManager.Create(role);
                        }
                        UserManager.AddToRole(user.Id, "Admin");

                        Thread thread = new Thread(() => EmailUtil.SendToUserWhenCreate(model.Username, generatedPassword, model.Fullname, model.Email));
                        thread.Start();

                        Response.Cookies.Add(new HttpCookie("FlashMessage", "Create Admin account successfully!") { Path = "/" });
                        Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "success") { Path = "/" });
                        return RedirectToAction("Admin", "ManageUser");
                    }
                    else
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

        // POST: Admin/ManageUser/CreateManager
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateManager(ManagerRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Username, Email = model.Email };
                string generatedPassword = GeneralUtil.GeneratePassword();
                var result = await UserManager.CreateAsync(user, generatedPassword);
                if (result.Succeeded)
                {
                    AspNetUser manager = _userService.GetUserById(user.Id);
                    manager.Fullname = model.Fullname;
                    manager.PhoneNumber = model.PhoneNumber;
                    manager.Birthday = model.Birthday;
                    manager.Address = model.Address;
                    manager.Gender = model.Gender;
                    manager.IsActive = true;
                    // handle avatar
                    if (model.Avatar != null)
                    {
                        string fileName = model.Avatar.FileName.Replace(Path.GetFileNameWithoutExtension(model.Avatar.FileName), user.Id);
                        string filePath = Path.Combine(Server.MapPath("~/Uploads/Avatar"), fileName);
                        model.Avatar.SaveAs(filePath);
                        manager.AvatarURL = "/Uploads/Avatar/" + fileName;
                    }
                    else
                    {
                        manager.AvatarURL = "/Uploads/Avatar/avatar_male.png";
                        if (manager.Gender != null)
                        {
                            if (manager.Gender == false)
                            {
                                manager.AvatarURL = "/Uploads/Avatar/avatar_female.png";
                            }
                        }
                    }

                    bool editResult = _userService.EditUser(manager);
                    if (editResult)
                    {
                        ApplicationDbContext context = new ApplicationDbContext();

                        var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                        if (!roleManager.RoleExists(ConstantUtil.UserRoleString.Manager))
                        {
                            var role = new IdentityRole();
                            role.Name = ConstantUtil.UserRoleString.Manager;
                            roleManager.Create(role);
                        }
                        UserManager.AddToRole(user.Id, ConstantUtil.UserRoleString.Manager);
                        // Send email asynchronously
                        Thread thread = new Thread(() => EmailUtil.SendToUserWhenCreate(model.Username, generatedPassword, model.Fullname, model.Email));
                        thread.Start();

                        Response.Cookies.Add(new HttpCookie("FlashMessage", "Create Manager account successfully!") { Path = "/" });
                        Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "success") { Path = "/" });
                        return RedirectToAction("Manager", "ManageUser");
                    }
                    else
                    {
                        Response.Cookies.Add(new HttpCookie("FlashMessage", "Create Manager account unsuccessfully!") { Path = "/" });
                        Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "error") { Path = "/" });
                        return RedirectToAction("Manager", "ManageUser");
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
                return HttpNotFound();
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
                return HttpNotFound();
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
                model.GroupID = technician.GroupID;

                ViewBag.id = id;
                ViewBag.username = technician.UserName;
                ViewBag.AvatarURL = technician.AvatarURL;
                ViewBag.groupList = new SelectList(_groupService.GetAll(), "ID", "Name");
                return View(model);
            }
            else
            {
                return HttpNotFound();
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
                return View(model);
            }
            else
            {
                return HttpNotFound();
            }
        }

        // GET: Admin/ManageUser/EditManager/{id}
        public ActionResult EditManager(string id)
        {
            AspNetUser manager = _userService.GetUserById(id);
            if (manager != null)
            {
                ManagerRegisterViewModel model = new ManagerRegisterViewModel();
                model.Fullname = manager.Fullname;
                model.PhoneNumber = manager.PhoneNumber;
                model.Email = manager.Email;
                model.Birthday = manager.Birthday;
                model.Address = manager.Address;
                model.Gender = manager.Gender;

                ViewBag.id = id;
                ViewBag.username = manager.UserName;
                ViewBag.AvatarURL = manager.AvatarURL;
                return View(model);
            }
            else
            {
                return HttpNotFound();
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
            if (requester != null)
            {
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

                    bool editResult = _userService.EditUser(requester);
                    if (editResult)
                    {
                        Response.Cookies.Add(new HttpCookie("FlashMessage", "Edit Requester account successfully!") { Path = "/" });
                        Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "success") { Path = "/" });
                    }
                    else
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
            else
            {
                return HttpNotFound();
            }
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
            if (helpdesk != null)
            {
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

                    bool editResult = _userService.EditUser(helpdesk);
                    if (editResult)
                    {
                        Response.Cookies.Add(new HttpCookie("FlashMessage", "Edit Help Desk account successfully!") { Path = "/" });
                        Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "success") { Path = "/" });
                    }
                    else
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
            else
            {
                return HttpNotFound();
            }
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
            if (technician != null)
            {
                if (ModelState.IsValid)
                {
                    technician.Fullname = model.Fullname;
                    technician.PhoneNumber = model.PhoneNumber;
                    technician.Email = model.Email;
                    technician.Birthday = model.Birthday;
                    technician.Address = model.Address;
                    technician.Gender = model.Gender;
                    technician.GroupID = model.GroupID;
                    // handle avatar
                    if (model.Avatar != null)
                    {
                        string fileName = model.Avatar.FileName.Replace(Path.GetFileNameWithoutExtension(model.Avatar.FileName), technician.Id);
                        string filePath = Path.Combine(Server.MapPath("~/Uploads/Avatar"), fileName);
                        model.Avatar.SaveAs(filePath);
                        technician.AvatarURL = "/Uploads/Avatar/" + fileName;
                    }

                    bool editResult = _userService.EditUser(technician);
                    if (editResult)
                    {
                        Response.Cookies.Add(new HttpCookie("FlashMessage", "Edit Technician account successfully!") { Path = "/" });
                        Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "success") { Path = "/" });
                    }
                    else
                    {
                        Response.Cookies.Add(new HttpCookie("FlashMessage", "Edit Technician account unsuccessfully!") { Path = "/" });
                        Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "error") { Path = "/" });
                    }
                    return RedirectToAction("Technician");
                }
                ViewBag.id = id;
                ViewBag.username = technician.UserName;
                ViewBag.AvatarURL = technician.AvatarURL;
                ViewBag.groupList = new SelectList(_groupService.GetAll(), "ID", "Name");
                return View(model);
            }
            else
            {
                return HttpNotFound();
            }
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
            if (admin != null)
            {
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

                    bool editResult = _userService.EditUser(admin);
                    if (editResult)
                    {
                        Response.Cookies.Add(new HttpCookie("FlashMessage", "Edit Admin account successfully!") { Path = "/" });
                        Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "success") { Path = "/" });
                    }
                    else
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
            else
            {
                return HttpNotFound();
            }
        }

        // POST: Admin/ManageUser/EditManager/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditManager(ManagerRegisterViewModel model, string id)
        {
            ModelState.Remove("UserName");
            ModelState.Remove("Password");
            if (_userService.IsDuplicatedEmail(id, model.Email))
            {
                ModelState.AddModelError("Email", String.Format("Email '{0}' is already taken.", model.Email));
            }

            AspNetUser manager = _userService.GetUserById(id);
            if (manager != null)
            {
                if (ModelState.IsValid)
                {
                    manager.Fullname = model.Fullname;
                    manager.PhoneNumber = model.PhoneNumber;
                    manager.Email = model.Email;
                    manager.Birthday = model.Birthday;
                    manager.Address = model.Address;
                    manager.Gender = model.Gender;
                    // handle avatar
                    if (model.Avatar != null)
                    {
                        string fileName = model.Avatar.FileName.Replace(Path.GetFileNameWithoutExtension(model.Avatar.FileName), manager.Id);
                        string filePath = Path.Combine(Server.MapPath("~/Uploads/Avatar"), fileName);
                        model.Avatar.SaveAs(filePath);
                        manager.AvatarURL = "/Uploads/Avatar/" + fileName;
                    }

                    bool editResult = _userService.EditUser(manager);
                    if (editResult)
                    {
                        Response.Cookies.Add(new HttpCookie("FlashMessage", "Edit Manager account successfully!") { Path = "/" });
                        Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "success") { Path = "/" });
                    }
                    else
                    {
                        Response.Cookies.Add(new HttpCookie("FlashMessage", "Edit Manager account unsuccessfully!") { Path = "/" });
                        Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "error") { Path = "/" });
                    }
                    return RedirectToAction("Manager");
                }
                ViewBag.id = id;
                ViewBag.username = manager.UserName;
                ViewBag.AvatarURL = manager.AvatarURL;
                return View(model);
            }
            else
            {
                return HttpNotFound();
            }
        }

        [HttpPost]
        public ActionResult ToggleStatus(string id)
        {
            AspNetUser user = _userService.GetUserById(id);
            if (user != null)
            {
                bool? wasActive = user.IsActive;
                bool toggleResult = _userService.ToggleStatus(user);
                var message = "";
                if (toggleResult)
                {
                    if (wasActive.HasValue && wasActive == true)
                    {
                        message = "Disable user successfully!";
                    }
                    else
                    {
                        message = "Enable user successfully!";
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
                        message = ConstantUtil.CommonError.DBExceptionError
                    });
                }
            }
            else
            {
                return Json(new
                {
                    success = false,
                    message = ConstantUtil.CommonError.UnavailableUser
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
                    message = ConstantUtil.CommonError.UnavailableUser
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