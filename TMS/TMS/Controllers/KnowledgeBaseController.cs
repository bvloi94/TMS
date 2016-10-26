using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.IO;
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

        public KnowledgeBaseController()
        {
            _ticketService = new TicketService(_unitOfWork);
            _userService = new UserService(_unitOfWork);
            _solutionServices = new SolutionService(_unitOfWork);
            _fileUploader = new FileUploader();
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
                    model.CategoryID = solution.CategoryID.Value;

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
            bool isDuplicateSubject = _solutionServices.IsDuplicateSubject(null, (model.Subject == null ? null : model.Subject.Trim()));
            if (isDuplicateSubject)
            {
                ModelState.AddModelError("Subject", string.Format("'{0}' have been used!", model.Subject));
            }
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

                Solution solution = _solutionServices.GetSolutionById(id.Value);

                if (solution != null)
                {
                    if (ModelState.IsValid)
                    {

                        solution.Subject = model.Subject;
                        solution.ContentText = model.Content;
                        solution.CategoryID = model.CategoryID;

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
    }
}