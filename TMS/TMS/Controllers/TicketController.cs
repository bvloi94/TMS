﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TMS.DAL;
using TMS.Models;
using TMS.Services;

namespace TMS.Controllers
{
    public class TicketController : Controller
    {
        private TMSEntities db = new TMSEntities();
        private UnitOfWork _unitOfWork;
        private TicketService _ticketService;

        public TicketController()
        {
            _unitOfWork = new UnitOfWork();
            _ticketService = new TicketService(_unitOfWork);
        }
        // GET: Tickets
        public ActionResult Index()
        {
            return View();
        }

        // GET: Tickets/Details/5
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

        // GET: Tickets/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Type,Mode,SolveID,TechnicianID,DepartmentID,RequesterID,ImpactID,ImpactDetail,UrgencyID,PriorityID,CategoryID,Status,Subject,Description,Solution,UnapproveReason,ScheduleStartDate,ScheduleEndDate,ActualStartDate,ActualEndDate,SolvedDate,CreatedTime,ModifiedTime,CreatedID")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                db.Tickets.Add(ticket);
                try
                {

                    db.SaveChanges();
                }
                catch (DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    throw;
                }
                return RedirectToAction("Index");
            }

            ViewBag.SolveID = new SelectList(db.AspNetUsers, "Id", "SecurityStamp", ticket.SolveID);
            ViewBag.TechnicianID = new SelectList(db.AspNetUsers, "Id", "SecurityStamp", ticket.TechnicianID);
            ViewBag.RequesterID = new SelectList(db.AspNetUsers, "Id", "SecurityStamp", ticket.RequesterID);
            ViewBag.CreatedID = new SelectList(db.AspNetUsers, "Id", "SecurityStamp", ticket.CreatedID);
            ViewBag.CategoryID = new SelectList(db.Categories, "ID", "Name", ticket.CategoryID);
            ViewBag.DepartmentID = new SelectList(db.Departments, "ID", "Name", ticket.DepartmentID);
            ViewBag.ImpactID = new SelectList(db.Impacts, "ID", "Name", ticket.ImpactID);
            ViewBag.PriorityID = new SelectList(db.Priorities, "ID", "Name", ticket.PriorityID);
            ViewBag.UrgencyID = new SelectList(db.Urgencies, "ID", "Name", ticket.UrgencyID);
            return View(ticket);
        }

        // GET: Tickets/Edit/5
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
            ViewBag.SolveID = new SelectList(db.AspNetUsers, "Id", "SecurityStamp", ticket.SolveID);
            ViewBag.TechnicianID = new SelectList(db.AspNetUsers, "Id", "SecurityStamp", ticket.TechnicianID);
            ViewBag.RequesterID = new SelectList(db.AspNetUsers, "Id", "SecurityStamp", ticket.RequesterID);
            ViewBag.CreatedID = new SelectList(db.AspNetUsers, "Id", "SecurityStamp", ticket.CreatedID);
            ViewBag.CategoryID = new SelectList(db.Categories, "ID", "Name", ticket.CategoryID);
            ViewBag.DepartmentID = new SelectList(db.Departments, "ID", "Name", ticket.DepartmentID);
            ViewBag.ImpactID = new SelectList(db.Impacts, "ID", "Name", ticket.ImpactID);
            ViewBag.PriorityID = new SelectList(db.Priorities, "ID", "Name", ticket.PriorityID);
            ViewBag.UrgencyID = new SelectList(db.Urgencies, "ID", "Name", ticket.UrgencyID);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Type,Mode,SolveID,TechnicianID,DepartmentID,RequesterID,ImpactID,ImpactDetail,UrgencyID,PriorityID,CategoryID,Status,Subject,Description,Solution,UnapproveReason,ScheduleStartDate,ScheduleEndDate,ActualStartDate,ActualEndDate,SolvedDate,CreatedTime,ModifiedTime,CreatedID")] Ticket ticket)
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
            ViewBag.DepartmentID = new SelectList(db.Departments, "ID", "Name", ticket.DepartmentID);
            ViewBag.ImpactID = new SelectList(db.Impacts, "ID", "Name", ticket.ImpactID);
            ViewBag.PriorityID = new SelectList(db.Priorities, "ID", "Name", ticket.PriorityID);
            ViewBag.UrgencyID = new SelectList(db.Urgencies, "ID", "Name", ticket.UrgencyID);
            return View(ticket);
        }

        // GET: Tickets/Delete/5
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

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Ticket ticket = db.Tickets.Find(id);
            db.Tickets.Remove(ticket);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        [HttpGet]
        public ActionResult GetTickets(jQueryDataTableParamModel param)
        {
            var ticketList = _ticketService.GetAll();

            //var jsonData = new
            //{
            //    data = ticketList
            //};
            //return Json(jsonData, JsonRequestBehavior.AllowGet);

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
        
    }
}