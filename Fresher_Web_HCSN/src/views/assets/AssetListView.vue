<script setup>
// Mô tả: Màn hình Danh sách tài sản (giữ nguyên phần tìm kiếm + thêm tài sản, dữ liệu lấy từ LocalStorage để chuẩn bị gắn API)
// Ngày tạo: 2026-01-14
import AssetPopup from "@/components/popup/AssetPopup.vue"
import { computed, onMounted, reactive, ref, watch } from "vue"

// ====== LocalStorage Keys ======
const LS_ASSET_KEY = "misa_assets_v1"
const LS_COL_KEY = "misa_assets_colwidth_v1"

// ====== Filters (giữ nguyên phần tìm kiếm) ======
const year = ref(2021)
const keyword = ref("")
const assetType = ref("")
const department = ref("")

// ====== Data ======
const rows = ref([])

// widths theo thứ tự cột (checkbox, STT, mã, tên, loại, bộ phận, số lượng, nguyên giá, HM/KH, còn lại, chức năng)
const defaultColWidths = [42, 52, 150, 180, 160, 220, 90, 120, 120, 120, 90]
const colWidths = reactive([...defaultColWidths])

const selectedId = ref(null)

// ====== Popup state ======
const isPopupOpen = ref(false) // dùng v-model với AssetPopup
const popupMode = ref("create") // "create" | "edit"
const editingAsset = ref(null)

// ====== Helpers ======
function readJson(key, fallback) {
  try {
    const raw = localStorage.getItem(key)
    if (!raw) return fallback
    return JSON.parse(raw)
  } catch {
    return fallback
  }
}
function writeJson(key, value) {
  localStorage.setItem(key, JSON.stringify(value))
}
function fmtMoney(v) {
  return (Number(v) || 0).toLocaleString("vi-VN")
}

// ====== Seed data (tạo 1 lần) ======
function buildMockAssets() {
  const uid = () =>
    globalThis.crypto?.randomUUID
      ? crypto.randomUUID()
      : String(Date.now()) + Math.random().toString(16).slice(2)

  return [
    { id: uid(), code: "55H7WN72/2022", name: "Dell Inspiron 3467", typeName: "Máy vi tính xách tay", department: "Phòng Hành chính Kế toán", qty: 1, cost: 20000000, dep: 894000, remain: 19106000 },
    { id: uid(), code: "MXT88618", name: "Máy tính xách tay Fujitsu", typeName: "Máy vi tính xách tay", department: "Phòng Hành chính Kế toán", qty: 1, cost: 10000000, dep: 1225000, remain: 8775000 },
    { id: uid(), code: "37H7WN72/2022", name: "Dell Inspiron 3467", typeName: "Máy vi tính xách tay", department: "Phòng Hành chính Kế toán", qty: 4, cost: 40000000, dep: 1730000, remain: 38270000 },
    { id: uid(), code: "MXT8866", name: "Máy tính xách tay Fujitsu", typeName: "Máy vi tính xách tay", department: "Phòng Thư ký", qty: 1, cost: 5000000, dep: 1646000, remain: 3354000 },
    { id: uid(), code: "14H7WN72/2019", name: "Dell Latitude E 5450", typeName: "Máy vi tính xách tay", department: "Phòng Hành chính Kế toán", qty: 1, cost: 10000000, dep: 2456000, remain: 7544000 },
    { id: uid(), code: "D8PQ3F2/2017", name: "DELL Inspiron 3467", typeName: "Máy vi tính xách tay", department: "Phòng Hành chính Kế toán", qty: 20, cost: 50000000, dep: 913000, remain: 49087000 },
    { id: uid(), code: "MXT8869", name: "Máy tính xách tay Fujitsu", typeName: "Máy vi tính xách tay", department: "Phòng Hành chính Kế toán", qty: 1, cost: 50000000, dep: 3929000, remain: 46071000 },
    { id: uid(), code: "49H7WN72/2022", name: "Dell Inspiron 3467", typeName: "Máy vi tính xách tay", department: "Phòng Tài chính Tổng hợp", qty: 1, cost: 4000000, dep: 432000, remain: 3568000 },
    { id: uid(), code: "33H7WN72/2022", name: "Dell Inspiron 3467", typeName: "Máy vi tính xách tay", department: "Phòng Tài chính Tổng hợp", qty: 1, cost: 20000000, dep: 3400000, remain: 16600000 },
    { id: uid(), code: "22H7WN72/2019", name: "Dell Latitude E 5450", typeName: "Máy vi tính xách tay", department: "Phòng Tài chính Tổng hợp", qty: 1, cost: 40000000, dep: 3091000, remain: 36909000 },
    { id: uid(), code: "MXT88617", name: "Máy vi tính xách tay Fujitsu", typeName: "Máy vi tính xách tay", department: "Phòng Tài chính Tổng hợp", qty: 1, cost: 40000000, dep: 1789000, remain: 38211000 },
    { id: uid(), code: "50H7WN72/2022", name: "Dell Inspiron 3467", typeName: "Máy vi tính xách tay", department: "Phòng Tài chính Tổng hợp", qty: 1, cost: 20000000, dep: 1521000, remain: 18479000 },
  ]
}

