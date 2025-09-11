<template>
  <Card padding="lg">
    <div class="w-full max-w-md mx-auto">
      <h2 class="text-2xl font-bold text-center text-gray-900 mb-6">
        Sign Up
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

      <Alert 
        v-if="successMessage" 
        type="success" 
        :message="successMessage" 
        :show="!!successMessage"
        dismissible
        @dismiss="clearSuccess"
        class="mb-4"
      />

      <form @submit.prevent="handleSubmit" class="space-y-4">
        <div>
          <label for="username" class="block text-sm font-medium text-gray-700 mb-1">
            Username
          </label>
          <Input
            id="username"
            v-model="form.username"
            type="text"
            placeholder="Choose a username"
            required
            :disabled="isLoading"
            :error="errors.username"
          />
        </div>

        <div>
          <label for="email" class="block text-sm font-medium text-gray-700 mb-1">
            Email
          </label>
          <Input
            id="email"
            v-model="form.email"
            type="email"
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
            placeholder="Choose a password"
            required
            :disabled="isLoading"
            :error="errors.password"
          />
        </div>

        <div>
          <label for="confirmPassword" class="block text-sm font-medium text-gray-700 mb-1">
            Confirm Password
          </label>
          <Input
            id="confirmPassword"
            v-model="form.confirmPassword"
            type="password"
            placeholder="Confirm your password"
            required
            :disabled="isLoading"
            :error="errors.confirmPassword"
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
          Sign Up
        </Button>
      </form>

      <div class="mt-6 text-center">
        <p class="text-sm text-gray-600">
          Already have an account?
          <button
            @click="$emit('switch-to-login')"
            class="font-medium text-blue-600 hover:text-blue-500"
            type="button"
          >
            Sign in
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
  username: '',
  email: '',
  password: '',
  confirmPassword: ''
})

const errors = reactive({
  username: '',
  email: '',
  password: '',
  confirmPassword: ''
})

const successMessage = ref('')

const isLoading = computed(() => authStore.isLoading)
const error = computed(() => authStore.error)

const isFormValid = computed(() => 
  form.username.trim().length > 0 && 
  form.email.trim().length > 0 && 
  form.password.length >= 6 &&
  form.confirmPassword === form.password
)

const clearError = () => {
  authStore.clearError()
  Object.keys(errors).forEach(key => errors[key] = '')
}

const clearSuccess = () => {
  successMessage.value = ''
}

const validateForm = () => {
  let isValid = true
  
  if (!form.username.trim()) {
    errors.username = 'Username is required'
    isValid = false
  } else if (form.username.trim().length < 3) {
    errors.username = 'Username must be at least 3 characters'
    isValid = false
  } else {
    errors.username = ''
  }
  
  if (!form.email.trim()) {
    errors.email = 'Email is required'
    isValid = false
  } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) {
    errors.email = 'Please enter a valid email address'
    isValid = false
  } else {
    errors.email = ''
  }
  
  if (!form.password) {
    errors.password = 'Password is required'
    isValid = false
  } else if (form.password.length < 6) {
    errors.password = 'Password must be at least 6 characters'
    isValid = false
  } else {
    errors.password = ''
  }
  
  if (!form.confirmPassword) {
    errors.confirmPassword = 'Please confirm your password'
    isValid = false
  } else if (form.password !== form.confirmPassword) {
    errors.confirmPassword = 'Passwords do not match'
    isValid = false
  } else {
    errors.confirmPassword = ''
  }
  
  return isValid
}

const emit = defineEmits(['switch-to-login', 'register-success'])

const handleSubmit = async () => {
  if (!validateForm()) return
  
  clearError()
  clearSuccess()
  
  const result = await authStore.register({
    username: form.username.trim(),
    email: form.email.trim(),
    password: form.password,
    confirmPassword: form.confirmPassword,
  })
  
  if (result.success) {
    successMessage.value = result.message || 'Registration successful! Please sign in.'
    form.username = ''
    form.email = ''
    form.password = ''
    form.confirmPassword = ''
    emit('register-success')
  }
}
</script>