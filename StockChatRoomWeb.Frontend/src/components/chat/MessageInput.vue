<template>
  <div class="bg-white border-t border-gray-200 p-4">
    <Alert 
      v-if="error" 
      type="error" 
      :message="error" 
      :show="!!error"
      dismissible
      @dismiss="clearError"
      class="mb-4"
    />

    <form @submit.prevent="handleSubmit" class="flex items-center space-x-3">
      <div class="flex-1">
        <Input
          v-model="message"
          type="text"
          placeholder="Type your message... (use /stock=symbol for stock quotes)"
          :disabled="isSending || !isConnected"
          @keydown.enter="handleSubmit"
          class="pr-20"
        />
      </div>
      
      <Button
        type="submit"
        variant="primary"
        size="md"
        :loading="isSending"
        :disabled="!canSend"
        class="flex-shrink-0"
      >
        Send
      </Button>
    </form>

    <div class="mt-2 flex items-center justify-between text-xs text-gray-500">
      <div class="flex items-center space-x-2">
        <div class="flex items-center">
          <div 
            class="w-2 h-2 rounded-full mr-1"
            :class="connectionStatusClasses"
          ></div>
          <span>{{ connectionStatus }}</span>
        </div>
        
        <span v-if="message.startsWith('/stock=')" class="text-blue-600">
          Stock command detected
        </span>
      </div>
      
      <div class="text-right">
        <span>{{ message.length }}/500</span>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useChatStore } from '@/stores/chatStore'
import Input from '@/components/ui/Input.vue'
import Button from '@/components/ui/Button.vue'
import Alert from '@/components/ui/Alert.vue'

const chatStore = useChatStore()

const message = ref('')

const isConnected = computed(() => chatStore.isConnected)
const isSending = computed(() => chatStore.isSending)
const error = computed(() => chatStore.error)

const canSend = computed(() => 
  message.value.trim().length > 0 && 
  message.value.length <= 500 &&
  !isSending.value && 
  isConnected.value
)

const connectionStatus = computed(() => {
  if (isConnected.value) return 'Connected'
  return 'Disconnected'
})

const connectionStatusClasses = computed(() => ({
  'bg-green-500': isConnected.value,
  'bg-red-500': !isConnected.value
}))

const clearError = () => {
  chatStore.clearError()
}

const handleSubmit = async () => {
  if (!canSend.value) return
  
  const messageContent = message.value.trim()
  message.value = ''
  
  await chatStore.sendMessage(messageContent)
}
</script>