using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Management;
using System.Web.Mvc;
using System.Web.Services.Description;
using TMS.DAL;
using TMS.Models;
using TMS.Services;
using TMS.ViewModels;

namespace TMS.Areas.Admin.Controllers
{
    public class ManageSCController : Controller
    {
        private TMSEntities db = new TMSEntities();
        private UnitOfWork _unitOfWork;
        private ImpactService _impactService;
        private UrgencyService _urgencyService;
        private PriorityService _priorityService;

        public ManageSCController()
        {
            _unitOfWork = new UnitOfWork();
            _impactService = new ImpactService(_unitOfWork);
            _urgencyService = new UrgencyService(_unitOfWork);
            _priorityService = new PriorityService(_unitOfWork);
        }

        // GET: Admin/ManageSC
        public ActionResult Priority()
        {
            return View();
        }

        public ActionResult Impact()
        {
            return View();

        }

        public ActionResult Urgency()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetImpacts(jQueryDataTableParamModel param)
        {
            var impactList = _impactService.GetAll();

            //var jsonData = new
            //{
            //    data = ticketList
            //};
            //return Json(jsonData, JsonRequestBehavior.AllowGet);

            IEnumerable<Impact> filteredListItems;
            if (!string.IsNullOrEmpty(param.sSearch))
            {
                filteredListItems = impactList.Where(p => p.Name.ToLower().Contains(param.sSearch.ToLower()));
            }
            else
            {
                filteredListItems = impactList;
            }
            // Sort.
            var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
            var sortDirection = Request["sSortDir_0"]; // asc or desc

            switch (sortColumnIndex)
            {
                case 2:
                    filteredListItems = sortDirection == "asc"
                        ? filteredListItems.OrderBy(p => p.Name)
                        : filteredListItems.OrderByDescending(p => p.Description);
                    break;
            }

            var displayedList = filteredListItems.Skip(param.start).Take(param.length);
            var result = displayedList.Select(p => new IConvertible[]
            {
                p.ID,
                p.Name,
                p.Description
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
        public ActionResult GetUrgency(jQueryDataTableParamModel param)
        {
            var urgencyList = _urgencyService.GetAll();

            //var jsonData = new
            //{
            //    data = ticketList
            //};
            //return Json(jsonData, JsonRequestBehavior.AllowGet);

            IEnumerable<Urgency> filteredListItems;
            if (!string.IsNullOrEmpty(param.sSearch))
            {
                filteredListItems = urgencyList.Where(p => p.Name.ToLower().Contains(param.sSearch.ToLower()));
            }
            else
            {
                filteredListItems = urgencyList;
            }
            // Sort.
            var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
            var sortDirection = Request["sSortDir_0"]; // asc or desc

            switch (sortColumnIndex)
            {
                case 2:
                    filteredListItems = sortDirection == "asc"
                        ? filteredListItems.OrderBy(p => p.Name)
                        : filteredListItems.OrderByDescending(p => p.Description);
                    break;
            }

            var displayedList = filteredListItems.Skip(param.start).Take(param.length);
            var result = displayedList.Select(p => new IConvertible[]
            {
                p.ID,
                p.Name,
                p.Description
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
        public ActionResult GetPriority(jQueryDataTableParamModel param)
        {
            var priorityList = _priorityService.GetAll();

            //var jsonData = new
            //{
            //    data = ticketList
            //};
            //return Json(jsonData, JsonRequestBehavior.AllowGet);

            IEnumerable<Priority> filteredListItems;
            if (!string.IsNullOrEmpty(param.sSearch))
            {
                filteredListItems = priorityList.Where(p => p.Name.ToLower().Contains(param.sSearch.ToLower()));
            }
            else
            {
                filteredListItems = priorityList;
            }
            // Sort.
            var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
            var sortDirection = Request["sSortDir_0"]; // asc or desc

            switch (sortColumnIndex)
            {
                case 2:
                    filteredListItems = sortDirection == "asc"
                        ? filteredListItems.OrderBy(p => p.Name)
                        : filteredListItems.OrderByDescending(p => p.Description);
                    break;
            }

            var displayedList = filteredListItems.Skip(param.start).Take(param.length);
            var result = displayedList.Select(p => new IConvertible[]
            {
                p.ID,
                p.Name,
                p.Description
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
        public ActionResult CreateImpact()
        {
            var name = Request["name"];
            var description = Request["description"];
            bool isDuplicatedName = _impactService.IsDuplicatedName(null, name);
            if (isDuplicatedName)
            {
                return Json(new
                {
                    success = false,
                    error = false,
                    message = string.Format("'{0}' has already been used.", name)
                });
            }
            else
            {
                Impact impact = new Impact();
                impact.Name = name;
                impact.Description = description;
                try
                {
                    _impactService.AddImpact(impact);
                    return Json(new
                    {
                        success = true,
                        message = "Create impact successfully!"
                    });
                }
                catch
                {
                    return Json(new
                    {
                        success = false,
                        error = true,
                        message = "Some errors occured. Please try again later!"
                    });
                }

            }
        }

        [HttpGet]
        public ActionResult GetImpactDetail()
        {
            try
            {
                int id = Int32.Parse(Request["id"]);
                Impact impact = _impactService.GetImpactByID(id);
                return Json(new
                {
                    success = true,
                    name = impact.Name,
                    description = impact.Description
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) when (ex is FormatException || ex is ArgumentNullException)
            {
                return Json(new
                {
                    success = false,
                    error = true,
                    message = "Cannot get impact detail!"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Admin/ManageSC/EditImpact
        [HttpPost]
        public ActionResult EditImpact()
        {
            try
            {
                var id = Int32.Parse(Request["id"]);
                var name = Request["name"];
                var description = Request["description"];
                bool isDuplicatedName = _impactService.IsDuplicatedName(id, name);
                if (isDuplicatedName)
                {
                    return Json(new
                    {
                        success = false,
                        error = false,
                        message = string.Format("'{0}' has already been used.", name)
                    });
                }
                else
                {
                    Impact impact = _impactService.GetImpactByID(id);
                    impact.Name = name;
                    impact.Description = description;

                    _impactService.UpdateImpact(impact);
                    return Json(new
                    {
                        success = true,
                        message = "Update impact successfully!"
                    });
                }
            }
            catch (Exception ex) when (ex is FormatException || ex is ArgumentNullException)
            {
                return Json(new
                {
                    success = false,
                    error = true,
                    message = "Cannot update!"
                });
            }
        }

        [HttpPost]
        public ActionResult CreatePriority()
        {
            var name = Request["name"];
            var description = Request["description"];
            bool isDuplicateName = _priorityService.IsDuplicateName(null, name);
            if (isDuplicateName)
            {
                return Json(new
                {
                    success = false,
                    error = false,
                    message = string.Format("'{0}' has already been used. ", name)
                });
            }
            else
            {
                Priority priority = new Priority();
                priority.Name = name;
                priority.Description = description;
                try
                {
                    _priorityService.AddPriority(priority);
                    return Json(new
                    {
                        success = true,
                        message = "Create priority sucessfull"
                    });
                }
                catch (Exception)
                {
                    return Json(new
                    {
                        success = false,
                        error = true,
                        message = "Some errors occured. Please try again later!"
                    });

                }



            }

        }

        [HttpGet]
        public ActionResult GetPriorityDetail()
        {
            try
            {
                int id = Int32.Parse(Request["id"]);
                Priority priority = _priorityService.GetPriorityByID(id);
                return Json(new
                {
                    success = true,
                    name = priority.Name,
                    description = priority.Description
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) when (ex is FormatException || ex is ArgumentNullException)
            {
                return Json(new
                {
                    success = false,
                    error = true,
                    message = "Cannot get priority detail!"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult CreateUrgency()
        {
            var name = Request["name"];
            var description = Request["description"];
            bool isDuplicatedName = _urgencyService.IsDuplicatedName(null, name);
            if (isDuplicatedName)
            {
                return Json(new
                {
                    success = false,
                    error = false,
                    message = string.Format("'{0}' has already been used.", name)
                });
            }
            else
            {
                Urgency urgency = new Urgency();
                urgency.Name = name;
                urgency.Description = description;
                try
                {
                    _urgencyService.AddUrgency(urgency);
                    return Json(new
                    {
                        success = true,
                        message = "Create urgency successfully!"
                    });
                }
                catch
                {
                    return Json(new
                    {
                        success = false,
                        error = true,
                        message = "Some errors occured. Please try again later!"
                    });
                }

            }
        }

        [HttpGet]
        public ActionResult GetUrgencyDetail()
        {
            try
            {
                int id = Int32.Parse(Request["id"]);
                Urgency urgency = _urgencyService.GetUrgencyByID(id);
                return Json(new
                {
                    success = true,
                    name = urgency.Name,
                    description = urgency.Description
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) when (ex is FormatException || ex is ArgumentNullException)
            {
                return Json(new
                {
                    success = false,
                    error = true,
                    message = "Cannot get urgency detail!"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditUrgency()
        {
            try
            {
                var id = int.Parse(Request["id"]);
                var name = Request["name"];
                var description = Request["description"];
                bool isDuplicatedName = _urgencyService.IsDuplicatedName(id, name);
                if (isDuplicatedName)
                {
                    return Json(new
                    {
                        success = false,
                        error = false,
                        message = string.Format("'{0}' has already been used.", name)
                    });
                }
                else
                {
                    Urgency urgency = _urgencyService.GetUrgencyByID(id);
                    urgency.Name = name;
                    urgency.Description = description;

                    _urgencyService.UpdateUrgency(urgency);
                    return Json(new
                    {
                        success = true,
                        message = "Update urgency successfully!"
                    });
                }
            }
            catch (Exception ex) when (ex is FormatException || ex is ArgumentNullException)
            {
                return Json(new
                {
                    success = false,
                    error = true,
                    message = "Cannot update!"
                });
            }
        }

        [HttpPost]
        public ActionResult EditPriority()
        {
            try
            {
                int id = Int32.Parse(Request["id"]);
                var name = Request["name"];
                var description = Request["description"];
                bool isDuplicatedName = _priorityService.IsDuplicatedName(id, name);
                if (isDuplicatedName)
                {
                    return Json(new
                    {
                        success = false,
                        error = false,
                        message = string.Format("'{0}' has already been used.", name)
                    });
                }
                else
                {
                    Priority priority = _priorityService.GetPriorityByID(id);
                    priority.Name = name;
                    priority.Description = description;

                    _priorityService.UpdatePriority(priority);
                    return Json(new
                    {
                        success = true,
                        message = "Update urgency successfully!"
                    });
                }
            }
            catch (Exception ex) when (ex is FormatException || ex is ArgumentNullException)
            {
                return Json(new
                {
                    success = false,
                    error = true,
                    message = "Cannot update!"
                });
            }


        }
    }
}