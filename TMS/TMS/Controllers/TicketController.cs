using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using TMS.DAL;
using TMS.Models;
using TMS.Services;
using TMS.Utils;
using TMS.ViewModels;

namespace TMS.Controllers
{
    public class TicketController : Controller
    {
        UnitOfWork _unitOfWork = new UnitOfWork();
        TicketService _ticketService;
        UserService _userService;
        GroupService _groupService;
        TicketAttachmentService _ticketAttachmentService;
        CategoryService _categoryService;
        SolutionService _solutionService;
        TicketHistoryService _ticketHistoryService;
        ImpactService _impactService;
        UrgencyService _urgencyService;
        KeywordService _keywordService;

        public TicketController()
        {
            _ticketService = new TicketService(_unitOfWork);
            _userService = new UserService(_unitOfWork);
            _groupService = new GroupService(_unitOfWork);
            _ticketAttachmentService = new TicketAttachmentService(_unitOfWork);
            _categoryService = new CategoryService(_unitOfWork);
            _solutionService = new SolutionService(_unitOfWork);
            _ticketHistoryService = new TicketHistoryService(_unitOfWork);
            _impactService = new ImpactService(_unitOfWork);
            _urgencyService = new UrgencyService(_unitOfWork);
            _keywordService = new KeywordService(_unitOfWork);
        }

