<script setup>
// Mô tả: Layout khung app (Header + Sidebar + Main content giống ảnh mẫu)
// Ngày tạo: 2026-01-13
import AppHeader from "./AppHeader.vue"
import AppSidebar from "./appSideBar.vue"

defineProps({
  // Tiêu đề hiển thị trên header
  title: { type: String, default: "Danh sách tài sản" },
})
</script>

<template>
  <div class="app">
    <AppHeader :title="title" />
    <div class="app-body">
      <AppSidebar />
      <main class="main-content">
        <slot />
      </main>
    </div>
  </div>
</template>

<style scoped>
.app {
  min-height: 100vh;
  background: #f3f7fb;
  display: flex;
  flex-direction: column;
}
.app-body {
  flex: 1;
  display: flex;
  min-height: 0;
}
.main-content {
  flex: 1;
  min-width: 0;
  padding: 16px 18px;
  overflow: auto;
}

@media (max-width: 768px) {
  .app-body {
    flex-direction: column;
  }

  .main-content {
    padding: 12px;
  }
}
</style>
