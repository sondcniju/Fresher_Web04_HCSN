<script setup>
// Mô tả: Dropdown dạng bảng 2 cột (Mã/Tên) theo guideline
// Ngày tạo: 2026-01-16
import { computed, onBeforeUnmount, onMounted, ref } from "vue"

const props = defineProps({
  modelValue: { type: [String, Number], default: "" },
  items: { type: Array, default: () => [] },

  // mapping field
  valueKey: { type: String, default: "id" },
  codeKey: { type: String, default: "code" },
  nameKey: { type: String, default: "name" },

  placeholder: { type: String, default: "Chọn..." },
  disabled: { type: Boolean, default: false },
  width: { type: [String, Number], default: 260 }, // px or css string
})

const emit = defineEmits(["update:modelValue", "change"])

const open = ref(false)
const rootEl = ref(null)

const selectedItem = computed(() => {
  const v = props.modelValue
  return props.items.find((x) => String(x?.[props.valueKey]) === String(v)) || null
})

const displayText = computed(() => {
  if (!selectedItem.value) return ""
  const c = selectedItem.value?.[props.codeKey] ?? ""
  const n = selectedItem.value?.[props.nameKey] ?? ""
  return c ? `${c} - ${n}` : String(n)
})

function toggle() {
  if (props.disabled) return
  open.value = !open.value
}

function close() {
  open.value = false
}

function pick(item) {
  const v = item?.[props.valueKey] ?? ""
  emit("update:modelValue", v)
  emit("change", item)
  close()
}

function onDocClick(e) {
  if (!open.value) return
  if (!rootEl.value) return
  if (!rootEl.value.contains(e.target)) close()
}

onMounted(() => document.addEventListener("mousedown", onDocClick))
onBeforeUnmount(() => document.removeEventListener("mousedown", onDocClick))

const wStyle = computed(() => {
  const w = props.width
  return { width: typeof w === "number" ? `${w}px` : w }
})
</script>

<template>
  <div ref="rootEl" class="mdd" :class="{ disabled }" :style="wStyle">
    <div class="mdd-input" :class="{ open }" @click="toggle">
      <span class="mdd-text" :class="{ placeholder: !displayText }">
        {{ displayText || placeholder }}
      </span>
      <span class="mdd-chev">▾</span>
    </div>

    <div v-if="open" class="mdd-panel">
      <div class="mdd-head">
        <div class="c1">Mã</div>
        <div class="c2">Tên</div>
      </div>

      <div class="mdd-body">
        <div
          v-for="it in items"
          :key="String(it?.[valueKey])"
          class="mdd-row"
          :class="{ active: String(it?.[valueKey]) === String(modelValue) }"
          @click="pick(it)"
        >
          <div class="c1 ell">{{ it?.[codeKey] }}</div>
          <div class="c2 ell">{{ it?.[nameKey] }}</div>
        </div>

        <div v-if="!items?.length" class="mdd-empty">Không có dữ liệu</div>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* ===== Dropdown table style theo guideline ===== */
.mdd {
  position: relative;
  font-family: Roboto, Arial, sans-serif;
  font-size: 13px;
}
.mdd.disabled {
  opacity: 0.7;
  pointer-events: none;
}

.mdd-input {
  height: 28px;
  border: 1px solid #d9dde6;
  border-radius: 2px;
  background: #fff;
  padding: 0 10px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  cursor: pointer;
}
.mdd-input:hover {
  border-color: #c8cdda;
}
.mdd-input.open {
  border-color: #2bb5ff;
  box-shadow: 0 0 0 2px rgba(43, 181, 255, 0.15);
}

.mdd-text.placeholder {
  color: #8a8f9c;
}
.mdd-chev {
  font-size: 12px;
  color: #666;
  margin-left: 10px;
}

.mdd-panel {
  position: absolute;
  top: calc(100% + 6px);
  left: 0;
  right: 0;
  border: 1px solid #e3e6ef;
  border-radius: 2px;
  background: #fff;
  box-shadow: 0 10px 22px rgba(0, 0, 0, 0.12);
  overflow: hidden;
  z-index: 9999;
}

.mdd-head {
  display: grid;
  grid-template-columns: 90px 1fr;
  background: #bfe7ff; /* xanh nhạt giống guideline */
  color: #111;
  font-weight: 600;
  height: 30px;
  align-items: center;
  padding: 0 10px;
  border-bottom: 1px solid #e3e6ef;
}

.mdd-body {
  max-height: 220px;
  overflow: auto;
}

.mdd-row {
  display: grid;
  grid-template-columns: 90px 1fr;
  height: 30px;
  align-items: center;
  padding: 0 10px;
  border-bottom: 1px solid #eef1f6;
  cursor: pointer;
}
.mdd-row:hover {
  background: #d9f3ff;
}
.mdd-row.active {
  background: #bfe7ff;
}

.c1,
.c2 {
  padding-right: 10px;
}
.ell {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.mdd-empty {
  padding: 10px;
  color: #666;
}
</style>
