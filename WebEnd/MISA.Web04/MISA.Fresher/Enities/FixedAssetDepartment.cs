using MISA.Fresher.Core.MISAAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.Fresher.Core.Enities
{
    [MISATableName("fixed_asset_department")]
    public class FixedAssetDepartment
    {
        [MISAColumnName("fixed_asset_department_id")]
        public Guid FixedAssetDepartmentId { get; set; }

        [MISAColumnName("fixed_asset_department_code")]
        public string FixedAssetDepartmentCode { get; set; } = string.Empty;

        [MISAColumnName("fixed_asset_department_name")]
        public string FixedAssetDepartmentName { get; set; } = string.Empty;

        [MISAColumnName("created_at")]
        public DateTime CreatedAt { get; set; }

        [MISAColumnName("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [MISAColumnName("created_by")]
        public string? CreatedBy { get; set; }

        [MISAColumnName("updated_by")]
        public string? UpdatedBy { get; set; }
    }
}
