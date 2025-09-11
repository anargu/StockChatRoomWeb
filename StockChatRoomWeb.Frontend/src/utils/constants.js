
export const API_ENDPOINTS = {
  AUTH: {
    LOGIN: `/auth/login`,
    REGISTER: `/auth/register`
  },
  CHAT: {
    MESSAGES: `/chat/messages`
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