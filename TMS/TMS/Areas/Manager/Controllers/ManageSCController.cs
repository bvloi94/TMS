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
        private KeywordService _keywordService;
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
            _keywordService = new KeywordService(_unitOfWork);
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
        public ActionResult Keywords()
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
            var sortColumnIndex = TMSUtils.StrToIntDef(Request["iSortCol_0"], 0);
            var sortDirection = Request["sSortDir_0"]; // asc or desc

            switch (sortColumnIndex)
            {
                case 0:
                    filteredListItems = sortDirection == "asc"
                        ? filteredListItems.OrderBy(p => p.Name)
                        : filteredListItems.OrderByDescending(p => p.Name);
                    break;
            }

            var displayedList = filteredListItems;
            if (param.length != -1)
            {
                displayedList = filteredListItems.Skip(param.start).Take(param.length);
            }

            IEnumerable<ImpactViewModel> result = displayedList.Select(p => new ImpactViewModel
            {
                Id = p.ID,
                Name = p.Name,
                Description = p.Description,
                IsSystem = p.IsSystem
            }).ToArray();

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
            var sortColumnIndex = TMSUtils.StrToIntDef(Request["iSortCol_0"], 0);
            var sortDirection = Request["sSortDir_0"]; // asc or desc

            switch (sortColumnIndex)
            {
                case 0:
                    filteredListItems = sortDirection == "asc"
                        ? filteredListItems.OrderBy(p => p.Name)
                        : filteredListItems.OrderByDescending(p => p.Name);
                    break;
            }

            var displayedList = filteredListItems;
            if (param.length != -1)
            {
                displayedList = filteredListItems.Skip(param.start).Take(param.length);
            }

            IEnumerable<UrgencyViewModel> result = displayedList.Select(p => new UrgencyViewModel
            {
                Id = p.ID,
                Name = p.Name,
                Description = p.Description,
                Duration = GeneralUtil.GetDuration(p.Duration),
                DurationOption = GeneralUtil.GetDurationOption(p.Duration),
                IsSystem = p.IsSystem
            }).ToArray();

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
            var sortColumnIndex = TMSUtils.StrToIntDef(Request["iSortCol_0"], 0);
            var sortDirection = Request["sSortDir_0"]; // asc or desc

            switch (sortColumnIndex)
            {
                case 0:
                    filteredListItems = sortDirection == "asc"
                        ? filteredListItems.OrderBy(p => p.Name)
                        : filteredListItems.OrderByDescending(p => p.Name);
                    break;
            }


            var displayedList = filteredListItems;
            if (param.length != -1)
            {
                displayedList = filteredListItems.Skip(param.start).Take(param.length);
            }

            IEnumerable<PriorityViewModel> result = displayedList.Select(p => new PriorityViewModel
            {
                Id = p.ID,
                Name = p.Name,
                Description = p.Description,
                Color = p.Color,
                Level = p.PriorityLevel,
                IsSystem = p.IsSystem
            }).OrderByDescending(m => m.Level).ToArray();

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
                Level = p.CategoryLevel,
                Impact = (p.Impact == null) ? "-" : p.Impact.Name,
                Urgency = (p.Urgency == null) ? "-" : p.Urgency.Name,
                Categories = _categoryService.GetSubCategories(p.ID).Select(m => new CategoryViewModel
                {
                    ID = m.ID,
                    Name = m.Name,
                    Description = m.Description,
                    Level = m.CategoryLevel,
                    Impact = (m.Impact == null) ? "-" : m.Impact.Name,
                    Urgency = (m.Urgency == null) ? "-" : m.Urgency.Name,
                    Categories = _categoryService.GetItems(m.ID).Select(n => new CategoryViewModel
                    {
                        ID = n.ID,
                        Name = n.Name,
                        Description = n.Description,
                        Level = n.CategoryLevel,
                        Impact = (n.Impact == null) ? "-" : n.Impact.Name,
                        Urgency = (n.Urgency == null) ? "-" : n.Urgency.Name,
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
        public ActionResult GetKeywords(jQueryDataTableParamModel param)
        {
            var keywordList = _keywordService.GetAll();
            var default_search_key = Request["search[value]"];

            IEnumerable<Keyword> filteredListItems;
            if (!string.IsNullOrEmpty(default_search_key))
            {
                filteredListItems = keywordList.Where(p => p.Name.ToLower().Contains(default_search_key.ToLower()));
            }
            else
            {
                filteredListItems = keywordList;
            }
            // Sort.
            var sortColumnIndex = Convert.ToInt32(Request["order[0][column]"]);
            var sortDirection = Request["order[0][dir]"];

            switch (sortColumnIndex)
            {
                case 0:
                    filteredListItems = sortDirection == "asc"
                        ? filteredListItems.OrderBy(p => p.Name)
                        : filteredListItems.OrderByDescending(p => p.Name);
                    break;
            }

            var displayedList = filteredListItems;
            if (param.length != -1)
            {
                displayedList = filteredListItems.Skip(param.start).Take(param.length);
            }

            IEnumerable<KeywordViewModel> result = displayedList.Select(p => new KeywordViewModel
            {
                ID = p.ID,
                Name = p.Name
            }).ToArray();

            return Json(new
            {
                param.sEcho,
                iTotalRecords = result.Count(),
                iTotalDisplayRecords = filteredListItems.Count(),
                aaData = result
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetCategoryModal()
        {
            var impactList = _impactService.GetAll().Select(m => new
            {
                m.ID,
                m.Name
            }).ToArray();

            var urgencyList = _urgencyService.GetAll().Select(m => new
            {
                m.ID,
                m.Name
            }).ToArray();

            return Json(new
            {
                impacts = impactList,
                urgencies = urgencyList
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetSubCategoryModal()
        {
            var categoryList = _categoryService.GetCategories().Select(m => new
            {
                m.ID,
                m.Name
            }).ToArray();

            var impactList = _impactService.GetAll().Select(m => new
            {
                m.ID,
                m.Name
            }).ToArray();

            var urgencyList = _urgencyService.GetAll().Select(m => new
            {
                m.ID,
                m.Name
            }).ToArray();

            return Json(new
            {
                categories = categoryList,
                impacts = impactList,
                urgencies = urgencyList
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetItemModal()
        {
            var categoryList = _categoryService.GetSubCategories().OrderBy(m => m.ParentID).Select(m => new Category
            {
                ID = m.ID,
                Name = _categoryService.GetCategoryById(m.ParentID.Value).Name + " > " + m.Name
            }).ToArray();

            var impactList = _impactService.GetAll().Select(m => new
            {
                m.ID,
                m.Name
            }).ToArray();

            var urgencyList = _urgencyService.GetAll().Select(m => new
            {
                m.ID,
                m.Name
            }).ToArray();

            return Json(new
            {
                categories = categoryList,
                impacts = impactList,
                urgencies = urgencyList
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetPriorityMatrixTable()
        {
            ViewBag.priorityList = new SelectList(_priorityService.GetAll(), "ID", "Name");
            ViewBag.impactList = _impactService.GetAll();
            ViewBag.urgencyList = _urgencyService.GetAll().OrderByDescending(m => m.Duration);
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
        public ActionResult CreateKeyword(KeywordViewModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.Name))
            {
                model.Name = model.Name.Trim();
                bool isDuplicatedName = _keywordService.IsDuplicatedName(null, model.Name);
                if (isDuplicatedName)
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
                Keyword keyword = new Keyword();
                keyword.Name = model.Name.Trim();
                bool resultInsert = _keywordService.AddKeyword(keyword);
                if (resultInsert)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Create keyword successfully!"
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
        public ActionResult GetKeywordDetail(int? id)
        {
            if (id.HasValue)
            {
                Keyword keyword = _keywordService.GetKeywordById(id.Value);
                return Json(new
                {
                    success = true,
                    name = keyword.Name
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new
                {
                    success = false,
                    message = "Unavailable keyword!"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditKeyword(int? id, KeywordViewModel model)
        {
            if (id.HasValue)
            {
                if (!string.IsNullOrWhiteSpace(model.Name))
                {
                    model.Name = model.Name.Trim();
                    bool isDuplicatedName = _keywordService.IsDuplicatedName(id, model.Name);
                    if (isDuplicatedName)
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
                    Keyword keyword = _keywordService.GetKeywordById(id.Value);
                    if (keyword != null)
                    {
                        keyword.Name = model.Name.Trim();
                        bool resultUpdate = _keywordService.UpdateKeyword(keyword);
                        if (resultUpdate)
                        {
                            return Json(new
                            {
                                success = true,
                                message = "Update keyword successfully!"
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
                            message = "Unavailable keyword!"
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
                    message = "Unavailable keyword!"
                });
            }
        }

        [HttpPost]
        public ActionResult DeleteKeyword(int? id)
        {
            if (!id.HasValue)
            {
                return Json(new
                {
                    success = false,
                    message = "Unavailable keyword!"
                });
            }

            Keyword keyword = _keywordService.GetKeywordById(id.Value);
            if (keyword == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Unavailable keyword!"
                });
            }
            else
            {
                if (_keywordService.IsInUse(keyword))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Keyword is being used! Can not be deleted!"
                    });
                }

                bool resultDelete = _keywordService.DeleteKeyword(keyword);
                if (resultDelete)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Delete keyword successfully!"
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
        public ActionResult GetImpactDetail(int? id)
        {
            if (id.HasValue)
            {
                Impact impact = _impactService.GetImpactById(id.Value);
                return Json(new
                {
                    success = true,
                    name = impact.Name,
                    description = impact.Description
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new
                {
                    success = false,
                    message = "Unavailable impact!"
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
                    if (impact != null)
                    {
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
                    else
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Unavailable impact!"
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
            else
            {
                if (_impactService.IsInUse(impact))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Impact is being used! Can not be deleted!"
                    });
                }
                if (impact.IsSystem)
                {
                    return Json(new
                    {
                        success = false,
                        message = "This is system impact! Can not be deleted!"
                    });
                }
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
                priority.PriorityLevel = model.Level;
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
        public ActionResult GetPriorityDetail(int? id)
        {
            if (id.HasValue)
            {
                Priority priority = _priorityService.GetPriorityByID(id.Value);
                PriorityViewModel model = new PriorityViewModel
                {
                    Id = priority.ID,
                    Name = priority.Name,
                    Description = priority.Description,
                    Color = priority.Color,
                    Level = priority.PriorityLevel
                };
                if (priority != null)
                {
                    return Json(new
                    {
                        success = true,
                        priority = model
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new
            {
                success = false,
                message = "Cannot get priority detail!"
            }, JsonRequestBehavior.AllowGet);
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
                urgency.Duration = 168;
                switch (model.DurationOption)
                {
                    case "hours":
                        urgency.Duration = model.Duration;
                        break;
                    case "days":
                        urgency.Duration = model.Duration * 24;
                        break;
                }

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
        public ActionResult GetUrgencyDetail(int? id)
        {
            if (id.HasValue)
            {
                Urgency urgency = _urgencyService.GetUrgencyByID(id.Value);
                UrgencyViewModel model = new UrgencyViewModel
                {
                    Name = urgency.Name,
                    Description = urgency.Description,
                    Duration = GeneralUtil.GetDuration(urgency.Duration),
                    DurationOption = GeneralUtil.GetDurationOption(urgency.Duration)
                };
                return Json(new
                {
                    success = true,
                    urgency = model
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new
                {
                    success = false,
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
                    switch (model.DurationOption)
                    {
                        case "hours":
                            urgency.Duration = model.Duration;
                            break;
                        case "days":
                            urgency.Duration = model.Duration * 24;
                            break;
                    }

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
                    priority.PriorityLevel = model.Level;
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

                if (priority.IsSystem)
                {
                    return Json(new
                    {
                        success = false,
                        message = "This is system priority! Can not be deleted!"
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

                if (urgency.IsSystem)
                {
                    return Json(new
                    {
                        success = false,
                        message = "This is system urgency! Can not be deleted!"
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
                category.ImpactID = model.ImpactId;
                category.UrgencyID = model.UrgencyId;

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
                    category.ImpactID = model.ImpactId;
                    category.UrgencyID = model.UrgencyId;
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
                    category.ImpactID = model.ImpactId;
                    category.UrgencyID = model.UrgencyId;

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
                var impactList = _impactService.GetAll().Select(m => new
                {
                    m.ID,
                    m.Name
                }).ToArray();

                var urgencyList = _urgencyService.GetAll().Select(m => new
                {
                    m.ID,
                    m.Name
                }).ToArray();

                Category category = _categoryService.GetCategoryById(id.Value);
                CategoryViewModel model = new CategoryViewModel
                {
                    Name = category.Name,
                    Description = category.Description,
                    ImpactId = category.ImpactID,
                    UrgencyId = category.UrgencyID
                };

                if (category != null)
                {
                    return Json(new
                    {
                        success = true,
                        category = model,
                        impacts = impactList,
                        urgencies = urgencyList
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
                var categories = _categoryService.GetCategories().Select(m => new
                {
                    m.ID,
                    m.Name
                }).ToArray();

                var impactList = _impactService.GetAll().Select(m => new
                {
                    m.ID,
                    m.Name
                }).ToArray();

                var urgencyList = _urgencyService.GetAll().Select(m => new
                {
                    m.ID,
                    m.Name
                }).ToArray();

                Category category = _categoryService.GetCategoryById(id.Value);
                CategoryViewModel model = new CategoryViewModel
                {
                    Name = category.Name,
                    Description = category.Description,
                    ParentId = category.ParentID,
                    ImpactId = category.ImpactID,
                    UrgencyId = category.UrgencyID
                };

                if (category != null)
                {
                    return Json(new
                    {
                        success = true,
                        category = model,
                        categories = categories,
                        impacts = impactList,
                        urgencies = urgencyList
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
                var categoryList = _categoryService.GetSubCategories().OrderBy(m => m.ParentID).Select(m => new Category
                {
                    ID = m.ID,
                    Name = _categoryService.GetCategoryById(m.ParentID.Value).Name + " > " + m.Name
                }).ToArray();

                var impactList = _impactService.GetAll().Select(m => new
                {
                    m.ID,
                    m.Name
                }).ToArray();

                var urgencyList = _urgencyService.GetAll().Select(m => new
                {
                    m.ID,
                    m.Name
                }).ToArray();

                Category category = _categoryService.GetCategoryById(id.Value);
                CategoryViewModel model = new CategoryViewModel
                {
                    Name = category.Name,
                    Description = category.Description,
                    ParentId = category.ParentID,
                    ImpactId = category.ImpactID,
                    UrgencyId = category.UrgencyID
                };

                if (category != null)
                {
                    return Json(new
                    {
                        success = true,
                        category = model,
                        categories = categoryList,
                        impacts = impactList,
                        urgencies = urgencyList
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
                    category.ImpactID = model.ImpactId;
                    category.UrgencyID = model.UrgencyId;

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
                        category.ImpactID = model.ImpactId;
                        category.UrgencyID = model.UrgencyId;

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
                        category.ImpactID = model.ImpactId;
                        category.UrgencyID = model.UrgencyId;

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
                    message = "Unavailable impact or urgency!"
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