<script setup>
// Mô tả: Danh sách tài sản - chỉ xử lý bảng dữ liệu và thao tác CRUD trên grid
// Ngày tạo: 2026-01-12

import { computed, onMounted, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useAssetStore } from '@/stores/asset.store'
import { formatMoney } from '@/utils/format'
import { formatDateVNFromISO } from '@/utils/date'

const router = useRouter()
const store = useAssetStore()

// ====== State bảng ======
const keyword = ref('')
const page = ref(1)
const pageSize = ref(20)

onMounted(() => {
  // Chức năng: Load dữ liệu khi mở bảng danh sách
  // Ngày tạo: 2026-01-12
  store.load()
  store.pagination.page = page.value
  store.pagination.pageSize = pageSize.value
})

watch(keyword, () => {
  // Chức năng: Tìm kiếm theo mã/tên tài sản
  // Ngày tạo: 2026-01-12
  store.filters.keyword = keyword.value
  page.value = 1
  store.pagination.page = 1
})

watch([page, pageSize], () => {
  // Chức năng: Đồng bộ phân trang vào store
  // Ngày tạo: 2026-01-12
  store.pagination.page = page.value
  store.pagination.pageSize = pageSize.value
})

const rows = computed(() => store.pagedAssets)
const total = computed(() => store.totalFiltered)
const totalPages = computed(() => Math.max(1, Math.ceil(total.value / (pageSize.value || 20))))

function onAdd() {
  // Chức năng: Mở form thêm mới tài sản
  // Ngày tạo: 2026-01-12
  router.push('/tai-san-moi')
}

function onEdit(row) {
  // Chức năng: Mở form sửa tài sản
  // Ngày tạo: 2026-01-12
  router.push(`/tai-san/${row.id}?mode=EDIT`)
}

function onDuplicate(row) {
  // Chức năng: Nhân bản tài sản (copy + tự tăng mã) và mở bản ghi mới
  // Ngày tạo: 2026-01-12
  const clone = store.duplicate(row.id)
  if (clone) router.push(`/tai-san/${clone.id}?mode=EDIT`)
}

function onDelete(row) {
  // Chức năng: Xóa 1 tài sản
  // Ngày tạo: 2026-01-12
  if (confirm(`Xóa tài sản "${row.assetName}"?`)) store.remove(row.id)
}

function prevPage() {
  // Chức năng: Về trang trước
  // Ngày tạo: 2026-01-12
  if (page.value > 1) page.value--
}

function nextPage() {
  // Chức năng: Sang trang sau
  // Ngày tạo: 2026-01-12
  if (page.value < totalPages.value) page.value++
}
</script>

<template>
  <div class="content">
    <!-- Toolbar tối thiểu cho bảng -->
    <div class="toolbar">
      <button class="primary" type="button" @click="onAdd">+ Thêm tài sản</button>

      <div class="spacer"></div>

      <div class="field">
        <label>Từ khóa</label>
        <input v-model="keyword" placeholder="Tìm theo mã/tên tài sản" />
      </div>
    </div>

    <div class="meta">
      <div>Tổng: <b>{{ total }}</b></div>

      <div class="paging">
        <button type="button" @click="prevPage" :disabled="page === 1">Trước</button>
        <span>Trang {{ page }} / {{ totalPages }}</span>
        <button type="button" @click="nextPage" :disabled="page === totalPages">Sau</button>

        <select v-model.number="pageSize">
          <option :value="10">10</option>
          <option :value="20">20</option>
          <option :value="50">50</option>
        </select>
      </div>
    </div>

    <table class="grid">
      <thead>
        <tr>
          <th style="width: 130px;">Mã tài sản</th>
          <th>Tên tài sản</th>
          <th style="width: 220px;">Bộ phận sử dụng</th>
          <th style="width: 220px;">Loại tài sản</th>
          <th style="width: 120px; text-align:center;">Ngày mua</th>
          <th style="width: 150px; text-align:right;">Nguyên giá</th>
          <th style="width: 260px;">Thao tác</th>
        </tr>
      </thead>

      <tbody>
        <tr v-if="rows.length === 0">
          <td colspan="7" class="empty">Không có dữ liệu</td>
        </tr>

        <tr v-for="r in rows" :key="r.id">
          <td>{{ r.assetCode }}</td>
          <td>{{ r.assetName }}</td>
          <td>{{ r.departmentName }}</td>
          <td>{{ r.assetTypeName }}</td>
          <td style="text-align:center;">{{ formatDateVNFromISO(r.purchaseDate) }}</td>
          <td style="text-align:right;">{{ formatMoney(r.originalPrice) }}</td>
          <td class="actions">
            <button type="button" @click="onEdit(r)">Sửa</button>
            <button type="button" @click="onDuplicate(r)">Nhân bản</button>
            <button type="button" class="danger" @click="onDelete(r)">Xóa</button>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style scoped>
.content {
  padding: 16px;
}

/* toolbar */
.toolbar {
  display: flex;
  align-items: end;
  gap: 12px;
  margin-bottom: 10px;
}
.primary {
  border: none;
  background: #2fb3e0;
  color: #fff;
  padding: 10px 12px;
  border-radius: 6px;
  cursor: pointer;
}
.primary:hover { opacity: 0.95; }
.spacer { flex: 1; }
.field {
  display: flex;
  flex-direction: column;
  gap: 4px;
}
.field input {
  height: 36px;
  padding: 0 10px;
  border: 1px solid #e5e8ef;
  border-radius: 6px;
  outline: none;
  background: #fff;
}

/* meta */
.meta {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin: 8px 0 12px;
}
.paging {
  display: flex;
  align-items: center;
  gap: 8px;
}
.paging button {
  height: 32px;
  padding: 0 10px;
  border: 1px solid #e5e8ef;
  border-radius: 6px;
  background: #fff;
  cursor: pointer;
}
.paging button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
.paging select {
  height: 32px;
  border: 1px solid #e5e8ef;
  border-radius: 6px;
  background: #fff;
}

/* grid */
.grid {
  width: 100%;
  border-collapse: collapse;
  background: #fff;
  border: 1px solid #eef1f6;
}
.grid th, .grid td {
  border-bottom: 1px solid #eef1f6;
  padding: 10px 12px;
  font-size: 14px;
  color: #1b2b3a;
}
.grid thead th {
  background: #f7f9fd;
  font-weight: 700;
}
.empty {
  text-align: center;
  padding: 24px 0;
  color: #6b7a90;
}
.actions {
  display: flex;
  gap: 8px;
}
.actions button {
  border: 1px solid #e5e8ef;
  background: #fff;
  height: 32px;
  padding: 0 10px;
  border-radius: 6px;
  cursor: pointer;
}
.actions .danger {
  border-color: #ffd6d6;
  color: #c62828;
}
</style>
