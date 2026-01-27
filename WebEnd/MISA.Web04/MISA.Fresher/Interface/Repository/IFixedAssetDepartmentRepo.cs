using MISA.Fresher.Core.DTO;
using MISA.Fresher.Core.Enities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.Fresher.Core.Interface.Repository
{
    public interface IFixedAssetDepartmentRepo: IBaseRepo<FixedAssetDepartment>
    {
        List<FixedAssetDepartmentComboBoxDto> GetForCombobox();

    }
}
