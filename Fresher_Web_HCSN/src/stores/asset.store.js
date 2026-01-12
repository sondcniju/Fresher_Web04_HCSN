import { defineStore } from 'pinia'
import { ASSET_TYPES, DEPARTMENTS } from '@/constants/masterData'
import { todayISO, yearFromISO } from '@/utils/date'

const LS_KEY = 'QLTS_ASSETS_V1'

function nextAssetCode(existingCodes) {
  // Mã tài sản auto tăng đơn giản: TS000001...
  // Chức năng: Tạo mã tài sản mới theo số lớn nhất hiện có
  // Ngày tạo: 2026-01-12
  const nums = existingCodes
    .map(c => String(c).replace(/\D/g, ''))
    .map(n => Number(n))
    .filter(n => Number.isFinite(n))

  const max = nums.length ? Math.max(...nums) : 0
  const next = max + 1
  return `TS${String(next).padStart(6, '0')}`
}

function calcDepreciationValueYear(originalPrice, rate) {
  // Chức năng: Tính giá trị hao mòn năm = Nguyên giá * Tỷ lệ hao mòn / 100
  // Ngày tạo: 2026-01-12
  const op = Number(originalPrice) || 0
  const r = Number(rate) || 0
  return (op * r) / 100
}

export const useAssetStore = defineStore('asset', {
  state: () => ({
    assets: [],
    filters: {
      keyword: '',
      departmentCode: '',
      assetTypeCode: '',
    },
    pagination: {
      page: 1,
      pageSize: 20,
    },
  }),

  getters: {
    filteredAssets(state) {
      const kw = String(state.filters.keyword || '').toLowerCase().trim()
      const dep = state.filters.departmentCode
      const type = state.filters.assetTypeCode

      return state.assets.filter(a => {
        const okKw =
          !kw ||
          String(a.assetCode || '').toLowerCase().includes(kw) ||
          String(a.assetName || '').toLowerCase().includes(kw)
        const okDep = !dep || a.departmentCode === dep
        const okType = !type || a.assetTypeCode === type
        return okKw && okDep && okType
      })
    },

    pagedAssets(state) {
      const list = this.filteredAssets
      const start = (state.pagination.page - 1) * state.pagination.pageSize
      return list.slice(start, start + state.pagination.pageSize)
    },

    totalFiltered() {
      return this.filteredAssets.length
    },
  },

  actions: {
    // Chức năng: Load dữ liệu tài sản từ LocalStorage
    // Ngày tạo: 2026-01-12
    load() {
      try {
        const raw = localStorage.getItem(LS_KEY)
        this.assets = raw ? JSON.parse(raw) : []
      } catch {
        this.assets = []
      }
    },

    // Chức năng: Lưu dữ liệu tài sản vào LocalStorage
    // Ngày tạo: 2026-01-12
    save() {
      localStorage.setItem(LS_KEY, JSON.stringify(this.assets))
    },

    // Chức năng: Tạo form mặc định (dùng khi thêm mới/nhân bản)
    // Ngày tạo: 2026-01-12
    createDefaultAsset() {
      const codes = this.assets.map(x => x.assetCode)
      const assetCode = nextAssetCode(codes)
      const purchaseDate = todayISO()
      const y = yearFromISO(purchaseDate)

      return {
        id: crypto.randomUUID(),
        assetCode,
        assetName: '',

        departmentCode: '',
        departmentName: '',

        assetTypeCode: '',
        assetTypeName: '',

        purchaseDate,
        usingYear: y,
        trackingStartYear: y,

        lifeYears: '',
        depreciationRate: '',
        quantity: 1,

        originalPrice: 0,
        depreciationValueYear: 0,
      }
    },

    // Chức năng: Áp dữ liệu theo bộ phận đã chọn (tự điền tên bộ phận)
    // Ngày tạo: 2026-01-12
    applyDepartment(asset, departmentCode) {
      const dep = DEPARTMENTS.find(x => x.code === departmentCode)
      asset.departmentCode = departmentCode || ''
      asset.departmentName = dep ? dep.name : ''
    },

    // Chức năng: Áp dữ liệu theo loại tài sản đã chọn (tự điền số năm sử dụng + tỷ lệ hao mòn + tên loại)
    // Ngày tạo: 2026-01-12
    applyAssetType(asset, assetTypeCode) {
      const t = ASSET_TYPES.find(x => x.code === assetTypeCode)
      asset.assetTypeCode = assetTypeCode || ''
      asset.assetTypeName = t ? t.name : ''
      asset.lifeYears = t ? t.lifeYears : ''
      asset.depreciationRate = t ? t.depreciationRate : ''
      asset.depreciationValueYear = calcDepreciationValueYear(asset.originalPrice, asset.depreciationRate)
    },

    // Chức năng: Áp dữ liệu theo ngày mua (tự điền năm sử dụng + năm bắt đầu theo dõi)
    // Ngày tạo: 2026-01-12
    applyPurchaseDate(asset, purchaseDateISO) {
      asset.purchaseDate = purchaseDateISO || ''
      const y = yearFromISO(asset.purchaseDate)
      asset.usingYear = y
      asset.trackingStartYear = y
    },

    // Chức năng: Tạo mới tài sản
    // Ngày tạo: 2026-01-12
    create(asset) {
      this.assets.unshift({ ...asset })
      this.save()
    },

    // Chức năng: Cập nhật tài sản theo id
    // Ngày tạo: 2026-01-12
    update(id, patch) {
      const idx = this.assets.findIndex(x => x.id === id)
      if (idx === -1) return
      this.assets[idx] = { ...this.assets[idx], ...patch }
      this.save()
    },

    // Chức năng: Xóa 1 tài sản theo id
    // Ngày tạo: 2026-01-12
    remove(id) {
      this.assets = this.assets.filter(x => x.id !== id)
      this.save()
    },

    // Chức năng: Nhân bản tài sản (copy + tự tăng mã)
    // Ngày tạo: 2026-01-12
    duplicate(id) {
      const found = this.assets.find(x => x.id === id)
      if (!found) return null

      const clone = { ...found }
      clone.id = crypto.randomUUID()
      clone.assetCode = nextAssetCode(this.assets.map(x => x.assetCode))

      this.assets.unshift(clone)
      this.save()
      return clone
    },

    // Chức năng: Kiểm tra trùng mã tài sản (dùng cho validate)
    // Ngày tạo: 2026-01-12
    isDuplicateCode(assetCode, ignoreId = null) {
      const code = String(assetCode || '').trim()
      if (!code) return false
      return this.assets.some(x => x.assetCode === code && x.id !== ignoreId)
    },
  },
})
