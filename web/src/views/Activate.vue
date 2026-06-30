<script setup>
import { ref, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute, useRouter } from 'vue-router'
import { auth } from '@/api'

const route = useRoute()
const router = useRouter()
const { t } = useI18n()

const loading = ref(true)
const success = ref(false)
const errorMsg = ref('')

onMounted(async () => {
  try {
    await auth.activate({ token: route.query.token })
    success.value = true
  } catch (e) {
    errorMsg.value = e.message || t('activate.failed')
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div class="main">
    <fluent-spinner v-if="loading" size="tiny"></fluent-spinner>
    <div v-else class="card">
      <template v-if="success">
        <fluent-text style="display:flex;align-items:center;gap:8px;">
          <svg width="24" height="24" viewBox="0 0 24 24" style="color:var(--colorPaletteGreenForeground1)" fill="currentcolor" xmlns="http://www.w3.org/2000/svg">
            <path d="M12 2C17.5228 2 22 6.47715 22 12C22 17.5228 17.5228 22 12 22C6.47715 22 2 17.5228 2 12C2 6.47715 6.47715 2 12 2ZM15.2197 8.96967L10.75 13.4393L8.78033 11.4697C8.48744 11.1768 8.01256 11.1768 7.71967 11.4697C7.42678 11.7626 7.42678 12.2374 7.71967 12.5303L10.2197 15.0303C10.5126 15.3232 10.9874 15.3232 11.2803 15.0303L16.2803 10.0303C16.5732 9.73744 16.5732 9.26256 16.2803 8.96967C15.9874 8.67678 15.5126 8.67678 15.2197 8.96967Z"/>
          </svg>
          {{ $t('activate.success') }}
        </fluent-text>
        <fluent-button @click="router.push({ name: 'login' })">{{ $t('activate.goToLogin') }}</fluent-button>
      </template>
      <template v-else>
        <fluent-text style="display:flex;align-items:center;gap:8px;">
          <svg width="24" height="24" viewBox="0 0 24 24" style="color:var(--colorStatusDangerForeground1)" fill="currentcolor" xmlns="http://www.w3.org/2000/svg">
            <path d="M12 2C17.5228 2 22 6.47715 22 12C22 17.5228 17.5228 22 12 22C6.47715 22 2 17.5228 2 12C2 6.47715 6.47715 2 12 2ZM12 10.5858L8.70711 7.29289C8.31658 6.90237 7.68342 6.90237 7.29289 7.29289C6.90237 7.68342 6.90237 8.31658 7.29289 8.70711L10.5858 12L7.29289 15.2929C6.90237 15.6834 6.90237 16.3166 7.29289 16.7071C7.68342 17.0976 8.31658 17.0976 8.70711 16.7071L12 13.4142L15.2929 16.7071C15.6834 17.0976 16.3166 17.0976 16.7071 16.7071C17.0976 16.3166 17.0976 15.6834 16.7071 15.2929L13.4142 12L16.7071 8.70711C17.0976 8.31658 17.0976 7.68342 16.7071 7.29289C16.3166 6.90237 15.6834 6.90237 15.2929 7.29289L12 10.5858Z"/>
          </svg>
          {{ errorMsg }}
        </fluent-text>
        <fluent-button @click="router.push({ name: 'login' })">{{ $t('activate.goToLogin') }}</fluent-button>
      </template>
    </div>
  </div>
</template>

<style scoped>
.main {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 16px;
  box-sizing: border-box;
}
.card {
  width: 100%;
  max-width: 440px;
  background-color: var(--colorNeutralBackground1);
  border-radius: 12px;
  box-shadow: var(--shadow16);
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 16px;
  padding: 16px;
}
fluent-button { width: 100%; }
</style>
