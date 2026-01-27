using System;

namespace MISA.Fresher.Core.DTO
{
    /// <summary>
    /// DTO cập nhật tài sản
    /// </summary>
    /// <remarks>
    /// Người tạo: Duong Cong Son  
    /// Ngày tạo: 17/01/2026
    /// </remarks>
    public class FixedAssetUpdateDto
    {
        /// <summary>
        /// Id tài sản (bắt buộc)
        /// </summary>
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

        /// <summary>
        /// Giá trị hao mòn năm
        /// </summary>
        public decimal FixedAssetDepreciationValueYear { get; set; }

        /// <summary>
        /// Tỷ lệ hao mòn (%)
        /// </summary>
        public decimal FixedAssetDepreciationRate { get; set; }

        public DateTime FixedAssetStartUsingDate { get; set; } = DateTime.Now;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public string? CreatedBy { get; set; } = "admin";
        public string? UpdatedBy { get; set; } = "";
    }
}
