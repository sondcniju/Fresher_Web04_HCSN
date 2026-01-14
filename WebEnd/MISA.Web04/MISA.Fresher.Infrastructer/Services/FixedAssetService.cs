using MISA.Fresher.Core.Enities;
using MISA.Fresher.Infrastructer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.Fresher.Infrastructer.Services
{

    /// <summary>
    /// Người tạo: Duong Cong Son
    /// Ngày tạo: 13/01/2026
    /// </summary>
    public class FixedAssetService : IFixedAssetService
    {
        private readonly IFixedAssetRepository _repo;

        public FixedAssetService(IFixedAssetRepository repo)
        {
            _repo = repo;
        }
        public async Task<FixedAsset> CreateAsync(FixedAsset entity, CancellationToken ct = default)
        {
            //Validate dữ liệu
            var existed = _repo.GetByCodeAsync(entity.FixedAssetCode, ct);
            if (existed.Result != null)
            {
                throw new Exception($"Mã tài sản {entity.FixedAssetCode} đã tồn tại trong hệ thống, vui lòng kiểm tra lại!");
            }
            // Giao cho DB/trigger tính năm + hao mòn (đúng SQL), service không tự “bịa” công thức.
            await _repo.InsertAsync(entity, ct);
            return (await _repo.GetByCodeAsync(entity.FixedAssetCode, ct))!;
        }
        public async Task<FixedAsset> UpdateAsync(Guid id, FixedAsset enity, CancellationToken ct = default)
        {
            var current = await _repo.GetByIdAsync(id, ct);
            if (current == null)
            {
                throw new Exception($"Không tìm thấy tài sản với ID: {id}");
            }
            //neu code trung
            if (current.FixedAssetCode != enity.FixedAssetCode)
            {
                var existed = await _repo.GetByCodeAsync(enity.FixedAssetCode, ct);
                if (existed != null)
                {
                    throw new Exception($"Mã tài sản {enity.FixedAssetCode} đã tồn tại trong hệ thống, vui lòng kiểm tra lại!");
                }
            }
            enity.FixedAssetId = id;
            await _repo.UpdateAsync(enity, ct);
            return (await _repo.GetByIdAsync(id, ct))!;
        }
        public async Task DeletedAsync(Guid fixedAssetId, CancellationToken ct = default)
        {
            var row =await _repo.DeleteAsync(fixedAssetId, ct);
            if (row == 0)
            {
                throw new Exception($"Xóa tài sản không thành công, vui lòng kiểm tra lại!");
            }
        }

        public Task<List<FixedAsset>> GetAllAsync(CancellationToken ct = default)
        => _repo.GetAllAsync(ct);

        public async Task<FixedAsset> GetByIdAsync(Guid fixedAssetId, CancellationToken ct = default)
        => await _repo.GetByIdAsync(fixedAssetId, ct) 
            ?? throw new Exception($"Không tìm thấy tài sản với ID: {fixedAssetId}");

        public Task<FixedAsset> UpdateAsync(FixedAsset fixedAsset, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
