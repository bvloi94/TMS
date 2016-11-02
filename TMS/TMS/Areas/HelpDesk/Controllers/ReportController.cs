using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TMS.DAL;
using TMS.Enumerator;
using TMS.Models;
using TMS.Services;
using TMS.Utils;

namespace TMS.Areas.HelpDesk.Controllers
{
    [CustomAuthorize(Roles = "Helpdesk")]
    public class ReportController : Controller
    {
        private TicketService _ticketService { get; set; }
        public UserService _userService { get; set; }
        public ImpactService _impactService { get; set; }
        public CategoryService _categoryService { get; set; }
        public UrgencyService _urgencyService { get; set; }
        public PriorityService _priorityService { get; set; }
        public DepartmentService _departmentService { get; set; }

        public ReportController()
        {
            var _UnitofWork = new UnitOfWork();
            _ticketService = new TicketService(_UnitofWork);
            _userService = new UserService(_UnitofWork);
            _impactService = new ImpactService(_UnitofWork);
            _categoryService = new CategoryService(_UnitofWork);
            _urgencyService = new UrgencyService(_UnitofWork);
            _priorityService = new PriorityService(_UnitofWork);
            _departmentService = new DepartmentService(_UnitofWork);
        }
        // GET: HelpDesk/Report
        public ActionResult Index()
        {
            return View();
        }

        // Helpdesk/report
        [HttpPost]
        public ActionResult GetTickets(int by, int type, DateTime? date_from_select, DateTime? date_to_select, jQueryDataTableParamModel param)
        {

            IEnumerable<Ticket> ticketsList = _ticketService.GetAll().OrderBy(m => m.CreatedTime);
            IEnumerable<Ticket> filteredListItems;
            filteredListItems = ticketsList;

            if (date_from_select.HasValue)
            {
                filteredListItems = filteredListItems.Where(p => p.CreatedTime.Date >= date_from_select.Value.Date);
            }
            if (date_to_select.HasValue)
            {
                filteredListItems = filteredListItems.Where(p => p.CreatedTime.Date <= date_to_select.Value.Date);
            }

            // Type
            switch (type)
            {
                // All Request 
                case ConstantUtil.TicketTypeValue.Request:
                    filteredListItems = filteredListItems.Where(p => p.Type == ConstantUtil.TicketType.Request);
                    break;
                // All Problems
                case ConstantUtil.TicketTypeValue.Problem:
                    filteredListItems = filteredListItems.Where(p => p.Type == ConstantUtil.TicketType.Problem);
                    break;
                // All Request
                case ConstantUtil.TicketTypeValue.Change:
                    filteredListItems = filteredListItems.Where(p => p.Type == ConstantUtil.TicketType.Change);
                    break;
                // All pending requests
                case ConstantUtil.TicketTypeValue.PendingRequest:
                    filteredListItems = filteredListItems.Where(p => p.Type == ConstantUtil.TicketType.Request);
                    filteredListItems = filteredListItems.Where(p => p.Status != ConstantUtil.TicketStatus.Cancelled
                        && p.Status != ConstantUtil.TicketStatus.Cancelled);
                    break;
                // All pending problems
                case ConstantUtil.TicketTypeValue.PendingProblem:
                    filteredListItems = filteredListItems.Where(p => p.Type == ConstantUtil.TicketType.Problem);
                    filteredListItems = filteredListItems.Where(p => p.Status != ConstantUtil.TicketStatus.Cancelled
                        && p.Status != ConstantUtil.TicketStatus.Cancelled);
                    break;
                // All pending changes
                case ConstantUtil.TicketTypeValue.PendingChange:
                    filteredListItems = filteredListItems.Where(p => p.Type == ConstantUtil.TicketType.Change);
                    filteredListItems = filteredListItems.Where(p => p.Status != ConstantUtil.TicketStatus.Cancelled
                        && p.Status != ConstantUtil.TicketStatus.Cancelled);
                    break;
                    // Default All tickets
            }

            var displayedList = filteredListItems.Skip(param.start).Take(param.length);
            var result = filteredListItems.Select(p => new IConvertible[]
            {
                p.Code,
                p.Subject,
                (p.CreatedTime == null) ? "" : ((DateTime) p.CreatedTime).ToString("dd/MM/yyyy HH:mm"),
                (p.ScheduleEndDate == null) ? "-": ((DateTime) p.ScheduleEndDate).ToString("dd/MM/yyyy"),
                (p.ActualEndDate == null) ? "-": ((DateTime) p.ActualEndDate).ToString("dd/MM/yyyy"),
                TMSUtils.ConvertTypeFromInt(p.Type),
                TMSUtils.ConvertStatusFromInt(p.Status),
                TMSUtils.ConvertModeFromInt(p.Mode),
                p.CategoryID == null ? "-" : _categoryService.GetCategoryById((int) p.CategoryID).Name,
                p.ImpactID == null ? "-" : _impactService.GetImpactById((int)p.ImpactID).Name,
                p.UrgencyID == null ? "-" : _urgencyService.GetUrgencyByID((int)p.UrgencyID).Name,
                p.PriorityID == null ? "-" : _priorityService.GetPriorityByID((int) p.PriorityID).Name,
                p.TechnicianID == null ? "-" : _userService.GetUserById(p.TechnicianID).Department.Name
            });


            return Json(new
            {
                param.sEcho,
                iTotalRecords = result.Count(),
                iTotalDisplayRecords = filteredListItems.Count(),
                aaData = result
            }, JsonRequestBehavior.AllowGet);

        }

