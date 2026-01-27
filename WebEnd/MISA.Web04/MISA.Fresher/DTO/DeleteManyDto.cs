using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.Fresher.Core.DTO
{
    public class DeleteManyDto
    {
            public List<Guid> Ids { get; set; } = new();
       
    }
}
