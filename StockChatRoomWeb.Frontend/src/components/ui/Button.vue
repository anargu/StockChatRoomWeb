<template>
  <button 
    :type="type"
    :disabled="disabled || loading"
    class="inline-flex items-center justify-center px-4 py-2 border border-transparent text-sm font-medium rounded-md focus:outline-none focus:ring-2 focus:ring-offset-2 transition-colors duration-200"
    :class="[
      variantClasses,
      sizeClasses,
      { 'opacity-50 cursor-not-allowed': disabled || loading }
    ]"
    v-bind="$attrs"
  >
    <svg 
      v-if="loading" 
      class="animate-spin -ml-1 mr-3 h-4 w-4" 
      fill="none" 
      viewBox="0 0 24 24"
    >
      <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
      <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
    </svg>
    <slot />
  </button>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  variant: { type: String, default: 'primary' },
  size: { type: String, default: 'md' },
  type: { type: String, default: 'button' },
  disabled: Boolean,
  loading: Boolean
})

const variantClasses = computed(() => ({
  'bg-blue-600 hover:bg-blue-700 text-white focus:ring-blue-500': props.variant === 'primary',
  'bg-gray-600 hover:bg-gray-700 text-white focus:ring-gray-500': props.variant === 'secondary',
  'bg-red-600 hover:bg-red-700 text-white focus:ring-red-500': props.variant === 'danger',
  'bg-green-600 hover:bg-green-700 text-white focus:ring-green-500': props.variant === 'success',
  'bg-white hover:bg-gray-50 text-gray-900 border-gray-300 focus:ring-blue-500': props.variant === 'outline'
}))

const sizeClasses = computed(() => ({
  'px-3 py-2 text-sm': props.size === 'sm',
  'px-4 py-2 text-sm': props.size === 'md',
  'px-6 py-3 text-base': props.size === 'lg'
}))
</script>