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

        public Category GetCategoryById(int id)
        {
            return _unitOfWork.CategoryRepository.GetByID(id);
        }

        public IEnumerable<Category> GetCategories()
        {
            return _unitOfWork.CategoryRepository.Get(m => m.CategoryLevel == ConstantUtil.CategoryLevel.Category);
        }

        public IEnumerable<Category> GetSubCategories()
        {
            return _unitOfWork.CategoryRepository.Get(m => m.CategoryLevel == ConstantUtil.CategoryLevel.SubCategory);
        }

        public IEnumerable<Category> GetSubCategories(int categoryId)
        {
            return _unitOfWork.CategoryRepository.Get(m => m.CategoryLevel == ConstantUtil.CategoryLevel.SubCategory
                && m.ParentID == categoryId);
        }

        public IEnumerable<Category> GetItems(int subCategoryId)
        {
            return _unitOfWork.CategoryRepository.Get(m => m.CategoryLevel == ConstantUtil.CategoryLevel.Item
                && m.ParentID == subCategoryId);
        }

        public bool IsDuplicatedName(int? id, string name, int? parentId)
        {
            if (id.HasValue)
            {
                return _unitOfWork.CategoryRepository.Get(m => !m.ID.Equals(id.Value) && m.Name.ToLower().Equals(name.ToLower())
                    && m.ParentID == parentId).Any();
            }
            else
            {
                return _unitOfWork.CategoryRepository.Get(m => m.Name.ToLower().Equals(name.ToLower())
                    && m.ParentID == parentId).Any();
            }
        }

        public void AddCategory(Category category)
        {
            try
            {
                _unitOfWork.CategoryRepository.Insert(category);
                _unitOfWork.Save();
            }
            catch
            {
                throw;
            }
        }

        public void UpdateCategory(Category category)
        {
            try
            {
                _unitOfWork.CategoryRepository.Update(category);
                _unitOfWork.Save();
            }
            catch
            {
                throw;
            }
        }
    }
}