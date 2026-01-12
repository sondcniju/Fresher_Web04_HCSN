<script setup>
// Mô tả: Header theo ảnh mẫu (tiêu đề trái, cụm cơ quan + năm + icon bên phải)
// Ngày tạo: 2026-01-12

const props = defineProps({
  title: { type: String, default: 'Danh sach tai san' },
  orgName: { type: String, default: 'Sở tài chính' },
  year: { type: Number, default: 2021 },
})

const emit = defineEmits(['update:year'])
</script>

<template>
  <header class="header">
    <div class="header-left">
      <div class="header-title">{{ title }}</div>
    </div>

    <div class="header-right">
      <div class="org">{{ orgName }}</div>

      <div class="year">
        <label class="year-label">Năm</label>
        <select class="year-select" :value="year" @change="emit('update:year', Number($event.target.value))">
          <option :value="2021">2021</option>
          <option :value="2022">2022</option>
          <option :value="2023">2023</option>
          <option :value="2024">2024</option>
          <option :value="2025">2025</option>
          <option :value="2026">2026</option>
        </select>
      </div>

      <!-- icon placeholder -->
      <button class="icon-btn" type="button" title="Thông báo">🔔</button>
      <button class="icon-btn" type="button" title="Ứng dụng">▦</button>
      <button class="icon-btn" type="button" title="Trợ giúp">?</button>
      <button class="icon-btn" type="button" title="Tài khoản">👤</button>
      <button class="icon-btn" type="button" title="Menu">▾</button>
    </div>
  </header>
</template>

<style scoped>
.header {
  height: 60px;
  width: 100%;
  padding: 0 20px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  border-bottom: 1px solid #e0e6ed;
  background-color: #ffffff;
}
.header-title {
  font-size: 16px;
  font-weight: 700;
  color: #1b2b3a;
}
.header-right {
  display: flex;
  align-items: center;
  gap: 10px;
}
.org {
  font-size: 14px;
  font-weight: 600;
  color: #1b2b3a;
  margin-right: 8px;
}
.year {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  background: #e9f7ff;
  border-radius: 4px;
  padding: 6px 8px;
  margin-right: 6px;
}
.year-label {
  font-size: 13px;
  color: #1b2b3a;
}
.year-select {
  border: none;
  background: transparent;
  outline: none;
  font-weight: 600;
  cursor: pointer;
}
.icon-btn {
  width: 34px;
  height: 34px;
  border-radius: 50%;
  border: none;
  background: transparent;
  cursor: pointer;
  color: #1b2b3a;
}
.icon-btn:hover {
  background: #f2f5fa;
}
</style>
