import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { STORAGE_KEYS, SIGNALR_METHODS } from '@/utils/constants'

export class SignalRService {
  constructor() {
    this.connection = null
    this.isConnected = false
  }

  async connect() {
    const token = localStorage.getItem(STORAGE_KEYS.JWT_TOKEN)
    
    if (!token) {
      throw new Error('No authentication token found')
    }

    this.connection = new HubConnectionBuilder()
      .withUrl('/chatHub', {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build()

    this.connection.onreconnecting(() => {
      console.log('SignalR reconnecting...')
      this.isConnected = false
    })

    this.connection.onreconnected(() => {
      console.log('SignalR reconnected')
      this.isConnected = true
    })

    this.connection.onclose(() => {
      console.log('SignalR connection closed')
      this.isConnected = false
    })

    await this.connection.start()
    this.isConnected = true
    console.log('SignalR connected successfully')
  }

  // Message handling
  onReceiveMessage(callback) {
    if (this.connection) {
      this.connection.on(SIGNALR_METHODS.RECEIVE_MESSAGE, callback)
    }
  }

  offReceiveMessage() {
    if (this.connection) {
      this.connection.off(SIGNALR_METHODS.RECEIVE_MESSAGE)
    }
  }

  // Global chat room methods (backward compatibility)
  async joinChatRoom() {
    if (this.connection) {
      await this.connection.invoke(SIGNALR_METHODS.JOIN_CHAT_ROOM)
    }
  }

  async leaveChatRoom() {
    if (this.connection) {
      await this.connection.invoke(SIGNALR_METHODS.LEAVE_CHAT_ROOM)
    }
  }

  // Specific room methods
  async joinRoom(roomId) {
    if (this.connection && roomId) {
      await this.connection.invoke(SIGNALR_METHODS.JOIN_ROOM, roomId)
    }
  }

  async leaveRoom(roomId) {
    if (this.connection && roomId) {
      await this.connection.invoke(SIGNALR_METHODS.LEAVE_ROOM, roomId)
    }
  }

  async disconnect() {
    if (this.connection) {
      await this.connection.stop()
      this.connection = null
      this.isConnected = false
    }
  }
}

export const signalRService = new SignalRService()