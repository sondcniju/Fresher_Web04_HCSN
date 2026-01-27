using MISA.Fresher.Core.MISAAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.Fresher.Core.Enities
{
    [MISATableName("fixed_asset_type")]
    public class FixedAssetType
    {
        [MISAColumnName("fixed_asset_type_id")]
        public Guid FixedAssetTypeId { get; set; }

        [MISAColumnName("fixed_asset_type_code")]
        public string FixedAssetTypeCode { get; set; } = string.Empty;

        [MISAColumnName("fixed_asset_type_name")]
        public string FixedAssetTypeName { get; set; } = string.Empty;

        [MISAColumnName("fixed_asset_life_year")]
        public int FixedAssetLifeYear { get; set; }

        [MISAColumnName("fixed_asset_depreciation_rate")]
        public decimal FixedAssetDepreciationRate { get; set; }

        [MISAColumnName("created_at")]
        public DateTime CreateAt { get; set; }

        [MISAColumnName("updated_at")]
        public DateTime UpdateAt { get; set; }

        [MISAColumnName("created_by")]
        public string? CreatedBy { get; set; }

        [MISAColumnName("updated_by")]
        public string? UpdatedBy { get; set; }
    }
}
