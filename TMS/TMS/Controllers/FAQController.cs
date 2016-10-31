using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TMS.DAL;
using TMS.Models;
using TMS.Services;
using TMS.Utils;
using TMS.ViewModels;

namespace TMS.Controllers
{
    public class FAQController : Controller
    {
        private UnitOfWork _unitOfWork = new UnitOfWork();
        private UserService _userService;
        private SolutionService _solutionService;
        private FileUploader _fileUploader;
        private CategoryService _categoryService;

        public FAQController()
        {
            _userService = new UserService(_unitOfWork);
            _solutionService = new SolutionService(_unitOfWork);
            _fileUploader = new FileUploader();
            _categoryService = new CategoryService(_unitOfWork);
        }
        // GET: FAQ
        public ActionResult Index()
        {
            ViewBag.Home = "/Index";
            ViewBag.ItemLink1 = "/FAQ/Index";
            ViewBag.Item1 = "FAQ";
            ViewBag.ItemLink2 = "/Ticket/Index";
            ViewBag.Item2 = "Ticket";
            ViewBag.Profile = "#";
            return View();
        }

        [HttpGet]
        public ActionResult GetFAQ()
        {
            IEnumerable<KnowledgeBaseViewModels> filteredListItems;
            filteredListItems = _solutionService.GetAllSolutions().Select(m => new KnowledgeBaseViewModels
            {
                ID = m.ID,
                Subject = m.Subject,
                CategoryID = m.CategoryID,
                CategoryPath = _categoryService.GetCategoryPath(m.Category),
                Content = m.ContentText,
                Keyword = m.Keyword == null ? "-" : m.Keyword,
                CreatedTime = m.CreatedTime,
                ModifiedTime = m.ModifiedTime
            }).OrderByDescending(m => m.ModifiedTime).ToArray().Take(20);

            return Json(new
            {
                data = filteredListItems
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetQuestionsByCategory(string name)
        {
            IEnumerable<KnowledgeBaseViewModels> filteredListItems;
            filteredListItems = _solutionService.GetAllSolutions().Select(m => new KnowledgeBaseViewModels
            {
                ID = m.ID,
                Subject = m.Subject,
                CategoryID = m.CategoryID,
                CategoryPath = _categoryService.GetCategoryPath(m.Category),
                Content = m.ContentText,
                Keyword = m.Keyword == null ? "-" : m.Keyword,
                CreatedTime = m.CreatedTime,
                ModifiedTime = m.ModifiedTime
            }).OrderByDescending(m => m.ModifiedTime).ToArray().Take(20);

            return Json(new
            {
                data = filteredListItems
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetSolutionsByCategory()
        {
            IEnumerable<KnowledgeBaseViewModels> filteredListItems;
            filteredListItems = _solutionService.GetAllSolutions().Select(m => new KnowledgeBaseViewModels
            {
                ID = m.ID,
                Subject = m.Subject,
                CategoryID = m.CategoryID,
                CategoryPath = _categoryService.GetCategoryPath(m.Category),
                Content = m.ContentText,
                Keyword = m.Keyword == null ? "-" : m.Keyword,
                CreatedTime = m.CreatedTime,
                ModifiedTime = m.ModifiedTime
            }).OrderByDescending(m => m.ModifiedTime).ToArray();

            return Json(new
            {
                data = filteredListItems
            }, JsonRequestBehavior.AllowGet);
        }
    }
}