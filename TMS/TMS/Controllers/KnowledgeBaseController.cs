using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
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
        [ActionName("Create")]
        public ActionResult CreateGet(KnowledgeBaseViewModel model)
        {
            ModelState.Clear();
            return View(model);
        }

        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (id.HasValue)
            {
                Solution solution = _solutionServices.GetSolutionById(id.Value);
                if (solution != null)
                {
                    KnowledgeBaseViewModel model = new KnowledgeBaseViewModel();
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
        public ActionResult Create(KnowledgeBaseViewModel model)
        {
            // validate subject
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

                if (path.StartsWith("-") || path.EndsWith("-"))
                {
                    ModelState.AddModelError("Path", "Invalid path! (example: how-to-use-tms)");
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


                if (!string.IsNullOrWhiteSpace(model.Keyword))
                {
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
                }


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
        public ActionResult Edit(int? id, KnowledgeBaseViewModel model)
        {
            if (id.HasValue)
            {
                // validate subject
                if (string.IsNullOrWhiteSpace(model.Subject))
                {
                    ModelState.AddModelError("Subject", "Please input subject.");
                }
                else
                {
                    var subject = model.Subject.Trim();
                    bool isDuplicateSubject = _solutionServices.IsDuplicateSubject(id.Value, subject);
                    if (isDuplicateSubject)
                    {
                        ModelState.AddModelError("Subject", string.Format("'{0}' have been used!", subject));
                    }
                }

                if (model.Path != null)
                {
                    var path = model.Path.Trim();

                    bool isDuplicatePath = _solutionServices.IsduplicatePath(id.Value, path);
                    if (isDuplicatePath)
                    {
                        ModelState.AddModelError("Path", string.Format("'{0}' have been used!", path));
                    }

                    if (path.StartsWith("-") || path.EndsWith("-"))
                    {
                        ModelState.AddModelError("Path", "Invalid path! (example: how-to-use-tms)");
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

                        if (!string.IsNullOrWhiteSpace(model.Keyword))
                        {
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
                        }
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
            IEnumerable<KnowledgeBaseViewModel> filteredListItems;
            filteredListItems = _solutionServices.GetAllSolutions().Select(m => new KnowledgeBaseViewModel
            {
                ID = m.ID,
                Subject = m.Subject,
                CategoryPath = _categoryService.GetCategoryPath(m.Category),
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
            IEnumerable<KnowledgeBaseViewModels> filteredListItems;
            if (id.HasValue)
            {
                IEnumerable<KnowledgeBaseViewModel> filteredListItems;
                List<int> childrenCategoriesIdList = _categoryService.GetChildrenCategoriesIdList(id.Value);
                filteredListItems = _solutionServices.GetAllSolutions()
                    .Where(m => m.CategoryID == id.Value || childrenCategoriesIdList.Contains(m.CategoryID))
                    .Select(m => new KnowledgeBaseViewModel
                    {
                        ID = m.ID,
                        Subject = m.Subject,
                        CategoryID = m.CategoryID,
                        CategoryPath = _categoryService.GetCategoryPath(m.Category),
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
            else
            {
                filteredListItems = _solutionServices.GetAllSolutions()
                    .Where(m => m.CategoryID == id.Value)
                    .Select(m => new KnowledgeBaseViewModels
                    {
                        ID = m.ID,
                        Subject = m.Subject,
                        CategoryID = m.CategoryID,
                        CategoryPath = _categoryService.GetCategoryPath(m.Category),
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
        }

        [HttpGet]
        public ActionResult GetSimilarTicketFrequency(JqueryDatatableParameterViewModel param, int? timeOption)
        {
            if (!timeOption.HasValue)
            {
                timeOption = ConstantUtil.TimeOption.ThisWeek;
            }

            IEnumerable<RecentTicketViewModel> recentTickets = _ticketService.GetSimilarTickets(_ticketService.GetRecentTickets(timeOption.Value));

            IEnumerable<RecentTicketViewModel> filteredListItems = recentTickets;
            if (!string.IsNullOrEmpty(param.search["value"]))
            {
                filteredListItems = filteredListItems.Where(p => p.Subject != null && (p.Subject.ToLower().Contains(param.search["value"].ToLower())));
            }

            //if (!string.IsNullOrWhiteSpace(keywords))
            //{
            //    keywords = GeneralUtil.RemoveSpecialCharacters(keywords);
            //    Regex regex = new Regex("[ ]{2,}", RegexOptions.None);
            //    keywords = regex.Replace(keywords, " ");
            //    string[] keywordArr = keywords.Split(' ');
            //    var predicate = PredicateBuilder.False<Ticket>();
            //    foreach (string keyword in keywordArr)
            //    {
            //        predicate = predicate.Or(p => p.Subject.ToLower().Contains(keyword.ToLower()));
            //    }
            //    filteredListItems = filteredListItems.Where(predicate);
            //}

            // Sort.
            var sortColumnIndex = Convert.ToInt32(param.order[0]["column"]);
            var sortDirection = param.order[0]["dir"];

            switch (sortColumnIndex)
            {
                case 0:
                    filteredListItems = sortDirection == "asc"
                        ? filteredListItems.OrderBy(p => p.Subject)
                        : filteredListItems.OrderByDescending(p => p.Subject);
                    break;
                case 1:
                    filteredListItems = sortDirection == "asc"
                        ? filteredListItems.OrderBy(p => p.Count)
                        : filteredListItems.OrderByDescending(p => p.Count);
                    break;
            }

            var displayedList = filteredListItems.Skip(param.start).Take(param.length).Select(m => new RecentTicketViewModel
            {
                Subject = m.Subject,
                Count = m.Count
            });

            //JqueryDatatableResultViewModel rsModel = new JqueryDatatableResultViewModel();
            //rsModel.draw = param.draw;
            //rsModel.recordsTotal = displayedList.ToList().Count();
            //rsModel.recordsFiltered = filteredListItems.Count();
            //rsModel.data = displayedList;
            var totalTicket = 0;
            foreach (RecentTicketViewModel ticket in recentTickets)
            {
                totalTicket += ticket.Count;
            }

            return Json(new
            {
                draw = param.draw,
                recordsTotal = displayedList.ToList().Count(),
                recordsFiltered = filteredListItems.Count(),
                data = displayedList,
                totalTicket = totalTicket
            }, JsonRequestBehavior.AllowGet);
        }
    }
}