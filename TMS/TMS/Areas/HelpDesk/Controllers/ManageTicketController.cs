using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using TMS.DAL;
using TMS.Enumerator;
using TMS.Models;
using TMS.Services;
using TMS.Utils;
using TMS.ViewModels;
using ModelError = TMS.ViewModels.ModelError;

namespace TMS.Areas.HelpDesk.Controllers
{
    public class ManageTicketController : Controller
    {
        private TMSEntities db = new TMSEntities();

        public TicketService _ticketService { get; set; }
        public UserService _userService { get; set; }
        public DepartmentService _departmentService { get; set; }
        public UrgencyService _urgencyService { get; set; }
        public PriorityService _priorityService { get; set; }
        public ImpactService _impactService { get; set; }
        public CategoryService _categoryService { get; set; }
        public TicketAttachmentService _ticketAttachmentService { get; set; }
        private UnitOfWork unitOfWork = new UnitOfWork();


        public ManageTicketController()
        {
            _ticketService = new TicketService(unitOfWork);
            _userService = new UserService(unitOfWork);
            _departmentService = new DepartmentService(unitOfWork);
            _urgencyService = new UrgencyService(unitOfWork);
            _priorityService = new PriorityService(unitOfWork);
            _impactService = new ImpactService(unitOfWork);
            _categoryService = new CategoryService(unitOfWork);
            _ticketAttachmentService = new TicketAttachmentService(unitOfWork);
        }

        // GET: HelpDesk/ManageTicket
        public ActionResult Index()
        {
            //var tickets = db.Tickets.Include(t => t.AspNetUser).Include(t => t.AspNetUser1).Include(t => t.AspNetUser2).Include(t => t.AspNetUser3).Include(t => t.Category).Include(t => t.Department).Include(t => t.Impact).Include(t => t.Priority).Include(t => t.Urgency);
            //return View(tickets.ToList());
            return View();
        }

        // GET: HelpDesk/ManageTicket/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }

