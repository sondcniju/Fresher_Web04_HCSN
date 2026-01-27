using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.Fresher.Core.DTO
{
    public class FixedAssetTableDto
    {
        public Guid FixedAssetId { get; set; }
        public string FixedAssetCode { get; set; } = "";
        public string FixedAssetName { get; set; } = "";

        public Guid FixedAssetDepartmentId { get; set; }
        public string FixedAssetDepartmentName { get; set; } = "";

        public Guid FixedAssetTypeId { get; set; }
        public string FixedAssetTypeName { get; set; } = "";

        public int FixedAssetQuantity { get; set; }
        public decimal FixedAssetCost { get; set; }
        public decimal FixedAssetDepreciationValueYear { get; set; }
        public decimal DepreciationRate { get; set; }

        public DateTime CreatedAt { get; set; }
    }


}
