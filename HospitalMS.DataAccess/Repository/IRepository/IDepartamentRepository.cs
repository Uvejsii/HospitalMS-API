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
        void UpdateAsync(Departament departament);
    }
}
