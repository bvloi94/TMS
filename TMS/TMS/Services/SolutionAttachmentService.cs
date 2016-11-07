using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using TMS.DAL;
using TMS.Models;
using TMS.Schedulers;
using TMS.Utils;

namespace TMS.Controllers
{
    public class SolutionAttachmentService
    {
        private ILog log = LogManager.GetLogger(typeof(JobManager));
        private readonly UnitOfWork _unitOfWork;

        public SolutionAttachmentService(UnitOfWork unitofwork)
        {
            _unitOfWork = unitofwork;
        }

        public IEnumerable<SolutionAttachment> GetSolutionAttachmentBySolutionID(int id)
        {
            return _unitOfWork.SolutionAttachmentRepository.Get(m => m.SolutionID == id);
        }

        public bool DeleteAttachment(SolutionAttachment solutionAttachment)
        {

            try
            {
                File.Delete(Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory + solutionAttachment.Path));
            }
            catch (Exception ex)
            {
                log.Error("Delete Attachment Failed", ex);
            }
            _unitOfWork.SolutionAttachmentRepository.Delete(solutionAttachment);
            return _unitOfWork.Commit();
        }
    }
}