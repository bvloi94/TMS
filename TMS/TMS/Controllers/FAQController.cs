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
    [CustomAuthorize]
    public class FAQController : Controller
    {
        private UnitOfWork _unitOfWork = new UnitOfWork();
        private TicketService _ticketService;
        private UserService _userService;
        private SolutionService _solutionService;
        private FileUploader _fileUploader;
        private CategoryService _categoryService;
        private KeywordService _keywordService;
        private SolutionAttachmentService _solutionAttachmentService;

        public FAQController()
        {
            _ticketService = new TicketService(_unitOfWork);
            _userService = new UserService(_unitOfWork);
            _solutionService = new SolutionService(_unitOfWork);
            _fileUploader = new FileUploader();
            _categoryService = new CategoryService(_unitOfWork);
            _keywordService = new KeywordService(_unitOfWork);
            _solutionAttachmentService = new SolutionAttachmentService(_unitOfWork);
        }
        // GET: FAQ
        public ActionResult Index(string search)
        {
            IEnumerable<Solution> solutions = _solutionService.GetAllSolutions();
            IEnumerable<KnowledgeBaseViewModel> model = solutions
            .Select(m => new KnowledgeBaseViewModel
            {
                ID = m.ID,
                Subject = m.Subject,
                Category = m.Category.Name,
                CategoryID = m.Category.ID,
                CategoryPath = _categoryService.GetCategoryPath(m.Category),
                Content = m.ContentText,
                Keywords = _keywordService.GetSolutionKeywordForDisplay(m.ID),
                Path = m.Path,
                CreatedTime = m.CreatedTime,
                ModifiedTime = m.ModifiedTime,
                IsPublish = m.IsPublish
            }).OrderBy(m => m.Subject);

            if (!string.IsNullOrWhiteSpace(search))
            {
                model = model.Where(p => p.Subject.ToLower().Contains(search.ToLower()));
                ViewBag.SearchKey = search;
            }

            AspNetRole userRole = null;
            if (User.Identity.GetUserId() != null)
            {
                userRole = _userService.GetUserById(User.Identity.GetUserId()).AspNetRoles.FirstOrDefault();
                model = GetDataByRoleName(userRole.Name, model);
            }

            return View(model);
        }

        public IEnumerable<KnowledgeBaseViewModel> GetDataByRoleName(string UserRoleName, IEnumerable<KnowledgeBaseViewModel> data)
        {
            IEnumerable<KnowledgeBaseViewModel> filteredData = data;
            switch (UserRoleName)
            {
                case ConstantUtil.UserRoleString.Technician:
                case ConstantUtil.UserRoleString.HelpDesk:
                    break;
                case ConstantUtil.UserRoleString.Requester:
                default:
                    filteredData = data.Where(m => m.IsPublish == true);
                    break;
            }
            return filteredData;
        }

        public void GetMenuItemByRoleName(string UserRoleName)
        {
            switch (UserRoleName)
            {
                case ConstantUtil.UserRoleString.Technician:
                    ViewBag.Home = "/FAQ/Index";
                    ViewBag.ItemLink1 = "/KnowledgeBase/Index";
                    ViewBag.Item1 = "Knowledge Base";
                    ViewBag.ItemLink2 = "/Technician/ManageTicket";
                    ViewBag.Item2 = "Ticket";
                    ViewBag.Profile = "/Technician/Profile";
                    break;
                case ConstantUtil.UserRoleString.HelpDesk:
                    ViewBag.Home = "/FAQ/Index";
                    ViewBag.ItemLink1 = "/KnowledgeBase/Index";
                    ViewBag.Item1 = "Knowledge Base";
                    ViewBag.ItemLink2 = "/HelpDesk/ManageTicket";
                    ViewBag.Item2 = "Ticket";
                    ViewBag.Profile = "/Helpdesk/Profile";
                    break;
                case ConstantUtil.UserRoleString.Requester:
                    ViewBag.Home = "/FAQ/Index";
                    ViewBag.ItemLink1 = "/Home/Index";
                    ViewBag.Item1 = "Home";
                    ViewBag.ItemLink2 = "/Ticket/Index";
                    ViewBag.Item2 = "Ticket";
                    ViewBag.Profile = "/Profile";
                    break;
                default:
                    ViewBag.Home = "/FAQ/Index";
                    break;
            }
        }

        //[HttpGet]
        //public ActionResult GetFAQ(int? id, string key_search)
        //{
        //    string keywords = key_search;
        //    IEnumerable<Solution> solutions = _solutionService.GetAllSolutions();
        //    IQueryable<KnowledgeBaseViewModel> filteredListItems = solutions.Select(m => new KnowledgeBaseViewModel
        //    {
        //        ID = m.ID,
        //        Subject = m.Subject,
        //        Category = m.Category.Name,
        //        CategoryID = m.Category.ID,
        //        CategoryPath = _categoryService.GetCategoryPath(m.Category),
        //        Content = m.ContentText,
        //        //Keyword = m.Keyword == null ? "-" : m.Keyword,
        //        Path = m.Path,
        //        CreatedTime = m.CreatedTime,
        //        ModifiedTime = m.ModifiedTime
        //    }).AsQueryable();

        //    var predicate = PredicateBuilder.False<KnowledgeBaseViewModel>();

        //    if (!string.IsNullOrWhiteSpace(key_search))
        //    {
        //        //keywords = GeneralUtil.RemoveSpecialCharacters(keywords);
        //        //Regex regex = new Regex("[ ]{2,}", RegexOptions.None);
        //        //keywords = regex.Replace(keywords, " ");
        //        //string[] keywordArr = keywords.Split(' ');
        //        //foreach (string keyword in keywordArr)
        //        //{
        //        //    predicate = predicate.Or(p => p.Keyword.ToLower().Contains(keyword.ToLower()));
        //        //}
        //        //predicate = predicate.Or(p => p.Subject.ToLower().Contains(key_search.ToLower()));
        //        //filteredListItems = filteredListItems.Where(predicate);
        //    }

        //    if (id.HasValue)
        //    {
        //        List<int> childrenCategoriesIdList = _categoryService.GetChildrenCategoriesIdList(id.Value);
        //        filteredListItems = filteredListItems.Where(m => m.CategoryID == id.Value
        //            || childrenCategoriesIdList.Contains(m.CategoryID));
        //    }
        //    return Json(new
        //    {
        //        data = filteredListItems.OrderBy(m => m.Subject)
        //    }, JsonRequestBehavior.AllowGet);
        //}

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
                    AspNetRole userRole = null;
                    if (User.Identity.GetUserId() != null)
                    {
                        userRole = _userService.GetUserById(User.Identity.GetUserId()).AspNetRoles.FirstOrDefault();
                        if (userRole.Name != ConstantUtil.UserRoleString.HelpDesk
                            && userRole.Name != ConstantUtil.UserRoleString.Technician)
                        {
                            if (solution.IsPublish == false)
                            {
                                return HttpNotFound();
                            }
                        }
                    }
                    KnowledgeBaseViewModel model = new KnowledgeBaseViewModel();
                    model.ID = solution.ID;
                    model.Subject = solution.Subject;
                    model.Content = solution.ContentText;
                    model.CategoryID = solution.CategoryID;
                    model.Category = solution.Category.Name;
                    model.CategoryPath = _categoryService.GetCategoryPath(solution.Category);
                    if (solution.CreatedTime != null && solution.ModifiedTime != null)
                    {
                        model.CreatedTime = solution.CreatedTime;
                        model.ModifiedTime = solution.ModifiedTime;
                    }
                    model.Keywords = _keywordService.GetSolutionKeywordForDisplay(solution.ID);
                    model.Path = solution.Path;
                    model.SolutionAttachmentsURL = GetSolutionAttachmentUrl(solution.ID);

                    ViewBag.relatedSolution = LoadRelatedArticle(solution.ID);

                    return View(model);
                }
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
                    CreatedTime = m.CreatedTime,
                    ModifiedTime = m.ModifiedTime,
                    IsPublish = m.IsPublish
                });

            AspNetRole userRole = null;
            if (User.Identity.GetUserId() != null)
            {
                userRole = _userService.GetUserById(User.Identity.GetUserId()).AspNetRoles.FirstOrDefault();
                switch (userRole.Name)
                {
                    case ConstantUtil.UserRoleString.HelpDesk:
                    case ConstantUtil.UserRoleString.Technician:
                        break;
                    default:
                        relatedSolution = relatedSolution.Where(m => m.IsPublish == true);
                        break;
                }
            }
            relatedSolution = relatedSolution.OrderBy(m => m.Subject).ToList().Take(5);

            return relatedSolution;
        }

        public ActionResult Category(string category)
        {
            IEnumerable<Solution> solutions = _solutionService.GetAllSolutions();
            IEnumerable<KnowledgeBaseViewModel> model = solutions.Select(m => new KnowledgeBaseViewModel
            {
                ID = m.ID,
                Subject = m.Subject,
                Category = m.Category.Name,
                CategoryID = m.Category.ID,
                CategoryPath = _categoryService.GetCategoryPath(m.Category),
                Content = m.ContentText,
                Keywords = _keywordService.GetSolutionKeywordForDisplay(m.ID),
                Path = m.Path,
                CreatedTime = m.CreatedTime,
                ModifiedTime = m.ModifiedTime,
                IsPublish = m.IsPublish
            }).OrderBy(m => m.Subject);

            if (!string.IsNullOrWhiteSpace(category))
            {
                Category cate = _categoryService.GetCategoryByName(category);
                if (cate != null)
                {
                    List<int> childrenCategoriesIdList = _categoryService.GetChildrenCategoriesIdList(cate.ID);
                    model = model.Where(m => m.CategoryID == cate.ID
                        || childrenCategoriesIdList.Contains(m.CategoryID));
                    ViewBag.Category = cate.Name;
                }
            }

            AspNetRole userRole = null;
            if (User.Identity.GetUserId() != null)
            {
                userRole = _userService.GetUserById(User.Identity.GetUserId()).AspNetRoles.FirstOrDefault();
                model = GetDataByRoleName(userRole.Name, model);
            }

            return View("Index", model);
        }

        public ActionResult Tags(string tag)
        {
            IEnumerable<Solution> solutions = _solutionService.GetSolutionsByTag(tag);
            IEnumerable<KnowledgeBaseViewModel> model = solutions.Select(m => new KnowledgeBaseViewModel
            {
                ID = m.ID,
                Subject = m.Subject,
                Category = m.Category.Name,
                CategoryID = m.Category.ID,
                CategoryPath = _categoryService.GetCategoryPath(m.Category),
                Content = m.ContentText,
                Keywords = _keywordService.GetSolutionKeywordForDisplay(m.ID),
                Path = m.Path,
                CreatedTime = m.CreatedTime,
                ModifiedTime = m.ModifiedTime,
                IsPublish = m.IsPublish
            }).OrderBy(m => m.Subject);

            ViewBag.Tag = tag;

            AspNetRole userRole = null;
            if (User.Identity.GetUserId() != null)
            {
                userRole = _userService.GetUserById(User.Identity.GetUserId()).AspNetRoles.FirstOrDefault();
                model = GetDataByRoleName(userRole.Name, model);
            }

            return View("Index", model);
        }

        public string GetSolutionAttachmentUrl(int? id)
        {
            IEnumerable<SolutionAttachment> solutionAttachments = _solutionAttachmentService.GetSolutionAttachmentBySolutionID(id.Value);
            string attachmentUrl = "";

            if (solutionAttachments.Count() > 0)
            {
                string fileName;
                foreach (var attachFile in solutionAttachments)
                {
                    fileName = TMSUtils.GetMinimizedAttachmentName(attachFile.Filename);
                    attachmentUrl += "<a download=\'" + fileName +
                                     "\' class=\'ibtn-xs ibtn-primary ibtn-flat\' href=\'" + attachFile.Path +
                                     "\' target=\'_blank\' >" + fileName + "</a> &nbsp;";
                }
            }

            return attachmentUrl == "" ? "None" : attachmentUrl;
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            AspNetRole userRole = null;
            if (User.Identity.GetUserId() != null)
            {
                userRole = _userService.GetUserById(User.Identity.GetUserId()).AspNetRoles.FirstOrDefault();
                GetMenuItemByRoleName(userRole.Name);
            }

            base.OnActionExecuting(filterContext);
        }
    }
}