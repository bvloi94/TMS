﻿using Microsoft.AspNet.Identity;
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
    [CustomAuthorize(Roles = "Helpdesk,Technician")]
    public class KnowledgeBaseController : Controller
    {
        private UnitOfWork _unitOfWork = new UnitOfWork();
        private TicketService _ticketService;
        private UserService _userService;
        private SolutionService _solutionService;
        private FileUploader _fileUploader;
        private CategoryService _categoryService;
        private SolutionAttachmentService _solutionAttachmentService;
        private TicketAttachmentService _ticketAttachmentService;
        private KeywordService _keywordService;

        public KnowledgeBaseController()
        {
            _ticketService = new TicketService(_unitOfWork);
            _userService = new UserService(_unitOfWork);
            _solutionService = new SolutionService(_unitOfWork);
            _fileUploader = new FileUploader();
            _categoryService = new CategoryService(_unitOfWork);
            _solutionAttachmentService = new SolutionAttachmentService(_unitOfWork);
            _ticketAttachmentService = new TicketAttachmentService(_unitOfWork);
            _keywordService = new KeywordService(_unitOfWork);
        }

        // GET: KB
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [ActionName("Create")]
        public ActionResult CreateGet(int? id)
        {
            KnowledgeBaseViewModel model = new KnowledgeBaseViewModel();
            if (id.HasValue)
            {
                Ticket ticket = _ticketService.GetTicketByID(id.Value);
                if (ticket != null)
                {
                    model.Subject = ticket.Subject;
                    model.Keywords = _keywordService.GetTicketKeywordForDisplay(ticket.ID);
                    model.Content = string.IsNullOrEmpty(ticket.Solution) ? string.Empty : ticket.Solution.Replace(Environment.NewLine, "<br />");
                    model.CategoryID = ticket.CategoryID.HasValue ? ticket.CategoryID.Value : 0;
                    if (ticket.Category != null)
                    {
                        model.Category = ticket.Category.Name;
                    }
                }
            }
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
                Solution solution = _solutionService.GetSolutionById(id.Value);
                // solution != null.
                if (solution != null)
                {
                    KnowledgeBaseViewModel model = new KnowledgeBaseViewModel();
                    model.Subject = solution.Subject;
                    model.Content = solution.ContentText;
                    model.Keywords = _keywordService.GetSolutionKeywordForDisplay(solution.ID);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(KnowledgeBaseViewModel model)
        {
            // Validate subject is null or empty.
            if (!string.IsNullOrWhiteSpace(model.Subject))
            {
                model.Subject = model.Subject.Trim();
                // Check duplicate subject.
                bool isDuplicatedSubject = _solutionService.IsDuplicatedSubject(null, model.Subject);
                // Subject is duplicate.
                if (isDuplicatedSubject)
                {
                    ModelState.AddModelError("Subject", string.Format("'{0}' have been used!", model.Subject));
                }
            }
            // Check path is null or spacebar
            if (!string.IsNullOrWhiteSpace(model.Path))
            {
                model.Path = model.Path.Trim();
                // Check duplicate path.
                bool isDuplicatedPath = _solutionService.IsDuplicatedPath(null, model.Path);
                // Path is duplicate. 
                if (isDuplicatedPath)
                {
                    ModelState.AddModelError("Path", string.Format("'{0}' have been used!", model.Path));
                }
            }
            // CategoryID != 0  user have selected category.
            Category category = _categoryService.GetCategoryById(model.CategoryID);
            // category != null
            if (category != null)
            {
                model.Category = category.Name;
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
                solution.SolutionKeywords = _keywordService.GetSolutionKeywordsForCreate(model.Keywords);
                solution.Path = model.Path.Trim().ToLower();
                solution.CreatedTime = DateTime.Now;
                solution.ModifiedTime = DateTime.Now;
                // Create Solution
                bool createSolutionResult = _solutionService.AddSolution(solution);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int? id, KnowledgeBaseViewModel model)
        {
            if (id.HasValue)
            {
                // Validate subject is null or empty.
                if (!string.IsNullOrWhiteSpace(model.Subject))
                {
                    model.Subject = model.Subject.Trim();
                    // Check duplicate subject.
                    bool isDuplicatedSubject = _solutionService.IsDuplicatedSubject(id.Value, model.Subject);
                    //   // Subject is duplicate.
                    if (isDuplicatedSubject)
                    {
                        ModelState.AddModelError("Subject", string.Format("'{0}' have been used!", model.Subject));
                    }
                }
                // Check path is null or spacebar.
                if (!string.IsNullOrWhiteSpace(model.Path))
                {
                    model.Path = model.Path.Trim();
                    // check duplicate path.
                    bool isDuplicatedPath = _solutionService.IsDuplicatedPath(id.Value, model.Path);
                    // path is duplicate.
                    if (isDuplicatedPath)
                    {
                        ModelState.AddModelError("Path", string.Format("'{0}' have been used!", model.Path));
                    }
                }
                Category category = _categoryService.GetCategoryById(model.CategoryID);
                // category != null
                if (category != null)
                {
                    model.Category = category.Name;
                }
                // Get solution by id.
                Solution solution = _solutionService.GetSolutionById(id.Value);
                // Solution != null.
                if (solution != null)
                {
                    // ModelState is valid.
                    if (ModelState.IsValid)
                    {
                        solution.Subject = model.Subject.Trim();
                        solution.ContentText = model.Content;
                        solution.SolutionKeywords = _keywordService.GetSolutionKeywordsForEdit(model.Keywords, solution.ID);
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
                        bool editResult = _solutionService.EditSolution(solution);
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
                bool resultDelete = _solutionService.DeleteSolution(selectedSolutions);
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
            IEnumerable<Solution> solutions = _solutionService.GetAllSolutions();
            IEnumerable<KnowledgeBaseViewModel> filteredListItems = solutions.Select(m => new KnowledgeBaseViewModel
            {
                ID = m.ID,
                Subject = m.Subject,
                Category = m.Category.Name,
                CategoryID = m.Category.ID,
                CategoryPath = _categoryService.GetCategoryPath(m.Category),
                Content = m.ContentText,
                Keywords = _keywordService.GetSolutionKeywordForDisplay(m.ID),
                Path = m.Path,
                CreatedTimeString = m.CreatedTime.ToString(ConstantUtil.DateTimeFormat2),
                ModifiedTimeString = m.ModifiedTime.ToString(ConstantUtil.DateTimeFormat2),
                IsPublish = m.IsPublish
            });

            if (!string.IsNullOrWhiteSpace(key_search))
            {
                filteredListItems = filteredListItems.Where(p => p.Subject.ToLower().Contains(key_search.ToLower()));
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
        public ActionResult GetFrequentlyAskedTickets(JqueryDatatableParameterViewModel param, int? timeOption, int? typeOption)
        {
            if (!timeOption.HasValue)
            {
                timeOption = ConstantUtil.TimeOption.ThisWeek;
            }
            if (!typeOption.HasValue)
            {
                typeOption = 0;
            }
            IEnumerable<Ticket> recentTickets = _ticketService.GetRecentTickets(timeOption.Value);
            IEnumerable<Ticket> typeTickets;
            switch (typeOption)
            {
                case ConstantUtil.TicketType.Request:
                    typeTickets = recentTickets.Where(m => m.Type == ConstantUtil.TicketType.Request);
                    break;
                case ConstantUtil.TicketType.Problem:
                    typeTickets = recentTickets.Where(m => m.Type == ConstantUtil.TicketType.Problem);
                    break;
                case 0:
                default:
                    typeTickets = recentTickets;
                    break;
            }
            ICollection<FrequentlyAskedTicketViewModel> frequentlyAskedTickets = _ticketService.GetFrequentlyAskedTickets(typeTickets);

            IEnumerable<FrequentlyAskedTicketViewModel> filteredListItems = frequentlyAskedTickets.Take(10);

            // Sort.
            var sortColumnIndex = Convert.ToInt32(param.order[0]["column"]);
            var sortDirection = param.order[0]["dir"];

            switch (sortColumnIndex)
            {
                case 0:
                    filteredListItems = sortDirection == "asc"
                        ? filteredListItems.OrderBy(p => p.Ticket.Subject)
                        : filteredListItems.OrderByDescending(p => p.Ticket.Subject);
                    break;
                case 1:
                    filteredListItems = sortDirection == "asc"
                        ? filteredListItems.OrderBy(p => p.Frequency)
                        : filteredListItems.OrderByDescending(p => p.Frequency);
                    break;
            }

            var displayedList = filteredListItems.Skip(param.start).Take(param.length).Select(m => new FrequentlyAskedTicketViewModel
            {
                Ticket = new Ticket
                {
                    ID = m.Ticket.ID,
                    Subject = m.Ticket.Subject
                },
                Frequency = m.Frequency
            });

            return Json(new
            {
                draw = param.draw,
                recordsTotal = displayedList.ToList().Count(),
                recordsFiltered = filteredListItems.Count(),
                data = displayedList
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetTicketsByTags(JqueryDatatableParameterViewModel param, int? timeOption, string tags)
        {
            if (!timeOption.HasValue)
            {
                timeOption = ConstantUtil.TimeOption.ThisWeek;
            }
            IEnumerable<Ticket> recentTickets = _ticketService.GetRecentTickets(timeOption.Value);

            IEnumerable<TicketViewModel> temp = recentTickets.Select(m => new TicketViewModel
            {
                Id = m.ID,
                Subject = m.Subject,
                //Tags = m.Tags == null ? "" : m.Tags,
                CategoryId = m.CategoryID.HasValue ? m.CategoryID.Value : 0
            }).AsQueryable();

            List<TicketViewModel> filteredListItems = new List<TicketViewModel>();

            if (!string.IsNullOrEmpty(tags))
            {
                //tags = GeneralUtil.ConvertToFormatKeyword(tags);
                //string[] tagsArr = tags.Split(',');
                //int numOfTags = tagsArr.Count();
                //int categoryId = 0;
                //foreach (TicketViewModel ticket in temp)
                //{
                //    int matchTag = 0;
                //    if (categoryId == 0 || ticket.CategoryId == categoryId)
                //    {
                //        foreach (string tagItem in tagsArr)
                //        {
                //            if (ticket.Tags != null && ticket.Tags.ToLower().Contains(tagItem.ToLower()))
                //            {
                //                matchTag++;
                //            }
                //        }
                //    }
                //    if (numOfTags <= 3)
                //    {
                //        if (matchTag == numOfTags)
                //        {
                //            if (!filteredListItems.Any())
                //            {
                //                categoryId = ticket.CategoryId;
                //            }
                //            filteredListItems.Add(ticket);
                //        }
                //    }
                //    else if (3 < numOfTags && numOfTags <= 5)
                //    {
                //        if (matchTag >= numOfTags - 1 && matchTag <= numOfTags + 1)
                //        {
                //            if (!filteredListItems.Any())
                //            {
                //                categoryId = ticket.CategoryId;
                //            }
                //            filteredListItems.Add(ticket);
                //        }
                //    }
                //    else
                //    {
                //        if (matchTag >= numOfTags - 2 && matchTag <= numOfTags + 2)
                //        {
                //            if (!filteredListItems.Any())
                //            {
                //                categoryId = ticket.CategoryId;
                //            }
                //            filteredListItems.Add(ticket);
                //        }
                //    }
                //}
            }
            // Sort.
            var sortColumnIndex = Convert.ToInt32(param.order[0]["column"]);
            var sortDirection = param.order[0]["dir"];

            switch (sortColumnIndex)
            {
                case 0:
                    filteredListItems = sortDirection == "asc"
                        ? filteredListItems.OrderBy(p => p.Subject).ToList()
                        : filteredListItems.OrderByDescending(p => p.Subject).ToList();
                    break;
            }

            var displayedList = filteredListItems.Skip(param.start).Take(param.length);

            return Json(new
            {
                draw = param.draw,
                recordsTotal = displayedList.Count(),
                recordsFiltered = filteredListItems.Count(),
                data = displayedList,
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

        [HttpPost]
        public ActionResult Toggle(int? id)
        {
            if (id.HasValue)
            {
                Solution solution = _solutionService.GetSolutionById(id.Value);
                if (solution != null)
                {
                    bool current = solution.IsPublish;
                    solution.IsPublish = !current;
                    bool updateResult = _solutionService.EditSolution(solution);
                    if (updateResult)
                    {
                        if (current)
                        {
                            return Json(new {
                                success = true,
                                message = "Solution has been set to private!"
                            });
                        }
                        else
                        {
                            return Json(new
                            {
                                success = true,
                                message = "Published solution successfully!"
                            });
                        }
                    }
                    else
                    {
                        return Json(new
                        {
                            success = false,
                            message = ConstantUtil.CommonError.DBExceptionError
                        });
                    }
                }
            }
            return Json(new
            {
                success = false,
                message = "Unavailable Solution!"
            });
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