<script setup>
// Mô tả: Popup thêm/sửa tài sản (emit payload để ListView gọi API)
// Ngày tạo: 2026-01-14
import { computed, reactive, watch } from "vue"

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  mode: { type: String, default: "create" }, // create|edit
  asset: { type: Object, default: null },

  departments: { type: Array, default: () => [] },
  assetTypes: { type: Array, default: () => [] },
})

const emit = defineEmits(["update:modelValue", "save"])

const form = reactive({
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

// Mô tả: Tự động tính "Giá trị hao mòn năm" = Nguyên giá * Tỷ lệ hao mòn / 100
// Ngày tạo: 2026-01-16
function recalcDepreciationValueYear() {
  const cost = Number(form.fixedAssetCost) || 0
  const rate = Number(form.fixedAssetDepreciationRate) || 0
  const value = (cost * rate) / 100

  // Tiền VNĐ thường hiển thị số nguyên
  form.fixedAssetDepreciationValueYear = Math.round(value)
}
// Mô tả: Map nhanh id -> name/code cho bộ phận và loại tài sản
// Ngày tạo: 2026-01-15

const departmentMap = computed(() => {
  const byId = {}
  for (const d of props.departments || []) {
    // backend bạn đang dùng key fixedAssetDepartmentId / fixedAssetDepartmentName / fixedAssetDepartmentCode
    const id = d.fixedAssetDepartmentId ?? d.id
    byId[id] = {
      name: d.fixedAssetDepartmentName ?? d.name ?? "",
      code: d.fixedAssetDepartmentCode ?? d.code ?? "",
    }
  }
  return byId
})

const typeMap = computed(() => {
  const byId = {}
  for (const t of props.assetTypes || []) {
    // tuỳ backend: fixedAssetTypeId / fixedAssetTypeName / fixedAssetTypeCode (hoặc id/name/code)
    const id = t.fixedAssetTypeId ?? t.id
    byId[id] = {
      name: t.fixedAssetTypeName ?? t.name ?? "",
      code: t.fixedAssetTypeCode ?? t.code ?? "",
    }
  }
  return byId
})

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

function close() {
  emit("update:modelValue", false)
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
    form.fixedAssetTypeName = typeMap.value[id]?.name || ""
  }
)
watch(
  () => [form.fixedAssetCost, form.fixedAssetDepreciationRate],
  () => recalcDepreciationValueYear(),
  { immediate: true }
)

watch(
  () => props.asset,
  (val) => {
    if (!val) {
      form.fixedAssetCode = ""
      form.fixedAssetName = ""
      form.fixedAssetDepartmentId = ""
      form.fixedAssetDepartmentName = ""
      form.fixedAssetTypeId = ""
      form.fixedAssetTypeName = ""
      form.fixedAssetQuantity = 1
      form.fixedAssetCost = 0
      form.fixedAssetDepreciationRate = 0
      form.fixedAssetPurchaseDate = ""
      form.fixedAssetStartUsingDate = ""
      form.fixedAssetTrackingYear = new Date().getFullYear()
      form.fixedAssetUsingYear = 0
      form.fixedAssetDepreciationValueYear = 0
      return
    }

    form.fixedAssetCode = val.fixedAssetCode ?? val.code ?? ""
    form.fixedAssetName = val.fixedAssetName ?? val.name ?? ""
    form.fixedAssetDepartmentId = val.fixedAssetDepartmentId ?? ""
    form.fixedAssetDepartmentName = val.fixedAssetDepartmentName ?? val.departmentName ?? ""
    form.fixedAssetTypeId = val.fixedAssetTypeId ?? ""
    form.fixedAssetTypeName = val.fixedAssetTypeName ?? val.typeName ?? ""
    form.fixedAssetQuantity = Number(val.fixedAssetQuantity ?? 1)
    form.fixedAssetCost = Number(val.fixedAssetCost ?? 0)
    form.fixedAssetDepreciationRate = Number(val.fixedAssetDepreciationRate ?? 0)
    form.fixedAssetPurchaseDate = normalizeToInputDate(val.fixedAssetPurchaseDate ?? val.purchaseDate)
    form.fixedAssetStartUsingDate = normalizeToInputDate(val.fixedAssetStartUsingDate ?? val.startUsingDate)
    form.fixedAssetTrackingYear = Number(val.fixedAssetTrackingYear ?? new Date().getFullYear())
    form.fixedAssetUsingYear = Number(val.fixedAssetUsingYear ?? 0)
    form.fixedAssetDepreciationValueYear = Number(val.fixedAssetDepreciationValueYear ?? 0)
  },
  { immediate: true }
)

const popupTitle = computed(() => (props.mode === "edit" ? "Sửa tài sản" : "Thêm tài sản"))

function onSave() {
  const payload = {
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
    recalcDepreciationValueYear() // ✅ tính lại trước khi emit


  emit("save", payload)
  close()
}
</script>

<template>
  <div v-if="modelValue" class="popup-mask" @mousedown.self="close">
    <section class="popup" role="dialog" aria-modal="true">
      <!-- Header -->
      <header class="popup-header">
        <h3 class="title">{{ popupTitle }}</h3>
        <button class="btn-icon" type="button" aria-label="Đóng" @click="close">✕</button>
      </header>

      <!-- Body -->
      <form class="popup-body" @submit.prevent="onSave">
        <!-- Form 2 cột giống guideline -->
        <div class="form-2col">
          <!-- LEFT -->
          <div class="form-col">
            <div class="row">
              <div class="row-label">Mã tài sản <span class="req">*</span></div>
              <div class="row-ctrl">
                <input v-model="form.fixedAssetCode" class="ctl" type="text" placeholder="TS00001" required />
              </div>
            </div>

            <div class="row">
              <div class="row-label">Mã bộ phận sử dụng <span class="req">*</span></div>
              <div class="row-ctrl">
                <div class="select-wrap">
                  <select v-model="form.fixedAssetDepartmentId" class="ctl" required>
                    <option value="" disabled>Chọn mã bộ phận</option>
                    <option v-for="d in departments" :key="d.fixedAssetDepartmentId ?? d.id"
                      :value="d.fixedAssetDepartmentId ?? d.id">
                      {{ d.fixedAssetDepartmentCode ?? d.code }}
                    </option>
                  </select>
                  <span class="chev">▾</span>
                </div>
              </div>
            </div>

            <div class="row">
              <div class="row-label">Mã loại tài sản <span class="req">*</span></div>
              <div class="row-ctrl">
                <div class="select-wrap">
                  <select v-model="form.fixedAssetTypeId" class="ctl" required>
                    <option value="" disabled>Chọn mã loại</option>
                    <option v-for="t in assetTypes" :key="t.fixedAssetTypeId ?? t.id"
                      :value="t.fixedAssetTypeId ?? t.id">
                      {{ t.fixedAssetTypeCode ?? t.code }}
                    </option>
                  </select>
                  <span class="chev">▾</span>
                </div>
              </div>
            </div>

            <div class="row">
              <div class="row-label">Số lượng <span class="req">*</span></div>
              <div class="row-ctrl">
                <input v-model.number="form.fixedAssetQuantity" class="ctl right" type="number" min="1" step="1"
                  required />
              </div>
            </div>
          </div>

          <!-- RIGHT -->
          <div class="form-col">
            <div class="row">
              <div class="row-label">Tên tài sản <span class="req">*</span></div>
              <div class="row-ctrl">
                <input v-model="form.fixedAssetName" class="ctl" type="text" placeholder="Nhập tên tài sản" required />
              </div>
            </div>

            <div class="row">
              <div class="row-label">Tên bộ phận sử dụng</div>
              <div class="row-ctrl">
                <input v-model="form.fixedAssetDepartmentName" class="ctl" type="text" disabled />
              </div>
            </div>

            <div class="row">
              <div class="row-label">Tên loại tài sản</div>
              <div class="row-ctrl">
                <input v-model="form.fixedAssetTypeName" class="ctl" type="text" disabled />
              </div>
            </div>

            <div class="row">
              <div class="row-label">Nguyên giá <span class="req">*</span></div>
              <div class="row-ctrl">
                <input v-model.number="form.fixedAssetCost" class="ctl right" type="number" min="0" step="1000"
                  required />
              </div>
            </div>
          </div>
        </div>

        <!-- Khối dưới (full width) giống form guideline -->
        <div class="form-full">
          <div class="row">
            <div class="row-label">Tỷ lệ hao mòn (%) <span class="req">*</span></div>
            <div class="row-ctrl">
              <input v-model.number="form.fixedAssetDepreciationRate" class="ctl right" type="number" min="0"
                step="0.01" required />
            </div>
          </div>

          <div class="row">
            <div class="row-label">Ngày mua <span class="req">*</span></div>
            <div class="row-ctrl">
              <input v-model="form.fixedAssetPurchaseDate" class="ctl" type="date" required />
            </div>
          </div>

          <div class="row">
            <div class="row-label">Ngày bắt đầu sử dụng <span class="req">*</span></div>
            <div class="row-ctrl">
              <input v-model="form.fixedAssetStartUsingDate" class="ctl" type="date" required />
            </div>
          </div>

          <div class="row">
            <div class="row-label">Năm theo dõi</div>
            <div class="row-ctrl">
              <input v-model.number="form.fixedAssetTrackingYear" class="ctl right" type="number" min="1900"
                max="9999" />
            </div>
          </div>

          <div class="row">
            <div class="row-label">Số năm sử dụng <span class="req">*</span></div>
            <div class="row-ctrl">
              <input v-model.number="form.fixedAssetUsingYear" class="ctl right" type="number" min="0" step="1"
                required />
            </div>
          </div>

          <div class="row">
            <div class="row-label">Giá trị hao mòn năm <span class="req">*</span></div>
            <div class="row-ctrl">
              <input v-model.number="form.fixedAssetDepreciationValueYear" class="ctl right" type="number"
                min="0" step="1000" required readonly />
            </div>
          </div>
        </div>

        <!-- Footer -->
        <footer class="popup-footer">
          <button class="btn" type="button" @click="close">Hủy</button>
          <button class="btn primary" type="submit">Lưu</button>
        </footer>
      </form>
    </section>
  </div>
</template>

<style scoped>
/* ====== Popup style theo guideline (2026-01-14) ====== */
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
  width: 760px;
  max-width: 100%;
  background: #fff;
  border-radius: 4px;
  box-shadow: 0 12px 32px rgba(0, 0, 0, 0.22);
  overflow: hidden;
  font-family: Roboto, Arial, sans-serif;
}

.popup-header {
  height: 48px;
  padding: 0 16px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  border-bottom: 1px solid var(--line);
}

.title {
  margin: 0;
  font-size: 18px;
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
  font-size: 18px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.btn-icon:hover {
  background: #f2f4f7;
}

.popup-body {
  padding: 16px;
  max-height: calc(100vh - 32px - 48px);
  overflow: auto;
}

/* ===== Form layout giống guideline ===== */
.form-2col {
  display: grid;
  grid-template-columns: 1fr 1fr;
  column-gap: 16px;
  margin-bottom: 12px;
}

.form-col {
  border: 1px solid var(--line);
  border-radius: 2px;
  overflow: hidden;
}

.form-col+.form-col {
  /* nhìn giống chia đôi 2 khối */
}

.form-full {
  border: 1px solid var(--line);
  border-radius: 2px;
  overflow: hidden;
}

/* mỗi dòng: label trái, control phải */
.row {
  display: flex;
  align-items: center;
  min-height: 32px;
  border-bottom: 1px solid var(--line);
  background: #fff;
}

.row:last-child {
  border-bottom: none;
}

.row-label {
  width: 170px;
  /* gần giống guideline */
  padding: 0 10px;
  font-size: 13px;
  color: var(--text);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.req {
  color: #e61d1d;
  font-weight: 700;
}

.row-ctrl {
  flex: 1;
  padding: 0 10px;
}

/* control */
.ctl {
  width: 100%;
  height: 28px;
  /* guideline nhìn nhỏ hơn 32, nằm trong row 32 */
  padding: 0 10px;
  border: 1px solid var(--line2);
  border-radius: 2px;
  outline: none;
  font-size: 13px;
  color: var(--text);
  background: #fff;
  box-sizing: border-box;
}

.ctl:hover {
  border-color: #c8cdda;
}

.ctl:focus {
  border-color: var(--pri);
  box-shadow: 0 0 0 2px var(--pri2);
}

.ctl:disabled {
  background: var(--dis);
  color: var(--sub);
}

.right {
  text-align: right;
}

/* select */
.select-wrap {
  position: relative;
}

.select-wrap select {
  appearance: none;
  -webkit-appearance: none;
  padding-right: 26px;
  cursor: pointer;
}

.chev {
  position: absolute;
  right: 10px;
  top: 50%;
  transform: translateY(-50%);
  font-size: 12px;
  color: var(--sub);
  pointer-events: none;
}

/* footer */
.popup-footer {
  margin-top: 12px;
  display: flex;
  justify-content: flex-end;
  gap: 10px;
}

.btn {
  min-width: 90px;
  height: 32px;
  padding: 0 14px;
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

/* responsive */
@media (max-width: 820px) {
  .popup {
    width: 100%;
  }

  .form-2col {
    grid-template-columns: 1fr;
    row-gap: 12px;
  }

  .row-label {
    width: 150px;
  }
}
</style>
