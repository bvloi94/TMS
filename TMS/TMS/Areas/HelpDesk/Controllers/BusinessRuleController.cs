using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TMS.DAL;
using TMS.Enumerator;
using TMS.Models;
using TMS.Services;
using TMS.Utils;
using TMS.ViewModels;

namespace TMS.Areas.HelpDesk.Controllers
{
    [CustomAuthorize(Roles = "Helpdesk")]
    public class BusinessRuleController : Controller
    {

        public BusinessRuleService _businessRuleService { get; set; }
        private UnitOfWork unitOfWork = new UnitOfWork();

        public BusinessRuleController()
        {
            _businessRuleService = new BusinessRuleService(unitOfWork);
        }

        public ActionResult Index()
        { 
            return View();
        }

        public ActionResult New()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LoadAll(JqueryDatatableParameterViewModel param)
        {
            var search_key = Request["search[value]"];

            var queriedResult = _businessRuleService.GetAll();
            IEnumerable<BusinessRule> filteredListItems;

            if (!string.IsNullOrEmpty(search_key))
            {
                filteredListItems = queriedResult.Where(p => p.Name.ToLower().Contains(search_key.ToLower()));
            }
            else
            {
                filteredListItems = queriedResult;
            }

            // Sort.
            var sortColumnIndex = Convert.ToInt32(param.order[0]["column"]);
            var sortDirection = param.order[0]["dir"];

            switch (sortColumnIndex)
            {
                case 1:
                    filteredListItems = sortDirection == "asc"
                        ? filteredListItems.OrderBy(p => p.Name)
                        : filteredListItems.OrderByDescending(p => p.Name);
                    break;
            }

            var result = filteredListItems.Skip(param.start).Take(param.length).ToList();
            var rules = new List<BusinessRuleViewModel>();
            int startNo = param.start;
            foreach (var item in result)
            {
                var s = new BusinessRuleViewModel();
                s.Id = item.ID;
                s.Name = item.Name;
                s.Description = item.Description;
                rules.Add(s);
            }
            JqueryDatatableResultViewModel rsModel = new JqueryDatatableResultViewModel();
            rsModel.draw = param.draw;
            rsModel.recordsTotal = queriedResult.Count();
            rsModel.recordsFiltered = filteredListItems.Count();
            rsModel.data = rules;
            return Json(rsModel, JsonRequestBehavior.AllowGet);
        }
    }
}