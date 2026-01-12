// Mô tả: Danh mục dùng chung (Bộ phận sử dụng, Loại tài sản)
// Ngày tạo: 2026-01-12

export const DEPARTMENTS = Object.freeze([
  { code: '01', name: 'Ban Giám hiệu' },
  { code: '02', name: 'Phòng Hành chính - Quản trị' },
  { code: '03', name: 'Phòng Tài vụ' },
  { code: '04', name: 'Phòng Đào tạo' },
  { code: '05', name: 'Tổ chuyên môn (Giáo viên bộ môn)' },
])

export const ASSET_TYPES = Object.freeze([
  { code: '1', name: 'Nhà, công trình xây dựng', lifeYears: 80, depreciationRate: 1.25 },
  { code: '2', name: 'Vật kiến trúc', lifeYears: 20, depreciationRate: 5.0 },
  { code: '3', name: 'Xe ô tô', lifeYears: 15, depreciationRate: 6.67 },
  { code: '4', name: 'Phương tiện vận tải khác (ngoài xe ô tô)', lifeYears: 20, depreciationRate: 5.0 },
  { code: '5', name: 'Máy móc, thiết bị', lifeYears: 5, depreciationRate: 20.0 },
  { code: '6', name: 'Cây lâu năm, súc vật làm việc và/hoặc cho sản phẩm', lifeYears: 4, depreciationRate: 25.0 },
  { code: '7', name: 'Tài sản cố định hữu hình khác', lifeYears: 10, depreciationRate: 10.0 },
])
