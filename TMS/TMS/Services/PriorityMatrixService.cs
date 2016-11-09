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
            return
                _unitOfWork.PriorityMatrixItemRepository.Get(m => m.ImpactID == impactId && m.UrgencyID == urgencyId)
                    .FirstOrDefault();
        }

        public IEnumerable<PriorityMatrixItem> GetPriorityMatrixItems()
        {
            return _unitOfWork.PriorityMatrixItemRepository.Get();
        }

        public bool ChangePriorityMatrixItem(int impactID, int urgencyID, int? priorityID)
        {
            PriorityMatrixItem entity = _unitOfWork.PriorityMatrixItemRepository.Get(m => m.ImpactID == impactID && m.UrgencyID == urgencyID).FirstOrDefault();
            if (entity != null)
            {
                entity.PriorityID = priorityID;
                _unitOfWork.PriorityMatrixItemRepository.Update(entity);
            }
            else
            {
                entity = new PriorityMatrixItem
                {
                    ImpactID = impactID,
                    UrgencyID = urgencyID,
                    PriorityID = priorityID
                };
                _unitOfWork.PriorityMatrixItemRepository.Insert(entity);
            }
            return _unitOfWork.Commit();
        }
    }
}