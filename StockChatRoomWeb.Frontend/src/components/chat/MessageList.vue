<template>
  <div class="flex-1 overflow-hidden">
    <div 
      ref="messagesContainer"
      class="h-full overflow-y-auto border border-gray-200 rounded-lg bg-white"
    >
      <div v-if="isLoading" class="p-4">
        <LoadingSpinner size="sm" text="Loading messages..." />
      </div>
      
      <div v-else-if="currentRoomMessages.length === 0" class="p-8 text-center text-gray-500">
        <p class="text-lg mb-2">No messages in {{ currentRoomName }} yet</p>
        <p class="text-sm">Start the conversation by sending a message!</p>
      </div>
      
      <div v-else class="divide-y divide-gray-100">
        <MessageItem
          v-for="message in sortedCurrentRoomMessages"
          :key="message.id"
          :message="message"
        />
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, nextTick, watch, onMounted } from 'vue'
import { useChatStore } from '@/stores/chatStore'
import MessageItem from './MessageItem.vue'
import LoadingSpinner from '@/components/ui/LoadingSpinner.vue'

const chatStore = useChatStore()
const messagesContainer = ref(null)

const messages = computed(() => chatStore.messages)
const currentRoomMessages = computed(() => chatStore.currentRoomMessages)
const sortedCurrentRoomMessages = computed(() => 
  [...currentRoomMessages.value].sort((a, b) => new Date(a.createdAt) - new Date(b.createdAt))
)
const isLoading = computed(() => chatStore.isLoading)
const currentRoomName = computed(() => chatStore.currentRoomName)

const scrollToBottom = async () => {
  await nextTick()
  if (messagesContainer.value) {
    messagesContainer.value.scrollTop = messagesContainer.value.scrollHeight
  }
}

watch(
  () => currentRoomMessages.value.length,
  () => {
    scrollToBottom()
  }
)

// Also watch for room changes to scroll to bottom
watch(
  () => chatStore.currentRoomId,
  () => {
    scrollToBottom()
  }
)

onMounted(() => {
  scrollToBottom()
})
</script>