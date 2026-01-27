<script setup>
// Mô tả: Popup thêm/sửa tài sản (emit payload để ListView gọi API)
// Ngày tạo: 2026-01-14
import { computed, onBeforeUnmount, onMounted, reactive, ref, watch } from "vue"
import ConfirmPopup from "./ConfirmPopup.vue"
import { assetService } from "@/service/AssetService.js"
import { useAssetPopupValidation } from "@/composables/useAssetPopupValidation"

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  mode: {
    type: String, default: "create",
    validator: (v) => ["create", "edit", "clone"].includes(v)
  }, // create|edit
  asset: { type: Object, default: null },

  departments: { type: Array, default: () => [] },
  assetTypes: { type: Array, default: () => [] },
})

const emit = defineEmits(["update:modelValue", "save"])

// ====== UI state: cảnh báo khi thoát + thông báo Lưu thành công ======
// Ngày tạo: 2026-01-17
const isConfirmOpen = ref(false)
let initialSnapshot = ""

// Lấy ngày hiện tại theo định dạng input date (yyyy-mm-dd)
function getTodayInputDate() {
  // lấy theo local time để tránh lệch ngày do timezone
  const d = new Date()
  const yyyy = d.getFullYear()
  const mm = String(d.getMonth() + 1).padStart(2, "0")
  const dd = String(d.getDate()).padStart(2, "0")
  return `${yyyy}-${mm}-${dd}`
}

// Ưu tiên lấy giá trị theo danh sách key, fallback nếu không có
function pickFieldValue(obj, keys, fallback) {
  for (const key of keys) {
    const val = obj?.[key]
    if (val !== undefined && val !== null && val !== "") return val
  }
  return fallback
}

const isCodeLoading = ref(false)
const codeLoadError = ref("")

// Gọi API lấy mã tài sản mới và gán vào form
async function fetchNewAssetCode() {
  isCodeLoading.value = true
  codeLoadError.value = ""
  try {
    const data = await assetService.getNewCode()

    // chuẩn hóa key (phòng backend trả camelCase / PascalCase)
    const code =
      data?.fixedAssetCode ||
      data?.FixedAssetCode ||
      ""

    if (!code) throw new Error("Empty code")

    form.fixedAssetCode = code
  } catch (e) {
    codeLoadError.value = "500"
    form.fixedAssetCode = ""
  } finally {
    isCodeLoading.value = false
  }
}

// Reset toàn bộ form về trạng thái tạo mới
function resetFormCreate() {
  form.fixedAssetId = null
  form.fixedAssetCode = ""
  form.fixedAssetName = ""
  form.fixedAssetDepartmentId = ""
  form.fixedAssetDepartmentName = ""
  form.fixedAssetTypeId = ""
  form.fixedAssetTypeName = ""
  form.fixedAssetQuantity = 1
  form.fixedAssetCost = 0
  form.fixedAssetDepreciationRate = 0

  const today = getTodayInputDate()
  form.fixedAssetPurchaseDate = today
  form.fixedAssetStartUsingDate = today

  form.fixedAssetTrackingYear = new Date().getFullYear()
  form.fixedAssetUsingYear = 0
  form.fixedAssetDepreciationValueYear = 0

  // nếu bạn có thêm field display kiểu "1.000.000" thì reset luôn
  costDisplay.value = "0"
  depreciationDisplay.value = "0"

  resetValidation()
}

// Áp dữ liệu từ asset khi clone (không lấy mã)
function applyCloneFrom(asset) {
  if (!asset) return

  // copy các field nghiệp vụ bạn muốn nhân bản
  form.fixedAssetName = asset.fixedAssetName ?? ""
  form.fixedAssetDepartmentId = asset.fixedAssetDepartmentId ?? ""
  form.fixedAssetDepartmentName = asset.fixedAssetDepartmentName ?? ""
  form.fixedAssetTypeId = asset.fixedAssetTypeId ?? ""
  form.fixedAssetTypeName = asset.fixedAssetTypeName ?? ""
  form.fixedAssetPurchaseDate = asset.fixedAssetPurchaseDate ?? getTodayInputDate()
  form.fixedAssetUsingYear = pickFieldValue(
    asset,
    ["fixedAssetUsingYear", "usingYear", "lifeYear", "FixedAssetUsingYear", "UsingYear", "LifeYear"],
    0
  )
  form.fixedAssetTrackingYear = new Date().getFullYear()
  form.fixedAssetQuantity = asset.fixedAssetQuantity ?? 1
  form.fixedAssetCost = asset.fixedAssetCost ?? 0
  form.fixedAssetDepreciationRate = pickFieldValue(
    asset,
    ["fixedAssetDepreciationRate", "depreciationRate", "FixedAssetDepreciationRate", "DepreciationRate"],
    0
  )
  form.fixedAssetDepreciationValueYear = asset.fixedAssetDepreciationValueYear ?? 0
  form.fixedAssetStartUsingDate = asset.fixedAssetStartUsingDate ?? getTodayInputDate()

  // nếu có ô display tiền, sync lại
  syncCostDisplay?.()
  syncDepreciationDisplay?.()
}


