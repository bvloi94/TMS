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
    public class KnowledgeBaseController : Controller
    {
        private UnitOfWork _unitOfWork = new UnitOfWork();
        private TicketService _ticketService;
        private UserService _userService;
        private SolutionService _solutionServices;
        private FileUploader _fileUploader;
        private CategoryService _categoryServices;

        public KnowledgeBaseController()
        {
            _ticketService = new TicketService(_unitOfWork);
            _userService = new UserService(_unitOfWork);
            _solutionServices = new SolutionService(_unitOfWork);
            _fileUploader = new FileUploader();
            _categoryServices = new CategoryService(_unitOfWork);
        }

        // GET: KB
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult CreateKB()
        {
            return View();
        }

        //   [Utils.Authorize(Roles = "Helpdesk")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateKB(KnowledgeBaseViewModels model)
        {
            if (ModelState.IsValid)
            {
                Solution solution = new Solution();
                solution.Subject = model.Subject.Trim();
                solution.ContentText = model.Content;

                string containFolder = "Attachments";
                if (model.SolutionAttachments != null && model.SolutionAttachments.ToList()[0] != null && model.SolutionAttachments.ToList().Count > 0)
                {
                    foreach (HttpPostedFileBase file in model.SolutionAttachments)
                    {
                        SolutionAttachment attachment = new SolutionAttachment();
                        attachment.Path = _fileUploader.UploadFile(file, containFolder);
                        attachment.Filename = file.FileName;
                        solution.SolutionAttachments.Add(attachment);
                    }
                }


                try
                {
                    _solutionServices.AddSolution(solution);
                    return RedirectToAction("Index");
                }
                catch
                {
                    return View(model);
                }
            }
            return View(model);
        }

        [HttpGet]
        public ActionResult GetSolutions(string key_search)
        {
            IEnumerable<KnowledgeBaseViewModels> filteredListItems;
            filteredListItems = _solutionServices.GetAllSolutions().Select(m => new KnowledgeBaseViewModels
            {
                ID = m.ID,
                Subject = m.Subject,
                CategoryID = m.CategoryID,
                Content = m.ContentText,
                Keyword = m.Keyword == null ? "-" : m.Keyword,
                CreatedTime = m.CreatedTime,
                ModifiedTime = m.ModifiedTime
            }).ToArray();

            if (!string.IsNullOrEmpty(key_search))
            {
                filteredListItems = filteredListItems.Where(p => p.Subject.ToLower().Contains(key_search.ToLower()));
            }

            return Json(new
            {
                data = filteredListItems
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetSolutionsByCategory(int? id, string key_search)
        {
            if (id.HasValue)
            {
                IEnumerable<KnowledgeBaseViewModels> filteredListItems;
                List<int> childrenCategoriesIdList = _categoryServices.GetChildrenCategoriesIdList(id.Value);
                filteredListItems = _solutionServices.GetAllSolutions()
                    .Where(m => m.CategoryID == id.Value || childrenCategoriesIdList.Contains(m.CategoryID))
                    .Select(m => new KnowledgeBaseViewModels
                    {
                        ID = m.ID,
                        Subject = m.Subject,
                        CategoryID = m.CategoryID,
                        Content = m.ContentText,
                        Keyword = m.Keyword == null ? "-" : m.Keyword,
                        CreatedTime = m.CreatedTime,
                        ModifiedTime = m.ModifiedTime
                    }).ToArray();

                if (!string.IsNullOrEmpty(key_search))
                {
                    filteredListItems = filteredListItems.Where(p => p.Subject.ToLower().Contains(key_search.ToLower()));
                }

                return Json(new
                {
                    data = filteredListItems
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                success = false,
                error = true,
                msg = "Cannot find solutions!"
            });
        }

        public IEnumerable<Solution> LoadRelatedArticle(int id)
        {
            Solution mainSolution = _solutionServices.GetSolutionById(id);
            List<int> childrenCategoriesIdList = _categoryServices.GetChildrenCategoriesIdList(id);
            IEnumerable<Solution> relatedSolution = _solutionServices.GetAllSolutions()
                .Where(m => (mainSolution.Category.CategoryLevel > 1 ? (m.Category.ParentID == mainSolution.Category.ParentID || m.CategoryID == id) : 
                        (m.CategoryID == id) || (childrenCategoriesIdList.Contains(m.CategoryID))) && m.ID != id)
                .Select(m => new Solution
                {
                    ID = m.ID,
                    Subject = m.Subject,
                    Category = m.Category,
                    ContentText = m.ContentText,
                    Keyword = m.Keyword == null ? "-" : m.Keyword,
                    CreatedTime = m.CreatedTime,
                    ModifiedTime = m.ModifiedTime
                }).ToList().Take(5);
            return relatedSolution;
        }

        [HttpGet]
        public ActionResult Detail(int? id)
        {
            if (id.HasValue)
            {
                Solution solution = _solutionServices.GetSolutionById(id.Value);
                if (solution != null)
                {
                    KnowledgeBaseViewModels model = new KnowledgeBaseViewModels();
                    model.ID = solution.ID;
                    model.Subject = solution.Subject;
                    model.Content = solution.ContentText;
                    model.CategoryPath = _categoryServices.GetCategoryPath(solution.Category);
                    if (solution.CreatedTime != null && solution.ModifiedTime != null)
                    {
                        model.CreatedTime = solution.CreatedTime;
                        model.ModifiedTime = solution.ModifiedTime;
                    }
                    model.Keyword = solution.Keyword == null ? "-" : solution.Keyword;
                    ViewBag.relatedSolution = LoadRelatedArticle(id.Value);
                    ViewBag.categories = _categoryServices.GetAll().Where(m => m.CategoryLevel == 1);
                    ViewBag.subcategories = _categoryServices.GetAll().Where(m => m.CategoryLevel == 2);
                    ViewBag.items = _categoryServices.GetAll().Where(m => m.CategoryLevel == 3);

                    AspNetRole userRole = null;
                    if (User.Identity.GetUserId() != null)
                    {
                        userRole = _userService.GetUserById(User.Identity.GetUserId()).AspNetRoles.FirstOrDefault();
                    }

                    switch (userRole.Name)
                    {
                        case "Requester":
                            ViewBag.ItemLink1 = "/Index";
                            ViewBag.Item1 = "Home";
                            ViewBag.ItemLink2 = "/Ticket/Index";
                            ViewBag.Item2 = "Ticket";
                            ViewBag.Profile = "#";
                            break;
                        case "Admin":
                            ViewBag.ItemLink1 = "/Admin/ManageUser/Admin";
                            ViewBag.Item1 = "Users";
                            ViewBag.ItemLink2 = "/Admin/ManageSC/Impact";
                            ViewBag.Item2 = "System configuration";
                            ViewBag.Profile = "#";
                            break;
                        case "Technician":
                            ViewBag.ItemLink1 = "#";
                            ViewBag.Item1 = "Home";
                            ViewBag.ItemLink2 = "/Technician/ManageTicket";
                            ViewBag.Item2 = "Ticket";
                            ViewBag.Profile = "#";
                            break;
                        case "Helpdesk":
                            ViewBag.ItemLink1 = "/HelpDesk/ManageTicket";
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
    }
}