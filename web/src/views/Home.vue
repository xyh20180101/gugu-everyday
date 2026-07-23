<script setup>
import { home, reminder } from '@/api'
import GuguCalendar from '@/components/GuguCalendar.vue'
import GuguWaterfall from '@/components/GuguWaterfall.vue'
import { NDataTable, NScrollbar } from 'naive-ui'
import { onMounted, ref, h } from 'vue'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()

const reminderColumns = [{
  title: t('home.project'),
  key: 'projectName',
  minWidth: 60
},
{
  title: t('home.reminderCount'),
  key: 'totalCount',
  minWidth: 60,
  render(row) {
    return h(
      'fluent-text',
      {},
      { default: () => row.incrementCount > 0 ? `${row.totalCount} (↑${row.incrementCount})` : `${row.totalCount}`}
    )
  }
},
{
  title: t('home.time'),
  key: 'latestReminderTime',
  minWidth: 120,
  render(row) {
    return h(
      'fluent-text',
      {},
      { default: () => new Date(row.latestReminderTime + 'Z').toLocaleString() }
    )
  }
}]

const waterfallData = ref([])
const calendarData = ref([])
const reminderData = ref([])

const now = new Date()
const year = ref(now.getFullYear())
const month = ref(now.getMonth() + 1)

onMounted(async () => {
  await getWaterfall()
  await getCalendar()
  await getReminder()
})

const getWaterfall = async () => {
  waterfallData.value = await home.waterfall({ year: year.value, month: month.value })
}

const getCalendar = async () => {
  calendarData.value = await home.calendar({ year: year.value, month: month.value })
}

const getReminder = async () => {
  reminderData.value = await reminder.getSummary()
}
</script>

<template>
  <div
    style="height: 0; flex: 1; display: flex; flex-direction: column;justify-content: center; padding-bottom: 16px; gap: 8px;">
    <GuguWaterfall v-if="false" v-model="waterfallData" v-model:year="year" v-model:month="month" @update:month="getWaterfall">
    </GuguWaterfall>
    <GuguCalendar :project-list="calendarData"></GuguCalendar>
    <fluent-text weight="bold">{{ $t('home.reminderLog') }}</fluent-text>
    <n-scrollbar style="height: 100%;">
      <n-data-table :columns="reminderColumns" :data="reminderData" :bordered="false">
        <template #empty>
          <fluent-text>{{ $t('home.noData') }}</fluent-text>
        </template>
      </n-data-table>
    </n-scrollbar>
  </div>
</template>

<style scoped></style>