using Microsoft.AspNetCore.Mvc;
using MISA.Fresher.Core.DTO;
using MISA.Fresher.Core.Enities;
using MISA.Fresher.Core.Interface.Repository;
using MISA.Fresher.Core.Interface.Service;
using MISA.Fresher.Core.Service;
using MySqlConnector;

namespace MISA.Fresher.Web04.Api.Controllers
{
    /// <summary>
    /// nguoi tao: duong cong son
    /// ngay tao: 13/01/2026
    /// </summary>
    [ApiController]
    [Route("api/v1/fixed-assets")]
    public class FixedAssetController : ControllerBase
    {
        IFixedAssetRepo _repo;
        IFixedAssetService _service;

        public FixedAssetController(IFixedAssetRepo repo, IFixedAssetService service)
        {
            _repo = repo;
            _service = service;
        }

        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            var data = _repo.GetAll();
            return Ok(data);
        }

        [HttpPost("Insert")]
        public IActionResult Post([FromBody] FixedAssetCreateDto dto)
        {
            // ✅ FIX NHANH: gán audit field tại Controller
            dto.CreatedBy = "admin";
            dto.UpdatedBy = "admin";
            dto.CreatedAt = DateTime.Now;
            dto.UpdatedAt = DateTime.Now;

            var res = _service.Insert(dto);
            return StatusCode(201, res);
        }
        [HttpPut("Update/{id}")]
        public IActionResult Put([FromRoute]Guid id, [FromBody] FixedAssetUpdateDto dto)
        {
            dto.CreatedBy = "admin";
            dto.UpdatedBy = "admin";
            dto.UpdatedAt = DateTime.Now;
            dto.UpdatedBy = "system";

            var res = _service.Update(id, dto);
            return Ok(res);
        }
        [HttpDelete("Delete/{id}")]
        public IActionResult Delete(Guid id)
        {
            var fixedAsset = new FixedAsset
            {
                FixedAssetId = id
            };
            var res = _service.Delete(fixedAsset);
            return StatusCode(201);
        }
        //xóa thêm có chọn nhiều column
        [HttpGet("filter")]
        public async Task<IActionResult> Filter([FromQuery] FixedAssetFilterDto filterDto)
        {
            var res = await _service.GetFilterAsync(filterDto);
            return Ok(res);
        }
        // Ngày tạo: 18/01/2026
        [HttpGet("new-code")]
        public IActionResult GetNewCode()
        {
            var code = _service.GenerateNewFixedAssetCode();
            return Ok(new { fixedAssetCode = code });
        }
        //[HttpPost("{id}/duplicate")]
        //public IActionResult Duplicate(Guid id)
        //{
        //    var newItem = _service.Duplicate(id);
        //    if (newItem == null) return NotFound();

        //    return StatusCode(201, newItem);
        //}
        public class DeleteBulkRequest
        {
            public List<Guid> Ids { get; set; } = new();
        }

        [HttpDelete("DeleteBulk")]
        public IActionResult DeleteBulk([FromBody] DeleteBulkRequest request)
        {
            if (request?.Ids == null || request.Ids.Count == 0)
                return BadRequest("Danh sách ids trống.");

            var affected = _service.DeleteMany(request.Ids);
            return Ok(new { Deleted = affected });
        }

        [HttpPut("{id}/clone")]
        public IActionResult Clone(Guid id, [FromBody] FixedAssetDuplicateDto dto)
        {
            var result = _service.Clone(id, dto);
            return Ok(result); // hoặc map sang DTO
        }
    }
}
