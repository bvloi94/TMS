using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TMS.DAL;
using TMS.Services;

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
        public ActionResult GetDepartment ()
        {

            return View();
        }
    }
}