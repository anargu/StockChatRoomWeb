
export const API_ENDPOINTS = {
  AUTH: {
    LOGIN: `/auth/login`,
    REGISTER: `/auth/register`
  },
  CHAT: {
    MESSAGES: `/chat/messages`
  },
  CHATROOM: {
    BASE: `/chatroom`,
    CREATE: `/chatroom`,
    LIST: `/chatroom`,
    GET_BY_ID: (id) => `/chatroom/${id}`
  }
}

export const MESSAGE_TYPES = {
  NORMAL: 0,
  STOCK_COMMAND: 1,
  STOCK_RESPONSE: 2
}

export const STORAGE_KEYS = {
  JWT_TOKEN: 'jwtToken',
  USER_DATA: 'userData'
}

export const SIGNALR_METHODS = {
  RECEIVE_MESSAGE: 'ReceiveMessage',
  JOIN_CHAT_ROOM: 'JoinChatRoom',
  LEAVE_CHAT_ROOM: 'LeaveChatRoom',
  JOIN_ROOM: 'JoinRoom',
  LEAVE_ROOM: 'LeaveRoom'
}

export const ROOM_CONSTANTS = {
  GLOBAL_ROOM_ID: null,
  GLOBAL_ROOM_NAME: 'Global Chat'
}