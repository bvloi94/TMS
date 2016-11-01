using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using log4net;
using TMS.DAL;
using TMS.Models;
using TMS.Schedulers;
using TMS.Utils;


namespace TMS.Services
{
    public class TicketAttachmentService
    {

        private ILog log = LogManager.GetLogger(typeof(JobManager));

        private readonly UnitOfWork _unitOfWork;

        public TicketAttachmentService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<TicketAttachment> GetAll()
        {
            return _unitOfWork.TicketAttachmentRepository.Get();
        }

        public TicketAttachment GetTicketAttachmentByID(int id)
        {
            return _unitOfWork.TicketAttachmentRepository.GetByID(id);
        }

        public void DeleteAttachment(TicketAttachment attachment)
        {

            _unitOfWork.TicketAttachmentRepository.Delete(attachment);
            _unitOfWork.Commit();
            try
            {
                File.Delete(Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory + attachment.Path));
            }
            catch (Exception ex)
            {
                log.Error("Delete Attachment Failed", ex);
            }
        }

        public IEnumerable<TicketAttachment> GetAttachmentByTicketID(int id)
        {
            return _unitOfWork.TicketAttachmentRepository.Get(m => m.TicketID == id);
        }

        public void saveFile(int id, IEnumerable<HttpPostedFileBase> uploadFiles, bool type)
        {
            try
            {
                string containFolder = "Attachments";
                TicketAttachment files = null;
                List<HttpPostedFileBase> upFiles = uploadFiles.ToList();
                FileUploader _fileUploadService = new FileUploader();
                for (int i = 0; i < upFiles.Count; i++)
                {
                    string filePath = _fileUploadService.UploadFile(upFiles[i], containFolder);
                    files = new TicketAttachment();
                    files.TicketID = id;
                    files.Path = filePath;
                    files.Filename = upFiles[i].FileName;
                    files.Type = type;
                    _unitOfWork.TicketAttachmentRepository.Insert(files);
                    _unitOfWork.Commit();
                }
            }
            catch (Exception ex)
            {
                log.Error("Save Attachment Failed", ex);
            }

        }

    }
}