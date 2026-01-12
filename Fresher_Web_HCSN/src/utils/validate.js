// Mô tả: Validate đơn giản cho form
// Ngày tạo: 2026-01-12

export function isRequired(v) {
  return v !== null && v !== undefined && String(v).trim() !== ''
}

export function isPositiveInt(v) {
  const n = Number(v)
  return Number.isInteger(n) && n > 0
}

export function isValidISODate(v) {
  // input type="date" cho ra yyyy-mm-dd
  return /^\d{4}-\d{2}-\d{2}$/.test(String(v || ''))
}
