import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { fileURLToPath, URL } from 'node:url'

export default defineConfig(async () => {
  const tailwindcss = (await import('@tailwindcss/vite')).default
  
  return {
    plugins: [vue(), tailwindcss()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    }
  },
  server: {
    port: 3000,
    proxy: {
      '/api': {
        target: 'http://localhost:5086',
        changeOrigin: true
      },
      '/chatHub': {
        target: 'http://localhost:5086',
        changeOrigin: true,
        ws: true
      }
    }
  },
  build: {
    outDir: '../StockChatRoomWeb.Api/wwwroot',
    emptyOutDir: true
  }
  }
})