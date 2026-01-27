using MISA.Fresher.Core.MISAAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.Fresher.Core.DTO
{
    [MISATableName("fixed_asset_department")]
    public class FixedAssetDepartmentComboBoxDto
    {
        [MISAColumnName("fixed_asset_department_id")]
        public Guid FixedAssetDepartmentId { get; set; }
        [MISAColumnName("fixed_asset_department_code")]
        public string FixedAssetDepartmentCode { get; set; } = string.Empty;
        [MISAColumnName("fixed_asset_department_name")]
        public string FixedAssetDepartmentName { get; set; } = string.Empty;
    }
}
