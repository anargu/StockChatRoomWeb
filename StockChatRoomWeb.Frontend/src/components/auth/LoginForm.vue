<template>
  <Card padding="lg">
    <div class="w-full max-w-md mx-auto">
      <h2 class="text-2xl font-bold text-center text-teal-900 mb-6">
        Sign In
      </h2>

      <Alert 
        v-if="error" 
        type="error" 
        :message="error" 
        :show="!!error"
        dismissible
        @dismiss="clearError"
        class="mb-4"
      />

      <form @submit.prevent="handleSubmit" class="space-y-4">
        <div>
          <label for="email" class="block text-sm font-medium text-gray-700 mb-1">
            Email
          </label>
          <Input
            id="email"
            v-model="form.email"
            type="text"
            placeholder="Enter your email"
            required
            :disabled="isLoading"
            :error="errors.email"
          />
        </div>

        <div>
          <label for="password" class="block text-sm font-medium text-gray-700 mb-1">
            Password
          </label>
          <Input
            id="password"
            v-model="form.password"
            type="password"
            placeholder="Enter your password"
            required
            :disabled="isLoading"
            :error="errors.password"
          />
        </div>

        <Button
          type="submit"
          variant="primary"
          size="md"
          class="w-full"
          :loading="isLoading"
          :disabled="!isFormValid || isLoading"
        >
          Sign In
        </Button>
      </form>

      <div class="mt-6 text-center">
        <p class="text-sm text-gray-600">
          Don't have an account?
          <button
            @click="$emit('switch-to-register')"
            class="font-medium text-blue-600 hover:text-blue-500"
            type="button"
          >
            Sign up
          </button>
        </p>
      </div>
    </div>
  </Card>
</template>

<script setup>
import { ref, computed, reactive } from 'vue'
import { useAuthStore } from '@/stores/authStore'
import Card from '@/components/ui/Card.vue'
import Input from '@/components/ui/Input.vue'
import Button from '@/components/ui/Button.vue'
import Alert from '@/components/ui/Alert.vue'

const authStore = useAuthStore()

const form = reactive({
  email: '',
  password: ''
})

const errors = reactive({
  email: '',
  password: ''
})

const isLoading = computed(() => authStore.isLoading)
const error = computed(() => authStore.error)

const isFormValid = computed(() => 
  form.email.trim().length > 0 &&
  form.password.length > 0
)

const clearError = () => {
  authStore.clearError()
  errors.email = ''
  errors.password = ''
}

const validateForm = () => {
  let isValid = true
  
  if (!form.email.trim()) {
    errors.email = 'Email is required'
    isValid = false
  } else {
    errors.email = ''
  }
  
  if (!form.password) {
    errors.password = 'Password is required'
    isValid = false
  } else {
    errors.password = ''
  }
  
  return isValid
}

const emit = defineEmits(['switch-to-register', 'login-success'])

const handleSubmit = async () => {
  if (!validateForm()) return
  
  clearError()
  
  const result = await authStore.login({
    email: form.email.trim(),
    password: form.password
  })
  
  if (result.success) {
    form.email = ''
    form.password = ''
    emit('login-success')
  }
}
</script>