<script setup>
// Mô tả: Popup confirm dùng chung (hủy khai báo / lưu thay đổi / xóa)
// Ngày tạo: 2026-01-17

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  title: { type: String, default: "Thông báo" },
  message: { type: String, default: "" },
  buttons: {
    type: Array,
    default: () => [],
    // [{ label:'Không', action:'cancel' }, { label:'Xóa', action:'delete', danger:true }]
  },
})

const emit = defineEmits(["update:modelValue", "action"])

function onClick(action) {
  emit("update:modelValue", false)
  emit("action", action)
}
</script>

<template>
  <div v-if="modelValue" class="confirm-mask" @mousedown.self="onClick('cancel')">
    <section class="confirm" role="dialog" aria-modal="true">
      <div class="confirm-body">
        <div class="confirm-icon">⚠️</div>
        <div class="confirm-content">
          <div class="confirm-title">{{ title }}</div>
          <div class="confirm-text">{{ message }}</div>
        </div>
      </div>

      <footer class="confirm-footer">
        <button
          v-for="(b, i) in buttons"
          :key="i"
          class="btn"
          :class="{ primary: b.primary, danger: b.danger }"
          type="button"
          @click="onClick(b.action)"
        >
          {{ b.label }}
        </button>
      </footer>
    </section>
  </div>
</template>

<style scoped>
.confirm-mask {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.35);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 16px;
  z-index: 10000;
  font-family: Roboto, Arial, sans-serif;
}
.confirm {
  width: 460px;
  max-width: 100%;
  background: #fff;
  border-radius: 4px;
  box-shadow: 0 12px 32px rgba(0, 0, 0, 0.22);
  padding: 18px 18px 14px;
}
.confirm-body {
  display: flex;
  gap: 12px;
  align-items: flex-start;
}
.confirm-icon {
  width: 32px;
  height: 32px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: 18px;
}
.confirm-title {
  font-size: 14px;
  font-weight: 700;
  margin-bottom: 6px;
  color: #1f1f1f;
}
.confirm-text {
  font-size: 13px;
  line-height: 1.45;
  color: #1f1f1f;
}
.confirm-footer {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  margin-top: 14px;
}

.confirm-footer .btn {
  min-width: 90px;
}
</style>
