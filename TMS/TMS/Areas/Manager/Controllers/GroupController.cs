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
    public class GroupController : Controller
    {
        private UnitOfWork _unitOfWork;
        private UserService _userService;
        private GroupService _groupService;

        public GroupController()
        {
            _unitOfWork = new UnitOfWork();
            _userService = new UserService(_unitOfWork);
            _groupService = new GroupService(_unitOfWork);
        }

        // GET: Manager/Group
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetGroup(jQueryDataTableParamModel param)
        {
            IEnumerable<Group> groupList = _groupService.GetAllGroup();
            var default_search_key = Request["search[value]"];
            IEnumerable<Group> filteredListItems;
            if (!string.IsNullOrEmpty(default_search_key))
            {
                filteredListItems = groupList.Where(p => p.Name.ToLower().Contains(default_search_key));
            }
            else
            {
                filteredListItems = groupList;
            }

            // sort 
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
        public ActionResult CreateGroup(GroupViewModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.Name))
            {
                model.Name = model.Name.Trim();
                bool isDuplicateName = _groupService.IsDuplicateName(null, model.Name);
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
                Group group = new Group();
                group.Name = model.Name;
                group.Description = model.Description;

                bool addResult = _groupService.AddGroup(group);
                if (addResult)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Create group succesfully!"
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
        public ActionResult GetGroupDetail()
        {
            try
            {
                int id = Int32.Parse(Request["id"]);
                Group group = _groupService.GetGroupById(id);
                return Json(new
                {
                    success = true,
                    name = group.Name,
                    description = group.Description
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) when (ex is FormatException || ex is ArgumentNullException)
            {
                return Json(new
                {
                    success = false,
                    error = true,
                    message = "Cannot get group detail!"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditGroup(int? id, GroupViewModel model)
        {
            if (id.HasValue)
            {
                if (!string.IsNullOrWhiteSpace(model.Name))
                {
                    model.Name = model.Name.Trim();
                    bool isDuplicateName = _groupService.IsDuplicateName(id, model.Name);
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
                    Group group = _groupService.GetGroupById(id.Value);
                    group.Name = name;
                    group.Description = model.Description;

                    bool resultEdit = _groupService.EditGroup(group);
                    if (resultEdit)
                    {
                        return Json(new
                        {
                            success = true,
                            message = "Update group successfully!"
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
                    message = "Unavailable Group!"
                });
            }
        }

        [HttpPost]
        public ActionResult DeleteGroup(int? id)
        {
            if (!id.HasValue)
            {
                return Json(new
                {
                    success = false,
                    message = "Unavailable Group!"
                });
            }
            Group group = _groupService.GetGroupById(id.Value);
            if (group == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Unavailable Group!"
                });
            }
            else
            {
                if (_groupService.IsInUse(group))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Group is being used! Can not be deleted!"
                    });
                }
                bool resultDelete = _groupService.DeleteGroup(group);
                if (resultDelete)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Delete group successfully!"
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