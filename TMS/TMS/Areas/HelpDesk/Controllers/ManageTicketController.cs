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
using log4net;
using Microsoft.AspNet.Identity;
using TMS.DAL;
using TMS.Enumerator;
using TMS.Models;
using TMS.Schedulers;
using TMS.Services;
using TMS.Utils;
using TMS.ViewModels;
using ModelError = TMS.ViewModels.ModelError;
using System.Threading.Tasks;
using System.Threading;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace TMS.Areas.HelpDesk.Controllers
{
    [CustomAuthorize(Roles = "Helpdesk")]
    public class ManageTicketController : Controller
    {

        private ILog log = LogManager.GetLogger(typeof(JobManager));

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

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CreateNewTicket()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddNewTicket(TicketViewModel model)
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

            if (string.IsNullOrEmpty(model.Subject) || model.Subject.Trim() == "")
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
                    msg = "Please select requester!",
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
            ticket.Status = (int)TicketStatusEnum.New;
            ticket.RequesterID = model.RequesterId;
            ticket.Subject = model.Subject.Trim();
            ticket.Description = model.Description;
            ticket.Solution = model.Solution;
            if (model.Type != 0) ticket.Type = model.Type;
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
                ticket.Status = (int)TicketStatusEnum.Assigned;
            }

            try
            {
                _ticketService.AddTicket(ticket);
                if (model.DescriptionFiles.ToList()[0] != null && model.DescriptionFiles.ToList().Count > 0)
                {
                    _ticketAttachmentService.saveFile(ticket.ID, model.DescriptionFiles, ConstantUtil.TicketAttachmentType.Description);
                }
                if (model.SolutionFiles.ToList()[0] != null && model.SolutionFiles.ToList().Count > 0)
                {
                    _ticketAttachmentService.saveFile(ticket.ID, model.SolutionFiles, ConstantUtil.TicketAttachmentType.Solution);
                }
            }
            catch (DbUpdateException ex)
            {
                log.Error("An error has occured while update ticket.", ex);
                return Json(new
                {
                    success = false,
                    msg = "Some error occured. Please try again later!"
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
            if (ticket == null || ticket.Status == ConstantUtil.TicketStatus.Cancelled
                || ticket.Status == ConstantUtil.TicketStatus.Closed
                || ticket.Status == ConstantUtil.TicketStatus.Unapproved)
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
            model.Mode = ticket.Mode;
            if (ticket.Type != null) model.Type = (int)ticket.Type;
            model.StatusId = ticket.Status;
            model.Status = ((TicketStatusEnum)ticket.Status).ToString();
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
            model.SolvedDate = ticket.SolvedDate?.ToString(ConstantUtil.DateTimeFormat) ?? "";
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

            IEnumerable<TicketAttachment> attachments = _ticketAttachmentService.GetAttachmentByTicketID(ticket.ID);
            if (attachments != null)
            {
                model.DescriptionAttachments = new List<AttachmentViewModel>();
                model.SolutionAttachments = new List<AttachmentViewModel>();
                foreach (var attachment in attachments)
                {
                    var att = new AttachmentViewModel();
                    att.id = attachment.ID;
                    att.name = TMSUtils.GetMinimizedAttachmentName(attachment.Filename);
                    if (attachment.Type == ConstantUtil.TicketAttachmentType.Description)
                    {
                        model.DescriptionAttachments.Add(att);
                    }
                    else
                    {
                        model.SolutionAttachments.Add(att);
                    }
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateTicket(TicketViewModel model)
        {
            ModelErrorViewModel errs = null;
            errs = new ModelErrorViewModel();
            Ticket ticket = _ticketService.GetTicketByID((int)model.Id);
            if (ticket == null)
            {
                return HttpNotFound();
            }

            if (string.IsNullOrEmpty(model.Subject) || model.Subject.Trim() == "")
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
                    msg = "Please select requester!",
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
                        Message = "Actual start date must before actual end date!",
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
            ticket.Subject = model.Subject.Trim();
            if (model.Type != 0) ticket.Type = model.Type;
            else ticket.Type = null;
            ticket.Description = model.Description;
            ticket.RequesterID = model.RequesterId;
            ticket.Solution = model.Solution;
            if (model.ImpactId != 0) ticket.ImpactID = model.ImpactId;
            else ticket.ImpactID = null;
            ticket.ImpactDetail = model.ImpactDetail;
            if (model.UrgencyId != 0) ticket.UrgencyID = model.UrgencyId;
            else ticket.UrgencyID = null;
            if (model.PriorityId != 0) ticket.PriorityID = model.PriorityId;
            else ticket.PriorityID = null;
            if (model.CategoryId != 0) ticket.CategoryID = model.CategoryId;
            else ticket.CategoryID = null;

            if (ticket.TechnicianID != model.TechnicianId)
            {
                ticket.TechnicianID = model.TechnicianId;
                ticket.AssignedByID = User.Identity.GetUserId();
                if ((ticket.Status == ConstantUtil.TicketStatus.New ||
                    ticket.Status == ConstantUtil.TicketStatus.Unapproved) && model.TechnicianId != null)
                {
                    ticket.Status = ConstantUtil.TicketStatus.Assigned;
                }
                else if (ticket.Status == ConstantUtil.TicketStatus.Assigned && model.TechnicianId == null)
                {
                    ticket.Status = ConstantUtil.TicketStatus.New;
                }
            }

            List<TicketAttachment> allTicketAttachments = _ticketAttachmentService.GetAttachmentByTicketID(ticket.ID).ToList();
            bool isDelete;
            for (int i = 0; i < allTicketAttachments.Count(); i++)
            {
                isDelete = true;
                if (allTicketAttachments[i].Type == ConstantUtil.TicketAttachmentType.Description)
                {
                    if (model.DescriptionAttachments != null && model.DescriptionAttachments.Count > 0)
                    {
                        for (int j = 0; j < model.DescriptionAttachments.Count; j++)
                        {
                            if (allTicketAttachments[i].ID == model.DescriptionAttachments[j].id)
                            {
                                isDelete = false;
                            }
                        }
                    }
                }
                else if (allTicketAttachments[i].Type == ConstantUtil.TicketAttachmentType.Solution)
                {
                    if (model.SolutionAttachments != null && model.SolutionAttachments.Count > 0)
                    {
                        for (int j = 0; j < model.SolutionAttachments.Count; j++)
                        {
                            if (allTicketAttachments[i].ID == model.SolutionAttachments[j].id)
                            {
                                isDelete = false;
                            }
                        }
                    }
                }
                if (isDelete) _ticketAttachmentService.DeleteAttachment(allTicketAttachments[i]);
            }

            try
            {
                _ticketService.UpdateTicket(ticket);
                if (model.DescriptionFiles.ToList()[0] != null && model.DescriptionFiles.ToList().Count > 0)
                {
                    _ticketAttachmentService.saveFile(ticket.ID, model.DescriptionFiles, ConstantUtil.TicketAttachmentType.Description);
                }
                if (model.SolutionFiles.ToList()[0] != null && model.SolutionFiles.ToList().Count > 0)
                {
                    _ticketAttachmentService.saveFile(ticket.ID, model.SolutionFiles, ConstantUtil.TicketAttachmentType.Solution);
                }
            }
            catch (DbUpdateException ex)
            {
                log.Error("An error has occured while update ticket.", ex);
                return Json(new
                {
                    success = false,
                    msg = ConstantUtil.CommonError.DBExceptionError
                });
            }

            return Json(new
            {
                success = true
            });
        }

        [HttpPost]
        public ActionResult CancelTicket(int? ticketId)
        {
            if (ticketId.HasValue)
            {
                Ticket ticket = _ticketService.GetTicketByID(ticketId.Value);
                if (ticket != null)
                {
                    if (ticket.Status != ConstantUtil.TicketStatus.New && ticket.Status != ConstantUtil.TicketStatus.Assigned)
                    {
                        return Json(new
                        {
                            success = false,
                            msg = "Ticket cannot be cancelled!"
                        });
                    }
                    try
                    {
                        int? status = ticket.Status;
                        _ticketService.CancelTicket(ticket);
                        if (status == ConstantUtil.TicketStatus.Assigned)
                        {
                            AspNetUser technician = _userService.GetUserById(ticket.TechnicianID);
                            Thread thread = new Thread(() => EmailUtil.SendToTechnicianWhenCancelTicket(ticket, technician));
                            thread.Start();
                        }
                        return Json(new
                        {
                            success = true,
                            msg = "Ticket was cancelled successfully!"
                        });
                    }
                    catch (Exception e)
                    {
                        log.Error("Cancel ticket error", e);
                        return Json(new
                        {
                            success = false,
                            msg = "Some error occured! Please try again later!"
                        });
                    }
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        msg = "This ticket is unavailable"
                    });
                }
            }
            else
            {
                return Json(new
                {
                    success = false,
                    msg = "This ticket is unavailable"
                });
            }
        }

        [HttpPost]
        public ActionResult CloseTicket(int? ticketId)
        {
            if (ticketId.HasValue)
            {
                Ticket ticket = _ticketService.GetTicketByID(ticketId.Value);
                if (ticket != null)
                {
                    if (ticket.Status != ConstantUtil.TicketStatus.Unapproved)
                    {
                        return Json(new
                        {
                            success = false,
                            msg = "Ticket cannot be closed!"
                        });
                    }
                    try
                    {
                        _ticketService.CloseTicket(ticket);
                        return Json(new
                        {
                            success = true,
                            msg = "Ticket was closed successfully!"
                        });
                    }
                    catch (Exception e)
                    {
                        log.Error("Close ticket error", e);
                        return Json(new
                        {
                            success = false,
                            msg = "Some error occured! Please try again later!"
                        });
                    }
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        msg = "This ticket is unavailable"
                    });
                }
            }
            else
            {
                return Json(new
                {
                    success = false,
                    msg = "This ticket is unavailable"
                });
            }
        }

        [HttpPost]
        public ActionResult MergeTicket(int[] selectedTickets)
        {
            if (selectedTickets.Length < 2)
            {
                return Json(new
                {
                    success = false,
                    msg = "Less than 2 tickets, can not merge!"
                });
            }
            List<Ticket> tickets = new List<Ticket>();
            string requesterId = "";
            for (int i = 0; i < selectedTickets.Length; i++)
            {
                Ticket ticket = _ticketService.GetTicketByID(selectedTickets[i]);
                if (ticket != null)
                {
                    if (i > 0 && !requesterId.Equals(ticket.RequesterID))
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
            try
            {
                tickets.Sort((x, y) => DateTime.Compare(x.CreatedTime, y.CreatedTime));
                Ticket newTicket = tickets[0];
                for (int i = 1; i < tickets.Count; i++)
                {
                    Ticket oldTicket = tickets[i];
                    if (oldTicket.Status != ConstantUtil.TicketStatus.New && oldTicket.Status != ConstantUtil.TicketStatus.Assigned)
                    {
                        return Json(new
                        {
                            success = false,
                            msg = "There are some children tickets which cannot be merged! \nOnly New or Assigned children tickets can be merged!"
                        });
                    }
                }
                for (int i = 1; i < tickets.Count; i++)
                {
                    Ticket oldTicket = tickets[i];
                    if (!string.IsNullOrWhiteSpace(newTicket.Description))
                    {
                        newTicket.Description += "\n\n[Merged from ticket #" + oldTicket.Code + "]:\n" + oldTicket.Description;
                    }
                    else
                    {
                        newTicket.Description = "[Merged from ticket #" + oldTicket.Code + "]:\n" + oldTicket.Description;
                    }
                    oldTicket.ModifiedTime = DateTime.Now;
                    int? status = oldTicket.Status = ConstantUtil.TicketStatus.Cancelled;
                    _ticketService.UpdateTicket(oldTicket);
                    if (status == ConstantUtil.TicketStatus.Assigned)
                    {
                        AspNetUser technician = _userService.GetUserById(oldTicket.TechnicianID);
                        Thread thread = new Thread(() => EmailUtil.SendToTechnicianWhenCancelTicket(oldTicket, technician));
                        thread.Start();
                    }
                }
                newTicket.ModifiedTime = DateTime.Now;
                _ticketService.UpdateTicket(newTicket);

                return Json(new
                {
                    success = true,
                    msg = string.Format("Tickets were merged into ticket #{0}!", newTicket.Code)
                });
            }
            catch
            {
                return Json(new
                {
                    success = false,
                    msg = "Some error occured! Please try again later!"
                });
            }
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
                else
                {
                    //Hide Canceled and Closed Tickets
                    filteredListItems = filteredListItems.Where(p => p.Status != (int)TicketStatusEnum.Canceled);
                    filteredListItems = filteredListItems.Where(p => p.Status != (int)TicketStatusEnum.Closed);
                }
            }

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
                case 2:
                    filteredListItems = sortDirection == "asc"
                        ? filteredListItems.OrderBy(p => _userService.GetUserById(p.RequesterID).Fullname)
                        : filteredListItems.OrderByDescending(p => _userService.GetUserById(p.RequesterID).Fullname);
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
                s.SolvedDate = item.SolvedDate?.ToString(ConstantUtil.DateTimeFormat) ?? "";
                s.Status = ((TicketStatusEnum)item.Status).ToString();
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

        [HttpGet]
        public ActionResult LoadTicketById(int? id)
        {
            if (id.HasValue)
            {
                Ticket ticket = _ticketService.GetTicketByID(id.Value);
                if (ticket != null)
                {
                    TicketViewModel model = new TicketViewModel();
                    model.Subject = ticket.Subject;
                    model.Description = ticket.Description;
                    model.Solution = ticket.Solution;
                    model.RequesterId = ticket.RequesterID;
                    model.Requester = _userService.GetUserById(ticket.RequesterID).Fullname;
                    if (ticket.Type.HasValue)
                    {
                        model.Type = ticket.Type.Value;
                    }
                    model.Mode = ticket.Mode;

                    if (ticket.CategoryID.HasValue)
                    {
                        model.CategoryId = ticket.CategoryID.Value;
                        model.Category = _categoryService.GetCategoryById(ticket.CategoryID.Value).Name;
                    }
                    if (ticket.UrgencyID.HasValue)
                    {
                        model.UrgencyId = ticket.UrgencyID.Value;
                        model.Urgency = _urgencyService.GetUrgencyByID(ticket.UrgencyID.Value).Name;
                    }
                    if (ticket.PriorityID.HasValue)
                    {
                        model.PriorityId = ticket.PriorityID.Value;
                        model.Priority = _priorityService.GetPriorityByID(ticket.PriorityID.Value).Name;
                    }
                    if (ticket.ImpactID.HasValue)
                    {
                        model.ImpactId = ticket.ImpactID.Value;
                        model.Impact = _impactService.GetImpactById(ticket.ImpactID.Value).Name;
                    }
                    model.ImpactDetail = ticket.ImpactDetail;
                    model.ScheduleStartDate = ticket.ScheduleStartDate?.ToString(ConstantUtil.DateTimeFormat) ?? "";
                    model.ScheduleEndDate = ticket.ScheduleEndDate?.ToString(ConstantUtil.DateTimeFormat) ?? "";
                    model.ActualStartDate = ticket.ActualStartDate?.ToString(ConstantUtil.DateTimeFormat) ?? "";
                    model.ActualEndDate = ticket.ActualEndDate?.ToString(ConstantUtil.DateTimeFormat) ?? "";

                    if (!string.IsNullOrEmpty(ticket.TechnicianID))
                    {
                        AspNetUser technician = _userService.GetActiveUserById(ticket.TechnicianID);
                        if (technician != null)
                        {
                            model.TechnicianId = technician.Id;
                            model.Technician = technician.Fullname;
                            if (technician.DepartmentID.HasValue)
                            {
                                model.DepartmentId = technician.DepartmentID.Value;
                                model.Department = _departmentService.GetDepartmentById(technician.DepartmentID.Value).Name;
                            }
                        }
                    }
                    return Json(new
                    {
                        success = true,
                        data = model,
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new
            {
                success = false,
                message = ConstantUtil.CommonError.UnavailableTicket,
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

        [HttpGet]
        public ActionResult GetTicketDetailForReassign(int? ticketId)
        {
            if (ticketId.HasValue)
            {
                Ticket ticket = _ticketService.GetTicketByID(ticketId.Value);
                if (ticket != null)
                {
                    AspNetUser technician = _userService.GetUserById(ticket.TechnicianID);
                    if (technician != null)
                    {
                        return Json(new
                        {
                            success = true,
                            technicianId = ticket.TechnicianID,
                            technician = technician.Fullname,
                            departmentId = technician.DepartmentID,
                            department = technician.Department.Name
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new
                        {
                            success = true,
                            technicianId = "",
                            technician = "",
                            departmentId = "",
                            department = ""
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json(new
            {
                success = false,
                message = "This ticket is unavailable"
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Reassign(string technicianId, int? ticketId)
        {
            if (ticketId.HasValue)
            {
                AspNetUser technician = _userService.GetActiveUserById(technicianId);
                if (technician != null)
                {
                    Ticket ticket = _ticketService.GetTicketByID(ticketId.Value);
                    if (ticket != null)
                    {
                        if (ticket.Status == ConstantUtil.TicketStatus.Unapproved)
                        {
                            try
                            {
                                ticket.TechnicianID = technicianId;
                                ticket.Status = ConstantUtil.TicketStatus.Assigned;
                                ticket.ModifiedTime = DateTime.Now;
                                _ticketService.UpdateTicket(ticket);
                                Thread thread = new Thread(() => EmailUtil.SendToTechnicianWhenAssignTicket(ticket, technician));
                                thread.Start();
                                return Json(new
                                {
                                    success = true,
                                    message = "Ticket was reassigned successfully!"
                                });
                            }
                            catch
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
                                message = ConstantUtil.CommonError.InvalidTicket
                            });
                        }
                    }
                    else
                    {
                        return Json(new
                        {
                            success = false,
                            message = ConstantUtil.CommonError.UnavailableTicket
                        });
                    }
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = ConstantUtil.CommonError.UnavailableTechnician
                    });
                }
            }
            else
            {
                return Json(new
                {
                    success = false,
                    message = ConstantUtil.CommonError.UnavailableTicket
                });
            }
        }

        [HttpGet]
        public ActionResult GetOlderTickets(JqueryDatatableParameterViewModel param, string keywords)
        {
            IEnumerable<Ticket> olderTickets = _ticketService.GetOlderTickets();

            IQueryable<Ticket> filteredListItems = olderTickets.AsQueryable();
            if (!string.IsNullOrEmpty(param.search["value"]))
            {
                filteredListItems = filteredListItems.Where(p => p.Code != null && (p.Code.ToLower().Contains(param.search["value"].ToLower())
                    || p.Subject.ToLower().Equals(param.search["value"].ToLower())));
            }

            if (!string.IsNullOrWhiteSpace(keywords))
            {
                keywords = GeneralUtil.RemoveSpecialCharacters(keywords);
                Regex regex = new Regex("[ ]{2,}", RegexOptions.None);
                keywords = regex.Replace(keywords, " ");
                string[] keywordArr = keywords.Split(' ');
                var predicate = PredicateBuilder.False<Ticket>();
                foreach (string keyword in keywordArr)
                {
                    predicate = predicate.Or(p => p.Subject.ToLower().Contains(keyword.ToLower()));
                }
                filteredListItems = filteredListItems.Where(predicate);
            }

            // Sort.
            var sortColumnIndex = Convert.ToInt32(param.order[0]["column"]);
            var sortDirection = param.order[0]["dir"];

            switch (sortColumnIndex)
            {
                case 0:
                    filteredListItems = sortDirection == "asc"
                        ? filteredListItems.OrderBy(p => p.Code)
                        : filteredListItems.OrderByDescending(p => p.Code);
                    break;
                case 1:
                    filteredListItems = sortDirection == "asc"
                        ? filteredListItems.OrderBy(p => p.Subject)
                        : filteredListItems.OrderByDescending(p => p.Subject);
                    break;
            }

            var displayedList = filteredListItems.Skip(param.start).Take(param.length).Select(m => new Ticket
            {
                ID = m.ID,
                Code = m.Code,
                Subject = m.Subject,
                Description = m.Description
            });

            JqueryDatatableResultViewModel rsModel = new JqueryDatatableResultViewModel();
            rsModel.draw = param.draw;
            rsModel.recordsTotal = displayedList.ToList().Count();
            rsModel.recordsFiltered = filteredListItems.Count();
            rsModel.data = displayedList;
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
