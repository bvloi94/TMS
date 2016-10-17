﻿using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TMS.DAL;
using TMS.Enumerator;
using TMS.Models;
using TMS.Services;
using TMS.Utils;
using TMS.ViewModels;

namespace TMS.Controllers
{
    public class TicketController : Controller
    {
        private TMSEntities db = new TMSEntities();
        public TicketService _ticketService { get; set; }
        public UserService _userService { get; set; }
        public DepartmentService _departmentService { get; set; }

        public TicketController()
        {
            var unitOfWork = new UnitOfWork();
            _ticketService = new TicketService(unitOfWork);
            _userService = new UserService(unitOfWork);
            _departmentService = new DepartmentService(unitOfWork);
            _ticketAttachmentService = new TicketAttachmentService(_unitOfWork);
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
        [ValidateInput(false)]
        public ActionResult Create(RequesterTicketViewModel model, IEnumerable<HttpPostedFileBase> uploadFiles)
        {
            if (ModelState.IsValid)
            {
                Ticket ticket = new Ticket();
                TicketAttachment ticketFiles = new TicketAttachment();

                ticket.Subject = model.Subject;
                ticket.Description = model.Description;
                ticket.Status = (int?)TicketStatusEnum.New;
                ticket.CreatedID = ticket.RequesterID = User.Identity.GetUserId();
                ticket.Mode = (int)TicketModeEnum.WebForm;
                ticket.CreatedTime = DateTime.Now;
                db.Tickets.Add(ticket);
                db.SaveChanges();

                if (uploadFiles.ToList()[0] != null && uploadFiles.ToList().Count > 0)
                {
                    _ticketAttachmentService.saveFile(ticket.ID, uploadFiles);
                    List<TicketAttachment> listFile = _unitOfWork.TicketAttachmentRepository.Get(i => i.TicketID == ticket.ID).ToList();
                    ticketFiles.Path = listFile[0].Path;
                    
                }



                return RedirectToAction("Index");
            }
            return View(model);
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
            //ViewBag.DepartmentID = new SelectList(db.Departments, "ID", "Name", ticket.DepartmentID);
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
            //ViewBag.DepartmentID = new SelectList(db.Departments, "ID", "Name", ticket.DepartmentID);
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

                return RedirectToAction("Index");
            }
            return View(model);
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
            //ViewBag.DepartmentID = new SelectList(db.Departments, "ID", "Name", ticket.DepartmentID);
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
            //ViewBag.DepartmentID = new SelectList(db.Departments, "ID", "Name", ticket.DepartmentID);
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
                solution = ticket.Solution == null ? "None" : ticket.Solution,
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
