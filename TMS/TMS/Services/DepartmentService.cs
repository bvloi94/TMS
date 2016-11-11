using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TMS.DAL;
using TMS.Models;

namespace TMS.Services
{
    public class DepartmentService //: IUserService
    {
        private readonly UnitOfWork _unitOfWork;

        public DepartmentService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<Department> GetAll()
        {
            return _unitOfWork.DepartmentRepository.Get();
        }

        public IEnumerable<Department> GetAllDepartment()
        {
            return _unitOfWork.DepartmentRepository.Get();
        }

        public bool AddDepartment(Department department)
        {
            _unitOfWork.DepartmentRepository.Insert(department);
            return _unitOfWork.Commit();
        }

        public Department GetDepartmentById(int id)
        {
            return _unitOfWork.DepartmentRepository.GetByID(id);
        }

        public bool EditDepartment(Department department)
        {
            _unitOfWork.DepartmentRepository.Update(department);
            return _unitOfWork.Commit();
        }

        public bool RemoveDepartment(string id)
        {
            Department dep = _unitOfWork.DepartmentRepository.GetByID(id);
            _unitOfWork.DepartmentRepository.Delete(dep);
            return _unitOfWork.Commit();
        }

        public bool DeleteDepartment(Department department)
        {
            _unitOfWork.DepartmentRepository.Delete(department);
            return _unitOfWork.Commit();
        }

        public bool IsDuplicateName(int? id, string name)
        {
            if (id == null)
            {
                return _unitOfWork.DepartmentRepository.Get(m => m.Name.ToLower().Equals(name.ToLower())).Any();
            }
            else
            {
                return _unitOfWork.DepartmentRepository.Get(m => m.ID != id && m.Name.ToLower().Equals(name.ToLower())).Any();
            }
        }

        public bool IsInUse(Department department)
        {
            return _unitOfWork.AspNetUserRepository.Get(m => m.DepartmentID == department.ID).Any();
        }
    }
}