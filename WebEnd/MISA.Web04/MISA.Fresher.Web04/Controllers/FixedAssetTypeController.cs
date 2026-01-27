using Microsoft.AspNetCore.Mvc;
using MISA.Fresher.Core.DTO;
using MISA.Fresher.Core.Interface.Repository;
using MISA.Fresher.Core.Interface.Service;

namespace MISA.Fresher.Web04.Api.Controllers
{
    [ApiController]
    [Route("api/v1/fixed-asset-type")]
    public class FixedAssetTypeController : Controller
    {
        IFixedAssetTypeRepo _repo;
        IFixedAssetTypeService _service;
        public FixedAssetTypeController(IFixedAssetTypeRepo repo, IFixedAssetTypeService service)
        {
            _repo = repo;
            _service = service; 
        }
        [HttpGet]
        public IActionResult GetAll()
        {
            var data = _repo.GetAll();
            return Ok(data);
        }
        [HttpGet("combobox")]
        public IActionResult GetCombobox()
        {
            var res = _service.GetForCombobox();   // nếu _service null => sẽ nổ ngay từ constructor
            return Ok(res ?? new List<FixedAssetTypeComboBoxDto>());
        }
    }
}
