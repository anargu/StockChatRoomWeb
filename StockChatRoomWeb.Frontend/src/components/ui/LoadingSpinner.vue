<template>
  <div class="flex items-center justify-center" :class="containerClasses">
    <div 
      class="animate-spin rounded-full border-2 border-gray-300"
      :class="[
        sizeClasses,
        colorClasses
      ]"
    >
    </div>
    <span v-if="text" class="ml-3 text-sm text-gray-600">
      {{ text }}
    </span>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  size: { type: String, default: 'md' },
  color: { type: String, default: 'blue' },
  text: { type: String, default: '' },
  fullHeight: { type: Boolean, default: false }
})

const containerClasses = computed(() => ({
  'h-screen': props.fullHeight,
  'py-4': !props.fullHeight
}))

const sizeClasses = computed(() => ({
  'h-4 w-4': props.size === 'sm',
  'h-8 w-8': props.size === 'md',
  'h-12 w-12': props.size === 'lg',
  'h-16 w-16': props.size === 'xl'
}))

const colorClasses = computed(() => ({
  'border-t-blue-600': props.color === 'blue',
  'border-t-green-600': props.color === 'green',
  'border-t-red-600': props.color === 'red',
  'border-t-yellow-600': props.color === 'yellow',
  'border-t-gray-600': props.color === 'gray'
}))
</script>