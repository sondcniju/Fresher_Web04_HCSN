using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.Fresher.Core.Interface.Repository
{
    public interface IBaseRepo<T>
    {
        List<T> GetAll();
        T GetById(Guid id);
        T Insert(T entity);
        T Update(Guid id, T entity);

        T Clone(T entity);
        void Delete(Guid id);
        int DeleteMany(IEnumerable<Guid> ids);   // <-- dùng IEnumerable
    }
}
