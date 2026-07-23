<script setup>
import { h } from 'vue'
import { NCalendar } from 'naive-ui'

const props = defineProps({
  projectList: { type: Array, default: () => [] }
})

const today = new Date()

const DateProjectList = (year, month, date) => {
  const day = new Date(year, month - 1, date)
  const list = []
  props.projectList.forEach(p => {
    const pStart = new Date(p.startTime)
    const pEnd = new Date(p.endTime)
    if ((!p.startTime || day >= pStart) && (!p.endTime || day <= pEnd))
      list.push(h('div', { class: 'project' }, [
        h('div', { class: 'dot', style: `background: ${p.color}` }),
        h('div', { class: 'text' }, () => p.name)
      ]))
  })
  return h('div', { class: 'project-list' }, list)
}
</script>

<template>
  <n-calendar :default-value="today">
    <template #default="{ year, month, date }">
      <component :is="DateProjectList(year, month, date)" />
    </template>
    <template #header="{ year, month }">
      <div style="font-size: 14px; font-weight: bold;">
        {{ year }}/{{ String(month).padStart(2, '0') }} {{ $t('guguCalendar.schedule') }}
      </div>
    </template>
  </n-calendar>
</template>

<style scoped>
.n-calendar {
  height: 60%;
  width: 100%;
}

::v-deep(.n-calendar-header){
  padding-bottom: 4px;
}

::v-deep(.n-calendar-dates) {
  overflow: auto;
  grid-template-columns: repeat(7, minmax(88px, 1fr)) !important;
  transition: opacity 0.25s ease-in-out, transform 0.25s ease-in-out;
}

.project-list {
  display: block;
  min-height: 40px;
}

.project {
  display: flex;
  align-items: center;
  gap: 4px;
  color: var(--colorNeutralForeground2);
  height: 24px;
}

.dot {
  width: 8px;
  height: 8px;
  border-radius: 4px;
}

.text {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
</style>