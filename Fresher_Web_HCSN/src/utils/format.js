// Mô tả: Format số/tiền hiển thị theo dạng 1.000.000
// Ngày tạo: 2026-01-12

export function toNumber(value) {
  if (value === null || value === undefined || value === '') return 0
  if (typeof value === 'number') return value
  // bỏ dấu chấm ngăn cách
  const cleaned = String(value).replace(/\./g, '').replace(/,/g, '.')
  const n = Number(cleaned)
  return Number.isFinite(n) ? n : 0
}

export function formatThousands(value) {
  const n = toNumber(value)
  return n.toLocaleString('vi-VN') // vi-VN dùng dấu chấm ngăn cách
}

export function formatMoney(value) {
  return formatThousands(value)
}
