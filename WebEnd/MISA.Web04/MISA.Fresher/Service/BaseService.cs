using MISA.Fresher.Core.Interface.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.Fresher.Core.Service
{
    public class BaseService<T>
    {
        public T Delete(T entity)
        {
            throw new NotImplementedException();
        }

        public int DeleteMany(List<Guid> ids)
        {
            throw new NotImplementedException();
        }
        
        public T Update(Guid id,  T entity)
        {
            throw new NotImplementedException();
        }

        public T Insert(T entity)
        {
            throw new NotImplementedException();
        }
        public T Clone(T entity)
        {
            throw new NotImplementedException();
        }
    }
}