        public ActionResult CreateNewTicket()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddNewTicket(TicketViewModel model, IEnumerable<HttpPostedFileBase> descriptionFiles)
        {
            ModelErrorViewModel errs = null;
            errs = new ModelErrorViewModel();

            var ticket = new Ticket();
            if (User.Identity.GetUserId() != null) ticket.CreatedID = User.Identity.GetUserId();
            else
            {
                return Json(new
                {
                    success = false,
                    msg = "Session time out! Please login again!",
                });

            }

            if (string.IsNullOrEmpty(model.Subject))
            {
                errs.Add(new ModelError
                {
                    Name = "Subject",
                    Message = "Ticket's subject required!",
                });
                return Json(new
                {
                    success = false,
                    data = errs,
                    msg = "Please enter ticket's subject!",
                });
            }

            if (model.RequesterId == null)
            {
                errs.Add(new ModelError
                {
                    Name = "Subject",
                    Message = "Ticket's requester required!",
                });
                return Json(new
                {
                    success = false,
                    data = errs,
                    msg = "Please input requester!",
                });
            }

            ticket.ScheduleStartDate = model.ScheduleStartDate != null
                ? DateTime.ParseExact(model.ScheduleStartDate, ConstantUtil.DateTimeFormat, null)
                : (DateTime?)null;
            ticket.ScheduleEndDate = model.ScheduleEndDate != null
                ? DateTime.ParseExact(model.ScheduleEndDate, ConstantUtil.DateTimeFormat, null)
                : (DateTime?)null;

            if (ticket.ScheduleStartDate.HasValue && ticket.ScheduleEndDate.HasValue)
            {
                if (DateTime.Compare((DateTime)ticket.ScheduleStartDate, (DateTime)ticket.ScheduleEndDate) > 0)
                {
                    errs.Add(new ModelError
                    {
                        Name = "ScheduleDate",
                        Message = "Schedule start date must before schedule end date!",
                    });
                    return Json(new
                    {
                        success = false,
                        data = errs,
                        msg = "Please check schedule date again!"
                    });
                }
            }

            ticket.ActualStartDate = model.ActualStartDate != null
                ? DateTime.ParseExact(model.ActualStartDate, ConstantUtil.DateTimeFormat, null)
                : (DateTime?)null;
            ticket.ActualEndDate = model.ActualEndDate != null
                ? DateTime.ParseExact(model.ActualEndDate, ConstantUtil.DateTimeFormat, null)
                : (DateTime?)null;

            if (ticket.ActualStartDate.HasValue && ticket.ActualEndDate.HasValue)
            {
                if (DateTime.Compare((DateTime)ticket.ActualStartDate, (DateTime)ticket.ActualEndDate) > 0)
                {
                    errs.Add(new ModelError
                    {
                        Name = "ActualDate",
                        Message = "actual start date must before actual end date!",
                    });
                    return Json(new
                    {
                        success = false,
                        data = errs,
                        msg = "Please check actual date again!"
                    });
                }
            }

            ticket.CreatedTime = DateTime.Now;
            ticket.ModifiedTime = DateTime.Now;
            ticket.Status = (int?)TicketStatusEnum.New;
            ticket.RequesterID = model.RequesterId;
            ticket.Subject = model.Subject;
            ticket.Description = model.Description;
            ticket.Solution = model.Solution;
            ticket.Type = model.Type;
            ticket.Mode = ConstantUtil.TicketMode.PhoneCall;
            if (model.ImpactId != 0) ticket.ImpactID = model.ImpactId;
            ticket.ImpactDetail = model.ImpactDetail;
            if (model.UrgencyId != 0) ticket.UrgencyID = model.UrgencyId;
            if (model.PriorityId != 0) ticket.PriorityID = model.PriorityId;
            if (model.CategoryId != 0) ticket.CategoryID = model.CategoryId;

            if (!string.IsNullOrEmpty(model.TechnicianId))
            {
                ticket.TechnicianID = model.TechnicianId;
                ticket.AssignedByID = User.Identity.GetUserId();
                ticket.Status = (int?)TicketStatusEnum.Assigned;
            }

            try
            {
                _ticketService.AddTicket(ticket);
                //TicketAttachment ticketFiles = new TicketAttachment();
                if (descriptionFiles.ToList()[0] != null && descriptionFiles.ToList().Count > 0)
                {
                    _ticketAttachmentService.saveFile(ticket.ID, descriptionFiles);
                    //List<TicketAttachment> listFile = unitOfWork.TicketAttachmentRepository.Get(i => i.TicketID == ticket.ID).ToList();
                    //ticketFiles.Path = listFile[0].Path;
                }
            }
            catch (DbUpdateException ex)
            {
                return Json(new
                {
                    success = false,
                    msg = ex.Message
                });
            }

            return Json(new
            {
                success = true
            });
        }

