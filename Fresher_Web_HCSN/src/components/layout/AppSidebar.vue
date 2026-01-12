<script setup>
import { ref, computed } from "vue"

const isCollapsed = ref(false)
const activeKey = ref("recruitment")
const navItems = [
  { key: "recruitment", label: "Tổng quan", icon: "icon-sb-recrument" },
  { key: "candidate", label: "Tài sản", icon: "icon-candidate" },
  { key: "calendar", label: "Tài sản HT-DB", icon: "icon-calendar" },
  { key: "talent-pool", label: "Công cụ dụng cụ", icon: "icon-storage" },
  { key: "campaign", label: "Danh mục", icon: "icon-marketing" },
  { key: "job", label: "Tra cứu", icon: "icon-job" },
  { key: "ai-marketing", label: "Báo cáo", icon: "icon-aimarketing" },
]
const toggleText = computed(() => (isCollapsed.value ? "Mở rộng" : "Thu gọn"))

const toggleSidebar = () => {
  isCollapsed.value = !isCollapsed.value
}
</script>

<template>
    <div class="sidebar-image">
        <div class="sidebar-title">MISA QLTS</div>
      <div class="sidebar-inner">
        <nav class="sidebar-menu display-flex flex-direction-column">
          <a v-for="item in navItems" :key="item.key" class="nav-item"
            :class="{ active: activeKey === item.key }" @click="activeKey = item.key">
            <span class="icon" :class="item.icon"></span>
            <span class="label">{{ item.label }}</span>
          </a>
          <div class="sidebar-footer">
            <button
              class="collapse-btn sidebar-collapse-btn"
              type="button"
              id="collapseBtn"
              :aria-pressed="isCollapsed"
              @click="toggleSidebar"
            >
              <span class="icon icon-shorten collape-main-icon"></span>
              <span class="label-btn collapse-label">{{ toggleText }}</span>
            </button>
          </div>
        </nav>
      </div>
    </div>
</template>

<style scoped>
  /* Sidebar token */
:root {
    --sidebar-w: 233px;
    --sidebar-collapsed-w: 72px;
}

/* Sidebar wrapper */
.sidebar {
    height: 100vh;
    min-width: var(--sidebar-w);
    width: var(--sidebar-w);
    color: #fff;
    position: fixed;
    top: 0;
    left: 0;
    z-index: 10;
    overflow: hidden;
}

/* Sidebar overlay */
.sidebar-inner {
    height: calc(100% + 48px);
    width: 100%;
    padding-top: 32px;
    padding-bottom: 16px;
}

/* Sidebar background carrier */
.sidebar-image {
    background-position: bottom;
    background-repeat: no-repeat;
    background-size: cover;
    background-color: #051529; 
    height: calc(100% + 48px);
    width: 100%;
}

.sidebar-title {
    font-family: 'Inter', sans-serif;
    font-weight: 700;
    font-size: 20px;
    line-height: 24px;
    color: #FFFFFF;
    padding-left: 16px;
    margin-bottom: 24px;
}

/* Sidebar menu container */
.sidebar .sidebar-inner .sidebar-menu {
    color: #c5ccd5;
    border-radius: 4px;
    margin-top: unset;
}
.sidebar-item{
    font-weight: 500;
    position: relative;
    font-size: 14px;
}
.item-content{
    color: #c5ccd5;
    border-radius: 4px;
    padding: 8px 12px;
    margin: 12px;
    margin-top: unset;
}
/* Menu item */
.nav-item {
    display: flex;
    align-items: center;
    height: 40px;
    min-width: 209px;
    gap: 12px;
    margin: 0 12px 12px;
    padding: 8px 12px;
    border-radius: 6px;
    color: #d8dce3;
    text-decoration: none;
    cursor: pointer;

    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    font-size: 14px;
}

/* Menu item icon */
.nav-item .icon {
    filter: brightness(0) invert(1);
    display: inline-block;
}

/* Hover va active item */
.nav-item:hover {
    background: rgba(255, 255, 255, 0.16);
    color: #fff;
}

.nav-item.active {
    background: var(--primary);
    color: #fff;
}



/* Nhan item dung elip khi collapse */
.nav-item .label {
    flex: 1;
    min-width: 0;
}

/* Sidebar footer va nut collapse */
.sidebar-footer {
    position: absolute;
    bottom: 16px;
    padding: 0 12px;
}

