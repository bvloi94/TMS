﻿using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
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
        UnitOfWork unitOfWork = new UnitOfWork();
        public TicketService _ticketService { get; set; }
        public UserService _userService { get; set; }
        public DepartmentService _departmentService { get; set; }
        public TicketAttachmentService _ticketAttachmentService { get; set; }
        public CategoryService _categoryService { get; set; }
        public SolutionService _solutionService { get; set; }

        public TicketController()
        {
            _ticketService = new TicketService(unitOfWork);
            _userService = new UserService(unitOfWork);
            _departmentService = new DepartmentService(unitOfWork);
            _ticketAttachmentService = new TicketAttachmentService(unitOfWork);
            _categoryService = new CategoryService(unitOfWork);
            _solutionService = new SolutionService(unitOfWork);
        }

        // GET: Tickets
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetRequesterTickets(jQueryDataTableParamModel param)
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
                //contains(keyword) = like "%keyword%" in SQL query
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
        public ActionResult Create(RequesterTicketViewModel model, IEnumerable<HttpPostedFileBase> uploadFiles)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Ticket ticket = new Ticket();
                    TicketAttachment ticketFiles = new TicketAttachment();

                    ticket.Subject = model.Subject;
                    ticket.Description = model.Description;
                    ticket.Status = ConstantUtil.TicketStatus.New;
                    ticket.CreatedID = User.Identity.GetUserId();
                    ticket.RequesterID = User.Identity.GetUserId();
                    ticket.Mode = ConstantUtil.TicketMode.WebForm;
                    ticket.CreatedTime = DateTime.Now;
                    ticket.ModifiedTime = DateTime.Now;
                    _ticketService.AddTicket(ticket);
                    if (uploadFiles != null && uploadFiles.ToList()[0] != null && uploadFiles.ToList().Count > 0)
                    {
                        _ticketAttachmentService.saveFile(ticket.ID, uploadFiles, ConstantUtil.TicketAttachmentType.Description);
                    }
                    return Json(new
                    {
                        success = true,
                        msg = "Create ticket successfully!"
                    });
                }
                catch
                {
                    return Json(new
                    {
                        success = false,
                        error = true,
                        msg = "Cannot create ticket. Please try again!"
                    });
                }
            }

            return Json(new
            {
                success = false,
                error = true,
                msg = "Cannot create ticket. Please try again!"
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
                    RequesterTicketViewModel model = new RequesterTicketViewModel();
                    AspNetUser solver = _userService.GetUserById(ticket.SolveID);
                    AspNetUser creater = _userService.GetUserById(ticket.CreatedID);

                    model.ID = ticket.ID;
                    model.Subject = ticket.Subject;
                    model.Description = ticket.Description == null ? "-" : ticket.Description;
                    model.CreatedBy = creater.Fullname;
                    model.SolvedBy = solver == null ? "-" : solver.Fullname;
                    model.Status = ticket.Status;
                    model.Code = ticket.Code;
                    model.UnapproveReason = ticket.UnapproveReason == null ? "" : ticket.UnapproveReason;

                    if (ticket.Status == ConstantUtil.TicketStatus.New || ticket.Status == ConstantUtil.TicketStatus.Assigned)
                    {
                        model.Solution = "-";
                    }
                    else
                    {
                        model.Solution = ticket.Solution == null ? "-" : ticket.Solution;
                    }

                    switch (ticket.Mode)
                    {
                        case ConstantUtil.TicketMode.PhoneCall: model.Mode = ConstantUtil.TicketModeString.PhoneCall; break;
                        case ConstantUtil.TicketMode.WebForm: model.Mode = ConstantUtil.TicketModeString.WebForm; break;
                        case ConstantUtil.TicketMode.Email: model.Mode = ConstantUtil.TicketModeString.Email; break;
                        default: model.Mode = "-"; break;
                    }

                    model.CreateTime = ticket.CreatedTime.ToString(ConstantUtil.DateTimeFormat);
                    model.SolvedTime = ticket.ModifiedTime.ToString(ConstantUtil.DateTimeFormat) ?? "-";
                    model.Category = _categoryService.GetCategoryPath(ticket.Category);
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

        [HttpGet]
        public ActionResult GetTicketDetail(int id)
        {
            Ticket ticket = _ticketService.GetTicketByID(id);
            AspNetUser solver = _userService.GetUserById(ticket.SolveID);
            AspNetUser creater = _userService.GetUserById(ticket.CreatedID);
            AspNetUser assigner = _userService.GetUserById(ticket.AssignedByID);
            AspNetUser technician = _userService.GetUserById(ticket.TechnicianID);
            String ticketType, ticketMode, solution, ticketUrgency, ticketPriority, ticketImpact, department = "-";
            String createdDate, modifiedDate, scheduleStartDate, scheduleEndDate, actualStartDate, actualEndDate, solvedDate;

            string userRole = null;
            if (User.Identity.GetUserId() != null)
            {
                userRole = _userService.GetUserById(User.Identity.GetUserId()).AspNetRoles.FirstOrDefault().Name;
            }

            if (userRole == ConstantUtil.UserRoleString.Requester)
            {
                if (ticket.Status <= 2)
                {
                    solution = "-";
                }
                else
                {
                    solution = ticket.Solution == null ? "-" : ticket.Solution;
                }
            }
            else
            {
                solution = ticket.Solution == null ? "-" : ticket.Solution;
            }

            IEnumerable<TicketAttachment> ticketAttachments = _ticketAttachmentService.GetAttachmentByTicketID(id);
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

            switch (ticket.Type)
            {
                case 1: ticketType = ConstantUtil.TicketTypeString.Request; break;
                case 2: ticketType = ConstantUtil.TicketTypeString.Problem; break;
                case 3: ticketType = ConstantUtil.TicketTypeString.Change; break;
                default: ticketType = "-"; break;
            }

            switch (ticket.Mode)
            {
                case 1: ticketMode = ConstantUtil.TicketModeString.PhoneCall; break;
                case 2: ticketMode = ConstantUtil.TicketModeString.WebForm; break;
                case 3: ticketMode = ConstantUtil.TicketModeString.Email; break;
                default: ticketMode = "-"; break;
            }

            ticketUrgency = ticket.Urgency == null ? "-" : ticket.Urgency.Name;
            ticketPriority = ticket.Priority == null ? "-" : ticket.Priority.Name;
            ticketImpact = ticket.Impact == null ? "-" : ticket.Impact.Name;
            createdDate = ticket.CreatedTime.ToString(ConstantUtil.DateTimeFormat);
            modifiedDate = ticket.ModifiedTime.ToString(ConstantUtil.DateTimeFormat);
            scheduleStartDate = ticket.ScheduleStartDate?.ToString(ConstantUtil.DateTimeFormat) ?? "-";
            scheduleEndDate = ticket.ScheduleEndDate?.ToString(ConstantUtil.DateTimeFormat) ?? "-";
            actualStartDate = ticket.ActualStartDate?.ToString(ConstantUtil.DateTimeFormat) ?? "-";
            actualEndDate = ticket.ActualEndDate?.ToString(ConstantUtil.DateTimeFormat) ?? "-";
            solvedDate = ticket.SolvedDate?.ToString(ConstantUtil.DateTimeFormat) ?? "-";

            if (technician != null)
            {
                department = technician.Department == null ? "-" : technician.Department.Name;
            }
            else
            {
                department = "-";
            }


            string categoryPath = "-";
            if (ticket.Category != null)
            {
                categoryPath = ticket.Category.Name;
                Category parentCate = ticket.Category;
                while (parentCate.ParentID != null)
                {
                    parentCate = _categoryService.GetCategoryById((int)parentCate.ParentID);
                    categoryPath = parentCate.Name + "  >  " + categoryPath;
                }
            }

            return Json(new
            {
                id = ticket.ID,
                subject = ticket.Subject,
                description = ticket.Description ?? "-",
                type = ticketType,
                mode = ticketMode,
                urgency = ticketUrgency,
                priority = ticketPriority,
                category = categoryPath,
                impact = ticketImpact,
                impactDetail = ticket.ImpactDetail ?? "-",
                status = ticket.Status,
                createdDate = createdDate,
                lastModified = modifiedDate,
                solvedDate = solvedDate,
                scheduleStart = scheduleStartDate,
                scheduleEnd = scheduleEndDate,
                actualStart = actualStartDate,
                actualEnd = actualEndDate,
                solution = solution,
                solver = solver == null ? "-" : solver.Fullname,
                creater = creater == null ? "-" : creater.Fullname,
                assigner = assigner == null ? "-" : assigner.Fullname,
                technician = technician == null ? "-" : technician.Fullname,
                department = department,
                descriptionAttachment = descriptionAttachment,
                solutionAttachment = solutionAttachment
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
                        if (ticket.Status != ConstantUtil.TicketStatus.New &&
                            ticket.Status != ConstantUtil.TicketStatus.Unapproved)
                        {
                            return RedirectToAction("Index", "ManageTicket", new { Area = "HelpDesk" }); // Redirect to Index so the Technician cannot go to Solve view.
                        }
                    }
                    // Get Ticket information
                    AspNetUser solvedUser = _userService.GetUserById(ticket.SolveID);
                    AspNetUser createdUser = _userService.GetUserById(ticket.CreatedID);
                    AspNetUser assigner = _userService.GetUserById(ticket.AssignedByID);
                    TicketSolveViewModel model = new TicketSolveViewModel();

                    model.ID = ticket.ID;
                    model.Subject = ticket.Subject;
                    model.Description = ticket.Description;

                    switch (ticket.Mode)
                    {
                        case ConstantUtil.TicketMode.PhoneCall: model.Mode = ConstantUtil.TicketModeString.PhoneCall; break;
                        case ConstantUtil.TicketMode.WebForm: model.Mode = ConstantUtil.TicketModeString.WebForm; break;
                        case ConstantUtil.TicketMode.Email: model.Mode = ConstantUtil.TicketModeString.Email; break;
                    }

                    switch (ticket.Type)
                    {
                        case ConstantUtil.TicketType.Request: model.Type = ConstantUtil.TicketTypeString.Request; break;
                        case ConstantUtil.TicketType.Problem: model.Type = ConstantUtil.TicketTypeString.Problem; break;
                        case ConstantUtil.TicketType.Change: model.Type = ConstantUtil.TicketTypeString.Change; break;
                    }

                    switch (ticket.Status)
                    {
                        case ConstantUtil.TicketStatus.New: model.Status = "New"; break;
                        case ConstantUtil.TicketStatus.Assigned: model.Status = "Assigned"; break;
                        case ConstantUtil.TicketStatus.Solved: model.Status = "Solved"; break;
                        case ConstantUtil.TicketStatus.Unapproved: model.Status = "Unapproved"; break;
                        case ConstantUtil.TicketStatus.Cancelled: model.Status = "Cancelled"; break;
                        case ConstantUtil.TicketStatus.Closed: model.Status = "Closed"; break;
                    }

                    model.Category = (ticket.Category == null) ? "-" : ticket.Category.Name;
                    model.Impact = (ticket.Impact == null) ? "-" : ticket.Impact.Name;
                    model.ImpactDetail = (ticket.ImpactDetail == null) ? "-" : ticket.ImpactDetail;
                    model.Urgency = (ticket.Urgency == null) ? "-" : ticket.Urgency.Name;
                    model.Priority = (ticket.Priority == null) ? "-" : ticket.Priority.Name;
                    model.CreateTime = ticket.CreatedTime;
                    model.ModifiedTime = ticket.ModifiedTime;
                    model.ScheduleEndTime = ticket.ScheduleEndDate;
                    model.ScheduleStartTime = ticket.ScheduleStartDate;
                    model.ActualStartTime = ticket.ActualStartDate;
                    model.ActualEndTime = ticket.ActualEndDate;
                    model.CreatedBy = (createdUser == null) ? "-" : createdUser.Fullname;
                    model.AssignedBy = (assigner == null) ? "-" : assigner.Fullname;
                    model.SolvedBy = (solvedUser == null) ? "-" : solvedUser.Fullname;
                    model.Solution = ticket.Solution;
                    model.UnapproveReason = (string.IsNullOrEmpty(ticket.UnapproveReason)) ? "-" : ticket.UnapproveReason;
                    return View(model);
                }
            }
            return HttpNotFound();
        }

        //[CustomAuthorize(Roles = "Helpdesk,Technician")]
        //[HttpPost]
        //public ActionResult Solve(int id, TicketSolveViewModel model, string command)
        //{
        //    Ticket ticket = _ticketService.GetTicketByID(id);
        //    if (ticket == null)
        //    {
        //        return HttpNotFound();
        //    }


        //    switch (command)
        //    {
        //        case "Solve":
        //            if (ModelState.IsValid)
        //            {
        //                ticket.SolveID = User.Identity.GetUserId();
        //                ticket.SolvedDate = DateTime.Now;
        //                ticket.ModifiedTime = DateTime.Now;
        //                ticket.Solution = model.Solution;
        //                _ticketService.SolveTicket(ticket);
        //                return RedirectToAction("Index");
        //            }
        //            break;
        //        case "Save":
        //            if (ModelState.IsValid)
        //            {
        //                ticket.ModifiedTime = DateTime.Now;
        //                ticket.Solution = model.Solution;
        //                _ticketService.UpdateTicket(ticket);
        //                return RedirectToAction("Index");
        //            }
        //            break;
        //    }

        //    // Get Ticket information
        //    AspNetUser solvedUser = _userService.GetUserById(ticket.SolveID);
        //    AspNetUser createdUser = _userService.GetUserById(ticket.CreatedID);
        //    AspNetUser assigner = _userService.GetUserById(ticket.AssignedByID);

        //    model.ID = ticket.ID;
        //    model.Subject = ticket.Subject;
        //    model.Description = ticket.Description;

        //    switch (ticket.Mode)
        //    {
        //        case 1: model.Mode = ConstantUtil.TicketModeString.PhoneCall; break;
        //        case 2: model.Mode = ConstantUtil.TicketModeString.WebForm; break;
        //        case 3: model.Mode = ConstantUtil.TicketModeString.Email; break;
        //    }

        //    switch (ticket.Type)
        //    {
        //        case 1: model.Type = ConstantUtil.TicketTypeString.Request; break;
        //        case 2: model.Type = ConstantUtil.TicketTypeString.Problem; break;
        //        case 3: model.Type = ConstantUtil.TicketTypeString.Change; break;
        //    }

        //    switch (ticket.Status)
        //    {
        //        case 1: model.Status = "New"; break;
        //        case 2: model.Status = "Assigned"; break;
        //        case 3: model.Status = "Solved"; break;
        //        case 4: model.Status = "Unapproved"; break;
        //        case 5: model.Status = "Cancelled"; break;
        //        case 6: model.Status = "Closed"; break;
        //    }

        //    model.Category = (ticket.Category == null) ? "-" : ticket.Category.Name;
        //    model.Impact = (ticket.Impact == null) ? "-" : ticket.Impact.Name;
        //    model.ImpactDetail = (ticket.ImpactDetail == null) ? "-" : ticket.ImpactDetail;
        //    model.Urgency = (ticket.Urgency == null) ? "-" : ticket.Urgency.Name;
        //    model.Priority = (ticket.Priority == null) ? "-" : ticket.Priority.Name;
        //    model.CreateTime = ticket.CreatedTime;
        //    model.ModifiedTime = ticket.ModifiedTime;
        //    model.ScheduleEndTime = ticket.ScheduleEndDate;
        //    model.ScheduleStartTime = ticket.ScheduleStartDate;
        //    model.ActualStartTime = ticket.ActualStartDate;
        //    model.ActualEndTime = ticket.ActualEndDate;
        //    model.CreatedBy = (createdUser == null) ? "-" : createdUser.Fullname;
        //    model.AssignedBy = (assigner == null) ? "-" : assigner.Fullname;
        //    model.SolvedBy = (solvedUser == null) ? "-" : solvedUser.Fullname;
        //    model.UnapproveReason = (string.IsNullOrEmpty(ticket.UnapproveReason)) ? "-" : ticket.UnapproveReason;

        //    return View(model);
        //}

        [CustomAuthorize(Roles = "Helpdesk,Technician")]
        [HttpPost]
        public ActionResult SolveTicket(int? id, string solution, string command)
        {
            if (id.HasValue)
            {
                AspNetUser user = _userService.GetUserById(User.Identity.GetUserId());
                string userRole = user.AspNetRoles.FirstOrDefault().Name;

                Ticket ticket = _ticketService.GetTicketByID(id.Value);
                if (ticket == null)
                {
                    return Json(new
                    {
                        success = false,
                        msg = ConstantUtil.CommonError.UnavailableTicket
                    });
                }

                if (string.IsNullOrWhiteSpace(solution))
                {
                    return Json(new
                    {
                        success = false,
                        msg = "Please enter solution!"
                    });
                }

                ticket.ModifiedTime = DateTime.Now;
                ticket.Solution = solution;
                string message = "";
                switch (command)
                {
                    case "solveBtn":
                        ticket.SolveID = User.Identity.GetUserId();
                        ticket.SolvedDate = DateTime.Now;
                        _ticketService.SolveTicket(ticket);
                        message = "Ticket was solved!";
                        break;
                    case "saveBtn":
                        _ticketService.UpdateTicket(ticket);
                        message = "Solution saved!";
                        break;
                }
                return Json(new
                {
                    success = true,
                    msg = message,
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
                            ticket.Status = ConstantUtil.TicketStatus.Unapproved;
                            ticket.UnapproveReason = unapprovedReason;
                            ticket.ModifiedTime = DateTime.Now;
                            try
                            {
                                _ticketService.UpdateTicket(ticket);
                                return Json(new
                                {
                                    success = true,
                                    msg = "Thank you for your feedback!"
                                });
                            }
                            catch
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
        public ActionResult SearchSolution(string searchtxt)
        {
            IEnumerable<Solution> solutions;
            if (!string.IsNullOrEmpty(searchtxt))
            {
                solutions = _solutionService.SearchSolutions(searchtxt);
            }
            else
            {
                solutions = _solutionService.GetAllSolutions();
            }

            if (solutions == null)
            {
                return Json(new
                {
                    success = false,
                    error = true,
                    msg = "No result!"
                });
            }
            List<Solution> result = solutions.ToList();
            List<SolutionViewModel> data = new List<SolutionViewModel>();
            foreach (var item in result)
            {
                SolutionViewModel model = new SolutionViewModel();
                model.Id = item.ID;
                model.Subject = item.Subject;
                model.ContentText = item.ContentText;
                data.Add(model);
            }
            return Json(new
            {
                success = true,
                data = data,
                msg = "Search finished!"
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
            var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
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