// ====== Data layer (để sau thay bằng API) ======
const departments = ref([])
const assetTypes = ref([])

async function loadCategories() {
  const [depRes, typeRes] = await Promise.all([
    assetService.getDepartments(),
    assetService.getTypes(),
  ])

  // tuỳ backend trả về list hay {data:[]}
  departments.value = depRes?.data ?? depRes ?? []
  assetTypes.value = typeRes?.data ?? typeRes ?? []
}
async function loadAssets() {
  const params = {
    year: year.value,
    keyword: keyword.value?.trim(),
    assetTypeId: assetType.value || null,
    departmentId: department.value || null,
    page: 1,
    pageSize: 50,
  }

  const res = await assetService.getList(params) // service đã có getList :contentReference[oaicite:3]{index=3}
  rows.value = res?.data ?? res ?? []
}

async function createAsset(payload) {
  const data = readJson(LS_ASSET_KEY, [])
  const uid = () =>
    globalThis.crypto?.randomUUID
      ? crypto.randomUUID()
      : String(Date.now()) + Math.random().toString(16).slice(2)

  const newItem = { id: uid(), ...payload }
  const next = [newItem, ...data]
  writeJson(LS_ASSET_KEY, next)
  rows.value = next
  selectedId.value = newItem.id
  return newItem
}

async function updateAsset(id, patch) {
  const data = readJson(LS_ASSET_KEY, [])
  const next = data.map((x) => (x.id === id ? { ...x, ...patch } : x))
  writeJson(LS_ASSET_KEY, next)
  rows.value = next
  selectedId.value = id
}

// ====== Popup actions ======
function onAddAsset() {
  popupMode.value = "create"
  editingAsset.value = null
  isPopupOpen.value = true
}

function openEdit(row) {
  popupMode.value = "edit"
  // clone để tránh sửa trực tiếp dữ liệu table khi chưa bấm lưu
  editingAsset.value = { ...row }
  isPopupOpen.value = true
}

async function handleSave(payload) {
  if (popupMode.value === "create") {
    await assetService.create(payload) // :contentReference[oaicite:4]{index=4}
  } else {
    const id = editingAsset.value?.fixedAssetId || editingAsset.value?.id
    await assetService.update(id, payload) // :contentReference[oaicite:5]{index=5}
  }
  await loadAssets()
}


// ====== Filtered rows ======
const filteredRows = computed(() => {
  const kw = keyword.value.trim().toLowerCase()
  return rows.value.filter((r) => {
    const matchKw =
      !kw ||
      String(r.code).toLowerCase().includes(kw) ||
      String(r.name).toLowerCase().includes(kw) ||
      String(r.department).toLowerCase().includes(kw)
    const matchType = !assetType.value || r.typeName === assetType.value
    const matchDept = !department.value || r.department === department.value
    return matchKw && matchType && matchDept
  })
})

// dropdown options (demo) lấy từ dữ liệu
const assetTypeOptions = computed(() => {
  const set = new Set(rows.value.map((x) => x.typeName).filter(Boolean))
  return ["", ...Array.from(set)]
})
const deptOptions = computed(() => {
  const set = new Set(rows.value.map((x) => x.department).filter(Boolean))
  return ["", ...Array.from(set)]
})

// ====== Totals (footer bám đúng cột) ======
const totalQty = computed(() => filteredRows.value.reduce((s, r) => s + (Number(r.qty) || 0), 0))
const totalCost = computed(() => filteredRows.value.reduce((s, r) => s + (Number(r.cost) || 0), 0))
const totalDep = computed(() => filteredRows.value.reduce((s, r) => s + (Number(r.dep) || 0), 0))
const totalRemain = computed(() => filteredRows.value.reduce((s, r) => s + (Number(r.remain) || 0), 0))

// ====== Column width persistence ======
function loadColWidths() {
  const saved = readJson(LS_COL_KEY, null)
  if (Array.isArray(saved) && saved.length === defaultColWidths.length) {
    saved.forEach((w, i) => (colWidths[i] = w))
  }
}
watch(
  () => [...colWidths],
  (val) => writeJson(LS_COL_KEY, val),
  { deep: true }
)