.collapse-btn {
    border: 1px solid white;
    color: #fff;
    cursor: pointer;
    height: 40px;
    border-radius: 4px;
    background-color: rgba(255, 255, 255, 0.1);
    display: flex;
    align-items: center;
    min-width: 209px;
    gap: 10px;
}

.collapse-btn.label-btn{
    font-weight: 500;
    margin-left: 12px;
    font-size: 14px;
}

.collapse-btn:hover {
    background: rgba(255, 255, 255, 0.10);
}

.collapse-btn .chev {
    font-size: 18px;
    line-height: 1;
}

.collapse-caret {
    display: inline-block;
    transition: transform 0.2s ease;
}

.collape-main-icon {
    display: inline-block;
}

/* Trang thai collapsed */
.sidebar.collapsed {
    width: var(--sidebar-collapsed-w);
    min-width: var(--sidebar-collapsed-w);
}

.sidebar.collapsed .sidebar-image,
.sidebar.collapsed .sidebar-inner {
    width: var(--sidebar-collapsed-w);
}

/* An label trong collapsed */
.sidebar.collapsed .nav-item .label ,.sidebar.collapsed .label-btn{
    display: none;
}

/* An text dropdown khi collapsed */
.sidebar.collapsed .nav-dropdown-toggle span:not(.icon) {
    display: none;
}

/* Active trong collapsed giu pill xanh */
.sidebar.collapsed .nav-item.active {
    background: var(--primary);
}

/* Footer button chi con icon */
.sidebar.collapsed .collapse-btn {
    height: 40px;
    width: 48px;
    min-width: 48px;
    border-radius: 4px;
    background-color: rgba(255, 255, 255, 0.1);
    padding: 8px;
}

.sidebar.collapsed .collape-main-icon {
    transform: rotate(180deg);
}

/* An caret dropdown khi collapsed */
.sidebar.collapsed .nav-dropdown-toggle .caret {
    display: none;
}

/* Icon trong collapsed giu canh */
.sidebar.collapsed .nav-item .icon {
    display: inline-block !important;
    margin: 0 !important;
}

.nav-submenu {
    display: flex;
    flex-direction: column;
    gap: 6px;
}

.nav-submenu .nav-item {
    padding-left: 28px;
}

.nav-submenu.is-collapsed {
    max-height: 0;
    overflow: hidden;
    opacity: 0;
    transition: max-height 0.25s ease, opacity 0.25s ease;
}

.nav-submenu.is-open {
    max-height: 400px;
    opacity: 1;
}

.collapse-label {
    font-family: Inter;
    color: #fff !important;
    font-weight: 500;
    margin-left: 12px;
    font-size: 14px;
}

.sidebar-collapse-btn {
    display: flex;
    align-items: center;
    gap: 10px;
    width: 100%;
    border: 1px solid #fff;
    background: rgba(255, 255, 255, 0.1);
    color: #d8dce3;
    padding: 10px 12px;
    cursor: pointer;
    margin-top: auto;
}

.sidebar-collapse-btn .collapse-caret {
    margin-left: auto;
    display: none;
    transform: rotate(0deg);
    transition: transform 0.2s ease;
}

.sidebar-collapse-btn .icon {
    filter: brightness(0) invert(1);
}

.sidebar.collapsed {
    width: var(--sidebar-collapsed-w);
    min-width: var(--sidebar-collapsed-w);
}

.sidebar.collapsed .sidebar-inner {
    padding-top: 32px;
    padding-bottom: 16px;
}

.sidebar.collapsed .nav-item,
.sidebar.collapsed .nav-dropdown-toggle {
    justify-content: center;
    gap: 0;
    margin: 12px;
    padding: 8px 12px;
    margin-top: unset;
    width: auto;
    min-width: 0;
}

.sidebar.collapsed .nav-item span:not(.icon),
.sidebar.collapsed .nav-dropdown-toggle span:not(.icon) {
    display: none;
}

.sidebar.collapsed .nav-dropdown-toggle .caret {
    display: none;
}

.sidebar.collapsed .sidebar-collapse-btn {
    justify-content: center;
    width: 48px;
    height: 40px;
    padding: 8px;
}

.sidebar.collapsed .sidebar-collapse-btn .collapse-label {
    display: none;
}

.sidebar.collapsed .sidebar-collapse-btn .collapse-main-icon {
    display: none;
}

.sidebar.collapsed .sidebar-collapse-btn .collapse-caret {
    display: inline-block;
    margin-left: 0;
    transform: rotate(180deg);
    font-size: 18px;
    line-height: 1;
    color: #fff;
}
</style>
