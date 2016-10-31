using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
        private CategoryService _categoryService;
        public KnowledgeBaseController()
        {
            _ticketService = new TicketService(_unitOfWork);
            _userService = new UserService(_unitOfWork);
            _solutionServices = new SolutionService(_unitOfWork);
            _fileUploader = new FileUploader();
            _categoryService = new CategoryService(_unitOfWork);
        }

        // GET: KB
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (id.HasValue)
            {
                Solution solution = _solutionServices.GetSolutionById(id.Value);
                if (solution != null)
                {
                    KnowledgeBaseViewModels model = new KnowledgeBaseViewModels();
                    model.Subject = solution.Subject;
                    model.Content = solution.ContentText;
                    model.Keyword = solution.Keyword.Replace("\"", "");
                    model.CategoryID = solution.CategoryID;
                    model.Category = _categoryService.GetCategoryById(solution.CategoryID).Name;
                    model.Path = solution.Path;

                    ViewBag.ID = solution.ID;
                    ViewBag.SolutionAttachments = solution.SolutionAttachments;

                    return View(model);
                }
                else
                {
                    return HttpNotFound();
                }
            }
            else
            {
                return HttpNotFound();
            }
        }


        //   [Utils.Authorize(Roles = "Helpdesk")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(KnowledgeBaseViewModels model)
        {
            if (string.IsNullOrWhiteSpace(model.Subject))
            {
                ModelState.AddModelError("Subject", "Please input subject.");
            }
            else
            {
                var subject = model.Subject.Trim();
                bool isDuplicateSubject = _solutionServices.IsDuplicateSubject(null, subject);
                if (isDuplicateSubject)
                {
                    ModelState.AddModelError("Subject", string.Format("'{0}' have been used!", subject));
                }
            }

            if (string.IsNullOrWhiteSpace(model.Path))
            {
                ModelState.AddModelError("Path", "Please input path.");
            }
            else
            {
                var path = model.Path.Trim();
                bool isDuplicatePath = _solutionServices.IsduplicatePath(null, path);
                if (isDuplicatePath)
                {
                    ModelState.AddModelError("Path", string.Format("'{0}' have been used!", path));
                }

                Match match = Regex.Match(model.Path.Trim(), "^[a-z0-9-]*$", RegexOptions.IgnoreCase);
                if (!match.Success)
                {
                    ModelState.AddModelError("Path", "Path can not contain special characters! ");
                }
            }

            if (model.Keyword != null)
            {
                Match match = Regex.Match(model.Keyword.Trim(), "^[a-z0-9-, ]*$", RegexOptions.IgnoreCase);
                if (!match.Success)
                {
                    ModelState.AddModelError("Keyword", "Keyword only contain characters 'a-z', '0-9' and separated by commas! ");
                }
            }
            if (model.CategoryID == 0)
            {
                ModelState.AddModelError("CategoryID", "Please select topic! ");
            }

            if (model.CategoryID != 0)
            {
                Category category = _categoryService.GetCategoryById(model.CategoryID);
                if (category != null)
                {
                    model.Category = category.Name;
                }
            }
            if (ModelState.IsValid)
            {
                Solution solution = new Solution();
                solution.Subject = model.Subject.Trim().ToLower();
                solution.ContentText = model.Content;
                solution.CategoryID = model.CategoryID;

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
                string keyword = "";

                if (!string.IsNullOrEmpty(model.Keyword))
                {
                    string[] keywordArr = model.Keyword.Trim().ToLower().Split(',');
                    string delimeter = "";
                    foreach (string keywordItem in keywordArr)
                    {
                        if (!string.IsNullOrWhiteSpace(keywordItem))
                        {
                            string keywordItemTmp = keywordItem.Trim().Replace(" ", String.Empty);
                            keyword += delimeter + '"' + keywordItemTmp + '"';
                            delimeter = ",";
                        }
                    }
                }

                solution.Keyword = keyword == null ? "" : keyword.Trim().ToLower();
                solution.Path = model.Path.Trim().ToLower();
                solution.CreatedTime = DateTime.Now;
                try
                {
                    _solutionServices.AddSolution(solution);
                    Response.Cookies.Add(new HttpCookie("FlashMessage", "Create solution successfully!") { Path = "/" });
                    Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "success") { Path = "/" });
                    return RedirectToAction("Index");
                }
                catch
                {
                    Response.Cookies.Add(new HttpCookie("FlashMessage", "Create solution unsuccessfully!") { Path = "/" });
                    Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "error") { Path = "/" });
                    return RedirectToAction("Index");
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int? id, KnowledgeBaseViewModels model)
        {
            if (id.HasValue)
            {
                bool isDuplicateSubject = _solutionServices.IsDuplicateSubject(id, (model.Subject == null ? null : model.Subject.Trim()));
                if (isDuplicateSubject)
                {
                    ModelState.AddModelError("Subject", string.Format("'{0}' have been used!", model.Subject));
                }
                bool isDuplicatePath = _solutionServices.IsduplicatePath(id, model.Path);
                if (isDuplicatePath)
                {
                    ModelState.AddModelError("Path", string.Format("'{0}' have been used!", model.Path));
                }
                if (model.Path != null)
                {
                    Match match = Regex.Match(model.Path.Trim(), "^[a-z0-9-]*$", RegexOptions.IgnoreCase);
                    if (!match.Success)
                    {
                        ModelState.AddModelError("Path", "Path can not contain special characters! ");
                    }
                }
                if (model.Keyword != null)
                {
                    Match match = Regex.Match(model.Keyword.Trim(), "^[a-z0-9-, ]*$", RegexOptions.IgnoreCase);
                    if (!match.Success)
                    {
                        ModelState.AddModelError("Keyword", "Keyword only contain characters 'a-z', '0-9' and separated by commas! ");
                    }
                }

                Solution solution = _solutionServices.GetSolutionById(id.Value);
                if (model.CategoryID != 0)
                {
                    Category category = _categoryService.GetCategoryById(model.CategoryID);
                    if (category != null)
                    {
                        model.Category = category.Name;
                    }
                }
                if (solution != null)
                {
                    if (ModelState.IsValid)
                    {

                        solution.Subject = model.Subject.Trim().ToLower();
                        solution.ContentText = model.Content;
                        string keyword = "";
                        string[] keywordArr = model.Keyword.Trim().ToLower().Split(',');
                        string delimeter = "";
                        foreach (string keywordItem in keywordArr)
                        {
                            if (!string.IsNullOrWhiteSpace(keywordItem))
                            {
                                string keywordItemTmp = keywordItem.Trim().Replace(" ", String.Empty);
                                keyword += delimeter + '"' + keywordItemTmp + '"';
                                delimeter = ",";
                            }
                        }
                        solution.Keyword = keyword;
                        solution.CategoryID = model.CategoryID;
                        solution.Path = model.Path.Trim().ToLower();
                        solution.ModifiedTime = DateTime.Now;
                        string containFolder = "Attachments";
                        if (model.SolutionAttachments != null && model.SolutionAttachments.ToList()[0] != null &&
                            model.SolutionAttachments.ToList().Count > 0)
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
                            _solutionServices.EditSolution(solution);
                            Response.Cookies.Add(new HttpCookie("FlashMessage", "Update solution successfully!") { Path = "/" });
                            Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "success") { Path = "/" });
                            return RedirectToAction("Index");
                        }
                        catch
                        {
                            Response.Cookies.Add(new HttpCookie("FlashMessage", "Update solution unsuccessfully!") { Path = "/" });
                            Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "error") { Path = "/" });
                            return RedirectToAction("Index");
                        }

                    }
                }
                else
                {
                    return HttpNotFound();
                }
                ViewBag.ID = solution.ID;
                ViewBag.SolutionAttachments = solution.SolutionAttachments;

                return View(model);
            }
            else
            {
                return HttpNotFound();
            }
        }

        [HttpGet]
        public ActionResult GetSolutions(string key_search)
        {
            IEnumerable<KnowledgeBaseViewModels> filteredListItems;
            filteredListItems = _solutionServices.GetAllSolutions().Select(m => new KnowledgeBaseViewModels
            {
                ID = m.ID,
                Subject = m.Subject,
                CategoryPath = _categoryServices.GetCategoryPath(m.Category),
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
                List<int> childrenCategoriesIdList = _categoryService.GetChildrenCategoriesIdList(id.Value);
                filteredListItems = _solutionServices.GetAllSolutions()
                    .Where(m => m.CategoryID == id.Value || childrenCategoriesIdList.Contains(m.CategoryID))
                    .Select(m => new KnowledgeBaseViewModels
                    {
                        ID = m.ID,
                        Subject = m.Subject,
                        CategoryPath = _categoryServices.GetCategoryPath(m.Category),
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
        
    }
}