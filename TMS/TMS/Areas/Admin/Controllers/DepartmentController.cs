﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TMS.DAL;
using TMS.Models;
using TMS.Services;
using TMS.ViewModels;

namespace TMS.Areas.Admin.Controllers
{
    public class DepartmentController : Controller
    {
        private UnitOfWork _unitOfWork;
        private DepartmentService _departmentService;

        public DepartmentController()
        {
            _unitOfWork = new UnitOfWork();
            _departmentService = new DepartmentService(_unitOfWork);

        }

        // GET: Admin/Department
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
        public ActionResult CreateDepartment()
        {
            var name = Request["name"];
            var description = Request["description"];
            bool isDuplicateName = _departmentService.IsDuplicatName(null, name.ToLower().Trim());
            if (isDuplicateName)
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
                Department department = new Department();
                department.Name = name.Trim();
                department.Description = description == null ? "" : description.Trim();
                try
                {
                    _departmentService.AddDepartment(department);
                    return Json(new
                    {
                        success = true,
                        message = "Create department succesfully!"
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
                bool isDuplicatedName = _departmentService.IsDuplicatName(id, model.Name.ToLower().Trim());
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
                    Department department = _departmentService.GetDepartmentById(id.Value);
                    department.Name = model.Name.Trim();
                    department.Description = model.Description == null ? "" : model.Description.Trim();
                    _departmentService.EditDepartment(department);
                    return Json(new
                    {
                        success = true,
                        message = "Update department successfully!"
                    });
                }
            }
            else
            {
                return Json(new
                {
                    success = false,
                    error = true,
                    message = "Cannot update department!"
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
                    error = false,
                    message = "Delete department unsuccessfully!"
                });
            }
            Department department = _departmentService.GetDepartmentById(id.Value);
            if (department == null)
            {
                return Json(new
                {
                    success = false,
                    error = true,
                    message = "Delete department unsuccessfully!"
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
                try
                {
                    _departmentService.DeleteDepartment(department);
                    return Json(new
                    {
                        success = true,
                        message = "Delete department successfully!"
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
    }
}