<template>
  <div class="h-screen flex flex-col">
    <!-- Header -->
    <div class="bg-white shadow-sm border-b border-gray-200 px-4 py-3">
      <div class="flex items-center justify-between">
        <div class="flex items-center space-x-3">
          <h1 class="text-xl font-semibold text-gray-900">Stock Chat Room</h1>
          <div class="flex items-center">
            <div 
              class="w-2 h-2 rounded-full mr-2"
              :class="connectionStatusClasses"
            ></div>
            <span class="text-sm text-gray-600">{{ connectionStatus }}</span>
          </div>
        </div>
        
        <div class="flex items-center space-x-3">
          <span class="text-sm text-gray-600">
            Welcome, {{ username }}
          </span>
          <Button
            variant="outline"
            size="sm"
            @click="handleLogout"
            :disabled="isLoading"
          >
            Logout
          </Button>
        </div>
      </div>
    </div>

    <!-- Messages Area -->
    <div class="flex-1 flex flex-col p-4 bg-gray-50">
      <MessageList />
    </div>

    <!-- Message Input -->
    <MessageInput />
  </div>
</template>

<script setup>
import { computed, onMounted, onUnmounted } from 'vue'
import { useAuthStore } from '@/stores/authStore'
import { useChatStore } from '@/stores/chatStore'
import MessageList from './MessageList.vue'
import MessageInput from './MessageInput.vue'
import Button from '@/components/ui/Button.vue'

const authStore = useAuthStore()
const chatStore = useChatStore()

const username = computed(() => authStore.username)
const isConnected = computed(() => chatStore.isConnected)
const isLoading = computed(() => chatStore.isLoading)

const connectionStatus = computed(() => {
  if (isConnected.value) return 'Connected'
  return 'Disconnected'
})

const connectionStatusClasses = computed(() => ({
  'bg-green-500': isConnected.value,
  'bg-red-500': !isConnected.value
}))

const emit = defineEmits(['logout'])

const handleLogout = async () => {
  try {
    await chatStore.disconnectSignalR()
    authStore.logout()
    emit('logout')
  } catch (error) {
    console.error('Error during logout:', error)
    authStore.logout()
    emit('logout')
  }
}

onMounted(async () => {
  try {
    await chatStore.connectSignalR()
    await chatStore.loadMessages()
  } catch (error) {
    console.error('Error initializing chat:', error)
  }
})

onUnmounted(async () => {
  try {
    await chatStore.disconnectSignalR()
  } catch (error) {
    console.error('Error disconnecting chat:', error)
  }
})
</script>