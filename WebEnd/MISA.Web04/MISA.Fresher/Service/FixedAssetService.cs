using Microsoft.AspNetCore.Http;
using MISA.Fresher.Core.DTO;
using MISA.Fresher.Core.Enities;
using MISA.Fresher.Core.Interface.Repository;
using MISA.Fresher.Core.Interface.Service;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.Fresher.Core.Service
{

    public class FixedAssetService : BaseService<FixedAsset>, IFixedAssetService
    {
        private readonly string _connectionString;
        IFixedAssetRepo _Repo;
        public FixedAssetService(IFixedAssetRepo repo)
        {
            _Repo = repo;
        }


        public IFormFile Export(string fixedassetId)
        {
            throw new NotImplementedException();
        }

        public List<FixedAsset> Import(IFormFile file)
        {
            throw new NotImplementedException();
        }

        public Guid Insert(FixedAssetCreateDto dto)
        {
            var entity = new FixedAsset
            {
                FixedAssetId = Guid.NewGuid(),

                FixedAssetCode = dto.FixedAssetCode,
                FixedAssetName = dto.FixedAssetName,
                FixedAssetDepartmentId = dto.FixedAssetDepartmentId,
                FixedAssetTypeId = dto.FixedAssetTypeId,

                FixedAssetPurchaseDate = dto.FixedAssetPurchaseDate,
                FixedAssetUsingYear = dto.FixedAssetUsingYear,
                FixedAssetTrackingYear = dto.FixedAssetTrackingYear,
                FixedAssetQuantity = dto.FixedAssetQuantity,
                FixedAssetCost = dto.FixedAssetCost,

                FixedAssetDepreciationValueYear = dto.FixedAssetDepreciationValueYear,
                DepreciationRate = dto.FixedAssetDepreciationRate,

                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                CreatedBy = "admin",
                UpdatedBy = "admin"
            };

            _Repo.Insert(entity);
            return entity.FixedAssetId;
        }



        public FixedAssetUpdateDto Update(Guid id, FixedAssetUpdateDto dto)
        {
            var entity = _Repo.GetById(id);
            if (entity == null) throw new Exception("Không tìm thấy tài sản");

            // map dto -> entity (các field cho phép sửa)
            entity.FixedAssetName = dto.FixedAssetName;
            entity.FixedAssetDepartmentId = dto.FixedAssetDepartmentId;
            entity.FixedAssetTypeId = dto.FixedAssetTypeId;
            entity.FixedAssetPurchaseDate = dto.FixedAssetPurchaseDate;
            entity.FixedAssetUsingYear = dto.FixedAssetUsingYear;
            entity.FixedAssetTrackingYear = dto.FixedAssetTrackingYear;
            entity.FixedAssetQuantity = dto.FixedAssetQuantity;
            entity.FixedAssetCost = dto.FixedAssetCost;
            entity.FixedAssetDepreciationValueYear = dto.FixedAssetDepreciationValueYear;
            entity.DepreciationRate = dto.FixedAssetDepreciationRate;
            entity.FixedAssetStartUsingDate = dto.FixedAssetStartUsingDate;

            // thường không cho sửa code trong update
            // entity.FixedAssetCode = dto.FixedAssetCode;

            entity.UpdatedAt = DateTime.Now;
            entity.UpdatedBy = dto.UpdatedBy ?? "admin";

            var updated = _Repo.Update(id, entity); // ✅ repo nhận entity

            // map entity -> dto để trả về
            return new FixedAssetUpdateDto
            {
                FixedAssetId = updated.FixedAssetId,
                FixedAssetCode = updated.FixedAssetCode,
                FixedAssetName = updated.FixedAssetName,
                FixedAssetDepartmentId = updated.FixedAssetDepartmentId,
                FixedAssetTypeId = updated.FixedAssetTypeId,
                FixedAssetPurchaseDate = updated.FixedAssetPurchaseDate,
                FixedAssetUsingYear = updated.FixedAssetUsingYear,
                FixedAssetTrackingYear = updated.FixedAssetTrackingYear,
                FixedAssetQuantity = updated.FixedAssetQuantity,
                FixedAssetCost = updated.FixedAssetCost,
                FixedAssetDepreciationValueYear = updated.FixedAssetDepreciationValueYear,
                FixedAssetDepreciationRate = updated.DepreciationRate,
                FixedAssetStartUsingDate = updated.FixedAssetStartUsingDate,
                CreatedAt = updated.CreatedAt,
                UpdatedAt = updated.UpdatedAt,
                CreatedBy = updated.CreatedBy,
                UpdatedBy = updated.UpdatedBy
            };
        }


        public FixedAssetDuplicateDto Clone(Guid id, FixedAssetDuplicateDto dto)
        {
            var entity = new FixedAsset
            {
                FixedAssetId = Guid.NewGuid(),
                FixedAssetCode = dto.FixedAssetCode,
                FixedAssetName = dto.FixedAssetName,
                FixedAssetDepartmentId = dto.FixedAssetDepartmentId,
                FixedAssetTypeId = dto.FixedAssetTypeId,
                FixedAssetPurchaseDate = dto.FixedAssetPurchaseDate,
                FixedAssetUsingYear = dto.FixedAssetUsingYear,
                FixedAssetTrackingYear = dto.FixedAssetTrackingYear,
                FixedAssetQuantity = dto.FixedAssetQuantity,
                FixedAssetCost = dto.FixedAssetCost,
                FixedAssetDepreciationValueYear = dto.FixedAssetDepreciationValueYear,
                DepreciationRate = dto.FixedAssetDepreciationRate,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                CreatedBy = "admin",
                UpdatedBy = "admin"
            };
            _Repo.Insert(entity);
            return dto;
        }


        public async Task<PagedResult<FixedAssetTableDto>> GetFilterAsync(FixedAssetFilterDto filterDto)
        {
            return await _Repo.GetFilterAsync(filterDto);
        }

        public int DeleteMany(List<Guid> ids)
        {
            return _Repo.DeleteMany(ids);
        }
        private const string Prefix = "TS";
        private const int NumberLength = 5; // TS00000 => 5 chữ số

        public string GenerateNewFixedAssetCode()
        {
            var currentMax = _Repo.GetMaxFixedAssetNumber(Prefix);
            var next = currentMax + 1;

            return $"{Prefix}{next.ToString().PadLeft(NumberLength, '0')}";
        }

    }
}
