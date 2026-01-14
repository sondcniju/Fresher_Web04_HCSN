using MISA.Fresher.Core.Enities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.Fresher.Infrastructer.Services
{
    public interface IFixedAssetService
    {
        Task<List<FixedAsset>> GetAllAsync(CancellationToken ct = default);
        Task<FixedAsset> GetByIdAsync(Guid fixedAssetId, CancellationToken ct = default);
        Task<FixedAsset> CreateAsync(FixedAsset fixedAsset, CancellationToken ct = default);
        Task<FixedAsset> UpdateAsync(FixedAsset fixedAsset, CancellationToken ct = default);
        Task DeletedAsync(Guid fixedAssetId, CancellationToken ct = default);
    }
}
