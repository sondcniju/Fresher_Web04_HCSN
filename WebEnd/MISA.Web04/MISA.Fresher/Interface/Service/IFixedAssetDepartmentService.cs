using MISA.Fresher.Core.DTO;
using MISA.Fresher.Core.Enities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.Fresher.Core.Interface.Service
{
    public interface IFixedAssetDepartmentService: IBaseService<FixedAssetDepartment>
    {
        List<FixedAssetDepartmentComboBoxDto> GetForCombobox();
    }
}
