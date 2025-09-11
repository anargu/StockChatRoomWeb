import api from './api'
import { API_ENDPOINTS } from '@/utils/constants'

export const authService = {
  async login(credentials) {
    const response = await api.post(API_ENDPOINTS.AUTH.LOGIN, credentials)
    return response.data
  },

  async register(userData) {
    const response = await api.post(API_ENDPOINTS.AUTH.REGISTER, userData)
    return response.data
  }
}