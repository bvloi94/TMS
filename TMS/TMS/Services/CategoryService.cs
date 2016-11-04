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

        public IEnumerable<Category> GetAll()
        {
            return _unitOfWork.CategoryRepository.Get();
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

        public Category GetCategoryByName(string name)
        {
            return _unitOfWork.CategoryRepository.Get(m => m.Name.Equals(name)).ElementAt(0);
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
                _unitOfWork.Commit();
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
                _unitOfWork.Commit();
            }
            catch
            {
                throw;
            }
        }

        private IEnumerable<Category> GetChildrenCategories(int parentId)
        {
            return _unitOfWork.CategoryRepository.Get(m => m.ParentID == parentId);
        }

        public bool IsInUse(Category category)
        {
            foreach (Category childCategory in GetChildrenCategories(category.ID))
            {
                if (IsInUse(childCategory))
                {
                    return true;
                }
            }
            return _unitOfWork.TicketRepository.Get(m => m.CategoryID == category.ID).Any() 
                || _unitOfWork.SolutionRepository.Get(m => m.CategoryID == category.ID).Any();
        }

        public void DeleteCategory(Category category)
        {
            foreach (Category childCategory in GetChildrenCategories(category.ID).ToList())
            {
                if (childCategory.CategoryLevel == ConstantUtil.CategoryLevel.SubCategory)
                {
                    foreach (Category item in GetChildrenCategories(childCategory.ID).ToList())
                    {
                        _unitOfWork.CategoryRepository.Delete(item);
                    }
                }
                _unitOfWork.CategoryRepository.Delete(childCategory);
            }
            _unitOfWork.CategoryRepository.Delete(category);
            _unitOfWork.Commit();
        }

        public List<int> GetChildrenCategoriesIdList(int categoryId)
        {
            List<int> list = new List<int>();
            IEnumerable<Category> childrenCategories = GetChildrenCategories(categoryId);
            foreach (Category childCategory in childrenCategories)
            {
                list.Add(childCategory.ID);
                if (childCategory.CategoryLevel == ConstantUtil.CategoryLevel.SubCategory)
                {
                    IEnumerable<Category> items = GetChildrenCategories(childCategory.ID);
                    foreach (Category item in items)
                    {
                        list.Add(item.ID);
                    }
                }
            }
            return list;
        }

        public string GetCategoryPath(Category category)
        {
            if (category == null)
            {
                return "-";
            }
            else
            {
                string path = "";
                path = category.Name;

                while (category.ParentID != null)
                {
                    category = GetCategoryById(category.ParentID.Value);
                    path = category.Name + " > " + path;
                }

                return path;
            }
        }
    }
}