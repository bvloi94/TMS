using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TMS.DAL;
using TMS.Models;

namespace TMS.Services
{
    public class ImpactService
    {
        private readonly UnitOfWork _unitOfWork;

        public ImpactService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<Impact> GetAll()
        {
            return _unitOfWork.ImpactRepository.Get();
        }

        public bool IsDuplicatedName(int? id, string name)
        {
            if (id == null)
            {
                return _unitOfWork.ImpactRepository.Get(p => p.Name.ToLower().Equals(name.ToLower())).Any();
            }
            else
            {
                return _unitOfWork.ImpactRepository.Get(p => p.ID != id && p.Name.ToLower().Equals(name.ToLower())).Any();
            }
        }

        public bool AddImpact(Impact impact)
        {
            _unitOfWork.ImpactRepository.Insert(impact);
            return _unitOfWork.Commit();
        }

        public Impact GetImpactById(int id)
        {
            return _unitOfWork.ImpactRepository.GetByID(id);
        }

        public bool UpdateImpact(Impact impact)
        {
            _unitOfWork.ImpactRepository.Update(impact);
            return _unitOfWork.Commit();
        }

        public bool DeleteImpact(Impact impact)
        {
            _unitOfWork.BeginTransaction();
            foreach (PriorityMatrixItem priorityMatrixItem in impact.PriorityMatrixItems.ToList())
            {
                _unitOfWork.PriorityMatrixItemRepository.Delete(priorityMatrixItem);
            }
            _unitOfWork.ImpactRepository.Delete(impact);
            return _unitOfWork.CommitTransaction();
        }

        public bool IsInUse(Impact impact)
        {
            return _unitOfWork.TicketRepository.Get(m => m.ImpactID == impact.ID).Any();
        }
    }
}