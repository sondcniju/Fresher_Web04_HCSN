<script setup>
// Mô t?: Màn hình Danh sách tài sản (gọi API để lấy dữ liệu hiển thị lên table, giữ nguyên style hiện tại)
// Ngày tạo: 2026-01-15
// Ngày sửa: 2026-01-17
import AssetPopup from "@/components/popup/AssetPopup.vue"
import { computed, nextTick, onBeforeUnmount, onMounted, reactive, ref, watch } from "vue"
import { assetService } from "@/service/AssetService"
import ConfirmDeletePopup from "@/components/popup/ConfirmDeletePopup.vue"

// ====== LocalStorage Keys (chứa luu width cột) ======
const LS_COL_KEY = "misa_assets_colwidth_v1"

// ====== Filters (server-side) ======
const year = ref(2021)
const keyword = ref("")
const assetTypeId = ref("") // lọc theo loại tài sản (server-side)
const departmentId = ref("") // lọc theo bộ phận sử dụng (server-side)
const isTypeDropdownOpen = ref(false)
const isDepartmentDropdownOpen = ref(false)
const typeFilterKeyword = ref("")
const departmentFilterKeyword = ref("")

// ====== Paging (server-side) ======
const pageNumber = ref(1)
const pageSize = ref(20)
const pageSizeOptions = [10, 20, 50, 100]

// ====== Data ======
const rows = ref([]) // data để render table (đã normalize theo format cũ)
const assetTypes = ref([]) // dùng cho popup dropdown + filter
const departments = ref([])
const totalRecords = ref(0)

// widths theo thứ tự cột (checkbox, STT, mã, tên, loại, bộ phận, số lượng, nguyên giá, HM/KH, còn lại, chức năng)
const defaultColWidths = [40, 40, 150, 180, 160, 220, 90, 120, 120, 120, 90]
const colWidths = reactive([...defaultColWidths])
const FIXED_COL_WIDTHS = { 0: 40, 1: 40 }

// FIX chọn cột: khóa width thật của table = tổng colWidths
const tableWidth = computed(() => colWidths.reduce((s, w) => s + Number(w || 0), 0))

const selectedId = ref(null)
const selectedIds = ref([]) // checkbox selections
const lastSelectedId = ref(null)
const pendingDeleteIds = ref([])

const isDeleteConfirmOpen = ref(false)
const deleteConfirmMessage = ref("")
const isDeleteInfoOnly = ref(false)
const deleteOkText = computed(() => (isDeleteInfoOnly.value ? "Đồng ý" : "Xóa"))
const deleteCancelText = computed(() => (isDeleteInfoOnly.value ? "" : "Không"))

const tableWrapRef = ref(null)
const tbodyRef = ref(null)
const contextMenu = reactive({
  visible: false,
  x: 0,
  y: 0,
  row: null,
})

// ====== Popup state ======
const isPopupOpen = ref(false) // dùng v-model với AssetPopup
const popupMode = ref("create") // "create" | "edit" | "clone"
const editingAsset = ref(null)
const editingId = ref(null)
const cloneSourceId = ref(null)

const isConfirmOpen = ref(false)
const confirmConfig = ref(null)

const toastVisible = ref(false)
let toastTimer = null

// ====== Helpers ======
// Đọc JSON từ localStorage, trả fallback nếu lỗi
function readJson(key, fallback) {
  try {
    const raw = localStorage.getItem(key)
    if (!raw) return fallback
    return JSON.parse(raw)
  } catch {
    return fallback
  }
}
// Ghi JSON vào localStorage
function writeJson(key, value) {
  localStorage.setItem(key, JSON.stringify(value))
}
// Format tiền theo locale VN
function fmtMoney(v) {
  return (Number(v) || 0).toLocaleString("vi-VN")
}
// Delay dạng Promise
function wait(ms) {
  return new Promise((resolve) => setTimeout(resolve, ms))
}
// Hiển thị toast trong 2s
function showToast() {
  toastVisible.value = true
  if (toastTimer) clearTimeout(toastTimer)
  toastTimer = window.setTimeout(() => {
    toastVisible.value = false
  }, 2000)
}

