<script setup>
// Mô tả: Màn hình Danh sách tài sản (gắn API để lấy dữ liệu hiển thị lên table, giữ nguyên style hiện tại)
// Ngày tạo: 2026-01-15
import AssetPopup from "@/components/popup/AssetPopup.vue"
import { computed, onMounted, reactive, ref, watch } from "vue"
import { assetService } from "@/service/AssetService"

// ====== LocalStorage Keys (chỉ lưu width cột) ======
const LS_COL_KEY = "misa_assets_colwidth_v1"

// ====== Filters (giữ nguyên phần tìm kiếm) ======
const year = ref(2021)
const keyword = ref("")
const assetTypeId = ref("") // đang filter theo typeName (client-side)
const departmentId = ref("") // đang filter theo departmentName (client-side)

// ====== Data ======
const rows = ref([]) // data để render table (đã normalize theo format cũ)
const assetTypes = ref([]) // dùng cho popup dropdown
const departments = ref([])

// widths theo thứ tự cột (checkbox, STT, mã, tên, loại, bộ phận, số lượng, nguyên giá, HM/KH, còn lại, chức năng)
const defaultColWidths = [42, 52, 150, 180, 160, 220, 90, 120, 120, 120, 90]
const colWidths = reactive([...defaultColWidths])

// ✅ FIX lệch cột: khóa width thật của table = tổng colWidths
// (tránh browser tự phân phối lại cột khi table width:100%)
const tableWidth = computed(() => colWidths.reduce((s, w) => s + Number(w || 0), 0))

const selectedId = ref(null)

// ====== Popup state ======
const isPopupOpen = ref(false) // dùng v-model với AssetPopup
const popupMode = ref("create") // "create" | "edit"
const editingAsset = ref(null)
const editingId = ref(null)

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

/**
 * Mô tả: Load danh mục (bộ phận + loại tài sản) để popup dropdown dùng
 * Ngày tạo: 2026-01-15
 */
async function loadCategories() {
  const [depList, typeList] = await Promise.all([
    assetService.getDepartmentCombobox(),
    assetService.getTypeCombobox(),
  ])

  departments.value = depList ?? []
  assetTypes.value = typeList ?? []
}

const departmentMap = computed(() => {
  const map = {}
  departments.value.forEach((d) => {
    map[d.fixedAssetDepartmentId] = d.fixedAssetDepartmentName
  })
  return map
})

const assetTypeMap = computed(() => {
  const map = {}
  assetTypes.value.forEach((t) => {
    map[t.fixedAssetTypeId ?? t.id] = t.fixedAssetTypeName ?? t.name
  })
  return map
})

/**
 * Mô tả: Load danh sách tài sản từ API
 * Ngày tạo: 2026-01-15
 */
async function loadAssets() {
  try {
    const list = await assetService.getAsset()

    rows.value = (list ?? []).map((x) => {
      const departmentName = x.fixedAssetDepartmentName || departmentMap.value[x.fixedAssetDepartmentId] || ""

      const typeName = x.fixedAssetTypeName || assetTypeMap.value[x.fixedAssetTypeId] || ""

      const assetId = x.fixedAssetId ?? x.FixedAssetId ?? x.id

      return {
        typeId: x.fixedAssetTypeId,
        departmentId: x.fixedAssetDepartmentId,
        id: assetId,
        fixedAssetId: assetId,
        __raw: x,
        code: x.fixedAssetCode,
        name: x.fixedAssetName,
        typeName,
        department: departmentName,
        qty: x.fixedAssetQuantity,
        cost: x.fixedAssetCost,
        dep: x.fixedAssetDepreciationValueYear ?? 0,
        remain:
          x.fixedAssetRemainValue ??
          Math.max(0, Number(x.fixedAssetCost) - Number(x.fixedAssetDepreciationValueYear ?? 0)),
      }
    })

    selectedId.value = rows.value[0]?.id ?? null
  } catch (err) {
    console.error("Load assets error:", err)
    rows.value = []
  }
}

// ====== Popup actions ======
function openCreate() {
  popupMode.value = "create"
  editingAsset.value = null
  editingId.value = null
  isPopupOpen.value = true
}

function openEdit(row) {
  popupMode.value = "edit"
  editingId.value = row.id
  editingAsset.value = row.__raw || row
  isPopupOpen.value = true
}

/**
 * Mô tả: Nhận payload từ popup -> Insert/Update -> reload list
 * Ngày tạo: 2026-01-15
 */
async function handleSave(payload) {
  try {
    if (popupMode.value === "create") {
      await assetService.create(payload)
    } else {
      const id = editingId.value
      if (!id) throw new Error("Không có id để cập nhật")
      await assetService.update(id, payload)
    }
    await loadAssets()
  } catch (e) {
    console.error("Save error:", e)
  }
}

