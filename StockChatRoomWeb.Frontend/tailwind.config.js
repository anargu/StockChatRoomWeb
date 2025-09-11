/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{vue,js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        'chat-bg': '#f8fafc',
        'message-user': '#3b82f6',
        'message-bot': '#10b981',
        'message-stock': '#f59e0b'
      }
    },
  },
  plugins: [],
}