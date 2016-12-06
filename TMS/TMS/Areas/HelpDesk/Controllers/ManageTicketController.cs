using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using log4net;
using Microsoft.AspNet.Identity;
using TMS.DAL;
using TMS.Models;
using TMS.Schedulers;
using TMS.Services;
using TMS.Utils;
using TMS.ViewModels;
using System.Threading;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using iTextSharp.text.pdf.qrcode;
using Microsoft.Ajax.Utilities;

namespace TMS.Areas.HelpDesk.Controllers
{
    [CustomAuthorize(Roles = "Helpdesk")]
    public class ManageTicketController : Controller
    {
        private ILog log = LogManager.GetLogger(typeof(ManageTicketController));

        public TicketService _ticketService { get; set; }
        public UserService _userService { get; set; }
        public GroupService _groupService { get; set; }
        public UrgencyService _urgencyService { get; set; }
        public PriorityService _priorityService { get; set; }
        public ImpactService _impactService { get; set; }
        public CategoryService _categoryService { get; set; }
        public TicketAttachmentService _ticketAttachmentService { get; set; }
        public KeywordService _keywordService { get; set; }
        private UnitOfWork unitOfWork = new UnitOfWork();

        public ManageTicketController()
        {
            _ticketService = new TicketService(unitOfWork);
            _userService = new UserService(unitOfWork);
            _groupService = new GroupService(unitOfWork);
            _urgencyService = new UrgencyService(unitOfWork);
            _priorityService = new PriorityService(unitOfWork);
            _impactService = new ImpactService(unitOfWork);
            _categoryService = new CategoryService(unitOfWork);
            _ticketAttachmentService = new TicketAttachmentService(unitOfWork);
            _keywordService = new KeywordService(unitOfWork);
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CreateNewTicket()
        {
            Impact impact = _impactService.GetSystemImpact();
            Urgency urgency = _urgencyService.GetSystemUrgency();
            if (impact == null || urgency == null)
            {
                return HttpNotFound();
            }

            ViewBag.Impact = impact.Name;
            ViewBag.ImpactId = impact.ID;
            ViewBag.Urgency = urgency.Name;
            ViewBag.UrgencyId = urgency.ID;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddNewTicket(TicketViewModel model)
        {
            if (!ModelState.IsValid)
            {
                foreach (ModelState modelState in ViewData.ModelState.Values)
                {
                    foreach (System.Web.Mvc.ModelError error in modelState.Errors)
                    {
                        return Json(new
                        {
                            success = false,
                            msg = error.ErrorMessage
                        });
                    }
                }
            }
            else
            {
                var ticket = new Ticket();
                ticket.Subject = model.Subject.Trim();
                ticket.Description = model.Description;
                ticket.ScheduleStartDate = model.ScheduleStartDate;
                ticket.ScheduleEndDate = model.DueByDate.AddDays(ConstantUtil.DayToCloseTicket);
                ticket.DueByDate = model.DueByDate;
                ticket.CreatedTime = DateTime.Now;
                ticket.ModifiedTime = DateTime.Now;
                ticket.Status = ConstantUtil.TicketStatus.Open;
                ticket.RequesterID = model.RequesterId;
                ticket.Solution = model.Solution;
                ticket.CreatedID = User.Identity.GetUserId();
                if (model.Type != 0) ticket.Type = model.Type;
                ticket.Mode = model.Mode;
                ticket.ImpactID = model.ImpactId;
                ticket.ImpactDetail = model.ImpactDetail;
                ticket.UrgencyID = _ticketService.GetUrgencyId(ticket.DueByDate);
                ticket.PriorityID = _ticketService.GetPriorityId(ticket.ImpactID, ticket.DueByDate);
                if (model.CategoryId != 0) ticket.CategoryID = model.CategoryId;
                ticket.TicketKeywords = _keywordService.GetTicketKeywordsForCreate(model.Keywords);
                ticket.Note = model.Note;

                string assignTechId = model.TechnicianId;
                if (string.IsNullOrEmpty(model.TechnicianId) && model.GroupId > 0)
                {
                    assignTechId = _userService.GetFreeTechnicianIdByGroup(model.GroupId);
                }
                if (!string.IsNullOrWhiteSpace(assignTechId))
                {
                    ticket.TechnicianID = assignTechId;
                    ticket.AssignedByID = User.Identity.GetUserId();
                    ticket.Status = ConstantUtil.TicketStatus.Assigned;
                }

                bool result = _ticketService.AddTicket(ticket);
                if (result)
                {
                    if (model.DescriptionFiles.ToList()[0] != null && model.DescriptionFiles.ToList().Count > 0)
                    {
                        result = _ticketAttachmentService.saveFile(ticket.ID, model.DescriptionFiles, ConstantUtil.TicketAttachmentType.Description);
                        if (!result)
                        {
                            return Json(new
                            {
                                success = false,
                                msg = ConstantUtil.CommonError.DBExceptionError
                            });
                        }
                    }
                    if (model.SolutionFiles.ToList()[0] != null && model.SolutionFiles.ToList().Count > 0)
                    {
                        result = _ticketAttachmentService.saveFile(ticket.ID, model.SolutionFiles, ConstantUtil.TicketAttachmentType.Solution);
                        if (!result)
                        {
                            return Json(new
                            {
                                success = false,
                                msg = ConstantUtil.CommonError.DBExceptionError
                            });
                        }
                    }
                    AspNetUser requester = _userService.GetUserById(ticket.RequesterID);
                    if (requester != null)
                    {
                        Thread thread = new Thread(() => EmailUtil.SendToRequesterWhenCreateTicket(ticket, requester));
                        thread.Start();
                    }
                }
                else
                {
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
            return Json(new
            {
                success = false,
                msg = ConstantUtil.CommonError.DBExceptionError
            });
        }

        public ActionResult EditTicket(int? id)
        {
            if (!id.HasValue)
            {
                return HttpNotFound();
            }
            else
            {
                Ticket ticket = _ticketService.GetTicketByID(id.Value);
                if (ticket == null || ticket.Status == ConstantUtil.TicketStatus.Cancelled
                        || ticket.Status == ConstantUtil.TicketStatus.Closed)
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
                if (ticket.Type.HasValue) model.Type = ticket.Type.Value;
                model.Status = GeneralUtil.GetTicketStatusByID(ticket.Status);
                model.StatusId = ticket.Status;
                if (ticket.CategoryID.HasValue)
                {
                    Category category = _categoryService.GetCategoryById(ticket.CategoryID.Value);
                    if (category != null)
                    {
                        model.CategoryId = category.ID;
                        model.Category = category.Name;
                    }
                }
                model.UrgencyId = ticket.UrgencyID;
                model.Urgency = ticket.Urgency.Name;
                model.PriorityId = ticket.PriorityID;
                model.Priority = ticket.Priority.Name;
                model.PriorityColor = ticket.Priority.Color;
                model.ImpactId = ticket.ImpactID;
                model.Impact = ticket.Impact.Name;
                model.ImpactDetail = ticket.ImpactDetail;
                model.ScheduleStartDate = ticket.ScheduleStartDate;
                model.DueByDate = ticket.DueByDate;
                model.SolvedDate = ticket.SolvedDate;
                model.CreatedTime = ticket.CreatedTime;
                model.ModifiedTime = ticket.ModifiedTime;
                model.Keywords = _keywordService.GetTicketKeywordForDisplay(ticket.ID);
                model.Note = ticket.Note;

                if (!string.IsNullOrEmpty(ticket.CreatedID))
                {
                    model.CreatedId = ticket.CreatedID;
                    model.CreatedBy = _userService.GetUserById(ticket.CreatedID).Fullname;
                }
                if (!string.IsNullOrEmpty(ticket.SolvedID))
                {
                    model.SolvedId = ticket.SolvedID;
                    model.SolvedBy = _userService.GetUserById(ticket.SolvedID).Fullname;
                }
                if (!string.IsNullOrEmpty(ticket.TechnicianID))
                {
                    AspNetUser technician = _userService.GetUserById(ticket.TechnicianID);
                    model.TechnicianId = technician.Id;
                    model.Technician = technician.Fullname;
                    if (technician.GroupID.HasValue)
                    {
                        if (technician.Group != null)
                        {
                            model.GroupId = technician.GroupID.Value;
                            model.Group = technician.Group.Name;
                        }
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
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateTicket(TicketViewModel model)
        {
            if (!ModelState.IsValid)
            {
                foreach (ModelState modelState in ViewData.ModelState.Values)
                {
                    foreach (System.Web.Mvc.ModelError error in modelState.Errors)
                    {
                        return Json(new
                        {
                            success = false,
                            msg = error.ErrorMessage
                        });
                    }
                }
            }
            else
            {
                Ticket ticket = _ticketService.GetTicketByID(model.Id);
                ticket.ScheduleStartDate = model.ScheduleStartDate;
                ticket.DueByDate = model.DueByDate;
                ticket.ScheduleEndDate = model.DueByDate.AddDays(ConstantUtil.DayToCloseTicket);
                ticket.ModifiedTime = DateTime.Now;
                ticket.Subject = model.Subject.Trim();
                ticket.Mode = model.Mode;
                if (model.Type != 0)
                {
                    ticket.Type = model.Type;
                }
                else
                {
                    ticket.Type = null;
                }
                ticket.Description = model.Description;
                ticket.RequesterID = model.RequesterId;
                ticket.Solution = model.Solution;
                ticket.TicketKeywords = _keywordService.GetTicketKeywordsForEdit(model.Keywords, ticket.ID);
                ticket.Note = model.Note;
                ticket.ImpactID = model.ImpactId;
                ticket.ImpactDetail = model.ImpactDetail;
                ticket.UrgencyID = _ticketService.GetUrgencyId(ticket.DueByDate);
                ticket.PriorityID = _ticketService.GetPriorityId(ticket.ImpactID, ticket.DueByDate);
                if (model.CategoryId != 0)
                {
                    ticket.CategoryID = model.CategoryId;
                }
                else
                {
                    ticket.CategoryID = null;
                }

                if (string.IsNullOrWhiteSpace(model.TechnicianId) && model.GroupId > 0)
                {
                    string assignedTechId = _userService.GetFreeTechnicianIdByGroup(model.GroupId);
                    if (!string.IsNullOrWhiteSpace(assignedTechId))
                    {
                        model.TechnicianId = assignedTechId;
                    }
                }

                if (ticket.TechnicianID != model.TechnicianId)
                {
                    if (string.IsNullOrWhiteSpace(model.TechnicianId))
                    {
                        ticket.TechnicianID = null;
                    }
                    else
                    {
                        ticket.TechnicianID = model.TechnicianId;
                        ticket.AssignedByID = User.Identity.GetUserId();
                    }
                    if ((ticket.Status == ConstantUtil.TicketStatus.Open ||
                         ticket.Status == ConstantUtil.TicketStatus.Unapproved) && ticket.TechnicianID != null)
                    {
                        ticket.Status = ConstantUtil.TicketStatus.Assigned;
                    }
                    else if (ticket.Status == ConstantUtil.TicketStatus.Assigned && ticket.TechnicianID == null)
                    {
                        ticket.Status = ConstantUtil.TicketStatus.Open;
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

                bool result = _ticketService.UpdateTicket(ticket, User.Identity.GetUserId());
                if (result)
                {
                    if (model.DescriptionFiles != null && model.DescriptionFiles.ToList()[0] != null && model.DescriptionFiles.ToList().Count > 0)
                    {
                        result = _ticketAttachmentService.saveFile(ticket.ID, model.DescriptionFiles, ConstantUtil.TicketAttachmentType.Description);
                        if (!result)
                        {
                            return Json(new
                            {
                                success = false,
                                msg = ConstantUtil.CommonError.DBExceptionError
                            });
                        }
                    }
                    if (model.SolutionFiles != null && model.SolutionFiles.ToList()[0] != null && model.SolutionFiles.ToList().Count > 0)
                    {
                        result = _ticketAttachmentService.saveFile(ticket.ID, model.SolutionFiles, ConstantUtil.TicketAttachmentType.Solution);
                        if (!result)
                        {
                            return Json(new
                            {
                                success = false,
                                msg = ConstantUtil.CommonError.DBExceptionError
                            });
                        }
                    }
                    return Json(new
                    {
                        success = true
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        msg = ConstantUtil.CommonError.DBExceptionError
                    });
                }
            }

            return Json(new
            {
                success = false,
                msg = ConstantUtil.CommonError.DBExceptionError
            });
        }

        [HttpGet]
        public ActionResult GetDueByDate(string scheduleStartDate, int? urgencyId)
        {
            DateTime? scheduleStartDateDatetime = null;
            DateTime temp;
            bool parseResult = DateTime.TryParseExact(scheduleStartDate, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out temp);
            if (parseResult)
            {
                scheduleStartDateDatetime = temp;
            }
            if (scheduleStartDateDatetime.HasValue && urgencyId.HasValue)
            {
                Urgency urgency = _urgencyService.GetUrgencyByID(urgencyId.Value);
                if (urgency != null)
                {
                    string dueByDate = scheduleStartDateDatetime.Value.AddHours(urgency.Duration).ToString(ConstantUtil.DateTimeFormat);
                    return Json(new
                    {
                        success = true,
                        dueByDate = dueByDate
                    }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new
            {
                success = false
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetImpactUrgencyByCategory(int? categoryId)
        {
            if (categoryId.HasValue)
            {
                Category category = _categoryService.GetCategoryById(categoryId.Value);
                if (category != null)
                {
                    if (category.Impact != null && category.Urgency != null)
                    {
                        return Json(new
                        {
                            success = true,
                            impact = category.Impact.Name,
                            urgency = category.Urgency.Name,
                            impactId = category.ImpactID,
                            urgencyId = category.UrgencyID
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
            }

            return Json(new
            {
                success = false
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetPriority(int? impactId, int? urgencyId)
        {
            if (impactId.HasValue && urgencyId.HasValue)
            {
                Priority priority = _ticketService.GetPriority(impactId.Value, urgencyId.Value);
                return Json(new
                {
                    success = true,
                    priority = priority.Name,
                    priorityColor = priority.Color
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                success = false
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CancelTicket(int? ticketId)
        {
            if (ticketId.HasValue)
            {
                Ticket ticket = _ticketService.GetTicketByID(ticketId.Value);
                if (ticket != null)
                {
                    if (ticket.Status != ConstantUtil.TicketStatus.Open && ticket.Status != ConstantUtil.TicketStatus.Assigned)
                    {
                        return Json(new
                        {
                            success = false,
                            msg = "Ticket cannot be cancelled!"
                        });
                    }

                    int status = ticket.Status;
                    bool cancelResult = _ticketService.CancelTicket(ticket, User.Identity.GetUserId());
                    if (cancelResult)
                    {
                        if (status == ConstantUtil.TicketStatus.Assigned)
                        {
                            AspNetUser technician = _userService.GetUserById(ticket.TechnicianID);
                            Thread thread = new Thread(() => EmailUtil.SendToTechnicianWhenCancelTicket(ticket, technician));
                            thread.Start();
                        }

                        AspNetUser requester = _userService.GetUserById(ticket.RequesterID);
                        if (requester != null)
                        {
                            Thread thread = new Thread(() => EmailUtil.SendToRequesterWhenCancelTicket(ticket, requester));
                            thread.Start();
                        }

                        return Json(new
                        {
                            success = true,
                            msg = "Ticket was cancelled successfully!"
                        });
                    }
                    else
                    {
                        return Json(new
                        {
                            success = false,
                            msg = ConstantUtil.CommonError.DBExceptionError
                        });
                    }
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        msg = ConstantUtil.CommonError.UnavailableTicket
                    });
                }
            }
            else
            {
                return Json(new
                {
                    success = false,
                    msg = ConstantUtil.CommonError.UnavailableTicket
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

                    bool closeResult = _ticketService.CloseTicket(ticket, User.Identity.GetUserId());
                    if (closeResult)
                    {
                        return Json(new
                        {
                            success = true,
                            msg = "Ticket was closed successfully!"
                        });
                    }
                    else
                    {
                        return Json(new
                        {
                            success = false,
                            msg = ConstantUtil.CommonError.DBExceptionError
                        });
                    }
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        msg = ConstantUtil.CommonError.UnavailableTicket
                    });
                }
            }
            else
            {
                return Json(new
                {
                    success = false,
                    msg = ConstantUtil.CommonError.UnavailableTicket
                });
            }
        }

        [HttpPost]
        public ActionResult MergeTicket(int[] selectedTickets)
        {
            if (selectedTickets == null || selectedTickets.Length < 2)
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

            tickets.Sort((x, y) => DateTime.Compare(x.CreatedTime, y.CreatedTime));
            Ticket newTicket = tickets[0];
            for (int i = 1; i < tickets.Count; i++)
            {
                Ticket oldTicket = tickets[i];
                if (oldTicket.Status != ConstantUtil.TicketStatus.Open && oldTicket.Status != ConstantUtil.TicketStatus.Assigned)
                {
                    return Json(new
                    {
                        success = false,
                        msg = "There are some children tickets which cannot be merged! \nOnly Open or Assigned children tickets can be merged!"
                    });
                }
            }
            bool mergeResult = _ticketService.MergeTicket(tickets, User.Identity.GetUserId());
            if (mergeResult)
            {
                return Json(new
                {
                    success = true,
                    msg = string.Format("Tickets were merged successfully into ticket #{0}!", newTicket.Code)
                });
            }
            else
            {
                return Json(new
                {
                    success = false,
                    msg = ConstantUtil.CommonError.DBExceptionError
                });
            }
        }

        [HttpPost]
        public ActionResult LoadAllTickets(JqueryDatatableParameterViewModel param)
        {
            var searchText = Request["filter_search"];
            var createdFilter = Request["filter_created"];
            var sortFilter = Request["filter_sort"];
            var duebyFilter = Request["filter_dueby"];
            var statusFilter = Request["filter_status"];
            var modeFilter = Request["filter_mode"];
            var requesterFilter = Request["filter_requester"];

            object[] duebyFilterItems = null;
            object[] statusFilterItems = null;
            object[] modeFilterItems = null;
            object[] requesterFilterItems = null;

            var js = new JavaScriptSerializer();
            if (duebyFilter != null)
            {
                duebyFilterItems = (object[])js.DeserializeObject(duebyFilter);
            }
            if (statusFilter != null)
            {
                statusFilterItems = (object[])js.DeserializeObject(Request["filter_status"]);
            }
            if (modeFilter != null)
            {
                modeFilterItems = (object[])js.DeserializeObject(Request["filter_mode"]);
            }
            if (requesterFilter != null)
            {
                requesterFilterItems = (object[])js.DeserializeObject(Request["filter_requester"]);
            }

            var queriedResult = _ticketService.GetAll();
            IEnumerable<Ticket> filteredListItems;

            // Search by subject
            if (!string.IsNullOrEmpty(searchText))
            {
                filteredListItems = queriedResult.Where(p => p.Subject.ToLower().Contains(searchText.ToLower()) 
                                        || p.Code.ToLower().Contains(searchText.ToLower()));
            }
            else
            {
                filteredListItems = queriedResult;
            }

            // Filter by created time
            if (!string.IsNullOrEmpty(createdFilter))
            {
                switch (createdFilter)
                {
                    case "5":
                    case "15":
                    case "30":
                    case "60":
                    case "240":
                    case "720":
                    case "1440":
                        filteredListItems = filteredListItems.Where(p => (DateTime.Now - p.CreatedTime).TotalMinutes <= Double.Parse(createdFilter));
                        break;
                    case "today":
                        filteredListItems = filteredListItems.Where(p => p.CreatedTime.Date == DateTime.Today);
                        break;
                    case "yesterday":
                        filteredListItems = filteredListItems.Where(p => p.CreatedTime.Date == DateTime.Today.AddDays(-1));
                        break;
                    case "week":
                        if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
                        {
                            filteredListItems = filteredListItems.Where(p => DateTime.Now.Date.AddDays(-6).Date <= p.CreatedTime.Date
                            && p.CreatedTime.Date <= DateTime.Now.Date);
                        }
                        else
                        {
                            filteredListItems = filteredListItems.Where(p => DateTime.Now.Date.AddDays(DayOfWeek.Monday - DateTime.Now.DayOfWeek).Date <= p.CreatedTime.Date
                            && p.CreatedTime.Date <= DateTime.Now.Date.AddDays(DayOfWeek.Sunday - DateTime.Now.DayOfWeek + 7).Date);
                        }
                        break;
                    case "last_week":
                        filteredListItems = filteredListItems.Where(p => p.CreatedTime.Date >= DateTime.Now.Date.AddDays(-7).Date
                        && p.CreatedTime.Date <= DateTime.Now.Date);
                        break;
                    case "month":
                        filteredListItems = filteredListItems.Where(p => p.CreatedTime.Month == DateTime.Now.Month
                        && p.CreatedTime.Year == DateTime.Now.Year);
                        break;
                    case "last_month":
                        filteredListItems = filteredListItems.Where(p => p.CreatedTime.Month == DateTime.Now.AddMonths(-1).Month
                       && p.CreatedTime.Year == DateTime.Now.Year);
                        break;
                    case "two_months":
                        filteredListItems = filteredListItems.Where(p => p.CreatedTime >= DateTime.Now.AddMonths(-2)
                        && p.CreatedTime.Year == DateTime.Now.Year);
                        break;
                    case "six_months":
                        filteredListItems = filteredListItems.Where(p => p.CreatedTime >= DateTime.Now.AddMonths(-6)
                        && p.CreatedTime.Year == DateTime.Now.Year);
                        break;
                    case "set_date":
                        var timePeriodFilter = Request["filter_time_period"];
                        if (!string.IsNullOrEmpty(timePeriodFilter))
                        {
                            timePeriodFilter = timePeriodFilter.Replace(" ", "");
                            string[] daterange = timePeriodFilter.Split('-');
                            if (daterange.Length == 2)
                            {
                                var startDate = DateTime.ParseExact(daterange[0], ConstantUtil.DateFormat, new DateTimeFormatInfo());
                                var endDate = DateTime.ParseExact(daterange[1], ConstantUtil.DateFormat,
                                    new DateTimeFormatInfo());
                                filteredListItems = filteredListItems.Where(p => p.CreatedTime.Date >= startDate
                                                                            && p.CreatedTime.Date <= endDate);
                            }
                        }
                        break;
                }
            }

            // Filter by status
            if (duebyFilterItems != null)
            {
                filteredListItems =
                    filteredListItems.Where(
                        p => (duebyFilterItems.Contains("Overdue") && GeneralUtil.IsOverdue(p.DueByDate, p.Status))
                        || (duebyFilterItems.Contains("Today") && p.DueByDate.Date == DateTime.Today && (p.Status == ConstantUtil.TicketStatus.Open || p.Status == ConstantUtil.TicketStatus.Assigned))
                        || (duebyFilterItems.Contains("Tomorrow") && p.DueByDate.Date == DateTime.Today.AddDays(1) && (p.Status == ConstantUtil.TicketStatus.Open || p.Status == ConstantUtil.TicketStatus.Assigned))
                        || (duebyFilterItems.Contains("Next_8_hours") && p.DueByDate >= DateTime.Now && p.ScheduleEndDate <= DateTime.Now.AddHours(8) && (p.Status == ConstantUtil.TicketStatus.Open || p.Status == ConstantUtil.TicketStatus.Assigned))
                        );
            }

            // Filter by status
            if (statusFilterItems != null)
            {
                filteredListItems = filteredListItems.Where(p => statusFilterItems.Contains(p.Status.ToString())
                || (statusFilterItems.Contains("0") && p.Status != ConstantUtil.TicketStatus.Cancelled && p.Status != ConstantUtil.TicketStatus.Closed));
            }

            // Filter by mode
            if (modeFilterItems != null)
            {
                filteredListItems = filteredListItems.Where(p => modeFilterItems.Contains(p.Mode.ToString()));
            }

            // Filter by requester
            if (requesterFilterItems != null)
            {
                filteredListItems = filteredListItems.Where(p => requesterFilter.Contains(p.RequesterID.ToString()));
            }

            if (!string.IsNullOrEmpty(sortFilter))
            {
                switch (sortFilter)
                {
                    case "SubjectAsc": filteredListItems = filteredListItems.OrderBy(p => p.Subject); break;
                    case "SubjectDsc": filteredListItems = filteredListItems.OrderByDescending(p => p.Subject); break;
                    case "PriorityAsc": filteredListItems = filteredListItems.OrderBy(p => p.Priority.PriorityLevel); break;
                    case "PriorityDsc": filteredListItems = filteredListItems.OrderByDescending(p => p.Priority.PriorityLevel); break;
                    case "DueDateAsc": filteredListItems = filteredListItems.OrderBy(p => p.DueByDate); break;
                    case "DueDateDsc": filteredListItems = filteredListItems.OrderByDescending(p => p.DueByDate); break;
                    case "CreatedDateAsc": filteredListItems = filteredListItems.OrderBy(p => p.CreatedTime); break;
                    case "CreatedDateDsc": filteredListItems = filteredListItems.OrderByDescending(p => p.CreatedTime); break;
                    case "ModifiedDateAsc": filteredListItems = filteredListItems.OrderBy(p => p.ModifiedTime); break;
                    default: filteredListItems = filteredListItems.OrderByDescending(p => p.ModifiedTime); break;
                }
            }

            var result = filteredListItems.Skip(param.start).Take(param.length).ToList();
            var tickets = new List<TicketViewModel>();
            int startNo = param.start;
            foreach (var item in result)
            {
                var s = new TicketViewModel();
                s.No = ++startNo;
                s.Code = item.Code;
                s.Id = item.ID;
                s.Subject = item.Subject;
                s.CreatedBy = item.CreatedID == null ? "" : _userService.GetUserById(item.CreatedID).Fullname;
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
                s.SolvedDateString = item.SolvedDate.HasValue ? item.SolvedDate.Value.ToString(ConstantUtil.DateTimeFormat) : "-";
                s.Status = GeneralUtil.GetTicketStatusByID(item.Status);
                s.CreatedTimeString = GeneralUtil.ShowDateTime(item.CreatedTime);
                s.ModifiedTimeString = GeneralUtil.ShowDateTime(item.ModifiedTime);
                s.OverdueDateString = GeneralUtil.GetOverdueDate(item.DueByDate, item.Status);
                s.IsOverdue = GeneralUtil.IsOverdue(item.DueByDate, item.Status);
                s.Priority = item.Priority == null ? "" : item.Priority.Name;
                s.PriorityColor = item.Priority == null ? "" : item.Priority.Color;
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
        public ActionResult LoadTicketToRefer(int? id)
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

                    if (ticket.Type.HasValue)
                    {
                        model.Type = ticket.Type.Value;
                    }

                    if (ticket.CategoryID.HasValue)
                    {
                        model.CategoryId = ticket.CategoryID.Value;
                        model.Category = _categoryService.GetCategoryById(ticket.CategoryID.Value).Name;
                    }

                    model.ImpactId = ticket.ImpactID;
                    model.Impact = ticket.Impact.Name;
                    model.ImpactDetail = ticket.ImpactDetail;
                    model.ScheduleStartDateString = ticket.ScheduleStartDate.ToString(ConstantUtil.DateTimeFormat);
                    model.ScheduleEndDateString = ticket.ScheduleEndDate.ToString(ConstantUtil.DateTimeFormat);

                    if (!string.IsNullOrEmpty(ticket.TechnicianID))
                    {
                        AspNetUser technician = _userService.GetActiveUserById(ticket.TechnicianID);
                        if (technician != null)
                        {
                            model.TechnicianId = technician.Id;
                            model.Technician = technician.Fullname;
                            if (technician.GroupID.HasValue)
                            {
                                model.GroupId = technician.GroupID.Value;
                                model.Group = (technician.Group == null) ? "-" : technician.Group.Name;
                            }
                        }
                    }

                    model.Keywords = _keywordService.GetTicketKeywordForDisplay(ticket.ID);
                    model.Note = ticket.Note;
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
            var sortColumnIndex = TMSUtils.StrToIntDef(param.order[0]["column"], 0);
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
                            groupId = technician.GroupID,
                            group = technician.Group.Name
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new
                        {
                            success = true,
                            technicianId = "",
                            technician = "",
                            groupId = "",
                            group = ""
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
                            Urgency defaultUrgency = _urgencyService.GetSystemUrgency();
                            ticket.TechnicianID = technicianId;
                            ticket.UrgencyID = defaultUrgency.ID;
                            ticket.DueByDate = DateTime.Now.AddHours(defaultUrgency.Duration);
                            ticket.PriorityID = _ticketService.GetPriorityId(ticket.ImpactID, ticket.DueByDate);
                            ticket.ScheduleEndDate = ticket.DueByDate.AddDays(ConstantUtil.DayToCloseTicket);
                            bool assignResult = _ticketService.ReassignTicket(ticket, User.Identity.GetUserId());
                            if (assignResult)
                            {
                                Thread thread = new Thread(() => EmailUtil.SendToTechnicianWhenAssignTicket(ticket, technician));
                                thread.Start();
                                return Json(new
                                {
                                    success = true,
                                    message = "Ticket was reassigned successfully!"
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
            var sortColumnIndex = TMSUtils.StrToIntDef(param.order[0]["column"], 1);
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
                Description = string.IsNullOrWhiteSpace(m.Description) ? "-" : m.Description
            });

            JqueryDatatableResultViewModel rsModel = new JqueryDatatableResultViewModel();
            rsModel.draw = param.draw;
            rsModel.recordsTotal = displayedList.ToList().Count();
            rsModel.recordsFiltered = filteredListItems.Count();
            rsModel.data = displayedList;
            return Json(rsModel, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetTags(string subject)
        {
            IEnumerable<Keyword> keywordList = _keywordService.GetAll();
            List<string> keywords = new List<string>();
            subject = GeneralUtil.RemoveSpecialCharacters(subject);
            //Regex regex = new Regex("[ ]{2,}", RegexOptions.None);
            //string words = regex.Replace(subject, " ");
            //string[] wordArr = words.Split(' ');
            //foreach (string word in wordArr)
            //{
            //    string lowerWord = word.ToLower();
            //    if (keywordList.Any(m => m.Name.Equals(lowerWord)))
            //    {
            //        keywords.Add(lowerWord);
            //    }
            //}

            if (!string.IsNullOrWhiteSpace(subject))
            {
                subject = ' ' + subject.ToLower() + ' ';
                keywords = keywordList.Where(m => subject.Contains(' ' + m.Name.ToLower() + ' ')).Select(m => m.Name).ToList();
            }

            return Json(new
            {
                data = keywords
            }, JsonRequestBehavior.AllowGet);
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