/**
 * Mô tả: Load danh mục (bộ phận + loại tài sản)  dùng cho popup dropdown & filter
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

const assetTypeOptions = computed(() =>
  (assetTypes.value || []).map((t) => ({
    id: t.fixedAssetTypeId ?? t.id,
    code: t.fixedAssetTypeCode ?? t.code ?? "",
    name: t.fixedAssetTypeName ?? t.name ?? "",
  }))
)

const departmentOptions = computed(() =>
  (departments.value || []).map((d) => ({
    id: d.fixedAssetDepartmentId ?? d.id,
    code: d.fixedAssetDepartmentCode ?? d.code ?? "",
    name: d.fixedAssetDepartmentName ?? d.name ?? "",
  }))
)

function normalizeFilterText(v) {
  return String(v || "").toLowerCase().trim()
}

const filteredAssetTypeOptions = computed(() => {
  const kw = normalizeFilterText(typeFilterKeyword.value)
  if (!kw) return assetTypeOptions.value
  return assetTypeOptions.value.filter((t) =>
    normalizeFilterText(`${t.code} ${t.name}`).includes(kw)
  )
})

const filteredDepartmentOptions = computed(() => {
  const kw = normalizeFilterText(departmentFilterKeyword.value)
  if (!kw) return departmentOptions.value
  return departmentOptions.value.filter((d) =>
    normalizeFilterText(`${d.code} ${d.name}`).includes(kw)
  )
})

const selectedTypeLabel = computed(() => {
  const id = assetTypeId.value
  if (!id) return "Loại tài sản"
  const found = assetTypeOptions.value.find((t) => t.id == id)
  if (!found) return "Loại tài sản"
  const code = found.code ? `${found.code} - ` : ""
  return `${code}${found.name}`.trim()
})

const selectedDepartmentLabel = computed(() => {
  const id = departmentId.value
  if (!id) return "Bộ phận sử dụng"
  const found = departmentOptions.value.find((d) => d.id == id)
  if (!found) return "Bộ phận sử dụng"
  const code = found.code ? `${found.code} - ` : ""
  return `${code}${found.name}`.trim()
})

watch(
  () => assetTypeId.value,
  () => {
    typeFilterKeyword.value = assetTypeId.value ? selectedTypeLabel.value : ""
  }
)

watch(
  () => departmentId.value,
  () => {
    departmentFilterKeyword.value = departmentId.value ? selectedDepartmentLabel.value : ""
  }
)

// Chọn hàm filter phù hợp theo service hiện có
function pickFilter() {
  const svc = assetService
  const candidates = [
    "getFilter",          // ? thêm dùng này (uu tiên)
    "getFilterPaging",
    "getFilteredPaging",
    "filterPaging",
    "getPaging",
    "getAssetsPaging",
    "getFilterAssets",
    "getFilteredAssets",
    "filterAssets",
    "filter",
  ]
  for (const k of candidates) {
    if (typeof svc?.[k] === "function") return svc[k].bind(svc)
  }
  return null
}


/**
 * Mô tả: Gửi API lấy danh sách tài sản.
 * - Lần đầu: load list bình thường (filter rỗng)
 * - Khi có keyword / department / type: gọi param lên backend để filter + paging
 * Ngày sửa: 2026-01-17
 */
async function loadAsset() {
  try {
    const kw = keyword.value.trim()

    const filterPayload = {
      keyword: kw || null,
      // gửi cả 2 key để khớp DTO từ bạn đặt tên  
      departmentId: departmentId.value || null,
      fixedAssetDepartmentId: departmentId.value || null,
      assetTypeId: assetTypeId.value || null,
      fixedAssetTypeId: assetTypeId.value || null,
      pageNumber: Number(pageNumber.value || 1),
      pageSize: Number(pageSize.value || 20),
    }

    const filterFn = pickFilter()

    let res
    if (filterFn) {
      res = await filterFn(filterPayload)
    } else {
      // fallback: n?u service getAsset() c?a b?n h? tr? query/paging th� truy?n payload
      res = await assetService.getAsset(filterPayload)
    }

    // Backend có thể trả:
    //  - []
    //  - { data: [], totalRecords: n }
    //  - { Data: [], TotalRecords: n }
    const list = res?.data ?? res?.Data ?? res?.items ?? res?.Items ?? res ?? []
    totalRecords.value =
      Number(res?.totalRecords ?? res?.TotalRecords ?? res?.total ?? res?.Total ?? list.length) || 0

    rows.value = (list ?? []).map((x) => {
      const departmentName = x.fixedAssetDepartmentName || departmentMap.value[x.fixedAssetDepartmentId] || ""
      const typeName = x.fixedAssetTypeName || assetTypeMap.value[x.fixedAssetTypeId] || ""
      const assetId = x.fixedAssetId ?? x.FixedAssetId ?? x.fixedassetId ?? x.fixed_asset_id ?? x.id

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

    const rowIds = new Set(rows.value.map((r) => r.id))
    selectedIds.value = selectedIds.value.filter((id) => rowIds.has(id))
    if (!rowIds.has(selectedId.value)) selectedId.value = null
  } catch (err) {
    console.error("Load assets error:", err)
    rows.value = []
    totalRecords.value = 0
  }
}


// ====== Popup actions ======
// Mở popup tạo mới
function openCreate() {
  popupMode.value = "create"
  editingAsset.value = null
  editingId.value = null
  cloneSourceId.value = null
  isPopupOpen.value = true
}

// Mở popup sửa với dữ liệu dòng
function openEdit(row) {
  popupMode.value = "edit"
  editingId.value = row.id
  cloneSourceId.value = null
  editingAsset.value = row.__raw || row
  isPopupOpen.value = true
}
// Mở popup nhân bản từ dòng
function openClone(row) {
  popupMode.value = "clone"
  editingId.value = null
  cloneSourceId.value = row.id ?? row.fixedAssetId ?? row.__raw?.fixedAssetId ?? null
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
    } else if (popupMode.value === "edit") {
      const id = editingId.value ?? payload.fixedAssetId ?? payload.id ?? editingAsset.value?.fixedAssetId ?? editingAsset.value?.id
      if (!id) throw new Error("Khong xac dinh duoc ID tai san de cap nhat")
      if (id && !payload.fixedAssetId) payload.fixedAssetId = id
      await assetService.update(id, payload)
    } else {
      const id = cloneSourceId.value ?? editingAsset.value?.fixedAssetId ?? editingAsset.value?.id
      if (!id) throw new Error("Khong xac dinh duoc ID tai san de nhan ban")
      await assetService.clone(id, payload)
    }
    isPopupOpen.value = false
    showToast()
    await wait(2000)
    await loadAsset()
  } catch (e) {
    console.error("Save error:", e)
  }
}

