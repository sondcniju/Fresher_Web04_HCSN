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
    public class FixedAssetTypeService :BaseService<FixedAssetType> ,IFixedAssetTypeService
    {
        private readonly IFixedAssetTypeRepo _repo;

        public FixedAssetTypeService(IFixedAssetTypeRepo repo)
        {
            _repo = repo;
        }

        public List<FixedAssetTypeComboBoxDto> GetForCombobox()
        {
            return _repo.GetForCombobox();
        }

        public FixedAssetType Update(Guid id, FixedAssetType entity)
        {
            throw new NotImplementedException();
        }
    }
}
