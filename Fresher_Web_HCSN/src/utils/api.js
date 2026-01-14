// Mô tả: Khởi tạo axios client dùng chung cho toàn dự án
// Ngày tạo: 2026-01-14
import axios from "axios"

const http = axios.create({
  baseURL: "http://localhost:7029/", // ví dụ: http://localhost:5159/api
  timeout: 15000,
})

// Interceptor (tuỳ chọn): log lỗi / gắn token sau này
http.interceptors.response.use(
  (res) => res,
  (err) => {
    console.error("API Error:", err?.response?.data || err.message)
    return Promise.reject(err)
  }
)

export default http
