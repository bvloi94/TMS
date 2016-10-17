using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TMS.Services;
using TMS.DAL;
using TMS.Models;
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
            IEnumerable<Category> categories = _categoryService.GetCategories();
            categories.OrderBy(c => c.CategoryLevel);

            foreach (var cate in categories)
            {
                CategoryViewModel model = new CategoryViewModel();
                model.Name = cate.Name;
                model.Description = cate.Description;
                model.ID = cate.ID;
                if (cate.CategoryLevel != null) model.Level = (int)cate.CategoryLevel;
                if (cate.ParentID != null) model.ParentId = (int)cate.ParentID;
                result.Add(model);
                IEnumerable<Category> subCategories = _categoryService.GetSubCategories(cate.ID);
                subCategories.OrderBy(c => c.Name);
                foreach (var subCate in subCategories)
                {
                    CategoryViewModel ca = new CategoryViewModel();
                    ca.Name = subCate.Name;
                    ca.Description = subCate.Description;
                    ca.ID = subCate.ID;
                    if (subCate.CategoryLevel != null) ca.Level = (int)subCate.CategoryLevel;
                    if (subCate.ParentID != null) ca.ParentId = (int)subCate.ParentID;
                    result.Add(ca);
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

    }
}