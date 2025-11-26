using System.Threading.Tasks;
using EncoraOne.Grievance.API.Data;
using EncoraOne.Grievance.API.Models;
using EncoraOne.Grievance.API.Repositories.Interfaces;

namespace EncoraOne.Grievance.API.Repositories.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        // Private fields for the repositories
        private IGenericRepository<Employee> _employees;
        private IGenericRepository<Manager> _managers;
        private IGenericRepository<Department> _departments;
        private IGenericRepository<Complaint> _complaints;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        // Lazy Loading Pattern: We only create the repository when it is requested.
        public IGenericRepository<Employee> Employees
        {
            get
            {
                if (_employees == null)
                {
                    _employees = new GenericRepository<Employee>(_context);
                }
                return _employees;
            }
        }

        public IGenericRepository<Manager> Managers
        {
            get
            {
                if (_managers == null)
                {
                    _managers = new GenericRepository<Manager>(_context);
                }
                return _managers;
            }
        }

        public IGenericRepository<Department> Departments
        {
            get
            {
                if (_departments == null)
                {
                    _departments = new GenericRepository<Department>(_context);
                }
                return _departments;
            }
        }

        public IGenericRepository<Complaint> Complaints
        {
            get
            {
                if (_complaints == null)
                {
                    _complaints = new GenericRepository<Complaint>(_context);
                }
                return _complaints;
            }
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}