// Mô tả: Xoá tài sản
// Ngày tạo: 2026-01-15
async function doDeleteSelected() {
  try {
    const ids = [...pendingDeleteIds.value]
    if (!ids.length) return

    if (ids.length === 1) {
      await assetService.remove(ids[0])
    } else if (typeof assetService.removebulk === "function") {
      await assetService.removebulk(ids)
    } else if (typeof assetService.removeMany === "function") {
      await assetService.removeMany(ids)
    } else if (typeof assetService.deleteMany === "function") {
      await assetService.deleteMany(ids)
    } else {
      await Promise.all(ids.map((id) => assetService.remove(id)))
    }

    pendingDeleteIds.value = []
    selectedIds.value = selectedIds.value.filter((id) => !ids.includes(id))
    if (ids.includes(selectedId.value)) selectedId.value = null

    if (pageNumber.value > 1 && rows.value.length <= ids.length) {
      pageNumber.value = pageNumber.value - 1
    }
    await loadAsset()
  } catch (e) {
    console.error("Delete error:", e)
    alert("500")
  }
}

// Xác nhận xóa trong popup confirm
function handleDeleteConfirm() {
  if (isDeleteInfoOnly.value) return
  doDeleteSelected()
}

// Mở popup confirm xóa (1 hoặc nhiều)
function handleDelete() {
  const ids = selectedIds.value.length ? [...selectedIds.value] : selectedId.value ? [selectedId.value] : []
  if (!ids.length) {
    isDeleteInfoOnly.value = true
    deleteConfirmMessage.value = "Vui lòng chọn tài sản cần xóa"
    isDeleteConfirmOpen.value = true
    return
  }

  isDeleteInfoOnly.value = false
  pendingDeleteIds.value = ids
  if (ids.length == 1) {
    const row = rows.value.find((r) => r.id === ids[0])
    const label = row ? `${row.code} - ${row.name}` : ids[0]
    deleteConfirmMessage.value = `Bạn có muốn xóa tài sản ${label} ?`
  } else {
    deleteConfirmMessage.value = `Bạn có muốn xóa ${ids.length} tài sản được chọn?`
  }
  isDeleteConfirmOpen.value = true
}
// ====== Backend d� filter + paging, frontend ch? render ======
const filteredRows = computed(() => rows.value)
const isEmpty = computed(() => filteredRows.value.length === 0)
const selectedIdSet = computed(() => new Set(selectedIds.value))
const visibleIds = computed(() => filteredRows.value.map((r) => r.id))
const allChecked = computed(
  () => visibleIds.value.length > 0 && visibleIds.value.every((id) => selectedIdSet.value.has(id))
)
const someChecked = computed(() => visibleIds.value.some((id) => selectedIdSet.value.has(id)))

// ====== Totals (footer: t?ng theo d? li?u dang hi?n th?) ======
const totalQty = computed(() => filteredRows.value.reduce((s, r) => s + (Number(r.qty) || 0), 0))
const totalCost = computed(() => filteredRows.value.reduce((s, r) => s + (Number(r.cost) || 0), 0))
const totalDep = computed(() => filteredRows.value.reduce((s, r) => s + (Number(r.dep) || 0), 0))
const totalRemain = computed(() => filteredRows.value.reduce((s, r) => s + (Number(r.remain) || 0), 0))

// ====== Paging UI helpers ======
const totalPages = computed(() => {
  const size = Number(pageSize.value || 1)
  const total = Number(totalRecords.value || 0)
  const pages = Math.ceil(total / size)
  return Math.max(1, pages)
})

// Chuyển đến trang cụ thể
function goToPage(p) {
  const next = Number(p)
  if (!next || next < 1 || next > totalPages.value) return
  pageNumber.value = next
  loadAsset()
}

// Sang trang tiếp theo
function nextPage() {
  if (pageNumber.value < totalPages.value) {
    pageNumber.value++
    loadAsset()
  }
}

// Quay lại trang trước
function prevPage() {
  if (pageNumber.value > 1) {
    pageNumber.value--
    loadAsset()
  }
}
// Danh sách page hiện thị (chi hiện thị 1 2 3, >3 hiện thị 1 2 ... last)
const pageItems = computed(() => {
  const tp = totalPages.value
  if (tp <= 3) {
    return Array.from({ length: tp }, (_, i) => i + 1)
  }
  return [1, 2, "...", tp]
})

// Đổi kích thước trang và reload
function onChangePageSize() {
  pageNumber.value = 1
  loadAsset()
}

// ====== Column width persistence ======
// Load width cột từ localStorage
function loadColWidths() {
  const saved = readJson(LS_COL_KEY, null)
  if (Array.isArray(saved) && saved.length === defaultColWidths.length) {
    saved.forEach((w, i) => (colWidths[i] = w))
  }
  Object.entries(FIXED_COL_WIDTHS).forEach(([i, w]) => (colWidths[Number(i)] = w))
}
watch(
  () => [...colWidths],
  (val) => writeJson(LS_COL_KEY, val),
  { deep: true }
)

