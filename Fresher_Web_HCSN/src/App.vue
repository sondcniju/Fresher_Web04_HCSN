<template>
  <div class="app">
    <div class="app-sidebar">
      <AppSidebar />
    </div>
   
    <div class="app-body">
        <AppHeader />
        <RouterView />  
    </div>
  </div>
</template>

<script setup>
import AppHeader from './components/layout/AppHeader.vue'
import AppSidebar from './components/layout/AppSidebar.vue'
</script>
<style scoped>
    .app {
        width: 100%;
        min-height: 100vh;
        overflow: hidden;
        display: flex;
    }
    .app-sidebar {
        display: flex;
        flex-shrink: 0;
    }
    .app-body {
        display: flex;
        flex-direction: column;
        flex: 1;
        min-width: 0;
        height: 100%;
    }
</style>