// ====== Resize columns ======
let resizing = null
function onResizeDown(e, colIndex) {
  e.preventDefault()
  resizing = { colIndex, startX: e.clientX, startW: colWidths[colIndex] }
  window.addEventListener("mousemove", onResizeMove)
  window.addEventListener("mouseup", onResizeUp)
}
function onResizeMove(e) {
  if (!resizing) return
  const dx = e.clientX - resizing.startX
  const MIN = 50
  colWidths[resizing.colIndex] = Math.max(MIN, resizing.startW + dx)
}
function onResizeUp() {
  resizing = null
  window.removeEventListener("mousemove", onResizeMove)
  window.removeEventListener("mouseup", onResizeUp)
}

// ====== init ======
onMounted(async () => {
  loadColWidths?.() // nếu bạn có
  await loadCategories()
  await loadAssets()
})
// chọn dòng
function selectRow(id) {
  selectedId.value = id
}
</script>

<template>
  <section class="page">
    <!-- Filters + actions -->
    <div class="toolbar">
      <div class="filters">
        <div class="field">
          <span class="prefix">🔎</span>
          <input v-model="keyword" class="input" placeholder="Tìm kiếm tài sản" />
        </div>

        <div class="field">
          <span class="prefix">⎘</span>
          <select v-model="assetType" class="input select">
            <option value="">Loại tài sản</option>
            <option v-for="t in assetTypeOptions" :key="t" :value="t" v-if="t">{{ t }}</option>
          </select>
        </div>

        <div class="field">
          <span class="prefix">⎘</span>
          <select v-model="department" class="input select">
            <option value="">Bộ phận sử dụng</option>
            <option v-for="d in deptOptions" :key="d" :value="d" v-if="d">{{ d }}</option>
          </select>
        </div>
      </div>

      <div class="actions">
        <button class="btn primary" type="button" @click="onAddAsset">+ Thêm tài sản</button>
        <button class="btn icon" type="button" title="Xuất">⤓</button>
        <button class="btn icon danger" type="button" title="Xoá">🗑</button>
      </div>
    </div>

    <!-- Table -->
    <div class="table-wrap">
      <table class="asset-table">
        <colgroup>
          <col v-for="(w, i) in colWidths" :key="i" :style="{ width: w + 'px' }" />
        </colgroup>

        <thead>
          <tr>
            <th class="th-center">
              <input class="chk" type="checkbox" />
              <span class="col-resizer" @mousedown="onResizeDown($event, 0)"></span>
            </th>

            <th class="th-center">
              STT
              <span class="col-resizer" @mousedown="onResizeDown($event, 1)"></span>
            </th>

            <th>
              Mã tài sản
              <span class="col-resizer" @mousedown="onResizeDown($event, 2)"></span>
            </th>

            <th>
              Tên tài sản
              <span class="col-resizer" @mousedown="onResizeDown($event, 3)"></span>
            </th>

            <th>
              Loại tài sản
              <span class="col-resizer" @mousedown="onResizeDown($event, 4)"></span>
            </th>

            <th>
              Bộ phận sử dụng
              <span class="col-resizer" @mousedown="onResizeDown($event, 5)"></span>
            </th>

            <th class="th-right">
              Số lượng
              <span class="col-resizer" @mousedown="onResizeDown($event, 6)"></span>
            </th>

            <th class="th-right">
              Nguyên giá
              <span class="col-resizer" @mousedown="onResizeDown($event, 7)"></span>
            </th>

            <th class="th-right">
              HM/KH lũy kế
              <span class="col-resizer" @mousedown="onResizeDown($event, 8)"></span>
            </th>

            <th class="th-right">
              Giá trị còn lại
              <span class="col-resizer" @mousedown="onResizeDown($event, 9)"></span>
            </th>

            <th class="th-center">
              Chức năng
              <span class="col-resizer" @mousedown="onResizeDown($event, 10)"></span>
            </th>
          </tr>
        </thead>

        <tbody>
          <tr v-for="(r, idx) in filteredRows" :key="r.id" :class="{ selected: r.id === selectedId }"
            @click="selectRow(r.id)">
            <td class="td-center"><input class="chk" type="checkbox" @click.stop /></td>
            <td class="td-center">{{ idx + 1 }}</td>
            <td class="td-ellipsis">{{ r.code }}</td>
            <td class="td-ellipsis">{{ r.name }}</td>
            <td class="td-ellipsis">{{ r.typeName }}</td>
            <td class="td-ellipsis">{{ r.department }}</td>
            <td class="td-right">{{ r.qty }}</td>
            <td class="td-right">{{ fmtMoney(r.cost) }}</td>
            <td class="td-right">{{ fmtMoney(r.dep) }}</td>
            <td class="td-right">{{ fmtMoney(r.remain) }}</td>

            <td class="td-center" @click.stop>
              <button class="icon-action" type="button" title="Sửa" @click="openEdit(r)">✎</button>
            </td>
          </tr>
        </tbody>

        <tfoot>
          <tr class="tfoot-row">
            <td colspan="6" class="tfoot-left">Tổng số: {{ filteredRows.length }} bản ghi</td>
            <td class="td-right tfoot-strong">{{ totalQty }}</td>
            <td class="td-right tfoot-strong">{{ fmtMoney(totalCost) }}</td>
            <td class="td-right tfoot-strong">{{ fmtMoney(totalDep) }}</td>
            <td class="td-right tfoot-strong">{{ fmtMoney(totalRemain) }}</td>
            <td></td>
          </tr>
        </tfoot>
      </table>
    </div>

    <!-- Popup -->
    <AssetPopup v-model="isPopupOpen" :mode="popupMode" :asset="editingAsset" @save="handleSave" />
  </section>
