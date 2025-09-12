import api from './api'
import { API_ENDPOINTS } from '@/utils/constants'

export const chatService = {
  // Message endpoints
  async getMessages(chatRoomId = null, count = 50) {
    const params = new URLSearchParams()
    if (chatRoomId) params.append('chatRoomId', chatRoomId)
    if (count !== 50) params.append('count', count.toString())
    
    const queryString = params.toString()
    const url = queryString ? `${API_ENDPOINTS.CHAT.MESSAGES}?${queryString}` : API_ENDPOINTS.CHAT.MESSAGES
    
    const response = await api.get(url)
    return response.data
  },

  async sendMessage(messageData) {
    const response = await api.post(API_ENDPOINTS.CHAT.MESSAGES, messageData)
    return response.data
  },

  // Room endpoints
  async getRooms() {
    const response = await api.get(API_ENDPOINTS.CHATROOM.LIST)
    return response.data
  },

  async createRoom(roomData) {
    const response = await api.post(API_ENDPOINTS.CHATROOM.CREATE, roomData)
    return response.data
  },

  async getRoom(roomId) {
    const response = await api.get(API_ENDPOINTS.CHATROOM.GET_BY_ID(roomId))
    return response.data
  }
}