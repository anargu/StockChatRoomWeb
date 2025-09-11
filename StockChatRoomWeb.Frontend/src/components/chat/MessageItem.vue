<template>
  <div 
    class="py-2 px-4 hover:bg-gray-50 transition-colors duration-150"
    :class="messageClasses"
  >
    <div class="flex items-start space-x-3">
      <div class="flex-shrink-0">
        <div class="w-8 h-8 rounded-full bg-blue-500 flex items-center justify-center">
          <span class="text-white text-sm font-medium">
            {{ userInitials }}
          </span>
        </div>
      </div>
      
      <div class="flex-1 min-w-0">
        <div class="flex items-baseline space-x-2">
          <p class="text-sm font-medium text-gray-900">
            {{ message.username }}
          </p>
          <p class="text-xs text-gray-500">
            {{ formattedTime }}
          </p>
        </div>
        
        <div class="mt-1">
          <p 
            class="text-sm"
            :class="messageContentClasses"
          >
            {{ message.content }}
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { MESSAGE_TYPES } from '@/utils/constants'

const props = defineProps({
  message: {
    type: Object,
    required: true
  }
})

const userInitials = computed(() => {
  const username = props.message.username || 'U'
  return username.charAt(0).toUpperCase()
})

const formattedTime = computed(() => {
  const date = new Date(props.message.createdAt)
  return date.toLocaleTimeString('en-US', {
    hour: '2-digit',
    minute: '2-digit',
    hour12: false
  })
})

const messageClasses = computed(() => ({
  'bg-blue-50': props.message.messageType === MESSAGE_TYPES.STOCK_COMMAND,
  'bg-green-50': props.message.messageType === MESSAGE_TYPES.STOCK_RESPONSE
}))

const messageContentClasses = computed(() => ({
  'text-gray-700': props.message.messageType === MESSAGE_TYPES.NORMAL,
  'text-blue-700 font-mono': props.message.messageType === MESSAGE_TYPES.STOCK_COMMAND,
  'text-green-700 font-mono': props.message.messageType === MESSAGE_TYPES.STOCK_RESPONSE
}))
</script>