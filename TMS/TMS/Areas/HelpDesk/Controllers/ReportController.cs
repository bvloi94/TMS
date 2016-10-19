using System;
using System.Collections.Generic;
//using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LumiSoft.Net.IMAP.Client;
using TMS.DAL;
using TMS.Models;
using TMS.Services;
using static TMS.Utils.ConstantUtil;

namespace TMS.Areas.HelpDesk.Controllers
{
    public class ReportController : Controller
    {
        private TicketService _ticketService { get; set; }
        public UserService _userService { get; set; }
        public ImpactService _impactService { get; set; }
        public CategoryService _categoryService { get; set; }
        public UrgencyService _urgencyService { get; set; }
        public PriorityService _priorityService { get; set; }

        public ReportController()
        {
            var _UnitofWork = new UnitOfWork();
            _ticketService = new TicketService(_UnitofWork);
            _userService = new UserService(_UnitofWork);
            _impactService = new ImpactService(_UnitofWork);
            _categoryService = new CategoryService(_UnitofWork);
            _urgencyService = new UrgencyService(_UnitofWork);
            _priorityService = new PriorityService(_UnitofWork);
        }
        // GET: HelpDesk/Report
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult GetTickets(jQueryDataTableParamModel param)
        {
            var ticketList = _ticketService.GetAll();
            var defaultSearchKey = Request["search[Value]"];
            var date_from_select = Request["date_from_select"];
            var date_to_select = Request["date_to_select"];

            IEnumerable<Ticket> filteredListItems;

            if (string.IsNullOrEmpty(defaultSearchKey))
            {
                filteredListItems = ticketList;
            }
            else
            {
                filteredListItems = ticketList.Where(p => p.Subject.ToLower().Contains(defaultSearchKey));
            }

            if (!string.IsNullOrEmpty(date_from_select))
            {
                DateTime fromDate = (DateTime)(date_from_select != null
                ? DateTime.ParseExact(date_from_select, "dd/MM/yyyy", null)
                : (DateTime?)null);
                fromDate = fromDate.Date;
                filteredListItems = filteredListItems.Where(p => DateTime.Compare(p.CreatedTime.Date, fromDate.Date) >= 0);
            }
            if (!string.IsNullOrEmpty(date_to_select))
            {
                DateTime toDate = (DateTime)(date_to_select != null
                ? DateTime.ParseExact(date_to_select, "dd/MM/yyyy", null)
                : (DateTime?)null);
                filteredListItems = filteredListItems.Where(p => DateTime.Compare(p.CreatedTime.Date, toDate.Date) <= 0);
            }
            // Sort.
            var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
            var sortDirection = Request["sSortDir_0"]; // asc or desc

            switch (sortColumnIndex)
            {
                case 2:
                    filteredListItems = sortDirection == "asc"
                        ? filteredListItems.OrderBy(p => p.ID)
                        : filteredListItems.OrderByDescending(p => p.Subject);
                    break;
            }

            var displayedList = filteredListItems.Skip(param.start).Take(param.length);
            var result = filteredListItems.Select(p => new IConvertible[]
            {
                p.Subject,
                _userService.GetUserById(p.RequesterID).Fullname,
                (p.TechnicianID == null) ? "-" : _userService.GetUserById(p.TechnicianID).Fullname,
                (p.SolvedDate== null) ? "-" : ((DateTime) p.SolvedDate).ToString("dd/MM/yyyy"),
                (p.CreatedTime == null) ? "" : ((DateTime) p.CreatedTime).ToString("dd/MM/yyyy"),
                p.Status,
                p.Type
                });

            return Json(new
            {
                param.sEcho,
                iTotalRecords = result.Count(),
                iTotalDisplayRecords = filteredListItems.Count(),
                aaData = result
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost] //int type, int by, 
        public ActionResult DrawGraph(int type, int by, string date_from_select, string date_to_select)
        {


            var ticketList = _ticketService.GetAll();
            IEnumerable<Ticket> filteredListItems;
            filteredListItems = ticketList;
            switch (type)
            {
                // All Request 
                case 1:
                    break;
                    {
                        filteredListItems = ticketList.Where(p => p.Type == TicketType.Request);
                        break;
                    }
                // All Problems
                case 2:
                    {
                        filteredListItems = ticketList.Where(p => p.Type == TicketType.Problem);
                        break;
                    }
                // All Request
                case 3:
                    {
                        filteredListItems = ticketList.Where(p => p.Type == TicketType.Change);
                        break;
                    }
                // All pending requests
                case 4:
                    {
                        break;
                    }
                // All pending problems
                case 5:
                    {
                        break;
                    }
                // All pending changes
                case 6:
                    {
                        break;
                    }
                // All tickets
                default:
                    {
                        filteredListItems = ticketList;
                        break;
                    }
            }

            List<string> labels = new List<string>();
            List<int> data = new List<int>();
            switch (by)
            {

                //Category
                case 1:
                    {
                        IEnumerable<Category> categories = _categoryService.GetCategories();
                        // cataglory list 
                        IEnumerable<Ticket> categoryList = null;
                        foreach (var category in categories)
                        {
                            labels.Add(category.Name);
                            categoryList = filteredListItems.Where(p => p.CategoryID == category.ID);
                            data.Add(categoryList.Count());
                        }
                        return Json(new
                        {
                            label = labels,
                            data = data

                        }, JsonRequestBehavior.AllowGet);

                    }

                // Impact
                case 2:
                    {
                        //_impacServie.
                        IEnumerable<Impact> impacts = _impactService.GetAll();
                        //duyet list 
                        IEnumerable<Ticket> impactInTicketList = null;
                        foreach (var impact in impacts)
                        {
                            labels.Add(impact.Name);
                            impactInTicketList = filteredListItems.Where(p => p.ImpactID == impact.ID);
                            data.Add(impactInTicketList.Count());
                        }
                        return Json(new
                        {
                            label = labels,
                            data = data,
                        }, JsonRequestBehavior.AllowGet);
                    }
                //Urgency
                case 3:
                    {
                        IEnumerable<Urgency> urgencyList = _urgencyService.GetAll();
                        IEnumerable<Ticket> urgencyListInTickets = null;
                        foreach (var urgency in urgencyList)
                        {
                            labels.Add(urgency.Name);
                            urgencyListInTickets = filteredListItems.Where(p => p.UrgencyID == urgency.ID);
                            data.Add(urgencyListInTickets.Count());
                        }
                        return Json(new
                        {
                            label = labels,
                            data = data
                        }, JsonRequestBehavior.AllowGet);
                    }

                // Priority
                case 4:
                {
                    IEnumerable<Priority> priorityList = _priorityService.GetAll();
                    IEnumerable<Ticket> priorityInTickets = new List<Ticket>();
                    foreach (var priority in priorityList)
                    {
                            labels.Add(priority.Name);
                            priorityInTickets = filteredListItems.Where(p => p.PriorityID == priority.ID);
                            data.Add(priorityInTickets.Count());
                    }
                    return Json(new
                    {
                        label= labels,
                        data = data

                    });

                }
                // Group
                case 5: break;
                // Status
                case 6: break;
                // Choose one
                default: break;

            }
            if (!string.IsNullOrEmpty(date_from_select))
            {
                DateTime fromDate = (DateTime)(date_from_select != null
                ? DateTime.ParseExact(date_from_select, "dd/MM/yyyy", null)
                : (DateTime?)null);
                fromDate = fromDate.Date;
                filteredListItems = filteredListItems.Where(p => DateTime.Compare(p.CreatedTime.Date, fromDate.Date) >= 0);
            }
            if (!string.IsNullOrEmpty(date_to_select))
            {
                DateTime toDate = (DateTime)(date_to_select != null
                ? DateTime.ParseExact(date_to_select, "dd/MM/yyyy", null)
                : (DateTime?)null);
                filteredListItems = filteredListItems.Where(p => DateTime.Compare(p.CreatedTime.Date, toDate.Date) <= 0);
            }

            //if (type == 1)

            //List<string> labels = new List<string>();
            //List<int> data = new List<int>();
            ////_impacServie.
            //IEnumerable<Impact> impacts = _impactService.GetAll();
            ////duyet list 
            //IEnumerable<Ticket> impactInTicketList = null;
            //foreach (var impact in impacts)
            //{
            //    labels.Add(impact.Name);
            //    impactInTicketList = filteredListItems.Where(p => p.ImpactID == impact.ID);
            //    data.Add(impactInTicketList.Count());
            //}
            //return Json(new
            //{
            //    label = labels,
            //    data = data,
            //}, JsonRequestBehavior.AllowGet);
            return null;
        }
    }
}