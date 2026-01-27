using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MISA.Fresher.Core.DTO;
using MISA.Fresher.Core.Enities;
using MISA.Fresher.Core.Interface.Repository;
using MISA.Fresher.Infrastructer.Repository;
using MySqlConnector;

namespace MISA.Fresher04.Infrastructer.Repositories;

/// <remarks>
/// Người tạo: duong cong son
/// Ngày tạo: 13/01/2026
/// </remarks>
public class FixedAssetRepo : BaseRepo<FixedAsset>, IFixedAssetRepo
{

    public FixedAssetRepo(IConfiguration config, IHostEnvironment env) : base(config, env)
    {

    }

    public async Task<PagedResult<FixedAssetTableDto>> GetFilterAsync(FixedAssetFilterDto filterDto)
    {
        const string fa = "fa";
        const string d = "d";
        const string t = "t";

        var where = new List<string>();
        var param = new DynamicParameters();

        // 1) Filter department/type
        if (filterDto.DepartmentId.HasValue)
        {
            where.Add($"{fa}.fixed_asset_department_id = @DepartmentId");
            param.Add("@DepartmentId", filterDto.DepartmentId.Value);
        }

        if (filterDto.TypeId.HasValue)
        {
            where.Add($"{fa}.fixed_asset_type_id = @TypeId");
            param.Add("@TypeId", filterDto.TypeId.Value);
        }

        // 2) Keyword (code/name)
        if (!string.IsNullOrWhiteSpace(filterDto.Keyword))
        {
            where.Add($"({fa}.fixed_asset_code LIKE @Keyword OR {fa}.fixed_asset_name LIKE @Keyword)");
            param.Add("@Keyword", $"%{filterDto.Keyword.Trim()}%");
        }

        var whereSql = where.Any() ? "WHERE " + string.Join(" AND ", where) : "";

        // 3) Paging
        var pageNumber = filterDto.PageNumber <= 0 ? 1 : filterDto.PageNumber;
        var pageSize = filterDto.PageSize <= 0 ? 20 : filterDto.PageSize;
        var skip = (pageNumber - 1) * pageSize;

        param.Add("@Limit", pageSize);
        param.Add("@Skip", skip);

        var sqlData = $@"
SELECT
    {fa}.fixed_asset_id                    AS FixedAssetId,
    {fa}.fixed_asset_code                  AS FixedAssetCode,
    {fa}.fixed_asset_name                  AS FixedAssetName,

    {fa}.fixed_asset_department_id         AS FixedAssetDepartmentId,
    {d}.fixed_asset_department_name        AS FixedAssetDepartmentName,

    {fa}.fixed_asset_type_id               AS FixedAssetTypeId,
    {t}.fixed_asset_type_name              AS FixedAssetTypeName,

    {fa}.fixed_asset_quantity              AS FixedAssetQuantity,
    {fa}.fixed_asset_cost                  AS FixedAssetCost,
    {fa}.fixed_asset_depreciation_value_year AS FixedAssetDepreciationValueYear,
    {fa}.depreciation_rate                 AS DepreciationRate,

    {fa}.created_at                        AS CreatedAt
FROM fixed_asset {fa}
LEFT JOIN fixed_asset_department {d}
    ON {fa}.fixed_asset_department_id = {d}.fixed_asset_department_id
LEFT JOIN fixed_asset_type {t}
    ON {fa}.fixed_asset_type_id = {t}.fixed_asset_type_id
{whereSql}
ORDER BY {fa}.created_at DESC
LIMIT @Limit OFFSET @Skip;
";

        var sqlCount = $@"
SELECT COUNT(1)
FROM fixed_asset {fa}
LEFT JOIN fixed_asset_department {d}
    ON {fa}.fixed_asset_department_id = {d}.fixed_asset_department_id
LEFT JOIN fixed_asset_type {t}
    ON {fa}.fixed_asset_type_id = {t}.fixed_asset_type_id
{whereSql};
";

        using var connection = new MySqlConnection(_connectionString);

        var data = await connection.QueryAsync<FixedAssetTableDto>(sqlData, param);
        var total = await connection.ExecuteScalarAsync<int>(sqlCount, param);

        return new PagedResult<FixedAssetTableDto>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = total,
            Data = data.ToList()
        };
    }


    public int GetMaxFixedAssetNumber(string prefix)
    {
        var sql = @"
        SELECT COALESCE(
            MAX(CAST(SUBSTRING(fixed_asset_code, CHAR_LENGTH(@Prefix) + 1) AS UNSIGNED)),
            0
        )
        FROM fixed_asset
        WHERE fixed_asset_code LIKE CONCAT(@Prefix, '%');
    ";

        using var conn = new MySqlConnection(_connectionString);
        return conn.QueryFirst<int>(sql, new { Prefix = prefix });
    }

    public FixedAssetDuplicateDto Clone(Guid id, FixedAssetDuplicateDto dto)
    {
        throw new NotImplementedException();
    }


    public FixedAsset Clone(FixedAsset entity)
    {
        throw new NotImplementedException();
    }
}
