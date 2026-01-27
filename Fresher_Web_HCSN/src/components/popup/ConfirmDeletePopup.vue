<script setup>
const props = defineProps({
  modelValue: { type: Boolean, default: false },
  message: { type: String, default: "" },
  okText: { type: String, default: "Xóa" },
  cancelText: { type: String, default: "Không" },
  hideCancel: { type: Boolean, default: false },
})

const emit = defineEmits(["update:modelValue", "confirm", "cancel"])

function close() {
  emit("update:modelValue", false)
}
function onCancel() {
  emit("cancel")
  close()
}
function onConfirm() {
  emit("confirm")
  close()
}
</script>

<template>
  <div v-if="modelValue" class="cd-mask" @mousedown.self="onCancel">
    <div class="cd" role="dialog" aria-modal="true">
      <div class="cd-body">
        <div class="cd-icon" aria-hidden="true">
          <!-- warning triangle -->
          <svg width="44" height="44" viewBox="0 0 64 64">
            <path
              d="M32 6 2 58h60L32 6z"
              fill="#FFC107"
            />
            <rect x="29" y="22" width="6" height="18" rx="3" fill="#fff" />
            <circle cx="32" cy="48" r="3.2" fill="#fff" />
          </svg>
        </div>

        <div class="cd-text">
          {{ message }}
        </div>
      </div>

      <div class="cd-footer">
<button v-if="!hideCancel" class="btn" type="button" @click="onCancel">{{ cancelText }}</button>
        <button class="btn primary" type="button" @click="onConfirm">{{ okText }}</button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.cd-mask {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.35);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 16px;
  z-index: 10000;
}

.cd {
  width: 620px;
  max-width: 100%;
  background: #fff;
  border-radius: 6px;
  box-shadow: 0 14px 40px rgba(0, 0, 0, 0.25);
  padding: 22px 22px 18px;
  font-family: Roboto, Arial, sans-serif;
}

.cd-body {
  display: flex;
  gap: 18px;
  align-items: center;
  padding: 6px 4px 10px;
}

.cd-icon {
  width: 64px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.cd-text {
  font-size: 16px;
  color: #1f1f1f;
  line-height: 1.55;
}

.cd-footer {
  display: flex;
  justify-content: center;
  gap: 18px;
  margin-top: 12px;
}

.cd-footer .btn {
  min-width: 120px;
}

</style>
