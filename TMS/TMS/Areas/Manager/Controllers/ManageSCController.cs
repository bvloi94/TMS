using log4net;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TMS.DAL;
using TMS.Models;
using TMS.Services;
using TMS.Utils;
using TMS.ViewModels;

namespace TMS.Areas.Manager.Controllers
{
    [CustomAuthorize(Roles = "Manager")]
    public class ManageSCController : Controller
    {
        private UnitOfWork _unitOfWork;
        private ImpactService _impactService;
        private UrgencyService _urgencyService;
        private PriorityService _priorityService;
        private CategoryService _categoryService;
        private UserService _userService;
        private PriorityMatrixService _priorityMatrixService;
        private ILog log = LogManager.GetLogger(typeof(ManageSCController));

        public ManageSCController()
        {
            _unitOfWork = new UnitOfWork();
            _impactService = new ImpactService(_unitOfWork);
            _urgencyService = new UrgencyService(_unitOfWork);
            _priorityService = new PriorityService(_unitOfWork);
            _categoryService = new CategoryService(_unitOfWork);
            _userService = new UserService(_unitOfWork);
            _priorityMatrixService = new PriorityMatrixService(_unitOfWork);
        }

        // GET: Manager/ManageSC/Priority
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

        // GET: Manager/ManageSC/Category
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
                p.Description,
                p.Color
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
        public ActionResult CreateImpact(ImpactViewModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.Name))
            {
                model.Name = model.Name.Trim();
                bool isDuplicateName = _impactService.IsDuplicatedName(null, model.Name);
                if (isDuplicateName)
                {
                    return Json(new
                    {
                        success = false,
                        message = string.Format("'{0}' has already been used.", model.Name)
                    });
                }
            }
            if (!ModelState.IsValid)
            {
                foreach (ModelState modelState in ViewData.ModelState.Values)
                {
                    foreach (System.Web.Mvc.ModelError error in modelState.Errors)
                    {
                        return Json(new
                        {
                            success = false,
                            message = error.ErrorMessage
                        });
                    }
                }
            }
            else
            {
                Impact impact = new Impact();
                impact.Name = model.Name.Trim();
                impact.Description = model.Description;
                bool resultInsert = _impactService.AddImpact(impact);
                if (resultInsert)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Create impact successfully!"
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = ConstantUtil.CommonError.DBExceptionError
                    });
                }
            }
            return Json(new
            {
                success = false,
                message = ConstantUtil.CommonError.DBExceptionError
            });
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

        // POST: Manager/ManageSC/EditImpact
        [HttpPost]
        public ActionResult EditImpact(int? id, ImpactViewModel model)
        {
            if (id.HasValue)
            {
                if (!string.IsNullOrWhiteSpace(model.Name))
                {
                    model.Name = model.Name.Trim();
                    bool isDuplicateName = _impactService.IsDuplicatedName(id, model.Name);
                    if (isDuplicateName)
                    {
                        return Json(new
                        {
                            success = false,
                            message = string.Format("'{0}' has already been used.", model.Name)
                        });
                    }
                }
                if (!ModelState.IsValid)
                {
                    foreach (ModelState modelState in ViewData.ModelState.Values)
                    {
                        foreach (System.Web.Mvc.ModelError error in modelState.Errors)
                        {
                            return Json(new
                            {
                                success = false,
                                message = error.ErrorMessage
                            });
                        }
                    }
                }
                else
                {
                    var name = model.Name.Trim();
                    Impact impact = _impactService.GetImpactById(id.Value);
                    impact.Name = name;
                    impact.Description = model.Description;
                    bool resultUpdate = _impactService.UpdateImpact(impact);
                    if (resultUpdate)
                    {
                        return Json(new
                        {
                            success = true,
                            message = "Update impact successfully!"
                        });
                    }
                    else
                    {
                        return Json(new
                        {
                            success = false,
                            message = ConstantUtil.CommonError.DBExceptionError
                        });
                    }
                }
                return Json(new
                {
                    success = false,
                    message = ConstantUtil.CommonError.DBExceptionError
                });
            }
            else
            {
                return Json(new
                {
                    success = false,
                    message = "Unavailable impact!"
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
                    message = "Unavailable impact."
                });
            }

            Impact impact = _impactService.GetImpactById(id.Value);
            if (impact == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Unavailable impact!"
                });
            }
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
                bool resultDelete = _impactService.DeleteImpact(impact);
                if (resultDelete)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Delete impact successfully!"
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = ConstantUtil.CommonError.DBExceptionError
                    });
                }
            }
        }

        [HttpPost]
        public ActionResult CreatePriority(PriorityViewModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.Name))
            {
                model.Name = model.Name.Trim();
                bool isDuplicateName = _priorityService.IsDuplicateName(null, model.Name);
                if (isDuplicateName)
                {
                    return Json(new
                    {
                        success = false,
                        message = string.Format("'{0}' has already been used.", model.Name)
                    });
                }
            }
            if (!ModelState.IsValid)
            {
                foreach (ModelState modelState in ViewData.ModelState.Values)
                {
                    foreach (System.Web.Mvc.ModelError error in modelState.Errors)
                    {
                        return Json(new
                        {
                            success = false,
                            message = error.ErrorMessage
                        });
                    }
                }
            }
            else
            {
                Priority priority = new Priority();
                priority.Name = model.Name.Trim();
                priority.Description = model.Description;
                priority.Color = model.Color;
                bool resultInsert = _priorityService.AddPriority(priority);
                if (resultInsert)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Create priority sucessfull!"
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = ConstantUtil.CommonError.DBExceptionError
                    });
                }
            }
            return Json(new
            {
                success = false,
                message = ConstantUtil.CommonError.DBExceptionError
            });
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
        public ActionResult CreateUrgency(UrgencyViewModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.Name))
            {
                model.Name = model.Name.Trim();
                bool isDuplicateName = _urgencyService.IsDuplicatedName(null, model.Name);
                if (isDuplicateName)
                {
                    return Json(new
                    {
                        success = false,
                        message = string.Format("'{0}' has already been used.", model.Name)
                    });
                }
            }
            if (!ModelState.IsValid)
            {
                foreach (ModelState modelState in ViewData.ModelState.Values)
                {
                    foreach (System.Web.Mvc.ModelError error in modelState.Errors)
                    {
                        return Json(new
                        {
                            success = false,
                            message = error.ErrorMessage
                        });
                    }
                }
            }
            else
            {
                Urgency urgency = new Urgency();
                urgency.Name = model.Name.Trim();
                urgency.Description = model.Description;

                bool resultInsert = _urgencyService.AddUrgency(urgency);
                if (resultInsert)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Create urgency successfully!"
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = ConstantUtil.CommonError.DBExceptionError
                    });
                }
            }
            return Json(new
            {
                success = false,
                message = ConstantUtil.CommonError.DBExceptionError
            });
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
                if (!string.IsNullOrWhiteSpace(model.Name))
                {
                    model.Name = model.Name.Trim();
                    bool isDuplicateName = _urgencyService.IsDuplicatedName(id, model.Name);
                    if (isDuplicateName)
                    {
                        return Json(new
                        {
                            success = false,
                            message = string.Format("'{0}' has already been used.", model.Name)
                        });
                    }
                }
                if (!ModelState.IsValid)
                {
                    foreach (ModelState modelState in ViewData.ModelState.Values)
                    {
                        foreach (System.Web.Mvc.ModelError error in modelState.Errors)
                        {
                            return Json(new
                            {
                                success = false,
                                message = error.ErrorMessage
                            });
                        }
                    }
                }
                else
                {
                    Urgency urgency = _urgencyService.GetUrgencyByID(id.Value);
                    urgency.Name = model.Name.Trim();
                    urgency.Description = model.Description;

                    bool resultUpdate = _urgencyService.UpdateUrgency(urgency);
                    if (resultUpdate)
                    {
                        return Json(new
                        {
                            success = true,
                            message = "Update urgency successfully!"
                        });
                    }
                    else
                    {
                        return Json(new
                        {
                            success = false,
                            message = ConstantUtil.CommonError.DBExceptionError
                        });
                    }
                }
                return Json(new
                {
                    success = false,
                    message = ConstantUtil.CommonError.DBExceptionError
                });
            }
            else
            {
                return Json(new
                {
                    success = false,
                    message = "Unavailable urgency!"
                });
            }
        }

        [HttpPost]
        public ActionResult EditPriority(int? id, PriorityViewModel model)
        {
            if (id.HasValue)
            {
                if (!string.IsNullOrWhiteSpace(model.Name))
                {
                    model.Name = model.Name.Trim();
                    bool isDuplicateName = _priorityService.IsDuplicateName(id, model.Name);
                    if (isDuplicateName)
                    {
                        return Json(new
                        {
                            success = false,
                            message = string.Format("'{0}' has already been used.", model.Name)
                        });
                    }
                }
                if (!ModelState.IsValid)
                {
                    foreach (ModelState modelState in ViewData.ModelState.Values)
                    {
                        foreach (System.Web.Mvc.ModelError error in modelState.Errors)
                        {
                            return Json(new
                            {
                                success = false,
                                message = error.ErrorMessage
                            });
                        }
                    }
                }
                else
                {
                    Priority priority = _priorityService.GetPriorityByID(id.Value);
                    priority.Name = model.Name.Trim();
                    priority.Description = model.Description;
                    priority.Color = model.Color;

                    bool resultUpdate = _priorityService.UpdatePriority(priority);
                    if (resultUpdate)
                    {
                        return Json(new
                        {
                            success = true,
                            message = "Update priority successfully!"
                        });
                    }
                    else
                    {
                        return Json(new
                        {
                            success = false,
                            message = ConstantUtil.CommonError.DBExceptionError
                        });
                    }
                }
                return Json(new
                {
                    success = false,
                    message = ConstantUtil.CommonError.DBExceptionError
                });
            }
            else
            {
                return Json(new
                {
                    success = false,
                    error = true,
                    message = "Unavailable priority!"
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
                    message = "Unavailable priority!"
                });
            }
            Priority priority = _priorityService.GetPriorityByID(id.Value);
            if (priority == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Unavailable priority!"
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

                bool resultDelete = _priorityService.DeletePriority(priority);
                if (resultDelete)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Delete priority successfully!"
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = ConstantUtil.CommonError.DBExceptionError
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
                    message = "Unavailable urgency!"
                });
            }
            Urgency urgency = _urgencyService.GetUrgencyByID(id.Value);
            if (urgency == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Unavailable urgency!"
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
                bool resultDelete = _urgencyService.DeleteUrgency(urgency);
                if (resultDelete)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Delete urgency successfully!"
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = ConstantUtil.CommonError.DBExceptionError
                    });
                }

            }
        }

        [HttpPost]
        public ActionResult CreateCategory(CategoryViewModel model)
        {
            bool isDuplicatedName = _categoryService.IsDuplicatedName(null, model.Name);
            if (isDuplicatedName)
            {
                return Json(new
                {
                    success = false,
                    message = string.Format("'{0}' has already been used.", model.Name)
                });
            }
            if (!ModelState.IsValid)
            {
                foreach (ModelState modelState in ViewData.ModelState.Values)
                {
                    foreach (System.Web.Mvc.ModelError error in modelState.Errors)
                    {
                        return Json(new
                        {
                            success = false,
                            message = error.ErrorMessage
                        });
                    }
                }
            }
            else
            {
                Category category = new Category();
                category.Name = model.Name;
                category.Description = model.Description;
                category.CategoryLevel = ConstantUtil.CategoryLevel.Category;

                bool addResult = _categoryService.AddCategory(category);
                if (addResult)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Create new category successfully!"
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = ConstantUtil.CommonError.DBExceptionError
                    });
                }
            }
            return Json(new
            {
                success = false,
                message = ConstantUtil.CommonError.DBExceptionError
            });
        }

        [HttpPost]
        public ActionResult CreateSubCategory(CategoryViewModel model)
        {
            bool isDuplicatedName = _categoryService.IsDuplicatedName(null, model.Name);
            if (isDuplicatedName)
            {
                return Json(new
                {
                    success = false,
                    message = string.Format("'{0}' has already been used.", model.Name)
                });
            }
            if (!ModelState.IsValid)
            {
                foreach (ModelState modelState in ViewData.ModelState.Values)
                {
                    foreach (System.Web.Mvc.ModelError error in modelState.Errors)
                    {
                        return Json(new
                        {
                            success = false,
                            message = error.ErrorMessage
                        });
                    }
                }
            }
            else
            {
                if (!model.ParentId.HasValue)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Category is required!"
                    });
                }
                else
                {
                    Category category = new Category();
                    category.Name = model.Name;
                    category.Description = model.Description;
                    category.CategoryLevel = ConstantUtil.CategoryLevel.SubCategory;
                    category.ParentID = model.ParentId;
                    bool addResult = _categoryService.AddCategory(category);
                    if (addResult)
                    {
                        return Json(new
                        {
                            success = true,
                            message = "Create new sub category successfully!"
                        });
                    }
                    else
                    {
                        return Json(new
                        {
                            success = false,
                            message = ConstantUtil.CommonError.DBExceptionError
                        });
                    }
                }
            }
            return Json(new
            {
                success = false,
                message = ConstantUtil.CommonError.DBExceptionError
            });
        }

        [HttpPost]
        public ActionResult CreateItem(CategoryViewModel model)
        {
            bool isDuplicatedName = _categoryService.IsDuplicatedName(null, model.Name);
            if (isDuplicatedName)
            {
                return Json(new
                {
                    success = false,
                    message = string.Format("'{0}' has already been used.", model.Name)
                });
            }
            if (!ModelState.IsValid)
            {
                foreach (ModelState modelState in ViewData.ModelState.Values)
                {
                    foreach (System.Web.Mvc.ModelError error in modelState.Errors)
                    {
                        return Json(new
                        {
                            success = false,
                            message = error.ErrorMessage
                        });
                    }
                }
            }
            else
            {
                if (!model.ParentId.HasValue)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Sub Category is required!"
                    });
                }
                else
                {
                    Category category = new Category();
                    category.Name = model.Name;
                    category.Description = model.Description;
                    category.CategoryLevel = ConstantUtil.CategoryLevel.Item;
                    category.ParentID = model.ParentId;

                    bool addResult = _categoryService.AddCategory(category);
                    if (addResult)
                    {
                        return Json(new
                        {
                            success = true,
                            message = "Create new item successfully!"
                        });
                    }
                    else
                    {
                        return Json(new
                        {
                            success = false,
                            message = ConstantUtil.CommonError.DBExceptionError
                        });
                    }
                }
            }
            return Json(new
            {
                success = false,
                message = ConstantUtil.CommonError.DBExceptionError
            });
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
                bool isDuplicatedName = _categoryService.IsDuplicatedName(model.ID, model.Name);
                if (isDuplicatedName)
                {
                    return Json(new
                    {
                        success = false,
                        message = string.Format("'{0}' has already been used.", model.Name)
                    });
                }
                if (!ModelState.IsValid)
                {
                    foreach (ModelState modelState in ViewData.ModelState.Values)
                    {
                        foreach (System.Web.Mvc.ModelError error in modelState.Errors)
                        {
                            return Json(new
                            {
                                success = false,
                                message = error.ErrorMessage
                            });
                        }
                    }
                }
                else
                {
                    Category category = _categoryService.GetCategoryById(model.ID.Value);
                    if (category == null)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Unavailable Category!"
                        });
                    }
                    category.Name = model.Name;
                    category.Description = model.Description;

                    bool updateResult = _categoryService.UpdateCategory(category);
                    if (updateResult)
                    {
                        return Json(new
                        {
                            success = true,
                            message = "Edit category successfully!"
                        });
                    }
                    else
                    {
                        return Json(new
                        {
                            success = false,
                            message = ConstantUtil.CommonError.DBExceptionError
                        });
                    }
                }
                return Json(new
                {
                    success = false,
                    message = ConstantUtil.CommonError.DBExceptionError
                });
            }
            else
            {
                return Json(new
                {
                    success = false,
                    message = "Unavailable category!"
                });
            }
        }

        [HttpPost]
        public ActionResult EditSubCategory(CategoryViewModel model)
        {
            if (model.ID.HasValue)
            {
                bool isDuplicatedName = _categoryService.IsDuplicatedName(model.ID, model.Name);
                if (isDuplicatedName)
                {
                    return Json(new
                    {
                        success = false,
                        error = false,
                        message = string.Format("'{0}' has already been used.", model.Name)
                    });
                }
                if (!ModelState.IsValid)
                {
                    foreach (ModelState modelState in ViewData.ModelState.Values)
                    {
                        foreach (System.Web.Mvc.ModelError error in modelState.Errors)
                        {
                            return Json(new
                            {
                                success = false,
                                message = error.ErrorMessage
                            });
                        }
                    }
                }
                else
                {
                    if (!model.ParentId.HasValue)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Category is required!"
                        });
                    }
                    else
                    {
                        Category category = _categoryService.GetCategoryById(model.ID.Value);
                        if (category == null)
                        {
                            return Json(new
                            {
                                success = false,
                                message = "Unavailable Sub Category!"
                            });
                        }
                        category.Name = model.Name;
                        category.Description = model.Description;
                        category.ParentID = model.ParentId;

                        bool updateResult = _categoryService.UpdateCategory(category);
                        if (updateResult)
                        {
                            return Json(new
                            {
                                success = true,
                                message = "Edit sub category successfully!"
                            });
                        }
                        else
                        {
                            return Json(new
                            {
                                success = false,
                                message = ConstantUtil.CommonError.DBExceptionError
                            });
                        }
                    }
                }
                return Json(new
                {
                    success = false,
                    message = ConstantUtil.CommonError.DBExceptionError
                });
            }
            else
            {
                return Json(new
                {
                    success = false,
                    message = "Unavailable Sub Category!"
                });
            }
        }

        [HttpPost]
        public ActionResult EditItem(CategoryViewModel model)
        {
            if (model.ID.HasValue)
            {
                bool isDuplicatedName = _categoryService.IsDuplicatedName(model.ID, model.Name);
                if (isDuplicatedName)
                {
                    return Json(new
                    {
                        success = false,
                        message = string.Format("'{0}' has already been used.", model.Name)
                    });
                }
                if (!ModelState.IsValid)
                {
                    foreach (ModelState modelState in ViewData.ModelState.Values)
                    {
                        foreach (System.Web.Mvc.ModelError error in modelState.Errors)
                        {
                            return Json(new
                            {
                                success = false,
                                message = error.ErrorMessage
                            });
                        }
                    }
                }
                else
                {
                    if (!model.ParentId.HasValue)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Sub Category is required!"
                        });
                    }
                    else
                    {
                        Category category = _categoryService.GetCategoryById(model.ID.Value);
                        if (category == null)
                        {
                            return Json(new
                            {
                                success = false,
                                message = "Unavailable Item!"
                            });
                        }
                        category.Name = model.Name;
                        category.Description = model.Description;
                        category.ParentID = model.ParentId;

                        bool updateResult = _categoryService.UpdateCategory(category);
                        if (updateResult)
                        {
                            return Json(new
                            {
                                success = true,
                                message = "Edit item successfully!"
                            });
                        }
                        else
                        {
                            return Json(new
                            {
                                success = false,
                                message = ConstantUtil.CommonError.DBExceptionError
                            });
                        }
                    }
                }
                return Json(new
                {
                    success = false,
                    message = ConstantUtil.CommonError.DBExceptionError
                });
            }
            else
            {
                return Json(new
                {
                    success = false,
                    message = "Unavailable Item!"
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
                    message = "This category has been removed or not existed!"
                });
            }
            Category category = _categoryService.GetCategoryById(id.Value);
            if (category == null)
            {
                return Json(new
                {
                    success = false,
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

                bool deleteResult = _categoryService.DeleteCategory(category);
                if (deleteResult)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Delete category successfully!"
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = ConstantUtil.CommonError.DBExceptionError
                    });
                }
            }
        }

        [HttpPost]
        public ActionResult ChangePriorityMatrixItem(PriorityMatrixItemViewModel model)
        {
            if (model.ImpactID.HasValue && model.UrgencyID.HasValue)
            {

                bool changeResult = _priorityMatrixService.ChangePriorityMatrixItem(model.ImpactID.Value, model.UrgencyID.Value, model.PriorityID);
                if (changeResult)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Change priority matrix item successfully!"
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = ConstantUtil.CommonError.DBExceptionError
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

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string id = User.Identity.GetUserId();
            AspNetUser manager = _userService.GetUserById(id);
            if (manager != null)
            {
                ViewBag.LayoutName = manager.Fullname;
                ViewBag.LayoutAvatarURL = manager.AvatarURL;
            }
            base.OnActionExecuting(filterContext);
        }
    }
}