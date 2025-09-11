import { defineStore } from 'pinia'
import { chatService } from '@/services/chatService'
import { signalRService } from '@/services/signalRService'
import { MESSAGE_TYPES } from '@/utils/constants'

export const useChatStore = defineStore('chat', {
  state: () => ({
    messages: [],
    isConnected: false,
    isLoading: false,
    isSending: false,
    error: null
  }),

  getters: {
    sortedMessages: (state) => 
      [...state.messages].sort((a, b) => new Date(a.createdAt) - new Date(b.createdAt)),
    
    normalMessages: (state) => 
      state.messages.filter(msg => msg.messageType === MESSAGE_TYPES.NORMAL),
    
    stockCommands: (state) => 
      state.messages.filter(msg => msg.messageType === MESSAGE_TYPES.STOCK_COMMAND),
    
    stockResponses: (state) => 
      state.messages.filter(msg => msg.messageType === MESSAGE_TYPES.STOCK_RESPONSE)
  },

  actions: {
    async connectSignalR() {
      try {
        await signalRService.connect()
        this.isConnected = true
        
        // Set up message listener
        signalRService.onReceiveMessage((message) => {
          this.addMessage(message)
        })
        
        console.log('Chat store: SignalR connected')
      } catch (error) {
        console.error('Failed to connect to SignalR:', error)
        this.error = 'Failed to connect to real-time chat'
        this.isConnected = false
      }
    },

    async disconnectSignalR() {
      try {
        signalRService.offReceiveMessage()
        await signalRService.disconnect()
        this.isConnected = false
        console.log('Chat store: SignalR disconnected')
      } catch (error) {
        console.error('Error disconnecting SignalR:', error)
      }
    },

    async loadMessages() {
      this.isLoading = true
      this.error = null
      
      try {
        const response = await chatService.getMessages()
        
        if (response.success) {
          this.messages = response.data || []
        } else {
          this.error = response.message || 'Failed to load messages'
        }
      } catch (error) {
        this.error = error.response?.data?.message || 'Failed to load messages'
        console.error('Error loading messages:', error)
      } finally {
        this.isLoading = false
      }
    },

    async sendMessage(content) {
      if (!content.trim()) return

      this.isSending = true
      this.error = null
      
      try {
        const response = await chatService.sendMessage({ content: content.trim() })
        
        if (!response.success) {
          this.error = response.message || 'Failed to send message'
        }
        // Note: Message will be added via SignalR, not here
      } catch (error) {
        this.error = error.response?.data?.message || 'Failed to send message'
        console.error('Error sending message:', error)
      } finally {
        this.isSending = false
      }
    },

    addMessage(message) {
      // Check if message already exists to avoid duplicates
      const exists = this.messages.some(msg => msg.id === message.id)
      if (!exists) {
        this.messages.push(message)
      }
    },

    clearMessages() {
      this.messages = []
    },

    clearError() {
      this.error = null
    }
  }
})