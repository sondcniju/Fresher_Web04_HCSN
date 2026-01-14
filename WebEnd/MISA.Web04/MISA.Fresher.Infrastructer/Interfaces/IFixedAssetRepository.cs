using MISA.Fresher.Core.Enities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.Fresher.Infrastructer.Interfaces
{
    /// <summary>
    /// Người tạo: Duong Cong Son
    /// Ngày tạo: 13/01/2026
    /// </summary>
    public interface IFixedAssetRepository
    {
        Task<List<FixedAsset>> GetAllAsync(CancellationToken ct = default);
        Task<FixedAsset?> GetByIdAsync(Guid fixedAssetId, CancellationToken ct = default);
        Task<FixedAsset?> GetByCodeAsync(string fixedAssetCode, CancellationToken ct = default);
        Task<int> InsertAsync(FixedAsset fixedAsset, CancellationToken ct = default);
        Task<int> UpdateAsync(FixedAsset fixedAsset, CancellationToken ct = default);
        Task<int> DeleteAsync(Guid fixedAssetId, CancellationToken ct = default);
    }
}
