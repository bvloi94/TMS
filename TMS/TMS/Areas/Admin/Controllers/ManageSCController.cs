using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TMS.DAL;
using TMS.Models;
using TMS.Services;
using TMS.Utils;
using TMS.ViewModels;

namespace TMS.Areas.Admin.Controllers
{
    public class ManageSCController : Controller
    {
        private UnitOfWork _unitOfWork;
        private ImpactService _impactService;
        private UrgencyService _urgencyService;
        private PriorityService _priorityService;
        private CategoryService _categoryService;
        private PriorityMatrixService _priorityMatrixService;
        private ILog log = LogManager.GetLogger(typeof(ManageSCController));

        public ManageSCController()
        {
            _unitOfWork = new UnitOfWork();
            _impactService = new ImpactService(_unitOfWork);
            _urgencyService = new UrgencyService(_unitOfWork);
            _priorityService = new PriorityService(_unitOfWork);
            _categoryService = new CategoryService(_unitOfWork);
            _priorityMatrixService = new PriorityMatrixService(_unitOfWork);
        }

        // GET: Admin/ManageSC/Priority
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

        // GET: Admin/ManageSC/Category
        [HttpGet]
        public ActionResult Category()
        {
            return View();
        }

        [HttpGet]
        public ActionResult PriorityMatrix()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetImpacts(jQueryDataTableParamModel param)
        {
            var impactList = _impactService.GetAll();
            var default_search_key = Request["search[value]"];

            IEnumerable<Impact> filteredListItems;
            if (!string.IsNullOrEmpty(default_search_key))
            {
                filteredListItems = impactList.Where(p => p.Name.ToLower().Contains(default_search_key.ToLower()));
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

            var displayedList = filteredListItems;
            if (param.length != -1)
            {
                displayedList = filteredListItems.Skip(param.start).Take(param.length);
            }

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
            var default_search_key = Request["search[value]"];
           
            IEnumerable<Urgency> filteredListItems;
            if (!string.IsNullOrEmpty(default_search_key))
            {
                filteredListItems = urgencyList.Where(p => p.Name.ToLower().Contains(default_search_key.ToLower()));
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

            var displayedList = filteredListItems;
            if (param.length != -1)
            {
                displayedList = filteredListItems.Skip(param.start).Take(param.length);
            }

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
            var default_search_key = Request["search[value]"];
           
            IEnumerable<Priority> filteredListItems;
            if (!string.IsNullOrEmpty(default_search_key))
            {
                filteredListItems = priorityList.Where(p => p.Name.ToLower().Contains(default_search_key.ToLower()));
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


            var displayedList = filteredListItems;
            if (param.length != -1)
            {
                displayedList = filteredListItems.Skip(param.start).Take(param.length);
            }

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
        public ActionResult GetCategoriesDataTable(jQueryDataTableParamModel param)
        {
            var categoryList = _categoryService.GetCategories();
            var default_search_key = Request["search[value]"];

            IEnumerable<Category> filteredListItems;
            if (!string.IsNullOrEmpty(default_search_key))
            {
                filteredListItems = categoryList.Where(p => p.Name.ToLower().Contains(default_search_key.ToLower()));
            }
            else
            {
                filteredListItems = categoryList;
            }
            // Sort.
            var sortColumnIndex = Convert.ToInt32(Request["order[0][column]"]);
            var sortDirection = Request["order[0][dir]"];

            switch (sortColumnIndex)
            {
                case 1:
                    filteredListItems = sortDirection == "asc"
                        ? filteredListItems.OrderBy(p => p.Name)
                        : filteredListItems.OrderByDescending(p => p.Name);
                    break;
            }

            var displayedList = filteredListItems.Skip(param.start).Take(param.length);
            var result = displayedList.Select(p => new CategoryViewModel
            {
                ID = p.ID,
                Name = p.Name,
                Description = p.Description,
                Level = p.CategoryLevel.Value,
                Categories = _categoryService.GetSubCategories(p.ID).Select(m => new CategoryViewModel
                {
                    ID = m.ID,
                    Name = m.Name,
                    Description = m.Description,
                    Level = m.CategoryLevel.Value,
                    Categories = _categoryService.GetItems(m.ID).Select(n => new CategoryViewModel
                    {
                        ID = n.ID,
                        Name = n.Name,
                        Description = n.Description,
                        Level = n.CategoryLevel.Value
                    }).ToArray()
                }).ToArray()
            }).ToArray();

            return Json(new
            {
                param.sEcho,
                recordsTotal = result.Count(),
                recordsFiltered = filteredListItems.Count(),
                data = result
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetCategories()
        {
            var categoryList = _categoryService.GetCategories().Select(m => new
            {
                m.ID,
                m.Name
            }).ToArray();
            return Json(new
            {
                data = categoryList
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetSubCategories()
        {
            var categoryList = _categoryService.GetSubCategories().OrderBy(m => m.ParentID).Select(m => new Category
            {
                ID = m.ID,
                Name = _categoryService.GetCategoryById(m.ParentID.Value).Name + " > " + m.Name
            }).ToArray();
            return Json(new
            {
                data = categoryList
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetPriorityMatrixTable()
        {
            ViewBag.priorityList = new SelectList(_priorityService.GetAll(), "ID", "Name"); 
            ViewBag.impactList = _impactService.GetAll();
            ViewBag.urgencyList = _urgencyService.GetAll();
            return PartialView("_PriorityMatrixTable");
        }

        [HttpGet]
        public ActionResult GetPriorityMatrixItems()
        {
            var result = _priorityMatrixService.GetPriorityMatrixItems().Select(m => new PriorityMatrixItem
            {
                ImpactID = m.ImpactID,
                UrgencyID = m.UrgencyID,
                PriorityID = m.PriorityID
            }).ToArray();
            return Json(new
            {
                data = result
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
                Impact impact = _impactService.GetImpactById(id);
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
        public ActionResult EditImpact(int? id, ImpactViewModel model)
        {
            if (id.HasValue)
            {
                bool isDuplicatedName = _impactService.IsDuplicatedName(id, model.Name);
                if (isDuplicatedName)
                {
                    return Json(new
                    {
                        success = false,
                        error = false,
                        message = string.Format("'{0}' has already been used.", model.Name)
                    });
                }
                else
                {
                    Impact impact = _impactService.GetImpactById(id.Value);
                    impact.Name = model.Name;
                    impact.Description = model.Description;

                    _impactService.UpdateImpact(impact);
                    return Json(new
                    {
                        success = true,
                        message = "Update impact successfully!"
                    });
                }
            }
            else
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
        public ActionResult DeleteImpact(int? id)
        {
            if (!id.HasValue)
            {
                return Json(new
                {
                    success = false,
                    error = false,
                    message = "Delete impact unsuccessfully!"
                });
            }

            Impact impact = _impactService.GetImpactById(id.Value);

            if (impact == null)
            {
                return Json(new
                {
                    success = false,
                    error = true,
                    message = "Delete impact unsuccessfully!"
                });
            }

            try
            {
                if (_impactService.IsInUse(impact))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Impact is being used! Can not be deleted!"
                    });
                }
                else
                {
                    _impactService.DeleteImpact(impact);
                    return Json(new
                    {
                        success = true,
                        message = "Delete impact successfully!"
                    });
                }
            }
            catch (Exception e)
            {
                return Json(new
                {
                    success = false,
                    error = true,
                    message = "Some error occured! Please try again later!"
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
                        message = "Create priority sucessfull!"
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
        public ActionResult EditUrgency(int? id, UrgencyViewModel model)
        {
            if (id.HasValue)
            {
                bool isDuplicatedName = _urgencyService.IsDuplicatedName(id, model.Name);
                if (isDuplicatedName)
                {
                    return Json(new
                    {
                        success = false,
                        error = false,
                        message = String.Format("'{0}' has already been used.", model.Name)
                    });
                }
                else
                {
                    Urgency urgency = _urgencyService.GetUrgencyByID(id.Value);
                    urgency.Name = model.Name;
                    urgency.Description = model.Description;

                    _urgencyService.UpdateUrgency(urgency);
                    return Json(new
                    {
                        success = true,
                        message = "Update urgency successfully!"
                    });
                }
            }
            else
            {
                return Json(new
                {
                    success = false,
                    error = true,
                    message = "Cannot update urgency!"
                });
            }
        }

        [HttpPost]
        public ActionResult EditPriority(int? id, PriorityViewModel model)
        {
            if (id.HasValue)
            {
                bool isDuplicatedName = _priorityService.IsDuplicatedName(id, model.Name);
                if (isDuplicatedName)
                {
                    return Json(new
                    {
                        success = false,
                        error = false,
                        message = string.Format("'{0}' has already been used.", model.Name)
                    });
                }
                else
                {
                    Priority priority = _priorityService.GetPriorityByID(id.Value);
                    priority.Name = model.Name;
                    priority.Description = model.Description;

                    _priorityService.UpdatePriority(priority);
                    return Json(new
                    {
                        success = true,
                        message = "Update priority successfully!"
                    });
                }
            }
            else
            {
                return Json(new
                {
                    success = false,
                    error = true,
                    message = "Cannot update priority!"
                });
            }
        }

        [HttpPost]
        public ActionResult DeletePriority(int? id)
        {
            if (!id.HasValue)
            {
                return Json(new
                {
                    success = false,
                    error = false,
                    message = "Delete priority unsuccessfully!"
                });
            }
            Priority priority = _priorityService.GetPriorityByID(id.Value);
            if (priority == null)
            {
                return Json(new
                {
                    success = false,
                    error = true,
                    message = "Delete priority unsuccessfully!"
                });
            }
            else
            {

                if (_priorityService.IsInUse(priority))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Priority is being used! Can not be deleted!"
                    });
                }
                try
                {
                    _priorityService.DeletePriority(priority);
                    return Json(new
                    {
                        success = true,
                        message = "Delete priority successfully!"
                    });
                }
                catch (Exception)
                {
                    return Json(new
                    {
                        success = false,
                        error = true,
                        message = "Some error occured! Please try again later!"
                    });
                }


            }
        }

        [HttpPost]
        public ActionResult DeleteUrgency(int? id)
        {
            if (!id.HasValue)
            {
                return Json(new
                {
                    success = false,
                    error = false,
                    message = "Delete urgency unsuccessfully!"
                });
            }
            Urgency urgency = _urgencyService.GetUrgencyByID(id.Value);
            if (urgency == null)
            {
                return Json(new
                {
                    success = false,
                    error = true,
                    message = "Delete urgency unsuccessfully!"
                });
            }
            else
            {

                if (_urgencyService.IsInUse(urgency))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Urgency is being used! Can not be deleted!"
                    });
                }
                try
                {
                    _urgencyService.DeleteUrgency(urgency);
                    return Json(new
                    {
                        success = true,
                        message = "Delete urgency successfully!"
                    });
                }
                catch (Exception)
                {
                    return Json(new
                    {
                        success = false,
                        error = true,
                        message = "Some error occured! Please try again later!"
                    });
                }


            }
        }

        [HttpPost]
        public ActionResult CreateCategory(CategoryViewModel model)
        {
            bool isDuplicatedName = _categoryService.IsDuplicatedName(null, model.Name, null);
            if (isDuplicatedName)
            {
                return Json(new
                {
                    success = false,
                    error = false,
                    message = string.Format("'{0}' has already been used.", model.Name)
                });
            }
            else
            {
                Category category = new Category();
                category.Name = model.Name;
                category.Description = model.Description;
                category.CategoryLevel = ConstantUtil.CategoryLevel.Category;
                try
                {
                    _categoryService.AddCategory(category);
                    return Json(new
                    {
                        success = true,
                        message = "Create new category successfully!"
                    });
                }
                catch (Exception e)
                {
                    log.Error("Create category error.", e);
                    return Json(new
                    {
                        success = false,
                        error = true,
                        message = "Some errors occured. Please try again later!"
                    });
                }
            }
        }

        [HttpPost]
        public ActionResult CreateSubCategory(CategoryViewModel model)
        {
            bool isDuplicatedName = _categoryService.IsDuplicatedName(null, model.Name, model.ParentId);
            if (isDuplicatedName)
            {
                return Json(new
                {
                    success = false,
                    error = false,
                    message = string.Format("'{0}' has already been used.", model.Name)
                });
            }
            else
            {
                Category category = new Category();
                category.Name = model.Name;
                category.Description = model.Description;
                category.CategoryLevel = ConstantUtil.CategoryLevel.SubCategory;
                category.ParentID = model.ParentId;
                try
                {
                    _categoryService.AddCategory(category);
                    return Json(new
                    {
                        success = true,
                        message = "Create new sub category successfully!"
                    });
                }
                catch (Exception e)
                {
                    log.Error("Create sub category error.", e);
                    return Json(new
                    {
                        success = false,
                        error = true,
                        message = "Some errors occured. Please try again later!"
                    });
                }
            }
        }

        [HttpPost]
        public ActionResult CreateItem(CategoryViewModel model)
        {
            bool isDuplicatedName = _categoryService.IsDuplicatedName(null, model.Name, model.ParentId);
            if (isDuplicatedName)
            {
                return Json(new
                {
                    success = false,
                    error = false,
                    message = string.Format("'{0}' has already been used.", model.Name)
                });
            }
            else
            {
                Category category = new Category();
                category.Name = model.Name;
                category.Description = model.Description;
                category.CategoryLevel = ConstantUtil.CategoryLevel.Item;
                category.ParentID = model.ParentId;
                try
                {
                    _categoryService.AddCategory(category);
                    return Json(new
                    {
                        success = true,
                        message = "Create new item successfully!"
                    });
                }
                catch (Exception e)
                {
                    log.Error("Create item error.", e);
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
        public ActionResult GetCategoryDetail(int? id)
        {
            if (id.HasValue)
            {
                Category category = _categoryService.GetCategoryById(id.Value);
                if (category != null)
                {
                    return Json(new
                    {
                        success = true,
                        name = category.Name,
                        description = category.Description
                    }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new
            {
                success = false,
                message = "Cannot get category detail!"
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetSubCategoryDetail(int? id)
        {
            if (id.HasValue)
            {
                Category category = _categoryService.GetCategoryById(id.Value);
                if (category != null)
                {
                    return Json(new
                    {
                        success = true,
                        name = category.Name,
                        description = category.Description,
                        parentId = category.ParentID,
                        categories = _categoryService.GetCategories().Select(m => new
                        {
                            m.ID,
                            m.Name
                        }).ToArray()
                    }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new
            {
                success = false,
                message = "Cannot get sub category detail!"
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetItemDetail(int? id)
        {
            if (id.HasValue)
            {
                Category category = _categoryService.GetCategoryById(id.Value);
                if (category != null)
                {
                    return Json(new
                    {
                        success = true,
                        name = category.Name,
                        description = category.Description,
                        parentId = category.ParentID,
                        categories = _categoryService.GetSubCategories().OrderBy(m => m.ParentID).Select(m => new CategoryViewModel
                        {
                            ID = m.ID,
                            Name = _categoryService.GetCategoryById(m.ParentID.Value).Name + " > " + m.Name
                        }).ToArray()
                    }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new
            {
                success = false,
                message = "Cannot get item detail!"
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EditCategory(CategoryViewModel model)
        {
            if (model.ID.HasValue)
            {
                bool isDuplicatedName = _categoryService.IsDuplicatedName(model.ID, model.Name, null);
                if (isDuplicatedName)
                {
                    return Json(new
                    {
                        success = false,
                        error = false,
                        message = string.Format("'{0}' has already been used.", model.Name)
                    });
                }
                else
                {
                    Category category = _categoryService.GetCategoryById(model.ID.Value);
                    category.Name = model.Name;
                    category.Description = model.Description;
                    try
                    {
                        _categoryService.UpdateCategory(category);
                        return Json(new
                        {
                            success = true,
                            message = "Edit category successfully!"
                        });
                    }
                    catch (Exception e)
                    {
                        log.Error("Edit category error.", e);
                        return Json(new
                        {
                            success = false,
                            error = true,
                            message = "Some errors occured. Please try again later!"
                        });
                    }
                }
            }
            else
            {
                return Json(new
                {
                    success = false,
                    error = false,
                    message = "Cannot get category detail!"
                });
            }
        }

        [HttpPost]
        public ActionResult EditSubCategory(CategoryViewModel model)
        {
            if (model.ID.HasValue)
            {
                bool isDuplicatedName = _categoryService.IsDuplicatedName(model.ID, model.Name, model.ParentId);
                if (isDuplicatedName)
                {
                    return Json(new
                    {
                        success = false,
                        error = false,
                        message = string.Format("'{0}' has already been used.", model.Name)
                    });
                }
                else
                {
                    Category category = _categoryService.GetCategoryById(model.ID.Value);
                    category.Name = model.Name;
                    category.Description = model.Description;
                    category.ParentID = model.ParentId;
                    try
                    {
                        _categoryService.UpdateCategory(category);
                        return Json(new
                        {
                            success = true,
                            message = "Edit sub category successfully!"
                        });
                    }
                    catch (Exception e)
                    {
                        log.Error("Edit sub category error.", e);
                        return Json(new
                        {
                            success = false,
                            error = true,
                            message = "Some errors occured. Please try again later!"
                        });
                    }
                }
            }
            else
            {
                return Json(new
                {
                    success = false,
                    error = false,
                    message = "Cannot get sub category detail!"
                });
            }
        }

        [HttpPost]
        public ActionResult EditItem(CategoryViewModel model)
        {
            if (model.ID.HasValue)
            {
                bool isDuplicatedName = _categoryService.IsDuplicatedName(model.ID, model.Name, model.ParentId);
                if (isDuplicatedName)
                {
                    return Json(new
                    {
                        success = false,
                        error = false,
                        message = string.Format("'{0}' has already been used.", model.Name)
                    });
                }
                else
                {
                    Category category = _categoryService.GetCategoryById(model.ID.Value);
                    category.Name = model.Name;
                    category.Description = model.Description;
                    category.ParentID = model.ParentId;
                    try
                    {
                        _categoryService.UpdateCategory(category);
                        return Json(new
                        {
                            success = true,
                            message = "Edit item successfully!"
                        });
                    }
                    catch (Exception e)
                    {
                        log.Error("Edit item error.", e);
                        return Json(new
                        {
                            success = false,
                            error = true,
                            message = "Some errors occured. Please try again later!"
                        });
                    }
                }
            }
            else
            {
                return Json(new
                {
                    success = false,
                    error = false,
                    message = "Cannot get sub category detail!"
                });
            }
        }

        [HttpPost]
        public ActionResult DeleteCategory(int? id)
        {
            if (!id.HasValue)
            {
                return Json(new
                {
                    success = false,
                    error = false,
                    message = "This category has been removed or not existed!"
                });
            }
            Category category = _categoryService.GetCategoryById(id.Value);
            if (category == null)
            {
                return Json(new
                {
                    success = false,
                    error = true,
                    message = "This category has been removed or not existed!"
                });
            }
            else
            {
                if (_categoryService.IsInUse(category))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Category is being used! Can not be removed!"
                    });
                }
                try
                {
                    _categoryService.DeleteCategory(category);
                    return Json(new
                    {
                        success = true,
                        message = "Delete category successfully!"
                    });
                }
                catch (Exception e)
                {
                    log.Error("Delete category error.", e);
                    return Json(new
                    {
                        success = false,
                        error = true,
                        message = "Some error occured! Please try again later!"
                    });
                }
            }
        }

        [HttpPost]
        public ActionResult ChangePriorityMatrixItem(PriorityMatrixItemViewModel model)
        {
            if (model.ImpactID.HasValue && model.UrgencyID.HasValue)
            {
                try
                {
                    _priorityMatrixService.ChangePriorityMatrixItem(model.ImpactID.Value, model.UrgencyID.Value, model.PriorityID);
                    return Json(new
                    {
                        success = true,
                        message = "Change priority matrix item successfully!"
                    });
                }
                catch (Exception e)
                {
                    log.Error("Change priority matrix item error", e);
                    return Json(new
                    {
                        success = false,
                        message = "Some error occured! Please try again later!"
                    });
                }
            }
            else
            {
                return Json(new
                {
                    success = false,
                    message = "Change priority matrix item unsuccessfully!"
                });
            }
        }

        [HttpGet]
        public ActionResult GetCategoryTreeViewData()
        {
            IEnumerable<CategoryViewModel> list = _categoryService.GetAll().Select(m => new CategoryViewModel
            {
                ID = m.ID,
                Name = m.Name,
                ParentId = m.ParentID,
                Level = m.CategoryLevel
            }).ToArray();
            return Json(new
            {
                data = list
            }, JsonRequestBehavior.AllowGet);
        }
    }
}