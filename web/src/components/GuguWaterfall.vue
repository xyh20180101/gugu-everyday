<script setup>
import * as echarts from 'echarts/core'
import {
  TitleComponent,
  TooltipComponent,
  GridComponent,
  MarkLineComponent
} from 'echarts/components'
import { BarChart } from 'echarts/charts'
import { SVGRenderer } from 'echarts/renderers'
import { ref, onMounted, inject, watch } from 'vue'
import { getProgressText } from '@/public'

const model = defineModel()
const year = defineModel('year')
const month = defineModel('month')
watch(() => model, () => {
  update()
}, { deep: true })


echarts.use([
  TitleComponent,
  TooltipComponent,
  GridComponent,
  MarkLineComponent,
  BarChart,
  SVGRenderer
])

const update = () => {
  const now = new Date()
  const _year = year.value ?? now.getFullYear()
  const _month = month.value ?? (now.getMonth() + 1)
  const _day = now.getDate()
  const options = {
    backgroundColor: 'transparent',
    tooltip: { show: false },
    title: {
      text: `${_year}-${_month}`,
      padding: 0
    },
    barMaxWidth: 30,
    itemStyle: {
      borderRadius: 4
    },
    grid: {
      left: '3%',
      right: '4%',
      bottom: '3%',
      containLabel: true
    },
    xAxis: {
      type: 'value',
      min: 1,
      max: new Date(_year, _month, 0).getDate(),
      minInterval: 1,
      axisPointer: {
        show: true,
        label: { formatter: function (params) { return Math.floor(params.value).toString() } }
      }
    },
    yAxis: {
      type: 'category',
      splitLine: { show: false },
      data: model.value[0].map(p => p.name),
      inverse: true,
      axisPointer: { show: false },
      axisLabel: { width: 100, overflow: 'truncate' },
    },
    series: [
      {
        name: 'StartTime',
        type: 'bar',
        stack: 'total',
        itemStyle: {
          borderColor: 'transparent',
          color: 'transparent'
        },
        emphasis: {
          itemStyle: {
            borderColor: 'transparent',
            color: 'transparent'
          }
        },
        data: model.value[0]
      },
      {
        name: '',
        type: 'bar',
        stack: 'total',
        label: {
          show: true,
          position: 'inside',
          formatter: (params) => {
            return getProgressText(params.data.project)
          }
        },
        data: model.value[1],
        markLine: {
          data: [
            {
              xAxis: _day
            },
          ],
          lineStyle: { type: 'solid', width: 2, opacity: _year === now.getFullYear() && _month === now.getMonth() + 1 ? 0.5 : 0 },
          symbol: 'none',
          label: { show: false }
        },
      }
    ]
  }
  chart.setOption(options)
}

const resize = () => {
  chart.resize()
}

const chartDom = ref(null)
let chart = null

const themeName = inject('themeName')
watch(themeName, (val) => {
  if (chart) {
    chart.dispose()
    chart = echarts.init(chartDom.value, val === 'webDarkTheme' ? 'dark' : 'light')
    update()
  }
})

onMounted(() => {
  chart = echarts.init(chartDom.value, themeName.value === 'webDarkTheme' ? 'dark' : 'light')
  window.removeEventListener('resize', resize)
  window.addEventListener('resize', resize)
})

const addMonth = (m) => {
  if (month.value + m < 1) {
    year.value--
    month.value = 12
  }
  else if (month.value + m > 12) {
    year.value++
    month.value = 1
  }
  else
    month.value += m
}
</script>

<template>
  <div
    style="width: 100%;height: 100%;display: flex; flex-direction: column;justify-content: center;align-items: center;position: relative;overflow: hidden;">
    <div style="display: flex;gap: 100px;position: absolute;top:13px;z-index: 1;">
      <fluent-button size="small" appearance="transparent" icon-only shape="circular" @click="addMonth(-1)">
        <svg style="width: 16px;height: 16px;" width="24" height="24" viewBox="0 0 24 24"
          xmlns="http://www.w3.org/2000/svg">
          <path
            d="M15.7071 4.29289C16.0976 4.68342 16.0976 5.31658 15.7071 5.70711L9.41421 12L15.7071 18.2929C16.0976 18.6834 16.0976 19.3166 15.7071 19.7071C15.3166 20.0976 14.6834 20.0976 14.2929 19.7071L7.29289 12.7071C6.90237 12.3166 6.90237 11.6834 7.29289 11.2929L14.2929 4.29289C14.6834 3.90237 15.3166 3.90237 15.7071 4.29289Z" />
        </svg>
      </fluent-button>
      <fluent-button size="small" appearance="transparent" icon-only shape="circular" @click="addMonth(1)">
        <svg style="width: 16px;height: 16px;" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
          <path
            d="M8.29289 4.29289C7.90237 4.68342 7.90237 5.31658 8.29289 5.70711L14.5858 12L8.29289 18.2929C7.90237 18.6834 7.90237 19.3166 8.29289 19.7071C8.68342 20.0976 9.31658 20.0976 9.70711 19.7071L16.7071 12.7071C17.0976 12.3166 17.0976 11.6834 16.7071 11.2929L9.70711 4.29289C9.31658 3.90237 8.68342 3.90237 8.29289 4.29289Z" />
        </svg>
      </fluent-button>
    </div>
    <div class="chart" ref="chartDom"></div>
  </div>
</template>

<style scoped>
.chart {
  width: 100%;
  height: 100%;
  flex: 1;
}
</style>