<script setup>
// Mô tả: Header theo ảnh mẫu (tiêu đề trái, cụm cơ quan + năm + icon bên phải)
// Ngày tạo: 2026-01-13

const props = defineProps({
  title: { type: String, default: "Danh sách tài sản" },
  orgName: { type: String, default: "Sở Tài chính" },
  year: { type: Number, default: 2021 },
})

const emit = defineEmits(["update:year"])

// Enum năm (demo) - sau này có thể lấy từ API
// Ngày tạo enum: 2026-01-13
const YEAR_OPTIONS = Object.freeze([2021, 2022, 2023, 2024, 2025, 2026])

function onYearChange(e) {
  const val = Number(e.target.value)
  emit("update:year", val)
}
</script>

<template>
  <header class="header">
    <div class="left">
      <h1 class="title">{{ props.title }}</h1>
    </div>

    <div class="right">
      <div class="org">{{ props.orgName }}</div>

      <div class="year-box">
        <span class="year-label">Năm</span>
        <select class="year-select" :value="props.year" @change="onYearChange">
          <option v-for="y in YEAR_OPTIONS" :key="y" :value="y">{{ y }}</option>
        </select>
      </div>

      <button class="icon-btn" type="button" title="Thông báo">
        <span class="icon icon-update" aria-hidden="true"></span>
      </button>

      <button class="icon-btn" type="button" title="Ứng dụng">
        <span class="icon icon-settings" aria-hidden="true"></span>
      </button>

      <button class="icon-btn" type="button" title="Trợ giúp">
        <span class="icon icon-help" aria-hidden="true"></span>
      </button>

      <button class="avatar" type="button" title="Tài khoản">
        <span class="avatar-dot" aria-hidden="true"></span>
      </button>
    </div>
  </header>
</template>

<style scoped>
.header {
  height: 56px;
  background: #fff;
  border-bottom: 1px solid #e7eef6;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 16px 0 14px;
}
.title {
  margin: 0;
  font-size: 18px;
  font-weight: 700;
  color: #1b2b3a;
}
.right {
  display: flex;
  align-items: center;
  gap: 10px;
}
.org {
  font-size: 14px;
  color: #2b3c4d;
  font-weight: 600;
  margin-right: 6px;
}
.year-box {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  background: #d9f1fb;
  border-radius: 3px;
  padding: 6px 10px;
}
.year-label {
  font-size: 13px;
  color: #1b2b3a;
  font-weight: 600;
}
.year-select {
  appearance: none;
  border: none;
  background: transparent;
  font-weight: 700;
  font-size: 13px;
  color: #1b2b3a;
  padding-right: 16px;
  outline: none;
  cursor: pointer;
  position: relative;
}
.year-box::after {
  content: "▼";
  font-size: 12px;
  color: #1b2b3a;
  margin-left: -14px;
  pointer-events: none;
}

/* Icon buttons and avatar */
.icon {
  width: 20px;
  height: 20px;
  display: inline-flex;
  background: url(/src/assets/icons/qlts-icon.svg);
  background-repeat: no-repeat;
}

.icon-update {
  background-position: -332px -109px;
  width: 16px;
  height: 20px;
}
.icon-settings {
  background-position: -199px -23px;
  width: 18px;
  height: 18px;
}
.icon-help {
  background-position: -67px -67px;
  width: 18px;
  height: 18px;
}
.icon-avatar {
  background-position: -153px -153px;
  width: 18px;
}
.icon-btn {
  width: 34px;
  height: 34px;
  border-radius: 50%;
  border: none;
  background: transparent;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}
.icon-btn:hover {
  background: #f2f5fa;
}
.icon-btn svg {
  width: 18px;
  height: 18px;
  fill: #1b2b3a;
  opacity: 0.9;
}
.avatar {
  width: 34px;
  height: 34px;
  border-radius: 50%;
  border: 1px solid #d7e3ef;
  background: #fff;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}
.avatar-dot {
  width: 18px;
  height: 18px;
  border-radius: 50%;
  background: radial-gradient(circle at 30% 30%, #2bb5ff, #1f6fff);
}
</style>
