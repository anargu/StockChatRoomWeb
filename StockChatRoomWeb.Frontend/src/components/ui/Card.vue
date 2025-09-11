<template>
  <div 
    class="bg-white shadow-sm border border-gray-200 rounded-lg"
    :class="[
      paddingClass,
      shadowClass
    ]"
  >
    <slot />
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  padding: { type: String, default: 'md' },
  shadow: { type: String, default: 'sm' }
})

const paddingClass = computed(() => ({
  'p-4': props.padding === 'sm',
  'p-6': props.padding === 'md',
  'p-8': props.padding === 'lg',
  'p-0': props.padding === 'none'
}))

const shadowClass = computed(() => ({
  'shadow-sm': props.shadow === 'sm',
  'shadow-md': props.shadow === 'md',
  'shadow-lg': props.shadow === 'lg',
  'shadow-none': props.shadow === 'none'
}))
</script>