const form = reactive({
  fixedAssetId: null,
  fixedAssetCode: "",
  fixedAssetName: "",
  fixedAssetDepartmentId: "",
  fixedAssetDepartmentName: "",
  fixedAssetTypeId: "",
  fixedAssetTypeName: "",
  fixedAssetQuantity: 1,
  fixedAssetCost: 0,
  fixedAssetDepreciationRate: 0,
  fixedAssetPurchaseDate: "",
  fixedAssetStartUsingDate: "",
  fixedAssetTrackingYear: new Date().getFullYear(),
  fixedAssetUsingYear: 0,
  fixedAssetDepreciationValueYear: 0,
})

const costDisplay = ref("")
const depreciationDisplay = ref("")
const isTypeDropdownOpen = ref(false)
const isDepartmentDropdownOpen = ref(false)

// Format số theo locale VN để hiển thị
function formatNumber(value) {
  const num = Number(value || 0)
  if (value === "" || value === null || value === undefined) return ""
  return num.toLocaleString("vi-VN")
}

// Lấy chữ cái đầu mỗi từ và viết hoa (dùng cho mã viết tắt)
function getInitials(text) {
  if (!text) return ""
  return String(text)
    .trim()
    .split(/[\s\-_/]+/g)
    .filter(Boolean)
    .map((w) => w[0]?.toUpperCase() ?? "")
    .join("")
}

// Đồng bộ hiển thị nguyên giá theo định dạng
function syncCostDisplay() {
  costDisplay.value = formatNumber(form.fixedAssetCost)
}

// Đồng bộ hiển thị giá trị hao mòn năm theo định dạng
function syncDepreciationDisplay() {
  depreciationDisplay.value = formatNumber(form.fixedAssetDepreciationValueYear)
}

// Xử lý input nguyên giá: chỉ nhận số và format hiển thị
function handleCostInput(event) {
  const raw = event?.target?.value ?? ""
  const digits = String(raw).replace(/[^\d]/g, "")
  if (!digits) {
    form.fixedAssetCost = ""
    costDisplay.value = ""
    return
  }
  const num = Number(digits)
  form.fixedAssetCost = num
  costDisplay.value = formatNumber(num)
}


const assetTypeOptions = computed(() =>
  (props.assetTypes || []).map((t) => ({
    id: t.fixedAssetTypeId ?? t.id,
    code: t.fixedAssetTypeCode ?? t.code ?? "",
    name: t.fixedAssetTypeName ?? t.name ?? "",
  }))
)

const departmentOptions = computed(() =>
  (props.departments || []).map((d) => ({
    id: d.fixedAssetDepartmentId ?? d.id,
    code: d.fixedAssetDepartmentCode ?? d.code ?? "",
    name: d.fixedAssetDepartmentName ?? d.name ?? "",
  }))
)

const selectedTypeLabel = computed(() => {
  const id = form.fixedAssetTypeId
  if (!id) return "Chọn mã loại"
  const found = assetTypeOptions.value.find((t) => t.id == id)
  if (!found) return "Chọn mã loại"
  const shortCode = getInitials(found.name) || found.code || ""
  return shortCode || "Chọn mã loại"
})

const selectedDepartmentLabel = computed(() => {
  const id = form.fixedAssetDepartmentId
  if (!id) return "Chọn mã bộ phận"
  const found = departmentOptions.value.find((d) => d.id == id)
  if (!found) return "Chọn mã bộ phận"
  const shortCode = getInitials(found.name) || found.code || ""
  return shortCode || "Chọn mã bộ phận"
})

const { fieldLabels, errors, touched, resetValidation, validateField, markTouched, showError, validateAll } =
  useAssetPopupValidation(form)

// Lưu snapshot form để so sánh dirty
function takeSnapshot() {
  initialSnapshot = JSON.stringify({
    fixedAssetCode: form.fixedAssetCode,
    fixedAssetName: form.fixedAssetName,
    fixedAssetDepartmentId: form.fixedAssetDepartmentId,
    fixedAssetTypeId: form.fixedAssetTypeId,
    fixedAssetQuantity: form.fixedAssetQuantity,
    fixedAssetCost: form.fixedAssetCost,
    fixedAssetDepreciationRate: form.fixedAssetDepreciationRate,
    fixedAssetPurchaseDate: form.fixedAssetPurchaseDate,
    fixedAssetStartUsingDate: form.fixedAssetStartUsingDate,
    fixedAssetTrackingYear: form.fixedAssetTrackingYear,
    fixedAssetUsingYear: form.fixedAssetUsingYear,
    fixedAssetDepreciationValueYear: form.fixedAssetDepreciationValueYear,
  })
}

