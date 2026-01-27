using MISA.Fresher.Core.DTO;
using MISA.Fresher.Core.Enities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.Fresher.Core.Interface.Service
{
    public interface IBaseService<T>
    {
        T Insert(T entity);
        T Delete(T entity);
        T Update (Guid id, T entity);
        T Clone( T entity);
        int DeleteMany(List<Guid> ids);

    }
}
