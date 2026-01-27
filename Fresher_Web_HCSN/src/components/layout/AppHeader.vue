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
        <span class="icon icon-slc"></span>
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
      <div>
        <button class="icon-btn" type="button" title="Tài khoản">
          <span class="icon icon-user" aria-hidden="true"></span>
        </button>
        <div class="icon icon-dropdown"></div>
      </div>


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
  font-weight: 500;
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
}

.year-select {
  appearance: none;
  border: none;
  background: transparent;
  font-weight: 700;
  font-size: 13px;
  color: #1b2b3a;
  outline: none;
  cursor: pointer;
  position: relative;
}


/* Icon buttons and avatar */
.icon {
  width: 20px;
  height: 20px;
  display: inline-flex;
  background: url(/src/assets/icons/qlts-icon.svg);
  background-repeat: no-repeat;
}

.icon-user {
  background: url(/src/assets/icons/user.svg);
  width: 18px;
  height: 18px;
  border-color: black;
}

.icon-dropdown {
  background-position: -72px -338px;
  width: 7px;
  height: 5px;
  margin-bottom: 5px;
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
.icon-slc {
  background-position: -204px -332px;
	width: 8px;
	height: 16px;
}


.icon-btn {
  width: 34px;
  height: 34px;
  border-radius: 50%;
  border:  #d8dfe8;
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

</style>
