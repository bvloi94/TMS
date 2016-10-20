using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TMS.DAL;
using TMS.Models;
using TMS.Utils;

namespace TMS.Services
{
    public class PriorityMatrixService
    {
        private readonly UnitOfWork _unitOfWork;

        public PriorityMatrixService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public PriorityMatrixItem GetPriorityMatrixItemByImpactAndUrgency(int impactId, int urgencyId)
        {
            return _unitOfWork.PriorityMatrixItemRepository.Get(m => m.ImpactID == impactId && m.UrgencyID == urgencyId).FirstOrDefault();
        }
    }
}