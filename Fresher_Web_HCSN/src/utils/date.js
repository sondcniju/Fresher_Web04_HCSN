// Mô tả: Hàm xử lý ngày tháng (dd/MM/yyyy)
// Ngày tạo: 2026-01-12

export function todayISO() {
  // yyyy-mm-dd để dùng cho input type="date"
  const d = new Date()
  const yyyy = d.getFullYear()
  const mm = String(d.getMonth() + 1).padStart(2, '0')
  const dd = String(d.getDate()).padStart(2, '0')
  return `${yyyy}-${mm}-${dd}`
}

export function yearFromISO(isoDate) {
  if (!isoDate) return ''
  return Number(String(isoDate).slice(0, 4))
}

export function formatDateVNFromISO(isoDate) {
  if (!isoDate) return ''
  const [yyyy, mm, dd] = String(isoDate).split('-')
  if (!yyyy || !mm || !dd) return ''
  return `${dd}/${mm}/${yyyy}`
}
