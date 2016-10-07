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
                return _unitOfWork.ImpactRepository.Get(p => p.Name == name).Any();
            }
            else
            {
                return _unitOfWork.ImpactRepository.Get(p => p.ID != id && p.Name == name).Any();
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

        public Impact GetImpactByID(int id)
        {
            return _unitOfWork.ImpactRepository.GetByID(id);
        }

        internal void UpdateImpact(Impact impact)
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
    }
}