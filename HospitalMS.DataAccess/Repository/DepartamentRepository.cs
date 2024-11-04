using HospitalMS.DataAccess.Data;
using HospitalMS.DataAccess.Repository.IRepository;
using HospitalMS.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalMS.DataAccess.Repository
{
    public class DepartamentRepository : Repository<Departament>, IDepartamentRepository
    {
        private ApplicationDbContext _db;

        public DepartamentRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<Departament?> UpdateAsync(int id, Departament departament)
        {
            var departmentFromDb = await _db.Departaments.FindAsync(id);
            
            if (departmentFromDb != null)
            {
                departmentFromDb.Name = departament.Name;
                departmentFromDb.Location = departament.Location;

                return departmentFromDb;
            }

            return null;
        }

        public async Task<bool> DeleteDepartment(int id)
        {
            var departmentFromDb = await _db.Departaments.FindAsync(id);

            if (departmentFromDb != null)
            {
                _db.Departaments.Remove(departmentFromDb);
                return true;
            }

            return false;
        }
    }
}
