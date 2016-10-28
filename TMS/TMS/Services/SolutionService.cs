using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TMS.DAL;
using TMS.Models;

namespace TMS.Services
{
    public class SolutionService
    {
        private readonly UnitOfWork _unitOfWork;

        public SolutionService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<Solution> GetAll()
        {
            return _unitOfWork.SolutionRepository.Get();
        }

        public IEnumerable<Solution> SearchSolutions(string searchtxt)
        {
            return _unitOfWork.SolutionRepository.Get(s => s.Subject.ToLower().Contains(searchtxt.ToLower()));
        }

        public Solution GetSolutionById(int id)
        {
            return _unitOfWork.SolutionRepository.GetByID(id);
        }
    }
}