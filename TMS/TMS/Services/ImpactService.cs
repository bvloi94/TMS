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

        public void AddImpact(Impact impact)
        {
            try
            {
                _unitOfWork.ImpactRepository.Insert(impact);
                _unitOfWork.Save();
            }
            catch
            {
                throw;
            }
        }

        public Impact GetImpactById(int id)
        {
            return _unitOfWork.ImpactRepository.GetByID(id);
        }

        public void UpdateImpact(Impact impact)
        {
            try
            {
                _unitOfWork.ImpactRepository.Update(impact);
                _unitOfWork.Save();
            }
            catch (Exception)
            {

                throw;
            }

        }

        public void DeleteImpact(Impact impact)
        {
            try
            {
                foreach (PriorityMatrixItem priorityMatrixItem in impact.PriorityMatrixItems.ToList())
                {
                    _unitOfWork.PriorityMatrixItemRepository.Delete(priorityMatrixItem);
                }
                _unitOfWork.ImpactRepository.Delete(impact);
                _unitOfWork.Save();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool IsInUse(Impact impact)
        {
            return _unitOfWork.TicketRepository.Get(m => m.ImpactID == impact.ID).Any();
        }
    }
}