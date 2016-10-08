using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TMS.DAL;
using TMS.Models;
using TMS.Services;

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
        public ActionResult GetTicketDetail (int id)
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
            
            ViewBag.Ticket = ticket;
            ViewBag.SolveUser = solveUser;
            ViewBag.CreatedUser = createdUser;

            return View();
        }
    }
}