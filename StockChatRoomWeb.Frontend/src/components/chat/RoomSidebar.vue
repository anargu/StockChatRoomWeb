<template>
  <div class="h-full flex flex-col bg-white border-r border-gray-200 w-64">
    <!-- Room List -->
    <RoomList @create-room="openCreateModal" />
    
    <!-- Create Room Modal -->
    <CreateRoomModal 
      :is-open="showCreateModal"
      @close="closeCreateModal"
      @created="handleRoomCreated"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'
import RoomList from './RoomList.vue'
import CreateRoomModal from './CreateRoomModal.vue'

// State
const showCreateModal = ref(false)

// Methods
const openCreateModal = () => {
  showCreateModal.value = true
}

const closeCreateModal = () => {
  showCreateModal.value = false
}

const handleRoomCreated = (room) => {
  console.log('Room created:', room)
  // The RoomList will automatically update via the store
}
</script>