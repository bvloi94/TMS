﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TMS.DAL;
using TMS.Enumerator;
using TMS.Models;
using TMS.Services;
using TMS.ViewModels;

namespace TMS.Areas.HelpDesk.Controllers
{
    public class ManageTicketController : Controller
    {
        private TMSEntities db = new TMSEntities();

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

        // GET: HelpDesk/ManageTicket/Create
        public ActionResult Create()
        {
            ViewBag.SolveID = new SelectList(db.AspNetUsers, "Id", "Fullname");
            ViewBag.TechnicianID = new SelectList(db.AspNetUsers, "Id", "Fullname");
            ViewBag.RequesterID = new SelectList(db.AspNetUsers, "Id", "SecurityStamp");
            ViewBag.CreatedID = new SelectList(db.AspNetUsers, "Id", "SecurityStamp");
            ViewBag.CategoryID = new SelectList(db.Categories, "ID", "Name");
            //ViewBag.DepartmentID = new SelectList(db.Departments, "ID", "Name");
            ViewBag.ImpactID = new SelectList(db.Impacts, "ID", "Name");
            ViewBag.PriorityID = new SelectList(db.Priorities, "ID", "Name");
            ViewBag.UrgencyID = new SelectList(db.Urgencies, "ID", "Name");
            return View();
        }

        // POST: HelpDesk/ManageTicket/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Type,TechnicianID,RequesterID,ImpactID,ImpactDetail,UrgencyID,PriorityID,CategoryID,Subject,Description,Solution")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                ticket.Status = (int?)TicketStatusEnum.Open;
                ticket.CreatedTime = DateTime.Now;
                db.Tickets.Add(ticket);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.SolveID = new SelectList(db.AspNetUsers, "Id", "SecurityStamp", ticket.SolveID);
            ViewBag.TechnicianID = new SelectList(db.AspNetUsers, "Id", "SecurityStamp", ticket.TechnicianID);
            ViewBag.CreatedID = new SelectList(db.AspNetUsers, "Id", "SecurityStamp", ticket.CreatedID);
            ViewBag.CategoryID = new SelectList(db.Categories, "ID", "Name", ticket.CategoryID);
            //ViewBag.DepartmentID = new SelectList(db.Departments, "ID", "Name");
            ViewBag.ImpactID = new SelectList(db.Impacts, "ID", "Name", ticket.ImpactID);
            ViewBag.PriorityID = new SelectList(db.Priorities, "ID", "Name", ticket.PriorityID);
            ViewBag.UrgencyID = new SelectList(db.Urgencies, "ID", "Name", ticket.UrgencyID);
            return View(ticket);
        }

