<script setup>
import { ref, inject, computed, watch } from 'vue'
import { RouterView } from 'vue-router'
import { messageStore } from '@/api'
import { NConfigProvider, zhCN, dateZhCN, darkTheme, lightTheme } from 'naive-ui'

const position = ref('top')

const themeName = inject('themeName')
const naiveUITheme = computed(() => themeName.value === 'webDarkTheme' ? darkTheme : lightTheme)

const localeName = inject('locale')
const naiveUILocale = ref(localeName.value === 'zh-CN' ? zhCN : null)
const naiveUIDateLocale = ref(localeName.value === 'zh-CN' ? dateZhCN : null)
watch(localeName, (val) => {
  naiveUILocale.value = val === 'zh-CN' ? zhCN : null
  naiveUIDateLocale.value = val === 'zh-CN' ? dateZhCN : null
})

const themeOverrides = {
  Button: {
    textColor: 'var(--colorNeutralForeground2)',
    textColorHover: 'var(--colorCompoundBrandStrokeHover)',
    textColorPressed: 'var(--colorCompoundBrandStrokePressed)',
    textColorFocus: 'var(--colorCompoundBrandStrokeHover)',
    border: '1px soild var(--colorCompoundBrandStroke)',
    borderHover: '1px soild var(--colorCompoundBrandStrokeHover)',
    borderPressed: '1px soild var(--colorCompoundBrandStrokePressed)',
    borderFocus: '1px soild var(--colorCompoundBrandStrokeHover)',
    rippleColor: 'transparent'
  },
  Calendar: {
    dateColorCurrent: 'var(--colorCompoundBrandStroke)',
    barColor: 'var(--colorCompoundBrandStroke)'
  },
  DataTable: {
    actionDividerColor: '',
    fontSizeSmall: '',
    borderColor: 'var(--colorNeutralStroke2)',
    tdColorHover: '',
    tdColorSorting: '',
    tdColorStriped: '',
    thColor: 'transparent',
    thColorHover: '',
    thColorSorting: '',
    tdColor: '',
    tdTextColor: 'var(--colorNeutralForeground2)',
    thTextColor: 'var(--colorNeutralForeground2)',
    thFontWeight: '600',
    thButtonColorHover: '',
    thIconColor: '',
    thIconColorActive: 'var(--colorCompoundBrandStroke)',
    borderColorModal: '',
    tdColorHoverModal: '',
    tdColorSortingModal: '',
    tdColorStripedModal: '',
    thColorModal: '',
    thColorHoverModal: '',
    thColorSortingModal: '',
    tdColorModal: '',
    borderColorPopover: '',
    tdColorHoverPopover: '',
    tdColorSortingPopover: '',
    tdColorStripedPopover: '',
    thColorPopover: '',
    thColorHoverPopover: '',
    thColorSortingPopover: '',
    tdColorPopover: '',
    loadingColor: 'var(--colorCompoundBrandStroke)'
  },
  DatePicker: {
    itemCellWidth: '28px',
    itemCellHeight: '28px',
    itemBorderRadius: 'var(--borderRadiusMedium)',
    itemTextColor: 'var(--colorNeutralForeground2)',
    itemTextColorDisabled: 'var(--colorNeutralForegroundDisabled)',
    itemTextColorActive: 'var(--colorNeutralForegroundOnBrand)',
    itemTextColorCurrent: 'var(--colorBrandBackground)',
    itemColorIncluded: '',
    itemColorHover: '',
    itemColorDisabled: '',
    itemColorActive: 'var(--colorBrandBackground)',
    panelColor: 'var(--colorNeutralBackground1)',
    panelTextColor: 'var(--colorNeutralForeground2)',
    arrowColor: '',
    calendarTitleTextColor: 'var(--colorNeutralForeground2)',
    calendarTitleColorHover: '',
    calendarDaysTextColor: '',
    panelHeaderDividerColor: '',
    calendarDaysDividerColor: '',
    calendarDividerColor: '',
    panelActionDividerColor: '',
    iconColor: '',
    iconColorDisabled: ''
  },
  Popover: {
    padding: '0'
  },
  Scrollbar: {
    color: 'var(--colorNeutralStroke2)',
    colorHover: 'var(--colorNeutralStroke1)'
  }
}
</script>

<template>
  <div class="app">
    <n-config-provider :theme="naiveUITheme" :theme-overrides="themeOverrides" :locale="naiveUILocale"
      :date-locale="naiveUIDateLocale">
      <RouterView />
      <transition name="fade">
        <div v-show="messageStore.messages.length > 0" :class="['message-stack', position]">
          <transition-group name="fade" tag="div" class="message-list">
            <fluent-message-bar v-for="msg in messageStore.messages" :key="msg.id" :intent="msg.intent">
              <fluent-text>{{ msg.text }}</fluent-text>
            </fluent-message-bar>
          </transition-group>
        </div>
      </transition>
    </n-config-provider>
  </div>
</template>

<style scoped>
.app {
  width: 100%;
  height: 100%;
  background-color: var(--colorNeutralBackground3);
}

.message-stack {
  position: fixed;
  display: flex;
  flex-direction: column;
  gap: 8px;
  z-index: 9999;
  padding: 20px;
  pointer-events: none;
  box-sizing: border-box;
  justify-self: center;
  width: 100%;
  align-items: center;
}

.message-stack.top {
  top: 0px;
}

.message-stack.bottom {
  bottom: 0px;
}

.message-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
  pointer-events: auto;
  align-items: center;
  width: fit-content;
}

.fade-enter-active,
.fade-leave-active {
  transition: all 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}

.fade-enter-to,
.fade-leave-from {
  opacity: 0.9;
  transform: translateY(0);
}

fluent-message-bar {
  opacity: 0.9;
  --spacingHorizontalS: 0px;
  width: fit-content;
}
</style>
