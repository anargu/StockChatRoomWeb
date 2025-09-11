import api from './api'
import { API_ENDPOINTS } from '@/utils/constants'

export const chatService = {
  async getMessages() {
    const response = await api.get(API_ENDPOINTS.CHAT.MESSAGES)
    return response.data
  },

  async sendMessage(messageData) {
    const response = await api.post(API_ENDPOINTS.CHAT.MESSAGES, messageData)
    return response.data
  }
}