        // GET: Tickets
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetRequesterTicketsTable(jQueryDataTableParamModel param)
        {
            // 1. Get Parameters
            string requesterID = User.Identity.GetUserId();
            var ticketList = _ticketService.GetRequesterTickets(requesterID);
            var default_search_key = Request["search[value]"];
            var search_text = Request["search_text"];


            // Initial variables
            IEnumerable<Ticket> filteredListItems;

            // Query data by params
            if (!string.IsNullOrEmpty(default_search_key)) //user have inputed keyword to search textbox
            {
                filteredListItems = ticketList.Where(p => p.Subject.ToLower().Contains(search_text.ToLower()));
            }
            else
            {
                filteredListItems = ticketList;
            }

            if (!string.IsNullOrEmpty(search_text))
            {
                filteredListItems = filteredListItems.Where(p => p.Subject.ToLower().Contains(search_text.ToLower()));
            }

            string userRole = null;

            if (User.Identity.GetUserId() != null)
            {
                userRole = _userService.GetUserById(User.Identity.GetUserId()).AspNetRoles.FirstOrDefault().Name;
            }

            foreach (Ticket ticket in filteredListItems)
            {
                if (userRole == ConstantUtil.UserRoleString.Requester && (ticket.Status <= 2 || string.IsNullOrEmpty(ticket.Solution)))
                {
                    ticket.Solution = "-";
                }
            }

            // Sort.
            var sortColumnIndex = Convert.ToInt32(Request["order[0][column]"]);
            var sortDirection = Request["order[0][dir]"];

            switch (sortColumnIndex)
            {
                case 0:
                    filteredListItems = sortDirection == "asc"
                        ? filteredListItems.OrderBy(p => p.CreatedTime)
                        : filteredListItems.OrderByDescending(p => p.CreatedTime);
                    break;
                case 1:
                    filteredListItems = sortDirection == "asc"
                        ? filteredListItems.OrderBy(p => p.Subject)
                        : filteredListItems.OrderByDescending(p => p.Subject);
                    break;
                case 2:
                    filteredListItems = sortDirection == "asc"
                        ? filteredListItems.OrderBy(p => p.Status)
                        : filteredListItems.OrderByDescending(p => p.Status);
                    break;
                default: break;
            }

            var displayedList = filteredListItems.Skip(param.start).Take(param.length);
            var result = displayedList.Select(p => new IConvertible[]{
                p.CreatedTime.ToString(),
                p.Subject,
                p.Status,
                p.Solution,
                p.ID
            }.ToArray());

            return Json(new
            {
                param.sEcho,
                iTotalRecords = result.Count(),
                iTotalDisplayRecords = filteredListItems.Count(),
                aaData = result
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetRequesterTickets(string filterItem, string sortItem, string keySearch)
        {
            IEnumerable<Ticket> ticketList = _ticketService.GetRequesterTickets(User.Identity.GetUserId());

            if (!string.IsNullOrWhiteSpace(sortItem))
            {
                switch (sortItem)
                {
                    case "DateAsc":
                        ticketList = ticketList.OrderBy(p => p.CreatedTime);
                        break;
                    case "StatusAsc":
                        ticketList = ticketList.OrderBy(p => p.Status);
                        break;
                    case "StatusDsc":
                        ticketList = ticketList.OrderByDescending(p => p.Status);
                        break;
                    case "SubjectAsc":
                        ticketList = ticketList.OrderBy(p => p.Subject);
                        break;
                    case "SubjectDsc":
                        ticketList = ticketList.OrderByDescending(p => p.Subject);
                        break;
                    default:
                        ticketList = ticketList.OrderByDescending(p => p.CreatedTime);
                        break;
                }
            }

            IEnumerable<BasicTicketViewModel> filteredListItems = ticketList.Select(p => new BasicTicketViewModel
            {
                ID = p.ID,
                Code = p.Code,
                CreatedBy = _userService.GetUserById(p.CreatedID).Fullname,
                Subject = p.Subject,
                Status = p.Status,
                Solution = p.Solution == null ? "-" : p.Solution,
                DescriptionAttachment = GetTicketAttachmentUrl(p.ID, ConstantUtil.TicketAttachmentType.Description),
                SolutionAttachment = GetTicketAttachmentUrl(p.ID, ConstantUtil.TicketAttachmentType.Solution),
                CreatedTime = GeneralUtil.ShowDateTime(p.CreatedTime),
                Mode = GeneralUtil.GetModeNameByMode(p.Mode),
                Category = p.Category == null ? "-" : p.Category.Name
            }).ToArray();

            if (!string.IsNullOrEmpty(keySearch))
            {
                filteredListItems = filteredListItems.Where(p => p.Subject.ToLower().Contains(keySearch.ToLower()));
            }

            if (!string.IsNullOrEmpty(filterItem))
            {
                switch (filterItem)
                {
                    case "Open":
                        filteredListItems = filteredListItems.Where(p => p.Status == ConstantUtil.TicketStatus.Open);
                        break;
                    case "Assigned":
                        filteredListItems = filteredListItems.Where(p => p.Status == ConstantUtil.TicketStatus.Assigned);
                        break;
                    case "Solved":
                        filteredListItems = filteredListItems.Where(p => p.Status == ConstantUtil.TicketStatus.Solved);
                        break;
                    case "Unapproved":
                        filteredListItems = filteredListItems.Where(p => p.Status == ConstantUtil.TicketStatus.Unapproved);
                        break;
                    case "Cancelled":
                        filteredListItems = filteredListItems.Where(p => p.Status == ConstantUtil.TicketStatus.Cancelled);
                        break;
                    case "Closed":
                        filteredListItems = filteredListItems.Where(p => p.Status == ConstantUtil.TicketStatus.Closed);
                        break;
                }
            }

            return Json(new
            {
                data = filteredListItems
            }, JsonRequestBehavior.AllowGet);
        }

        public string GetTicketAttachmentUrl(int? id, bool type)
        {
            IEnumerable<TicketAttachment> ticketAttachments = _ticketAttachmentService.GetAttachmentByTicketID(id.Value);
            string attachmentUrl = "";

            if (ticketAttachments.Count() > 0)
            {
                string fileName;
                foreach (var attachFile in ticketAttachments)
                {
                    fileName = TMSUtils.GetMinimizedAttachmentName(attachFile.Filename);
                    if (attachFile.Type == type)
                    {
                        attachmentUrl += "<a download=\'" + fileName +
                                         "\' class=\'btn-xs btn-primary btn-flat\' href=\'" + attachFile.Path +
                                         "\' target=\'_blank\' >" + fileName + "</a> &nbsp;";
                    }
                }
            }

            return attachmentUrl == "" ? "None" : attachmentUrl;
        }

        // GET: Tickets/Create
        [CustomAuthorize(Roles = "Requester")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [CustomAuthorize(Roles = "Requester")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(BasicTicketViewModel model, IEnumerable<HttpPostedFileBase> uploadFiles)
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
                Ticket ticket = new Ticket();
                TicketAttachment ticketFiles = new TicketAttachment();
                Urgency defaultUrgency = _urgencyService.GetSystemUrgency();

                ticket.Subject = model.Subject;
                ticket.Description = model.Description;
                ticket.Status = ConstantUtil.TicketStatus.Open;
                ticket.CreatedID = User.Identity.GetUserId();
                ticket.RequesterID = User.Identity.GetUserId();
                ticket.Mode = ConstantUtil.TicketMode.WebForm;
                ticket.CreatedTime = DateTime.Now;
                ticket.ModifiedTime = DateTime.Now;
                ticket.ScheduleStartDate = DateTime.Now;
                ticket.DueByDate = DateTime.Now.AddHours(defaultUrgency.Duration);
                ticket.ScheduleEndDate = ticket.DueByDate.AddDays(ConstantUtil.DayToCloseTicket);
                ticket.ImpactID = _impactService.GetSystemImpact().ID;
                ticket.UrgencyID = defaultUrgency.ID;
                ticket.PriorityID = _ticketService.GetPriorityId(ticket.ImpactID, ticket.DueByDate);
                ticket.TicketKeywords = _ticketService.GetTicketKeywords(ticket.Subject);
                bool addResult = _ticketService.AddTicket(ticket);
                if (addResult)
                {
                    if (uploadFiles != null && uploadFiles.ToList()[0] != null && uploadFiles.ToList().Count > 0)
                    {
                        _ticketAttachmentService.saveFile(ticket.ID, uploadFiles, ConstantUtil.TicketAttachmentType.Description);
                    }
                    AspNetUser requester = _userService.GetUserById(ticket.RequesterID);
                    if (requester != null)
                    {
                        Thread thread = new Thread(() => EmailUtil.SendToRequesterWhenCreateTicket(ticket, requester));
                        thread.Start();
                    }
                    return Json(new
                    {
                        success = true,
                        msg = "Create ticket successfully!"
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

        // Requester View Ticket
        [CustomAuthorize(Roles = "Requester")]
        [HttpGet]
        public ActionResult Detail(int? id)
        {
            if (id.HasValue)
            {
                Ticket ticket = _ticketService.GetTicketByID(id.Value);
                if (ticket != null)
                {
                    BasicTicketViewModel model = new BasicTicketViewModel();
                    AspNetUser solver = _userService.GetUserById(ticket.SolvedID);
                    AspNetUser creater = _userService.GetUserById(ticket.CreatedID);

                    model.ID = ticket.ID;
                    model.Subject = ticket.Subject;
                    model.Description = ticket.Description == null ? "-" : ticket.Description.Trim();
                    IEnumerable<Ticket> mergedTickets = _ticketService.GetMergedTickets(ticket.ID);
                    foreach (Ticket mergedTicket in mergedTickets)
                    {
                        string ticketCode = '#' + mergedTicket.Code;
                        model.Description = model.Description.Replace(ticketCode, "<a href='/Ticket/Detail/" + mergedTicket.ID + "'>" + ticketCode + "</a>");
                    }
                    model.Description = _ticketService.ReplaceURL(model.Description);
                    model.CreatedBy = creater.Fullname;
                    model.SolvedBy = solver == null ? "-" : solver.Fullname;
                    model.Status = ticket.Status;
                    model.Code = ticket.Code;
                    model.UnapproveReason = ticket.UnapproveReason == null ? "" : ticket.UnapproveReason.Trim();

                    if (ticket.Status == ConstantUtil.TicketStatus.Open || ticket.Status == ConstantUtil.TicketStatus.Assigned)
                    {
                        model.Solution = "-";
                    }
                    else
                    {
                        model.Solution = ticket.Solution == null ? "-" : ticket.Solution.Trim();
                        model.Solution = _ticketService.ReplaceURL(model.Solution);
                    }

                    model.Mode = GeneralUtil.GetModeNameByMode(ticket.Mode);
                    model.CreatedTime = GeneralUtil.ShowDateTime(ticket.CreatedTime);
                    model.SolvedTime = ticket.ModifiedTime.ToString("MMM d, yyyy hh:mm") ?? "-";
                    model.Category = ticket.Category == null ? " - " : ticket.Category.Name;

                    IEnumerable<TicketAttachment> ticketAttachments = _ticketAttachmentService.GetAttachmentByTicketID(id.Value);

                    if (ticketAttachments.Count() > 0)
                    {
                        string fileName;
                        foreach (var attachFile in ticketAttachments)
                        {
                            fileName = TMSUtils.GetMinimizedAttachmentName(attachFile.Filename);
                            if (attachFile.Type == ConstantUtil.TicketAttachmentType.Description)
                            {
                                model.DescriptionAttachment += "<a download=\'" + fileName +
                                                 "\' class=\'btn-xs btn-success\' href=\'" + attachFile.Path +
                                                 "\' target=\'_blank\' >" + fileName + "</a>&nbsp;";
                            }
                            else if (attachFile.Type == ConstantUtil.TicketAttachmentType.Solution)
                            {
                                model.SolutionAttachment += "<a download=\'" + fileName +
                                                 "\' class=\'btn-xs btn-success\' href=\'" + attachFile.Path +
                                                 "\' target=\'_blank\' >" + fileName + "</a>&nbsp;";
                            }
                        }
                    }
                    if (ticket.MergedID.HasValue)
                    {
                        Ticket mergedTicket = _ticketService.GetTicketByID(ticket.MergedID.Value);
                        model.MergedTicketString = GeneralUtil.GetRequesterMergedTicketInfo(mergedTicket);
                    }

                    return View(model);
                }
                else
                {
                    return HttpNotFound();
                }
            }
            else
            {
                return HttpNotFound();
            }
        }

        [CustomAuthorize(Roles = "Helpdesk,Technician")]
        [HttpGet]
        public ActionResult TicketDetail(int? id)
        {
            if (id.HasValue)
            {
                Ticket ticket = _ticketService.GetTicketByID(id.Value);
                if (ticket != null)
                {
                    AspNetUser currentUser = _userService.GetUserById(User.Identity.GetUserId());
                    AspNetRole userRole = currentUser.AspNetRoles.FirstOrDefault();

                    // Get Ticket information
                    AspNetUser solvedUser = _userService.GetUserById(ticket.SolvedID);
                    AspNetUser createdUser = _userService.GetUserById(ticket.CreatedID);
                    AspNetUser assignedUser = _userService.GetUserById(ticket.AssignedByID);
                    AspNetUser technician = _userService.GetUserById(ticket.TechnicianID);
                    AspNetUser requester = _userService.GetUserById(ticket.RequesterID);

                    if (ticket.Status == ConstantUtil.TicketStatus.Assigned
                        && userRole.Name == ConstantUtil.UserRoleString.Technician
                        && currentUser.Id == ticket.TechnicianID)
                    {
                        ViewBag.Role = "TechnicianInCharge";
                    }
                    else
                    {
                        ViewBag.Role = userRole.Name;
                    }

                    TicketViewModel model = new TicketViewModel();

                    model.Id = ticket.ID;
                    model.Code = ticket.Code;
                    model.Subject = ticket.Subject;
                    model.Description = ticket.Description == null ? "-" : ticket.Description.Trim();
                    model.Description = _ticketService.ReplaceURL(model.Description);
                    IEnumerable<Ticket> mergedTickets = _ticketService.GetMergedTickets(ticket.ID);
                    foreach (Ticket mergedTicket in mergedTickets)
                    {
                        string ticketCode = '#' + mergedTicket.Code;
                        model.Description = model.Description.Replace(ticketCode, "<a href='/Ticket/TicketDetail/" + mergedTicket.ID + "'>" + ticketCode + "</a>");
                    }
                    model.ModeString = GeneralUtil.GetModeNameByMode(ticket.Mode);
                    model.TypeString = GeneralUtil.GetTypeNameByType(ticket.Type);
                    if (ticket.MergedID.HasValue)
                    {
                        Ticket mergedTicket = _ticketService.GetTicketByID(ticket.MergedID.Value);
                        model.MergedTicketString = GeneralUtil.GetMergedTicketInfo(mergedTicket);
                    }
                    model.Status = GeneralUtil.GetTicketStatusByID(ticket.Status);
                    IEnumerable<TicketAttachment> ticketAttachments = _ticketAttachmentService.GetAttachmentByTicketID(id.Value);

                    if (ticketAttachments != null)
                    {
                        model.DescriptionAttachments = new List<AttachmentViewModel>();
                        model.SolutionAttachments = new List<AttachmentViewModel>();
                        foreach (var attachment in ticketAttachments)
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

                    model.Category = (ticket.Category == null) ? "-" : ticket.Category.Name;
                    model.Impact = ticket.Impact.Name;
                    model.ImpactDetail = (ticket.ImpactDetail == null) ? "-" : ticket.ImpactDetail;
                    model.Urgency = ticket.Urgency.Name;
                    model.Priority = ticket.Priority.Name;
                    model.CreatedTimeString = ticket.CreatedTime.ToString(ConstantUtil.DateTimeFormat3);
                    model.ModifiedTimeString = ticket.ModifiedTime.ToString(ConstantUtil.DateTimeFormat3);
                    model.ScheduleEndDateString = ticket.ScheduleEndDate.ToString(ConstantUtil.DateTimeFormat3);
                    model.ScheduleStartDateString = ticket.ScheduleStartDate.ToString(ConstantUtil.DateTimeFormat3);
                    model.ActualEndDateString = ticket.ActualEndDate == null ? "-" : ticket.ActualEndDate.Value.ToString(ConstantUtil.DateTimeFormat3);
                    model.SolvedDateString = ticket.SolvedDate == null ? "-" : ticket.SolvedDate.Value.ToString(ConstantUtil.DateTimeFormat3);
                    model.DueByDateString = ticket.DueByDate.ToString(ConstantUtil.DateTimeFormat3);
                    model.Group = "-";
                    if (technician != null)
                    {
                        if (technician.Group != null)
                        {
                            model.Group = technician.Group.Name;
                        }
                    }
                    model.Technician = GeneralUtil.GetUserInfo(technician);
                    model.CreatedBy = GeneralUtil.GetUserInfo(createdUser);
                    model.AssignedBy = GeneralUtil.GetUserInfo(assignedUser);
                    model.SolvedBy = GeneralUtil.GetUserInfo(solvedUser);
                    model.Requester = GeneralUtil.GetUserInfo(requester);
                    model.Solution = ticket.Solution == null ? string.Empty : ticket.Solution.Trim();
                    model.Solution = _ticketService.ReplaceURL(model.Solution);
                    model.DescriptionAttachmentsURL = GetTicketAttachmentUrl(ticket.ID, ConstantUtil.TicketAttachmentType.Description); 
                    model.SolutionAttachmentsURL = GetTicketAttachmentUrl(ticket.ID, ConstantUtil.TicketAttachmentType.Solution);
                    model.UnapproveReason = (string.IsNullOrEmpty(ticket.UnapproveReason)) ? "-" : ticket.UnapproveReason.Trim();
                    model.Keywords = _keywordService.GetTicketKeywordForDisplay(ticket.ID);
                    model.Note = (string.IsNullOrEmpty(ticket.Note)) ? "-" : ticket.Note.Trim();

                    return View(model);
                }
            }
            return HttpNotFound();
        }

        [CustomAuthorize(Roles = "Requester,Helpdesk,Technician")]
        [HttpGet]
        public ActionResult GetTicketDetail(int? id)
        {
            if (id.HasValue)
            {
                Ticket ticket = _ticketService.GetTicketByID(id.Value);
                if (ticket != null)
                {
                    AspNetUser solvedUser = _userService.GetUserById(ticket.SolvedID);
                    AspNetUser createdUser = _userService.GetUserById(ticket.CreatedID);
                    AspNetUser assignedUser = _userService.GetUserById(ticket.AssignedByID);
                    AspNetUser requester = _userService.GetUserById(ticket.RequesterID);
                    AspNetUser technician = _userService.GetUserById(ticket.TechnicianID);

                    string ticketType, ticketMode, solution, ticketUrgency, ticketPriority, ticketImpact, group, category, description, note;
                    string createdTime, modifiedTime, scheduleStartDate, scheduleEndDate, actualEndDate, solvedDate, dueByDate;

                    string userRole = _userService.GetUserById(User.Identity.GetUserId()).AspNetRoles.FirstOrDefault().Name;

                    if (userRole == ConstantUtil.UserRoleString.Requester)
                    {
                        if (ticket.Status == ConstantUtil.TicketStatus.Open
                            || ticket.Status == ConstantUtil.TicketStatus.Assigned)
                        {
                            solution = "-";
                        }
                        else
                        {
                            solution = string.IsNullOrWhiteSpace(ticket.Solution) ? "-" : ticket.Solution.Trim();
                        }
                    }
                    else
                    {
                        solution = string.IsNullOrWhiteSpace(ticket.Solution) ? "-" : ticket.Solution.Trim();
                    }

                    IEnumerable<TicketAttachment> ticketAttachments = _ticketAttachmentService.GetAttachmentByTicketID(id.Value);
                    string descriptionAttachment = "";
                    string solutionAttachment = "";

                    if (ticketAttachments.Count() > 0)
                    {
                        string fileName;
                        foreach (var attachFile in ticketAttachments)
                        {
                            fileName = TMSUtils.GetMinimizedAttachmentName(attachFile.Filename);
                            if (attachFile.Type == ConstantUtil.TicketAttachmentType.Description)
                            {
                                descriptionAttachment += "<a download=\'" + fileName +
                                                 "\' class=\'btn-xs btn-primary btn-flat\' href=\'" + attachFile.Path +
                                                 "\' target=\'_blank\' >" + fileName + "</a>";
                            }
                            else if (attachFile.Type == ConstantUtil.TicketAttachmentType.Solution)
                            {
                                solutionAttachment += "<a download=\'" + fileName +
                                                 "\' class=\'btn-xs btn-primary btn-flat\' href=\'" + attachFile.Path +
                                                 "\' target=\'_blank\' >" + fileName + "</a>";
                            }
                        }
                    }

                    ticketType = GeneralUtil.GetTypeNameByType(ticket.Type);
                    ticketMode = GeneralUtil.GetModeNameByMode(ticket.Mode);
                    ticketUrgency = ticket.Urgency.Name;
                    ticketPriority = ticket.Priority.Name;
                    ticketImpact = ticket.Impact.Name;
                    createdTime = ticket.CreatedTime.ToString(ConstantUtil.DateTimeFormat);
                    modifiedTime = ticket.ModifiedTime.ToString(ConstantUtil.DateTimeFormat);
                    scheduleStartDate = ticket.ScheduleStartDate.ToString(ConstantUtil.DateTimeFormat);
                    scheduleEndDate = ticket.ScheduleEndDate.ToString(ConstantUtil.DateTimeFormat);
                    dueByDate = ticket.DueByDate.ToString(ConstantUtil.DateTimeFormat);
                    actualEndDate = ticket.ActualEndDate?.ToString(ConstantUtil.DateTimeFormat) ?? "-";
                    solvedDate = ticket.SolvedDate?.ToString(ConstantUtil.DateTimeFormat) ?? "-";

                    group = "-";
                    if (technician != null)
                    {
                        if (technician.Group != null)
                        {
                            group = technician.Group.Name;
                        }
                    }

                    category = _categoryService.GetCategoryPath(ticket.Category);
                    description = ticket.Description ?? "-";
                    IEnumerable<Ticket> mergedTickets = _ticketService.GetMergedTickets(ticket.ID);
                    foreach (Ticket mergedTicket in mergedTickets)
                    {
                        string ticketCode = '#' + mergedTicket.Code;
                        description = description.Replace(ticketCode, "<a href='/Ticket/TicketDetail/" + mergedTicket.ID + "'>" + ticketCode + "</a>");
                    }
                    string mergedTicketString = null;
                    if (ticket.MergedID.HasValue)
                    {
                        Ticket mergedTicket = _ticketService.GetTicketByID(ticket.MergedID.Value);
                        if (mergedTicket != null)
                        {
                            mergedTicketString = mergedTicket.Subject + " (<a href='/Ticket/TicketDetail/" + ticket.MergedID + "'>" + mergedTicket.Code + "</a>)";
                        }
                    }
                    note = (string.IsNullOrWhiteSpace(ticket.Note)) ? "-" : ticket.Note.Trim();

                    return Json(new
                    {
                        success = true,
                        id = ticket.ID,
                        subject = ticket.Subject,
                        description = description,
                        type = ticketType,
                        mode = ticketMode,
                        urgency = ticketUrgency,
                        priority = ticketPriority,
                        category = category,
                        impact = ticketImpact,
                        impactDetail = ticket.ImpactDetail ?? "-",
                        status = ticket.Status,
                        createdDate = createdTime,
                        lastModified = modifiedTime,
                        solvedDate = solvedDate,
                        scheduleStart = scheduleStartDate,
                        scheduleEnd = scheduleEndDate,
                        actualEnd = actualEndDate,
                        dueByDate = dueByDate,
                        solution = solution,
                        solver = GeneralUtil.GetUserInfo(solvedUser),
                        creater = GeneralUtil.GetUserInfo(createdUser),
                        assigner = GeneralUtil.GetUserInfo(assignedUser),
                        technician = GeneralUtil.GetUserInfo(technician),
                        requester = GeneralUtil.GetUserInfo(requester),
                        group = group,
                        descriptionAttachment = descriptionAttachment,
                        solutionAttachment = solutionAttachment,
                        mergeTicket = mergedTicketString,
                        tags = _keywordService.GetTicketKeywordForDisplay(ticket.ID),
                        note = note
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new
            {
                success = false,
                message = ConstantUtil.CommonError.UnavailableTicket
            }, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Roles = "Helpdesk,Technician")]
        [HttpGet]
        public ActionResult Solve(int? id)
        {
            if (id.HasValue)
            {
                Ticket ticket = _ticketService.GetTicketByID(id.Value);
                if (ticket != null)
                {
                    AspNetRole userRole = _userService.GetUserById(User.Identity.GetUserId()).AspNetRoles.FirstOrDefault();

                    if (userRole.Id == ConstantUtil.UserRole.Technician.ToString())
                    {
                        ViewBag.Role = "Technician";
                        if (ticket.Status != ConstantUtil.TicketStatus.Assigned) // Ticket status is not "Assigned"
                        {
                            return RedirectToAction("Index", new { Area = "Technician" }); // Redirect to Index so the Technician cannot go to Solve view.
                        }
                    }
                    else if (userRole.Id == ConstantUtil.UserRole.HelpDesk.ToString())
                    {
                        ViewBag.Role = "HelpDesk";
                        if (ticket.Status != ConstantUtil.TicketStatus.Open &&
                            ticket.Status != ConstantUtil.TicketStatus.Unapproved)
                        {
                            return RedirectToAction("Index", "ManageTicket", new { Area = "HelpDesk" }); // Redirect to Index so the Technician cannot go to Solve view.
                        }
                    }
                    // Get Ticket information
                    AspNetUser solvedUser = _userService.GetUserById(ticket.SolvedID);
                    AspNetUser createdUser = _userService.GetUserById(ticket.CreatedID);
                    AspNetUser assignedUser = _userService.GetUserById(ticket.AssignedByID);
                    AspNetUser requester = _userService.GetUserById(ticket.RequesterID);
                    TicketSolveViewModel model = new TicketSolveViewModel();

                    model.ID = ticket.ID;
                    model.Code = ticket.Code;
                    model.Subject = ticket.Subject;
                    model.Description = string.IsNullOrWhiteSpace(ticket.Description) ? "-" : ticket.Description.Trim();
                    model.DescriptionAttachmentsURL = GetTicketAttachmentUrl(ticket.ID, ConstantUtil.TicketAttachmentType.Description);
                    model.Mode = GeneralUtil.GetModeNameByMode(ticket.Mode);
                    model.Type = GeneralUtil.GetTypeNameByType(ticket.Type);
                    model.Status = GeneralUtil.GetTicketStatusByID(ticket.Status);
                    model.Category = (ticket.Category == null) ? "-" : ticket.Category.Name;
                    model.Impact = (ticket.Impact == null) ? "-" : ticket.Impact.Name;
                    model.ImpactDetail = string.IsNullOrWhiteSpace(ticket.ImpactDetail) ? "-" : ticket.ImpactDetail.Trim();
                    model.Urgency = (ticket.Urgency == null) ? "-" : ticket.Urgency.Name;
                    model.Priority = (ticket.Priority == null) ? "-" : ticket.Priority.Name;
                    model.CreatedTimeString = ticket.CreatedTime.ToString(ConstantUtil.DateTimeFormat);
                    model.ModifiedTimeString = ticket.ModifiedTime.ToString(ConstantUtil.DateTimeFormat);
                    model.ScheduleStartDateString = ticket.ScheduleStartDate.ToString(ConstantUtil.DateTimeFormat);
                    model.ScheduleEndDateString = ticket.ScheduleEndDate.ToString(ConstantUtil.DateTimeFormat);
                    model.ActualEndDateString = ticket.ActualEndDate.HasValue ? ticket.ActualEndDate.Value.ToString(ConstantUtil.DateTimeFormat) : "-";
                    model.SolvedDateString = ticket.SolvedDate.HasValue ? ticket.SolvedDate.Value.ToString(ConstantUtil.DateTimeFormat) : "-";
                    model.OverdueDateString = ticket.DueByDate.ToString(ConstantUtil.DateTimeFormat);
                    model.CreatedBy = GeneralUtil.GetUserInfo(createdUser);
                    model.AssignedBy = GeneralUtil.GetUserInfo(assignedUser);
                    model.SolvedBy = GeneralUtil.GetUserInfo(solvedUser);
                    model.Requester = GeneralUtil.GetUserInfo(requester);
                    model.Solution = ticket.Solution;
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
                    model.UnapproveReason = (string.IsNullOrWhiteSpace(ticket.UnapproveReason)) ? "-" : ticket.UnapproveReason.Trim();
                    model.Tags = _keywordService.GetTicketKeywordForDisplay(ticket.ID);
                    model.Note = (string.IsNullOrWhiteSpace(ticket.Note)) ? "-" : ticket.Note.Trim();
                    return View(model);
                }
            }
            return HttpNotFound();
        }

        [CustomAuthorize(Roles = "Helpdesk,Technician")]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SolveTicket(TicketSolveViewModel model)
        {
            if (model.ID.HasValue)
            {
                AspNetUser user = _userService.GetUserById(User.Identity.GetUserId());
                string userRole = user.AspNetRoles.FirstOrDefault().Name;

                Ticket ticket = _ticketService.GetTicketByID(model.ID.Value);
                if (ticket == null)
                {
                    return Json(new
                    {
                        success = false,
                        msg = ConstantUtil.CommonError.UnavailableTicket
                    });
                }

                if (string.IsNullOrWhiteSpace(model.Solution))
                {
                    return Json(new
                    {
                        success = false,
                        msg = "Please enter solution!"
                    });
                }

                ticket.ModifiedTime = DateTime.Now;
                ticket.Solution = model.Solution;
                switch (model.Command)
                {
                    case "Solve":
                        ticket.SolvedID = User.Identity.GetUserId();
                        ticket.SolvedDate = DateTime.Now;
                        bool solveResult = _ticketService.SolveTicket(ticket, User.Identity.GetUserId());
                        if (solveResult)
                        {
                            if (model.SolutionFiles != null && model.SolutionFiles.ToList()[0] != null && model.SolutionFiles.ToList().Count > 0)
                            {
                                _ticketAttachmentService.saveFile(ticket.ID, model.SolutionFiles, ConstantUtil.TicketAttachmentType.Solution);
                            }
                            AspNetUser requester = _userService.GetUserById(ticket.RequesterID);
                            if (requester != null)
                            {
                                Thread thread = new Thread(() => EmailUtil.SendToRequesterWhenSolveTicket(ticket, requester));
                                thread.Start();
                            }

                            return Json(new
                            {
                                success = true,
                                msg = "Ticket was solved!",
                                userRole = userRole
                            });
                        }
                        break;
                    case "Save":
                        bool updateResult = _ticketService.UpdateTicket(ticket, User.Identity.GetUserId());
                        if (updateResult)
                        {
                            if (model.SolutionFiles != null && model.SolutionFiles.ToList()[0] != null && model.SolutionFiles.ToList().Count > 0)
                            {
                                _ticketAttachmentService.saveFile(ticket.ID, model.SolutionFiles, ConstantUtil.TicketAttachmentType.Solution);
                            }
                            return Json(new
                            {
                                success = true,
                                msg = "Solution is saved!",
                                userRole = userRole
                            });
                        }
                        break;
                }
                return Json(new
                {
                    success = false,
                    msg = ConstantUtil.CommonError.DBExceptionError,
                    userRole = userRole
                });
            }

            return Json(new
            {
                success = false,
                msg = ConstantUtil.CommonError.UnavailableTicket
            });
        }

        [HttpPost]
        public ActionResult ApproveTicket(int? id, string unapprovedReason)
        {
            if (id.HasValue)
            {
                Ticket ticket = _ticketService.GetTicketByID(id.Value);
                if (ticket != null)
                {
                    if (ticket.Status == ConstantUtil.TicketStatus.Solved)
                    {
                        if (!string.IsNullOrWhiteSpace(unapprovedReason))
                        {
                            ticket.UnapproveReason = unapprovedReason;
                            bool approveResult = _ticketService.ApproveTicket(ticket, User.Identity.GetUserId());
                            if (approveResult)
                            {
                                return Json(new
                                {
                                    success = true,
                                    msg = "Thank you for your feedback!"
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
                                msg = "Please tell us what's wrong with this solution!"
                            });
                        }
                    }
                    else
                    {
                        return Json(new
                        {
                            success = false,
                            msg = ConstantUtil.CommonError.InvalidTicket
                        });
                    }
                }
            }
            return Json(new
            {
                success = false,
                msg = ConstantUtil.CommonError.UnavailableTicket
            });
        }

        [HttpPost]
        public ActionResult SearchSolution(string keywords)
        {
            IEnumerable<Solution> solutions;

            solutions = _solutionService.SearchSolutions(keywords);

            IEnumerable<SolutionViewModel> result = solutions.Select(m => new SolutionViewModel
            {
                Id = m.ID,
                Subject = m.Subject,
                ContentText = m.ContentText,
                Path = Url.Action("Detail", "FAQ", new { path = m.Path }, protocol: Request.Url.Scheme /* This is the trick */)
            });
            return Json(new
            {
                success = true,
                data = result,
            });
        }

        [HttpPost]
        public ActionResult GetSolutionContent(int? id)
        {
            Solution solution = null;
            if (id.HasValue)
            {
                solution = _solutionService.GetSolutionById(id.Value);
            }

            if (solution == null || solution.ContentText == null || solution.ContentText.Trim().Length == 0)
            {
                return Json(new
                {
                    success = false,
                });
            }
            return Json(new
            {
                success = true,
                data = solution.ContentText.Trim(),
            });
        }

        [HttpGet]
        public ActionResult GetTickets(jQueryDataTableParamModel param)
        {
            var ticketList = _ticketService.GetAll();

            IEnumerable<Ticket> filteredListItems;
            if (!string.IsNullOrEmpty(param.sSearch))
            {
                filteredListItems = ticketList.Where(p => p.Subject.ToLower().Contains(param.sSearch.ToLower()));
            }
            else
            {
                filteredListItems = ticketList;
            }
            // Sort.
            var sortColumnIndex = TMSUtils.StrToIntDef(Request["iSortCol_0"], 2);
            var sortDirection = Request["sSortDir_0"]; // asc or desc

            switch (sortColumnIndex)
            {
                case 2:
                    filteredListItems = sortDirection == "asc"
                        ? filteredListItems.OrderBy(p => p.Subject)
                        : filteredListItems.OrderByDescending(p => p.Subject);
                    break;
            }

            var displayedList = filteredListItems.Skip(param.start).Take(param.length);
            var result = displayedList.Select(p => new IConvertible[]{
                p.CreatedTime.ToString(),
                p.Subject,
                p.Description,
                p.Status,
                p.Solution,
                p.ModifiedTime.ToString()
            }.ToArray());

            return Json(new
            {
                param.sEcho,
                iTotalRecords = result.Count(),
                iTotalDisplayRecords = filteredListItems.Count(),
                aaData = result
            }, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Roles = "Helpdesk,Technician")]
        [HttpGet]
        public ActionResult History(int? id)
        {
            if (id.HasValue)
            {
                Ticket ticket = _ticketService.GetTicketByID(id.Value);
                if (ticket != null)
                {
                    IEnumerable<TicketHistoryViewModel> historyTickets = _ticketHistoryService.GetHistoryTicketsByTicketID(id.Value)
                        .Select(m => new TicketHistoryViewModel
                        {
                            ActedDate = m.ActedTime.HasValue ? GeneralUtil.ShowDateTime(m.ActedTime.Value) : "-",
                            Action = m.Action,
                            Performer = _userService.GetUserById(m.ActID) != null ? _userService.GetUserById(m.ActID).Fullname : "System",
                            Type = GeneralUtil.GetTicketHistoryTypeName(m.Type)
                        });
                    AspNetUser requester = _userService.GetUserById(ticket.RequesterID);
                    ViewBag.Subject = ticket.Subject;
                    ViewBag.Priority = ticket.Priority.Name;
                    ViewBag.Requester = requester.Fullname;
                    ViewBag.CreateTime = ticket.CreatedTime.ToString(ConstantUtil.DateTimeFormat);
                    ViewBag.Role = _userService.GetUserById(User.Identity.GetUserId()).AspNetRoles.FirstOrDefault().Name;
                    ViewBag.Code = ticket.Code;
                    return View(historyTickets);
                }
            }
            return HttpNotFound();
        }

        [HttpGet]
        public ActionResult GetTags(string subject)
        {
            IEnumerable<Keyword> keywordList = _keywordService.GetAll();
            List<string> keywords = new List<string>();
            subject = GeneralUtil.RemoveSpecialCharacters(subject);
            Regex regex = new Regex("[ ]{2,}", RegexOptions.None);
            string words = regex.Replace(subject, " ");
            string[] wordArr = words.Split(' ');
            foreach (string word in wordArr)
            {
                string lowerWord = word.ToLower();
                if (keywordList.Any(m => m.Name.Equals(lowerWord)))
                {
                    keywords.Add(lowerWord);
                }
            }

            return Json(new
            {
                data = keywords
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetKeywords(string term)
        {
            List<string> labels = _keywordService.GetAll()
            .Where(m => m.Name.Contains(term))
            .Select(m => m.Name).ToList();

            return Json(labels, JsonRequestBehavior.AllowGet);
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string id = User.Identity.GetUserId();
            AspNetUser requester = _userService.GetUserById(id);
            if (requester != null)
            {
                ViewBag.LayoutName = requester.Fullname;
                ViewBag.LayoutAvatarURL = requester.AvatarURL;
            }
            base.OnActionExecuting(filterContext);
        }

    }
}
