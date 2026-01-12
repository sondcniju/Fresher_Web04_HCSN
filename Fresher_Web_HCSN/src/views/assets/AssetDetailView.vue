<script setup>
import { computed, onMounted, reactive } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAssetStore } from '@/stores/asset.store'
import { DEPARTMENTS, ASSET_TYPES } from '@/constants/masterData'
import { FormMode } from '@/enums/formMode.enum'
import { isRequired, isPositiveInt, isValidISODate } from '@/utils/validate'
import { toNumber } from '@/utils/format'

const route = useRoute()
const router = useRouter()
const store = useAssetStore()

const assetId = computed(() => route.params.id)
const mode = computed(() => route.query.mode || FormMode.CREATE)

const form = reactive(store.createDefaultAsset())
const errors = reactive({})

onMounted(() => {
  // Chức năng: Load dữ liệu + nạp form theo mode (create/edit)
  // Ngày tạo: 2026-01-12
  store.load()

  if (assetId.value) {
    const found = store.assets.find(x => x.id === assetId.value)
    if (found) Object.assign(form, found)
  }
})

function validateForm() {
  // Chức năng: Validate form chi tiết tài sản
  // Ngày tạo: 2026-01-12
  Object.keys(errors).forEach(k => delete errors[k])

  if (!isRequired(form.assetCode)) errors.assetCode = 'Mã tài sản không được để trống'
  if (!isRequired(form.assetName)) errors.assetName = 'Tên tài sản không được để trống'
  if (!isRequired(form.departmentCode)) errors.departmentCode = 'Vui lòng chọn bộ phận sử dụng'
  if (!isRequired(form.assetTypeCode)) errors.assetTypeCode = 'Vui lòng chọn loại tài sản'
  if (!isValidISODate(form.purchaseDate)) errors.purchaseDate = 'Ngày mua không hợp lệ'
  if (!isPositiveInt(form.quantity)) errors.quantity = 'Số lượng phải là số nguyên dương'

  // Check trùng mã (khi sửa thì bỏ qua chính nó)
  if (store.isDuplicateCode(form.assetCode, form.id)) {
    errors.assetCode = 'Mã tài sản đã tồn tại, vui lòng nhập mã khác'
  }

  return Object.keys(errors).length === 0
}

function onChangeDepartment(e) {
  // Chức năng: Tự động điền tên bộ phận khi chọn mã bộ phận
  // Ngày tạo: 2026-01-12
  store.applyDepartment(form, e.target.value)
}

function onChangeAssetType(e) {
  // Chức năng: Tự động điền thông tin theo loại tài sản (tên, số năm sử dụng, tỷ lệ hao mòn)
  // Ngày tạo: 2026-01-12
  store.applyAssetType(form, e.target.value)
}

function onChangePurchaseDate(e) {
  // Chức năng: Tự động điền năm sử dụng + năm bắt đầu theo dõi theo ngày mua
  // Ngày tạo: 2026-01-12
  store.applyPurchaseDate(form, e.target.value)
}

function onChangeOriginalPrice(e) {
  // Chức năng: Khi đổi nguyên giá thì tính lại giá trị hao mòn năm
  // Ngày tạo: 2026-01-12
  form.originalPrice = toNumber(e.target.value)
  store.applyAssetType(form, form.assetTypeCode) // reuse: tự tính depreciationValueYear
}

function onSave() {
  // Chức năng: Lưu (thêm mới hoặc cập nhật) tài sản
  // Ngày tạo: 2026-01-12
  if (!validateForm()) return

  if (assetId.value) {
    store.update(form.id, form)
  } else {
    store.create(form)
  }

  router.push('/tai-san')
}

function onCancel() {
  // Chức năng: Hủy thao tác và quay về danh sách
  // Ngày tạo: 2026-01-12
  router.push('/tai-san')
}
</script>

<template>
  <div>
    <h2 style="margin:0 0 12px;">
      {{ assetId ? 'Chi tiết tài sản' : 'Thêm mới tài sản' }}
    </h2>

    <div style="display:grid; grid-template-columns: 1fr 1fr; gap: 12px; max-width: 900px;">
      <div>
        <label>Mã tài sản *</label>
        <input v-model="form.assetCode" />
        <div v-if="errors.assetCode" style="color:red;">{{ errors.assetCode }}</div>
      </div>

      <div>
        <label>Tên tài sản *</label>
        <input v-model="form.assetName" />
        <div v-if="errors.assetName" style="color:red;">{{ errors.assetName }}</div>
      </div>

      <div>
        <label>Mã bộ phận sử dụng *</label>
        <select :value="form.departmentCode" @change="onChangeDepartment">
          <option value="">-- Chọn --</option>
          <option v-for="d in DEPARTMENTS" :key="d.code" :value="d.code">
            {{ d.code }} - {{ d.name }}
          </option>
        </select>
        <div v-if="errors.departmentCode" style="color:red;">{{ errors.departmentCode }}</div>
      </div>

      <div>
        <label>Tên bộ phận sử dụng</label>
        <input v-model="form.departmentName" disabled />
      </div>

      <div>
        <label>Mã loại tài sản *</label>
        <select :value="form.assetTypeCode" @change="onChangeAssetType">
          <option value="">-- Chọn --</option>
          <option v-for="t in ASSET_TYPES" :key="t.code" :value="t.code">
            {{ t.code }} - {{ t.name }}
          </option>
        </select>
        <div v-if="errors.assetTypeCode" style="color:red;">{{ errors.assetTypeCode }}</div>
      </div>

      <div>
        <label>Tên loại tài sản</label>
        <input v-model="form.assetTypeName" disabled />
      </div>

      <div>
        <label>Ngày mua *</label>
        <input type="date" :value="form.purchaseDate" @change="onChangePurchaseDate" />
        <div v-if="errors.purchaseDate" style="color:red;">{{ errors.purchaseDate }}</div>
      </div>

      <div>
        <label>Năm sử dụng</label>
        <input v-model="form.usingYear" disabled />
      </div>

      <div>
        <label>Số năm sử dụng</label>
        <input v-model="form.lifeYears" disabled />
      </div>

      <div>
        <label>Tỷ lệ hao mòn (%)</label>
        <input v-model="form.depreciationRate" disabled />
      </div>

      <div>
        <label>Số lượng *</label>
        <input v-model="form.quantity" />
        <div v-if="errors.quantity" style="color:red;">{{ errors.quantity }}</div>
      </div>

      <div>
        <label>Nguyên giá</label>
        <input :value="form.originalPrice" @input="onChangeOriginalPrice" />
      </div>

      <div>
        <label>Giá trị hao mòn năm</label>
        <input v-model="form.depreciationValueYear" disabled />
      </div>

      <div>
        <label>Năm bắt đầu theo dõi</label>
        <input v-model="form.trackingStartYear" disabled />
      </div>
    </div>

    <div style="margin-top: 16px; display:flex; gap: 8px;">
      <button @click="onSave">Lưu</button>
      <button @click="onCancel">Hủy</button>
    </div>
  </div>
</template>