const isDirty = computed(() => {
  const now = JSON.stringify({
    fixedAssetCode: form.fixedAssetCode,
    fixedAssetName: form.fixedAssetName,
    fixedAssetDepartmentId: form.fixedAssetDepartmentId,
    fixedAssetTypeId: form.fixedAssetTypeId,
    fixedAssetQuantity: form.fixedAssetQuantity,
    fixedAssetCost: form.fixedAssetCost,
    fixedAssetDepreciationRate: form.fixedAssetDepreciationRate,
    fixedAssetPurchaseDate: form.fixedAssetPurchaseDate,
    fixedAssetStartUsingDate: form.fixedAssetStartUsingDate,
    fixedAssetTrackingYear: form.fixedAssetTrackingYear,
    fixedAssetUsingYear: form.fixedAssetUsingYear,
    fixedAssetDepreciationValueYear: form.fixedAssetDepreciationValueYear,
  })
  return initialSnapshot && now !== initialSnapshot
})

// Mô tả: Tự động tính "Giá trị hao mòn năm" = Nguyên giá * Tỷ lệ hao mòn / 100
// Ngày tạo: 2026-01-16
const departmentMap = computed(() => {
  const byId = {}
  for (const d of props.departments || []) {
    const id = d.fixedAssetDepartmentId ?? d.id
    byId[id] = {
      name: d.fixedAssetDepartmentName ?? d.name ?? "",
      code: d.fixedAssetDepartmentCode ?? d.code ?? "",
    }
  }
  return byId
})

// Tính giá trị hao mòn năm từ nguyên giá và tỷ lệ
function recalcDepreciationValueYear() {
  const cost = Number(form.fixedAssetCost) || 0
  const rate = Number(form.fixedAssetDepreciationRate) || 0
  const value = (cost * rate) / 100
  form.fixedAssetDepreciationValueYear = Math.round(value)
}

const typeMap = computed(() => {
  const byId = {}
  for (const t of props.assetTypes || []) {
    const id = t.fixedAssetTypeId ?? t.id
    byId[id] = {
      name: t.fixedAssetTypeName ?? t.name ?? "",
      code: t.fixedAssetTypeCode ?? t.code ?? "",
      depreciationRate: Number(t.depreciationRate ?? t.fixedAssetDepreciationRate ?? 0),
      lifeYear: Number(t.lifeYear ?? t.fixedAssetUsingYear ?? 0),
    }
  }
  return byId
})

const isFillingFromAsset = ref(false)

// Áp mặc định tỷ lệ hao mòn và số năm sử dụng theo loại tài sản
function applyTypeDefaultsById(id, force = false) {
  const selected = (props.assetTypes || []).find((t) => (t.fixedAssetTypeId ?? t.id) == id)
  if (!selected) return
  if (props.mode !== "edit" || force) {
    form.fixedAssetDepreciationRate = Number(selected.depreciationRate ?? selected.fixedAssetDepreciationRate ?? 0)
    form.fixedAssetUsingYear = Number(selected.lifeYear ?? selected.fixedAssetUsingYear ?? 0)
  }
}

// Xử lý đổi loại tài sản (set touched + áp defaults)
function handleTypeChange(eventOrId) {
  const id = eventOrId?.target?.value ?? eventOrId
  markTouched("fixedAssetTypeId")
  applyTypeDefaultsById(id, true)
}

// Toggle dropdown loại tài sản
function toggleTypeDropdown() {
  isTypeDropdownOpen.value = !isTypeDropdownOpen.value
  if (isTypeDropdownOpen.value) isDepartmentDropdownOpen.value = false
}

// Toggle dropdown bộ phận sử dụng
function toggleDepartmentDropdown() {
  isDepartmentDropdownOpen.value = !isDepartmentDropdownOpen.value
  if (isDepartmentDropdownOpen.value) isTypeDropdownOpen.value = false
}

// Đóng tất cả dropdown
function closeDropdowns() {
  isTypeDropdownOpen.value = false
  isDepartmentDropdownOpen.value = false
}

// Chọn loại tài sản và đóng dropdown
function selectType(id) {
  form.fixedAssetTypeId = id
  handleTypeChange(id)
  closeDropdowns()
}

// Chọn bộ phận sử dụng và đóng dropdown
function selectDepartment(id) {
  form.fixedAssetDepartmentId = id
  markTouched("fixedAssetDepartmentId")
  closeDropdowns()
}

// Click ngoài dropdown thì đóng
function onGlobalClick(e) {
  if (!e.target?.closest?.(".combo-wrap")) closeDropdowns()
}

