using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
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
        public DepartmentService _departmentService { get; set; }
        public UrgencyService _urgencyService { get; set; }
        public PriorityService _priorityService { get; set; }
        public ImpactService _impactService { get; set; }
        public CategoryService _categoryService { get; set; }


        public DropdownController()
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

        public ActionResult LoadDepartmentDropdown()
        {
            var result = new List<DepartmentViewModel>();
            var queryResult = _departmentService.GetAll();
            foreach (var urg in queryResult)
            {
                result.Add(new DepartmentViewModel()
                {
                    Name = urg.Name,
                    Description = urg.Description,
                    Id = urg.ID,
                });

            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadTechnicianDropdown(string query, int? departmentId)
        {
            var result = new List<DropdownTechnicianViewModel>();
            List<AspNetUser> queryResult = null;
            if (!departmentId.HasValue)
            {
                queryResult = _userService.GetTechnicians().ToList();
            }
            else queryResult = _userService.GetTechnicianByPattern(query, departmentId).ToList();
            foreach (var tech in queryResult)
            {
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

        public ActionResult LoadCategoryDropDown()
        {
            var result = new List<CategoryViewModel>();
            addChildCates(ref result, 1, 0);
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

    }
}