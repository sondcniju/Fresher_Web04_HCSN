using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MISA.Fresher.Core.DTO;
using MISA.Fresher.Core.Enities;
using MISA.Fresher.Core.Interface.Repository;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.Fresher.Infrastructer.Repository
{
     public class FixedAssetTypeRepo: BaseRepo<FixedAssetType>, IFixedAssetTypeRepo
    {
        private readonly string _connectionString;
        MySqlConnection connection;
        public FixedAssetTypeRepo(IConfiguration config, IHostEnvironment env) : base(config, env)
        {
            if (env.IsDevelopment())
            {
                _connectionString = config.GetConnectionString("Development");
            }
            else
            {
                _connectionString = config.GetConnectionString("Production");
            }
            }

        public FixedAssetType Clone(FixedAssetType entity)
        {
            throw new NotImplementedException();
        }

        public FixedAssetType Duplicate(Guid sourceId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Lấy danh sách loại tài sản cho combobox
        /// CreateBy: bạn
        /// (16/01/2026)
        /// </summary>
        public List<FixedAssetTypeComboBoxDto> GetForCombobox()
        {
            var sql = @"
        SELECT
            fixed_asset_type_id            AS FixedAssetTypeId,
            fixed_asset_type_code          AS FixedAssetTypeCode,
            fixed_asset_type_name          AS FixedAssetTypeName,
            fixed_asset_depreciation_rate  AS DepreciationRate,
            fixed_asset_life_year     AS LifeYear
        FROM fixed_asset_type
        ORDER BY fixed_asset_type_code;
    ";

            using var connection = new MySqlConnection(_connectionString);
            return connection.Query<FixedAssetTypeComboBoxDto>(sql).ToList();
        }

        public FixedAssetType Update(FixedAssetType entity)
        {
            throw new NotImplementedException();
        }
    }
}