// Chuẩn hóa ngày về định dạng input[type=date]
function normalizeToInputDate(value) {
  if (!value) return ""
  const s = String(value).trim()
  if (/^\d{4}-\d{2}-\d{2}/.test(s)) return s.slice(0, 10)
  const m = s.match(/^(\d{1,2})[\/\-](\d{1,2})[\/\-](\d{4})$/)
  if (m) {
    const dd = String(m[1]).padStart(2, "0")
    const mm = String(m[2]).padStart(2, "0")
    const yyyy = m[3]
    return `${yyyy}-${mm}-${dd}`
  }
  return ""
}

// Đóng popup (internal)
// Đóng popup và reset trạng thái confirm
function closePopup() {
  isConfirmOpen.value = false
  emit("update:modelValue", false)
}

// Khi bấm X / Hủy / click ra ngoài
const confirmConfig = ref(null)

// Mở confirm khi người dùng muốn đóng popup
function requestClose() {
  // ====== THÊM MỚI ======
  if (props.mode !== "edit") {
    confirmConfig.value = {
      title: "",
      message: "Bạn có muốn Hủy bỏ khai báo tài sản này?",
      buttons: [
        { label: "Không", action: "cancel" },
        { label: "Hủy bỏ", action: "discard", primary: true },
      ],
    }
    isConfirmOpen.value = true
    return
  }

  // ====== SỬA ======
  if (!isDirty.value) {
    closePopup()
    return
  }

  confirmConfig.value = {
    title: "",
    message: "Thông tin thay đổi sẽ không được cập nhật nếu bạn không Lưu. Bạn có muốn Lưu các thay đổi này?",
    buttons: [
      { label: "Hủy bỏ", action: "cancel" },
      { label: "Không Lưu", action: "discard" },
      { label: "Lưu", action: "save", primary: true },
    ],
  }
  isConfirmOpen.value = true
}

// Xử lý action của confirm popup
function handleConfirmAction(action) {
  if (action === "cancel") {
    isConfirmOpen.value = false
    return
  }
  if (action === "discard") {
    isConfirmOpen.value = false
    closePopup()
    return
  }
  if (action === "save") {
    onSave()
  }
}

watch(
  () => form.fixedAssetDepartmentId,
  (id) => {
    form.fixedAssetDepartmentName = departmentMap.value[id]?.name || ""
  }
)


watch(
  () => form.fixedAssetTypeId,
  (id) => {
    const meta = typeMap.value[id] || {}
    form.fixedAssetTypeName = meta.name || ""
    if (!id) {
      if (props.mode !== "edit") {
        form.fixedAssetDepreciationRate = 0
        form.fixedAssetUsingYear = 0
      }
      return
    }
    if (props.mode !== "edit" && !isFillingFromAsset.value) {
      if (meta.depreciationRate !== undefined) form.fixedAssetDepreciationRate = meta.depreciationRate
      if (meta.lifeYear !== undefined) form.fixedAssetUsingYear = meta.lifeYear
      if (meta.depreciationRate === undefined && meta.lifeYear === undefined) {
        applyTypeDefaultsById(id)
      }
    }
  }
)



watch(
  () => [form.fixedAssetCost, form.fixedAssetDepreciationRate],
  () => recalcDepreciationValueYear(),
  { immediate: true }
)

watch(() => form.fixedAssetCost, () => syncCostDisplay(), { immediate: true })
watch(() => form.fixedAssetDepreciationValueYear, () => syncDepreciationDisplay(), { immediate: true })

