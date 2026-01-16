import http from "@/utils/api"

export const assetService = {
  // GET ALL
  async getAsset() {
    const res = await http.get("/api/v1/fixed-assets/GetAll")
     return res.data  
  },
    // GET department
  async getDepartment() {
    const res = await http.get("/api/v1/fixed-asset-department")
     return res.data  
  },
    // GET type
  async getType() {
    const res = await http.get("/api/v1/fixed-asset-type")
     return res.data  
  },
    // combobox departments
  async getDepartmentCombobox() {
    const res = await http.get("/api/v1/fixed-asset-department/combobox")
    return res.data
  },

  // combobox types
  async getTypeCombobox() {
    const res = await http.get("/api/v1/fixed-asset-type/combobox")
    return res.data
  },

  // INSERT
  async create(payload) {
    const res = await http.post("/api/v1/fixed-assets/Insert", payload)
    return res.data
  },

  // UPDATE
  async update(id, payload) {
    const res = await http.put(`/api/v1/fixed-assets/Update:${id}`, payload)
    return res.data
  },

  // DELETE
  async remove(id) {
    const res = await http.delete(`/api/v1/fixed-assets/Delete:${id}`)
    return res.data
  },
}
