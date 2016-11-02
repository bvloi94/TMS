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
        public ActionResult GetFAQ(int? id)
        {
            IEnumerable<KnowledgeBaseViewModel> filteredListItems;
            if (id.HasValue)
            {
                List<int> childrenCategoriesIdList = _categoryService.GetChildrenCategoriesIdList(id.Value);
                filteredListItems = _solutionService.GetAllSolutions()
                    .Where(m => m.CategoryID == id.Value || childrenCategoriesIdList.Contains(m.CategoryID))
                    .Select(m => new KnowledgeBaseViewModel
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
                    }).OrderByDescending(m => m.ModifiedTime).ToArray().Take(20);
            }
            else
            {
                filteredListItems = _solutionService.GetAllSolutions().Select(m => new KnowledgeBaseViewModel
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
                }).OrderByDescending(m => m.ModifiedTime).ToArray().Take(20);
            }


            return Json(new
            {
                data = filteredListItems
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

                    switch (userRole.Name)
                    {
                        case "Requester":
                            ViewBag.Home = "/Index";
                            ViewBag.ItemLink1 = "/FAQ/Index";
                            ViewBag.Item1 = "FAQ";
                            ViewBag.ItemLink2 = "/Ticket/Index";
                            ViewBag.Item2 = "Ticket";
                            ViewBag.Profile = "#";
                            break;
                        case "Admin":
                            ViewBag.Home = "#";
                            ViewBag.ItemLink1 = "/Admin/ManageUser/Admin";
                            ViewBag.Item1 = "Users";
                            ViewBag.ItemLink2 = "/Admin/ManageSC/Impact";
                            ViewBag.Item2 = "System configuration";
                            ViewBag.Profile = "#";
                            break;
                        case "Technician":
                            ViewBag.Home = "#";
                            ViewBag.ItemLink1 = "#";
                            ViewBag.Item1 = "Home";
                            ViewBag.ItemLink2 = "/Technician/ManageTicket";
                            ViewBag.Item2 = "Ticket";
                            ViewBag.Profile = "#";
                            break;
                        case "Helpdesk":
                            ViewBag.Home = "#";
                            ViewBag.ItemLink1 = "/KnowledgeBase";
                            ViewBag.Item1 = "Knowledge base";
                            ViewBag.ItemLink2 = "/HelpDesk/ManageTicket";
                            ViewBag.Item2 = "Ticket";
                            ViewBag.Profile = "#";
                            break;
                        default: break;
                    }

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
                .Where(m => (m.CategoryID == mainSolution.CategoryID || (childrenCategoriesIdList.Contains(m.CategoryID)) && m.ID != id))
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
                }).ToList().Take(5);
            return relatedSolution;
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
    }
}