<script setup>
import { useRoute, useRouter } from 'vue-router'
import { project as projectApi, reminder as reminderApi, addMessage } from '@/api'
import { onMounted, ref, inject } from 'vue'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
import { NScrollbar } from 'naive-ui'
import GuguIntroduction from '@/components/GuguIntroduction.vue'
import GuguProjectCard from '@/components/GuguProjectCard.vue'
import GuguLangSwitch from '@/components/GuguLangSwitch.vue'

const route = useRoute()
const router = useRouter()
const publicData = ref({})
const showIntroduction = ref(false)

onMounted(async () => {
  if (route.params.idhash) {
    publicData.value = await projectApi.publicGetList({ idHash: route.params.idhash })
    document.title = publicData.value.showPageTitle
  } else {
    showIntroduction.value = true
  }
})

const themeName = inject('themeName')
const switchTheme = () => {
  themeName.value = themeName.value === 'webDarkTheme' ? 'webLightTheme' : 'webDarkTheme'
}

const doReminder = async (projectId) => {
  await reminderApi.create({ projectId })
  addMessage(t('publicPage.reminderSuccess'), 'success')
}
</script>

<template>
  <div style="display:flex;flex-direction:column;height:100%;position:relative;overflow:hidden;">
    <div style="display:flex;justify-content:end;padding:8px;gap:4px;">
      <GuguLangSwitch />
      <fluent-button appearance="transparent" icon-only shape="circular" :title="$t('publicPage.switchTheme')" @click="switchTheme">
        <svg width="24" height="24" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
          <path d="M12 22C17.5228 22 22 17.5228 22 12C22 6.47715 17.5228 2 12 2C6.47715 2 2 6.47715 2 12C2 17.5228 6.47715 22 12 22ZM12 20.5V3.5C16.6944 3.5 20.5 7.30558 20.5 12C20.5 16.6944 16.6944 20.5 12 20.5Z" />
        </svg>
      </fluent-button>
    </div>
    <transition name="scale" mode="out-in" appear v-if="showIntroduction">
      <GuguIntroduction />
    </transition>
    <n-scrollbar style="flex:1;height:0;" v-else>
      <div class="page">
        <fluent-text size="800" weight="bold">{{ publicData.showPageTitle }}</fluent-text>
        <fluent-text>{{ publicData.userName }}</fluent-text>
        <fluent-textarea style="max-width:600px;width:100%;" block autoResize readonly spellcheck="false"
          v-model="publicData.bulletin" />
        <GuguProjectCard v-for="p in publicData.items" :key="p.id" :project="p" @reminder="doReminder" />
      </div>
    </n-scrollbar>
  </div>
</template>

<style scoped>
.page {
  display: flex;
  flex-direction: column;
  gap: 8px;
  align-items: center;
  padding: 0 16px;
  box-sizing: border-box;
}

.scale-enter-active {
  transition: opacity var(--durationGentle) var(--curveDecelerateMid), transform var(--durationGentle) var(--curveDecelerateMid);
  transform-origin: center;
}
.scale-leave-active {
  transition: opacity var(--durationGentle) var(--curveAccelerateMid), transform var(--durationGentle) var(--curveAccelerateMid);
  transform-origin: center;
}
.scale-enter-from, .scale-leave-to { opacity: 0; transform: scale(0.85); }
.scale-enter-to, .scale-leave-from { opacity: 1; transform: scale(1); }

::v-deep(.n-popover:not(.n-popover--raw)) {
  background: color-mix(in srgb, var(--colorNeutralBackground1) 95%, transparent) !important;
}
</style>
