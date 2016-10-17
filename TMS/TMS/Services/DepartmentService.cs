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
            return _unitOfWork.DepartmentRepository.Get(a => (bool) a.IsActive);
        }

        public void AddDepartment(Department dep)
        {
            _unitOfWork.DepartmentRepository.Insert(dep);
            _unitOfWork.Save();
        }

        public Department GetDepartmentById(int id)
        {
            return _unitOfWork.DepartmentRepository.GetByID(id);
        }

        public void EditDepartment(Department dep)
        {
            _unitOfWork.DepartmentRepository.Update(dep);
            _unitOfWork.Save();
        }

        public void RemoveDepartment(string id)
        {
            Department dep = _unitOfWork.DepartmentRepository.GetByID(id);
            dep.IsActive = false;
            _unitOfWork.DepartmentRepository.Update(dep);
            _unitOfWork.Save();
        }
    }
}