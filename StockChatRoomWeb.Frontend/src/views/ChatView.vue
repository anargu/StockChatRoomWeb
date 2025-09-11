<template>
  <ChatRoom @logout="handleLogout" />
</template>

<script setup>
import { useRouter } from 'vue-router'
import ChatRoom from '@/components/chat/ChatRoom.vue'

const router = useRouter()

const handleLogout = () => {
  router.push('/auth')
}
</script>