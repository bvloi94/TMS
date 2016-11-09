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
    public class DepartmentController : Controller
    {
        private UnitOfWork _unitOfWork;
        private UserService _userService;
        private DepartmentService _departmentService;

        public DepartmentController()
        {
            _unitOfWork = new UnitOfWork();
            _userService = new UserService(_unitOfWork);
            _departmentService = new DepartmentService(_unitOfWork);
        }

        // GET: Manager/Department
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetDepartment(jQueryDataTableParamModel param)
        {

            IEnumerable<Department> departmentList = _departmentService.GetAllDepartment();
            var default_search_key = Request["search[value]"];
            IEnumerable<Department> filteredListItems;
            if (!string.IsNullOrEmpty(default_search_key))
            {
                filteredListItems = departmentList.Where(p => p.Name.ToLower().Contains(default_search_key));
            }
            else
            {
                filteredListItems = departmentList;
            }

            // sort 
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

        [HttpPost]
        public ActionResult CreateDepartment(DepartmentViewModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.Name))
            {
                model.Name = model.Name.Trim();
                bool isDuplicateName = _departmentService.IsDuplicateName(null, model.Name);
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
                Department department = new Department();
                department.Name = model.Name;
                department.Description = model.Description;

                bool addResult = _departmentService.AddDepartment(department);
                if (addResult)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Create department succesfully!"
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
        public ActionResult GetDepartmentDetail()
        {
            try
            {
                int id = Int32.Parse(Request["id"]);
                Department department = _departmentService.GetDepartmentById(id);
                return Json(new
                {
                    success = true,
                    name = department.Name,
                    description = department.Description
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) when (ex is FormatException || ex is ArgumentNullException)
            {
                return Json(new
                {
                    success = false,
                    error = true,
                    message = "Cannot get department detail!"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditDepartment(int? id, DepartmentViewModel model)
        {
            if (id.HasValue)
            {
                if (!string.IsNullOrWhiteSpace(model.Name))
                {
                    model.Name = model.Name.Trim();
                    bool isDuplicateName = _departmentService.IsDuplicateName(id, model.Name);
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
                    Department department = _departmentService.GetDepartmentById(id.Value);
                    department.Name = name;
                    department.Description = model.Description;

                    bool resultEdit = _departmentService.EditDepartment(department);
                    if (resultEdit)
                    {
                        return Json(new
                        {
                            success = true,
                            message = "Update department successfully!"
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
                    message = "Unavailable Department!"
                });
            }
        }

        [HttpPost]
        public ActionResult DeleteDepartment(int? id)
        {
            if (!id.HasValue)
            {
                return Json(new
                {
                    success = false,
                    message = "Unavailable Department!"
                });
            }
            Department department = _departmentService.GetDepartmentById(id.Value);
            if (department == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Unavailable Department!"
                });
            }
            else
            {
                if (_departmentService.IsInUse(department))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Department is being used! Can not be deleted!"
                    });
                }
                bool resultDelete = _departmentService.DeleteDepartment(department);
                if (resultDelete)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Delete department successfully!"
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