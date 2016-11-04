using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
        private TicketService _ticketService;
        private UserService _userService;
        private SolutionService _solutionService;
        private FileUploader _fileUploader;
        private CategoryService _categoryService;

        public FAQController()
        {
            _ticketService = new TicketService(_unitOfWork);
            _userService = new UserService(_unitOfWork);
            _solutionService = new SolutionService(_unitOfWork);
            _fileUploader = new FileUploader();
            _categoryService = new CategoryService(_unitOfWork);
        }
        // GET: FAQ
        public ActionResult Index(string SearchKey, string Category)
        {
            AspNetRole userRole = null;
            if (User.Identity.GetUserId() != null)
            {
                userRole = _userService.GetUserById(User.Identity.GetUserId()).AspNetRoles.FirstOrDefault();
            }

            GetMenuItemByRoleName(userRole.Name);

            IEnumerable<Solution> solutions = _solutionService.GetAllSolutions();
            IQueryable<KnowledgeBaseViewModel> model = solutions.Select(m => new KnowledgeBaseViewModel
            {
                ID = m.ID,
                Subject = m.Subject,
                Category = m.Category.Name,
                CategoryID = m.Category.ID,
                CategoryPath = _categoryService.GetCategoryPath(m.Category),
                Content = m.ContentText,
                Keyword = m.Keyword == null ? "-" : m.Keyword,
                Path = m.Path,
                CreatedTime = m.CreatedTime,
                ModifiedTime = m.ModifiedTime
            }).AsQueryable().OrderBy(m => m.Subject);

            if (!string.IsNullOrWhiteSpace(SearchKey))
            {

                var predicate = PredicateBuilder.False<KnowledgeBaseViewModel>();

                SearchKey = GeneralUtil.RemoveSpecialCharacters(SearchKey);
                Regex regex = new Regex("[ ]{2,}", RegexOptions.None);
                SearchKey = regex.Replace(SearchKey, " ");
                string[] keywordArr = SearchKey.Split(' ');
                foreach (string keyword in keywordArr)
                {
                    predicate = predicate.Or(p => p.Keyword.ToLower().Contains(keyword.ToLower()));
                }
                predicate = predicate.Or(p => p.Subject.ToLower().Contains(SearchKey.ToLower()));
                model = model.Where(predicate);
                ViewBag.SearchKey = SearchKey;
                return View(model);
            }

            if (!string.IsNullOrWhiteSpace(Category))
            {
                Category category = _categoryService.GetCategoryByName(Category);
                if (category != null)
                {
                    List<int> childrenCategoriesIdList = _categoryService.GetChildrenCategoriesIdList(category.ID);
                    model = model.Where(m => m.CategoryID == category.ID
                        || childrenCategoriesIdList.Contains(m.CategoryID));
                }
                ViewBag.Category = Category;
                return View(model);
            }

            ViewBag.Category = Category;
            ViewBag.SearchKey = SearchKey;
            return View(model);
        }

        public void GetMenuItemByRoleName(string UserRoleName)
        {
            switch (UserRoleName)
            {
                case "Admin":
                    ViewBag.Home = "/FAQ/Index";
                    ViewBag.ItemLink1 = "/Admin/ManageSC/Impact";
                    ViewBag.Item1 = "System Config";
                    ViewBag.ItemLink2 = "/Admin/ManageUser/Admin";
                    ViewBag.Item2 = "Manage User";
                    ViewBag.Profile = "#"; break;
                case "Technician":
                    ViewBag.Home = "/FAQ/Index";
                    ViewBag.ItemLink1 = "/KnowledgeBase/Index";
                    ViewBag.Item1 = "Knowledge Base";
                    ViewBag.ItemLink2 = "/Technician/ManageTicket";
                    ViewBag.Item2 = "Ticket";
                    ViewBag.Profile = "#"; break;
                case "Helpdesk":
                    ViewBag.Home = "/FAQ/Index";
                    ViewBag.ItemLink1 = "/KnowledgeBase/Index";
                    ViewBag.Item1 = "Knowledge Base";
                    ViewBag.ItemLink2 = "/HelpDesk/ManageTicket";
                    ViewBag.Item2 = "Ticket";
                    ViewBag.Profile = "#"; break;
                default:
                    ViewBag.Home = "/FAQ/Index";
                    ViewBag.ItemLink1 = "/FAQ/Index";
                    ViewBag.Item1 = "FAQ";
                    ViewBag.ItemLink2 = "/Ticket/Index";
                    ViewBag.Item2 = "Ticket";
                    ViewBag.Profile = "#";
                    break;
            }
        }

        [HttpGet]
        public ActionResult GetFAQ(int? id, string key_search)
        {
            string keywords = key_search;
            IEnumerable<Solution> solutions = _solutionService.GetAllSolutions();
            IQueryable<KnowledgeBaseViewModel> filteredListItems = solutions.Select(m => new KnowledgeBaseViewModel
            {
                ID = m.ID,
                Subject = m.Subject,
                Category = m.Category.Name,
                CategoryID = m.Category.ID,
                CategoryPath = _categoryService.GetCategoryPath(m.Category),
                Content = m.ContentText,
                Keyword = m.Keyword == null ? "-" : m.Keyword,
                Path = m.Path,
                CreatedTime = m.CreatedTime,
                ModifiedTime = m.ModifiedTime
            }).AsQueryable();

            var predicate = PredicateBuilder.False<KnowledgeBaseViewModel>();

            if (!string.IsNullOrWhiteSpace(key_search))
            {
                keywords = GeneralUtil.RemoveSpecialCharacters(keywords);
                Regex regex = new Regex("[ ]{2,}", RegexOptions.None);
                keywords = regex.Replace(keywords, " ");
                string[] keywordArr = keywords.Split(' ');
                foreach (string keyword in keywordArr)
                {
                    predicate = predicate.Or(p => p.Keyword.ToLower().Contains(keyword.ToLower()));
                }
                predicate = predicate.Or(p => p.Subject.ToLower().Contains(key_search.ToLower()));
                filteredListItems = filteredListItems.Where(predicate);
            }

            if (id.HasValue)
            {
                List<int> childrenCategoriesIdList = _categoryService.GetChildrenCategoriesIdList(id.Value);
                filteredListItems = filteredListItems.Where(m => m.CategoryID == id.Value
                    || childrenCategoriesIdList.Contains(m.CategoryID));
            }
            return Json(new
            {
                data = filteredListItems.OrderBy(m => m.Subject)
            }, JsonRequestBehavior.AllowGet);
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

        [HttpGet]
        public ActionResult Detail(string path)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                Solution solution = _solutionService.GetSolutionByPath(path);
                if (solution != null)
                {
                    KnowledgeBaseViewModel model = new KnowledgeBaseViewModel();
                    model.ID = solution.ID;
                    model.Subject = solution.Subject;
                    model.Content = solution.ContentText;
                    model.CategoryID = solution.CategoryID;
                    model.CategoryPath = _categoryService.GetCategoryPath(solution.Category);
                    if (solution.CreatedTime != null && solution.ModifiedTime != null)
                    {
                        model.CreatedTime = solution.CreatedTime;
                        model.ModifiedTime = solution.ModifiedTime;
                    }
                    model.Keyword = solution.Keyword == null ? "-" : solution.Keyword;
                    model.Path = solution.Path;
                    ViewBag.relatedSolution = LoadRelatedArticle(solution.ID);

                    AspNetRole userRole = null;
                    if (User.Identity.GetUserId() != null)
                    {
                        userRole = _userService.GetUserById(User.Identity.GetUserId()).AspNetRoles.FirstOrDefault();
                    }
                    GetMenuItemByRoleName(userRole.Name);

                    return View(model);
                }
                return HttpNotFound();
            }
            return HttpNotFound();
        }

        public IEnumerable<Solution> LoadRelatedArticle(int id)
        {
            Solution mainSolution = _solutionService.GetSolutionById(id);
            List<int> childrenCategoriesIdList = _categoryService.GetChildrenCategoriesIdList(mainSolution.CategoryID);
            IEnumerable<Solution> relatedSolution = _solutionService.GetAllSolutions()
                .Where(m => m.ID != id && (m.CategoryID == mainSolution.CategoryID || childrenCategoriesIdList.Contains(m.CategoryID)))
                .Select(m => new Solution
                {
                    ID = m.ID,
                    Subject = m.Subject,
                    Category = m.Category,
                    CategoryID = m.CategoryID,
                    ContentText = m.ContentText,
                    Path = m.Path,
                    Keyword = m.Keyword == null ? "-" : m.Keyword,
                    CreatedTime = m.CreatedTime,
                    ModifiedTime = m.ModifiedTime
                }).OrderBy(m => m.Subject).ToList().Take(5);
            return relatedSolution;
        }
    }
}