﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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

        public bool AddSolution(Solution solution)
        {
            _unitOfWork.SolutionRepository.Insert(solution);
            return _unitOfWork.Commit();
        }
        /// <summary>
        /// IsDuplicateSubject
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="subject">subject</param>
        /// <returns>True | False</returns>
        public bool IsDuplicatedSubject(int? id, string subject)
        {
            // id == null
            if (id == null)
            {
                return _unitOfWork.SolutionRepository.Get(m => m.Subject.ToLower().Equals(subject.ToLower())).Any();
            }
            // id != null
            else
            {
                return _unitOfWork.SolutionRepository.Get(m => m.ID != id && m.Subject.ToLower().Equals(subject.ToLower())).Any();
            }

        }

        public Solution GetSolutionById(int? id)
        {
            if (id.HasValue)
            {
                return _unitOfWork.SolutionRepository.GetByID(id);
            }
            else
            {
                return null;
            }
        }

        public bool EditSolution(Solution solution)
        {
            _unitOfWork.BeginTransaction();
            _unitOfWork.SolutionKeywordRepository.Delete(m => m.SolutionID == solution.ID);
            if (solution.SolutionKeywords != null)
            {
                foreach (SolutionKeyword solutionKeyword in solution.SolutionKeywords)
                {
                    _unitOfWork.SolutionKeywordRepository.Insert(solutionKeyword);
                }
            }
            _unitOfWork.SolutionRepository.Update(solution);
            return _unitOfWork.CommitTransaction();
        }

        public bool ToggleSolution(Solution solution)
        {
            _unitOfWork.SolutionRepository.Update(solution);
            return _unitOfWork.Commit();
        }

        public IEnumerable<Solution> GetSolutionsByCategory(int id)
        {
            return _unitOfWork.SolutionRepository.Get(m => m.CategoryID == id);
        }
        /// <summary>
        /// IsduplicatePath
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="path">path</param>
        /// <returns>True | False</returns>
        public bool IsDuplicatedPath(int? id, string path)
        {
            // id == null
            if (id == null)
            {
                return _unitOfWork.SolutionRepository.Get(m => m.Path.ToLower().Equals(path.ToLower())).Any();
            }
            // id != null
            else
            {
                return _unitOfWork.SolutionRepository.Get(m => m.ID != id && m.Path.ToLower().Equals(path.ToLower())).Any();
            }

        }

        public IEnumerable<Solution> SearchSolutions(string keywords)
        {
            if (!string.IsNullOrWhiteSpace(keywords))
            {
                string[] keywordArr = keywords.Split(',');
                return _unitOfWork.SolutionRepository.Get(s => keywordArr.Any(k => s.Subject.ToLower().Contains(k.ToLower())));
            }
            return GetAllSolutions();
        }

        public Solution GetSolutionById(int id)
        {
            return _unitOfWork.SolutionRepository.GetByID(id);
        }

        public Solution GetSolutionByPath(string path)
        {
            return _unitOfWork.SolutionRepository.Get(m => m.Path.Equals(path)).FirstOrDefault();
        }
        /// <summary>
        /// Delete Solution
        /// </summary>
        /// <param name="solution">solution</param>
        /// <returns>True | False</returns>
        public bool DeleteSolution(int[] selectedSolutions)
        {
            _unitOfWork.BeginTransaction();

            var solutions = _unitOfWork.SolutionRepository.Get(m => selectedSolutions.Contains(m.ID));
            foreach (var solution in solutions.ToList())
            {
                _unitOfWork.SolutionAttachmentRepository.Delete(m => m.SolutionID == solution.ID);
                _unitOfWork.SolutionKeywordRepository.Delete(m => m.SolutionID == solution.ID);
                _unitOfWork.SolutionRepository.Delete(solution.ID);
            }
            return _unitOfWork.CommitTransaction();
        }

        public IEnumerable<Solution> GetSolutionsByTag(string tag)
        {
            if (tag != null)
            {
                tag = tag.ToLower();
            }
            return _unitOfWork.SolutionRepository.Get(m => m.SolutionKeywords
                .Any(n => n.Keyword.Name.Equals(tag)));
        }
    }
}