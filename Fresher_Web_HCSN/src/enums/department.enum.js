// Mô tả: Enum map Bộ phận sử dụng (sinh từ API)
// Ngày tạo: 2026-01-15

/**
 * Tạo map:
 *  code -> name
 *  id   -> name
 */
export function buildDepartmentEnum(list = []) {
  const byCode = {}
  const byId = {}

  list.forEach((d) => {
    byCode[d.fixedAssetDepartmentCode] = d.fixedAssetDepartmentName
    byId[d.fixedAssetDepartmentId] = d.fixedAssetDepartmentName
  })

  return {
    byCode,
    byId,
  }
}
