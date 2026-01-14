import AssetListView from '@/views/assets/AssetListView.vue'
import { createRouter, createWebHistory } from 'vue-router'
const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'assets',
      component: AssetListView,
    },
    // {
    //   path: '/about',
    //   name: 'about',
     
    //   component: () => import('../views/AboutView.vue')
    // }
  ]
})

export default router
