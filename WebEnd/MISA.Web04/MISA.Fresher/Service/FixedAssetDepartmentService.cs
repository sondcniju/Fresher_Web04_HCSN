using MISA.Fresher.Core.DTO;
using MISA.Fresher.Core.Enities;
using MISA.Fresher.Core.Interface.Repository;
using MISA.Fresher.Core.Interface.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.Fresher.Core.Service
{
    public class FixedAssetDepartmentService : BaseService<FixedAssetDepartment>, IFixedAssetDepartmentService
    {
        private readonly IFixedAssetDepartmentRepo _repo;

        public FixedAssetDepartmentService(IFixedAssetDepartmentRepo repo)
        {
            _repo = repo;
        }

        public List<FixedAssetDepartmentComboBoxDto> GetForCombobox()
        {
            return _repo.GetForCombobox();
        }

    }
}
