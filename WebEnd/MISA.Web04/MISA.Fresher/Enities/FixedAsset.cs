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
    public class FixedAsset
    {
        public Guid FixedAssetId { get; set; }
        public string FixedAssetCode { get; set; } = string.Empty;
        public string FixedAssetName { get; set; } = string.Empty;
        public Guid FixedAssetDepartmentId { get; set; }
        public Guid FixedAssetTypeId { get; set; }
        public DateTime FixedAssetPurchaseDate { get; set; }
        public int FixedAssetUsingYear { get; set; }
        public int FixedAssetTrackingYear { get; set; }

        public int FixedAssetQuantity { get; set; }
        public decimal FixedAssetCost { get; set; }
        public decimal FixedAssetDepreciationValueYear { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

    }   
}
