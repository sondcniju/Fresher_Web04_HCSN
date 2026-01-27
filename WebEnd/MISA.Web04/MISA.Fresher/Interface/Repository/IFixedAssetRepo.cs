using MISA.Fresher.Core.DTO;
using MISA.Fresher.Core.Enities;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.Fresher.Core.Interface.Repository
{
    public interface IFixedAssetRepo: IBaseRepo<FixedAsset>
    {
        /// <summary>
        /// lấy toàn bộ asset
        /// Ngày tạo: 14/01/2026
        /// Người tạo: Duong Cong Son
        Task<PagedResult<FixedAssetTableDto>> GetFilterAsync(FixedAssetFilterDto filterDto);

        FixedAssetDuplicateDto Clone(Guid id, FixedAssetDuplicateDto dto);

        int GetMaxFixedAssetNumber(string prefix);
    }
}
