using MISA.Fresher.Core.Enities;
using MISA.Fresher.Infrastructer.Data;
using MISA.Fresher.Infrastructer.Interfaces;
using MySqlConnector;
using System.Data;
using MySqlConn = MySqlConnector.MySqlConnection;

namespace MISA.Fresher04.Infrastructer.Repositories;

/// <remarks>
/// Người tạo: duong cong son
/// Ngày tạo: 13/01/2026
/// </remarks>
public class FixedAssetRepository : IFixedAssetRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public FixedAssetRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<FixedAsset>> GetAllAsync(CancellationToken ct = default)
    {
        const string sql = @"
SELECT
  fixed_asset_id,
  fixed_asset_code,
  fixed_asset_name,
  fixed_asset_department_id,
  fixed_asset_type_id,
  fixed_asset_purchase_date,
  fixed_asset_using_year,
  fixed_asset_tracking_year,
  fixed_asset_quantity,
  fixed_asset_cost,
  fixed_asset_depreciation_value_year,
  created_at,
  updated_at,
  created_by,
  updated_by
FROM fixed_asset
ORDER BY created_at DESC;";

        using var conn = (MySqlConn)_connectionFactory.CreateConnection();
        await conn.OpenAsync(ct);

        using var cmd = new MySqlCommand(sql, conn);
        using var reader = await cmd.ExecuteReaderAsync(ct);

        var list = new List<FixedAsset>();
        while (await reader.ReadAsync(ct))
        {
            list.Add(MapFixedAsset(reader));
        }

        return list;
    }

    public async Task<FixedAsset?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        const string sql = @"
SELECT
  fixed_asset_id,
  fixed_asset_code,
  fixed_asset_name,
  fixed_asset_department_id,
  fixed_asset_type_id,
  fixed_asset_purchase_date,
  fixed_asset_using_year,
  fixed_asset_tracking_year,
  fixed_asset_quantity,
  fixed_asset_cost,
  fixed_asset_depreciation_value_year,
  created_at,
  updated_at,
  created_by,
  updated_by
FROM fixed_asset
WHERE fixed_asset_id = @id
LIMIT 1;";

        using var conn = (MySqlConn)_connectionFactory.CreateConnection();
        await conn.OpenAsync(ct);

        using var cmd = new MySqlCommand(sql, conn);
        AddParam(cmd, "@id", id.ToString());

        using var reader = await cmd.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
        {
            return MapFixedAsset(reader);
        }

        return null;
    }

    public async Task<FixedAsset?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        const string sql = @"
SELECT
  fixed_asset_id,
  fixed_asset_code,
  fixed_asset_name,
  fixed_asset_department_id,
  fixed_asset_type_id,
  fixed_asset_purchase_date,
  fixed_asset_using_year,
  fixed_asset_tracking_year,
  fixed_asset_quantity,
  fixed_asset_cost,
  fixed_asset_depreciation_value_year,
  created_at,
  updated_at,
  created_by,
  updated_by