// ====== Resize columns ======
let resizing = null
// Bắt đầu kéo resize cột
function onResizeDown(e, colIndex) {
  e.preventDefault()
  if (colIndex in FIXED_COL_WIDTHS) return
  resizing = { colIndex, startX: e.clientX, startW: colWidths[colIndex] }
  window.addEventListener("mousemove", onResizeMove)
  window.addEventListener("mouseup", onResizeUp)
}
// Đang kéo resize cột
function onResizeMove(e) {
  if (!resizing) return
  const dx = e.clientX - resizing.startX
  const MIN = 32
  colWidths[resizing.colIndex] = Math.max(MIN, resizing.startW + dx)
}
// Kết thúc kéo resize cột
function onResizeUp() {
  resizing = null
  window.removeEventListener("mousemove", onResizeMove)
  window.removeEventListener("mouseup", onResizeUp)
}

// Chọn dòng và toggle checkbox selection
function getRowIndexById(id) {
  return filteredRows.value.findIndex((r) => r.id === id)
}

function selectRange(fromId, toId) {
  const fromIdx = getRowIndexById(fromId)
  const toIdx = getRowIndexById(toId)
  if (fromIdx < 0 || toIdx < 0) return
  const [start, end] = fromIdx < toIdx ? [fromIdx, toIdx] : [toIdx, fromIdx]
  const rangeIds = filteredRows.value.slice(start, end + 1).map((r) => r.id)
  selectedIds.value = rangeIds
}

function selectRow(id, e) {
  selectedId.value = id

  if (e?.shiftKey && lastSelectedId.value) {
    selectRange(lastSelectedId.value, id)
    return
  }

  if (selectedIds.value.includes(id)) {
    selectedIds.value = selectedIds.value.filter((x) => x !== id)
    lastSelectedId.value = id
    return
  }
  selectedIds.value = [...selectedIds.value, id]
  lastSelectedId.value = id
}

// Scroll dòng vào view sau khi đổi selection
function scrollRowIntoView(id) {
  nextTick(() => {
    const el = tbodyRef.value?.querySelector(`tr[data-row-id="${id}"]`)
    if (el) el.scrollIntoView({ block: "nearest" })
  })
}

// Điều hướng selection bằng phím lên/xuống
function handleTableKeydown(e) {
  const tag = e.target?.tagName?.toLowerCase()
  if (tag === "input" || tag === "select" || tag === "textarea" || e.target?.isContentEditable) return

  if (e.key !== "ArrowDown" && e.key !== "ArrowUp") return
  e.preventDefault()
  const list = filteredRows.value
  if (!list.length) return

  let idx = list.findIndex((r) => r.id === selectedId.value)
  if (idx < 0) idx = e.key === "ArrowDown" ? -1 : list.length
  const nextIdx = e.key === "ArrowDown" ? Math.min(list.length - 1, idx + 1) : Math.max(0, idx - 1)
  const nextId = list[nextIdx].id
  const prevId = selectedId.value
  selectedId.value = nextId
  if (e.shiftKey && (lastSelectedId.value || prevId)) {
    const anchor = lastSelectedId.value ?? prevId
    selectRange(anchor, nextId)
  } else if (!selectedIds.value.length) {
    selectedIds.value = [nextId]
  }
  lastSelectedId.value = nextId
  scrollRowIntoView(nextId)
}

// Mở context menu theo vị trí chuột
function openContextMenu(e, row) {
  e.preventDefault()
  e.stopPropagation()

  if (!selectedIdSet.value.has(row.id)) {
    selectedId.value = row.id
    selectedIds.value = [row.id]
  }

  const menuWidth = 160
  const menuHeight = 120
  const maxX = window.innerWidth - menuWidth - 8
  const maxY = window.innerHeight - menuHeight - 8
  contextMenu.x = Math.max(8, Math.min(e.clientX, maxX))
  contextMenu.y = Math.max(8, Math.min(e.clientY, maxY))
  contextMenu.row = row
  contextMenu.visible = true
}

// Đóng context menu
function closeContextMenu() {
  contextMenu.visible = false
  contextMenu.row = null
}

// Action sửa từ context menu
function handleContextEdit() {
  if (!contextMenu.row) return
  openEdit(contextMenu.row)
  closeContextMenu()
}

// Action nhân bản từ context menu
function handleContextClone() {
  if (!contextMenu.row) return
  openClone(contextMenu.row)
  closeContextMenu()
}

// Action xóa từ context menu
function handleContextDelete() {
  if (!contextMenu.row) return
  if (!selectedIdSet.value.has(contextMenu.row.id)) {
    selectedId.value = contextMenu.row.id
    selectedIds.value = [contextMenu.row.id]
  }
  handleDelete()
  closeContextMenu()
}


// Toggle chọn tất cả dòng đang hiển thị
function toggleAllSelection(e) {
  const checked = e.target.checked
  selectedIds.value = checked ? [...visibleIds.value] : []
}

// Toggle chọn một dòng
function toggleRowSelection(id, e) {
  const checked = e.target.checked
  if (checked) {
    if (!selectedIds.value.includes(id)) {
      selectedIds.value = [...selectedIds.value, id]
    }
  } else {
    selectedIds.value = selectedIds.value.filter((x) => x !== id)
  }
}

// Toggle dropdown loại tài sản (filter)
function toggleTypeDropdown() {
  isTypeDropdownOpen.value = !isTypeDropdownOpen.value
  if (isTypeDropdownOpen.value) isDepartmentDropdownOpen.value = false
}

// Toggle dropdown bộ phận sử dụng (filter)
function toggleDepartmentDropdown() {
  isDepartmentDropdownOpen.value = !isDepartmentDropdownOpen.value
  if (isDepartmentDropdownOpen.value) isTypeDropdownOpen.value = false
}

