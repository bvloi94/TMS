using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TMS.DAL;
using TMS.Models;
using TMS.Utils;

namespace TMS.Services
{
    public class CategoryService
    {
        private readonly UnitOfWork _unitOfWork;

        public CategoryService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<Category> GetCategories()
        {
            return _unitOfWork.CategoryRepository.Get(m => m.CategoryLevel == ConstantUtil.CategoryLevel.Category);
        }

        public IEnumerable<Category> GetSubCategories(int categoryId)
        {
            return _unitOfWork.CategoryRepository.Get(m => m.CategoryLevel == ConstantUtil.CategoryLevel.SubCategory
                && m.ParentID == categoryId);
        }

        public Category GetCategoryById(int id)
        {
            return _unitOfWork.CategoryRepository.GetByID(id);
        }
    }
}