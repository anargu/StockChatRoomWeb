import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { STORAGE_KEYS } from '@/utils/constants'

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

  onReceiveMessage(callback) {
    if (this.connection) {
      this.connection.on('ReceiveMessage', callback)
    }
  }

  offReceiveMessage() {
    if (this.connection) {
      this.connection.off('ReceiveMessage')
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