        // GET: HelpDesk/ManageTicket/Edit/5
        public ActionResult Edit(int? id)
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
            ViewBag.SolveID = new SelectList(db.AspNetUsers, "Id", "Fullname", ticket.SolveID);
            ViewBag.TechnicianID = new SelectList(db.AspNetUsers, "Id", "Fullname", ticket.TechnicianID);
            ViewBag.RequesterID = new SelectList(db.AspNetUsers, "Id", "Fullname", ticket.RequesterID);
            ViewBag.CreatedID = new SelectList(db.AspNetUsers, "Id", "Fullname", ticket.CreatedID);
            ViewBag.CategoryID = new SelectList(db.Categories, "ID", "Name", ticket.CategoryID);
            //ViewBag.DepartmentID = new SelectList(db.Departments, "ID", "Name", ticket.DepartmentID);
            ViewBag.ImpactID = new SelectList(db.Impacts, "ID", "Name", ticket.ImpactID);
            ViewBag.PriorityID = new SelectList(db.Priorities, "ID", "Name", ticket.PriorityID);
            ViewBag.UrgencyID = new SelectList(db.Urgencies, "ID", "Name", ticket.UrgencyID);
            return View(ticket);
        }

        // POST: HelpDesk/ManageTicket/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Type,Mode,SolveID,TechnicianID,RequesterID,ImpactID,ImpactDetail,UrgencyID,PriorityID,CategoryID,Status,Subject,Description,Solution,UnapproveReason,ScheduleStartDate,ScheduleEndDate,ActualStartDate,ActualEndDate,SolvedDate,CreatedTime,ModifiedTime,CreatedID")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                db.Entry(ticket).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.SolveID = new SelectList(db.AspNetUsers, "Id", "SecurityStamp", ticket.SolveID);
            ViewBag.TechnicianID = new SelectList(db.AspNetUsers, "Id", "SecurityStamp", ticket.TechnicianID);
            ViewBag.RequesterID = new SelectList(db.AspNetUsers, "Id", "SecurityStamp", ticket.RequesterID);
            ViewBag.CreatedID = new SelectList(db.AspNetUsers, "Id", "SecurityStamp", ticket.CreatedID);
            ViewBag.CategoryID = new SelectList(db.Categories, "ID", "Name", ticket.CategoryID);
            //ViewBag.DepartmentID = new SelectList(db.Departments, "ID", "Name", ticket.DepartmentID);
            ViewBag.ImpactID = new SelectList(db.Impacts, "ID", "Name", ticket.ImpactID);
            ViewBag.PriorityID = new SelectList(db.Priorities, "ID", "Name", ticket.PriorityID);
            ViewBag.UrgencyID = new SelectList(db.Urgencies, "ID", "Name", ticket.UrgencyID);
            return View(ticket);
        }

        // GET: HelpDesk/ManageTicket/Delete/5
        public ActionResult Delete(int? id)
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

        // POST: HelpDesk/ManageTicket/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Ticket ticket = db.Tickets.Find(id);
            db.Tickets.Remove(ticket);
            db.SaveChanges();
            return RedirectToAction("Index");
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
                p.Subject,
                p.RequesterID==null?"":_userService.GetUserById(p.RequesterID).Fullname,
                p.TechnicianID==null?"":_userService.GetUserById(p.TechnicianID).Fullname,
                //p.DepartmentID==null?"":_departmentService.GetDepartmentById((int) p.DepartmentID).Name,
                "",
                p.SolvedDate.ToString(),
                p.Status,
                p.CreatedTime.ToString(),
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

        public ActionResult LoadAllTickets(JqueryDatatableParameterViewModel model)
        {
            JqueryDatatableResultViewModel rsModel = new JqueryDatatableResultViewModel();
            rsModel.sEcho = model.sEcho;
            var queriedResult = _ticketService.GetAll();
            rsModel.iTotalRecords = queriedResult.Count();
            var result = queriedResult.Skip(model.iDisplayStart).Take(model.iDisplayLength).ToList();
            var tickets = new List<TicketViewModel>();
            int startNo = model.iDisplayStart;
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
                    s.AssignedTo = technician.Fullname;
                    s.Department = technician.DepartmentID == null
                        ? ""
                        : _departmentService.GetDepartmentById((int) technician.DepartmentID).Name;
                }
                else
                {
                    s.AssignedTo = "";
                    s.Department = "";
                }
                s.SolvedDate = item.SolvedDate?.ToString("dd/MM/yyyy") ?? "";
                s.Status = item.Status.HasValue ? ((TicketStatusEnum)item.Status).ToString() : "";
                s.CreatedTime = item.CreatedTime?.ToString("dd/MM/yyyy") ?? "";
                tickets.Add(s);
            }
            rsModel.aaData = tickets;
            rsModel.iTotalDisplayRecords = queriedResult.Count();
            return Json(rsModel, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetRequesterList(jQueryDataTableParamModel param)
        {
            var requesterList = _userService.GetRequesters();

            IEnumerable<AspNetUser> filteredListItems;
            if (!string.IsNullOrEmpty(param.sSearch))
            {
                filteredListItems = requesterList.Where(p => p.Fullname.ToLower().Contains(param.sSearch.ToLower()));
            }
            else
            {
                filteredListItems = requesterList;
            }
            // Sort.
            var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
            var sortDirection = Request["sSortDir_0"]; // asc or desc

            switch (sortColumnIndex)
            {
                case 2:
                    filteredListItems = sortDirection == "asc"
                        ? filteredListItems.OrderBy(p => p.Fullname)
                        : filteredListItems.OrderByDescending(p => p.Fullname);
                    break;
            }

            var displayedList = filteredListItems.Skip(param.start).Take(param.length);
            var result = displayedList.Select(p => new IConvertible[]{
                p.Id,
                p.Fullname,
                p.Email,
                p.DepartmentName,
                p.PhoneNumber,
                p.JobTitle
            }.ToArray());

            return Json(new
            {
                param.sEcho,
                iTotalRecords = result.Count(),
                iTotalDisplayRecords = filteredListItems.Count(),
                aaData = result
            }, JsonRequestBehavior.AllowGet);
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
