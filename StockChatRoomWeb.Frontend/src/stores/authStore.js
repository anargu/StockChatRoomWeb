import { defineStore } from 'pinia'
import { authService } from '@/services/authService'
import { STORAGE_KEYS } from '@/utils/constants'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: JSON.parse(localStorage.getItem(STORAGE_KEYS.USER_DATA) || 'null'),
    token: localStorage.getItem(STORAGE_KEYS.JWT_TOKEN),
    isAuthenticated: !!localStorage.getItem(STORAGE_KEYS.JWT_TOKEN),
    isLoading: false,
    error: null
  }),

  getters: {
    username: (state) => state.user?.username || 'Unknown',
    email: (state) => state.user?.email || ''
  },

  actions: {
    async login(credentials) {
      this.isLoading = true
      this.error = null
      
      try {
        const response = await authService.login(credentials)
        
        if (response.success) {
          this.setAuth(response.data)
          return { success: true }
        } else {
          this.error = response.message || 'Login failed'
          return { success: false, error: this.error }
        }
      } catch (error) {
        this.error = error.response?.data?.message || 'Network error occurred'
        return { success: false, error: this.error }
      } finally {
        this.isLoading = false
      }
    },

    async register(userData) {
      this.isLoading = true
      this.error = null
      
      try {
        const response = await authService.register(userData)
        
        if (response.success) {
          return { success: true, message: response.message }
        } else {
          this.error = response.message || 'Registration failed'
          return { success: false, error: this.error }
        }
      } catch (error) {
        this.error = error.response?.data?.message || 'Network error occurred'
        return { success: false, error: this.error }
      } finally {
        this.isLoading = false
      }
    },

    setAuth(authData) {
      this.user = {
        username: authData.username,
        email: authData.email
      }
      this.token = authData.token
      this.isAuthenticated = true
      
      localStorage.setItem(STORAGE_KEYS.JWT_TOKEN, authData.token)
      localStorage.setItem(STORAGE_KEYS.USER_DATA, JSON.stringify(this.user))
    },

    logout() {
      this.user = null
      this.token = null
      this.isAuthenticated = false
      this.error = null
      
      localStorage.removeItem(STORAGE_KEYS.JWT_TOKEN)
      localStorage.removeItem(STORAGE_KEYS.USER_DATA)
    },

    clearError() {
      this.error = null
    }
  }
})