watch(
  () => props.asset,
  (val) => {
    if (!val) return

    // ❗ CHỈ FILL KHI EDIT
    if (props.mode !== "edit") return

    isFillingFromAsset.value = true

    form.fixedAssetId = val.fixedAssetId ?? val.FixedAssetId ?? val.id
    form.fixedAssetCode = val.fixedAssetCode ?? val.FixedAssetCode ?? ""
    form.fixedAssetName = val.fixedAssetName ?? val.FixedAssetName ?? ""
    form.fixedAssetDepartmentId =
      val.fixedAssetDepartmentId ?? val.FixedAssetDepartmentId ?? val.departmentId ?? ""
    form.fixedAssetDepartmentName =
      val.fixedAssetDepartmentName ?? val.FixedAssetDepartmentName ?? ""
    form.fixedAssetTypeId = val.fixedAssetTypeId ?? val.FixedAssetTypeId ?? val.assetTypeId ?? ""
    form.fixedAssetTypeName = val.fixedAssetTypeName ?? val.FixedAssetTypeName ?? ""
    const purchaseDate = normalizeToInputDate(
      val.fixedAssetPurchaseDate ?? val.FixedAssetPurchaseDate ?? val.purchaseDate ?? ""
    )
    const startUsingDate = normalizeToInputDate(
      val.fixedAssetStartUsingDate ?? val.FixedAssetStartUsingDate ?? val.startUsingDate ?? ""
    )
    form.fixedAssetPurchaseDate = purchaseDate || getTodayInputDate()
    form.fixedAssetUsingYear = Number(
      pickFieldValue(
        val,
        ["fixedAssetUsingYear", "usingYear", "lifeYear", "FixedAssetUsingYear", "UsingYear", "LifeYear"],
        0
      )
    )
    form.fixedAssetTrackingYear = Number(
      pickFieldValue(
        val,
        ["fixedAssetTrackingYear", "trackingYear", "FixedAssetTrackingYear", "TrackingYear"],
        new Date().getFullYear()
      )
    )
    form.fixedAssetQuantity = Number(val.fixedAssetQuantity ?? val.FixedAssetQuantity ?? 1)
    form.fixedAssetCost = Number(val.fixedAssetCost ?? val.FixedAssetCost ?? 0)
    form.fixedAssetDepreciationRate = Number(
      pickFieldValue(
        val,
        ["fixedAssetDepreciationRate", "depreciationRate", "FixedAssetDepreciationRate", "DepreciationRate"],
        0
      )
    )
    form.fixedAssetDepreciationValueYear = Number(
      val.fixedAssetDepreciationValueYear ?? val.FixedAssetDepreciationValueYear ?? 0
    )
    form.fixedAssetStartUsingDate = startUsingDate || getTodayInputDate()

    syncCostDisplay()
    syncDepreciationDisplay()

    isFillingFromAsset.value = false
  },
  { immediate: true }
)


const popupTitle = computed(() => (props.mode === "edit" ? "Sửa tài sản" : "Thêm tài sản"))

watch(
  () => props.modelValue,
  async (open) => {
    if (!open) return
    isConfirmOpen.value = false

    if (props.mode === "edit") {
      // edit: dữ liệu sẽ được fill từ watcher props.asset của bạn (như hiện tại)
      resetValidation()
      takeSnapshot()
      return
    }

    // create / clone: luôn clear trước để không dính dữ liệu cũ
    resetFormCreate()

    // clone: copy dữ liệu từ asset được chọn
    if (props.mode === "clone") {
      applyCloneFrom(props.asset)
    }

    // lấy mã mới cho create/clone
    await fetchNewAssetCode()

    resetValidation()
    takeSnapshot()
  }
)

onMounted(() => {
  window.addEventListener("click", onGlobalClick)
})

onBeforeUnmount(() => {
  window.removeEventListener("click", onGlobalClick)
})

// Validate và emit payload khi bấm Lưu
function onSave() {
  if (!validateAll()) return
  recalcDepreciationValueYear()

  const payload = {
    fixedAssetId: form.fixedAssetId ?? props.asset?.fixedAssetId ?? props.asset?.id ?? null,
    fixedAssetCode: form.fixedAssetCode.trim(),
    fixedAssetName: form.fixedAssetName.trim(),
    fixedAssetDepartmentId: form.fixedAssetDepartmentId,
    fixedAssetTypeId: form.fixedAssetTypeId,
    fixedAssetQuantity: Number(form.fixedAssetQuantity || 0),
    fixedAssetCost: Number(form.fixedAssetCost || 0),
    fixedAssetDepreciationRate: Number(form.fixedAssetDepreciationRate || 0),
    fixedAssetPurchaseDate: form.fixedAssetPurchaseDate || null,
    fixedAssetStartUsingDate: form.fixedAssetStartUsingDate || null,
    fixedAssetTrackingYear: Number(form.fixedAssetTrackingYear || new Date().getFullYear()),
    fixedAssetUsingYear: Number(form.fixedAssetUsingYear || 0),
    fixedAssetDepreciationValueYear: Number(form.fixedAssetDepreciationValueYear || 0),
  }
  if (props.mode !== "edit") {
    delete payload.fixedAssetId
  }

  emit("save", payload)
  isConfirmOpen.value = false
}
</script>

