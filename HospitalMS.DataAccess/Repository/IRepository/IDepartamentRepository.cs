using HospitalMS.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalMS.DataAccess.Repository.IRepository
{
    public interface IDepartamentRepository : IRepository<Departament>
    {
        Task<Departament?> UpdateAsync(int id, Departament departament);
        Task<bool> DeleteDepartment(int id);
    }
}
