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

        [HttpGet]
        public ActionResult Detail()
        {
            return View();
        }
    }
}