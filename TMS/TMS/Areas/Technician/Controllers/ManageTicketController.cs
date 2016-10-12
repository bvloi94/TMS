using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TMS.DAL;
using TMS.Models;
using TMS.Services;
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
                        ? filteredListItems.OrderBy(p => p.Subject)
                        : filteredListItems.OrderByDescending(p => p.Subject);
                    break;
                case 4:
                    filteredListItems = sortDirection == "asc"
                        ? filteredListItems.OrderBy(p => p.ModifiedTime)
                        : filteredListItems.OrderByDescending(p => p.ModifiedTime);
                    break;
                default: break;
            }

            var displayedList = filteredListItems.Skip(param.start).Take(param.length);
            var result = displayedList.Select(p => new IConvertible[]{
                p.CreatedTime.ToString(),
                p.Subject,
                p.Status,
                p.Solution,
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
            AspNetUser solveUser = _userService.GetUserById(ticket.SolveID);
            AspNetUser createdUser = _userService.GetUserById(ticket.CreatedID);
            String solveUsername;
            if (solveUser != null)
            {
                solveUsername = solveUser.Fullname;
            }
            else
            {
                solveUsername = "None";
            }

            return Json(new
            {
                id = ticket.ID,
                subject = ticket.Subject,
                description = ticket.Description,
                createdDate = ticket.CreatedTime.ToString(),
                status = ticket.Status,
                lastModified = ticket.ModifiedTime.ToString(),
                solution = ticket.Solution,
                solveUser = solveUsername,
                createBy = createdUser.Fullname,

            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Solve(int id)
        {
            Ticket ticket = _ticketService.GetTicketByID(id);
            if (ticket.Status != 2) // Ticket status is not "Assigned"
            {
                return RedirectToAction("Index"); // Redirect to Index so the Technician cannot go to Solve view.
            }
            else
            {
                // Get Ticket information
                AspNetUser solvedUser = _userService.GetUserById(ticket.SolveID);
                AspNetUser createdUser = _userService.GetUserById(ticket.CreatedID);
                TicketSolveViewModel model = new TicketSolveViewModel();

                model.ID = ticket.ID;
                model.Subject = ticket.Subject;
                model.Description = ticket.Description;
                model.Status = ticket.Status;
                model.CreateTime = ticket.CreatedTime;
                model.ModifiedTime = ticket.ModifiedTime;
                model.CreatedBy = createdUser.Fullname;

                if (!string.IsNullOrEmpty(ticket.SolveID))
                {
                    model.SolvedBy = solvedUser.Fullname;
                    model.Solution = ticket.Solution;
                }

                return View(model);
            }
        }

        [HttpPost]
        public ActionResult Solve(int id, TicketSolveViewModel model)
        {
            // Get Ticket information
            Ticket ticket = _ticketService.GetTicketByID(id);
            AspNetUser solvedUser = _userService.GetUserById(ticket.SolveID);
            AspNetUser createdUser = _userService.GetUserById(ticket.CreatedID);

            model.ID = ticket.ID;
            model.Subject = ticket.Subject;
            model.Description = ticket.Description;
            model.Status = ticket.Status;
            model.CreateTime = ticket.CreatedTime;
            model.ModifiedTime = ticket.ModifiedTime;
            model.CreatedBy = createdUser.Fullname;

            if (ticket == null)
            {
                return HttpNotFound();
            }
            else
            {
                if (ModelState.IsValid)
                {
                    ticket.Solution = model.Solution;
                    _ticketService.SolveTicket(ticket);
                    return RedirectToAction("Index");
                }
            }
            return View(model);
        }

    }
}