using MISA.Fresher.Core.MISAAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.Fresher.Core.DTO
{
    [MISATableName("fixed_asset_type")]
    public class FixedAssetTypeComboBoxDto
    {

            [MISAColumnName("fixed_asset_type_id")]    
            public Guid FixedAssetTypeId { get; set; }
            [MISAColumnName("fixed_asset_type_code")]
            public string FixedAssetTypeCode { get; set; } = "";
            [MISAColumnName("fixed_asset_type_name")]
             public string FixedAssetTypeName { get; set; } = "";
            [MISAColumnName("fixed_asset_depreciation_rate")]
            public decimal DepreciationRate { get; set; } // nếu có
            [MISAColumnName("fixed_asset_life_year")]
            public int LifeYear { get; set; }             // nếu có
        }
}//combobox sẽ để chung lấy id code name 
