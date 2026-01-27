using MISA.Fresher.Core.MISAAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.Fresher.Core.Enities
{
    /// <summary>
    /// Tài sản
    /// </summary>
    /// <remarks>
    /// Người tạo: Duong Cong Son
    /// Ngày tạo: 13/01/2026
    /// </remarks>
    [MISATableName("fixed_asset")]
    public class FixedAsset
    {
        [MISAColumnName("fixed_asset_id")]
        public Guid FixedAssetId { get; set; }
        [MISAColumnName("fixed_asset_code")]
        public string FixedAssetCode { get; set; } = string.Empty;

        [MISAColumnName("fixed_asset_name")]
        public string FixedAssetName { get; set; } = string.Empty;

        [MISAColumnName("fixed_asset_department_id")]
        public Guid FixedAssetDepartmentId { get; set; }

        [MISAColumnName("fixed_asset_type_id")]
        public Guid FixedAssetTypeId { get; set; }

        [MISAColumnName("fixed_asset_purchase_date")]
        public DateTime FixedAssetPurchaseDate { get; set; }

        [MISAColumnName("fixed_asset_start_using_date")]
        public DateTime FixedAssetStartUsingDate { get; set; } = DateTime.Now;

        [MISAColumnName("fixed_asset_using_year")]
        public int FixedAssetUsingYear { get; set; }

        [MISAColumnName("fixed_asset_tracking_year")]
        public int FixedAssetTrackingYear { get; set; }

        [MISAColumnName("fixed_asset_quantity")]
        public int FixedAssetQuantity { get; set; }

        [MISAColumnName("fixed_asset_cost")]
        public decimal FixedAssetCost { get; set; }

        [MISAColumnName("fixed_asset_depreciation_value_year")]
        public decimal FixedAssetDepreciationValueYear { get; set; }

        [MISAColumnName("depreciation_rate")]
        public decimal DepreciationRate { get; set; }

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
