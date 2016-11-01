using System;
using System.Collections.Generic;
using TMS.DAL;
using TMS.Models;

namespace TMS.Services
{
    public class SolutionService
    {
        private UnitOfWork _unitOfWork;

        public SolutionService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<Solution> GetAllSolutions()
        {
            return _unitOfWork.SolutionRepository.Get();
        }

        public void AddSolution(Solution solution)
        {
            try

            {
                _unitOfWork.SolutionRepository.Insert(solution);
                _unitOfWork.Commit();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IEnumerable<Solution> GetSolutionsByCategory(int id)
        {
            return _unitOfWork.SolutionRepository.Get(m => m.CategoryID == id);
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