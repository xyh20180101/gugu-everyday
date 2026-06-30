<script setup>
import { useI18n } from 'vue-i18n'

const { locale } = useI18n()

const toggle = () => {
  locale.value = locale.value === 'zh-CN' ? 'en' : 'zh-CN'
  localStorage.setItem('locale', locale.value)
}
</script>

<template>
  <fluent-button appearance="transparent" icon-only shape="circular" :title="$t('lang.switch')" @click="toggle">
    <span style="font-size: 14px; font-weight: 600;">{{ locale === 'zh-CN' ? 'EN' : '中' }}</span>
  </fluent-button>
</template>