<template>
  <div v-if="modelValue" class="popup-mask" @mousedown.self="requestClose">
    <ConfirmPopup v-if="confirmConfig" v-model="isConfirmOpen" :title="confirmConfig.title"
      :message="confirmConfig.message" :buttons="confirmConfig.buttons" @action="handleConfirmAction" />

    <section class="popup" role="dialog" aria-modal="true">
      <!-- Header -->
      <header class="popup-header">
        <h3 class="title">{{ popupTitle }}</h3>
        <button class="btn-icon" type="button" aria-label="Đóng" @click="requestClose">×</button>
      </header>

      <!-- Body -->
      <form class="popup-body" novalidate @submit.prevent="onSave">
        <div class="form-rows">
          <!-- Hàng 1: 1/3 | 2/3 -->
          <div class="form-row form-row--2">
            <div class="row">
              <div class="row-label">Mã tài sản <span class="req">*</span></div>
              <div class="row-ctrl">
                <input v-model="form.fixedAssetCode" class="ctl"
                  :class="{ error: showError('fixedAssetCode') || codeLoadError }" type="text"
                  :readonly="props.mode !== 'edit'" :disabled="isCodeLoading"
                  @blur="markTouched('fixedAssetCode')" />
                <div v-if="isCodeLoading" class="field-hint">Đang tạo mã tài sản…</div>
                <div v-if="codeLoadError" class="field-error">{{ codeLoadError }}</div>
              </div>
            </div>

            <div class="row">
              <div class="row-label">Tên tài sản <span class="req">*</span></div>
              <div class="row-ctrl">
                <input v-model="form.fixedAssetName" class="ctl"
                  :class="{ error: showError('fixedAssetName') }" type="text"
                  placeholder="Nhập tên tài sản" @blur="markTouched('fixedAssetName')" />
                <div v-if="showError('fixedAssetName')" class="field-error">
                  <span class="field-error-text">{{ errors.fixedAssetName?.text }}</span>
                  <span class="field-error-label">{{ errors.fixedAssetName?.label }}</span>       
                </div>
              </div>
            </div>
          </div>

          <!-- Hàng 2: 1/3 | 2/3 -->
          <div class="form-row form-row--2">
            <div class="row">
              <div class="row-label">Mã bộ phận sử dụng <span class="req">*</span></div>
              <div class="row-ctrl">
                <div class="combo-wrap" @click.stop>
                  <button class="ctl combo-input" :class="{ error: showError('fixedAssetDepartmentId') }"
                    type="button" @click="toggleDepartmentDropdown">
                    <span class="combo-text">{{ selectedDepartmentLabel }}</span>
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

                      <button v-for="d in departmentOptions" :key="d.id" class="combo-item"
                        :class="{ active: d.id == form.fixedAssetDepartmentId }" type="button"
                        @click="selectDepartment(d.id)">
                        <span class="combo-col combo-code">{{ d.code }}</span>
                        <span class="combo-col combo-name">{{ d.name }}</span>
                      </button>
                    </div>
                  </div>
                </div>

                <div v-if="showError('fixedAssetDepartmentId')" class="field-error">
                  <span class="field-error-text">{{ errors.fixedAssetDepartmentId?.text }}</span>
                  <span class="field-error-label">{{ errors.fixedAssetDepartmentId?.label }}</span>
                </div>
              </div>
            </div>

            <div class="row">
              <div class="row-label">Tên bộ phận sử dụng</div>
              <div class="row-ctrl">
                <input v-model="form.fixedAssetDepartmentName" class="ctl" type="text" disabled />
              </div>
            </div>
          </div>

          <!-- Hàng 3: 1/3 | 2/3 -->
          <div class="form-row form-row--2">
            <div class="row">
              <div class="row-label">Mã loại tài sản <span class="req">*</span></div>
              <div class="row-ctrl">
                <div class="combo-wrap" @click.stop>
                  <button class="ctl combo-input" :class="{ error: showError('fixedAssetTypeId') }" type="button"
                    @click="toggleTypeDropdown">
                    <span class="combo-text">{{ selectedTypeLabel }}</span>
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

                      <button v-for="t in assetTypeOptions" :key="t.id" class="combo-item"
                        :class="{ active: t.id == form.fixedAssetTypeId }" type="button" @click="selectType(t.id)">
                        <span class="combo-col combo-code">{{ t.code }}</span>
                        <span class="combo-col combo-name">{{ t.name }}</span>
                      </button>
                    </div>
                  </div>
                </div>

                <div v-if="showError('fixedAssetTypeId')" class="field-error">
                  <span class="field-error-text">{{ errors.fixedAssetTypeId?.text }}</span>
                  <span class="field-error-label">{{ errors.fixedAssetTypeId?.label }}</span>
                </div>
              </div>
            </div>

            <div class="row">
              <div class="row-label">Tên loại tài sản</div>
              <div class="row-ctrl">
                <input v-model="form.fixedAssetTypeName" class="ctl" type="text" disabled />
              </div>
            </div>
          </div>

          <!-- Hàng 4: 1/3 | 1/3 | 1/3 -->
          <div class="form-row form-row--3">
            <div class="row">
              <div class="row-label">Số lượng <span class="req">*</span></div>
              <div class="row-ctrl">
                <input v-model.number="form.fixedAssetQuantity" class="ctl right"
                  :class="{ error: showError('fixedAssetQuantity') }" type="number" min="1" step="1"
                  @blur="markTouched('fixedAssetQuantity')" />
                <div v-if="showError('fixedAssetQuantity')" class="field-error">
                  <span class="field-error-label">{{ errors.fixedAssetQuantity?.label }}</span>
                  <span class="field-error-text">{{ errors.fixedAssetQuantity?.text }}</span>
                </div>
              </div>
            </div>

            <div class="row">
              <div class="row-label">Nguyên giá <span class="req">*</span></div>
              <div class="row-ctrl">
                <input :value="costDisplay" class="ctl right" :class="{ error: showError('fixedAssetCost') }"
                  type="text" inputmode="numeric" @input="handleCostInput" @blur="markTouched('fixedAssetCost')" />
                <div v-if="showError('fixedAssetCost')" class="field-error">
                  <span class="field-error-label">{{ errors.fixedAssetCost?.label }}</span>
                  <span class="field-error-text">{{ errors.fixedAssetCost?.text }}</span>
                </div>
              </div>
            </div>

            <div class="row">
              <div class="row-label">Tỷ lệ hao mòn (%) <span class="req">*</span></div>
              <div class="row-ctrl">
                <input v-model.number="form.fixedAssetDepreciationRate" class="ctl right"
                  :class="{ error: showError('fixedAssetDepreciationRate') }" type="number" min="0" step="0.01"
                  @blur="markTouched('fixedAssetDepreciationRate')" />
                <div v-if="showError('fixedAssetDepreciationRate')" class="field-error">
                  <span class="field-error-label">{{ errors.fixedAssetDepreciationRate?.label }}</span>
                  <span class="field-error-text">{{ errors.fixedAssetDepreciationRate?.text }}</span>
                </div>
              </div>
            </div>
          </div>

          <!-- Hàng 5: 1/3 | 1/3 | 1/3 -->
          <div class="form-row form-row--3">
            <div class="row">
              <div class="row-label">Ngày mua <span class="req">*</span></div>
              <div class="row-ctrl">
                <input v-model="form.fixedAssetPurchaseDate" class="ctl"
                  :class="{ error: showError('fixedAssetPurchaseDate') }" type="date"
                  @blur="markTouched('fixedAssetPurchaseDate')" />
                <div v-if="showError('fixedAssetPurchaseDate')" class="field-error">
                  <span class="field-error-label">{{ errors.fixedAssetPurchaseDate?.label }}</span>
                  <span class="field-error-text">{{ errors.fixedAssetPurchaseDate?.text }}</span>
                </div>
              </div>
            </div>

            <div class="row">
              <div class="row-label">Ngày bắt đầu sử dụng <span class="req">*</span></div>
              <div class="row-ctrl">
                <input v-model="form.fixedAssetStartUsingDate" class="ctl"
                  :class="{ error: showError('fixedAssetStartUsingDate') }" type="date"
                  @blur="markTouched('fixedAssetStartUsingDate')" />
                <div v-if="showError('fixedAssetStartUsingDate')" class="field-error">
                  <span class="field-error-label">{{ errors.fixedAssetStartUsingDate?.label }}</span>
                  <span class="field-error-text">{{ errors.fixedAssetStartUsingDate?.text }}</span>
                </div>
              </div>
            </div>

            <div class="row">
              <div class="row-label">Năm theo dõi</div>
              <div class="row-ctrl">
                <input v-model.number="form.fixedAssetTrackingYear" class="ctl right" type="number" disabled />
              </div>
            </div>
          </div>

          <!-- Hàng 6: 1/3 | 1/3 | 1/3 -->
          <div class="form-row form-row--3">
            <div class="row">
              <div class="row-label">Số năm sử dụng <span class="req">*</span></div>
              <div class="row-ctrl">
                <input v-model.number="form.fixedAssetUsingYear" class="ctl right"
                  :class="{ error: showError('fixedAssetUsingYear') }" type="number" min="0" step="1"
                  @blur="markTouched('fixedAssetUsingYear')" />
                <div v-if="showError('fixedAssetUsingYear')" class="field-error">
                  <span class="field-error-label">{{ errors.fixedAssetUsingYear?.label }}</span>
                  <span class="field-error-text">{{ errors.fixedAssetUsingYear?.text }}</span>
                </div>
              </div>
            </div>

            <div class="row">
              <div class="row-label">Giá trị hao mòn năm</div>
              <div class="row-ctrl">
                <input :value="depreciationDisplay" class="ctl right" type="text" inputmode="numeric" readonly />
                <div v-if="showError('fixedAssetDepreciationValueYear')" class="field-error">
                  <span class="field-error-label">{{ errors.fixedAssetDepreciationValueYear?.label }}</span>
                  <span class="field-error-text">{{ errors.fixedAssetDepreciationValueYear?.text }}</span>
                </div>
              </div>
            </div>

            <!-- cột 3 để trống nếu bạn muốn canh đều -->
          </div>
        </div>

        <!-- Footer -->
        <footer class="popup-footer">
          <button class="btn" type="button" @click="requestClose">Hủy</button>
          <button class="btn primary" type="submit">Lưu</button>
        </footer>
      </form>
    </section>
  </div>
