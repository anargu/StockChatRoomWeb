import { defineStore } from 'pinia'
import { chatService } from '@/services/chatService'
import { signalRService } from '@/services/signalRService'
import { MESSAGE_TYPES, ROOM_CONSTANTS } from '@/utils/constants'

export const useChatStore = defineStore('chat', {
  state: () => ({
    messages: [],
    rooms: [],
    currentRoomId: ROOM_CONSTANTS.GLOBAL_ROOM_ID,
    currentRoomName: ROOM_CONSTANTS.GLOBAL_ROOM_NAME,
    isConnected: false,
    isLoading: false,
    isSending: false,
    isLoadingRooms: false,
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
      state.messages.filter(msg => msg.messageType === MESSAGE_TYPES.STOCK_RESPONSE),

    // Room-specific getters
    currentRoomMessages: (state) => {
      return state.messages.filter(msg => {
        // If current room is global (null), show only global messages
        if (state.currentRoomId === ROOM_CONSTANTS.GLOBAL_ROOM_ID) {
          return msg.chatRoomId === null || msg.chatRoomId === undefined
        }
        // Otherwise show messages for the specific room
        return msg.chatRoomId === state.currentRoomId
      })
    },

    allRoomsWithGlobal: (state) => [
      { 
        id: ROOM_CONSTANTS.GLOBAL_ROOM_ID, 
        name: ROOM_CONSTANTS.GLOBAL_ROOM_NAME, 
        createdAt: new Date(0) 
      },
      ...state.rooms
    ]
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

    async loadMessages(roomId = this.currentRoomId) {
      this.isLoading = true
      this.error = null
      
      try {
        const response = await chatService.getMessages(roomId)
        
        if (response.success) {
          // Replace messages for current room or merge if different rooms
          if (roomId === this.currentRoomId) {
            this.messages = response.data || []
          } else {
            // Remove existing messages for this room and add new ones
            this.messages = this.messages.filter(msg => {
              const msgRoomId = msg.chatRoomId || ROOM_CONSTANTS.GLOBAL_ROOM_ID
              const filterRoomId = roomId || ROOM_CONSTANTS.GLOBAL_ROOM_ID
              return msgRoomId !== filterRoomId
            })
            this.messages.push(...(response.data || []))
          }
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
        const messageData = { 
          content: content.trim(),
          chatRoomId: this.currentRoomId
        }
        
        const response = await chatService.sendMessage(messageData)
        
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
    },

    // Room-specific actions
    async loadRooms() {
      this.isLoadingRooms = true
      this.error = null
      
      try {
        const response = await chatService.getRooms()
        
        if (response.success) {
          this.rooms = response.data || []
        } else {
          this.error = response.message || 'Failed to load rooms'
        }
      } catch (error) {
        this.error = error.response?.data?.message || 'Failed to load rooms'
        console.error('Error loading rooms:', error)
      } finally {
        this.isLoadingRooms = false
      }
    },

    async createRoom(roomName) {
      this.error = null
      
      try {
        const response = await chatService.createRoom({ name: roomName.trim() })
        
        if (response.success) {
          this.rooms.push(response.data)
          return response.data
        } else {
          this.error = response.message || 'Failed to create room'
          return null
        }
      } catch (error) {
        this.error = error.response?.data?.message || 'Failed to create room'
        console.error('Error creating room:', error)
        return null
      }
    },

    async switchToRoom(roomId, roomName) {
      try {
        // Leave current room if it's not global
        if (this.currentRoomId !== ROOM_CONSTANTS.GLOBAL_ROOM_ID) {
          await signalRService.leaveRoom(this.currentRoomId)
        } else {
          await signalRService.leaveChatRoom()
        }

        // Update current room
        this.currentRoomId = roomId
        this.currentRoomName = roomName

        // Join new room
        if (roomId !== ROOM_CONSTANTS.GLOBAL_ROOM_ID) {
          await signalRService.joinRoom(roomId)
        } else {
          await signalRService.joinChatRoom()
        }

        // Load messages for the new room
        await this.loadMessages(roomId)
        
        console.log(`Switched to room: ${roomName}`)
      } catch (error) {
        this.error = 'Failed to switch rooms'
        console.error('Error switching rooms:', error)
      }
    },

    async switchToGlobal() {
      await this.switchToRoom(ROOM_CONSTANTS.GLOBAL_ROOM_ID, ROOM_CONSTANTS.GLOBAL_ROOM_NAME)
    }
  }
})