// Mô tả: Xoá tài sản theo dòng đang chọn
// Ngày tạo: 2026-01-15
async function handleDelete() {
  try {
    const id = selectedId.value
    if (!id) {
      alert("Vui lòng chọn 1 dòng để xoá")
      return
    }

    const ok = confirm("Bạn có chắc chắn muốn xoá tài sản này?")
    if (!ok) return

    await assetService.remove(id)

    // reset chọn + reload list
    selectedId.value = null
    await loadAssets()
  } catch (e) {
    console.error("Delete error:", e)
    alert("Xoá thất bại, vui lòng kiểm tra lại backend")
  }
}

// ====== Filtered rows (giữ nguyên logic filter client-side) ======
const filteredRows = computed(() => {
  const kw = keyword.value.trim().toLowerCase()
  return rows.value.filter((r) => {
    const matchKw =
      !kw ||
      String(r.code).toLowerCase().includes(kw) ||
      String(r.name).toLowerCase().includes(kw) ||
      String(r.department).toLowerCase().includes(kw)
    const matchType = !assetTypeId.value || r.typeId === assetTypeId.value
    const matchDept = !departmentId.value || r.departmentId === departmentId.value
    return matchKw && matchType && matchDept
  })
})

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

function selectRow(id) {
  selectedId.value = id
}

onMounted(async () => {
  loadColWidths()
  await loadCategories()
  await loadAssets()
})
</script>

<template>
  <section class="page">
    <!-- Filters + actions -->
    <div class="toolbar">
      <div class="filters">
        <div class="field">
          <span class="prefix"><span class="icon icon-search" aria-hidden="true"></span></span>
          <input v-model="keyword" class="input" placeholder="Tìm kiếm tài sản" />
        </div>

        <div class="field">
          <span class="prefix"><span class="icon icon-type" aria-hidden="true"></span></span>
          <select v-model="assetTypeId" class="input select">
            <option value="">Loại tài sản</option>
            <option v-for="t in assetTypes" :key="t.fixedAssetTypeId" :value="t.fixedAssetTypeId">
              {{ t.fixedAssetTypeName }} <!-- hoặc code tuỳ bạn -->
            </option>
          </select>
        </div>

        <div class="field">
          <span class="prefix"><span class="icon icon-type" aria-hidden="true"></span></span>
          <select v-model="departmentId" class="input select">
            <option value="">Bộ phận sử dụng</option>
            <option v-for="d in departments" :key="d.fixedAssetDepartmentId" :value="d.fixedAssetDepartmentId">
              {{ d.fixedAssetDepartmentName }} <!-- hoặc code -->
            </option>
          </select>
        </div>
      </div>

      <div class="actions">
        <button class="btn primary" type="button" @click="openCreate">+ Thêm tài sản</button>
        <button class="btn icon" type="button" title="Xuất" aria-label="Xuất">
          <span class="icon icon-export" aria-hidden="true"></span>
        </button>
        <!-- <button class="btn icon" type="button" title="Xóa" aria-label="Xóa">
          
        </button> -->
        <button class="btn icon " type="button" title="Xoá" @click="handleDelete"><span class="icon icon-delete"
            aria-hidden="true"></span>
        </button>

      </div>
    </div>

    <!-- Table -->
    <div class="table-wrap">
      <!-- ✅ FIX lệch cột: width table = tổng colWidths, min-width:100% để vẫn phủ khung -->
      <table class="asset-table" :style="{ width: tableWidth + 'px' }">
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
    <AssetPopup v-model="isPopupOpen" :mode="popupMode" :asset="editingAsset" :departments="departments"
      :assetTypes="assetTypes" @save="handleSave" />
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
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

/* Icon */
.icon {
  width: 20px;
  height: 20px;
  display: inline-flex;
  background: url(/src/assets/icons/qlts-icon.svg);
  background-repeat: no-repeat;
}

.icon-search {
  background-position: -241px -153px;
  width: 22px;
  height: 22px;
}

.icon-type {
  background-position: -243px -69px;
  width: 17px;
  height: 16px;
}

.icon-export {
  background-position: -287px -111px;
  width: 18px;
  height: 17px;
}

.icon-delete {
  background-position: -463px -111px;
  width: 18px;
  height: 18px;
}

/* Table wrap */
.table-wrap {
  border: 1px solid #e3e6ef;
  border-radius: 4px;
  overflow: auto;
  background: #fff;
  scrollbar-gutter: stable;
}

/* Table */
.asset-table {
  border-collapse: collapse;
  table-layout: fixed;
  font-family: Roboto, Arial, sans-serif;
  font-size: 12px;
  min-width: 100%;
  /* nếu tổng cột nhỏ hơn khung thì vẫn phủ full */
}

/* ✅ FIX lệch khi resize: width tính luôn padding/border */
.asset-table th,
.asset-table td {
  box-sizing: border-box;
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