</template>

<style scoped>
/* ====== Popup style theo guideline (2026-01-14) ====== */
/* Icon */
.icon {
  width: 20px;
  height: 20px;
  display: inline-flex;
  background-image: url("@/assets/icons/qlts-icon.svg");
  background-repeat: no-repeat;
  background-size: 504px 754px;
}

.icon-dropdown {
  background-position: -72px -338px;
  width: 7px;
  height: 5px;
}

.popup-mask {
  --line: #e6e8ed;
  --line2: #d9dde6;
  --text: #1f1f1f;
  --sub: #666;
  --dis: #f5f6f8;
  --pri: #2bb5ff;
  --pri2: rgba(43, 181, 255, 0.15);

  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.35);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 16px;
  z-index: 9999;
}

.popup {
  width: 920px;
  max-width: 100%;
  background: #fff;
  border-radius: 6px;
  box-shadow: 0 12px 32px rgba(0, 0, 0, 0.22);
  overflow: hidden;
  font-family: Roboto, Arial, sans-serif;
}

.popup-header {
  height: 52px;
  padding: 0 20px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  border-bottom: 1px solid var(--line);
}

.title {
  margin: 0;
  font-size: 16px;
  font-weight: 700;
  color: var(--text);
}

.btn-icon {
  width: 32px;
  height: 32px;
  border-radius: 4px;
  border: 1px solid transparent;
  background: transparent;
  cursor: pointer;
  color: #333;
  font-size: 22px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.btn-icon:hover {
  background: #f2f4f7;
}

.popup-body {
  padding: 20px 22px 16px;
  max-height: calc(100vh - 32px - 52px);
  overflow: auto;
}

/* ===== Layout theo yêu cầu: 6 hàng ===== */
.form-rows {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.form-row {
  display: grid;
  gap: 16px;
}

/* 3 hàng đầu: 1/3 - 2/3 */
.form-row--2 {
  grid-template-columns: 1fr 2fr;
}

/* 3 hàng sau: 1/3 - 1/3 - 1/3 */
.form-row--3 {
  grid-template-columns: repeat(3, 1fr);
}

.row--empty .row-label {
  visibility: hidden;
}

.ctl--ghost {
  height: 34px;
  border: 1px solid transparent;
  background: transparent;
}

@media (max-width: 900px) {
  .popup {
    width: 96vw;
  }

  .form-row--3 {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (max-width: 640px) {
  .form-row--2 {
    grid-template-columns: 1fr;
  }

  .form-row--3 {
    grid-template-columns: 1fr;
  }
}

/* Mỗi ô: label trên, control dưới */
.row {
  display: flex;
  flex-direction: column;
  gap: 6px;
  padding: 4px 0;
}

.row-label {
  font-size: 13px;
  font-weight: 600;
  color: var(--text);
}

.req {
  color: #e61d1d;
  font-weight: 700;
}

.row-ctrl {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

/* control */
.ita {
  font-style: italic;
}
.heavy {
  font-weight: 700;
}
.ctl {
  width: 100%;
  height: 34px;
  padding: 0 12px;
  border: 1px solid #b9b9b9;
  border-radius: 4px;
  outline: none;
  font-size: 13px;
  color: var(--text);
  background: #fff;
  box-sizing: border-box;
}

.ctl:hover {
  border-color: var(--line2);
}

.ctl:focus {
  border-color: var(--pri);
  box-shadow: 0 0 0 2px var(--pri2);
}

.ctl.error {
  border-color: #e61d1d;
}

.ctl.error:focus {
  border-color: #e61d1d;
  box-shadow: 0 0 0 2px rgba(230, 29, 29, 0.18);
}

.ctl:disabled {
  background: var(--dis);
  color: var(--sub);
}

.right {
  text-align: right;
}

.field-error {
  color: #e61d1d;
  font-size: 12px;
  line-height: 1.2;
}

.field-error-label {
  font-weight: 700;
  margin-right: 4px;
}

.field-error-text {
  font-weight: 400;
}

/* combo dropdown */
.combo-wrap {
  position: relative;
}

.combo-input {
  display: inline-flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  width: 100%;
  text-align: left;
  cursor: pointer;
  min-width: 0;
}

.combo-text {
  flex: 1 1 auto;
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
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

/* footer */
.popup-footer {
  margin-top: 18px;
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

.btn {
  min-width: 90px;
  height: 34px;
  padding: 0 18px;
  border-radius: 4px;
  border: 1px solid var(--line2);
  background: #fff;
  cursor: pointer;
  font-size: 13px;
  color: var(--text);
}

.btn:hover {
  background: #f2f4f7;
}

.btn.primary {
  background: var(--pri);
  border-color: var(--pri);
  color: #fff;
  font-weight: 600;
}

.btn.primary:hover {
  filter: brightness(0.98);
}
</style>
