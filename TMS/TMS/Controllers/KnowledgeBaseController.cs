using log4net;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
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
        private SolutionAttachmentService _solutionAttachmentService;

        public KnowledgeBaseController()
        {
            _ticketService = new TicketService(_unitOfWork);
            _userService = new UserService(_unitOfWork);
            _solutionServices = new SolutionService(_unitOfWork);
            _fileUploader = new FileUploader();
            _categoryService = new CategoryService(_unitOfWork);
            _solutionAttachmentService = new SolutionAttachmentService(_unitOfWork);
        }

        // GET: KB
        public ActionResult Index()
        {
            switch (UserRole())
            {
                case "Admin": ViewBag.Layout = "~/Areas/Admin/Views/Shared/_Layout.cshtml"; break;
                case "Technician": ViewBag.Layout = "~/Areas/Technician/Views/Shared/_Layout.cshtml"; break;
                case "Helpdesk": ViewBag.Layout = "~/Areas/HelpDesk/Views/Shared/_Layout.cshtml"; break;
                default: ViewBag.Layout = "~/Views/Shared/TMSRequesterLayout.cshtml"; break;
            }
            return View();
        }

        public string UserRole()
        {
            AspNetRole userRole = null;
            if (User.Identity.GetUserId() != null)
            {
                userRole = _userService.GetUserById(User.Identity.GetUserId()).AspNetRoles.FirstOrDefault();
                return userRole.Name;
            }

            return "Guest";
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
            // id != null.
            if (id.HasValue)
            {
                // Get solution by ID.
                Solution solution = _solutionServices.GetSolutionById(id.Value);
                // solution != null.
                if (solution != null)
                {
                    KnowledgeBaseViewModel model = new KnowledgeBaseViewModel();
                    model.Subject = solution.Subject;
                    model.Content = solution.ContentText;
                    model.Keyword = string.IsNullOrWhiteSpace(solution.Keyword) ? "" : solution.Keyword.Replace("\"", "");
                    model.CategoryID = solution.CategoryID;
                    model.Category = _categoryService.GetCategoryById(solution.CategoryID).Name;
                    model.Path = solution.Path;
                    // Get SolutionAttachment By SolutionID
                    IEnumerable<SolutionAttachment> attachments = _solutionAttachmentService.GetSolutionAttachmentBySolutionID(solution.ID);
                    if (attachments != null)
                    {
                        model.SolutionAttachmentsList = new List<AttachmentViewModel>();
                        foreach (var attachment in attachments)
                        {
                            var att = new AttachmentViewModel();
                            att.id = attachment.ID;
                            att.name = TMSUtils.GetMinimizedAttachmentName(attachment.Filename);

                            model.SolutionAttachmentsList.Add(att);
                        }
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
            else
            {
                return HttpNotFound();
            }
        }


        // [Utils.Authorize(Roles = "Helpdesk")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(KnowledgeBaseViewModel model)
        {
            // Validate subject is null or empty.
            if (string.IsNullOrWhiteSpace(model.Subject))
            {
                ModelState.AddModelError("Subject", "Please input subject.");
            }
            // Subject != null or "".
            else
            {
                var subject = model.Subject.Trim();
                // Check duplicate subject.
                bool isDuplicateSubject = _solutionServices.IsDuplicateSubject(null, subject);
                // Subject is duplicate.
                if (isDuplicateSubject)
                {
                    ModelState.AddModelError("Subject", string.Format("'{0}' have been used!", subject));
                }
            }
            // Check path is null or spacebar
            if (string.IsNullOrWhiteSpace(model.Path))
            {
                ModelState.AddModelError("Path", "Please input path.");
            }
            // path != null
            else
            {
                var path = model.Path.Trim();
                // Check duplicate path.
                bool isDuplicatePath = _solutionServices.IsduplicatePath(null, path);
                // Path is duplicate. 
                if (isDuplicatePath)
                {
                    ModelState.AddModelError("Path", string.Format("'{0}' have been used!", path));
                }
                // Path can not allow "-" in begin or end string.
                if (path.StartsWith("-") || path.EndsWith("-"))
                {
                    ModelState.AddModelError("Path", "Invalid path! (example: how-to-use-tms)");
                }
                // Path follow format a-z, 0-9 and separated by "-".
                Match pathFormat = Regex.Match(model.Path.Trim(), "^[a-z0-9-]*$", RegexOptions.IgnoreCase);
                // False format path
                if (!pathFormat.Success)
                {
                    ModelState.AddModelError("Path", "Path can not contain special characters and spaces! ");
                }
            }
            // Keyword != null
            if (model.Keyword != null)
            {
                //  Keyword follow format a-z, 0-9 and separated by comas (",").
                Match keywordIsMatchFormat = Regex.Match(model.Keyword.Trim(), "^[a-z0-9-, ]*$", RegexOptions.IgnoreCase);
                // Keyword is not match format.
                if (!keywordIsMatchFormat.Success)
                {
                    ModelState.AddModelError("Keyword", "Keyword only contain characters 'a-z', '0-9' and separated by commas! ");
                }
            }
            // CategoryID == 0 (default) User is not choose category.
            if (model.CategoryID == 0)
            {
                ModelState.AddModelError("CategoryID", "Please select topic! ");
            }
            // CategoryID != 0  user have selected category.
            if (model.CategoryID != 0)
            {
                Category category = _categoryService.GetCategoryById(model.CategoryID);
                // category != null
                if (category != null)
                {
                    model.Category = category.Name;
                }
            }
            // ModelState is valid.
            if (ModelState.IsValid)
            {
                Solution solution = new Solution();
                solution.Subject = model.Subject.Trim();
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

                // Keyword is null or whitespace
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
                solution.ModifiedTime = DateTime.Now;
                // Create Solution
                bool createSolutionResult = _solutionServices.AddSolution(solution);
                // Create Solution success.
                if (createSolutionResult)
                {
                    Response.Cookies.Add(new HttpCookie("FlashMessage", "Create solution successfully!") { Path = "/" });
                    Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "success") { Path = "/" });
                    return RedirectToAction("Index");
                }
                // Create Solution fail.
                else
                {
                    Response.Cookies.Add(new HttpCookie("FlashMessage", "Create solution unsuccessfully!") { Path = "/" });
                    Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "error") { Path = "/" });
                    return RedirectToAction("Index");
                }
            }
            return View(model);
        }

        //   [CustomAuthorize(Roles = "Helpdesk")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int? id, KnowledgeBaseViewModel model)
        {
            if (id.HasValue)
            {
                // Validate subject is null or empty.
                if (string.IsNullOrWhiteSpace(model.Subject))
                {
                    ModelState.AddModelError("Subject", "Please input subject.");
                }
                // Subject != null or "".
                else
                {
                    var subject = model.Subject.Trim();
                    // Check duplicate subject.
                    bool isDuplicateSubject = _solutionServices.IsDuplicateSubject(id.Value, subject);
                    //   // Subject is duplicate.
                    if (isDuplicateSubject)
                    {
                        ModelState.AddModelError("Subject", string.Format("'{0}' have been used!", subject));
                    }
                }
                // Check path is null or spacebar.
                if (string.IsNullOrWhiteSpace(model.Path))
                {
                    ModelState.AddModelError("Path", "Please input path.");
                }
                // path != null
                else
                {
                    var path = model.Path.Trim();
                    // check duplicate path.
                    bool isDuplicatePath = _solutionServices.IsduplicatePath(id.Value, path);
                    // path is duplicate.
                    if (isDuplicatePath)
                    {
                        ModelState.AddModelError("Path", string.Format("'{0}' have been used!", path));
                    }
                    // Path can not allow "-" in begin or end string.
                    if (path.StartsWith("-") || path.EndsWith("-"))
                    {
                        ModelState.AddModelError("Path", "Invalid path! (example: how-to-use-tms)");
                    }
                    // Path follow format a-z, 0-9 and separated by "-".
                    Match pathFormat = Regex.Match(model.Path.Trim(), "^[a-z0-9-]*$", RegexOptions.IgnoreCase);
                    // False format path.
                    if (!pathFormat.Success)
                    {
                        ModelState.AddModelError("Path", "Path can not contain special characters and spaces! ");
                    }
                }
                // Keyword != null.
                if (model.Keyword != null)
                {
                    // Keyword follow format a-z, 0-9 and separated by comas (",").
                    Match keywordIsMatchFormat = Regex.Match(model.Keyword.Trim(), "^[a-z0-9-, ]*$", RegexOptions.IgnoreCase);
                    // Keyword is not match format.
                    if (!keywordIsMatchFormat.Success)
                    {
                        ModelState.AddModelError("Keyword", "Keyword only contain characters 'a-z', '0-9' and separated by commas! ");
                    }
                }
                // CategoryID == 0 (default) User is not choose category
                if (model.CategoryID != 0)
                {
                    Category category = _categoryService.GetCategoryById(model.CategoryID);
                    // category != null
                    if (category != null)
                    {
                        model.Category = category.Name;
                    }
                }
                // Get solution by id.
                Solution solution = _solutionServices.GetSolutionById(id.Value);
                // Solution != null.
                if (solution != null)
                {
                    // ModelState is valid.
                    if (ModelState.IsValid)
                    {
                        solution.Subject = model.Subject.Trim();
                        solution.ContentText = model.Content;
                        // Keyword is null or whitespace
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
                        // Get Solution Attachment By SolutionID
                        List<SolutionAttachment> attachments = _solutionAttachmentService.GetSolutionAttachmentBySolutionID(solution.ID).ToList();
                        bool isDelete;
                        for (int i = 0; i < attachments.Count(); i++)
                        {
                            isDelete = true;
                            if (model.SolutionAttachmentsList != null && model.SolutionAttachmentsList.Count > 0)
                            {
                                for (int j = 0; j < model.SolutionAttachmentsList.Count; j++)
                                {
                                    if (attachments[i].ID == model.SolutionAttachmentsList[j].id)
                                    {
                                        isDelete = false;
                                    }
                                }
                            }
                            if (isDelete) _solutionAttachmentService.DeleteAttachment(attachments[i]);
                        }
                        // Edit solution.
                        bool editResult = _solutionServices.EditSolution(solution);
                        // Eit success.
                        if (editResult)
                        {
                            Response.Cookies.Add(new HttpCookie("FlashMessage", "Update solution successfully!") { Path = "/" });
                            Response.Cookies.Add(new HttpCookie("FlashMessageStatus", "success") { Path = "/" });
                            return RedirectToAction("Index");
                        }
                        // Edit fail.
                        else
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

        ////   [CustomAuthorize(Roles = "Helpdesk")]
        //[HttpPost]
        //public ActionResult Delete(int[] selectedSolutions)
        //{
        //    if (selectedSolutions == null || selectedSolutions.Count() == 0)
        //    {
        //        return Json(new
        //        {
        //            success = false,
        //            message = "Please choose at least 1 solution to delete!"
        //        });
        //    }
        //    else
        //    {
        //        foreach (var solutionId in selectedSolutions)
        //        {
        //            Solution solution = _solutionServices.GetSolutionById(solutionId);
        //            if (solution == null)
        //            {
        //                return Json(new
        //                {
        //                    success = false,
        //                    message = "Delete solution unsuccessfully!"
        //                });
        //            }
        //            else
        //            {
        //                bool resultDelete = _solutionServices.DeleteSolution(solution);
        //                if (!resultDelete)
        //                {
        //                    return Json(new
        //                    {
        //                        success = false,
        //                        message = ConstantUtil.CommonError.DBExceptionError
        //                    });
        //                }
        //            }
        //        }
        //        return Json(new
        //        {
        //            success = true,
        //            message = "Delete solution successfully!"
        //        });
        //    }
        //}

        //   [CustomAuthorize(Roles = "Helpdesk")]

        [HttpPost]
        public ActionResult Delete(int[] selectedSolutions)
        {
            if (selectedSolutions == null || selectedSolutions.Count() == 0)
            {
                return Json(new
                {
                    success = false,
                    message = "Please choose at least 1 solution to delete!"
                });
            }
            else
            {
                bool resultDelete = _solutionServices.DeleteSolution(selectedSolutions);
                if (!resultDelete)
                {
                    return Json(new
                    {
                        success = false,
                        message = ConstantUtil.CommonError.DBExceptionError
                    });
                }
                return Json(new
                {
                    success = true,
                    message = "Delete solution successfully!"
                });
            }
        }

        [HttpGet]
        public ActionResult GetSolutionsByCategory(int? id, string key_search)
        {
            string keywords = key_search;
            IEnumerable<Solution> solutions = _solutionServices.GetAllSolutions();
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

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string id = User.Identity.GetUserId();
            AspNetUser user = _userService.GetUserById(id);
            if (user != null)
            {
                ViewBag.LayoutName = user.Fullname;
                ViewBag.LayoutAvatarURL = user.AvatarURL;
                ViewBag.LayoutRole = user.AspNetRoles.FirstOrDefault().Name;
            }
            base.OnActionExecuting(filterContext);
        }
    }
}