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

        public void UpdateAsync(Departament departament)
        {
            _db.Departaments.Update(departament);
        }
    }
}