</template>

<style scoped>
.page {
  padding: 12px 16px;
  background: #f4f7fb;
  min-height: calc(100vh - 60px);
}

/* toolbar */
.toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 10px;
}

.filters {
  display: flex;
  align-items: center;
  gap: 10px;
}

.field {
  position: relative;
  display: inline-flex;
  align-items: center;
}

.prefix {
  position: absolute;
  left: 10px;
  font-size: 12px;
  opacity: 0.7;
}

.input {
  height: 34px;
  padding: 0 10px 0 30px;
  border: 1px solid #d8dfe8;
  border-radius: 4px;
  background: #fff;
  font-size: 12px;
  width: 220px;
  outline: none;
}

.select {
  width: 200px;
  padding-left: 30px;
}

/* actions */
.actions {
  display: flex;
  align-items: center;
  gap: 8px;
}

.btn {
  height: 34px;
  border-radius: 4px;
  border: 1px solid #d8dfe8;
  background: #fff;
  padding: 0 12px;
  font-size: 12px;
  cursor: pointer;
}

.btn.primary {
  background: #2bb5ff;
  border-color: #2bb5ff;
  color: #fff;
  font-weight: 600;
}

.btn.icon {
  width: 34px;
  padding: 0;
}

.btn.danger {
  color: #ff3b30;
}

/* Table wrap */
.table-wrap {
  border: 1px solid #e3e6ef;
  border-radius: 4px;
  overflow: auto;
  background: #fff;
}

/* Table */
.asset-table {
  width: 100%;
  border-collapse: collapse;
  table-layout: fixed;
  font-family: Roboto, Arial, sans-serif;
  font-size: 12px;
}

.asset-table thead th {
  position: relative;
  background: #f7f8fb;
  border-bottom: 1px solid #e3e6ef;
  height: 34px;
  padding: 0 10px;
  text-align: left;
  font-weight: 600;
  white-space: nowrap;
}

.asset-table td {
  border-bottom: 1px solid #eef1f6;
  height: 34px;
  padding: 0 10px;
  vertical-align: middle;
  background: #fff;
}

.td-ellipsis {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.th-center,
.td-center {
  text-align: center;
}

.th-right,
.td-right {
  text-align: right;
}

.chk {
  width: 14px;
  height: 14px;
  accent-color: #2bb5ff;
}

/* selected row giống ảnh */
.asset-table tbody tr.selected td {
  background: #d9f3ff;
}

/* Footer trong table */
.asset-table tfoot td {
  background: #fff;
  border-top: 1px solid #e3e6ef;
  border-bottom: none;
  height: 40px;
}

.tfoot-left {
  color: #111;
}

.tfoot-strong {
  font-weight: 700;
}

/* resizer kéo cột */
.col-resizer {
  position: absolute;
  top: 0;
  right: 0;
  width: 8px;
  height: 100%;
  cursor: col-resize;
  user-select: none;
}

.col-resizer:hover {
  background: rgba(43, 181, 255, 0.15);
}

/* nút icon action */
.icon-action {
  width: 28px;
  height: 28px;
  border: none;
  background: transparent;
  cursor: pointer;
  border-radius: 4px;
}

.icon-action:hover {
  background: rgba(43, 181, 255, 0.12);
}
</style>
