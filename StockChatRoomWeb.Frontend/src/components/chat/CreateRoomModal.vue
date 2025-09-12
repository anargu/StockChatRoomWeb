<template>
  <!-- Modal Overlay -->
  <div 
    v-if="isOpen" 
    class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50"
    @click="closeModal"
  >
    <!-- Modal Content -->
    <div 
      class="bg-white rounded-lg shadow-xl w-full max-w-md mx-4"
      @click.stop
    >
      <!-- Header -->
      <div class="flex items-center justify-between p-6 border-b border-gray-200">
        <h3 class="text-lg font-semibold text-gray-900">Create New Room</h3>
        <button 
          class="text-gray-400 hover:text-gray-600 transition-colors"
          @click="closeModal"
        >
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
          </svg>
        </button>
      </div>

      <!-- Form -->
      <form @submit.prevent="handleSubmit" class="p-6">
        <div class="mb-4">
          <label for="roomName" class="block text-sm font-medium text-gray-700 mb-2">
            Room Name
          </label>
          <Input
            id="roomName"
            v-model="roomName"
            placeholder="Enter room name..."
            :disabled="isCreating"
            class="w-full"
            maxlength="100"
            required
          />
          <div class="mt-2 text-sm text-gray-500">
            {{ roomName.length }}/100 characters
          </div>
        </div>

        <!-- Error Message -->
        <Alert 
          v-if="error" 
          variant="error" 
          :message="error" 
          class="mb-4"
        />

        <!-- Actions -->
        <div class="flex justify-end space-x-3">
          <Button 
            type="button" 
            variant="outline" 
            @click="closeModal"
            :disabled="isCreating"
          >
            Cancel
          </Button>
          <Button 
            type="submit" 
            :disabled="!roomName.trim() || isCreating"
            :loading="isCreating"
          >
            {{ isCreating ? 'Creating...' : 'Create Room' }}
          </Button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
import { useChatStore } from '@/stores/chatStore'
import Button from '@/components/ui/Button.vue'
import Input from '@/components/ui/Input.vue'
import Alert from '@/components/ui/Alert.vue'

const chatStore = useChatStore()

const props = defineProps({
  isOpen: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['close', 'created'])

const roomName = ref('')
const isCreating = ref(false)
const error = ref(null)

watch(() => props.isOpen, (newValue) => {
  if (newValue) {
    // Reset form when modal opens
    roomName.value = ''
    error.value = null
    isCreating.value = false
  }
})

const closeModal = () => {
  if (isCreating.value) return
  emit('close')
}

const handleSubmit = async () => {
  if (!roomName.value.trim() || isCreating.value) return

  isCreating.value = true
  error.value = null

  try {
    const room = await chatStore.createRoom(roomName.value.trim())
    
    if (room) {
      emit('created', room)
      emit('close')
      
      // Auto-switch to the newly created room
      await chatStore.switchToRoom(room.id, room.name)
    } else {
      // Error should be in chatStore.error
      error.value = chatStore.error || 'Failed to create room'
    }
  } catch (err) {
    error.value = 'An unexpected error occurred'
    console.error('Error creating room:', err)
  } finally {
    isCreating.value = false
  }
}

const handleKeydown = (event) => {
  if (event.key === 'Escape' && props.isOpen && !isCreating.value) {
    closeModal()
  }
}

watch(() => props.isOpen, (newValue) => {
  if (newValue) {
    document.addEventListener('keydown', handleKeydown)
  } else {
    document.removeEventListener('keydown', handleKeydown)
  }
})
</script>

<style scoped>
/* Additional styles for modal animation if needed */
.modal-enter-active, .modal-leave-active {
  transition: opacity 0.3s ease;
}

.modal-enter-from, .modal-leave-to {
  opacity: 0;
}
</style>