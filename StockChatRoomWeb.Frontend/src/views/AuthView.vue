<template>
  <div class="min-h-screen bg-gray-50 flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8">
    <div class="max-w-md w-full space-y-8">
      <div v-if="currentView === 'login'">
        <LoginForm 
          @switch-to-register="currentView = 'register'"
          @login-success="handleLoginSuccess"
        />
      </div>
      
      <div v-else>
        <RegisterForm 
          @switch-to-login="currentView = 'login'"
          @register-success="handleRegisterSuccess"
        />
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import LoginForm from '@/components/auth/LoginForm.vue'
import RegisterForm from '@/components/auth/RegisterForm.vue'

const router = useRouter()
const currentView = ref('login')

const handleLoginSuccess = () => {
  router.push('/chat')
}

const handleRegisterSuccess = () => {
  setTimeout(() => {
    currentView.value = 'login'
  }, 2000)
}
</script>