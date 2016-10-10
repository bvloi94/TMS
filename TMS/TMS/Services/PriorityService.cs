using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TMS.DAL;
using TMS.Models;

namespace TMS.Services
{
    public class PriorityService
    {
        private UnitOfWork _unitOfWork;

        public PriorityService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<Priority> GetAll()
        {
            return _unitOfWork.PriorityRepository.Get();
        }

        public bool IsDuplicateName(Int32? id, string name)
        {
            if (id == null)
            {
                return _unitOfWork.PriorityRepository.Get(p => p.Name == name).Any();
            }
            else
            {
                return _unitOfWork.PriorityRepository.Get(p => p.ID != id && p.Name == name).Any();
            }
        }

        public void AddPriority(Priority priority)
        {
            try
            {
                _unitOfWork.PriorityRepository.Insert(priority);
                _unitOfWork.Save();
            }
            catch (Exception)
            {
                
                throw;
            }
           
        }

        public Priority GetPriorityByID(int id)
        {
            return _unitOfWork.PriorityRepository.GetByID(id);
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

        public void UpdatePriority(Priority priority)
        {
            try
            {
                _unitOfWork.PriorityRepository.Update(priority);
                _unitOfWork.Save();
            }
            catch (Exception)
            {
                
                throw;
            }
        }
    }
}