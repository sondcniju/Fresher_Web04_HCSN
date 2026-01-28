<script setup>
// Mô tả: Sidebar mặc định mở rộng, bấm để thu gọn chỉ còn icon (giống UI MISA QLTS)
// Ngafy tạo: 2026-01-13
import { computed, onBeforeUnmount, onMounted, ref } from "vue"

// Enum key menu (demo)
// Ngày tạo enum: 2026-01-13
const SidebarKey = Object.freeze({
  Overview: "overview",
  Asset: "asset",
  AssetHTDB: "asset_htdb",
  Tools: "tools",
  Category: "category",
  Lookup: "lookup",
  Report: "report",
})

const activeKey = ref(SidebarKey.Asset)


const AUTO_COLLAPSE_WIDTH = 1024

const isAutoCollapsed = ref(false)
const userCollapsed = ref(false)
const isCollapsed = computed(() => userCollapsed.value || isAutoCollapsed.value)


const items = computed(() => [
  { key: SidebarKey.Overview, title: "Tổng quan", icon: "overview", hasChildren: false },
  { key: SidebarKey.Asset, title: "Tài sản", icon: "asset", hasChildren: true },
  { key: SidebarKey.AssetHTDB, title: "Tài sản HT-ĐB", icon: "flask", hasChildren: true },
  { key: SidebarKey.Tools, title: "Công cụ dụng cụ", icon: "tools", hasChildren: true },
  { key: SidebarKey.Category, title: "Danh mục", icon: "category", hasChildren: false },
  { key: SidebarKey.Lookup, title: "Tra cứu", icon: "search", hasChildren: true },
  { key: SidebarKey.Report, title: "Báo cáo", icon: "report", hasChildren: true },
])

function setActive(key) {
  activeKey.value = key
}

// Toggle thu gọn/mở rộng
function toggleSidebar() {
  userCollapsed.value = !userCollapsed.value
}

function updateAutoCollapse() {
  isAutoCollapsed.value = window.innerWidth <= AUTO_COLLAPSE_WIDTH
}

onMounted(() => {
  updateAutoCollapse()
  window.addEventListener("resize", updateAutoCollapse)
})

onBeforeUnmount(() => {
  window.removeEventListener("resize", updateAutoCollapse)
})
</script>

<template>
  <aside class="sidebar" :class="{ collapsed: isCollapsed }">
    <!-- Header logo + brand -->
    <div class="sidebar-top">
      <div class="brand">
        <div class="icon icon-brand">
        </div>

        <!-- Text brand -->
        <div v-if="!isCollapsed" class="brand-text">
          <div class="brand-name">MISA QLTS</div>
        </div>
      </div>
    </div>

    <!-- Menu -->
    <nav class="nav">
      <button
        v-for="it in items"
        :key="it.key"
        type="button"
        class="nav-item"
        :class="{ active: it.key === activeKey }"
        :title="it.title"
        @click="setActive(it.key)"
      >
        <span class="active-bar" aria-hidden="true"></span>

        <!-- Icon (sprite) -->
        <span class="icon" :class="`icon-${it.icon}`" aria-hidden="true"></span>

        <!-- Text (icon khi collapsed) -->
        <span v-if="!isCollapsed" class="text">{{ it.title }}</span>

        <span v-if="!isCollapsed && it.hasChildren" class="caret" aria-hidden="true">
          <svg viewBox="0 0 24 24"><path d="M7 10l5 5 5-5H7Z" /></svg>
        </span>
      </button>
    </nav>

    <div class="sidebar-bottom">
      <button class="collapse-btn" type="button" :title="isCollapsed ? 'Mở rộng' : 'Thu gọn'" @click="toggleSidebar">
        <svg viewBox="0 0 24 24" aria-hidden="true" :class="{ rotate: isCollapsed }">
          <path d="M15 18 9 12l6-6" />
        </svg> 
      </button>
    </div>
  </aside>
</template>

<style scoped>
/* ============ Layout chung ============ */
.sidebar {
  width: 240px;
  height: 100%;
  background: #1f3b57;
  color: #cfe1f1;
  display: flex;
  flex-direction: column;
  padding: 10px 10px;
  box-sizing: border-box;
  border-right: 1px solid rgba(255, 255, 255, 0.06);
  font-family: Roboto, Arial, sans-serif;
}

