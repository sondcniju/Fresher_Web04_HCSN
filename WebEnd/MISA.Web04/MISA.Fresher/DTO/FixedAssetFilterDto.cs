using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.Fresher.Core.DTO
{
    public class FixedAssetFilterDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        public string? Keyword { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? TypeId { get; set; }
    }
}
