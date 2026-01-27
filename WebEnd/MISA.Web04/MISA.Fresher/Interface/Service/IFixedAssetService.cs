using Microsoft.AspNetCore.Http;
using MISA.Fresher.Core.DTO;
using MISA.Fresher.Core.Enities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.Fresher.Core.Interface.Service
{
    public interface IFixedAssetService: IBaseService<FixedAsset>
    {
        List<FixedAsset> Import(IFormFile file);
        /// <summary>
        /// Xuất khẩu  sách tài sản cố định ra file excel
        /// 
        IFormFile Export(String fixedassetId);
        Guid Insert(FixedAssetCreateDto dto);
        FixedAssetUpdateDto Update(Guid id, FixedAssetUpdateDto dto);
        FixedAssetDuplicateDto Clone(Guid id, FixedAssetDuplicateDto dto);
        Task<PagedResult<FixedAssetTableDto>> GetFilterAsync(FixedAssetFilterDto filterDto);

        string GenerateNewFixedAssetCode();
    }
}