.sidebar.collapsed {
  width: 66px; 
  padding: 10px 8px;
}

/* ============ Top / Brand ============ */
.sidebar-top {
  padding: 2px 0 10px;
  padding-left: 8px;
}

.brand {
  display: flex;
  align-items: center;
  gap : 10px;
}



.brand-text .brand-name {
  font-size: 18px;
  font-weight: 700;
  color: #ffffff;
  letter-spacing: 0.2px;
  display: flex;
  align-items: center;
}

/* ============ Nav ============ */
.nav {
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding-top: 6px;
}

.nav-item {
  width: 100%;
  height: 44px;
  border-radius: 8px;
  border: none;
  background: transparent;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 0 10px 0 14px;
  position: relative;
  color: inherit;
}

.nav-item:hover {
  background: rgba(255, 255, 255, 0.06);
}

.nav-item .active-bar {
  position: absolute;
  left: 0;
  width: 6px;
  height: 18px;
  border-radius: 6px;
  background: transparent;
}

.nav-item.active {
  background: #2bb5ff;
  color: #ffffff;
}

.nav-item.active .active-bar {
  background: #2bb5ff;
}

.nav-item.active .icon {
  filter: brightness(0) invert(1);
}
/* Icon */
.icon{
  width: 20px;
  height: 20px;
  display: inline-flex;
  background: url(/src/assets/icons/qlts-icon.svg);
  background-repeat: no-repeat;
}

.icon-brand {
  background-position:  -20px -680px;
	width: 36px;
	height: 36px;
  display: flex;
  align-self: center;
}
.icon-overview {
  background-position:  -21px -153px;
  width: 22px;
	height: 22px;
}
.icon-asset {
  background-position: -65px -153px;
	width: 22px;
	height: 23px;
}
.icon-flask {
  background-position:  -110px -153px;
	width: 21px;
	height: 23px;
}
.icon-tools {
  background-position:  -153px -153px;
	width: 22px;
	height: 22px;
}
.icon-category {
  background-position: -197px -155px;
	width: 22px;
	height: 18px;
} 
.icon-search {
  background-position:  -241px -153px;
	width: 22px;
	height: 22px;
}
.icon-report {
  background-position:  -329px -153px;
	width: 22px;
	height: 22px;
}

.text {
  flex: 1;
  text-align: left;
  font-size: 13px;
  font-weight: 500;
  white-space: nowrap;
}

.caret {
  width: 18px;
  height: 18px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  opacity: 0.9;
}
.caret svg {
  width: 18px;
  height: 18px;
  fill: currentColor;
}

/* ============ Khi collapsed: chỉ icon ============ */
.sidebar.collapsed .nav-item {
  width: 44px;
  padding: 0;
  justify-content: center;
  margin: 0 auto;
}



/* Center brand icon when collapsed */
.sidebar.collapsed .sidebar-top {
  display: flex;
  justify-content: center;
  padding-left: 0;
}

.sidebar.collapsed .brand {
  justify-content: center;
}

@media (max-width: 768px) {
  .sidebar {
    width: 100%;
    height: auto;
    border-right: none;
    border-bottom: 1px solid rgba(255, 255, 255, 0.06);
  }

  .sidebar.collapsed {
    width: 100%;
  }
}

/* ============ Bottom toggle ============ */
.sidebar-bottom {
  margin-top: auto;
  padding-top: 10px;
  display: flex;
  justify-content: flex-end;
}

.sidebar.collapsed .sidebar-bottom {
  justify-content: center;
}

.collapse-btn {
  width: 28px;
  height: 28px;
  border: 1px solid rgba(255, 255, 255, 0.12);
  background: rgba(255, 255, 255, 0.06);
  border-radius: 6px;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.collapse-btn:hover {
  background: rgba(255, 255, 255, 0.1);
}

.collapse-btn svg {
  width: 18px;
  height: 18px;
  fill: none;
  stroke: #ffffff;
  stroke-width: 2;
  stroke-linecap: round;
  stroke-linejoin: round;
}

.collapse-btn svg.rotate {
  transform: rotate(180deg);
}
</style>