FROM fixed_asset
WHERE fixed_asset_code = @code
LIMIT 1;";

        using var conn = (MySqlConn)_connectionFactory.CreateConnection();
        await conn.OpenAsync(ct);

        using var cmd = new MySqlCommand(sql, conn);
        AddParam(cmd, "@code", code);

        using var reader = await cmd.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
        {
            return MapFixedAsset(reader);
        }

        return null;
    }

    public async Task<int> InsertAsync(FixedAsset entity, CancellationToken ct = default)
    {
        const string sql = @"
INSERT INTO fixed_asset
(
  fixed_asset_id,
  fixed_asset_code,
  fixed_asset_name,
  fixed_asset_department_id,
  fixed_asset_type_id,
  fixed_asset_purchase_date,
  fixed_asset_quantity,
  fixed_asset_cost,
  fixed_asset_depreciation_value_year,
  created_by,
  updated_by
)
VALUES
(
  @id,
  @code,
  @name,
  @department_id,
  @type_id,
  @purchase_date,
  @qty,
  @cost,
  @dep_year,
  @created_by,
  @updated_by
);";

        using var conn = (MySqlConnector.MySqlConnection)_connectionFactory.CreateConnection();
        await conn.OpenAsync(ct);

        using var cmd = new MySqlConnector.MySqlCommand(sql, conn);

        AddParam(cmd, "@id", entity.FixedAssetId == Guid.Empty ? Guid.NewGuid().ToString() : entity.FixedAssetId.ToString());
        AddParam(cmd, "@code", entity.FixedAssetCode);
        AddParam(cmd, "@name", entity.FixedAssetName);
        AddParam(cmd, "@department_id", entity.FixedAssetDepartmentId.ToString());
        AddParam(cmd, "@type_id", entity.FixedAssetTypeId.ToString());
        AddParam(cmd, "@purchase_date", entity.FixedAssetPurchaseDate.Date);
        AddParam(cmd, "@qty", entity.FixedAssetQuantity);
        AddParam(cmd, "@cost", entity.FixedAssetCost);

        // Fix lỗi null: luôn set 0 nếu chưa có
        AddParam(cmd, "@dep_year", entity.FixedAssetDepreciationValueYear <= 0 ? 0 : entity.FixedAssetDepreciationValueYear);

        AddParam(cmd, "@created_by", entity.CreatedBy);
        AddParam(cmd, "@updated_by", entity.UpdatedBy);

        return await cmd.ExecuteNonQueryAsync(ct);
    }
    public async Task<int> UpdateAsync(FixedAsset entity, CancellationToken ct = default)
    {
        const string sql = @"
UPDATE fixed_asset
SET
  fixed_asset_code = @code,
  fixed_asset_name = @name,
  fixed_asset_department_id = @department_id,
  fixed_asset_type_id = @type_id,
  fixed_asset_purchase_date = @purchase_date,
  fixed_asset_quantity = @qty,
  fixed_asset_cost = @cost,
  updated_by = @updated_by
WHERE fixed_asset_id = @id;";

        using var conn = (MySqlConn)_connectionFactory.CreateConnection();
        await conn.OpenAsync(ct);

        using var cmd = new MySqlCommand(sql, conn);

        AddParam(cmd, "@id", entity.FixedAssetId.ToString());
        AddParam(cmd, "@code", entity.FixedAssetCode);
        AddParam(cmd, "@name", entity.FixedAssetName);
        AddParam(cmd, "@department_id", entity.FixedAssetDepartmentId.ToString());
        AddParam(cmd, "@type_id", entity.FixedAssetTypeId.ToString());
        AddParam(cmd, "@purchase_date", entity.FixedAssetPurchaseDate.Date);
        AddParam(cmd, "@qty", entity.FixedAssetQuantity);
        AddParam(cmd, "@cost", entity.FixedAssetCost);
        AddParam(cmd, "@updated_by", entity.UpdatedBy);

        return await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<int> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        const string sql = @"DELETE FROM fixed_asset WHERE fixed_asset_id = @id;";

        using var conn = (MySqlConn)_connectionFactory.CreateConnection();
        await conn.OpenAsync(ct);

        using var cmd = new MySqlCommand(sql, conn);
        AddParam(cmd, "@id", id.ToString());

        return await cmd.ExecuteNonQueryAsync(ct);
    }

    // =========================
    // Helpers
    // =========================
    private static void AddParam(MySqlCommand cmd, string name, object? value)
    {
        cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);
    }

    private static FixedAsset MapFixedAsset(IDataRecord r)
    {
        return new FixedAsset
        {
            FixedAssetId = Guid.Parse(r["fixed_asset_id"].ToString()!),
            FixedAssetCode = r["fixed_asset_code"].ToString() ?? "",
            FixedAssetName = r["fixed_asset_name"].ToString() ?? "",

            FixedAssetDepartmentId = Guid.Parse(r["fixed_asset_department_id"].ToString()!),
            FixedAssetTypeId = Guid.Parse(r["fixed_asset_type_id"].ToString()!),

            FixedAssetPurchaseDate = Convert.ToDateTime(r["fixed_asset_purchase_date"]),
            FixedAssetUsingYear = Convert.ToInt32(r["fixed_asset_using_year"]),
            FixedAssetTrackingYear = Convert.ToInt32(r["fixed_asset_tracking_year"]),

            FixedAssetQuantity = Convert.ToInt32(r["fixed_asset_quantity"]),
            FixedAssetCost = Convert.ToDecimal(r["fixed_asset_cost"]),
            FixedAssetDepreciationValueYear = Convert.ToDecimal(r["fixed_asset_depreciation_value_year"]),

            CreatedAt = Convert.ToDateTime(r["created_at"]),
            UpdatedAt = Convert.ToDateTime(r["updated_at"]),
            CreatedBy = r["created_by"] == DBNull.Value ? null : r["created_by"].ToString(),
            UpdatedBy = r["updated_by"] == DBNull.Value ? null : r["updated_by"].ToString()
        };
    }
}