function openTypeDropdown() {
  isTypeDropdownOpen.value = true
  isDepartmentDropdownOpen.value = false
}

function openDepartmentDropdown() {
  isDepartmentDropdownOpen.value = true
  isTypeDropdownOpen.value = false
}

// Đóng các dropdown filter
function closeDropdowns() {
  isTypeDropdownOpen.value = false
  isDepartmentDropdownOpen.value = false
}

// Chọn loại tài sản và reload list
function selectType(id) {
  assetTypeId.value = id
  pageNumber.value = 1
  closeDropdowns()
  loadAsset()
}

// Chọn bộ phận sử dụng và reload list
function selectDepartment(id) {
  departmentId.value = id
  pageNumber.value = 1
  closeDropdowns()
  loadAsset()
}

// ====== Watch filters -> g?i API filter (debounce) ======
let filterTimer = null
watch(
  () => keyword.value,
  () => {
    pageNumber.value = 1
    if (filterTimer) clearTimeout(filterTimer)
    filterTimer = setTimeout(() => {
      loadAsset()
    }, 350)
  }
)

watch(
  () => pageSize.value,
  () => {
    onChangePageSize()
  }
)

onMounted(async () => {
  loadColWidths()
  await loadCategories()
  await loadAsset()
})

// Click ngoài: đóng dropdown/context menu
function onGlobalClick(e) {
  if (!e.target?.closest?.(".combo-wrap")) closeDropdowns()
  if (!contextMenu.visible) return
  if (e.target?.closest?.(".context-menu")) return
  closeContextMenu()
}

onMounted(() => {
  window.addEventListener("click", onGlobalClick)
  window.addEventListener("scroll", closeContextMenu, true)
})

onBeforeUnmount(() => {
  window.removeEventListener("click", onGlobalClick)
  window.removeEventListener("scroll", closeContextMenu, true)
  if (toastTimer) clearTimeout(toastTimer)
})
</script>

