using Microsoft.AspNetCore.Mvc;
using MISA.Fresher.Infrastructer.Services;
using MISA.Fresher.Core.Enities;

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
        private readonly IFixedAssetService _fixedAssetService;

        public FixedAssetController(IFixedAssetService fixedAssetService)
        {
            _fixedAssetService = fixedAssetService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync(CancellationToken ct)
        {
            var data = await _fixedAssetService.GetAllAsync(ct);
            return Ok(data);
        }

        [HttpGet("{id:guid}", Name = "GetFixedAssetById")]
        public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken ct)
        {
            var data = await _fixedAssetService.GetByIdAsync(id, ct);
            if (data == null) return NotFound();
            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] FixedAsset fixedAsset, CancellationToken ct)
        {
            var data = await _fixedAssetService.CreateAsync(fixedAsset, ct);

            return CreatedAtAction(
                nameof(GetByIdAsync),
                new { id = data.FixedAssetId },
                data
            );
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] FixedAsset fixedAsset, CancellationToken ct)
        {
            // Khuyến nghị: URL là nguồn sự thật, không bắt body phải có đúng ID
            fixedAsset.FixedAssetId = id;

            var data = await _fixedAssetService.UpdateAsync(fixedAsset, ct);
            return Ok(data);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken ct)
        {
            await _fixedAssetService.DeletedAsync(id, ct);
            return NoContent();
        }
    }
}