        public ActionResult EditTicket(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = _ticketService.GetTicketByID((int)id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            TicketViewModel model = new TicketViewModel();
            model.Id = ticket.ID;
            model.Subject = ticket.Subject;
            model.Description = ticket.Description;
            model.Solution = ticket.Solution;
            //unapprove reason

            if (!string.IsNullOrEmpty(ticket.RequesterID))
            {
                model.RequesterId = ticket.RequesterID;
                model.Requester = _userService.GetUserById(ticket.RequesterID).Fullname;
            }
            if (ticket.Type != null) model.Type = (int)ticket.Type;
            model.Mode = ticket.Mode;
            if (ticket.Status != null)
            {
                model.StatusId = (int)ticket.Status;
                model.Status = ((TicketStatusEnum)ticket.Status).ToString();
            }
            if (ticket.CategoryID != null)
            {
                model.CategoryId = (int)ticket.CategoryID;
                model.Category = _categoryService.GetCategoryById((int)ticket.CategoryID).Name;
            }
            if (ticket.UrgencyID != null)
            {
                model.UrgencyId = (int)ticket.UrgencyID;
                model.Urgency = ticket.UrgencyID == null ? "" : _urgencyService.GetUrgencyByID((int)ticket.UrgencyID).Name;
            }
            if (ticket.PriorityID != null)
            {
                model.PriorityId = (int)ticket.PriorityID;
                model.Priority = ticket.PriorityID == null
                    ? ""
                    : _priorityService.GetPriorityByID((int)ticket.PriorityID).Name;
            }
            if (ticket.ImpactID != null)
            {
                model.ImpactId = (int)ticket.ImpactID;
                model.Impact = ticket.ImpactID == null ? "" : _impactService.GetImpactById((int)ticket.ImpactID).Name;
            }
            model.ImpactDetail = ticket.ImpactDetail;
            model.ScheduleStartDate = ticket.ScheduleStartDate?.ToString(ConstantUtil.DateTimeFormat) ?? "";
            model.ScheduleEndDate = ticket.ScheduleEndDate?.ToString(ConstantUtil.DateTimeFormat) ?? "";
            model.ActualStartDate = ticket.ActualStartDate?.ToString(ConstantUtil.DateTimeFormat) ?? "";
            model.ActualEndDate = ticket.ActualEndDate?.ToString(ConstantUtil.DateTimeFormat) ?? "";
            model.SolvedDate = ticket.SolvedDate?.ToString(ConstantUtil.DateFormat) ?? "";
            model.CreatedTime = ticket.CreatedTime.ToString(ConstantUtil.DateTimeFormat);
            model.ModifiedTime = ticket.ModifiedTime.ToString(ConstantUtil.DateTimeFormat);

            if (!string.IsNullOrEmpty(ticket.CreatedID))
            {
                model.CreatedId = ticket.CreatedID;
                model.CreatedBy = _userService.GetUserById(ticket.CreatedID).Fullname;
            }
            if (!string.IsNullOrEmpty(ticket.SolveID))
            {
                model.SolvedId = ticket.SolveID;
                model.SolvedBy = _userService.GetUserById(ticket.SolveID).Fullname;
            }
            if (!string.IsNullOrEmpty(ticket.TechnicianID))
            {
                AspNetUser technician = _userService.GetUserById(ticket.TechnicianID);
                model.TechnicianId = technician.Id;
                model.Technician = technician.Fullname;
                if (technician.DepartmentID.HasValue)
                {
                    model.DepartmentId = (int)technician.DepartmentID;
                    model.Department = _departmentService.GetDepartmentById((int)technician.DepartmentID).Name;
                }
            }
            return View(model);
        }


        [HttpPost]
        public ActionResult UpdateTicket(TicketViewModel model, IEnumerable<HttpPostedFileBase> descriptionFiles)
        {
            ModelErrorViewModel errs = null;
            errs = new ModelErrorViewModel();
            Ticket ticket = _ticketService.GetTicketByID((int)model.Id);
            if (ticket == null)
            {
                return HttpNotFound();
            }

            if (string.IsNullOrEmpty(model.Subject))
            {
                errs.Add(new ModelError
                {
                    Name = "Subject",
                    Message = "Ticket's subject required!",
                });
                return Json(new
                {
                    success = false,
                    data = errs,
                    msg = "Please enter ticket's subject!",
                });
            }

            if (model.RequesterId == null)
            {
                errs.Add(new ModelError
                {
                    Name = "Subject",
                    Message = "Ticket's requester required!",
                });
                return Json(new
                {
                    success = false,
                    data = errs,
                    msg = "Please input requester!",
                });
            }

            ticket.ScheduleStartDate = model.ScheduleStartDate != null
                ? DateTime.ParseExact(model.ScheduleStartDate, ConstantUtil.DateTimeFormat, null)
                : (DateTime?)null;
            ticket.ScheduleEndDate = model.ScheduleEndDate != null
                ? DateTime.ParseExact(model.ScheduleEndDate, ConstantUtil.DateTimeFormat, null)
                : (DateTime?)null;

            if (ticket.ScheduleStartDate.HasValue && ticket.ScheduleEndDate.HasValue)
            {
                if (DateTime.Compare((DateTime)ticket.ScheduleStartDate, (DateTime)ticket.ScheduleEndDate) > 0)
                {
                    errs.Add(new ModelError
                    {
                        Name = "ScheduleDate",
                        Message = "Schedule start date must before schedule end date!",
                    });
                    return Json(new
                    {
                        success = false,
                        data = errs,
                        msg = "Please check schedule date again!"
                    });
                }
            }

            ticket.ActualStartDate = model.ActualStartDate != null
                ? DateTime.ParseExact(model.ActualStartDate, ConstantUtil.DateTimeFormat, null)
                : (DateTime?)null;
            ticket.ActualEndDate = model.ActualEndDate != null
                ? DateTime.ParseExact(model.ActualEndDate, ConstantUtil.DateTimeFormat, null)
                : (DateTime?)null;

            if (ticket.ActualStartDate.HasValue && ticket.ActualEndDate.HasValue)
            {
                if (DateTime.Compare((DateTime)ticket.ActualStartDate, (DateTime)ticket.ActualEndDate) > 0)
                {
                    errs.Add(new ModelError
                    {
                        Name = "ActualDate",
                        Message = "actual start date must before actual end date!",
                    });
                    return Json(new
                    {
                        success = false,
                        data = errs,
                        msg = "Please check actual date again!"
                    });
                }
            }

            ticket.ModifiedTime = DateTime.Now;
            ticket.Subject = model.Subject;
            ticket.Type = model.Type;
            ticket.Mode = model.Mode;
            ticket.Description = model.Description;
            ticket.RequesterID = model.RequesterId;
            ticket.Solution = model.Solution;
            if (model.ImpactId != 0) ticket.ImpactID = model.ImpactId;
            ticket.ImpactDetail = model.ImpactDetail;
            if (model.UrgencyId != 0) ticket.UrgencyID = model.UrgencyId;
            if (model.PriorityId != 0) ticket.PriorityID = model.PriorityId;
            if (model.CategoryId != 0) ticket.CategoryID = model.CategoryId;

            if (!string.IsNullOrEmpty(model.TechnicianId))
            {
                if (ticket.TechnicianID != model.TechnicianId)
                {
                    ticket.TechnicianID = model.TechnicianId;
                    ticket.AssignedByID = User.Identity.GetUserId();
                    ticket.Status = (int?)TicketStatusEnum.Assigned;
                }
            }
            
            try
            {
                _ticketService.UpdateTicket(ticket);
            }
            catch (DbUpdateException ex)
            {
                return Json(new
                {
                    success = false,
                    msg = ex.Message
                });
            }

            return Json(new
            {
                success = true
            });
        }

        public ActionResult CancelTicket(int? ticketId)
        {

            bool result = _ticketService.CancelTicket(ticketId);
            if (!result) return Json(new
            {
                success = false,
                msg = "Error occured! Please try again!"
            });

            return Json(new
            {
                success = true
            });
        }

        public ActionResult MergeTicket(int[] selectedTickets)
        {
            List<Ticket> tickets = new List<Ticket>();
            string requesterId = "";
            for (int i = 0; i < selectedTickets.Length; i++)
            {
                Ticket ticket = _ticketService.GetTicketByID(selectedTickets[i]);
                if (ticket != null)
                {
                    if (i > 0 && requesterId != ticket.RequesterID)
                    {
                        return Json(new
                        {
                            success = false,
                            msg = "Merging tickets must have the same requester!"
                        });
                    }
                    requesterId = ticket.RequesterID;
                    tickets.Add(ticket);
                }
            }
            tickets.Sort((x, y) => DateTime.Compare((DateTime)x.CreatedTime, (DateTime)y.CreatedTime));
            Ticket newTicket = tickets[0];
            for (int i = 1; i < tickets.Count; i++)
            {
                Ticket oldTicket = tickets[i];
                newTicket.Description += "<br/>" + oldTicket.Description;
                oldTicket.ModifiedTime = DateTime.Now;
                oldTicket.Status = (int?)TicketStatusEnum.Canceled;
                _ticketService.UpdateTicket(oldTicket);
            }
            newTicket.ModifiedTime = DateTime.Now;
            _ticketService.UpdateTicket(newTicket);

            return Json(new
            {
                success = true
            });
        }

        [HttpPost]
        public ActionResult LoadAllTickets(JqueryDatatableParameterViewModel param)
        {
            var default_search_key = Request["search[value]"];
            var status_filter = Request["status_filter"];
            var search_text = Request["search_text"];

            var queriedResult = _ticketService.GetAll();
            IEnumerable<Ticket> filteredListItems;

            if (!string.IsNullOrEmpty(default_search_key))
            {
                filteredListItems = queriedResult.Where(p => p.Subject.ToLower().Contains(default_search_key.ToLower()));
            }
            else
            {
                filteredListItems = queriedResult;
            }
            // Search by custom
            if (!string.IsNullOrEmpty(status_filter))
            {
                int statusNo = Int32.Parse(status_filter);
                if (statusNo > 0)
                {
                    filteredListItems = filteredListItems.Where(p => p.Status == statusNo);
                }
            }

            //Hide Canceled Tickets
            filteredListItems = filteredListItems.Where(p => p.Status != (int)TicketStatusEnum.Canceled);

            if (!string.IsNullOrEmpty(search_text))
            {
                filteredListItems = filteredListItems.Where(p => p.Subject.ToLower().Contains(search_text.ToLower()));
            }

            // Sort.
            var sortColumnIndex = Convert.ToInt32(param.order[0]["column"]);
            var sortDirection = param.order[0]["dir"];

            switch (sortColumnIndex)
            {
                case 1:
                    filteredListItems = sortDirection == "asc"
                        ? filteredListItems.OrderBy(p => p.Subject)
                        : filteredListItems.OrderByDescending(p => p.Subject);
                    break;
                case 5:
                    filteredListItems = sortDirection == "asc"
                        ? filteredListItems.OrderBy(p => p.Status)
                        : filteredListItems.OrderByDescending(p => p.Status);
                    break;
                case 6:
                    filteredListItems = sortDirection == "asc"
                        ? filteredListItems.OrderBy(p => p.CreatedTime)
                        : filteredListItems.OrderByDescending(p => p.CreatedTime);
                    break;
            }

            var result = filteredListItems.Skip(param.start).Take(param.length).ToList();
            var tickets = new List<TicketViewModel>();
            int startNo = param.start;
            foreach (var item in result)
            {
                var s = new TicketViewModel();
                s.No = ++startNo;
                s.Id = item.ID;
                s.Subject = item.Subject;
                s.Requester = item.RequesterID == null ? "" : _userService.GetUserById(item.RequesterID).Fullname;
                if (item.TechnicianID != null)
                {
                    AspNetUser technician = _userService.GetUserById(item.TechnicianID);
                    s.Technician = technician.Fullname;
                }
                else
                {
                    s.Technician = "";
                }
                s.SolvedDate = item.SolvedDate?.ToString(ConstantUtil.DateFormat) ?? "";
                s.Status = item.Status.HasValue ? ((TicketStatusEnum)item.Status).ToString() : "";
                s.ModifiedTime = item.ModifiedTime.ToString(ConstantUtil.DateTimeFormat);
                tickets.Add(s);
            }
            JqueryDatatableResultViewModel rsModel = new JqueryDatatableResultViewModel();
            rsModel.draw = param.draw;
            rsModel.recordsTotal = queriedResult.Count();
            rsModel.recordsFiltered = filteredListItems.Count();
            rsModel.data = tickets;
            return Json(rsModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadTicketById(int? id)
        {
            Ticket ticket = _ticketService.GetTicketByID((int)id);
            TicketViewModel model = new TicketViewModel();
            model.Id = ticket.ID;
            model.Subject = ticket.Subject;
            model.Description = ticket.Description;
            model.Solution = ticket.Solution;
            if (!string.IsNullOrEmpty(ticket.RequesterID))
            {
                model.RequesterId = ticket.RequesterID;
                model.Requester = _userService.GetUserById(ticket.RequesterID).Fullname;
            }
            if (ticket.Type != null) model.Type = (int)ticket.Type;
            model.Mode = ticket.Mode;
            if (ticket.Status != null)
            {
                model.StatusId = (int)ticket.Status;
                model.Status = ((TicketStatusEnum)ticket.Status).ToString();
            }
            if (ticket.CategoryID != null)
            {
                model.CategoryId = (int)ticket.CategoryID;
                model.Category = _categoryService.GetCategoryById((int)ticket.CategoryID).Name;
            }
            if (ticket.UrgencyID != null)
            {
                model.UrgencyId = (int)ticket.UrgencyID;
                model.Urgency = ticket.UrgencyID == null ? "" : _urgencyService.GetUrgencyByID((int)ticket.UrgencyID).Name;
            }
            if (ticket.PriorityID != null)
            {
                model.PriorityId = (int)ticket.PriorityID;
                model.Priority = ticket.PriorityID == null
                    ? ""
                    : _priorityService.GetPriorityByID((int)ticket.PriorityID).Name;
            }
            if (ticket.ImpactID != null)
            {
                model.ImpactId = (int)ticket.ImpactID;
                model.Impact = ticket.ImpactID == null ? "" : _impactService.GetImpactById((int)ticket.ImpactID).Name;
            }
            model.ImpactDetail = ticket.ImpactDetail;
            model.UnapproveReason = ticket.UnapproveReason;
            model.ScheduleStartDate = ticket.ScheduleStartDate?.ToString(ConstantUtil.DateTimeFormat) ?? "";
            model.ScheduleEndDate = ticket.ScheduleEndDate?.ToString(ConstantUtil.DateTimeFormat) ?? "";
            model.ActualStartDate = ticket.ActualStartDate?.ToString(ConstantUtil.DateTimeFormat) ?? "";
            model.ActualEndDate = ticket.ActualEndDate?.ToString(ConstantUtil.DateTimeFormat) ?? "";
            model.SolvedDate = ticket.SolvedDate?.ToString(ConstantUtil.DateFormat) ?? "";
            model.CreatedTime = ticket.CreatedTime.ToString(ConstantUtil.DateTimeFormat);
            model.ModifiedTime = ticket.ModifiedTime.ToString(ConstantUtil.DateTimeFormat);
            if (!string.IsNullOrEmpty(ticket.CreatedID))
            {
                model.CreatedId = ticket.CreatedID;
                model.CreatedBy = _userService.GetUserById(ticket.CreatedID).Fullname;
            }
            if (!string.IsNullOrEmpty(ticket.TechnicianID))
            {
                AspNetUser technician = _userService.GetUserById(ticket.TechnicianID);
                model.TechnicianId = technician.Id;
                model.Technician = technician.Fullname;
                if (technician.DepartmentID.HasValue)
                {
                    model.DepartmentId = (int)technician.DepartmentID;
                    model.Department = _departmentService.GetDepartmentById((int)technician.DepartmentID).Name;
                }
            }
            return Json(new
            {
                success = true,
                data = model,
            });
        }

        [HttpGet]
        public ActionResult GetRequesterList(JqueryDatatableParameterViewModel param)
        {
            var requesterList = _userService.GetRequesters();

            IEnumerable<AspNetUser> filteredListItems;
            if (!string.IsNullOrEmpty(param.search["value"]))
            {
                filteredListItems = requesterList.Where(p => p.Fullname.ToLower().Contains(param.search["value"].ToLower()));
            }
            else
            {
                filteredListItems = requesterList;
            }
            // Sort.
            //var sortColumnIndex = Convert.ToInt32(param.order[0].column);
            //var sortDirection = param.order[0].dir;
            var sortColumnIndex = Convert.ToInt32(param.order[0]["column"]);
            var sortDirection = param.order[0]["dir"];

            switch (sortColumnIndex)
            {
                case 0:
                    filteredListItems = sortDirection == "asc"
                        ? filteredListItems.OrderBy(p => p.Fullname)
                        : filteredListItems.OrderByDescending(p => p.Fullname);
                    break;
                case 1:
                    filteredListItems = sortDirection == "asc"
                        ? filteredListItems.OrderBy(p => p.Email)
                        : filteredListItems.OrderByDescending(p => p.Email);
                    break;
            }

            var displayedList = filteredListItems.Skip(param.start).Take(param.length);
            var requesters = new List<RequesterViewModel>();
            foreach (var requester in displayedList)
            {
                RequesterViewModel req = new RequesterViewModel();
                req.Id = requester.Id;
                req.Fullname = requester.Fullname;
                req.Email = requester.Email;
                req.DepartmentName = requester.DepartmentName;
                req.PhoneNumber = requester.PhoneNumber;
                req.JobTitle = requester.JobTitle;
                requesters.Add(req);
            }

            JqueryDatatableResultViewModel rsModel = new JqueryDatatableResultViewModel();
            rsModel.draw = param.draw;
            rsModel.recordsTotal = requesterList.Count();
            rsModel.recordsFiltered = filteredListItems.Count();
            rsModel.data = requesters;
            return Json(rsModel, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