        // Helpdesk/report
        [HttpPost]
        public ActionResult DrawGraph(int type, int by, DateTime? date_from_select, DateTime? date_to_select)
        {
            var ticketList = _ticketService.GetAll();
            IEnumerable<Ticket> filteredListItems;
            filteredListItems = ticketList;
            if (date_from_select.HasValue)
            {
                filteredListItems = filteredListItems.Where(p => p.CreatedTime.Date >= date_from_select.Value);
            }
            if (date_to_select.HasValue)
            {
                filteredListItems = filteredListItems.Where(p => p.CreatedTime.Date <= date_to_select.Value);
            }
            switch (type)
            {
                // All Request 
                case ConstantUtil.TicketTypeValue.Request:
                    filteredListItems = filteredListItems.Where(p => p.Type == ConstantUtil.TicketType.Request);
                    break;
                // All Problems
                case ConstantUtil.TicketTypeValue.Problem:
                    filteredListItems = filteredListItems.Where(p => p.Type == ConstantUtil.TicketType.Problem);
                    break;
                // All Request
                case ConstantUtil.TicketTypeValue.Change:
                    filteredListItems = filteredListItems.Where(p => p.Type == ConstantUtil.TicketType.Request);
                    break;
                // All pending requests
                case ConstantUtil.TicketTypeValue.PendingRequest:
                    filteredListItems = filteredListItems.Where(p => p.Type == ConstantUtil.TicketType.Request);
                    filteredListItems = filteredListItems.Where(p => p.Status != ConstantUtil.TicketStatus.Cancelled 
                        && p.Status != ConstantUtil.TicketStatus.Cancelled);
                    break;
                // All pending problems
                case ConstantUtil.TicketTypeValue.PendingProblem:
                    filteredListItems = filteredListItems.Where(p => p.Type == ConstantUtil.TicketType.Problem);
                    filteredListItems = filteredListItems.Where(p => p.Status != ConstantUtil.TicketStatus.Cancelled 
                        && p.Status != ConstantUtil.TicketStatus.Cancelled);
                    break;
                // All pending changes
                case ConstantUtil.TicketTypeValue.PendingChange:
                    filteredListItems = filteredListItems.Where(p => p.Type == ConstantUtil.TicketType.Change);
                    filteredListItems = filteredListItems.Where(p => p.Status != ConstantUtil.TicketStatus.Cancelled 
                        && p.Status != ConstantUtil.TicketStatus.Cancelled);
                    break;
                    // Default All tickets
            }

            List<string> labels = new List<string>();
            List<int> data = new List<int>();
            // Switch by
            switch (by)
            {
                // mode
                case 0:
                    foreach (TicketModeEnum mode in System.Enum.GetValues(typeof(TicketModeEnum)))
                    {
                        labels.Add(mode.ToString());
                        var ticketModes = filteredListItems.Where(p => p.Mode == (int)mode);
                        data.Add(ticketModes.Count());
                    }
                    return Json(new
                    {
                        label = labels,
                        data = data
                    });
                //Category
                case 1:
                    {
                        labels.Add("Unassigned");
                        data.Add(filteredListItems.Where(p => p.Category == null).Count());
                        IEnumerable<Category> categories = _categoryService.GetCategories();
                        foreach (Category category in categories)
                        {
                            List<int> childrenCategoriesIdList = _categoryService.GetChildrenCategoriesIdList(category.ID);
                            labels.Add(category.Name);
                            data.Add(filteredListItems.Where(m => m.CategoryID != null && (m.CategoryID == category.ID || childrenCategoriesIdList.Contains(m.CategoryID.Value))).Count());
                        }

                        return Json(new
                        {
                            label = labels,
                            data = data
                        }, JsonRequestBehavior.AllowGet);

                    }

                // Impact
                case 2:
                    {
                        IEnumerable<Impact> impacts = _impactService.GetAll();
                        //duyet list 
                        IEnumerable<Ticket> impactInTicketList = null;
                        labels.Add("Unassigned");
                        data.Add(filteredListItems.Where(p => p.Impact == null).Count());
                        foreach (var impact in impacts)
                        {
                            labels.Add(impact.Name);
                            impactInTicketList = filteredListItems.Where(p => p.ImpactID == impact.ID);
                            data.Add(impactInTicketList.Count());
                        }
                        return Json(new
                        {
                            label = labels,
                            data = data,
                        }, JsonRequestBehavior.AllowGet);
                    }
                //Urgency
                case 3:
                    {
                        IEnumerable<Urgency> urgencyList = _urgencyService.GetAll();
                        IEnumerable<Ticket> urgencyListInTickets = null;
                        labels.Add("Unassigned");
                        data.Add(filteredListItems.Where(p => p.Urgency == null).Count());
                        foreach (var urgency in urgencyList)
                        {
                            labels.Add(urgency.Name);
                            urgencyListInTickets = filteredListItems.Where(p => p.UrgencyID == urgency.ID);
                            data.Add(urgencyListInTickets.Count());
                        }
                        return Json(new
                        {
                            label = labels,
                            data = data
                        }, JsonRequestBehavior.AllowGet);
                    }

                // Priority
                case 4:
                    {
                        IEnumerable<Priority> priorityList = _priorityService.GetAll();
                        IEnumerable<Ticket> priorityInTickets;
                        // Label and data for null priority
                        labels.Add("Unassigned");
                        data.Add(filteredListItems.Where(p => p.PriorityID == null).Count());

                        foreach (var priority in priorityList)
                        {
                            labels.Add(priority.Name);
                            priorityInTickets = filteredListItems.Where(p => p.PriorityID == priority.ID);
                            data.Add(priorityInTickets.Count());
                        }
                        return Json(new
                        {
                            label = labels,
                            data = data
                        });

                    }
                // Department
                case 5:
                    {
                        IEnumerable<AspNetUser> listTechinicians = _userService.GetTechnicians();
                        IEnumerable<Ticket> techinicianInteTickets = new List<Ticket>();
                        labels.Add("Unassigned");
                        data.Add(filteredListItems.Where(p => p.TechnicianID == null).Count());
                        foreach (var techinician in listTechinicians)
                        {
                            techinicianInteTickets = filteredListItems.Where(p => p.TechnicianID == techinician.Id);
                            IEnumerable<Department> departments = _departmentService.GetAll();
                            foreach (var department in departments)
                            {
                                labels.Add(department.Name);
                                listTechinicians = listTechinicians.Where(p => p.DepartmentID == department.ID);
                                data.Add(listTechinicians.Count());
                            }
                        }
                        return Json(new
                        {
                            label = labels,
                            data = data
                        }, JsonRequestBehavior.AllowGet);

                    }
                // Status
                case 6:
                    {
                        IEnumerable<Ticket> statusInTickets;
                        foreach (TicketStatusEnum status in Enum.GetValues(typeof(TicketStatusEnum)))
                        {
                            labels.Add(status.ToString());
                            statusInTickets = filteredListItems.Where(p => p.Status == (int)status);
                            data.Add(statusInTickets.Count());
                        }

                        return Json(new
                        {
                            label = labels,
                            data = data
                        }, JsonRequestBehavior.AllowGet);
                    }
            }
            return null;
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