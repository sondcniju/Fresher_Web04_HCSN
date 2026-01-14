// Mô tả: Service gọi API tài sản cố định
// Ngày tạo: 2026-01-14

import http from "@/utils/api"

// Enum endpoint (dễ maintain)
export const AssetApi = Object.freeze({
  Assets: "/fixed-assets",
  Departments: "/fixed-asset-departments",
  Types: "/fixed-asset-types",
})

export const assetService = {
  // Lấy danh sách (có filter)
  async getList(params) {
    // params: { keyword, year, assetTypeId, departmentId, page, pageSize }
    const res = await http.get(AssetApi.Assets, { params })
    return res.data
  },

  async getById(id) {
    const res = await http.get(`${AssetApi.Assets}/${id}`)
    return res.data
  },

  async create(payload) {
    const res = await http.post(AssetApi.Assets, payload)
    return res.data
  },

  async update(id, payload) {
    const res = await http.put(`${AssetApi.Assets}/${id}`, payload)
    return res.data
  },

  async remove(id) {
    const res = await http.delete(`${AssetApi.Assets}/${id}`)
    return res.data
  },

  async getDepartments() {
    const res = await http.get(AssetApi.Departments)
    return res.data
  },

  async getTypes() {
    const res = await http.get(AssetApi.Types)
    return res.data
  },
}