<template>
  <section class="page">
    <div v-if="toastVisible" class="toast" role="status" aria-live="polite">
      <span class="toast-icon">OK</span>
      <span>Lưu dữ liệu thành công</span>
    </div>
    <!-- Filters + actions -->
    <div class="toolbar">
      <div class="filters">
        <div class="field">
          <span class="prefix"><span class="icon icon-search" aria-hidden="true"></span></span>
          <input v-model="keyword" class="input" placeholder="Tìm kiếm tài sản" />
        </div>

        <div class="field">
          <span class="prefix"><span class="icon icon-type" aria-hidden="true"></span></span>
          <div class="combo-wrap" @click.stop>
            <input v-model="typeFilterKeyword" class="input combo-input" placeholder="Loại tài sản"
              @focus="openTypeDropdown" @input="openTypeDropdown" autocomplete="off" />
            <button class="combo-caret-btn" type="button" @click="toggleTypeDropdown">
              <span class="icon icon-dropdown"></span>
            </button>
            <div v-if="isTypeDropdownOpen" class="combo-menu" role="listbox">
              <div class="combo-header">
                <span class="combo-col combo-code">Mã</span>
                <span class="combo-col combo-name">Tên</span>
              </div>
              <div class="combo-list">
                <button class="combo-item" type="button" @click="selectType('')">
                  <span class="combo-col combo-code"></span>
                  <span class="combo-col combo-name">Tất cả</span>
                </button>
                <button v-for="t in filteredAssetTypeOptions" :key="t.id" class="combo-item"
                  :class="{ active: t.id == assetTypeId }" type="button" @click="selectType(t.id)">
                  <span class="combo-col combo-code">{{ t.code }}</span>
                  <span class="combo-col combo-name">{{ t.name }}</span>
                </button>
              </div>
            </div>
          </div>
        </div>

        <div class="field">
          <span class="prefix"><span class="icon icon-type" aria-hidden="true"></span></span>
          <div class="combo-wrap" @click.stop>
            <input v-model="departmentFilterKeyword" class="input combo-input" placeholder="Bộ phận sử dụng"
              @focus="openDepartmentDropdown" @input="openDepartmentDropdown" autocomplete="off" />
            <button class="combo-caret-btn" type="button" @click="toggleDepartmentDropdown">
              <span class="icon icon-dropdown"></span>
            </button>
            <div v-if="isDepartmentDropdownOpen" class="combo-menu" role="listbox">
              <div class="combo-header">
                <span class="combo-col combo-code">Mã</span>
                <span class="combo-col combo-name">Tên</span>
              </div>
              <div class="combo-list">
                <button class="combo-item" type="button" @click="selectDepartment('')">
                  <span class="combo-col combo-code"></span>
                  <span class="combo-col combo-name">Tất cả</span>
                </button>
                <button v-for="d in filteredDepartmentOptions" :key="d.id" class="combo-item"
                  :class="{ active: d.id == departmentId }" type="button" @click="selectDepartment(d.id)">
                  <span class="combo-col combo-code">{{ d.code }}</span>
                  <span class="combo-col combo-name">{{ d.name }}</span>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="actions">
        <button class="btn primary" type="button" @click="openCreate">+ Thêm tài sản</button>
        <button class="btn" type="button" title="Xuất" aria-label="Xuất">
          <span class="icon icon-export" aria-hidden="true"></span>
        </button>
        <button class="btn" type="button" title="Xóa" aria-label="Xóa" @click="handleDelete">
          <span class="icon icon-delete" aria-hidden="true"></span>
        </button>
      </div>
    </div>
    <ConfirmDeletePopup v-model="isDeleteConfirmOpen" :message="deleteConfirmMessage" :okText="deleteOkText"
      :cancelText="deleteCancelText" :hideCancel="isDeleteInfoOnly" @confirm="handleDeleteConfirm" />
    <!-- Table -->
   <div class="table-wrap" ref="tableWrapRef" tabindex="0" @keydown="handleTableKeydown" @contextmenu.prevent>
  <!-- HEADER (không scroll) -->
  <div class="table-head">
    <table class="asset-table" :style="{ minWidth: tableWidth + 'px' }">
      <colgroup>
        <col v-for="(w, i) in colWidths" :key="i" :style="{ width: w + 'px' }" />
      </colgroup>

      <thead>
        <tr>
          <th class="th-center cell-checkbox">
            <input class="chk" type="checkbox"
              :checked="allChecked"
              :indeterminate="someChecked && !allChecked"
              @change="toggleAllSelection" />
          </th>

          <th class="th-center col-stt">
            STT
          </th>

          <th>Mã tài sản <span class="col-resizer" @mousedown="onResizeDown($event, 2)"></span></th>
          <th>Tên tài sản <span class="col-resizer" @mousedown="onResizeDown($event, 3)"></span></th>
          <th>Loại tài sản <span class="col-resizer" @mousedown="onResizeDown($event, 4)"></span></th>
          <th>Bộ phận sử dụng <span class="col-resizer" @mousedown="onResizeDown($event, 5)"></span></th>

          <th class="th-right">Số lượng <span class="col-resizer" @mousedown="onResizeDown($event, 6)"></span></th>
          <th class="th-right">Nguyên giá <span class="col-resizer" @mousedown="onResizeDown($event, 7)"></span></th>
          <th class="th-right">HM/KH lũy kế <span class="col-resizer" @mousedown="onResizeDown($event, 8)"></span></th>
          <th class="th-right">Giá trị còn lại <span class="col-resizer" @mousedown="onResizeDown($event, 9)"></span></th>

          <th class="th-center">
            Chức năng
            <span class="col-resizer" @mousedown="onResizeDown($event, 10)"></span>
          </th>
        </tr>
      </thead>
    </table>
  </div>

  <!-- BODY (scroll chỉ ở đây) -->
  <div class="table-body">
    <table class="asset-table" :style="{ minWidth: tableWidth + 'px' }">
      <colgroup>
        <col v-for="(w, i) in colWidths" :key="i" :style="{ width: w + 'px' }" />
      </colgroup>

      <tbody ref="tbodyRef">
        <tr v-for="(r, idx) in filteredRows" :key="r.id" :data-row-id="r.id"
          :class="{ selected: r.id === selectedId || selectedIdSet.has(r.id) }"
          @click="selectRow(r.id, $event)"
          @contextmenu="openContextMenu($event, r)">
          <td class="td-center cell-checkbox">
            <input class="chk" type="checkbox" :checked="selectedIdSet.has(r.id)"
              @change="toggleRowSelection(r.id, $event)" @click.stop />
          </td>
          <td class="td-center col-stt">{{ (pageNumber - 1) * pageSize + idx + 1 }}</td>
          <td class="td-ellipsis">{{ r.code }}</td>
          <td class="td-ellipsis">{{ r.name }}</td>
          <td class="td-ellipsis">{{ r.typeName }}</td>
          <td class="td-ellipsis">{{ r.department }}</td>
          <td class="td-right">{{ r.qty }}</td>
          <td class="td-right">{{ fmtMoney(r.cost) }}</td>
          <td class="td-right">{{ fmtMoney(r.dep) }}</td>
          <td class="td-right">{{ fmtMoney(r.remain) }}</td>

          <td class="td-center sticky-col" @click.stop>
            <button class="icon-action" type="button" title="Sửa" @click="openEdit(r)">
              <div class="icon icon-edit"></div>
            </button>
            <button class="icon-action" type="button" title="Nhân bản" @click="openClone(r)">
              <div class="icon icon-clone"></div>
            </button>
          </td>
        </tr>
        <tr class="tbody-spacer">
          <td colspan="11"></td>
        </tr>
      </tbody>
      <tfoot>
          <tr class="tfoot-row sticky-footer">
            <td colspan="6" class="tfoot-left"><!-- Paging bar  -->
              <div class="paging-bar">
                <div class="paging-left">
                  <span>Tổng số: <span class="tfoot-strong">{{ totalRecords || 0 }}</span> bản ghi</span>
                  <div class="page-size">
                    <select v-model.number="pageSize" class="page-size-select">
                      <option v-for="s in pageSizeOptions" :key="s" :value="s">{{ s }}</option>
                    </select>
                  </div>
                </div>

                <div class="paging-right">
                  <button class="page-btn-paging" type="button" :disabled="pageNumber <= 1" @click="prevPage"><</button>

                  <button v-for="(p, i) in pageItems" :key="i" class="page-btn-number"
                    :class="{ active: p === pageNumber, ellipsis: p === '...' }" type="button" :disabled="p === '...'"
                    @click="p !== '...' && goToPage(p)">
                    {{ p }}
                  </button>

                  <button class="page-btn-paging" type="button" :disabled="pageNumber >= totalPages"
                    @click="nextPage">></button>
                </div>
              </div>
            </td>
            <td class="td-right tfoot-strong">{{ totalQty }}</td>
            <td class="td-right tfoot-strong">{{ fmtMoney(totalCost) }}</td>
            <td class="td-right tfoot-strong">{{ fmtMoney(totalDep) }}</td>
            <td class="td-right tfoot-strong">{{ fmtMoney(totalRemain) }}</td>
            <td></td>
          </tr>
        </tfoot>
    </table>

    <div v-if="isEmpty" class="table-empty">
      <span>Không có dữ liệu</span>
    </div>
  </div>
  
  <!-- FOOTER (không scroll) giữ nguyên bạn đang làm -->
  <!-- Bạn có thể để tfoot ở ngoài như hiện tại hoặc tách ra giống header -->
