import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import AuthView from '@/views/AuthView.vue'
import ChatView from '@/views/ChatView.vue'

const routes = [
  {
    path: '/auth',
    name: 'auth',
    component: AuthView,
    meta: { 
      requiresGuest: true,
      title: 'Authentication'
    }
  },
  {
    path: '/chat',
    name: 'chat', 
    component: ChatView,
    meta: { 
      requiresAuth: true,
      title: 'Stock Chat Room'
    }
  },
  {
    path: '/',
    redirect: () => {
      const authStore = useAuthStore()
      return authStore.isAuthenticated ? '/chat' : '/auth'
    }
  },
  {
    path: '/:pathMatch(.*)*',
    redirect: '/'
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()
  
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    next('/auth')
    return
  }
  
  if (to.meta.requiresGuest && authStore.isAuthenticated) {
    next('/chat')
    return
  }
  
  if (to.meta.title) {
    document.title = to.meta.title
  }
  
  next()
})

export default router