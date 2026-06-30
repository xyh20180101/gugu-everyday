<script setup>
import { home, reminder } from '@/api'
import GuguWaterfall from '@/components/GuguWaterfall.vue'
import { NDataTable, NScrollbar } from 'naive-ui'
import { onMounted, ref, h } from 'vue'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()

const reminderColumns = [{
  title: t('home.project'),
  key: 'project.name',
  minWidth: 60
},
{
  title: t('home.time'),
  key: 'creationTime',
  minWidth: 120,
  render(row) {
    return h(
      'fluent-text',
      {},
      { default: () => new Date(row.creationTime + 'Z').toLocaleString() }
    )
  }
}]

const waterfallData = ref([])
const reminderData = ref({})
const reminderPageRequest = ref({ page: 1, count: 10 })

const now = new Date()
const year = ref(now.getFullYear())
const month = ref(now.getMonth() + 1)

onMounted(async () => {
  await getWaterfall()
  await getReminder()
})

const getWaterfall = async () => {
  waterfallData.value = await home.waterfall({ year: year.value, month: month.value })
}

const getReminder = async () => {
  reminderData.value = await reminder.getList({ skip: (reminderPageRequest.value.page - 1) * reminderPageRequest.value.count, count: reminderPageRequest.value.count })
}
</script>

<template>
  <div
    style="height: 0; flex: 1; display: flex; flex-direction: column;justify-content: center; padding-bottom: 16px; gap: 8px;">
    <GuguWaterfall v-model="waterfallData" v-model:year="year" v-model:month="month" @update:month="getWaterfall">
    </GuguWaterfall>
    <fluent-text weight="bold">{{ $t('home.reminderLog') }}</fluent-text>
    <n-scrollbar style="height: 100%;">
      <n-data-table :columns="reminderColumns" :data="reminderData?.items" :bordered="false">
        <template #empty>
          <fluent-text>{{ $t('home.noData') }}</fluent-text>
        </template>
      </n-data-table>
    </n-scrollbar>
  </div>
</template>

<style scoped></style>