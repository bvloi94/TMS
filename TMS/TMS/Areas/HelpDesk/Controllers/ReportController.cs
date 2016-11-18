using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TMS.DAL;
using TMS.Models;
using TMS.Services;
using TMS.Utils;
using TMS.ViewModels;

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
        public GroupService _groupService { get; set; }

        public ReportController()
        {
            var _UnitofWork = new UnitOfWork();
            _ticketService = new TicketService(_UnitofWork);
            _userService = new UserService(_UnitofWork);
            _impactService = new ImpactService(_UnitofWork);
            _categoryService = new CategoryService(_UnitofWork);
            _urgencyService = new UrgencyService(_UnitofWork);
            _priorityService = new PriorityService(_UnitofWork);
            _groupService = new GroupService(_UnitofWork);
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
                // All Request : All ticket have type request
                case ConstantUtil.TicketTypeValue.Request:
                    filteredListItems = filteredListItems.Where(p => p.Type == ConstantUtil.TicketType.Request);
                    break;
                // All Problems: All ticket have type problem
                case ConstantUtil.TicketTypeValue.Problem:
                    filteredListItems = filteredListItems.Where(p => p.Type == ConstantUtil.TicketType.Problem);
                    break;
                // All Request:  All ticket have type change
                case ConstantUtil.TicketTypeValue.Change:
                    filteredListItems = filteredListItems.Where(p => p.Type == ConstantUtil.TicketType.Change);
                    break;
                // All pending requests: All ticket have type request and have status != cancel and closed
                case ConstantUtil.TicketTypeValue.PendingRequest:
                    filteredListItems = filteredListItems.Where(p => p.Type == ConstantUtil.TicketType.Request);
                    filteredListItems = filteredListItems.Where(p => p.Status != ConstantUtil.TicketStatus.Cancelled
                        && p.Status != ConstantUtil.TicketStatus.Closed);
                    break;
                // All pending problems: All ticket have type problems and have status != cancel and closed
                case ConstantUtil.TicketTypeValue.PendingProblem:
                    filteredListItems = filteredListItems.Where(p => p.Type == ConstantUtil.TicketType.Problem);
                    filteredListItems = filteredListItems.Where(p => p.Status != ConstantUtil.TicketStatus.Cancelled
                        && p.Status != ConstantUtil.TicketStatus.Closed);
                    break;
                // All pending changes: All ticket have type change and have status != cancel and closed
                case ConstantUtil.TicketTypeValue.PendingChange:
                    filteredListItems = filteredListItems.Where(p => p.Type == ConstantUtil.TicketType.Change);
                    filteredListItems = filteredListItems.Where(p => p.Status != ConstantUtil.TicketStatus.Cancelled
                        && p.Status != ConstantUtil.TicketStatus.Closed);
                    break;
                    // Default All tickets
            }

            var displayedList = filteredListItems.Skip(param.start).Take(param.length);

            var result = filteredListItems.Select(p => new TicketViewModel
            {
                Code = p.Code,
                Subject = p.Subject,
                CreatedTimeString = p.CreatedTime.ToString(ConstantUtil.DateTimeFormat),
                DueByDateString = p.DueByDate.ToString(ConstantUtil.DateTimeFormat),
                ScheduleEndDateString = p.ScheduleEndDate.ToString(ConstantUtil.DateTimeFormat),
                ActualEndDateString = p.ActualEndDate.HasValue ? p.ActualEndDate.Value.ToString(ConstantUtil.DateTimeFormat) : "-",
                TypeString = GeneralUtil.GetTypeNameByType(p.Type),
                Status = GeneralUtil.GetTicketStatusByID(p.Status),
                ModeString = GeneralUtil.GetModeNameByMode(p.Mode),
                Category = (p.Category == null) ? "-" : p.Category.Name,
                Impact = p.Impact.Name,
                Urgency = p.Urgency.Name,
                Priority = p.Priority.Name,
                Group = (_userService.GetUserById(p.TechnicianID) == null) ? "-" : (_userService.GetUserById(p.TechnicianID).Group == null ? "-" : _userService.GetUserById(p.TechnicianID).Group.Name),
                IsOverdue = DateTime.Now > p.DueByDate
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
                        && p.Status != ConstantUtil.TicketStatus.Closed);
                    break;
                // All pending problems
                case ConstantUtil.TicketTypeValue.PendingProblem:
                    filteredListItems = filteredListItems.Where(p => p.Type == ConstantUtil.TicketType.Problem);
                    filteredListItems = filteredListItems.Where(p => p.Status != ConstantUtil.TicketStatus.Cancelled
                        && p.Status != ConstantUtil.TicketStatus.Closed);
                    break;
                // All pending changes
                case ConstantUtil.TicketTypeValue.PendingChange:
                    filteredListItems = filteredListItems.Where(p => p.Type == ConstantUtil.TicketType.Change);
                    filteredListItems = filteredListItems.Where(p => p.Status != ConstantUtil.TicketStatus.Cancelled
                        && p.Status != ConstantUtil.TicketStatus.Closed);
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
                    foreach (var mode in typeof(ConstantUtil.TicketMode).GetFields())
                    {
                        labels.Add(typeof(ConstantUtil.TicketModeString).GetField(mode.Name).GetValue(null).ToString());
                        var ticketModes = filteredListItems.Where(p => p.Mode == (int)mode.GetValue(null));
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
                // Group
                case 5:
                    {
                        IEnumerable<AspNetUser> allTehnicians = _userService.GetTechnicians();
                        IEnumerable<Ticket> techinicianInteTickets = new List<Ticket>();
                        labels.Add("Unassigned");
                        data.Add(filteredListItems.Where(p => p.TechnicianID == null).Count());
                        IEnumerable<Group> groups = _groupService.GetAll();
                        foreach (var group in groups)
                        {
                            int ticketCount = 0;
                            var listTechOfGroup = allTehnicians.Where(p => p.GroupID == group.ID);
                            // lay list technician co group la "group"
                            foreach (var techinician in listTechOfGroup)
                            {
                                // duyet list ticket theo technician ID 
                                techinicianInteTickets = filteredListItems.Where(p => p.TechnicianID == techinician.Id);
                                ticketCount += techinicianInteTickets.Count();
                            }
                            labels.Add(group.Name);
                            data.Add(ticketCount);
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
                        foreach (var status in typeof(ConstantUtil.TicketStatus).GetFields())
                        {
                            labels.Add(typeof(ConstantUtil.TicketStatusString).GetField(status.Name).GetValue(null).ToString());
                            var ticketStatus = filteredListItems.Where(p => p.Status == (int)status.GetValue(null));
                            data.Add(ticketStatus.Count());
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

        [HttpPost]
        public FileContentResult Export(string GraphImage, string DataImage)
        {
            byte[] graphBytes = Convert.FromBase64String(GraphImage.Replace("data:image/png;base64,", String.Empty));
            byte[] dataBytes = Convert.FromBase64String(DataImage.Replace("data:image/png;base64,", String.Empty));
            List<byte[]> imagesByte = new List<byte[]>();
            imagesByte.Add(graphBytes);
            imagesByte.Add(dataBytes);

            var content = PDFUtil.ReportContent(imagesByte);

            string fileName = DateTime.Now.ToShortDateString().Replace("/", String.Empty) + "report.pdf";

            Response.Buffer = false;
            Response.Clear();
            Response.ClearContent();
            Response.ClearHeaders();
            Response.AppendHeader("content-disposition", "attachment;filename=" + fileName);
            Response.AppendHeader("Set-Cookie", "fileDownload=true; path=/");
            Response.ContentType = "Application/pdf";

            //Write the file content directly to the HTTP content output stream.    
            Response.BinaryWrite(content);
            Response.Flush();
            Response.End();
            return File(content, "application/pdf");
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