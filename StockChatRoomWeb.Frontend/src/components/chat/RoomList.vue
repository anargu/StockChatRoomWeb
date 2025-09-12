<template>
  <div class="flex flex-col h-full">
    <!-- Header -->
    <div class="p-4 border-b border-gray-200">
      <div class="flex items-center justify-between mb-3">
        <h2 class="text-lg font-semibold text-gray-900">Chat Rooms</h2>
        <Button 
          size="sm" 
          @click="$emit('create-room')"
          :disabled="isLoadingRooms"
        >
          + Create
        </Button>
      </div>
    </div>

    <!-- Room List -->
    <div class="flex-1 overflow-y-auto">
      <!-- Loading State -->
      <div v-if="isLoadingRooms" class="flex items-center justify-center py-8">
        <LoadingSpinner />
        <span class="ml-2 text-sm text-gray-500">Loading rooms...</span>
      </div>

      <!-- Room Items -->
      <div v-else class="py-2">
        <div
          v-for="room in allRoomsWithGlobal"
          :key="room.id || 'global'"
          class="px-4 py-3 cursor-pointer transition-colors hover:bg-gray-100 border-l-4 border-transparent"
          :class="{
            'bg-blue-50 border-l-blue-500 text-blue-900': isCurrentRoom(room.id),
            'text-gray-700': !isCurrentRoom(room.id)
          }"
          @click="switchRoom(room.id, room.name)"
        >
          <div class="flex items-center justify-between">
            <div class="flex-1">
              <div class="font-medium text-sm">{{ room.name }}</div>
              <div v-if="room.id" class="text-xs text-gray-500 mt-1">
                Created {{ formatDate(room.createdAt) }}
              </div>
            </div>
            
            <!-- Active Indicator -->
            <div 
              v-if="isCurrentRoom(room.id)"
              class="w-2 h-2 bg-blue-500 rounded-full"
            ></div>
          </div>
        </div>
      </div>

      <!-- Empty State -->
      <div v-if="!isLoadingRooms && rooms.length === 0" class="p-4 text-center">
        <div class="text-gray-500 text-sm mb-2">No chat rooms yet</div>
        <Button 
          size="sm" 
          variant="outline" 
          @click="$emit('create-room')"
        >
          Create First Room
        </Button>
      </div>
    </div>

    <!-- Error State -->
    <div v-if="error" class="p-4 border-t border-gray-200">
      <Alert variant="error" :message="error" />
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted } from 'vue'
import { useChatStore } from '@/stores/chatStore'
import { ROOM_CONSTANTS } from '@/utils/constants'
import Button from '@/components/ui/Button.vue'
import LoadingSpinner from '@/components/ui/LoadingSpinner.vue'
import Alert from '@/components/ui/Alert.vue'

const chatStore = useChatStore()

const rooms = computed(() => chatStore.rooms)
const currentRoomId = computed(() => chatStore.currentRoomId)
const currentRoomName = computed(() => chatStore.currentRoomName)
const isLoadingRooms = computed(() => chatStore.isLoadingRooms)
const error = computed(() => chatStore.error)
const allRoomsWithGlobal = computed(() => chatStore.allRoomsWithGlobal)

defineEmits(['create-room'])

const isCurrentRoom = (roomId) => {
  return roomId === currentRoomId.value
}

const switchRoom = async (roomId, roomName) => {
  if (roomId === currentRoomId.value) return
  
  await chatStore.switchToRoom(roomId, roomName)
}

const formatDate = (dateString) => {
  if (!dateString) return ''
  const date = new Date(dateString)
  if (date.getFullYear() === 1970) return ''
  return date.toLocaleDateString()
}

// Load rooms on mount
onMounted(async () => {
  await chatStore.loadRooms()
})
</script>