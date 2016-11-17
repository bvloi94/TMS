using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TMS.Services;
using TMS.DAL;
using TMS.Models;
using TMS.Utils;
using TMS.ViewModels;

namespace TMS.Controllers
{
    public class DropdownController : Controller
    {
        public TicketService _ticketService { get; set; }
        public UserService _userService { get; set; }
        public GroupService _groupService { get; set; }
        public UrgencyService _urgencyService { get; set; }
        public PriorityService _priorityService { get; set; }
        public ImpactService _impactService { get; set; }
        public CategoryService _categoryService { get; set; }

        public DropdownController()
        {
            var unitOfWork = new UnitOfWork();
            _ticketService = new TicketService(unitOfWork);
            _userService = new UserService(unitOfWork);
            _groupService = new GroupService(unitOfWork);
            _urgencyService = new UrgencyService(unitOfWork);
            _priorityService = new PriorityService(unitOfWork);
            _impactService = new ImpactService(unitOfWork);
            _categoryService = new CategoryService(unitOfWork);
        }

        public ActionResult LoadUrgencyDropdown()
        {
            var result = new List<UrgencyViewModel>();
            result.Add(new UrgencyViewModel()
            {
                Name = "None",
                Description = "None",
                Id = 0,
            });
            var queryResult = _urgencyService.GetAll();
            foreach (var urg in queryResult)
            {
                result.Add(new UrgencyViewModel
                {
                    Name = urg.Name,
                    Description = urg.Description,
                    Id = urg.ID,
                });
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadPriorityDropdown()
        {
            var result = new List<PriorityViewModel>();
            result.Add(new PriorityViewModel()
            {
                Name = "None",
                Description = "None",
                Id = 0,
            });
            var queryResult = _priorityService.GetAll();
            foreach (var urg in queryResult)
            {
                result.Add(new PriorityViewModel()
                {
                    Name = urg.Name,
                    Description = urg.Description,
                    Id = urg.ID,
                });
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadImpactDropDown()
        {
            var result = new List<ImpactViewModel>();
            result.Add(new ImpactViewModel()
            {
                Name = "None",
                Description = "None",
                Id = 0,
            });
            var queryResult = _impactService.GetAll();
            foreach (var urg in queryResult)
            {
                result.Add(new ImpactViewModel()
                {
                    Name = urg.Name,
                    Description = urg.Description,
                    Id = urg.ID,
                });
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadGroupDropdown()
        {
            var result = new List<GroupViewModel>();
            var queryResult = _groupService.GetAll();
            result.Add(new GroupViewModel()
            {
                Name = "None",
                Description = "None",
                Id = 0,
            });
            foreach (var urg in queryResult)
            {
                result.Add(new GroupViewModel()
                {
                    Name = urg.Name,
                    Description = urg.Description,
                    Id = urg.ID,
                });

            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadTechnicianDropdown(string ignore, string query, int? groupId)
        {
            var js = new JavaScriptSerializer();
            var ignoreItems = (object[])js.DeserializeObject(ignore);

            var result = new List<DropdownTechnicianViewModel>();
            List<AspNetUser> queryResult = _userService.GetTechnicianByPattern(query, groupId).ToList();
            foreach (var tech in queryResult)
            {
                if (ignoreItems != null && ignoreItems.Length > 0)
                {
                    if (ignoreItems.Any(a => (string)a == tech.Id))
                    {
                        continue;
                    }
                }
                var technicianItem = new DropdownTechnicianViewModel
                {
                    Id = tech.Id,
                    Name = tech.Fullname,
                    Email = tech.Email
                };
                result.Add(technicianItem);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadCategoryDropDown(string query)
        {
            var result = new List<CategoryViewModel>();
            addChildCates(ref result, 1, 0);
            List<CategoryViewModel> queriedList = new List<CategoryViewModel>();
            if (!string.IsNullOrEmpty(query))
            {
                foreach (var cate in result)
                {
                    if (cate.Name.ToLower().Contains(query.ToLower()))
                    {
                        cate.Level = 1;
                        queriedList.Add(cate);
                    }
                }
                return Json(queriedList, JsonRequestBehavior.AllowGet);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        void addChildCates(ref List<CategoryViewModel> cates, int level, int parentId)
        {
            IEnumerable<Category> childCategories = null;
            switch (level)
            {
                case 1:
                    childCategories = _categoryService.GetCategories();
                    break;
                case 2:
                    childCategories = _categoryService.GetSubCategories(parentId);
                    break;
                case 3:
                    childCategories = _categoryService.GetItems(parentId);
                    break;
            }
            childCategories.OrderBy(c => c.Name);
            foreach (var child in childCategories)
            {
                CategoryViewModel cate = new CategoryViewModel();
                cate.Name = child.Name;
                cate.Description = child.Description;
                cate.ID = child.ID;
                if (child.CategoryLevel != null) cate.Level = (int)child.CategoryLevel;
                if (child.ParentID != null) cate.ParentId = (int)child.ParentID;
                cates.Add(cate);
                if (level < ConstantUtil.CategoryLevel.Item) addChildCates(ref cates, level + 1, cate.ID.Value);
            }
        }

        public ActionResult LoadCategoryDropdownByLevel(int level)
        {
            List<DropDownViewModel> result = new List<DropDownViewModel>();
            IEnumerable<Category> categories = null;
            switch (level)
            {
                case 1:
                    categories = _categoryService.GetCategories();
                    break;
                case 2:
                    categories = _categoryService.GetSubCategories();
                    break;
                case 3:
                    categories = _categoryService.GetItems();
                    break;
            }
            foreach (var cate in categories)
            {
                DropDownViewModel item = new DropDownViewModel();
                item.Id = cate.ID.ToString();
                item.Name = cate.Name;
                var parentId = cate.ParentID;
                while (parentId != null)
                {
                    Category parentCategory = _categoryService.GetCategoryById((int)parentId);
                    item.Name = parentCategory.Name + " >> " + item.Name;
                    parentId = parentCategory.ParentID;
                }
                result.Add(item);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public ActionResult LoadStatusDropdown()
        {
            return Json(TMSUtils.GetDefaultStatus(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadModeDropdown(string query)
        {
            IEnumerable<DropDownViewModel> modeList = TMSUtils.GetDefaultMode();
            modeList = modeList.Where(m => m.Name.Contains(query));
            return Json(modeList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadActionDropdown()
        {
            return Json(TMSUtils.GetDefaultActions(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadCriteriaDropdown(string query)
        {
            List<DropDownViewModel> criterias = TMSUtils.GetDefaultCritetia();
            if (query != null)
            {
                List<DropDownViewModel> result = new List<DropDownViewModel>();
                foreach (var criteria in criterias)
                {
                    if (criteria.Name.Contains(query)) result.Add(criteria);
                }
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            return Json(criterias, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadConditionDropdown(string query, string criteria)
        {
            List<DropDownViewModel> conditions = TMSUtils.GetDefaultCondition();
            List<DropDownViewModel> result = new List<DropDownViewModel>();
            query = query ?? "";
            foreach (var condition in conditions)
            {
                if (condition.Name.ToLower().Contains(query.ToLower()))
                {
                    if (criteria != "Subject" && criteria != "Description")
                    {
                        if (condition.Name == "is" || condition.Name == "is not")
                        {
                            result.Add(condition);
                        }
                    }
                    else
                    {
                        result.Add(condition);
                    }
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public ActionResult LoadRequesterDropdown(string ignore, string query)
        {
            var js = new JavaScriptSerializer();
            var ignoreItems = (object[])js.DeserializeObject(ignore);
            var result = new List<DropdownTechnicianViewModel>();
            List<AspNetUser> queryResult = _userService.SearchRequesters(query).ToList();
            foreach (var tech in queryResult)
            {
                if (ignoreItems != null && ignoreItems.Length > 0)
                {
                    if (ignoreItems.Any(a => (string)a == tech.Id))
                    {
                        continue;
                    }
                }
                var requesterItem = new DropdownTechnicianViewModel
                {
                    Id = tech.Id,
                    Name = tech.Fullname,
                    Email = tech.Email
                };
                result.Add(requesterItem);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadGroupConditionDropdown(string ignore, string query)
        {
            var js = new JavaScriptSerializer();
            var ignoreItems = (object[])js.DeserializeObject(ignore);

            var result = new List<GroupViewModel>();
            var queryResult = _groupService.GetAll();
            foreach (var group in queryResult)
            {
                if (ignoreItems != null && ignoreItems.Length > 0)
                {
                    if (ignoreItems.Any(a => (string)a == group.ID.ToString()))
                    {
                        continue;
                    }
                }
                var dep = new GroupViewModel
                {
                    Id = group.ID,
                    Name = group.Name,
                };
                result.Add(dep);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadConditionValueDropdown(string ignore, string query, string criteria)
        {
            var js = new JavaScriptSerializer();
            var ignoreItems = (object[])js.DeserializeObject(ignore);
            var result = new List<DropDownViewModel>();
            query = query ?? "";
            query = query.ToLower();
            switch (criteria)
            {
                case "Group":
                    var groupResult = _groupService.GetAll();
                    groupResult = groupResult.Where(d => d.Name.Contains(query));
                    foreach (var item in groupResult)
                    {
                        if (ignoreItems != null && ignoreItems.Length > 0)
                        {
                            if (ignoreItems.Any(a => a.ToString() == item.ID.ToString()))
                            {
                                continue;
                            }
                        }
                        var newItem = new DropDownViewModel
                        {
                            Id = item.ID.ToString(),
                            Name = item.Name,
                        };
                        result.Add(newItem);
                    }
                    break;
                case "Priority":
                    var priorityResult = _priorityService.GetAll();
                    priorityResult = priorityResult.Where(d => d.Name.ToString().ToLower().Contains(query));
                    foreach (var item in priorityResult)
                    {
                        if (ignoreItems != null && ignoreItems.Length > 0)
                        {
                            if (ignoreItems.Any(a => a.ToString() == item.ID.ToString()))
                            {
                                continue;
                            }
                        }
                        var newItem = new DropDownViewModel
                        {
                            Id = item.ID.ToString(),
                            Name = item.Name,
                        };
                        result.Add(newItem);
                    }
                    break;
                case "Impact":
                    var impactResult = _impactService.GetAll();
                    impactResult = impactResult.Where(d => d.Name.ToString().ToLower().Contains(query));
                    foreach (var item in impactResult)
                    {
                        if (ignoreItems != null && ignoreItems.Length > 0)
                        {
                            if (ignoreItems.Any(a => a.ToString() == item.ID.ToString()))
                            {
                                continue;
                            }
                        }
                        var newItem = new DropDownViewModel
                        {
                            Id = item.ID.ToString(),
                            Name = item.Name,
                        };
                        result.Add(newItem);
                    }
                    break;
                case "Urgency":
                    var urgencyResult = _urgencyService.GetAll();
                    urgencyResult = urgencyResult.Where(d => d.Name.ToString().ToLower().Contains(query));
                    foreach (var item in urgencyResult)
                    {
                        if (ignoreItems != null && ignoreItems.Length > 0)
                        {
                            if (ignoreItems.Any(a => a.ToString() == item.ID.ToString()))
                            {
                                continue;
                            }
                        }
                        var newItem = new DropDownViewModel
                        {
                            Id = item.ID.ToString(),
                            Name = item.Name,
                        };
                        result.Add(newItem);
                    }
                    break;
                case "Mode":
                    IEnumerable<DropDownViewModel> modeList = TMSUtils.GetDefaultMode();
                    modeList = modeList.Where(m => m.Name.ToString().ToLower().Contains(query));
                    foreach (var item in modeList)
                    {
                        if (ignoreItems != null && ignoreItems.Length > 0)
                        {
                            if (ignoreItems.Any(a => a.ToString() == item.Id.ToString()))
                            {
                                continue;
                            }
                        }
                        result.Add(item);
                    }
                    break;

                    //case "Category":
                    //    var categoryResult = new List<CategoryViewModel>();
                    //    addChildCates(ref categoryResult, 1, 0);
                    //    foreach (var item in categoryResult)
                    //    {
                    //        var newItem = new DropDownViewModel
                    //        {
                    //            Id = item.ID,
                    //            Name = item.Name,
                    //        };
                    //        result.Add(newItem);
                    //    }
                    //    break;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadConditionValueDropdownInit(string ids, string criteria)
        {
            var result = new List<DropDownViewModel>();
            string[] idList = ids.Split(',');
            if (idList.Length > 0)
            {
                switch (criteria)
                {
                    case "Group":
                        foreach (var id in idList)
                        {
                            var item = _groupService.GetGroupById(Int32.Parse(id));
                            if (item != null)
                            {
                                var newItem = new DropDownViewModel
                                {
                                    Id = item.ID.ToString(),
                                    Name = item.Name,
                                };

                                result.Add(newItem);
                            }
                        }
                        break;
                    case "Priority":
                        foreach (var id in idList)
                        {
                            var item = _priorityService.GetPriorityByID(Int32.Parse(id));
                            if (item != null)
                            {
                                var newItem = new DropDownViewModel
                                {
                                    Id = item.ID.ToString(),
                                    Name = item.Name,
                                };

                                result.Add(newItem);
                            }
                        }
                        break;
                    case "Impact":
                        foreach (var id in idList)
                        {
                            var item = _impactService.GetImpactById(Int32.Parse(id));
                            if (item != null)
                            {
                                var newItem = new DropDownViewModel
                                {
                                    Id = item.ID.ToString(),
                                    Name = item.Name,
                                };

                                result.Add(newItem);
                            }
                        }
                        break;
                    case "Urgency":
                        foreach (var id in idList)
                        {
                            var item = _urgencyService.GetUrgencyByID(Int32.Parse(id));
                            if (item != null)
                            {
                                var newItem = new DropDownViewModel
                                {
                                    Id = item.ID.ToString(),
                                    Name = item.Name,
                                };

                                result.Add(newItem);
                            }
                        }
                        break;
                    case "Category":
                        foreach (var id in idList)
                        {
                            var item = _categoryService.GetCategoryById(Int32.Parse(id));
                            if (item != null)
                            {
                                var newItem = new DropDownViewModel
                                {
                                    Id = item.ID.ToString(),
                                    Name = item.Name,
                                };

                                result.Add(newItem);
                            }
                        }
                        break;
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}