</div>


    <div v-if="contextMenu.visible" class="context-menu"
      :style="{ top: contextMenu.y + 'px', left: contextMenu.x + 'px' }" @click.stop>
      <button class="context-item" type="button" @click="openCreate">Thêm</button>
      <button class="context-item" type="button" @click="handleContextEdit">Sửa</button>
      <button class="context-item" type="button" @click="handleContextClone">Nhân bản</button>
      <button class="context-item danger" type="button" @click="handleContextDelete">Xóa</button>
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
  height: calc(100vh - 60px);
  display: flex;
  flex-direction: column;
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

.input {
  width: 220px;
}

.prefix {
  z-index: 2;
  pointer-events: none;
}

.combo-wrap {
  position: relative;
}

.combo-input {
  width: 220px;
  text-align: left;
  cursor: text;
  padding-left: 30px;
  padding-right: 28px;
}

.combo-text {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.combo-caret {
  font-size: 10px;
  color: #6b7280;
}

.combo-caret-btn {
  position: absolute;
  right: 4px;
  top: 50%;
  transform: translateY(-50%);
  border: none;
  background: transparent;
  cursor: pointer;
  padding: 4px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.combo-menu {
  position: absolute;
  top: calc(100% + 4px);
  left: 0;
  width: 320px;
  background: #fff;
  border: 1px solid #d8dfe8;
  border-radius: 4px;
  box-shadow: 0 8px 18px rgba(0, 0, 0, 0.12);
  z-index: 20;
}

.combo-header {
  display: flex;
  gap: 8px;
  padding: 6px 10px;
  border-bottom: 1px solid #e3e6ef;
  font-weight: 600;
  font-size: 12px;
  color: #111;
  background: #f7f8fb;
}

.combo-list {
  max-height: 220px;
  overflow: auto;
}

.combo-item {
  width: 100%;
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 6px 10px;
  border: none;
  background: transparent;
  text-align: left;
  cursor: pointer;
  font-size: 12px;
}

.combo-item:hover {
  background: #c7e0f5;
}

.combo-item.active {
  background: #96c6ee;
}

.combo-col {
  display: inline-flex;
  align-items: center;
}

.combo-code {
  width: 40px;
  flex: 0 0 auto;
}

.combo-name {
  flex: 1 1 auto;
}

/* actions */
.actions {
  display: flex;
  align-items: center;
  gap: 8px;
}

/* Icon */
.icon {
  width: 20px;
  height: 20px;
  display: inline-flex;
  background-image: url('@/assets/icons/qlts-icon.svg');
  background-repeat: no-repeat;
  background-size: 504px 754px;
}

.icon-dropdown {
  background-position: -72px -338px;
  width: 7px;
  height: 5px;
}

.icon-search {
  background-position: -243px -23px;
  width: 18px;
  height: 18px;
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

.icon-edit {
  background-position: -156px -68px;
  width: 16px;
  height: 16px;
}
.icon-clone {
  background-position:  -244px -112px;
	width: 16px;
	height: 16px;
}

/* Table wrap */
.table-wrap{
  border: 1px solid #e3e6ef;
  border-radius: 4px;
  background:#fff;
  position: relative;
  display: flex;
  flex-direction: column;
  flex: 1 1 auto;
  min-height: 240px;
  overflow: hidden; /* QUAN TRỌNG: không cho scroll ở wrap */
}

/* Header không scroll */
.table-head{
  flex: 0 0 auto;
  overflow: hidden;
  border-bottom: 1px solid #e3e6ef;
}

/* Body mới là nơi scroll */
.table-body{
  flex: 1 1 auto;
  overflow: auto;
  position: relative;
  scrollbar-gutter: stable;
}

/* .table-wrap:focus {
  outline: 2px solid rgba(43, 181, 255, 0.35);
  outline-offset: 2px;
} */

/* Table */
.asset-table {
  border-collapse: collapse;
  table-layout: fixed;
  font-family: Roboto, Arial, sans-serif;
  font-size: 12px;
  width: 100%;
  min-width: 100%;
  height: 100%;
}

.asset-table th,
.asset-table td {
  box-sizing: border-box;
}

.asset-table thead th {
  position: relative;
  background: #f7f8fb;
  border-bottom: 1px solid #e3e6ef;
  height: 38px;
  padding: 0 10px;
  text-align: left;
  font-weight: 600;
  white-space: nowrap;
}

.asset-table td {
  border-bottom: 1px solid #eef1f6;
  height: 38px;
  padding: 0 10px;
  vertical-align: middle;
  background: #fff;
}

.tbody-spacer td {
  border-bottom: none;
  padding: 0;
  height: 100%;
  background: transparent;
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

.asset-table thead th.th-center {
  text-align: center;
}

.asset-table thead th.th-right {
  text-align: right;
}

.chk {
  width: 14px;
  height: 14px;
  accent-color: #2bb5ff;
}

/* selected row gi?ng ?nh */
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
  background: transparent;
  border: none;
  cursor: pointer;
}

.icon-action:hover,
.icon-action:focus,
.icon-action:active {
  background: transparent;
  box-shadow: none;
  outline: none;
}

/* btn style */

.btn-action {
  height: 32px;
  border-radius: 4px;
  border: 1px solid var(--ui-border);
  background: var(--ui-bg);
  padding: 0 12px;
  font-size: 12px;
  color: var(--ui-text);
  cursor: pointer;
}
/* Paging bar */
.paging-bar {
  margin-top: 8px;
  display: flex;
  align-items: center;
  gap: 12px;
  font-size: 12px;
  color: #111;
}

.paging-left {
  display: inline-flex;
  align-items: center;
  gap: 12px;
}

.page-size-select {
  height: 28px;
  border: 1px solid #d8dfe8;
  border-radius: 4px;
  background: #fff;
  padding: 0 8px;
  outline: none;
}

.paging-right {
  display: inline-flex;
  align-items: center;
  gap: 6px;
}

.page-btn {
  min-width: 28px;
  height: 28px;
  padding: 0 8px;
  border: 1px solid #d8dfe8;
  border-radius: 4px;
  background: #fff;
  cursor: pointer;
  font-size: 12px;
}

.page-btn-paging {
  min-width: 28px;
  height: 28px;
  padding: 0 8px;
  border-radius: 4px;
  cursor: pointer;
  font-size: 12px;
  border: none;
  background: transparent;
}

.page-btn-number {
  border: none;
  background: transparent;
  border-radius: 4px;
}

/* ACTIVE giữ màu */
.page-btn-number.active {
  background: #f5f5f5;
  font-weight: 700;
}

/* Hover không đè lên active */
.page-btn-number:hover:not(.active):not(.ellipsis) {
  background: #e5e7eb;
}
.page-btn:disabled {
  cursor: not-allowed;
  opacity: 0.6;
}

.page-btn.active {
  background: #2bb5ff;
  border-color: #2bb5ff;
  color: #fff;
  font-weight: 700;
}

.page-btn.ellipsis {
  border-color: transparent;
  background: transparent;
  cursor: default;
}

.table-empty {
  position: absolute;
  left: 0;
  right: 0;
  top: 34px;
  /* header height */
  bottom: 40px;
  /* footer height */
  display: flex;
  align-items: center;
  justify-content: center;
  color: #6b7280;
  font-size: 12px;
  background: #fff;
  pointer-events: none;
}

.asset-table thead th {
  padding: 0 10px;
}

.asset-table td {
  padding: 0 10px;
}

.th-right,
.td-right {
  text-align: right;
  font-variant-numeric: tabular-nums;
}

/* Checkbox center like ?nh */
.cell-checkbox {
  text-align: center;
  vertical-align: middle;
  max-width: 40px;
}

.col-stt {
  padding-left: 0;
  max-width: 40px;
}

.cell-checkbox .chk {
  margin: 0;
  vertical-align: middle;
}

.sticky-footer {
  position: sticky;
  bottom: 0;
  z-index: 1;
}

.sticky-title {
  position: sticky;
  top: 0;
  z-index: 2;
}

/* Sticky c?t ch?c nang */
.sticky-col {
  position: sticky;
  right: 0;
  z-index: 2;
  background: inherit;
  border-left: 1px solid #eef1f6;
}

.asset-table thead th.sticky-col {
  z-index: 3;
  border-left: 1px solid #e3e6ef;
}

/* Icon ch? hi?n khi hover */
.icon-action {
  opacity: 0;
  visibility: hidden;
  pointer-events: none;
}

.asset-table tbody tr:hover .icon-action {
  opacity: 1;
  visibility: visible;
  pointer-events: auto;
}

/* Gi? m�u n?n selected cho c?t sticky */
.asset-table tbody tr.selected td.sticky-col {
  background: #d9f3ff;
}

/* Hover row to highlight full line */
.asset-table tbody tr:hover td {
  background: #eef7ff;
}

.toast {
  position: fixed;
  right: 18px;
  top: 18px;
  background: #fff;
  border: 1px solid #e3e6ef;
  box-shadow: 0 12px 32px rgba(0, 0, 0, 0.18);
  border-radius: 4px;
  padding: 10px 14px;
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 13px;
  color: #111;
  z-index: 10001;
}

.toast-icon {
  width: 22px;
  height: 22px;
  border-radius: 50%;
  background: #2eb15f;
  color: #fff;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: 14px;
  font-weight: 700;
}
.context-menu {
  position: fixed;
  min-width: 160px;
  background: #fff;
  border: 1px solid #e3e6ef;
  box-shadow: 0 8px 20px rgba(0, 0, 0, 0.12);
  border-radius: 6px;
  padding: 6px;
  z-index: 50;
}

.context-item {
  width: 100%;
  text-align: left;
  padding: 8px 10px;
  border: none;
  background: transparent;
  border-radius: 4px;
  cursor: pointer;
  font-size: 12px;
}

.context-item:hover {
  background: #f2f4f7;
}

.context-item.danger {
  color: #d14343;
}
</style>





