using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MISA.Fresher.Core.DTO;
using MISA.Fresher.Core.Enities;
using MISA.Fresher.Core.Interface.Repository;
using MISA.Fresher.Core.MISAAttribute;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.Fresher.Infrastructer.Repository
{
    public class FixedAssetDepartmentRepo:BaseRepo<FixedAssetDepartment>, IFixedAssetDepartmentRepo
    {
        private readonly string _connectionString;
        MySqlConnection connection;
        public FixedAssetDepartmentRepo(IConfiguration config, IHostEnvironment env) : base(config, env)
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

        public FixedAssetDepartment Clone(FixedAssetDepartment entity)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Lấy danh sách bộ phận sử dụng cho combobox
        /// CreateBy: Duong Cong Son
        /// (16/01/2026)
        /// </summary>
        public List<FixedAssetDepartmentComboBoxDto> GetForCombobox()
        {
            var tablename = GetTableName();
            var sql = $"SELECT {tablename}_id AS FixedAssetDepartmentId, " +
                      $"{tablename}_code AS FixedAssetDepartmentCode, " +
                      $"{tablename}_name AS FixedAssetDepartmentName " +
                      $"FROM {tablename}";

            using var connection = new MySqlConnection(_connectionString);
            return connection.Query<FixedAssetDepartmentComboBoxDto>(sql).ToList();
        }

        public FixedAssetDepartment Update(FixedAssetDepartment entity)
        {
            throw new NotImplementedException();
        }
    }
}
