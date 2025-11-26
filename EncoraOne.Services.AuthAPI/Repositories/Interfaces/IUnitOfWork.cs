using System;
using System.Threading.Tasks;
using EncoraOne.Grievance.API.Models;

namespace EncoraOne.Grievance.API.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // We expose specific repositories here
        IGenericRepository<Employee> Employees { get; }
        IGenericRepository<Manager> Managers { get; }
        IGenericRepository<Department> Departments { get; }
        IGenericRepository<Complaint> Complaints { get; }

        // The method to commit changes to the database
        Task<int> CompleteAsync();
    }
}