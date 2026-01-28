import { reactive } from "vue"

function isFieldEmpty(value) {
  return (
    value === "" ||
    value === null ||
    value === undefined ||
    (typeof value === "number" && Number.isNaN(value)) ||
    (typeof value === "string" && value.trim() === "")
  )
}

function createAssetPopupValidation({ form, fieldLabels, errors, touched }) {
  function resetValidation() {
    Object.keys(errors).forEach((key) => {
      errors[key] = null
    })
    Object.keys(touched).forEach((key) => {
      touched[key] = false
    })
  }

  function validateField(field) {
    const value = form[field]
    if (isFieldEmpty(value)) {
      const label = fieldLabels[field] || ""
      errors[field] = { label, text: "Cần nhập thông tin " }
    } else {
      errors[field] = null
    }
    return !errors[field]
  }

  function markTouched(field) {
    touched[field] = true
    validateField(field)
  }

  function showError(field) {
    return touched[field] && errors[field]
  }

  function validateAll() {
    let ok = true
    Object.keys(errors).forEach((field) => {
      touched[field] = true
      if (!validateField(field)) ok = false
    })

    const usingYear = Number(form.fixedAssetUsingYear)
    const rate = Number(form.fixedAssetDepreciationRate)
    if (!Number.isNaN(usingYear) && usingYear > 0 && !Number.isNaN(rate)) {
      const expectedRate = 100 / usingYear
      const diff = Math.abs(rate - expectedRate)
      if (diff > 0.0001) {
        errors.fixedAssetDepreciationRate = {
          label: "Tỷ lệ hao mòn",
          text: "Tỷ lệ hao mòn không hợp lệ",
        }
        ok = false
      }
    }

    const cost = Number(form.fixedAssetCost)
    const depreciationValueYear = Number(form.fixedAssetDepreciationValueYear)
    if (!Number.isNaN(cost) && !Number.isNaN(depreciationValueYear) && depreciationValueYear > cost) {
      errors.fixedAssetDepreciationValueYear = {
        label: "Hao mòn năm",
        text: "Giá trị hao mòn năm không hợp lệ",
      }
      ok = false
    }

    return ok
  }

  return { resetValidation, validateField, markTouched, showError, validateAll }
}

export function useAssetPopupValidation(form) {
  const fieldLabels = {
    fixedAssetCode: "Mã tài sản",
    fixedAssetName: "Tên tài sản",
    fixedAssetDepartmentId: "Mã bộ phận sử dụng",
    fixedAssetDepartmentName: "Tên bộ phận sử dụng",
    fixedAssetTypeId: "Mã loại tài sản",
    fixedAssetTypeName: "Tên loại tài sản",
    fixedAssetQuantity: "Số lượng",
    fixedAssetCost: "Nguyên giá",
    fixedAssetDepreciationRate: "Tỷ lệ hao mòn",
    fixedAssetPurchaseDate: "Ngày mua",
    fixedAssetStartUsingDate: "Ngày bắt đầu sử dụng",
    fixedAssetTrackingYear: "Năm theo dõi",
    fixedAssetUsingYear: "Số năm sử dụng",
    fixedAssetDepreciationValueYear: "Giá trị hao mòn năm",
  }

  const errors = reactive({
    fixedAssetCode: null,
    fixedAssetDepartmentId: null,
    fixedAssetTypeId: null,
    fixedAssetQuantity: null,
    fixedAssetName: null,
    fixedAssetCost: null,
    fixedAssetDepreciationRate: null,
    fixedAssetPurchaseDate: null,
    fixedAssetStartUsingDate: null,
    fixedAssetUsingYear: null,
    fixedAssetDepreciationValueYear: null,
  })

  const touched = reactive({
    fixedAssetCode: false,
    fixedAssetDepartmentId: false,
    fixedAssetTypeId: false,
    fixedAssetQuantity: false,
    fixedAssetName: false,
    fixedAssetCost: false,
    fixedAssetDepreciationRate: false,
    fixedAssetPurchaseDate: false,
    fixedAssetStartUsingDate: false,
    fixedAssetUsingYear: false,
    fixedAssetDepreciationValueYear: false,
  })

  const { resetValidation, validateField, markTouched, showError, validateAll } = createAssetPopupValidation({
    form,
    fieldLabels,
    errors,
    touched,
  })

  return { fieldLabels, errors, touched, resetValidation, validateField, markTouched, showError, validateAll }
}
