using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TMS.DAL;
using TMS.Models;
using TMS.Services;
using TMS.Utils;
using TMS.ViewModels;

namespace TMS.Areas.Technician.Controllers
{
    public class ManageTicketController : Controller
    {

        public TicketService _ticketService { get; set; }
        public UserService _userService { get; set; }
        public DepartmentService _departmentService { get; set; }

        public ManageTicketController()
        {
            var unitOfWork = new UnitOfWork();
            _ticketService = new TicketService(unitOfWork);
            _userService = new UserService(unitOfWork);
            _departmentService = new DepartmentService(unitOfWork);
        }

        // GET: Technician/Ticket
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetTechnicianTickets(jQueryDataTableParamModel param)
        {
            // 1. Get Parameters
            string technicianId = User.Identity.GetUserId();
            var ticketList = _ticketService.GetTechnicianTickets(technicianId);
            var default_search_key = Request["search[value]"];
            var select_status = Request["select_status"];
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
                filteredListItems = filteredListItems.Where(p => p.Subject.ToLower().Contains(search_text.ToLower())
                    || p.Description.ToLower().Contains(search_text.ToLower()));
            }

            if (!string.IsNullOrEmpty(select_status))
            {
                switch (select_status)
                {
                    case "2":
                        filteredListItems = filteredListItems.Where(p => p.Status == 2);
                        break;
                    case "3":
                        filteredListItems = filteredListItems.Where(p => p.Status == 3);
                        break;
                    case "4":
                        filteredListItems = filteredListItems.Where(p => p.Status == 4);
                        break;
                    case "5":
                        filteredListItems = filteredListItems.Where(p => p.Status == 5);
                        break;
                    case "6":
                        filteredListItems = filteredListItems.Where(p => p.Status == 6);
                        break;
                    default: break;
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
                        ? filteredListItems.OrderBy(p => p.RequesterID)
                        : filteredListItems.OrderByDescending(p => p.RequesterID);
                    break;
                case 2:
                    filteredListItems = sortDirection == "asc"
                        ? filteredListItems.OrderBy(p => p.Subject)
                        : filteredListItems.OrderByDescending(p => p.Subject);
                    break;
                case 5:
                    filteredListItems = sortDirection == "asc"
                        ? filteredListItems.OrderBy(p => p.ModifiedTime)
                        : filteredListItems.OrderByDescending(p => p.ModifiedTime);
                    break;
                default: break;
            }

            var displayedList = filteredListItems.Skip(param.start).Take(param.length);
            var result = displayedList.Select(p => new IConvertible[]{
                p.CreatedTime.ToString(),
                p.RequesterID == null ? "" : _userService.GetUserById(p.RequesterID).Fullname,
                p.Subject,
                p.Solution,
                p.Status,
                p.ModifiedTime.ToString(),
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
        public ActionResult GetTicketDetail(int id)
        {
            Ticket ticket = _ticketService.GetTicketByID(id);
            AspNetUser solver = _userService.GetUserById(ticket.SolveID);
            AspNetUser creater = _userService.GetUserById(ticket.CreatedID);
            AspNetUser assigner = _userService.GetUserById(ticket.AssignedByID);
            String ticketType, ticketMode;

            switch (ticket.Type)
            {
                case 1: ticketType = ConstantUtil.TicketTypeString.Request; break;
                case 2: ticketType = ConstantUtil.TicketTypeString.Problem; break;
                case 3: ticketType = ConstantUtil.TicketTypeString.Change; break;
                default: ticketType = "None"; break;
            }

            switch (ticket.Mode)
            {
                case 1: ticketMode = ConstantUtil.TicketModeString.PhoneCall; break;
                case 2: ticketMode = ConstantUtil.TicketModeString.WebForm; break;
                case 3: ticketMode = ConstantUtil.TicketModeString.Email; break;
                default: ticketMode = "None"; break;
            }

            return Json(new
            {
                id = ticket.ID,
                subject = ticket.Subject,
                description = ticket.Description,
                type = ticketType,
                mode = ticketMode,
                urgency = ticket.Urgency == null ? "None" : ticket.Urgency.Name,
                priority = ticket.Priority == null ? "None" : ticket.Priority.Name,
                category = ticket.Category == null ? "None" : ticket.Category.Name,
                impact = ticket.Impact == null ? "None" : ticket.Impact.Name,
                impactDetail = ticket.ImpactDetail == null ? "None" : ticket.ImpactDetail,
                status = ticket.Status,
                createdDate = ticket.CreatedTime.ToString(),
                lastModified = ticket.ModifiedTime.ToString(),
                scheduleStart = ticket.ScheduleStartDate.ToString(),
                scheduleEnd = ticket.ScheduleEndDate.ToString(),
                actualStart = ticket.ActualStartDate.ToString(),
                actualEnd = ticket.ActualEndDate.ToString(),
                solution = ticket.Solution == null ? "None": ticket.Solution,
                solver = solver == null ? "None" : solver.Fullname,
                creater = creater == null ? "None" : creater.Fullname,
                assigner = assigner == null ? "None" : assigner.Fullname,
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Solve(int id)
        {
            Ticket ticket = _ticketService.GetTicketByID(id);
            if (ticket.Status != ConstantUtil.TicketStatus.Assigned) // Ticket status is not "Assigned"
            {
                return RedirectToAction("Index"); // Redirect to Index so the Technician cannot go to Solve view.
            }
            else
            {
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
                    case 1: model.Mode = ConstantUtil.TicketModeString.PhoneCall; break;
                    case 2: model.Mode = ConstantUtil.TicketModeString.WebForm; break;
                    case 3: model.Mode = ConstantUtil.TicketModeString.Email; break;
                }

                switch (ticket.Type)
                {
                    case 1: model.Type = ConstantUtil.TicketTypeString.Request; break;
                    case 2: model.Type = ConstantUtil.TicketTypeString.Problem; break;
                    case 3: model.Type = ConstantUtil.TicketTypeString.Change; break;
                }

                switch (ticket.Status)
                {
                    case 1: model.Status = "New"; break;
                    case 2: model.Status = "Assigned"; break;
                    case 3: model.Status = "Solved"; break;
                    case 4: model.Status = "Unapproved"; break;
                    case 5: model.Status = "Cancelled"; break;
                    case 6: model.Status = "Closed"; break;
                }

                model.Category = (ticket.Category == null) ? "None" : ticket.Category.Name;
                model.Impact = (ticket.Impact == null) ? "None" : ticket.Impact.Name;
                model.ImpactDetail = (ticket.ImpactDetail == null) ? "None" : ticket.ImpactDetail;
                model.Urgency = (ticket.Urgency == null) ? "None" : ticket.Urgency.Name;
                model.Priority = (ticket.Priority == null) ? "None" : ticket.Priority.Name;
                model.CreateTime = ticket.CreatedTime;
                model.ModifiedTime = ticket.ModifiedTime;
                model.ScheduleEndTime = ticket.ScheduleEndDate;
                model.ScheduleStartTime = ticket.ScheduleStartDate;
                model.ActualStartTime = ticket.ActualStartDate;
                model.ActualEndTime = ticket.ActualEndDate;
                model.CreatedBy = (createdUser == null) ? "None" : createdUser.Fullname;
                model.AssignedBy = (createdUser == null) ? "None" : assigner.Fullname;
                model.SolvedBy = (solvedUser == null) ? "None" : solvedUser.Fullname;
                model.Solution = ticket.Solution;
                model.UnapproveReason = (string.IsNullOrEmpty(ticket.UnapproveReason)) ? "None" : ticket.UnapproveReason;

                return View(model);
            }
        }

        [HttpPost]
        public ActionResult Solve(int id, TicketSolveViewModel model, string command)
        {
            Ticket ticket = _ticketService.GetTicketByID(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }


            switch (command)
            {
                case "Solve":
                    if (ModelState.IsValid)
                    {
                        ticket.SolveID = User.Identity.GetUserId();
                        ticket.SolvedDate = DateTime.Now;
                        ticket.ModifiedTime = DateTime.Now;
                        ticket.Solution = model.Solution;
                        _ticketService.SolveTicket(ticket);
                        return RedirectToAction("Index");
                    }
                    break;
                case "Save":
                    if (ModelState.IsValid)
                    {
                        ticket.ModifiedTime = DateTime.Now;
                        ticket.Solution = model.Solution;
                        _ticketService.UpdateTicket(ticket);
                        return RedirectToAction("Index");
                    }
                    break;
            }

            // Get Ticket information
            AspNetUser solvedUser = _userService.GetUserById(ticket.SolveID);
            AspNetUser createdUser = _userService.GetUserById(ticket.CreatedID);
            AspNetUser assigner = _userService.GetUserById(ticket.AssignedByID);

            model.ID = ticket.ID;
            model.Subject = ticket.Subject;
            model.Description = ticket.Description;

            switch (ticket.Mode)
            {
                case 1: model.Mode = ConstantUtil.TicketModeString.PhoneCall; break;
                case 2: model.Mode = ConstantUtil.TicketModeString.WebForm; break;
                case 3: model.Mode = ConstantUtil.TicketModeString.Email; break;
            }

            switch (ticket.Type)
            {
                case 1: model.Type = ConstantUtil.TicketTypeString.Request; break;
                case 2: model.Type = ConstantUtil.TicketTypeString.Problem; break;
                case 3: model.Type = ConstantUtil.TicketTypeString.Change; break;
            }

            switch (ticket.Status)
            {
                case 1: model.Status = "New"; break;
                case 2: model.Status = "Assigned"; break;
                case 3: model.Status = "Solved"; break;
                case 4: model.Status = "Unapproved"; break;
                case 5: model.Status = "Cancelled"; break;
                case 6: model.Status = "Closed"; break;
            }

            model.Category = (ticket.Category == null) ? "None" : ticket.Category.Name;
            model.Impact = (ticket.Impact == null) ? "None" : ticket.Impact.Name;
            model.ImpactDetail = (ticket.ImpactDetail == null) ? "None" : ticket.ImpactDetail;
            model.Urgency = (ticket.Urgency == null) ? "None" : ticket.Urgency.Name;
            model.Priority = (ticket.Priority == null) ? "None" : ticket.Priority.Name;
            model.CreateTime = ticket.CreatedTime;
            model.ModifiedTime = ticket.ModifiedTime;
            model.ScheduleEndTime = ticket.ScheduleEndDate;
            model.ScheduleStartTime = ticket.ScheduleStartDate;
            model.ActualStartTime = ticket.ActualStartDate;
            model.ActualEndTime = ticket.ActualEndDate;
            model.CreatedBy = (createdUser == null) ? "None" : createdUser.Fullname;
            model.AssignedBy = (createdUser == null) ? "None" : assigner.Fullname;
            model.SolvedBy = (solvedUser == null) ? "None" : solvedUser.Fullname;
            model.UnapproveReason = (string.IsNullOrEmpty(ticket.UnapproveReason)) ? "None" : ticket.UnapproveReason;

            return View(model);
        }

    }
}