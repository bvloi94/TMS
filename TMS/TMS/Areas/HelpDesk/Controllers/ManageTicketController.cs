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
using ModelError = TMS.ViewModels.ModelError;

namespace TMS.Areas.HelpDesk.Controllers
{
    public class ManageTicketController : Controller
    {
        private TMSEntities db = new TMSEntities();

        public TicketService _ticketService { get; set; }
        public UserService _userService { get; set; }
        public DepartmentService _departmentService { get; set; }
        public UrgencyService _urgencyService { get; set; }
        public PriorityService _priorityService { get; set; }
        public ImpactService _impactService { get; set; }
        public CategoryService _categoryService { get; set; }


        public ManageTicketController()
        {
            var unitOfWork = new UnitOfWork();
            _ticketService = new TicketService(unitOfWork);
            _userService = new UserService(unitOfWork);
            _departmentService = new DepartmentService(unitOfWork);
            _urgencyService = new UrgencyService(unitOfWork);
            _priorityService = new PriorityService(unitOfWork);
            _impactService = new ImpactService(unitOfWork);
            _categoryService = new CategoryService(unitOfWork);
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

        public ActionResult CreateNewTicket()
        {
            return View();
        }


        public ActionResult AddNewTicket(TicketViewModel model)
        {
            ModelErrorViewModel errs = null;
            errs = new ModelErrorViewModel();

            if (model.Subject == null || model.Subject.ToString().Trim() == "")
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

            var ticket = new Ticket
            {
                Subject = model.Subject,
                Description = model.Description,
                Solution = model.Solution,
                RequesterID = model.RequesterId,
                Type = model.Type,
                Mode = model.Mode,
                ScheduleStartDate = model.ScheduleStartDate != null ? DateTime.ParseExact(model.ScheduleStartDate, "dd/MM/yyyy", null) : (DateTime?)null,
                ScheduleEndDate = model.ScheduleEndDate != null ? DateTime.ParseExact(model.ScheduleEndDate, "dd/MM/yyyy", null) : (DateTime?)null,
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
                Status = (int?)TicketStatusEnum.New
            };

            //TO-DO
            // ticket.CreatedID = 

            // Attachment

            if (model.UrgencyId != 0)
            {
                ticket.UrgencyID = model.UrgencyId;
            }
            if (model.PriorityId != 0)
            {
                ticket.PriorityID = model.PriorityId;
            }
            if (model.ImpactId != 0)
            {
                ticket.ImpactID = model.ImpactId;
            }
            if (model.CategoryId != 0)
            {
                ticket.CategoryID = model.CategoryId;
            }

            if (model.TechnicianId != null && model.TechnicianId.ToString().Trim() != "")
            {
                ticket.TechnicianID = model.TechnicianId;
                ticket.Status = (int?)TicketStatusEnum.Assigned;
            }

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

            try
            {
                _ticketService.AddTicket(ticket);
            }
            catch (Exception e)
            {
                return Json(new
                {
                    success = false,
                    data = errs,
                    msg = "Error occured! Please try again!"
                });
            }

            return Json(new
            {
                success = true
            });
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
                ticket.Status = (int?)TicketStatusEnum.New;
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

        public ActionResult EditTicket(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = _ticketService.GetTicketByID((int)id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            TicketViewModel model = new TicketViewModel();
            model.Id = ticket.ID;
            model.Subject = ticket.Subject;
            model.Description = ticket.Description;
            model.Solution = ticket.Solution;
            if (!string.IsNullOrEmpty(ticket.RequesterID))
            {
                model.RequesterId = ticket.RequesterID;
                model.Requester = _userService.GetUserById(ticket.RequesterID).Fullname;
            }
            if (ticket.Type != null) model.Type = (int)ticket.Type;
            model.Mode = ticket.Mode;
            if (ticket.Status != null)
            {
                model.StatusId = (int)ticket.Status;
                model.Status = ((TicketStatusEnum)ticket.Status).ToString();
            }
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
            model.UnapproveReason = ticket.UnapproveReason;
            model.ScheduleStartDate = ticket.ScheduleStartDate?.ToString("dd/MM/yyyy") ?? "";
            model.ScheduleEndDate = ticket.ScheduleEndDate?.ToString("dd/MM/yyyy") ?? "";
            model.ActualStartDate = ticket.ActualStartDate?.ToString("dd/MM/yyyy") ?? "";
            model.ActualEndDate = ticket.ActualEndDate?.ToString("dd/MM/yyyy") ?? "";
            model.SolvedDate = ticket.SolvedDate?.ToString("dd/MM/yyyy") ?? "";
            model.CreatedTime = ticket.CreatedTime?.ToString("dd/MM/yyyy") ?? "";
            model.ModifiedTime = ticket.ModifiedTime?.ToString("dd/MM/yyyy") ?? "";
            if (!string.IsNullOrEmpty(ticket.CreatedID))
            {
                model.CreatedId = ticket.CreatedID;
                model.CreatedBy = _userService.GetUserById(ticket.CreatedID).Fullname;
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
            return View(model);
        }

        public ActionResult UpdateTicket(TicketViewModel model)
        {
            ModelErrorViewModel errs = null;
            errs = new ModelErrorViewModel();
            Ticket ticket = _ticketService.GetTicketByID((int)model.Id);
            if (ticket == null)
            {
                return HttpNotFound();
            }

            if (string.IsNullOrEmpty(model.Subject))
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
            else
            {
                ticket.Subject = model.Subject;
            }
            ticket.Description = model.Description;
            ticket.Solution = model.Solution;
            if (!string.IsNullOrEmpty(model.RequesterId))
            {
                ticket.RequesterID = model.RequesterId;
            }
            if (!string.IsNullOrEmpty(model.TechnicianId))
            {
                if (ticket.TechnicianID != model.TechnicianId)
                {
                    ticket.TechnicianID = model.TechnicianId;
                    ticket.Status = (int?)TicketStatusEnum.Assigned;
                }
            }
            ticket.Type = model.Type;
            ticket.Mode = model.Mode;
            ticket.ScheduleStartDate = model.ScheduleStartDate != null
                ? DateTime.ParseExact(model.ScheduleStartDate, "dd/MM/yyyy", null)
                : (DateTime?)null;
            ticket.ScheduleEndDate = model.ScheduleEndDate != null
                ? DateTime.ParseExact(model.ScheduleEndDate, "dd/MM/yyyy", null)
                : (DateTime?)null;
            ticket.ActualStartDate = model.ActualStartDate != null
                ? DateTime.ParseExact(model.ActualStartDate, "dd/MM/yyyy", null)
                : (DateTime?)null;
            ticket.ActualEndDate = model.ActualEndDate != null
                ? DateTime.ParseExact(model.ActualEndDate, "dd/MM/yyyy", null)
                : (DateTime?)null;
            ticket.ModifiedTime = DateTime.Now;

            //TO-DO
            // ticket.CreatedID = 

            // Attachment

            if (model.UrgencyId != 0)
            {
                ticket.UrgencyID = model.UrgencyId;
            }
            if (model.PriorityId != 0)
            {
                ticket.PriorityID = model.PriorityId;
            }
            if (model.ImpactId != 0)
            {
                ticket.ImpactID = model.ImpactId;
            }
            if (model.CategoryId != 0)
            {
                ticket.CategoryID = model.CategoryId;
            }

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

            try
            {
                _ticketService.UpdateTicket(ticket);
            }
            catch (Exception e)
            {
                return Json(new
                {
                    success = false,
                    data = errs,
                    msg = "Error occured! Please try again!"
                });
            }

            return Json(new
            {
                success = true
            });
        }

        public ActionResult CancelTicket(int? ticketId)
        {

            bool result = _ticketService.CancelTicket(ticketId);
            if (!result) return Json(new
            {
                success = false,
                msg = "Error occured! Please try again!"
            });

            return Json(new
            {
                success = true
            });
        }

        public ActionResult MergeTicket(int[] selectedTickets)
        {
            List<Ticket> tickets = new List<Ticket>();
            string requesterId = "";
            for (int i = 0; i < selectedTickets.Length; i++)
            {
                Ticket ticket = _ticketService.GetTicketByID(selectedTickets[i]);
                if (ticket != null)
                {
                    if (i > 0 && requesterId != ticket.RequesterID)
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
            tickets.Sort((x, y) => DateTime.Compare((DateTime)x.CreatedTime, (DateTime)y.CreatedTime));
            Ticket newTicket = tickets[0];
            for (int i = 1; i < tickets.Count; i++)
            {
                Ticket oldTicket = tickets[i];
                newTicket.Description += "<br/>" + oldTicket.Description;
                oldTicket.ModifiedTime = DateTime.Now;
                oldTicket.Status = (int?)TicketStatusEnum.Canceled;
                _ticketService.UpdateTicket(oldTicket);
            }
            newTicket.ModifiedTime = DateTime.Now;
            _ticketService.UpdateTicket(newTicket);

            return Json(new
            {
                success = true
            });
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

        [HttpPost]
        public ActionResult LoadAllTickets(JqueryDatatableParameterViewModel model)
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
            }

            //Hide Canceled Tickets
            filteredListItems = filteredListItems.Where(p => p.Status != (int)TicketStatusEnum.Canceled);

            if (!string.IsNullOrEmpty(search_text))
            {
                filteredListItems = filteredListItems.Where(p => p.Subject.ToLower().Contains(search_text.ToLower()));
            }

            // Sort.
            var sortColumnIndex = Convert.ToInt32(Request["order[0][column]"]);
            var sortDirection = Request["order[0][dir]"];

            switch (sortColumnIndex)
            {
                case 2:
                    filteredListItems = sortDirection == "asc"
                        ? filteredListItems.OrderBy(p => p.Subject)
                        : filteredListItems.OrderByDescending(p => p.Subject);
                    break;
                case 6:
                    filteredListItems = sortDirection == "asc"
                        ? filteredListItems.OrderBy(p => p.Status)
                        : filteredListItems.OrderByDescending(p => p.Status);
                    break;
                case 7:
                    filteredListItems = sortDirection == "asc"
                        ? filteredListItems.OrderBy(p => p.CreatedTime)
                        : filteredListItems.OrderByDescending(p => p.CreatedTime);
                    break;
            }

            var result = filteredListItems.Skip(model.start).Take(model.length).ToList();
            var tickets = new List<TicketViewModel>();
            int startNo = model.start;
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
                s.SolvedDate = item.SolvedDate?.ToString("dd/MM/yyyy") ?? "";
                s.Status = item.Status.HasValue ? ((TicketStatusEnum)item.Status).ToString() : "";
                s.CreatedTime = item.CreatedTime?.ToString("dd/MM/yyyy") ?? "";
                tickets.Add(s);
            }
            JqueryDatatableResultViewModel rsModel = new JqueryDatatableResultViewModel();
            rsModel.sEcho = model.sEcho;
            rsModel.recordsTotal = queriedResult.Count();
            rsModel.recordsFiltered = filteredListItems.Count();
            rsModel.data = tickets;
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
